$ErrorActionPreference = "Stop"
Write-Host "Installing Ki-Downloader..." -ForegroundColor Cyan

$installDir = Join-Path $env:LOCALAPPDATA "Programs\Ki-Downloader"
$zipPath = Join-Path $env:TEMP "Ki-Downloader-Portable.zip"
$url = "https://github.com/farzadzx1234/Ki-Downloader/releases/latest/download/Ki-Downloader-Portable.zip"

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

Write-Host "Downloading latest release package..."
Invoke-WebRequest -Uri $url -OutFile $zipPath -UseBasicParsing
Unblock-File -Path $zipPath -ErrorAction SilentlyContinue

if (!(Test-Path $installDir)) { New-Item -ItemType Directory -Force -Path $installDir | Out-Null }
Expand-Archive -Path $zipPath -DestinationPath $installDir -Force
Remove-Item $zipPath -Force -ErrorAction SilentlyContinue

Get-ChildItem -Path $installDir -Recurse -File | ForEach-Object {
    Unblock-File -Path $_.FullName -ErrorAction SilentlyContinue
}

$exePath = Join-Path $installDir "Ki-Downloader.exe"
$psLauncher = Join-Path $installDir "Ki-Downloader.ps1"
$icoPath = Join-Path $installDir "assets\ki-downloader.ico"
$psExe = Join-Path $env:WINDIR "System32\WindowsPowerShell\v1.0\powershell.exe"

$sacState = $null
try {
    $sacState = (Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy" -ErrorAction SilentlyContinue).VerifiedAndReputablePolicyState
} catch {}

$usePsHost = ($sacState -eq 1 -or $sacState -eq 2) -or (-not (Test-Path $exePath))
if (-not $usePsHost) {
    try {
        Start-Process -FilePath $exePath -WorkingDirectory $installDir -ErrorAction Stop | Out-Null
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
    $sc.WorkingDirectory = $installDir
    if (Test-Path $icoPath) { $sc.IconLocation = $icoPath }
    $sc.Save()
}

if ($usePsHost) {
    Start-Process -FilePath $psExe -ArgumentList @("-STA", "-NoProfile", "-ExecutionPolicy", "Bypass", "-WindowStyle", "Hidden", "-File", "`"$psLauncher`"") -WorkingDirectory $installDir
}

Write-Host "Ki-Downloader installed to $installDir" -ForegroundColor Green
