using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace KiDownloader
{
    public class TokenBucketLimiter
    {
        private readonly object _lock = new object();
        private double _tokens;
        private long _lastTicks;
        private int _limitKBps;

        public TokenBucketLimiter(int limitKBps)
        {
            _limitKBps = limitKBps;
            _lastTicks = DateTime.UtcNow.Ticks;
            _tokens = limitKBps > 0 ? limitKBps * 1024.0 : 0;
        }

        public void SetLimitKBps(int limitKBps)
        {
            lock (_lock)
            {
                _limitKBps = limitKBps;
                if (limitKBps > 0)
                {
                    _tokens = Math.Min(_tokens, limitKBps * 1024.0);
                }
            }
        }

        public async Task ConsumeAsync(int bytes, CancellationToken token)
        {
            if (bytes <= 0) return;

            while (true)
            {
                int waitMs = 0;
                lock (_lock)
                {
                    if (_limitKBps <= 0) return;

                    long now = DateTime.UtcNow.Ticks;
                    double elapsedSec = (now - _lastTicks) / 10000000.0;
                    _lastTicks = now;

                    double rateBps = _limitKBps * 1024.0;
                    _tokens = Math.Min(rateBps, _tokens + elapsedSec * rateBps);

                    if (_tokens >= bytes)
                    {
                        _tokens -= bytes;
                        return;
                    }

                    double needed = bytes - _tokens;
                    _tokens = 0;
                    waitMs = Math.Max(10, Math.Min(500, (int)(needed / rateBps * 1000.0)));
                }

                await Task.Delay(waitMs, token).ConfigureAwait(false);
            }
        }
    }

    public sealed class Socks5HttpRelay : IDisposable
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private readonly object _cfgLock = new object();
        private string _socksHost = "127.0.0.1";
        private int _socksPort = 10808;
        private string _username;
        private string _password;

        public int LocalPort { get; private set; }

        public void Start()
        {
            if (_listener != null) return;
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start(128);
            LocalPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
            Task.Run(delegate { AcceptLoop(_cts.Token); });
        }

        public void Configure(string host, int port, string username, string password)
        {
            lock (_cfgLock)
            {
                _socksHost = string.IsNullOrEmpty(host) ? "127.0.0.1" : host.Trim();
                _socksPort = port > 0 ? port : 10808;
                _username = username;
                _password = password;
            }
        }

        private async Task AcceptLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                bool acceptError = false;
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    client.NoDelay = true;
                    Task ignored = Task.Run(delegate { HandleClientAsync(client, ct); });
                }
                catch
                {
                    if (ct.IsCancellationRequested) break;
                    acceptError = true;
                }
                if (acceptError)
                {
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            TcpClient upstream = null;
            try
            {
                using (client)
                {
                    NetworkStream clientStream = client.GetStream();
                    byte[] headerBuf = new byte[8192];
                    int totalRead = 0;
                    int headerEnd = -1;

                    while (totalRead < headerBuf.Length)
                    {
                        int n = await clientStream.ReadAsync(headerBuf, totalRead, headerBuf.Length - totalRead, ct).ConfigureAwait(false);
                        if (n <= 0) return;
                        totalRead += n;
                        string s = Encoding.ASCII.GetString(headerBuf, 0, totalRead);
                        headerEnd = s.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                        if (headerEnd >= 0) break;
                    }

                    if (headerEnd < 0) return;

                    string headerText = Encoding.ASCII.GetString(headerBuf, 0, headerEnd);
                    string[] lines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);
                    if (lines.Length == 0) return;

                    string[] parts = lines[0].Split(' ');
                    if (parts.Length < 2) return;

                    string method = parts[0].ToUpperInvariant();
                    string target = parts[1];

                    string targetHost;
                    int targetPort = 80;
                    bool isConnect = method == "CONNECT";

                    if (isConnect)
                    {
                        int colon = target.LastIndexOf(':');
                        if (colon > 0)
                        {
                            targetHost = target.Substring(0, colon);
                            int.TryParse(target.Substring(colon + 1), out targetPort);
                        }
                        else
                        {
                            targetHost = target;
                            targetPort = 443;
                        }
                    }
                    else
                    {
                        Uri uri;
                        if (!Uri.TryCreate(target, UriKind.Absolute, out uri)) return;
                        targetHost = uri.Host;
                        targetPort = uri.Port;
                    }

                    string sHost;
                    int sPort;
                    string sUser;
                    string sPass;
                    lock (_cfgLock)
                    {
                        sHost = _socksHost;
                        sPort = _socksPort;
                        sUser = _username;
                        sPass = _password;
                    }

                    upstream = new TcpClient();
                    upstream.NoDelay = true;
                    await upstream.ConnectAsync(sHost, sPort).ConfigureAwait(false);
                    NetworkStream upStream = upstream.GetStream();

                    bool hasAuth = !string.IsNullOrEmpty(sUser);
                    byte[] greeting = hasAuth ? new byte[] { 0x05, 0x02, 0x00, 0x02 } : new byte[] { 0x05, 0x01, 0x00 };
                    await upStream.WriteAsync(greeting, 0, greeting.Length, ct).ConfigureAwait(false);

                    byte[] greetResp = new byte[2];
                    await ReadExactAsync(upStream, greetResp, 2, ct).ConfigureAwait(false);
                    if (greetResp[0] != 0x05 || greetResp[1] == 0xFF)
                        throw new IOException("SOCKS5 authentication negotiation failed.");

                    if (greetResp[1] == 0x02)
                    {
                        byte[] uBytes = Encoding.UTF8.GetBytes(sUser ?? "");
                        byte[] pBytes = Encoding.UTF8.GetBytes(sPass ?? "");
                        byte[] authReq = new byte[3 + uBytes.Length + pBytes.Length];
                        authReq[0] = 0x01;
                        authReq[1] = (byte)uBytes.Length;
                        Buffer.BlockCopy(uBytes, 0, authReq, 2, uBytes.Length);
                        authReq[2 + uBytes.Length] = (byte)pBytes.Length;
                        Buffer.BlockCopy(pBytes, 0, authReq, 3 + uBytes.Length, pBytes.Length);
                        await upStream.WriteAsync(authReq, 0, authReq.Length, ct).ConfigureAwait(false);
                        byte[] authResp = new byte[2];
                        await ReadExactAsync(upStream, authResp, 2, ct).ConfigureAwait(false);
                        if (authResp[1] != 0x00) throw new IOException("SOCKS5 username/password rejected.");
                    }

                    byte[] hostBytes = Encoding.ASCII.GetBytes(targetHost);
                    byte[] connReq = new byte[7 + hostBytes.Length];
                    connReq[0] = 0x05;
                    connReq[1] = 0x01;
                    connReq[2] = 0x00;
                    connReq[3] = 0x03;
                    connReq[4] = (byte)hostBytes.Length;
                    Buffer.BlockCopy(hostBytes, 0, connReq, 5, hostBytes.Length);
                    connReq[5 + hostBytes.Length] = (byte)((targetPort >> 8) & 0xFF);
                    connReq[6 + hostBytes.Length] = (byte)(targetPort & 0xFF);
                    await upStream.WriteAsync(connReq, 0, connReq.Length, ct).ConfigureAwait(false);

                    byte[] connRespHead = new byte[4];
                    await ReadExactAsync(upStream, connRespHead, 4, ct).ConfigureAwait(false);
                    if (connRespHead[1] != 0x00)
                        throw new IOException("SOCKS5 connect failed with code " + connRespHead[1]);

                    int addrLen = 0;
                    if (connRespHead[3] == 0x01) addrLen = 4;
                    else if (connRespHead[3] == 0x04) addrLen = 16;
                    else if (connRespHead[3] == 0x03)
                    {
                        byte[] lenB = new byte[1];
                        await ReadExactAsync(upStream, lenB, 1, ct).ConfigureAwait(false);
                        addrLen = lenB[0];
                    }
                    if (addrLen > 0)
                    {
                        byte[] skip = new byte[addrLen + 2];
                        await ReadExactAsync(upStream, skip, skip.Length, ct).ConfigureAwait(false);
                    }
                    else
                    {
                        byte[] skipPort = new byte[2];
                        await ReadExactAsync(upStream, skipPort, 2, ct).ConfigureAwait(false);
                    }

                    if (isConnect)
                    {
                        byte[] ok = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
                        await clientStream.WriteAsync(ok, 0, ok.Length, ct).ConfigureAwait(false);
                    }
                    else
                    {
                        Uri uri = new Uri(target);
                        StringBuilder sb = new StringBuilder();
                        sb.Append(method).Append(" ").Append(uri.PathAndQuery).Append(" HTTP/1.1\r\n");
                        for (int i = 1; i < lines.Length; i++)
                        {
                            if (lines[i].StartsWith("Proxy-Connection:", StringComparison.OrdinalIgnoreCase)) continue;
                            sb.Append(lines[i]).Append("\r\n");
                        }
                        sb.Append("\r\n");
                        byte[] rewritten = Encoding.ASCII.GetBytes(sb.ToString());
                        await upStream.WriteAsync(rewritten, 0, rewritten.Length, ct).ConfigureAwait(false);
                        int bodyOffset = headerEnd + 4;
                        if (totalRead > bodyOffset)
                        {
                            await upStream.WriteAsync(headerBuf, bodyOffset, totalRead - bodyOffset, ct).ConfigureAwait(false);
                        }
                    }

                    Task t1 = clientStream.CopyToAsync(upStream, 65536, ct);
                    Task t2 = upStream.CopyToAsync(clientStream, 65536, ct);
                    await Task.WhenAny(t1, t2).ConfigureAwait(false);
                }
            }
            catch { }
            finally
            {
                if (upstream != null)
                {
                    try { upstream.Close(); } catch { }
                }
            }
        }

        private static async Task ReadExactAsync(Stream s, byte[] buf, int count, CancellationToken ct)
        {
            int offset = 0;
            while (offset < count)
            {
                int n = await s.ReadAsync(buf, offset, count - offset, ct).ConfigureAwait(false);
                if (n <= 0) throw new EndOfStreamException();
                offset += n;
            }
        }

        public void Dispose()
        {
            try { if (_cts != null) _cts.Cancel(); } catch { }
            try { if (_listener != null) _listener.Stop(); } catch { }
        }
    }

    public static class NetworkHelper
    {
        private static readonly Dictionary<string, Tuple<string, DateTime>> _dohCache = new Dictionary<string, Tuple<string, DateTime>>();
        private static readonly object _dohLock = new object();
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        public static void ConfigureTlsAndServicePoint()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)12288;
            }
            catch
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            ServicePointManager.DefaultConnectionLimit = 512;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.MaxServicePointIdleTime = 30000;
            ServicePointManager.DnsRefreshTimeout = 60000;
        }

        public static IWebProxy BuildWebProxy(ProxySettings proxy, Socks5HttpRelay socksRelay)
        {
            if (proxy == null || proxy.Mode == ProxyMode.Direct) return null;
            if (proxy.Mode == ProxyMode.System) return WebRequest.GetSystemWebProxy();

            if (proxy.Mode == ProxyMode.Http)
            {
                if (string.IsNullOrEmpty(proxy.Host)) return null;
                WebProxy wp = new WebProxy(proxy.Host, proxy.Port > 0 ? proxy.Port : 8080);
                if (!string.IsNullOrEmpty(proxy.Username))
                {
                    wp.Credentials = new NetworkCredential(proxy.Username, proxy.Password ?? "");
                }
                return wp;
            }

            if (proxy.Mode == ProxyMode.Socks5 && socksRelay != null)
            {
                socksRelay.Configure(proxy.Host, proxy.Port, proxy.Username, proxy.Password);
                return new WebProxy("127.0.0.1", socksRelay.LocalPort);
            }

            return null;
        }

        public static async Task<string> ResolveDohAsync(string host, DohSettings doh)
        {
            if (doh == null || !doh.Enabled || string.IsNullOrEmpty(host)) return null;
            IPAddress parsed;
            if (IPAddress.TryParse(host, out parsed)) return host;

            lock (_dohLock)
            {
                Tuple<string, DateTime> cached;
                if (_dohCache.TryGetValue(host, out cached) && cached.Item2 > DateTime.UtcNow)
                {
                    return cached.Item1;
                }
            }

            try
            {
                string endpoint = string.IsNullOrEmpty(doh.Endpoint) ? "https://1.1.1.1/dns-query" : doh.Endpoint.Trim();
                string url = endpoint + (endpoint.Contains("?") ? "&" : "?") + "name=" + Uri.EscapeDataString(host) + "&type=A";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Accept = "application/dns-json";
                req.Timeout = 4000;
                req.Proxy = null;

                using (WebResponse resp = await req.GetResponseAsync().ConfigureAwait(false))
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    string body = await sr.ReadToEndAsync().ConfigureAwait(false);
                    var dict = _json.Deserialize<Dictionary<string, object>>(body);
                    if (dict != null && dict.ContainsKey("Answer"))
                    {
                        var answers = dict["Answer"] as System.Collections.ArrayList;
                        if (answers != null)
                        {
                            foreach (object obj in answers)
                            {
                                var ans = obj as Dictionary<string, object>;
                                if (ans != null && ans.ContainsKey("type") && Convert.ToInt32(ans["type"]) == 1 && ans.ContainsKey("data"))
                                {
                                    string ip = Convert.ToString(ans["data"]);
                                    if (IPAddress.TryParse(ip, out parsed))
                                    {
                                        lock (_dohLock)
                                        {
                                            _dohCache[host] = Tuple.Create(ip, DateTime.UtcNow.AddMinutes(5));
                                        }
                                        return ip;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
