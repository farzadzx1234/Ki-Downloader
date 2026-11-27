using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;

namespace KiDownloader
{
    public class MainForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NCCALCSIZE_PARAMS
        {
            public RECT rgrc0;
            public RECT rgrc1;
            public RECT rgrc2;
            public IntPtr lppos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        private readonly DownloadEngine _engine;
        private readonly IpcServer _ipc;
        private readonly string _baseDir;
        private readonly string _uiDir;
        private readonly string _dataDir;
        private readonly JavaScriptSerializer _json = new JavaScriptSerializer { MaxJsonLength = 32 * 1024 * 1024 };

        private WebView2 _webView;
        private NotifyIcon _trayIcon;
        private TrayPopupForm _trayPopup;
        private System.Windows.Forms.Timer _uiPushTimer;
        private bool _uiReady;
        private bool _allowExit;
        private string _lastClipboardUrl = "";
        private string _pendingQuickAddJson;

        private static readonly Regex UrlExtractRegex = new Regex(
            @"https?://[^\s""'<>\]\)]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly HashSet<string> DownloadableExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".zip", ".rar", ".7z", ".tar", ".gz", ".xz", ".bz2", ".iso", ".img",
            ".exe", ".msi", ".msix", ".apk", ".dmg", ".deb", ".rpm",
            ".mp4", ".mkv", ".webm", ".avi", ".mov", ".flv", ".ts",
            ".mp3", ".flac", ".wav", ".aac", ".ogg", ".m4a",
            ".pdf", ".epub", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".bin", ".dat", ".torrent"
        };

        public MainForm(string baseDir)
        {
            _baseDir = baseDir;
            _dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ki-Downloader");
            _uiDir = Path.Combine(baseDir, "ui");
            string assetsDir = Path.Combine(baseDir, "assets");
            try { AppIcons.SaveAssets(assetsDir); } catch { }

            _engine = new DownloadEngine(_dataDir);
            _engine.OnDownloadCompleted += HandleDownloadCompleted;
            _engine.OnPostQueueActionRequested += HandlePostQueueAction;

            _ipc = new IpcServer(_engine, HandleExternalAdd, RestoreWindowFromTray);
            _ipc.Start();

            Text = "Ki-Downloader";
            Width = 1140;
            Height = 760;
            MinimumSize = new Size(860, 580);
            StartPosition = FormStartPosition.CenterScreen;
            Icon = AppIcons.CreateAppIcon(128);
            UpdateFormColors();

            // Subtle 1px edge keeps native resize borders crisp without a thick frame
            Padding = new Padding(1);

            SetupTrayIcon();
            SetupWebView();

            SystemEvents.UserPreferenceChanged += delegate { ApplyDwmThemeAttributes(); };

            _uiPushTimer = new System.Windows.Forms.Timer();
            _uiPushTimer.Interval = 400;
            _uiPushTimer.Tick += delegate { PushStateToUi(); };
            _uiPushTimer.Start();
        }

        private bool EffectiveDarkMode()
        {
            string th = (_engine != null && _engine.Settings != null) ? (_engine.Settings.Theme ?? "auto") : "auto";
            if (th == "dark") return true;
            if (th == "light") return false;
            return IsWindowsDarkTheme();
        }

        private void UpdateFormColors()
        {
            bool dark = EffectiveDarkMode();
            BackColor = dark ? Color.Black : Color.White;
            if (_webView != null) _webView.DefaultBackgroundColor = dark ? Color.Black : Color.White;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (_webView != null && !_webView.Focused)
            {
                try { _webView.Focus(); } catch { }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyDwmThemeAttributes();
            try { AddClipboardFormatListener(Handle); } catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowExit && _engine.Settings.MinimizeToTray && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            try { RemoveClipboardFormatListener(Handle); } catch { }
            try { if (_uiPushTimer != null) _uiPushTimer.Stop(); } catch { }
            try { if (_trayIcon != null) _trayIcon.Visible = false; } catch { }
            _ipc.Dispose();
            _engine.Dispose();
            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Padding = WindowState == FormWindowState.Maximized ? new Padding(6) : new Padding(1);
            SendMessageToUi("window_state", _json.Serialize(new { maximized = WindowState == FormWindowState.Maximized }));
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCCALCSIZE && m.WParam != IntPtr.Zero)
            {
                // Strip standard Windows title bar while keeping native resize/Aero Snap/rounded corners
                m.Result = IntPtr.Zero;
                return;
            }

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if ((int)m.Result == 1) // HTCLIENT
                {
                    Point p = PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF, (m.LParam.ToInt32() >> 16) & 0xFFFF));
                    int grip = 8;
                    bool left = p.X < grip;
                    bool right = p.X >= Width - grip;
                    bool top = p.Y < grip;
                    bool bottom = p.Y >= Height - grip;
                    if (top && left) { m.Result = (IntPtr)HTTOPLEFT; return; }
                    if (top && right) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                    if (bottom && left) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                    if (bottom && right) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                    if (left) { m.Result = (IntPtr)HTLEFT; return; }
                    if (right) { m.Result = (IntPtr)HTRIGHT; return; }
                    if (top) { m.Result = (IntPtr)HTTOP; return; }
                    if (bottom) { m.Result = (IntPtr)HTBOTTOM; return; }
                }
                return;
            }

            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                OnClipboardChanged();
            }
            base.WndProc(ref m);
        }

        private void ApplyDwmThemeAttributes()
        {
            if (Handle == IntPtr.Zero) return;
            try
            {
                UpdateFormColors();
                int dark = EffectiveDarkMode() ? 1 : 0;
                DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
                int corner = 2; // DWMWCP_ROUND
                DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
                MARGINS margins = new MARGINS { cxLeftWidth = 1, cxRightWidth = 1, cyTopHeight = 1, cyBottomHeight = 1 };
                DwmExtendFrameIntoClientArea(Handle, ref margins);
            }
            catch { }
        }

        private static bool IsWindowsDarkTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AppsUseLightTheme");
                        if (val != null) return Convert.ToInt32(val) == 0;
                    }
                }
            }
            catch { }
            return true;
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = AppIcons.CreateTrayIcon(32),
                Text = "Ki-Downloader",
                Visible = true
            };
            _trayIcon.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
                {
                    ShowTrayPopup();
                }
            };
            _trayIcon.DoubleClick += delegate { RestoreWindowFromTray(); };
        }

        private void ShowTrayPopup()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(ShowTrayPopup));
                return;
            }
            try
            {
                if (_trayPopup != null && !_trayPopup.IsDisposed)
                {
                    _trayPopup.Close();
                    _trayPopup = null;
                    return;
                }
            }
            catch { }

            _trayPopup = new TrayPopupForm(
                _engine,
                _baseDir,
                EffectiveDarkMode(),
                delegate { RestoreWindowFromTray(); },
                delegate
                {
                    RestoreWindowFromTray();
                    SendMessageToUi("quick_add", _json.Serialize(new { url = "", source = "tray" }));
                },
                delegate
                {
                    _allowExit = true;
                    Close();
                });
            _trayPopup.FormClosed += delegate { _trayPopup = null; };
            _trayPopup.ShowNearCursor();
        }

        private void RestoreWindowFromTray()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RestoreWindowFromTray));
                return;
            }
            Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
        }

        private async void SetupWebView()
        {
            _webView = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = EffectiveDarkMode() ? Color.Black : Color.White
            };
            Controls.Add(_webView);

            string userData = Path.Combine(_dataDir, "WebView2Cache");
            try { CoreWebView2Environment.SetLoaderDllFolderPath(_baseDir); } catch { }
            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, userData);
            await _webView.EnsureCoreWebView2Async(env);

            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.ki-downloader.local",
                _uiDir,
                CoreWebView2HostResourceAccessKind.Allow);

            _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            _webView.CoreWebView2.NavigationCompleted += delegate
            {
                _uiReady = true;
                try { _webView.Focus(); } catch { }
                PushStateToUi();
                SendMessageToUi("window_state", _json.Serialize(new { maximized = WindowState == FormWindowState.Maximized }));
                if (!string.IsNullOrEmpty(_pendingQuickAddJson))
                {
                    SendMessageToUi("quick_add", _pendingQuickAddJson);
                    _pendingQuickAddJson = null;
                }
            };

            _webView.CoreWebView2.Navigate("https://app.ki-downloader.local/index.html");
        }

        private void OnClipboardChanged()
        {
            try
            {
                if (!_engine.Settings.ClipboardMonitor) return;
                if (!Clipboard.ContainsText()) return;
                string txt = (Clipboard.GetText() ?? "").Trim();
                if (txt.Length > 2048 || txt.Length < 10) return;
                Match m = UrlExtractRegex.Match(txt);
                if (!m.Success) return;

                string url = m.Value;
                if (url == _lastClipboardUrl) return;

                Uri uri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) return;
                string ext = Path.GetExtension(uri.AbsolutePath ?? "");
                if (!DownloadableExts.Contains(ext)) return;

                _lastClipboardUrl = url;
                string payload = _json.Serialize(new { url = url, source = "clipboard" });
                SendMessageToUi("clipboard_url", payload);
            }
            catch { }
        }

        private void HandleExternalAdd(string url, string fileName, string referer, string cookies)
        {
            string payload = _json.Serialize(new
            {
                url = url,
                fileName = fileName ?? "",
                referer = referer ?? "",
                cookies = cookies ?? "",
                source = "browser"
            });
            RestoreWindowFromTray();
            if (_uiReady) SendMessageToUi("quick_add", payload);
            else _pendingQuickAddJson = payload;
        }

        private void HandleDownloadCompleted(DownloadItem item)
        {
            if (_engine.Settings.ShowNotifications && _trayIcon != null)
            {
                BeginInvoke(new Action(delegate
                {
                    _trayIcon.ShowBalloonTip(3500, "Ki-Downloader", "Completed: " + item.FileName, ToolTipIcon.Info);
                }));
            }
            SendMessageToUi("toast", _json.Serialize(new
            {
                kind = "success",
                title = "Download Complete",
                titleFa = "دانلود با موفقیت کامل شد",
                message = item.FileName
            }));
        }

        private void HandlePostQueueAction(PostQueueAction action)
        {
            BeginInvoke(new Action(delegate
            {
                if (action == PostQueueAction.Sleep)
                    Application.SetSuspendState(PowerState.Suspend, true, false);
                else if (action == PostQueueAction.Shutdown)
                    Process.Start("shutdown", "/s /t 30");
            }));
        }

        private async void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string raw = e.WebMessageAsJson;
                var msg = _json.Deserialize<Dictionary<string, object>>(raw);
                if (msg == null || !msg.ContainsKey("action")) return;

                string action = Convert.ToString(msg["action"]);
                string reqId = msg.ContainsKey("reqId") ? Convert.ToString(msg["reqId"]) : "";
                Dictionary<string, object> data = msg.ContainsKey("data")
                    ? msg["data"] as Dictionary<string, object>
                    : new Dictionary<string, object>();

                switch (action)
                {
                    case "window_drag":
                        ReleaseCapture();
                        SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                        break;

                    case "window_minimize":
                        WindowState = FormWindowState.Minimized;
                        break;

                    case "window_maximize":
                        WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                        break;

                    case "window_close":
                        Close();
                        break;

                    case "get_clipboard_url":
                        {
                            string clipUrl = "";
                            string rawClip = "";
                            if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
                                rawClip = Clipboard.GetText(TextDataFormat.UnicodeText) ?? "";
                            else if (Clipboard.ContainsText())
                                rawClip = Clipboard.GetText() ?? "";
                            rawClip = rawClip.Trim();
                            Match m = UrlExtractRegex.Match(rawClip);
                            if (m.Success) clipUrl = m.Value;
                            SendResponseToUi(reqId, new { url = clipUrl, raw = rawClip });
                        }
                        break;

                    case "get_state":
                        PushStateToUi();
                        break;

                    case "probe_url":
                        {
                            string url = GetStr(data, "url");
                            string referer = GetStr(data, "referer");
                            string cookies = GetStr(data, "cookies");
                            ProbeResult res = await _engine.ProbeUrlAsync(url, referer, cookies, null);
                            SendResponseToUi(reqId, res);
                        }
                        break;

                    case "add_download":
                        {
                            string url = GetStr(data, "url");
                            string fileName = GetStr(data, "fileName");
                            string folder = GetStr(data, "folder");
                            int conns = GetInt(data, "connections", _engine.Settings.DefaultConnections);
                            bool startNow = GetBool(data, "startNow", true);
                            bool iranMode = GetBool(data, "iranResilient", _engine.Settings.IranResilientModeDefault);
                            string referer = GetStr(data, "referer");
                            string cookies = GetStr(data, "cookies");
                            string scheduleIso = GetStr(data, "scheduledFor");
                            DateTime? sched = null;
                            DateTime parsedDt;
                            if (!string.IsNullOrEmpty(scheduleIso) && DateTime.TryParse(scheduleIso, out parsedDt)) sched = parsedDt;

                            List<string> mirrors = new List<string>();
                            System.Collections.ArrayList list = (data != null && data.ContainsKey("mirrors"))
                                ? data["mirrors"] as System.Collections.ArrayList
                                : null;
                            if (list != null)
                            {
                                foreach (object m in list) mirrors.Add(Convert.ToString(m));
                            }

                            var item = _engine.AddDownload(url, fileName, folder, conns, startNow, iranMode, mirrors, referer, cookies, sched);
                            SendResponseToUi(reqId, new { ok = true, item = item });
                            PushStateToUi();
                        }
                        break;

                    case "batch_add":
                        {
                            var urlsObj = data != null && data.ContainsKey("urls") ? data["urls"] as System.Collections.ArrayList : null;
                            string folder = GetStr(data, "folder");
                            int conns = GetInt(data, "connections", _engine.Settings.DefaultConnections);
                            bool startNow = GetBool(data, "startNow", true);
                            bool iranMode = GetBool(data, "iranResilient", _engine.Settings.IranResilientModeDefault);
                            int count = 0;
                            if (urlsObj != null)
                            {
                                foreach (object uObj in urlsObj)
                                {
                                    string u = Convert.ToString(uObj);
                                    if (!string.IsNullOrWhiteSpace(u))
                                    {
                                        _engine.AddDownload(u.Trim(), null, folder, conns, startNow, iranMode, null, null, null, null);
                                        count++;
                                    }
                                }
                            }
                            SendResponseToUi(reqId, new { ok = true, added = count });
                            PushStateToUi();
                        }
                        break;

                    case "pause":
                        _engine.Pause(GetStr(data, "id"));
                        PushStateToUi();
                        break;

                    case "resume":
                        _engine.StartOrResume(GetStr(data, "id"));
                        PushStateToUi();
                        break;

                    case "pause_all":
                        _engine.PauseAll();
                        PushStateToUi();
                        break;

                    case "resume_all":
                        _engine.ResumeAll();
                        PushStateToUi();
                        break;

                    case "remove":
                        _engine.Remove(GetStr(data, "id"), GetBool(data, "deleteFiles", false));
                        PushStateToUi();
                        break;

                    case "refresh_address":
                        _engine.RefreshAddress(GetStr(data, "id"), GetStr(data, "url"));
                        PushStateToUi();
                        break;

                    case "set_item_speed_limit":
                        _engine.SetItemSpeedLimit(GetStr(data, "id"), GetInt(data, "kbps", 0));
                        PushStateToUi();
                        break;

                    case "compute_checksum":
                        {
                            var hashes = await _engine.ComputeChecksumsAsync(GetStr(data, "id"));
                            SendResponseToUi(reqId, new { sha256 = hashes.Item1, md5 = hashes.Item2 });
                            PushStateToUi();
                        }
                        break;

                    case "open_file":
                        {
                            string p = GetStr(data, "path");
                            if (!string.IsNullOrEmpty(p) && File.Exists(p))
                                Process.Start(new ProcessStartInfo(p) { UseShellExecute = true });
                        }
                        break;

                    case "open_folder":
                        {
                            string p = GetStr(data, "path");
                            if (!string.IsNullOrEmpty(p) && File.Exists(p))
                                Process.Start("explorer.exe", "/select,\"" + p + "\"");
                            else if (!string.IsNullOrEmpty(p) && Directory.Exists(p))
                                Process.Start("explorer.exe", "\"" + p + "\"");
                        }
                        break;

                    case "open_ext_folder":
                        {
                            string extDir = Path.Combine(_baseDir, "browser-extension");
                            if (Directory.Exists(extDir)) Process.Start("explorer.exe", "\"" + extDir + "\"");
                        }
                        break;

                    case "pick_folder":
                        {
                            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                            {
                                fbd.SelectedPath = _engine.Settings.DefaultSaveFolder;
                                SendResponseToUi(reqId, new { folder = fbd.ShowDialog(this) == DialogResult.OK ? fbd.SelectedPath : "" });
                            }
                        }
                        break;

                    case "save_settings":
                        {
                            string settingsJson = _json.Serialize(data["settings"]);
                            AppSettings updated = _json.Deserialize<AppSettings>(settingsJson);
                            _engine.UpdateSettings(updated);
                            ApplyDwmThemeAttributes();
                            SendResponseToUi(reqId, new { ok = true });
                            PushStateToUi();
                        }
                        break;

                    case "test_proxy":
                        {
                            string proxyJson = _json.Serialize(data["proxy"]);
                            ProxySettings ps = _json.Deserialize<ProxySettings>(proxyJson);
                            var result = await TestProxyAsync(ps);
                            SendResponseToUi(reqId, result);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Bridge error: " + ex.Message);
            }
        }

        private async Task<object> TestProxyAsync(ProxySettings ps)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (Socks5HttpRelay tempRelay = new Socks5HttpRelay())
                {
                    if (ps.Mode == ProxyMode.Socks5) tempRelay.Start();
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.cloudflare.com/cdn-cgi/trace");
                    req.Timeout = 7000;
                    req.Proxy = NetworkHelper.BuildWebProxy(ps, tempRelay);
                    using (HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync().ConfigureAwait(false))
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        string text = await sr.ReadToEndAsync().ConfigureAwait(false);
                        sw.Stop();
                        string ip = "", loc = "";
                        foreach (string line in text.Split('\n'))
                        {
                            if (line.StartsWith("ip=")) ip = line.Substring(3).Trim();
                            if (line.StartsWith("loc=")) loc = line.Substring(4).Trim();
                        }
                        return new { ok = true, latencyMs = sw.ElapsedMilliseconds, ip = ip, loc = loc };
                    }
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new { ok = false, latencyMs = sw.ElapsedMilliseconds, error = ex.Message };
            }
        }

        private void PushStateToUi()
        {
            if (!_uiReady || _webView == null || _webView.CoreWebView2 == null) return;
            try { SendMessageToUi("state", _engine.GetSnapshotJson()); } catch { }
        }

        private void SendResponseToUi(string reqId, object obj)
        {
            if (string.IsNullOrEmpty(reqId)) return;
            SendMessageToUi("rpc_response", _json.Serialize(new { reqId = reqId, result = obj }));
        }

        private void SendMessageToUi(string type, string payloadJson)
        {
            if (InvokeRequired) { BeginInvoke(new Action(delegate { SendMessageToUi(type, payloadJson); })); return; }
            if (!_uiReady || _webView == null || _webView.CoreWebView2 == null) return;
            try
            {
                string envelope = string.Format("{{\"type\":{0},\"payload\":{1}}}",
                    _json.Serialize(type),
                    string.IsNullOrEmpty(payloadJson) ? "null" : payloadJson);
                _webView.CoreWebView2.PostWebMessageAsJson(envelope);
            }
            catch { }
        }

        private static string GetStr(Dictionary<string, object> d, string key)
        {
            return (d != null && d.ContainsKey(key) && d[key] != null) ? Convert.ToString(d[key]) : "";
        }

        private static int GetInt(Dictionary<string, object> d, string key, int fallback)
        {
            if (d == null || !d.ContainsKey(key) || d[key] == null) return fallback;
            int v;
            return int.TryParse(Convert.ToString(d[key]), out v) ? v : fallback;
        }

        private static bool GetBool(Dictionary<string, object> d, string key, bool fallback)
        {
            if (d == null || !d.ContainsKey(key) || d[key] == null) return fallback;
            bool v;
            return bool.TryParse(Convert.ToString(d[key]), out v) ? v : fallback;
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "KiDownloader_SingleInstance_Mutex", out createdNew))
            {
                if (!createdNew)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            if (args != null && args.Length > 0 && args[0].StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            {
                                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                                wc.UploadString(
                                    string.Format("http://127.0.0.1:{0}/api/add", IpcServer.Port),
                                    "{\"url\":\"" + args[0].Replace("\"", "\\\"") + "\"}");
                            }
                            else
                            {
                                wc.DownloadString(string.Format("http://127.0.0.1:{0}/api/focus", IpcServer.Port));
                            }
                        }
                    }
                    catch { }
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(AppDomain.CurrentDomain.BaseDirectory));
            }
        }
    }
}
