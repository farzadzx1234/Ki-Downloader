@echo off
powershell -STA -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File "%~dp0Ki-Downloader.ps1" %*
