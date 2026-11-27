$ErrorActionPreference = "Stop"
$src = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrEmpty($src)) { $src = $PWD.Path }
$dst = Join-Path $env:LOCALAPPDATA "Programs\Ki-Downloader"

Write-Host "Installing Ki-Downloader..." -ForegroundColor Cyan

Get-Process -Name "Ki-Downloader" -ErrorAction SilentlyContinue | Stop-Process -Force
$pidFile = Join-Path $env:APPDATA "Ki-Downloader\host.pid"
if (Test-Path $pidFile) {
    $oldPid = (Get-Content $pidFile -ErrorAction SilentlyContinue | Select-Object -First 1) -as [int]
    if ($oldPid -and $oldPid -ne $PID) {
        Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
    }
    Remove-Item $pidFile -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Milliseconds 350

Get-ChildItem -Path $src -Recurse -File | Unblock-File -ErrorAction SilentlyContinue

if (!(Test-Path $dst)) { New-Item -ItemType Directory -Force -Path $dst | Out-Null }
Copy-Item -Path (Join-Path $src "*") -Destination $dst -Recurse -Force
Get-ChildItem -Path $dst -Recurse -File | Unblock-File -ErrorAction SilentlyContinue

$exePath = Join-Path $dst "Ki-Downloader.exe"
$psLauncher = Join-Path $dst "Ki-Downloader.ps1"
$icoPath = Join-Path $dst "assets\ki-downloader.ico"
$psExe = Join-Path $env:WINDIR "System32\WindowsPowerShell\v1.0\powershell.exe"

$sacState = $null
try {
    $sacState = (Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy" -ErrorAction SilentlyContinue).VerifiedAndReputablePolicyState
} catch {}

$usePsHost = ($sacState -eq 1 -or $sacState -eq 2) -or (-not (Test-Path $exePath))

if (-not $usePsHost) {
    try {
        $p = Start-Process -FilePath $exePath -WorkingDirectory $dst -PassThru -ErrorAction Stop
        Start-Sleep -Milliseconds 250
    } catch {
        $usePsHost = $true
    }
}

$wsh = New-Object -ComObject WScript.Shell
$shortcutPaths = @(
    (Join-Path ([Environment]::GetFolderPath("Programs")) "Ki-Downloader.lnk"),
    (Join-Path ([Environment]::GetFolderPath("DesktopDirectory")) "Ki-Downloader.lnk")
)

foreach ($lnkPath in $shortcutPaths) {
    $sc = $wsh.CreateShortcut($lnkPath)
    if ($usePsHost) {
        $sc.TargetPath = $psExe
        $sc.Arguments = "-STA -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File `"$psLauncher`""
    } else {
        $sc.TargetPath = $exePath
        $sc.Arguments = ""
    }
    $sc.WorkingDirectory = $dst
    if (Test-Path $icoPath) { $sc.IconLocation = $icoPath }
    $sc.Save()
}

if ($usePsHost) {
    Start-Process -FilePath $psExe -ArgumentList @("-STA", "-NoProfile", "-ExecutionPolicy", "Bypass", "-WindowStyle", "Hidden", "-File", "`"$psLauncher`"") -WorkingDirectory $dst
}

Write-Host "Ki-Downloader installed successfully to $dst" -ForegroundColor Green
