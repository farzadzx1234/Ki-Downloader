# Ki-Downloader

<p align="center">
  <img src="assets/ki-logo.svg" width="110" alt="Ki-Downloader Logo" />
</p>

<p align="center">
  <strong>A modern, fast, multi-stream Windows download manager inspired by Apple Liquid Glass &amp; IDM.</strong><br/>
  مدیریت دانلود سریع و چنداستریمی ویندوز با طراحی Liquid Glass، فونت یکان‌بخ و گوگل‌سنس
</p>

---

## Features

- **Parallel Multi-Stream Engine (Up to 32 Sections):** Splits files into concurrent byte-range segments with dynamic work-stealing and automatic stall watchdog so slow connections never stall at 99%.
- **Resilient Disconnect & Pause/Resume:** Preserves exact byte offsets across every section (`.kidown`) when your connection drops, VPN reconnects, or the app closes — resume right where you left off.
- **Liquid Glass UI & Spring Motion:** Custom frameless Apple-style window controls, Light & Dark modes (plus Auto system sync), smooth spring-animated floating pill dock, and full **Persian (RTL) / English (LTR)** support (`YekanBakh` + `Google Sans`).
- **IDM-Style `Ctrl+V` Quick Capture:** Press `Ctrl+V` anywhere in the window (or copy a downloadable link) to probe file name, MIME/category, exact size, and resume support before starting.
- **Proxy Profile Manager & DoH:** Save and switch between **SOCKS5** (e.g. v2rayN/Nekoray), **HTTP/HTTPS** (Clash), System Proxy, or Direct, with built-in latency/exit-IP testing and optional **DNS-over-HTTPS (DoH)**.
- **Speed Limiter & Night Scheduler:** Cap global or per-download bandwidth (`KB/s`) and schedule automatic night downloads with Sleep/Shutdown post-actions.
- **Batch & Pattern Grabber:** Expand numbered URL ranges (`https://example.com/part-[01-12].rar`) or extract links from pasted text.
- **Standalone Browser Extension:** Chrome/Edge Manifest V3 extension (`browser-extension/`) with automatic download interception, right-click menu, and one-click page link grabbing.

## Installation

1. Download **`Ki-Downloader-Setup.exe`** from the [Releases](../../releases) page and run it.
2. *(Optional)* For browser integration, download **`Ki-Downloader-Extension.zip`** (or open the `browser-extension` folder from Settings), extract it, enable **Developer mode** in `chrome://extensions` or `edge://extensions`, and click **Load unpacked**.

## Building from Source (Windows)

Requires Windows 10/11 (Edge WebView2 Runtime + built-in .NET Framework 4.8 `csc.exe` — no heavy IDE needed):

```powershell
powershell -ExecutionPolicy Bypass -File build-installer.ps1
```

This compiles `Ki-Downloader.exe` and packages `dist\Ki-Downloader-Setup.exe` + `dist\Ki-Downloader-Extension.zip`.

## License

MIT License — see [LICENSE](LICENSE).
