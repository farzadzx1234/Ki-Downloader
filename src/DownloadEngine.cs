using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace KiDownloader
{
    public class DownloadEngine : IDisposable
    {
        private readonly string _dataDir;
        private readonly string _statePath;
        private readonly object _stateLock = new object();
        private readonly object _fileIoLock = new object();
        private readonly Dictionary<string, CancellationTokenSource> _activeTokens = new Dictionary<string, CancellationTokenSource>();
        private readonly Dictionary<string, HashSet<HttpWebRequest>> _activeRequests = new Dictionary<string, HashSet<HttpWebRequest>>();
        private readonly Dictionary<string, HashSet<IDisposable>> _activeStreams = new Dictionary<string, HashSet<IDisposable>>();
        private readonly Dictionary<string, TokenBucketLimiter> _itemLimiters = new Dictionary<string, TokenBucketLimiter>();
        private readonly TokenBucketLimiter _globalLimiter = new TokenBucketLimiter(0);
        private readonly Socks5HttpRelay _socksRelay = new Socks5HttpRelay();
        private readonly JavaScriptSerializer _json = new JavaScriptSerializer { MaxJsonLength = 32 * 1024 * 1024 };
        private readonly Random _rng = new Random();

        private System.Threading.Timer _persistTimer;
        private System.Threading.Timer _schedulerTimer;
        private bool _stateDirty;
        private bool _queueRunning = true;
        private bool _nightWindowTriggered;

        public AppSettings Settings { get; private set; }
        public List<DownloadItem> Downloads { get; private set; }

        public event Action OnStateUpdated;
        public event Action<DownloadItem> OnDownloadCompleted;
        public event Action<PostQueueAction> OnPostQueueActionRequested;

        public DownloadEngine(string dataDir)
        {
            _dataDir = dataDir;
            if (!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
            _statePath = Path.Combine(_dataDir, "state.json");

            NetworkHelper.ConfigureTlsAndServicePoint();
            _socksRelay.Start();

            Downloads = new List<DownloadItem>();
            Settings = new AppSettings();
            LoadState();
            _globalLimiter.SetLimitKBps(Settings.GlobalSpeedLimitKBps);

            if (!Directory.Exists(Settings.DefaultSaveFolder))
            {
                try { Directory.CreateDirectory(Settings.DefaultSaveFolder); } catch { }
            }

            // Recover interrupted downloads from previous crash/exit into Paused state
            lock (_stateLock)
            {
                foreach (var d in Downloads)
                {
                    if (d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Probing || d.Status == DownloadStatus.Merging)
                    {
                        d.Status = DownloadStatus.Paused;
                        d.SpeedBps = 0;
                        d.EtaSeconds = 0;
                        d.StatusDetail = "Paused (recovered)";
                        foreach (var s in d.Segments)
                        {
                            if (s.Status != SegmentStatus.Done)
                                s.Status = SegmentStatus.Pending;
                            s.SpeedBps = 0;
                        }
                    }
                }
            }

            _persistTimer = new System.Threading.Timer(delegate
            {
                if (_stateDirty) SaveState();
            }, null, 1500, 1500);

            _schedulerTimer = new System.Threading.Timer(delegate { TickQueueAndScheduler(); }, null, 1000, 1000);
        }

        public static string CategorizeFile(string fileName, string mime)
        {
            string ext = (Path.GetExtension(fileName ?? "") ?? "").ToLowerInvariant();
            if (ext == ".zip" || ext == ".rar" || ext == ".7z" || ext == ".tar" || ext == ".gz" || ext == ".xz" || ext == ".iso")
                return "Compressed";
            if (ext == ".mp4" || ext == ".mkv" || ext == ".webm" || ext == ".avi" || ext == ".mov" || ext == ".flv" || ext == ".ts")
                return "Video";
            if (ext == ".mp3" || ext == ".flac" || ext == ".wav" || ext == ".aac" || ext == ".ogg" || ext == ".m4a")
                return "Music";
            if (ext == ".exe" || ext == ".msi" || ext == ".msix" || ext == ".apk" || ext == ".dmg" || ext == ".deb")
                return "Programs";
            if (ext == ".pdf" || ext == ".doc" || ext == ".docx" || ext == ".xls" || ext == ".xlsx" || ext == ".ppt" || ext == ".pptx" || ext == ".txt" || ext == ".epub")
                return "Documents";
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".webp" || ext == ".gif" || ext == ".svg")
                return "Images";

            if (!string.IsNullOrEmpty(mime))
            {
                string m = mime.ToLowerInvariant();
                if (m.StartsWith("video/")) return "Video";
                if (m.StartsWith("audio/")) return "Music";
                if (m.StartsWith("image/")) return "Images";
                if (m.Contains("zip") || m.Contains("rar") || m.Contains("compressed")) return "Compressed";
                if (m.Contains("pdf") || m.Contains("document")) return "Documents";
            }
            return "Other";
        }

        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "download_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            name = Uri.UnescapeDataString(name.Trim());
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name.Trim();
        }

        public string ResolveSaveFolder(string category, string customFolder)
        {
            string baseDir = string.IsNullOrWhiteSpace(customFolder) ? Settings.DefaultSaveFolder : customFolder.Trim();
            if (Settings.AutoCategorizeFolders && string.IsNullOrWhiteSpace(customFolder) && !string.IsNullOrEmpty(category) && category != "Other")
            {
                baseDir = Path.Combine(baseDir, category);
            }
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
            return baseDir;
        }

        public string GetUniqueFilePath(string folder, string fileName)
        {
            string clean = SanitizeFileName(fileName);
            string full = Path.Combine(folder, clean);
            if (!File.Exists(full)) return full;

            string stem = Path.GetFileNameWithoutExtension(clean);
            string ext = Path.GetExtension(clean);
            int counter = 1;
            while (File.Exists(full))
            {
                full = Path.Combine(folder, string.Format("{0} ({1}){2}", stem, counter, ext));
                counter++;
            }
            return full;
        }

        public Task<ProbeResult> ProbeUrlAsync(string url, string referer, string cookies, string userAgent)
        {
            return ProbeUrlAsync(url, referer, cookies, userAgent, CancellationToken.None, null);
        }

        public async Task<ProbeResult> ProbeUrlAsync(string url, string referer, string cookies, string userAgent, CancellationToken ct, string trackItemId)
        {
            ProbeResult res = new ProbeResult { FinalUrl = url };
            try
            {
                Uri uri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                {
                    res.Error = "Invalid HTTP/HTTPS URL.";
                    return res;
                }

                // Attempt HEAD first
                ProbeResult headRes = await ProbeWithMethodAsync(url, "HEAD", false, referer, cookies, userAgent, ct, trackItemId).ConfigureAwait(false);
                if (ct.IsCancellationRequested) return res;
                if (headRes.Success && (headRes.TotalBytes > 0 || headRes.SupportsRange))
                {
                    return headRes;
                }

                // Fallback: GET with Range: bytes=0-0 (many Iranian CDNs / Cloudflare Workers / reverse proxies block HEAD)
                ProbeResult rangeRes = await ProbeWithMethodAsync(url, "GET", true, referer, cookies, userAgent, ct, trackItemId).ConfigureAwait(false);
                if (ct.IsCancellationRequested) return res;
                if (rangeRes.Success)
                {
                    return rangeRes;
                }

                return headRes.Success ? headRes : rangeRes;
            }
            catch (Exception ex)
            {
                res.Error = ex.Message;
                return res;
            }
        }

        private async Task<ProbeResult> ProbeWithMethodAsync(string url, string method, bool useRangeZero, string referer, string cookies, string userAgent, CancellationToken ct, string trackItemId)
        {
            ProbeResult res = new ProbeResult { FinalUrl = url };
            HttpWebRequest req = CreateRequest(url, method, referer, cookies, userAgent);
            req.Timeout = 15000;
            req.ReadWriteTimeout = 15000;
            if (useRangeZero)
            {
                req.AddRange(0, 0);
            }

            RegisterActiveRequest(trackItemId, req);
            try
            {
                using (ct.Register(delegate { try { req.Abort(); } catch { } }))
                using (HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync().ConfigureAwait(false))
                {
                    res.Success = true;
                    res.StatusCode = (int)resp.StatusCode;
                    res.FinalUrl = resp.ResponseUri != null ? resp.ResponseUri.AbsoluteUri : url;
                    res.MimeType = resp.ContentType ?? "";
                    res.ETag = resp.Headers["ETag"] ?? "";

                    string acceptRanges = (resp.Headers["Accept-Ranges"] ?? "").ToLowerInvariant();
                    string contentRange = resp.Headers["Content-Range"] ?? "";

                    if (!string.IsNullOrEmpty(contentRange) && contentRange.Contains("/"))
                    {
                        string totalPart = contentRange.Substring(contentRange.LastIndexOf('/') + 1).Trim();
                        long total;
                        if (long.TryParse(totalPart, out total))
                        {
                            res.TotalBytes = total;
                            res.SupportsRange = true;
                        }
                    }

                    if (res.TotalBytes <= 0 && resp.ContentLength > 0 && !useRangeZero)
                    {
                        res.TotalBytes = resp.ContentLength;
                    }

                    if (acceptRanges.Contains("bytes") || resp.StatusCode == HttpStatusCode.PartialContent)
                    {
                        res.SupportsRange = true;
                    }

                    res.FileName = ExtractFileName(resp, res.FinalUrl);
                    res.Category = CategorizeFile(res.FileName, res.MimeType);
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse errResp = wex.Response as HttpWebResponse;
                if (errResp != null)
                {
                    res.StatusCode = (int)errResp.StatusCode;
                    res.Error = string.Format("HTTP {0}: {1}", (int)errResp.StatusCode, errResp.StatusDescription);
                }
                else
                {
                    res.Error = wex.Message;
                }
            }
            catch (Exception ex)
            {
                res.Error = ex.Message;
            }
            finally
            {
                UnregisterActiveRequest(trackItemId, req);
            }
            return res;
        }

        private void RegisterActiveRequest(string itemId, HttpWebRequest req)
        {
            if (string.IsNullOrEmpty(itemId) || req == null) return;
            lock (_stateLock)
            {
                HashSet<HttpWebRequest> set;
                if (!_activeRequests.TryGetValue(itemId, out set))
                {
                    set = new HashSet<HttpWebRequest>();
                    _activeRequests[itemId] = set;
                }
                set.Add(req);
            }
        }

        private void UnregisterActiveRequest(string itemId, HttpWebRequest req)
        {
            if (string.IsNullOrEmpty(itemId) || req == null) return;
            lock (_stateLock)
            {
                HashSet<HttpWebRequest> set;
                if (_activeRequests.TryGetValue(itemId, out set)) set.Remove(req);
            }
        }

        private void RegisterActiveStream(string itemId, IDisposable stream)
        {
            if (string.IsNullOrEmpty(itemId) || stream == null) return;
            lock (_stateLock)
            {
                HashSet<IDisposable> set;
                if (!_activeStreams.TryGetValue(itemId, out set))
                {
                    set = new HashSet<IDisposable>();
                    _activeStreams[itemId] = set;
                }
                set.Add(stream);
            }
        }

        private void UnregisterActiveStream(string itemId, IDisposable stream)
        {
            if (string.IsNullOrEmpty(itemId) || stream == null) return;
            lock (_stateLock)
            {
                HashSet<IDisposable> set;
                if (_activeStreams.TryGetValue(itemId, out set)) set.Remove(stream);
            }
        }

        private void CancelAndAbortItem(string id, out CancellationTokenSource cts)
        {
            List<HttpWebRequest> reqs = null;
            List<IDisposable> streams = null;
            lock (_stateLock)
            {
                if (_activeTokens.TryGetValue(id, out cts)) _activeTokens.Remove(id);
                HashSet<HttpWebRequest> rset;
                if (_activeRequests.TryGetValue(id, out rset))
                {
                    reqs = rset.ToList();
                    _activeRequests.Remove(id);
                }
                HashSet<IDisposable> sset;
                if (_activeStreams.TryGetValue(id, out sset))
                {
                    streams = sset.ToList();
                    _activeStreams.Remove(id);
                }
            }
            if (cts != null) { try { cts.Cancel(); } catch { } }
            if (streams != null)
            {
                foreach (var s in streams) { try { s.Dispose(); } catch { } }
            }
            if (reqs != null)
            {
                foreach (var r in reqs) { try { r.Abort(); } catch { } }
            }
        }

        private HttpWebRequest CreateRequest(string url, string method, string referer, string cookies, string userAgent)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = method;
            req.AllowAutoRedirect = true;
            req.MaximumAutomaticRedirections = 12;
            req.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? Settings.DefaultUserAgent : userAgent;
            req.Accept = "*/*";
            req.AutomaticDecompression = DecompressionMethods.None;
            req.ServicePoint.Expect100Continue = false;
            req.ServicePoint.UseNagleAlgorithm = false;

            if (!string.IsNullOrWhiteSpace(referer)) req.Referer = referer;
            if (!string.IsNullOrWhiteSpace(cookies)) req.Headers["Cookie"] = cookies;

            req.Proxy = NetworkHelper.BuildWebProxy(Settings.Proxy, _socksRelay);
            return req;
        }

        private static string ExtractFileName(HttpWebResponse resp, string finalUrl)
        {
            string cd = resp.Headers["Content-Disposition"];
            if (!string.IsNullOrEmpty(cd))
            {
                Match mStar = Regex.Match(cd, @"filename\*\s*=\s*UTF-8''([^;\r\n]+)", RegexOptions.IgnoreCase);
                if (mStar.Success)
                {
                    return SanitizeFileName(Uri.UnescapeDataString(mStar.Groups[1].Value.Trim('"')));
                }
                Match m = Regex.Match(cd, @"filename\s*=\s*""?([^"";\r\n]+)""?", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    return SanitizeFileName(m.Groups[1].Value);
                }
            }

            try
            {
                Uri uri = new Uri(finalUrl);
                string seg = uri.Segments.LastOrDefault();
                if (!string.IsNullOrEmpty(seg) && seg != "/")
                {
                    return SanitizeFileName(seg.TrimEnd('/'));
                }
            }
            catch { }

            return SanitizeFileName("download_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
        }

        public DownloadItem AddDownload(
            string url,
            string fileName,
            string saveFolder,
            int connections,
            bool startNow,
            bool iranResilient,
            List<string> mirrors,
            string referer,
            string cookies,
            DateTime? scheduledFor)
        {
            string cleanName = string.IsNullOrWhiteSpace(fileName) ? ExtractNameFromUrl(url) : SanitizeFileName(fileName);
            string category = CategorizeFile(cleanName, "");
            string folder = ResolveSaveFolder(category, saveFolder);
            string finalPath = GetUniqueFilePath(folder, cleanName);

            DownloadItem item = new DownloadItem
            {
                Url = url.Trim(),
                FileName = Path.GetFileName(finalPath),
                SaveFolder = folder,
                FinalFilePath = finalPath,
                TempFilePath = finalPath + ".kidown",
                Category = category,
                MaxConnections = Math.Max(1, Math.Min(32, connections > 0 ? connections : Settings.DefaultConnections)),
                IranResilientMode = iranResilient,
                Referer = referer ?? "",
                Cookies = cookies ?? "",
                ScheduledFor = scheduledFor,
                MirrorUrls = mirrors != null ? mirrors.Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()).ToList() : new List<string>()
            };

            if (scheduledFor.HasValue && scheduledFor.Value > DateTime.Now)
            {
                item.Status = DownloadStatus.Scheduled;
                item.StatusDetail = "Scheduled for " + scheduledFor.Value.ToString("HH:mm");
            }
            else if (startNow)
            {
                item.Status = DownloadStatus.Queued;
                item.StatusDetail = "Queued";
            }
            else
            {
                item.Status = DownloadStatus.Paused;
                item.StatusDetail = "Paused";
            }

            lock (_stateLock)
            {
                Downloads.Insert(0, item);
            }
            MarkDirtyAndNotify();

            if (startNow && item.Status == DownloadStatus.Queued)
            {
                TryStartNextQueued();
            }
            return item;
        }

        private static string ExtractNameFromUrl(string url)
        {
            try
            {
                Uri u = new Uri(url);
                string seg = u.Segments.LastOrDefault();
                if (!string.IsNullOrEmpty(seg) && seg != "/") return seg.TrimEnd('/');
            }
            catch { }
            return "download";
        }

        public void StartOrResume(string id)
        {
            DownloadItem item;
            lock (_stateLock)
            {
                item = Downloads.FirstOrDefault(x => x.Id == id);
                if (item == null || item.Status == DownloadStatus.Completed || item.Status == DownloadStatus.Downloading) return;
                item.Status = DownloadStatus.Queued;
                item.ErrorMessage = null;
                item.StatusDetail = "Queued";
                item.ScheduledFor = null;
            }
            MarkDirtyAndNotify();
            TryStartNextQueued();
        }

        public void Pause(string id)
        {
            CancellationTokenSource cts = null;
            lock (_stateLock)
            {
                var item = Downloads.FirstOrDefault(x => x.Id == id);
                if (item == null) return;
                if (item.Status == DownloadStatus.Completed) return;

                item.Status = DownloadStatus.Paused;
                item.SpeedBps = 0;
                item.EtaSeconds = 0;
                item.StatusDetail = "Paused";
                foreach (var s in item.Segments)
                {
                    if (s.Status != SegmentStatus.Done) s.Status = SegmentStatus.Pending;
                    s.SpeedBps = 0;
                }
            }
            CancelAndAbortItem(id, out cts);
            MarkDirtyAndNotify();
            TryStartNextQueued();
        }

        public void PauseAll()
        {
            List<string> ids;
            lock (_stateLock)
            {
                ids = Downloads.Where(x => x.Status == DownloadStatus.Downloading || x.Status == DownloadStatus.Queued || x.Status == DownloadStatus.Probing || x.Status == DownloadStatus.Merging).Select(x => x.Id).ToList();
            }
            foreach (string id in ids) Pause(id);
        }

        public void ResumeAll()
        {
            lock (_stateLock)
            {
                foreach (var d in Downloads)
                {
                    if (d.Status == DownloadStatus.Paused || d.Status == DownloadStatus.Error)
                    {
                        d.Status = DownloadStatus.Queued;
                        d.StatusDetail = "Queued";
                        d.ErrorMessage = null;
                    }
                }
            }
            MarkDirtyAndNotify();
            TryStartNextQueued();
        }

        public void Remove(string id, bool deleteFiles)
        {
            DownloadItem item = null;
            CancellationTokenSource cts = null;
            lock (_stateLock)
            {
                item = Downloads.FirstOrDefault(x => x.Id == id);
                if (item == null) return;
                Downloads.Remove(item);
                _itemLimiters.Remove(id);
            }
            CancelAndAbortItem(id, out cts);

            if (deleteFiles && item != null)
            {
                Task.Run(delegate
                {
                    Thread.Sleep(250);
                    try { if (!string.IsNullOrEmpty(item.TempFilePath) && File.Exists(item.TempFilePath)) File.Delete(item.TempFilePath); } catch { }
                    try { if (!string.IsNullOrEmpty(item.FinalFilePath) && File.Exists(item.FinalFilePath)) File.Delete(item.FinalFilePath); } catch { }
                });
            }
            MarkDirtyAndNotify();
        }

        public void RefreshAddress(string id, string newUrl)
        {
            if (string.IsNullOrWhiteSpace(newUrl)) return;
            lock (_stateLock)
            {
                var item = Downloads.FirstOrDefault(x => x.Id == id);
                if (item == null) return;
                item.Url = newUrl.Trim();
                item.ActiveMirrorIndex = -1;
                item.ErrorMessage = null;
                item.StatusDetail = "Address refreshed";
            }
            StartOrResume(id);
        }

        public void SetItemSpeedLimit(string id, int kbps)
        {
            lock (_stateLock)
            {
                var item = Downloads.FirstOrDefault(x => x.Id == id);
                if (item != null) item.SpeedLimitKBps = Math.Max(0, kbps);
                if (_itemLimiters.ContainsKey(id)) _itemLimiters[id].SetLimitKBps(Math.Max(0, kbps));
            }
            MarkDirtyAndNotify();
        }

        public void UpdateSettings(AppSettings updated)
        {
            if (updated == null) return;
            lock (_stateLock)
            {
                Settings = updated;
                _globalLimiter.SetLimitKBps(Settings.GlobalSpeedLimitKBps);
                if (!Directory.Exists(Settings.DefaultSaveFolder))
                {
                    try { Directory.CreateDirectory(Settings.DefaultSaveFolder); } catch { }
                }
            }
            SaveState();
            MarkDirtyAndNotify();
            TryStartNextQueued();
        }

        public async Task<Tuple<string, string>> ComputeChecksumsAsync(string id)
        {
            string path = null;
            lock (_stateLock)
            {
                var item = Downloads.FirstOrDefault(x => x.Id == id);
                if (item != null) path = item.FinalFilePath;
            }
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return Tuple.Create("", "");

            return await Task.Run(delegate
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 262144))
                using (SHA256 sha = SHA256.Create())
                using (MD5 md5 = MD5.Create())
                {
                    byte[] buf = new byte[262144];
                    int read;
                    while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                    {
                        sha.TransformBlock(buf, 0, read, null, 0);
                        md5.TransformBlock(buf, 0, read, null, 0);
                    }
                    sha.TransformFinalBlock(buf, 0, 0);
                    md5.TransformFinalBlock(buf, 0, 0);

                    string shaHex = BitConverter.ToString(sha.Hash).Replace("-", "").ToLowerInvariant();
                    string md5Hex = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();

                    lock (_stateLock)
                    {
                        var item = Downloads.FirstOrDefault(x => x.Id == id);
                        if (item != null)
                        {
                            item.Sha256 = shaHex;
                            item.Md5 = md5Hex;
                        }
                    }
                    MarkDirtyAndNotify();
                    return Tuple.Create(shaHex, md5Hex);
                }
            }).ConfigureAwait(false);
        }

        private void TickQueueAndScheduler()
        {
            bool changed = false;
            bool shouldTriggerPostAction = false;
            PostQueueAction postAction = PostQueueAction.None;

            lock (_stateLock)
            {
                DateTime now = DateTime.Now;
                // Check scheduled downloads
                foreach (var d in Downloads)
                {
                    if (d.Status == DownloadStatus.Scheduled && d.ScheduledFor.HasValue && now >= d.ScheduledFor.Value)
                    {
                        d.Status = DownloadStatus.Queued;
                        d.StatusDetail = "Scheduled start";
                        changed = true;
                    }
                }

                // Night Free Traffic Mode (e.g., 02:00 - 07:00)
                if (Settings.Scheduler.NightModeEnabled)
                {
                    TimeSpan startTs, endTs;
                    if (TimeSpan.TryParse(Settings.Scheduler.StartTime, out startTs) &&
                        TimeSpan.TryParse(Settings.Scheduler.EndTime, out endTs))
                    {
                        TimeSpan cur = now.TimeOfDay;
                        bool inWindow = startTs <= endTs
                            ? (cur >= startTs && cur <= endTs)
                            : (cur >= startTs || cur <= endTs);

                        if (inWindow && !_nightWindowTriggered)
                        {
                            _nightWindowTriggered = true;
                            _queueRunning = true;
                            foreach (var d in Downloads)
                            {
                                if (d.Status == DownloadStatus.Paused || d.Status == DownloadStatus.Scheduled)
                                {
                                    d.Status = DownloadStatus.Queued;
                                    d.StatusDetail = "Night Free Mode";
                                    changed = true;
                                }
                            }
                        }
                        else if (!inWindow && _nightWindowTriggered)
                        {
                            _nightWindowTriggered = false;
                            // Pause active downloads when night free traffic window closes
                            foreach (var d in Downloads.ToList())
                            {
                                if (d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Queued || d.Status == DownloadStatus.Probing)
                                {
                                    CancellationTokenSource cts;
                                    CancelAndAbortItem(d.Id, out cts);
                                    d.Status = DownloadStatus.Paused;
                                    d.SpeedBps = 0;
                                    d.StatusDetail = "Night window ended";
                                    changed = true;
                                }
                            }
                        }
                    }
                }

                // Speed telemetry aggregation for active downloads
                foreach (var d in Downloads)
                {
                    if (d.Status == DownloadStatus.Downloading)
                    {
                        double totalSpeed = 0;
                        long totalDone = 0;
                        foreach (var s in d.Segments)
                        {
                            totalSpeed += s.SpeedBps;
                            totalDone += s.DownloadedBytes;
                        }
                        d.SpeedBps = totalSpeed;
                        if (totalSpeed > d.PeakSpeedBps) d.PeakSpeedBps = totalSpeed;
                        if (d.TotalBytes > 0)
                        {
                            d.DownloadedBytes = Math.Min(d.TotalBytes, totalDone);
                            long rem = Math.Max(0, d.TotalBytes - d.DownloadedBytes);
                            d.EtaSeconds = totalSpeed > 100 ? (rem / totalSpeed) : 0;
                        }
                        else
                        {
                            d.DownloadedBytes = totalDone;
                        }
                        d.SpeedHistory.Add(totalSpeed);
                        if (d.SpeedHistory.Count > 60) d.SpeedHistory.RemoveAt(0);
                        changed = true;
                    }
                }

                // Check if queue finished and post-queue action needed
                if (Settings.Scheduler.PostAction != PostQueueAction.None &&
                    Settings.Scheduler.PostAction != PostQueueAction.Notify)
                {
                    bool anyRunning = Downloads.Any(x => x.Status == DownloadStatus.Downloading || x.Status == DownloadStatus.Queued || x.Status == DownloadStatus.Probing);
                    if (!anyRunning && Downloads.Any(x => x.Status == DownloadStatus.Completed && x.CompletedAt.HasValue && (DateTime.UtcNow - x.CompletedAt.Value).TotalSeconds < 3))
                    {
                        postAction = Settings.Scheduler.PostAction;
                        Settings.Scheduler.PostAction = PostQueueAction.None;
                        shouldTriggerPostAction = true;
                    }
                }
            }

            if (changed)
            {
                _stateDirty = true;
                var handler = OnStateUpdated;
                if (handler != null) handler();
            }

            TryStartNextQueued();

            if (shouldTriggerPostAction)
            {
                var pHandler = OnPostQueueActionRequested;
                if (pHandler != null) pHandler(postAction);
            }
        }

        private void TryStartNextQueued()
        {
            if (!_queueRunning) return;

            List<DownloadItem> toStart = new List<DownloadItem>();
            lock (_stateLock)
            {
                int runningCount = Downloads.Count(x => x.Status == DownloadStatus.Downloading || x.Status == DownloadStatus.Probing || x.Status == DownloadStatus.Merging);
                int slots = Math.Max(1, Settings.MaxConcurrentDownloads) - runningCount;
                if (slots <= 0) return;

                foreach (var d in Downloads.Where(x => x.Status == DownloadStatus.Queued).Take(slots))
                {
                    d.Status = DownloadStatus.Probing;
                    d.StatusDetail = "Probing server...";
                    var cts = new CancellationTokenSource();
                    _activeTokens[d.Id] = cts;
                    if (!_itemLimiters.ContainsKey(d.Id)) _itemLimiters[d.Id] = new TokenBucketLimiter(d.SpeedLimitKBps);
                    toStart.Add(d);
                }
            }

            foreach (var item in toStart)
            {
                CancellationToken ct;
                lock (_stateLock)
                {
                    if (!_activeTokens.ContainsKey(item.Id)) continue;
                    ct = _activeTokens[item.Id].Token;
                }
                Task.Run(delegate { ExecuteDownloadAsync(item, ct); });
            }
        }

        private string GetActiveUrl(DownloadItem item)
        {
            if (item.ActiveMirrorIndex >= 0 && item.ActiveMirrorIndex < item.MirrorUrls.Count)
            {
                return item.MirrorUrls[item.ActiveMirrorIndex];
            }
            return item.Url;
        }

        private async Task ExecuteDownloadAsync(DownloadItem item, CancellationToken ct)
        {
            try
            {
                // Optional DoH warmup
                if (Settings.Doh != null && Settings.Doh.Enabled)
                {
                    try
                    {
                        Uri u = new Uri(GetActiveUrl(item));
                        await NetworkHelper.ResolveDohAsync(u.Host, Settings.Doh).ConfigureAwait(false);
                    }
                    catch { }
                }

                // Probe only if we don't already have segments & size from a prior paused run
                if (item.Segments.Count == 0 || item.TotalBytes <= 0)
                {
                    ProbeResult probe = await ProbeUrlAsync(GetActiveUrl(item), item.Referer, item.Cookies, item.UserAgent, ct, item.Id).ConfigureAwait(false);
                    if (ct.IsCancellationRequested) return;

                    // Try mirror fallback if primary probe failed
                    if (!probe.Success && item.MirrorUrls.Count > 0)
                    {
                        for (int m = 0; m < item.MirrorUrls.Count; m++)
                        {
                            if (ct.IsCancellationRequested) return;
                            item.ActiveMirrorIndex = m;
                            probe = await ProbeUrlAsync(GetActiveUrl(item), item.Referer, item.Cookies, item.UserAgent, ct, item.Id).ConfigureAwait(false);
                            if (probe.Success) break;
                        }
                    }

                    if (ct.IsCancellationRequested) return;
                    if (!probe.Success)
                    {
                        FailItem(item, probe.Error ?? "Failed to connect to server.");
                        return;
                    }

                    lock (_stateLock)
                    {
                        if (ct.IsCancellationRequested || item.Status == DownloadStatus.Paused) return;
                        item.TotalBytes = probe.TotalBytes;
                        item.SupportsRange = probe.SupportsRange && probe.TotalBytes > 262144; // >256KB for multi-stream
                        item.MimeType = probe.MimeType;
                        item.ETag = probe.ETag;
                        if (!string.IsNullOrEmpty(probe.FileName) && item.DownloadedBytes == 0)
                        {
                            string oldCat = item.Category;
                            item.Category = CategorizeFile(probe.FileName, probe.MimeType);
                            item.SaveFolder = ResolveSaveFolder(item.Category, null);
                            item.FinalFilePath = GetUniqueFilePath(item.SaveFolder, probe.FileName);
                            item.FileName = Path.GetFileName(item.FinalFilePath);
                            item.TempFilePath = item.FinalFilePath + ".kidown";
                        }

                        item.Segments.Clear();
                        if (item.SupportsRange)
                        {
                            int segCount = Math.Min(item.MaxConnections, (int)Math.Max(1, item.TotalBytes / (256 * 1024)));
                            long partSize = item.TotalBytes / segCount;
                            for (int i = 0; i < segCount; i++)
                            {
                                long start = i * partSize;
                                long end = (i == segCount - 1) ? (item.TotalBytes - 1) : ((i + 1) * partSize - 1);
                                item.Segments.Add(new SegmentState
                                {
                                    Index = i,
                                    StartByte = start,
                                    EndByte = end,
                                    CurrentByte = start,
                                    Status = SegmentStatus.Pending
                                });
                            }
                        }
                        else
                        {
                            item.Segments.Add(new SegmentState
                            {
                                Index = 0,
                                StartByte = 0,
                                EndByte = item.TotalBytes > 0 ? item.TotalBytes - 1 : -1,
                                CurrentByte = 0,
                                Status = SegmentStatus.Pending
                            });
                        }
                    }
                }

                // Ensure temp file exists and is pre-sized when total size known
                using (FileStream fs = new FileStream(item.TempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    if (item.TotalBytes > 0 && fs.Length != item.TotalBytes)
                    {
                        fs.SetLength(item.TotalBytes);
                    }
                }

                lock (_stateLock)
                {
                    if (ct.IsCancellationRequested || item.Status == DownloadStatus.Paused) return;
                    item.Status = DownloadStatus.Downloading;
                    item.StatusDetail = item.SupportsRange
                        ? string.Format("Streaming ({0} connections)", item.Segments.Count)
                        : "Streaming (single stream)";
                }
                MarkDirtyAndNotify();

                if (!item.SupportsRange)
                {
                    await DownloadSingleStreamAsync(item, item.Segments[0], ct).ConfigureAwait(false);
                }
                else
                {
                    await RunParallelSegmentedAsync(item, ct).ConfigureAwait(false);
                }

                if (ct.IsCancellationRequested) return;

                // Verify completion
                bool allDone;
                lock (_stateLock)
                {
                    allDone = item.Segments.All(s => s.Status == SegmentStatus.Done);
                }

                if (allDone)
                {
                    lock (_stateLock)
                    {
                        item.Status = DownloadStatus.Merging;
                        item.StatusDetail = "Finalizing file...";
                    }
                    MarkDirtyAndNotify();

                    if (File.Exists(item.FinalFilePath))
                    {
                        item.FinalFilePath = GetUniqueFilePath(item.SaveFolder, item.FileName);
                        item.FileName = Path.GetFileName(item.FinalFilePath);
                    }
                    File.Move(item.TempFilePath, item.FinalFilePath);

                    lock (_stateLock)
                    {
                        if (item.TotalBytes <= 0 && File.Exists(item.FinalFilePath))
                        {
                            item.TotalBytes = new FileInfo(item.FinalFilePath).Length;
                        }
                        item.DownloadedBytes = item.TotalBytes;
                        item.SpeedBps = 0;
                        item.EtaSeconds = 0;
                        item.Status = DownloadStatus.Completed;
                        item.StatusDetail = "Completed";
                        item.CompletedAt = DateTime.UtcNow;
                        _activeTokens.Remove(item.Id);
                    }
                    SaveState();
                    MarkDirtyAndNotify();
                    var doneHandler = OnDownloadCompleted;
                    if (doneHandler != null) doneHandler(item);
                }
            }
            catch (OperationCanceledException)
            {
                // Paused or removed
            }
            catch (Exception ex)
            {
                if (!ct.IsCancellationRequested)
                {
                    if (ex.Message != null && ex.Message.Contains("Connection interrupted"))
                    {
                        Pause(item.Id);
                        lock (_stateLock)
                        {
                            item.StatusDetail = "Interrupted (Resumable)";
                        }
                        MarkDirtyAndNotify();
                    }
                    else
                    {
                        FailItem(item, ex.Message);
                    }
                }
            }
            finally
            {
                TryStartNextQueued();
            }
        }

        private async Task RunParallelSegmentedAsync(DownloadItem item, CancellationToken ct)
        {
            List<Task> workers = new List<Task>();
            List<SegmentState> initial;
            lock (_stateLock)
            {
                initial = item.Segments.Where(s => s.Status != SegmentStatus.Done).ToList();
            }

            foreach (var seg in initial)
            {
                var captured = seg;
                workers.Add(Task.Run(delegate { return SegmentWorkerLoopAsync(item, captured, ct); }, ct));
            }

            await Task.WhenAll(workers).ConfigureAwait(false);
        }

        private async Task SegmentWorkerLoopAsync(DownloadItem item, SegmentState seg, CancellationToken ct)
        {
            SegmentState currentSeg = seg;
            while (!ct.IsCancellationRequested)
            {
                await DownloadSegmentWithIranResilienceAsync(item, currentSeg, ct).ConfigureAwait(false);
                if (ct.IsCancellationRequested) return;

                if (currentSeg.Status == SegmentStatus.Error) return;

                // Dynamic Segment Stealing (Iran-Resilient Mode):
                // When a fast stream finishes early, split the largest remaining slow/stalled segment in half!
                SegmentState stolen = null;
                if (item.IranResilientMode && Settings.DynamicSegmentStealing)
                {
                    lock (_stateLock)
                    {
                        if (item.Segments.Count < 48)
                        {
                            SegmentState target = item.Segments
                                .Where(s => s.Status != SegmentStatus.Done && s.RemainingBytes > 512 * 1024)
                                .OrderByDescending(s => s.RemainingBytes)
                                .FirstOrDefault();

                            if (target != null)
                            {
                                long splitPoint = target.CurrentByte + (target.RemainingBytes / 2);
                                if (splitPoint > target.CurrentByte + 65536 && splitPoint < target.EndByte - 65536)
                                {
                                    long oldEnd = target.EndByte;
                                    target.EndByte = splitPoint - 1;
                                    stolen = new SegmentState
                                    {
                                        Index = item.Segments.Count,
                                        StartByte = splitPoint,
                                        EndByte = oldEnd,
                                        CurrentByte = splitPoint,
                                        Status = SegmentStatus.Stealing
                                    };
                                    item.Segments.Add(stolen);
                                    item.SegmentSteals++;
                                }
                            }
                        }
                    }
                }

                if (stolen == null) break;
                currentSeg = stolen;
            }
        }

        private async Task DownloadSegmentWithIranResilienceAsync(DownloadItem item, SegmentState seg, CancellationToken ct)
        {
            int maxRetries = item.IranResilientMode ? Math.Max(10, Settings.MaxRetriesPerSegment) : 5;
            byte[] buffer = new byte[65536]; // 64KB chunk buffer
            TokenBucketLimiter itemLimiter = _itemLimiters.ContainsKey(item.Id) ? _itemLimiters[item.Id] : null;

            while (!ct.IsCancellationRequested)
            {
                long startPos;
                long endPos;
                lock (_stateLock)
                {
                    startPos = seg.CurrentByte;
                    endPos = seg.EndByte;
                    if (startPos > endPos)
                    {
                        seg.Status = SegmentStatus.Done;
                        seg.SpeedBps = 0;
                        return;
                    }
                    seg.Status = SegmentStatus.Connecting;
                }

                int retryDelayMs = 0;
                HttpWebRequest req = null;
                HttpWebResponse resp = null;
                Stream stream = null;
                CancellationTokenSource stallCts = null;
                try
                {
                    if (ct.IsCancellationRequested) return;
                    string activeUrl = GetActiveUrl(item);
                    req = CreateRequest(activeUrl, "GET", item.Referer, item.Cookies, item.UserAgent);
                    req.Timeout = 15000;
                    req.ReadWriteTimeout = 10000;
                    req.AddRange(startPos, endPos);

                    RegisterActiveRequest(item.Id, req);
                    stallCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    using (ct.Register(delegate
                    {
                        try { if (stream != null) stream.Dispose(); } catch { }
                        try { if (resp != null) resp.Close(); } catch { }
                        try { if (req != null) req.Abort(); } catch { }
                    }))
                    {
                        resp = (HttpWebResponse)await req.GetResponseAsync().ConfigureAwait(false);
                        RegisterActiveStream(item.Id, resp);
                        stream = resp.GetResponseStream();
                        RegisterActiveStream(item.Id, stream);
                        if (ct.IsCancellationRequested) return;

                        // Check if server ignored Range on a non-zero offset
                        if (startPos > 0 && resp.StatusCode == HttpStatusCode.OK)
                        {
                            throw new IOException("Server returned 200 OK instead of 206 Partial Content.");
                        }

                        lock (_stateLock)
                        {
                            if (ct.IsCancellationRequested || item.Status == DownloadStatus.Paused) return;
                            seg.Status = SegmentStatus.Streaming;
                        }

                        long bytesSinceLastTick = 0;
                        long lastTickBytesTotal = 0;
                        DateTime lastRateCheck = DateTime.UtcNow;
                        DateTime stallStart = DateTime.UtcNow;

                        // Watchdog timer to abort stalled Iran sockets
                        using (System.Threading.Timer watchdog = new System.Threading.Timer(delegate
                        {
                            if (!item.IranResilientMode || ct.IsCancellationRequested) return;
                            double elapsed = (DateTime.UtcNow - stallStart).TotalSeconds;
                            if (elapsed >= Settings.StallTimeoutSeconds)
                            {
                                try
                                {
                                    lock (_stateLock)
                                    {
                                        if (ct.IsCancellationRequested) return;
                                        seg.Status = SegmentStatus.Stalled;
                                        seg.StallsRecovered++;
                                        item.StallsRecovered++;
                                    }
                                    stallCts.Cancel();
                                    if (stream != null) stream.Dispose();
                                    if (req != null) req.Abort();
                                }
                                catch { }
                            }
                        }, null, 1000, 1000))
                        {
                            while (!stallCts.IsCancellationRequested && !ct.IsCancellationRequested)
                            {
                                long remaining;
                                lock (_stateLock)
                                {
                                    remaining = seg.EndByte - seg.CurrentByte + 1;
                                }
                                if (remaining <= 0)
                                {
                                    lock (_stateLock)
                                    {
                                        seg.Status = SegmentStatus.Done;
                                        seg.SpeedBps = 0;
                                    }
                                    return;
                                }

                                int toRead = (int)Math.Min(buffer.Length, remaining);
                                int read = await stream.ReadAsync(buffer, 0, toRead, stallCts.Token).ConfigureAwait(false);
                                if (ct.IsCancellationRequested || stallCts.IsCancellationRequested) return;
                                if (read <= 0) break;

                                if (Settings.GlobalSpeedLimitKBps > 0)
                                    await _globalLimiter.ConsumeAsync(read, stallCts.Token).ConfigureAwait(false);
                                if (itemLimiter != null && item.SpeedLimitKBps > 0)
                                    await itemLimiter.ConsumeAsync(read, stallCts.Token).ConfigureAwait(false);
                                if (ct.IsCancellationRequested) return;

                                long writePos;
                                lock (_stateLock)
                                {
                                    if (ct.IsCancellationRequested || item.Status == DownloadStatus.Paused) return;
                                    long maxAllowed = seg.EndByte - seg.CurrentByte + 1;
                                    if (maxAllowed <= 0)
                                    {
                                        seg.Status = SegmentStatus.Done;
                                        seg.SpeedBps = 0;
                                        return;
                                    }
                                    if (read > maxAllowed) read = (int)maxAllowed;
                                    writePos = seg.CurrentByte;
                                    seg.CurrentByte += read;
                                }

                                lock (_fileIoLock)
                                {
                                    using (FileStream fs = new FileStream(item.TempFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                                    {
                                        fs.Position = writePos;
                                        fs.Write(buffer, 0, read);
                                    }
                                }

                                bytesSinceLastTick += read;
                                lastTickBytesTotal += read;

                                double dt = (DateTime.UtcNow - lastRateCheck).TotalSeconds;
                                if (dt >= 0.35)
                                {
                                    double instSpeed = bytesSinceLastTick / dt;
                                    lock (_stateLock)
                                    {
                                        if (!ct.IsCancellationRequested)
                                            seg.SpeedBps = seg.SpeedBps <= 0 ? instSpeed : (seg.SpeedBps * 0.45 + instSpeed * 0.55);
                                    }
                                    if (instSpeed >= Settings.MinStallSpeedKBps * 1024.0)
                                    {
                                        stallStart = DateTime.UtcNow;
                                    }
                                    bytesSinceLastTick = 0;
                                    lastRateCheck = DateTime.UtcNow;
                                }
                                else if (lastTickBytesTotal > 32768)
                                {
                                    stallStart = DateTime.UtcNow;
                                    lastTickBytesTotal = 0;
                                }
                            }
                        }

                        if (ct.IsCancellationRequested) return;
                        lock (_stateLock)
                        {
                            if (seg.CurrentByte > seg.EndByte)
                            {
                                seg.Status = SegmentStatus.Done;
                                seg.SpeedBps = 0;
                                return;
                            }
                        }
                        throw new IOException("Stream closed prematurely before segment completion.");
                    }
                }
                catch (Exception ex)
                {
                    if (ct.IsCancellationRequested) return;

                    int retryCount;
                    lock (_stateLock)
                    {
                        seg.Retries++;
                        item.TotalRetries++;
                        retryCount = seg.Retries;
                        seg.SpeedBps = 0;
                        seg.Status = SegmentStatus.Retrying;
                        seg.LastError = ex.Message;

                        // Rotate mirror if available after every 3 consecutive failures
                        if (item.MirrorUrls.Count > 0 && retryCount % 3 == 0)
                        {
                            item.ActiveMirrorIndex = (item.ActiveMirrorIndex + 1) % (item.MirrorUrls.Count + 1);
                            if (item.ActiveMirrorIndex == item.MirrorUrls.Count) item.ActiveMirrorIndex = -1; // back to primary
                        }
                    }

                    if (retryCount > maxRetries)
                    {
                        lock (_stateLock)
                        {
                            seg.Status = SegmentStatus.Error;
                            seg.LastError = ex.Message;
                        }
                        throw new IOException(string.Format("Segment {0} failed after {1} retries: {2}", seg.Index, maxRetries, ex.Message));
                    }

                    // When internet drops: either pause cleanly like IDM (resumable from exact byte) or hold/wait if AutoResumeOnReconnect is enabled
                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (!Settings.AutoResumeOnReconnect)
                        {
                            throw new IOException("Connection interrupted (Paused - Resumable)");
                        }
                        lock (_stateLock)
                        {
                            seg.Retries = Math.Max(0, seg.Retries - 1);
                            item.StatusDetail = "Waiting for network...";
                        }
                        retryDelayMs = 2000;
                    }
                    else
                    {
                        // Exponential backoff with random jitter (350ms up to 10s)
                        int baseDelay = (int)Math.Min(10000, 350 * Math.Pow(1.5, Math.Min(10, retryCount - 1)));
                        int jitter;
                        lock (_rng) { jitter = _rng.Next(100, 600); }
                        retryDelayMs = baseDelay + jitter;
                    }
                }
                finally
                {
                    UnregisterActiveStream(item.Id, stream);
                    UnregisterActiveStream(item.Id, resp);
                    UnregisterActiveRequest(item.Id, req);
                    try { if (stream != null) stream.Dispose(); } catch { }
                    try { if (resp != null) resp.Close(); } catch { }
                    if (stallCts != null) stallCts.Dispose();
                }

                if (retryDelayMs > 0)
                {
                    await Task.Delay(retryDelayMs, ct).ConfigureAwait(false);
                }
            }
        }

        private async Task DownloadSingleStreamAsync(DownloadItem item, SegmentState seg, CancellationToken ct)
        {
            byte[] buffer = new byte[65536];
            TokenBucketLimiter itemLimiter = _itemLimiters.ContainsKey(item.Id) ? _itemLimiters[item.Id] : null;
            int retries = 0;

            while (!ct.IsCancellationRequested)
            {
                int singleDelayMs = 0;
                HttpWebRequest req = null;
                HttpWebResponse resp = null;
                Stream stream = null;
                try
                {
                    lock (_stateLock)
                    {
                        if (ct.IsCancellationRequested || item.Status == DownloadStatus.Paused) return;
                        seg.CurrentByte = 0;
                        seg.Status = SegmentStatus.Connecting;
                    }

                    req = CreateRequest(GetActiveUrl(item), "GET", item.Referer, item.Cookies, item.UserAgent);
                    req.Timeout = 20000;
                    RegisterActiveRequest(item.Id, req);
                    using (ct.Register(delegate
                    {
                        try { if (stream != null) stream.Dispose(); } catch { }
                        try { if (resp != null) resp.Close(); } catch { }
                        try { if (req != null) req.Abort(); } catch { }
                    }))
                    {
                        resp = (HttpWebResponse)await req.GetResponseAsync().ConfigureAwait(false);
                        RegisterActiveStream(item.Id, resp);
                        stream = resp.GetResponseStream();
                        RegisterActiveStream(item.Id, stream);
                        using (FileStream fs = new FileStream(item.TempFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            lock (_stateLock)
                            {
                                if (ct.IsCancellationRequested || item.Status == DownloadStatus.Paused) return;
                                seg.Status = SegmentStatus.Streaming;
                            }
                            long bytesTick = 0;
                            DateTime lastCheck = DateTime.UtcNow;

                            int read;
                            while (!ct.IsCancellationRequested && (read = await stream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
                            {
                                if (ct.IsCancellationRequested) return;
                                if (Settings.GlobalSpeedLimitKBps > 0)
                                    await _globalLimiter.ConsumeAsync(read, ct).ConfigureAwait(false);
                                if (itemLimiter != null && item.SpeedLimitKBps > 0)
                                    await itemLimiter.ConsumeAsync(read, ct).ConfigureAwait(false);
                                if (ct.IsCancellationRequested) return;

                                fs.Write(buffer, 0, read);
                                bytesTick += read;
                                lock (_stateLock)
                                {
                                    if (ct.IsCancellationRequested || item.Status == DownloadStatus.Paused) return;
                                    seg.CurrentByte += read;
                                    double dt = (DateTime.UtcNow - lastCheck).TotalSeconds;
                                    if (dt >= 0.35)
                                    {
                                        seg.SpeedBps = bytesTick / dt;
                                        bytesTick = 0;
                                        lastCheck = DateTime.UtcNow;
                                    }
                                }
                            }

                            if (ct.IsCancellationRequested) return;
                            lock (_stateLock)
                            {
                                seg.Status = SegmentStatus.Done;
                                seg.SpeedBps = 0;
                            }
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ct.IsCancellationRequested) return;
                    retries++;
                    lock (_stateLock)
                    {
                        seg.Retries = retries;
                        item.TotalRetries++;
                        seg.Status = SegmentStatus.Retrying;
                        seg.LastError = ex.Message;
                    }
                    if (retries > 8) throw;
                    singleDelayMs = 1200 * retries;
                }
                finally
                {
                    UnregisterActiveStream(item.Id, stream);
                    UnregisterActiveStream(item.Id, resp);
                    UnregisterActiveRequest(item.Id, req);
                    try { if (stream != null) stream.Dispose(); } catch { }
                    try { if (resp != null) resp.Close(); } catch { }
                }
                if (singleDelayMs > 0)
                {
                    await Task.Delay(singleDelayMs, ct).ConfigureAwait(false);
                }
            }
        }

        private void FailItem(DownloadItem item, string message)
        {
            lock (_stateLock)
            {
                item.Status = DownloadStatus.Error;
                item.SpeedBps = 0;
                item.EtaSeconds = 0;
                item.ErrorMessage = message;
                item.StatusDetail = "Error: " + message;
                _activeTokens.Remove(item.Id);
            }
            MarkDirtyAndNotify();
        }

        public string GetSnapshotJson()
        {
            lock (_stateLock)
            {
                var payload = new
                {
                    downloads = Downloads,
                    settings = Settings,
                    queueRunning = _queueRunning,
                    totalSpeedBps = Downloads.Where(x => x.Status == DownloadStatus.Downloading).Sum(x => x.SpeedBps),
                    activeCount = Downloads.Count(x => x.Status == DownloadStatus.Downloading || x.Status == DownloadStatus.Probing),
                    queuedCount = Downloads.Count(x => x.Status == DownloadStatus.Queued)
                };
                return _json.Serialize(payload);
            }
        }

        private void MarkDirtyAndNotify()
        {
            _stateDirty = true;
            var handler = OnStateUpdated;
            if (handler != null) handler();
        }

        public void SaveState()
        {
            try
            {
                string jsonStr;
                lock (_stateLock)
                {
                    _stateDirty = false;
                    var state = new PersistedState
                    {
                        Settings = Settings,
                        Downloads = Downloads
                    };
                    jsonStr = _json.Serialize(state);
                }
                string tmp = _statePath + ".tmp";
                File.WriteAllText(tmp, jsonStr, Encoding.UTF8);
                if (File.Exists(_statePath)) File.Delete(_statePath);
                File.Move(tmp, _statePath);
            }
            catch { }
        }

        private void LoadState()
        {
            try
            {
                if (!File.Exists(_statePath)) return;
                string raw = File.ReadAllText(_statePath, Encoding.UTF8);
                PersistedState state = _json.Deserialize<PersistedState>(raw);
                if (state != null)
                {
                    if (state.Settings != null) Settings = state.Settings;
                    if (state.Downloads != null) Downloads = state.Downloads;
                }
            }
            catch { }
        }

        public void Dispose()
        {
            try { if (_persistTimer != null) _persistTimer.Dispose(); } catch { }
            try { if (_schedulerTimer != null) _schedulerTimer.Dispose(); } catch { }
            PauseAll();
            SaveState();
            _socksRelay.Dispose();
        }

        private class PersistedState
        {
            public AppSettings Settings { get; set; }
            public List<DownloadItem> Downloads { get; set; }
        }
    }
}
