$ErrorActionPreference = "Stop"
Write-Host "Installing Ki-Downloader..." -ForegroundColor Cyan

$installDir = Join-Path $env:LOCALAPPDATA "Programs\Ki-Downloader"
$zipPath = Join-Path $env:TEMP "Ki-Downloader-Portable.zip"
$url = "https://github.com/farzadzx1234/Ki-Downloader/releases/latest/download/Ki-Downloader-Portable.zip"

Get-Process -Name "Ki-Downloader" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 300

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
$icoPath = Join-Path $installDir "assets\ki-downloader.ico"

$wsh = New-Object -ComObject WScript.Shell
$startMenu = Join-Path ([Environment]::GetFolderPath("Programs")) "Ki-Downloader.lnk"
$s1 = $wsh.CreateShortcut($startMenu)
$s1.TargetPath = $exePath
$s1.WorkingDirectory = $installDir
if (Test-Path $icoPath) { $s1.IconLocation = $icoPath }
$s1.Save()

$desktop = Join-Path ([Environment]::GetFolderPath("DesktopDirectory")) "Ki-Downloader.lnk"
$s2 = $wsh.CreateShortcut($desktop)
$s2.TargetPath = $exePath
$s2.WorkingDirectory = $installDir
if (Test-Path $icoPath) { $s2.IconLocation = $icoPath }
$s2.Save()

Write-Host "Ki-Downloader installed to $installDir" -ForegroundColor Green
Start-Process -FilePath $exePath
