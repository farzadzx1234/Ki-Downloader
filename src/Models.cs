using System;
using System.Collections.Generic;

namespace KiDownloader
{
    public enum DownloadStatus
    {
        Queued = 0,
        Probing = 1,
        Downloading = 2,
        Paused = 3,
        Completed = 4,
        Error = 5,
        Scheduled = 6,
        Merging = 7
    }

    public enum SegmentStatus
    {
        Pending = 0,
        Connecting = 1,
        Streaming = 2,
        Stalled = 3,
        Retrying = 4,
        Stealing = 5,
        Done = 6,
        Error = 7
    }

    public enum ProxyMode
    {
        Direct = 0,
        System = 1,
        Http = 2,
        Socks5 = 3
    }

    public enum PostQueueAction
    {
        None = 0,
        Notify = 1,
        Sleep = 2,
        Shutdown = 3
    }

    public class ProxyProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ProxyMode Mode { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ProxyProfile()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            Mode = ProxyMode.Socks5;
            Host = "127.0.0.1";
            Port = 10808;
        }
    }

    public class SegmentState
    {
        public int Index { get; set; }
        public long StartByte { get; set; }
        public long EndByte { get; set; }
        public long CurrentByte { get; set; }
        public double SpeedBps { get; set; }
        public int Retries { get; set; }
        public int StallsRecovered { get; set; }
        public SegmentStatus Status { get; set; }
        public string LastError { get; set; }

        public long TotalBytes
        {
            get { return EndByte >= StartByte ? (EndByte - StartByte + 1) : 0; }
        }

        public long DownloadedBytes
        {
            get { return Math.Max(0, CurrentByte - StartByte); }
        }

        public long RemainingBytes
        {
            get { return Math.Max(0, EndByte - CurrentByte + 1); }
        }
    }

    public class DownloadItem
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public List<string> MirrorUrls { get; set; }
        public int ActiveMirrorIndex { get; set; }
        public string FileName { get; set; }
        public string SaveFolder { get; set; }
        public string TempFilePath { get; set; }
        public string FinalFilePath { get; set; }
        public string Category { get; set; }
        public string MimeType { get; set; }
        public string ETag { get; set; }
        public string Referer { get; set; }
        public string UserAgent { get; set; }
        public string Cookies { get; set; }
        public string CustomHeaders { get; set; }
        public long TotalBytes { get; set; }
        public long DownloadedBytes { get; set; }
        public bool SupportsRange { get; set; }
        public int MaxConnections { get; set; }
        public int SpeedLimitKBps { get; set; }
        public bool IranResilientMode { get; set; }
        public DownloadStatus Status { get; set; }
        public string StatusDetail { get; set; }
        public string ErrorMessage { get; set; }
        public double SpeedBps { get; set; }
        public double PeakSpeedBps { get; set; }
        public double EtaSeconds { get; set; }
        public int TotalRetries { get; set; }
        public int StallsRecovered { get; set; }
        public int SegmentSteals { get; set; }
        public string Sha256 { get; set; }
        public string Md5 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public List<SegmentState> Segments { get; set; }
        public List<double> SpeedHistory { get; set; }

        public DownloadItem()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 12);
            MirrorUrls = new List<string>();
            Segments = new List<SegmentState>();
            SpeedHistory = new List<double>();
            MaxConnections = 16;
            IranResilientMode = true;
            CreatedAt = DateTime.UtcNow;
            Status = DownloadStatus.Queued;
            StatusDetail = "Ready";
            Category = "Other";
        }
    }

    public class ProxySettings
    {
        public ProxyMode Mode { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PresetName { get; set; }
        public string ActiveProfileId { get; set; }
        public List<ProxyProfile> Profiles { get; set; }

        public ProxySettings()
        {
            Mode = ProxyMode.System;
            Host = "127.0.0.1";
            Port = 10808;
            PresetName = "System";
            ActiveProfileId = "system";
            Profiles = new List<ProxyProfile>
            {
                new ProxyProfile { Id = "v2ray", Name = "v2rayN / Nekoray", Mode = ProxyMode.Socks5, Host = "127.0.0.1", Port = 10808 },
                new ProxyProfile { Id = "clash", Name = "Clash / Mihomo", Mode = ProxyMode.Http, Host = "127.0.0.1", Port = 7890 }
            };
        }
    }

    public class DohSettings
    {
        public bool Enabled { get; set; }
        public string Provider { get; set; }
        public string Endpoint { get; set; }

        public DohSettings()
        {
            Enabled = false;
            Provider = "cloudflare";
            Endpoint = "https://1.1.1.1/dns-query";
        }
    }

    public class SchedulerSettings
    {
        public bool NightModeEnabled { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public PostQueueAction PostAction { get; set; }
        public bool AutoStartQueue { get; set; }

        public SchedulerSettings()
        {
            NightModeEnabled = false;
            StartTime = "02:00";
            EndTime = "07:00";
            PostAction = PostQueueAction.Notify;
            AutoStartQueue = true;
        }
    }

    public class AppSettings
    {
        public string DefaultSaveFolder { get; set; }
        public bool AutoCategorizeFolders { get; set; }
        public int DefaultConnections { get; set; }
        public int MaxConcurrentDownloads { get; set; }
        public int GlobalSpeedLimitKBps { get; set; }
        public bool ClipboardMonitor { get; set; }
        public bool IranResilientModeDefault { get; set; }
        public bool AutoResumeOnReconnect { get; set; }
        public int StallTimeoutSeconds { get; set; }
        public int MinStallSpeedKBps { get; set; }
        public int MaxRetriesPerSegment { get; set; }
        public bool DynamicSegmentStealing { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool ShowNotifications { get; set; }
        public string Language { get; set; } // "fa" or "en"
        public string Theme { get; set; }    // "auto", "dark", "light"
        public bool ReduceMotion { get; set; }
        public bool ReduceTransparency { get; set; }
        public ProxySettings Proxy { get; set; }
        public DohSettings Doh { get; set; }
        public SchedulerSettings Scheduler { get; set; }
        public string UserAgent { get; set; }

        public AppSettings()
        {
            DefaultSaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\Ki-Downloader";
            AutoCategorizeFolders = true;
            DefaultConnections = 16;
            MaxConcurrentDownloads = 3;
            GlobalSpeedLimitKBps = 0;
            ClipboardMonitor = true;
            IranResilientModeDefault = true;
            AutoResumeOnReconnect = false;
            StallTimeoutSeconds = 8;
            MinStallSpeedKBps = 4;
            MaxRetriesPerSegment = 24;
            DynamicSegmentStealing = true;
            MinimizeToTray = true;
            ShowNotifications = true;
            Language = "fa";
            Theme = "auto";
            ReduceMotion = false;
            ReduceTransparency = false;
            Proxy = new ProxySettings();
            Doh = new DohSettings();
            Scheduler = new SchedulerSettings();
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
        }
    }

    public class ProbeResult
    {
        public bool Success { get; set; }
        public string FinalUrl { get; set; }
        public string FileName { get; set; }
        public string Category { get; set; }
        public long TotalBytes { get; set; }
        public bool SupportsRange { get; set; }
        public string MimeType { get; set; }
        public string ETag { get; set; }
        public int StatusCode { get; set; }
        public string Error { get; set; }
        public string EffectiveReferer { get; set; }
    }
}
