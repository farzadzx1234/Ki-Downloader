@echo off
title Ki-Downloader Setup
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$src = '%~dp0'; $dst = Join-Path $env:LOCALAPPDATA 'Programs\Ki-Downloader'; " ^
  "Get-Process -Name 'Ki-Downloader' -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep -Milliseconds 300; " ^
  "Get-ChildItem -Path $src -Recurse -File | Unblock-File -ErrorAction SilentlyContinue; " ^
  "New-Item -ItemType Directory -Force -Path $dst | Out-Null; " ^
  "Copy-Item -Path (Join-Path $src '*') -Destination $dst -Recurse -Force; " ^
  "$exe = Join-Path $dst 'Ki-Downloader.exe'; $ico = Join-Path $dst 'assets\ki-downloader.ico'; " ^
  "$wsh = New-Object -ComObject WScript.Shell; " ^
  "foreach ($p in @((Join-Path ([Environment]::GetFolderPath('Programs')) 'Ki-Downloader.lnk'), (Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'Ki-Downloader.lnk'))) { " ^
  "  $s = $wsh.CreateShortcut($p); $s.TargetPath = $exe; $s.WorkingDirectory = $dst; if (Test-Path $ico) { $s.IconLocation = $ico }; $s.Save() " ^
  "}; " ^
  "Write-Host 'Installed Ki-Downloader!' -ForegroundColor Green; Start-Process $exe"
