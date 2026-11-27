@echo off
title Uninstall Ki-Downloader
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$dst = Join-Path $env:LOCALAPPDATA 'Programs\Ki-Downloader'; " ^
  "Get-Process -Name 'Ki-Downloader' -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep -Milliseconds 300; " ^
  "Remove-Item (Join-Path ([Environment]::GetFolderPath('Programs')) 'Ki-Downloader.lnk') -Force -ErrorAction SilentlyContinue; " ^
  "Remove-Item (Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'Ki-Downloader.lnk') -Force -ErrorAction SilentlyContinue; " ^
  "Remove-Item $dst -Recurse -Force -ErrorAction SilentlyContinue; " ^
  "Write-Host 'Ki-Downloader uninstalled.' -ForegroundColor Yellow"
pause
