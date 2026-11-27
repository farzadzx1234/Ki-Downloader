using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace KiDownloader
{
    public sealed class IpcServer : IDisposable
    {
        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private readonly DownloadEngine _engine;
        private readonly Action<string, string, string, string> _onExternalAddRequest;
        private readonly Action _onBringToFront;
        private readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        public const int Port = 45780;

        public IpcServer(DownloadEngine engine, Action<string, string, string, string> onExternalAddRequest, Action onBringToFront)
        {
            _engine = engine;
            _onExternalAddRequest = onExternalAddRequest;
            _onBringToFront = onBringToFront;
        }

        public void Start()
        {
            try
            {
                _cts = new CancellationTokenSource();
                _listener = new HttpListener();
                _listener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", Port));
                _listener.Start();
                Task.Run(delegate { ListenLoop(_cts.Token); });
            }
            catch { }
        }

        private async Task ListenLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _listener != null && _listener.IsListening)
            {
                try
                {
                    HttpListenerContext ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                    Task ignored = Task.Run(delegate { HandleRequestAsync(ctx); });
                }
                catch
                {
                    if (ct.IsCancellationRequested) break;
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext ctx)
        {
            try
            {
                ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
                ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
                ctx.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";

                if (ctx.Request.HttpMethod == "OPTIONS")
                {
                    ctx.Response.StatusCode = 204;
                    ctx.Response.Close();
                    return;
                }

                string path = (ctx.Request.Url.AbsolutePath ?? "/").ToLowerInvariant();
                if (path == "/api/ping")
                {
                    WriteJson(ctx.Response, 200, new { ok = true, app = "Ki-Downloader", version = "1.0.0" });
                    return;
                }

                if (path == "/api/focus")
                {
                    if (_onBringToFront != null) _onBringToFront();
                    WriteJson(ctx.Response, 200, new { ok = true });
                    return;
                }

                if (path == "/api/add" && ctx.Request.HttpMethod == "POST")
                {
                    using (StreamReader sr = new StreamReader(ctx.Request.InputStream, Encoding.UTF8))
                    {
                        string body = await sr.ReadToEndAsync().ConfigureAwait(false);
                        var data = _json.Deserialize<Dictionary<string, object>>(body);
                        string url = data != null && data.ContainsKey("url") ? Convert.ToString(data["url"]) : "";
                        string fileName = data != null && data.ContainsKey("fileName") ? Convert.ToString(data["fileName"]) : "";
                        string referer = data != null && data.ContainsKey("referer") ? Convert.ToString(data["referer"]) : "";
                        string cookies = data != null && data.ContainsKey("cookies") ? Convert.ToString(data["cookies"]) : "";
                        bool silent = data != null && data.ContainsKey("silent") && Convert.ToBoolean(data["silent"]);

                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            if (silent)
                            {
                                _engine.AddDownload(url, fileName, null, _engine.Settings.DefaultConnections, true, _engine.Settings.IranResilientModeDefault, null, referer, cookies, null);
                            }
                            else if (_onExternalAddRequest != null)
                            {
                                _onExternalAddRequest(url, fileName, referer, cookies);
                            }
                            WriteJson(ctx.Response, 200, new { ok = true });
                            return;
                        }
                    }
                    WriteJson(ctx.Response, 400, new { ok = false, error = "Missing url" });
                    return;
                }

                WriteJson(ctx.Response, 404, new { ok = false, error = "Not found" });
            }
            catch
            {
                try { ctx.Response.StatusCode = 500; ctx.Response.Close(); } catch { }
            }
        }

        private void WriteJson(HttpListenerResponse resp, int status, object obj)
        {
            byte[] buf = Encoding.UTF8.GetBytes(_json.Serialize(obj));
            resp.StatusCode = status;
            resp.ContentType = "application/json; charset=utf-8";
            resp.ContentLength64 = buf.Length;
            resp.OutputStream.Write(buf, 0, buf.Length);
            resp.Close();
        }

        public void Dispose()
        {
            try { if (_cts != null) _cts.Cancel(); } catch { }
            try { if (_listener != null) _listener.Stop(); } catch { }
        }
    }
}
