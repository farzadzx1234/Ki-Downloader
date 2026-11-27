# Ki-Downloader

<p align="center">
  <img src="assets/ki-logo.svg" width="110" alt="Ki-Downloader Logo" />
</p>

<p align="center">
  <strong>A modern, fast, multi-stream Windows download manager inspired by Apple Liquid Glass &amp; IDM.</strong><br/>
  مدیریت دانلود سریع و چنداستریمی ویندوز با طراحی Liquid Glass، فونت یکان‌بخ و گوگل‌سنس
</p>

---

## Installation

### Option 1: One-Line PowerShell Install (Recommended — No SmartScreen Prompt)
Open **PowerShell** on Windows 10/11 and run:

```powershell
irm https://raw.githubusercontent.com/farzadzx1234/Ki-Downloader/main/install.ps1 | iex
```

This downloads the latest package, unblocks files automatically, installs to `%LocalAppData%\Programs\Ki-Downloader`, creates Start Menu & Desktop shortcuts, and launches Ki-Downloader.

### Option 2: Portable ZIP (Recommended Manual Download)
1. Download **`Ki-Downloader-Portable.zip`** from the [Releases](../../releases) page.
2. Extract the ZIP folder and either run **`Install.cmd`** (creates shortcuts) or open **`Ki-Downloader.exe`** directly.

### Option 3: Standalone Browser Extension (Chrome / Edge)
1. Download **`Ki-Downloader-Extension.zip`** from [Releases](../../releases) (or open the `browser-extension` folder from Settings).
2. Extract it, open `chrome://extensions` or `edge://extensions`, turn on **Developer mode**, and click **Load unpacked**.

---

## Features

- **Parallel Multi-Stream Engine (Up to 32 Sections):** Splits files into concurrent byte-range segments with dynamic work-stealing and automatic stall watchdog so slow connections never stall at 99%.
- **Resilient Disconnect & Pause/Resume:** Preserves exact byte offsets across every section (`.kidown`) when your connection drops, VPN reconnects, or the app closes — resume right where you left off.
- **Liquid Glass UI & Spring Motion:** Custom frameless Apple-style window controls, Light & Dark modes (plus Auto system sync), smooth spring-animated floating pill dock, and full **Persian (RTL) / English (LTR)** support (`YekanBakh` + `Google Sans`).
- **IDM-Style `Ctrl+V` Quick Capture:** Press `Ctrl+V` anywhere in the window (or copy a downloadable link) to probe file name, MIME/category, exact size, and resume support before starting.
- **Proxy Profile Manager & DoH:** Save and switch between **SOCKS5** (e.g. v2rayN/Nekoray), **HTTP/HTTPS** (Clash), System Proxy, or Direct, with built-in latency/exit-IP testing and optional **DNS-over-HTTPS (DoH)**.
- **Speed Limiter & Night Scheduler:** Cap global or per-download bandwidth (`KB/s`) and schedule automatic night downloads with Sleep/Shutdown post-actions.
- **Batch & Pattern Grabber:** Expand numbered URL ranges (`https://example.com/part-[01-12].rar`) or extract links from pasted text.

## Building from Source (Windows)

Requires Windows 10/11 (Edge WebView2 Runtime + built-in .NET Framework 4.8 `csc.exe` — no heavy IDE needed):

```powershell
powershell -ExecutionPolicy Bypass -File build-installer.ps1
```

## License

MIT License — see [LICENSE](LICENSE).
