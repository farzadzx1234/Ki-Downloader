param(
    [string]$Url = ""
)

$ErrorActionPreference = "Stop"
if ([System.Threading.Thread]::CurrentThread.ApartmentState -ne [System.Threading.ApartmentState]::STA) {
    $argList = @("-STA", "-NoProfile", "-ExecutionPolicy", "Bypass", "-WindowStyle", "Hidden", "-File", $MyInvocation.MyCommand.Path)
    if ($Url) { $argList += @("-Url", $Url) }
    Start-Process -FilePath "powershell.exe" -ArgumentList $argList
    exit 0
}

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrEmpty($root)) { $root = $PWD.Path }
if ($env:PATH -notlike "*$root*") { $env:PATH = "$root;$env:PATH" }

$createdNew = $false
$mutex = New-Object System.Threading.Mutex($true, "KiDownloader_SingleInstance_Mutex", [ref]$createdNew)
try {
    if (-not $createdNew) {
        try {
            $wc = New-Object System.Net.WebClient
            if ($Url -and $Url.StartsWith("http", [System.StringComparison]::OrdinalIgnoreCase)) {
                $wc.Headers.Add("Content-Type", "application/json")
                $escaped = $Url.Replace('"', '\"')
                $wc.UploadString("http://127.0.0.1:45780/api/add", "{`"url`":`"$escaped`"}") | Out-Null
            } else {
                $wc.DownloadString("http://127.0.0.1:45780/api/focus") | Out-Null
            }
            $wc.Dispose()
        } catch {}
        exit 0
    }

    Add-Type -Path (Join-Path $root "Microsoft.Web.WebView2.Core.dll")
    Add-Type -Path (Join-Path $root "Microsoft.Web.WebView2.WinForms.dll")

    $srcFiles = @(
        (Join-Path $root "src\Models.cs"),
        (Join-Path $root "src\AppIcons.cs"),
        (Join-Path $root "src\NetworkHelper.cs"),
        (Join-Path $root "src\DownloadEngine.cs"),
        (Join-Path $root "src\IpcServer.cs"),
        (Join-Path $root "src\TrayPopupForm.cs"),
        (Join-Path $root "src\MainWindow.cs")
    )
    $refs = @(
        "System.dll",
        "System.Core.dll",
        "System.Drawing.dll",
        "System.Windows.Forms.dll",
        "System.Net.Http.dll",
        "System.Web.Extensions.dll",
        "System.IO.Compression.dll",
        "System.IO.Compression.FileSystem.dll",
        "Microsoft.CSharp.dll",
        (Join-Path $root "Microsoft.Web.WebView2.Core.dll"),
        (Join-Path $root "Microsoft.Web.WebView2.WinForms.dll")
    )

    Add-Type -Path $srcFiles -ReferencedAssemblies $refs -IgnoreWarnings

    [System.Windows.Forms.Application]::EnableVisualStyles()
    try { [System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false) } catch {}

    $pidDir = Join-Path $env:APPDATA "Ki-Downloader"
    if (!(Test-Path $pidDir)) { New-Item -ItemType Directory -Force -Path $pidDir | Out-Null }
    $pidFile = Join-Path $pidDir "host.pid"
    Set-Content -Path $pidFile -Value $PID -Force

    $form = New-Object KiDownloader.MainForm($root)
    [System.Windows.Forms.Application]::Run($form)
} finally {
    $pidFile = Join-Path $env:APPDATA "Ki-Downloader\host.pid"
    if (Test-Path $pidFile) { Remove-Item $pidFile -Force -ErrorAction SilentlyContinue }
    if ($mutex -and $createdNew) {
        try { $mutex.ReleaseMutex() } catch {}
    }
    if ($mutex) {
        try { $mutex.Dispose() } catch {}
    }
}
