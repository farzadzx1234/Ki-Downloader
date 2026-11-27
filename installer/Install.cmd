@echo off
title Ki-Downloader Installer
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-local.ps1"
if errorlevel 1 pause
