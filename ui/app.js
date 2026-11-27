(function () {
  "use strict";

  const I18N = {
    fa: {
      pageTitle: "دانلودها",
      catAll: "همه",
      catVideo: "ویدیو",
      catCompressed: "فشرده",
      catPrograms: "برنامه",
      catMusic: "موسیقی",
      catDocuments: "اسناد",
      tabAll: "همه",
      tabActive: "دریافت",
      tabCompleted: "تکمیل",
      tabBatch: "دسته‌ای",
      searchPlaceholder: "جستجو...",
      resumeAll: "شروع همه",
      pauseAll: "توقف همه",
      newDownload: "دانلود جدید",
      dropHint: "لینک دانلود را اینجا رها کنید",
      emptyTitle: "فایلی در لیست نیست",
      emptySub: "لینک را با Ctrl+V جایگذاری کنید تا مشخصات فایل بررسی شود، یا دکمه دانلود جدید را بزنید.",
      cancel: "انصراف",
      close: "بستن",
      save: "ذخیره",
      newDownloadTitle: "دانلود جدید",
      startDownload: "شروع دانلود",
      urlLabel: "لینک فایل",
      probeBtn: "بررسی",
      fileNameLabel: "نام فایل",
      categoryLabel: "نوع و دسته‌بندی فایل",
      connectionsLabel: "بخش‌بندی و استریم موازی",
      saveFolderLabel: "مسیر ذخیره",
      browse: "پوشه",
      advancedMirrors: "لینک کمکی (Mirror)، زمان‌بندی و هدرها",
      mirrorsLabel: "لینک‌های جایگزین (هر خط یک لینک)",
      scheduleStart: "زمان‌بندی شروع",
      addPaused: "افزودن متوقف به صف",
      batchTitle: "دانلود دسته‌ای",
      queueSelected: "افزودن",
      batchPatternLabel: "لینک‌ها یا الگوی بازه‌ای مثل [01-12]",
      parseLinks: "استخراج لینک‌ها",
      settingsTitle: "تنظیمات",
      stabGeneral: "عمومی",
      stabNetwork: "پروکسی و شبکه",
      stabScheduler: "زمان‌بند",
      defaultFolderLabel: "پوشه پیش‌فرض دانلود",
      themeLabel: "ظاهر (Theme)",
      themeAuto: "خودکار سیستم",
      themeDark: "تیره (Dark)",
      themeLight: "روشن (Light)",
      defaultConnsLabel: "بخش‌بندی پیش‌فرض",
      maxConcLabel: "دانلود هم‌زمان",
      speedCapLabel: "محدودیت سرعت کل (KB/s، ۰ = نامحدود)",
      unlimited: "نامحدود",
      autoCatLabel: "دسته‌بندی خودکار پوشه‌ها",
      autoCatSub: "ذخیره در پوشه‌های Video، Compressed و ...",
      clipMonLabel: "تشخیص خودکار لینک از کلیپ‌بورد",
      trayLabel: "کوچک‌سازی به System Tray",
      extLabel: "افزونه مرورگر (Chrome / Edge)",
      extSub: "پوشه افزونه را باز کنید و در Extensions با Load Unpacked نصب کنید",
      openExtFolder: "پوشه افزونه",
      reduceMotionLabel: "کاهش انیمیشن",
      reduceTransLabel: "کاهش شفافیت شیشه",
      proxyProfilesLabel: "پروفایل‌های پروکسی ذخیره شده",
      addProxyTitle: "افزودن پروکسی جدید",
      proxyNamePh: "نام (مثلاً v2rayN)",
      addProxyBtn: "افزودن پروکسی",
      testConn: "تست اتصال پروکسی فعال",
      autoResumeNetLabel: "ادامه خودکار پس از وصل شدن اینترنت",
      autoResumeNetSub: "اگر خاموش باشد، با قطع اینترنت دانلود متوقف (قابل ادامه) می‌شود",
      dohLabel: "DNS رمزنگاری‌شده (DoH)",
      stallSecLabel: "تایم‌اوت فریز (ثانیه)",
      minSpeedLabel: "حداقل سرعت (KB/s)",
      maxRetriesLabel: "حداکثر تلاش مجدد",
      nightModeTitle: "زمان‌بندی خودکار شبانه",
      nightModeSub: "شروع صف در ساعت آغاز و توقف در ساعت پایان",
      startTimeLabel: "ساعت شروع",
      endTimeLabel: "ساعت پایان",
      postActionLabel: "پس از اتمام صف",
      actNone: "هیچ‌کدام",
      actNotify: "فقط اعلان",
      actSleep: "Sleep",
      actShutdown: "Shutdown",
      statusLabels: ["در صف", "در حال بررسی", "در حال دانلود", "متوقف (قابل ادامه)", "تکمیل شد", "خطا (قابل ادامه)", "زمان‌بندی", "نهایی‌سازی"],
      segLabels: ["انتظار", "اتصال", "دریافت", "فریز", "تلاش مجدد", "همگام‌سازی", "تکمیل", "خطا"],
      refreshUrl: "تازه‌سازی لینک",
      computeHash: "محاسبه SHA-256 و MD5",
      clipboardDetected: "لینک دانلود در کلیپ‌بورد",
      downloadNow: "دانلود",
      resumable: "قابل ادامه",
      singleStream: "تک‌استریم"
    },
    en: {
      pageTitle: "Downloads",
      catAll: "All",
      catVideo: "Video",
      catCompressed: "Archives",
      catPrograms: "Apps",
      catMusic: "Music",
      catDocuments: "Docs",
      tabAll: "All",
      tabActive: "Active",
      tabCompleted: "Done",
      tabBatch: "Batch",
      searchPlaceholder: "Search...",
      resumeAll: "Resume All",
      pauseAll: "Pause All",
      newDownload: "New Download",
      dropHint: "Drop a download link here",
      emptyTitle: "No downloads yet",
      emptySub: "Press Ctrl+V anywhere to inspect and download a copied link, or click New Download.",
      cancel: "Cancel",
      close: "Close",
      save: "Save",
      newDownloadTitle: "New Download",
      startDownload: "Start Download",
      urlLabel: "File URL",
      probeBtn: "Probe",
      fileNameLabel: "File Name",
      categoryLabel: "File Type & Category",
      connectionsLabel: "Parallel Sections",
      saveFolderLabel: "Save Folder",
      browse: "Browse",
      advancedMirrors: "Mirrors, Schedule & Headers",
      mirrorsLabel: "Mirror URLs (one per line)",
      scheduleStart: "Schedule Start",
      addPaused: "Add Paused",
      batchTitle: "Batch Download",
      queueSelected: "Queue",
      batchPatternLabel: "Paste links or range pattern like [01-12]",
      parseLinks: "Extract Links",
      settingsTitle: "Settings",
      stabGeneral: "General",
      stabNetwork: "Proxy & Network",
      stabScheduler: "Scheduler",
      defaultFolderLabel: "Default Download Folder",
      themeLabel: "Appearance",
      themeAuto: "System Auto",
      themeDark: "Dark",
      themeLight: "Light",
      defaultConnsLabel: "Default Sections",
      maxConcLabel: "Max Concurrent",
      speedCapLabel: "Global Speed Limit (KB/s, 0 = Unlimited)",
      unlimited: "Unlimited",
      autoCatLabel: "Auto-categorize folders",
      autoCatSub: "Organize into Video, Compressed, etc.",
      clipMonLabel: "Smart clipboard capture",
      trayLabel: "Minimize to System Tray",
      extLabel: "Browser Extension (Chrome / Edge)",
      extSub: "Open extension folder and install via Load Unpacked",
      openExtFolder: "Open Folder",
      reduceMotionLabel: "Reduce Motion",
      reduceTransLabel: "Reduce Glass Transparency",
      proxyProfilesLabel: "Saved Proxy Profiles",
      addProxyTitle: "Add New Proxy",
      proxyNamePh: "Profile Name (e.g. v2rayN)",
      addProxyBtn: "Add Proxy",
      testConn: "Test Active Proxy",
      autoResumeNetLabel: "Auto-resume when network returns",
      autoResumeNetSub: "When off, disconnects pause safely like IDM so you can resume manually",
      dohLabel: "Encrypted DNS (DoH)",
      stallSecLabel: "Stall Timeout (sec)",
      minSpeedLabel: "Min Speed (KB/s)",
      maxRetriesLabel: "Max Retries",
      nightModeTitle: "Night Auto-Schedule",
      nightModeSub: "Auto-start queue at start time and pause at end time",
      startTimeLabel: "Start Time",
      endTimeLabel: "End Time",
      postActionLabel: "After Queue Finishes",
      actNone: "Do Nothing",
      actNotify: "Notify Only",
      actSleep: "Sleep",
      actShutdown: "Shutdown",
      statusLabels: ["Queued", "Probing", "Downloading", "Paused (Resumable)", "Completed", "Interrupted (Resumable)", "Scheduled", "Finalizing"],
      segLabels: ["Pending", "Connecting", "Streaming", "Stalled", "Retrying", "Syncing", "Done", "Error"],
      refreshUrl: "Refresh URL",
      computeHash: "Compute SHA-256 & MD5",
      clipboardDetected: "Download link in clipboard",
      downloadNow: "Download",
      resumable: "Resumable",
      singleStream: "Single Stream"
    }
  };

  let appState = { downloads: [], settings: null, totalSpeedBps: 0, activeCount: 0 };
  let lang = "fa";
  let theme = "auto";
  let activeCategory = "All";
  let activeTab = "all";
  let searchQuery = "";
  let selectedId = null;
  let inspectorId = null;
  let ndConnections = 16;
  let ndProbeTimer = null;
  let ndLastProbedUrl = "";
  let ndNameCustomized = false;
  const pendingRpc = new Map();
  let rpcCounter = 0;

  // BitChord Spring Physics Engine (GlassSpring: dampingRatio=0.72, stiffness=320, STRETCH=0.16, SQUASH=0.5)
  const pillPhysics = {
    x: 0, w: 72, vx: 0, vw: 0,
    targetX: 0, targetW: 72, tabStep: 78,
    dragOffset: 0, isDragging: false, rafId: 0, lastTime: 0
  };

  function stepPillSpring(now) {
    if (!pillPhysics.lastTime) pillPhysics.lastTime = now;
    const dt = Math.min(0.032, Math.max(0.001, (now - pillPhysics.lastTime) / 1000));
    pillPhysics.lastTime = now;
    const pillEl = document.getElementById("liquidNavPill");
    if (!pillEl) { pillPhysics.rafId = 0; return; }

    const reduced = document.body.classList.contains("reduce-motion") ||
      window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const goalX = pillPhysics.targetX + pillPhysics.dragOffset;
    const goalW = pillPhysics.targetW;

    if (reduced) {
      pillPhysics.x = goalX; pillPhysics.w = goalW;
      pillPhysics.vx = 0; pillPhysics.vw = 0;
      pillEl.style.width = pillPhysics.w + "px";
      pillEl.style.transform = `translate3d(${pillPhysics.x}px, 0, 0)`;
      pillPhysics.rafId = 0;
      return;
    }

    const stiffness = 320, dampingRatio = 0.72;
    const damping = 2 * dampingRatio * Math.sqrt(stiffness);
    pillPhysics.vx += (-stiffness * (pillPhysics.x - goalX) - damping * pillPhysics.vx) * dt;
    pillPhysics.x += pillPhysics.vx * dt;
    pillPhysics.vw += (-stiffness * (pillPhysics.w - goalW) - damping * pillPhysics.vw) * dt;
    pillPhysics.w += pillPhysics.vw * dt;

    const lag = Math.min(1, Math.abs(goalX - pillPhysics.x) / Math.max(40, pillPhysics.tabStep));
    const scaleX = 1 + lag * 0.16;
    const scaleY = 1 - lag * 0.16 * 0.5;
    pillEl.style.width = pillPhysics.w.toFixed(2) + "px";
    pillEl.style.transform = `translate3d(${pillPhysics.x.toFixed(2)}px, 0, 0) scale(${scaleX.toFixed(4)}, ${scaleY.toFixed(4)})`;

    if (Math.abs(goalX - pillPhysics.x) < 0.15 && Math.abs(pillPhysics.vx) < 0.5 &&
        Math.abs(goalW - pillPhysics.w) < 0.15 && Math.abs(pillPhysics.vw) < 0.5 && !pillPhysics.isDragging) {
      pillPhysics.x = goalX; pillPhysics.w = goalW; pillPhysics.vx = 0; pillPhysics.vw = 0;
      pillEl.style.width = pillPhysics.w + "px";
      pillEl.style.transform = `translate3d(${pillPhysics.x}px, 0, 0) scale(1, 1)`;
      pillPhysics.rafId = 0;
      return;
    }
    pillPhysics.rafId = requestAnimationFrame(stepPillSpring);
  }

  function kickPillSpring(snapImmediate) {
    const bar = document.getElementById("bottomTabBar");
    const activeBtn = bar && bar.querySelector(".bottom-tab.active");
    if (!activeBtn) return;
    pillPhysics.targetX = activeBtn.offsetLeft;
    pillPhysics.targetW = activeBtn.offsetWidth;
    pillPhysics.tabStep = activeBtn.offsetWidth + 6;
    if (snapImmediate) {
      pillPhysics.x = pillPhysics.targetX; pillPhysics.w = pillPhysics.targetW;
      const pillEl = document.getElementById("liquidNavPill");
      if (pillEl) {
        pillEl.style.width = pillPhysics.w + "px";
        pillEl.style.transform = `translate3d(${pillPhysics.x}px, 0, 0)`;
      }
      return;
    }
    if (!pillPhysics.rafId) {
      pillPhysics.lastTime = performance.now();
      pillPhysics.rafId = requestAnimationFrame(stepPillSpring);
    }
  }

  function setupTabBarDrag() {
    const bar = document.getElementById("bottomTabBar");
    const tabs = Array.from(bar.querySelectorAll(".bottom-tab"));
    let startX = 0, moved = false, startTargetX = 0;

    bar.addEventListener("pointerdown", (e) => { startX = e.clientX; moved = false; startTargetX = pillPhysics.targetX; });
    bar.addEventListener("pointermove", (e) => {
      if (e.buttons !== 1) return;
      const dx = e.clientX - startX;
      if (!moved && Math.abs(dx) > 8) { moved = true; pillPhysics.isDragging = true; bar.setPointerCapture(e.pointerId); }
      if (!moved) return;
      const minX = Math.min(...tabs.map((b) => b.offsetLeft));
      const maxX = Math.max(...tabs.map((b) => b.offsetLeft));
      pillPhysics.dragOffset = Math.max(minX, Math.min(maxX, startTargetX + dx)) - pillPhysics.targetX;
      if (!pillPhysics.rafId) { pillPhysics.lastTime = performance.now(); pillPhysics.rafId = requestAnimationFrame(stepPillSpring); }
    });
    const endDrag = () => {
      if (!moved) return;
      moved = false; pillPhysics.isDragging = false;
      const curX = pillPhysics.targetX + pillPhysics.dragOffset;
      pillPhysics.dragOffset = 0;
      let closest = tabs[0], bestDist = Infinity;
      tabs.forEach((btn) => { const d = Math.abs(btn.offsetLeft - curX); if (d < bestDist) { bestDist = d; closest = btn; } });
      selectBottomTab(closest.getAttribute("data-tab"));
    };
    bar.addEventListener("pointerup", endDrag);
    bar.addEventListener("pointercancel", endDrag);
  }

  function selectBottomTab(tabName) {
    if (tabName === "batch") { openSheet("batchSheet"); kickPillSpring(false); return; }
    activeTab = tabName;
    document.querySelectorAll("#bottomTabBar .bottom-tab").forEach((b) => {
      const act = b.getAttribute("data-tab") === tabName;
      b.classList.toggle("active", act);
      b.setAttribute("aria-selected", act ? "true" : "false");
    });
    kickPillSpring(false);
    renderDownloadList();
  }

  // WebView2 IPC
  function callHost(action, data) {
    if (!window.chrome || !window.chrome.webview) return Promise.resolve(null);
    const reqId = "req_" + (++rpcCounter);
    return new Promise((resolve) => {
      pendingRpc.set(reqId, resolve);
      window.chrome.webview.postMessage({ action, reqId, data: data || {} });
      setTimeout(() => { if (pendingRpc.has(reqId)) { pendingRpc.delete(reqId); resolve(null); } }, 15000);
    });
  }
  function sendHost(action, data) {
    if (!window.chrome || !window.chrome.webview) return;
    window.chrome.webview.postMessage({ action, data: data || {} });
  }

  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener("message", (event) => {
      const msg = event.data;
      if (!msg || !msg.type) return;
      if (msg.type === "state" && msg.payload) {
        const first = !appState.settings;
        appState = msg.payload;
        if (first && appState.settings) {
          lang = appState.settings.Language === "en" ? "en" : "fa";
          theme = appState.settings.Theme || "auto";
          applyLanguage();
          applyTheme();
          applyAccessibilityClasses();
        }
        renderAll();
      } else if (msg.type === "rpc_response" && msg.payload) {
        const { reqId, result } = msg.payload;
        if (pendingRpc.has(reqId)) { const fn = pendingRpc.get(reqId); pendingRpc.delete(reqId); fn(result); }
      } else if (msg.type === "quick_add" && msg.payload) {
        openNewDownloadWithUrl(msg.payload.url, msg.payload.fileName, msg.payload.referer);
      } else if (msg.type === "clipboard_url" && msg.payload) {
        showClipboardToast(msg.payload.url);
      } else if (msg.type === "toast" && msg.payload) {
        showToast(lang === "fa" && msg.payload.titleFa ? msg.payload.titleFa : msg.payload.title, msg.payload.message);
      } else if (msg.type === "window_state" && msg.payload) {
        const maxBtn = document.getElementById("winMaxBtn");
        if (maxBtn) maxBtn.classList.toggle("maximized", !!msg.payload.maximized);
      }
    });
  }

  function t(k) { return (I18N[lang] && I18N[lang][k]) || I18N.en[k] || k; }
  function formatBytes(b) {
    if (!b || b <= 0) return "0 B";
    const u = ["B", "KB", "MB", "GB", "TB"];
    const i = Math.min(u.length - 1, Math.floor(Math.log(b) / Math.log(1024)));
    return (b / Math.pow(1024, i)).toFixed(i === 0 ? 0 : 2) + " " + u[i];
  }
  function formatSpeed(bps) {
    if (!bps || bps <= 0) return "0 KB/s";
    if (bps >= 1048576) return (bps / 1048576).toFixed(2) + " MB/s";
    return (bps / 1024).toFixed(0) + " KB/s";
  }
  function formatEta(s) {
    if (!s || s <= 0 || !isFinite(s)) return "";
    s = Math.round(s);
    const h = Math.floor(s / 3600), m = Math.floor((s % 3600) / 60), sec = s % 60;
    if (h > 0) return `${h}h ${m}m`;
    if (m > 0) return `${m}m ${sec}s`;
    return `${sec}s`;
  }
  function escapeHtml(str) {
    return String(str || "").replace(/[&<>"']/g, (m) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" }[m]));
  }

  function applyTheme() {
    document.documentElement.setAttribute("data-theme", theme || "auto");
    const icon = document.getElementById("themeIcon");
    if (!icon) return;
    if (theme === "light") {
      icon.innerHTML = `<circle cx="12" cy="12" r="4"/><path d="M12 2v2m0 16v2M4.93 4.93l1.41 1.41m11.32 11.32l1.41 1.41M2 12h2m16 0h2M4.93 19.07l1.41-1.41m11.32-11.32l1.41-1.41"/>`;
    } else if (theme === "dark") {
      icon.innerHTML = `<path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>`;
    } else {
      icon.innerHTML = `<circle cx="12" cy="12" r="9"/><path d="M12 3v18a9 9 0 0 0 0-18z" fill="currentColor"/>`;
    }
  }

  function applyLanguage() {
    document.documentElement.lang = lang;
    document.documentElement.dir = lang === "fa" ? "rtl" : "ltr";
    document.getElementById("langToggleBtn").textContent = lang === "fa" ? "FA" : "EN";
    document.querySelectorAll("[data-i18n]").forEach((el) => { el.textContent = t(el.getAttribute("data-i18n")); });
    document.querySelectorAll("[data-i18n-placeholder]").forEach((el) => { el.placeholder = t(el.getAttribute("data-i18n-placeholder")); });
    document.querySelectorAll("[data-i18n-title]").forEach((el) => { el.title = t(el.getAttribute("data-i18n-title")); });
    requestAnimationFrame(() => kickPillSpring(true));
  }

  function applyAccessibilityClasses() {
    if (!appState.settings) return;
    document.body.classList.toggle("reduce-motion", !!appState.settings.ReduceMotion);
    document.body.classList.toggle("reduce-transparency", !!appState.settings.ReduceTransparency);
  }

  // Sheets + Drag-to-Dismiss
  function openSheet(id) {
    const backdrop = document.getElementById("sheetBackdrop");
    const sheet = document.getElementById(id);
    if (!sheet) return;
    document.querySelectorAll(".glass-sheet.open").forEach((s) => closeSheet(s.id));
    backdrop.classList.remove("hidden");
    sheet.classList.remove("hidden");
    sheet.style.transform = "";
    requestAnimationFrame(() => { backdrop.classList.add("visible"); sheet.classList.add("open"); });
  }

  function closeSheet(id) {
    const sheet = document.getElementById(id);
    const backdrop = document.getElementById("sheetBackdrop");
    if (!sheet) return;
    sheet.classList.remove("open");
    sheet.style.transform = "";
    backdrop.classList.remove("visible");
    setTimeout(() => {
      sheet.classList.add("hidden");
      if (!document.querySelector(".glass-sheet.open")) backdrop.classList.add("hidden");
    }, 260);
    if (id === "inspectorSheet") inspectorId = null;
  }

  function setupSheetDrag() {
    document.querySelectorAll(".sheet-grabber").forEach((grabber) => {
      const sheetId = grabber.getAttribute("data-dismiss-sheet");
      const sheet = document.getElementById(sheetId);
      let dragging = false, startY = 0, lastY = 0, lastT = 0, vy = 0;
      grabber.addEventListener("pointerdown", (e) => {
        dragging = true; startY = e.clientY; lastY = startY; lastT = performance.now(); vy = 0;
        sheet.style.transition = "none"; grabber.setPointerCapture(e.pointerId);
      });
      grabber.addEventListener("pointermove", (e) => {
        if (!dragging) return;
        const now = performance.now();
        vy = ((e.clientY - lastY) / Math.max(1, now - lastT)) * 1000;
        lastY = e.clientY; lastT = now;
        const dy = e.clientY - startY;
        const clamped = dy >= 0 ? dy : (dy * 320 * 0.55) / (320 + 0.55 * Math.abs(dy));
        sheet.style.transform = `translate(-50%, calc(-50% + ${clamped}px)) scale(1)`;
      });
      grabber.addEventListener("pointerup", (e) => {
        if (!dragging) return;
        dragging = false; sheet.style.transition = "";
        const dy = e.clientY - startY;
        const projected = dy + (vy / 1000) * 0.998 / (1 - 0.998) * 0.25;
        if (projected > 140) closeSheet(sheetId);
        else sheet.style.transform = "";
      });
    });
  }

  // Rendering
  function renderAll() {
    renderHeaderQuickPills();
    renderDownloadList();
    renderMiniMonitor();
    if (inspectorId) renderInspector();
  }

  function renderHeaderQuickPills() {
    if (!appState.settings) return;
    const p = appState.settings.Proxy;
    const pPill = document.getElementById("quickProxyPill");
    if (pPill && p) {
      if (p.Mode === 0) pPill.textContent = "Proxy: Direct";
      else if (p.Mode === 1) pPill.textContent = "Proxy: System";
      else pPill.textContent = `${p.Mode === 3 ? "SOCKS5" : "HTTP"} ${p.Host}:${p.Port}`;
    }
    const cPill = document.getElementById("quickCapPill");
    if (cPill) {
      const cap = appState.settings.GlobalSpeedLimitKBps || 0;
      cPill.textContent = cap > 0 ? `Cap: ${cap} KB/s` : "Speed: ∞";
    }
  }

  function getFilteredDownloads() {
    return (appState.downloads || []).filter((d) => {
      if (activeCategory !== "All" && d.Category !== activeCategory) return false;
      if (activeTab === "active" && d.Status !== 2 && d.Status !== 1 && d.Status !== 7 && d.Status !== 0) return false;
      if (activeTab === "completed" && d.Status !== 4) return false;
      if (searchQuery) {
        const q = searchQuery.toLowerCase();
        if (!(d.FileName || "").toLowerCase().includes(q) && !(d.Url || "").toLowerCase().includes(q)) return false;
      }
      return true;
    });
  }

  function renderDownloadList() {
    const listEl = document.getElementById("downloadList");
    const emptyEl = document.getElementById("emptyState");
    const items = getFilteredDownloads();
    if (items.length === 0) {
      listEl.innerHTML = "";
      emptyEl.classList.remove("hidden");
      return;
    }
    emptyEl.classList.add("hidden");
    const existing = new Map();
    Array.from(listEl.children).forEach((el) => existing.set(el.dataset.id, el));
    const nodes = items.map((d) => {
      const card = existing.get(d.Id) || createDownloadRow(d);
      updateDownloadRow(card, d);
      return card;
    });
    listEl.replaceChildren(...nodes);
  }

  function createDownloadRow(d) {
    const row = document.createElement("div");
    row.className = "dl-row";
    row.dataset.id = d.Id;
    row.setAttribute("role", "listitem");
    row.innerHTML = `
      <div class="dl-top">
        <div class="dl-badge"></div>
        <div class="dl-info">
          <div class="dl-name"></div>
          <div class="dl-sub"></div>
        </div>
        <div class="dl-actions">
          <button class="dl-btn pressable" data-act="toggle"></button>
          <button class="dl-btn pressable" data-act="inspect" title="Inspector">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          </button>
          <button class="dl-btn pressable" data-act="folder">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>
          </button>
          <button class="dl-btn danger pressable" data-act="delete">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/></svg>
          </button>
        </div>
      </div>
      <div class="progress-track"><div class="progress-fill"></div></div>
    `;
    row.addEventListener("click", (e) => {
      const btn = e.target.closest("[data-act]");
      if (btn) {
        e.stopPropagation();
        const act = btn.getAttribute("data-act");
        const id = row.dataset.id;
        const item = (appState.downloads || []).find((x) => x.Id === id);
        if (!item) return;
        if (act === "toggle") {
          if (item.Status === 4) sendHost("open_file", { path: item.FinalFilePath });
          else if (item.Status === 2 || item.Status === 1 || item.Status === 0) sendHost("pause", { id });
          else sendHost("resume", { id });
        } else if (act === "inspect") {
          inspectorId = id; openSheet("inspectorSheet"); renderInspector();
        } else if (act === "folder") {
          sendHost("open_folder", { path: item.Status === 4 ? item.FinalFilePath : item.SaveFolder });
        } else if (act === "delete") {
          sendHost("remove", { id, deleteFiles: item.Status !== 4 });
        }
        return;
      }
      selectedId = row.dataset.id;
      document.querySelectorAll(".dl-row.selected").forEach((r) => r.classList.remove("selected"));
      row.classList.add("selected");
    });
    row.addEventListener("dblclick", () => {
      const item = (appState.downloads || []).find((x) => x.Id === row.dataset.id);
      if (item && item.Status === 4) sendHost("open_file", { path: item.FinalFilePath });
      else { inspectorId = row.dataset.id; openSheet("inspectorSheet"); renderInspector(); }
    });
    return row;
  }

  function updateDownloadRow(row, d) {
    row.classList.toggle("selected", selectedId === d.Id);
    const badge = row.querySelector(".dl-badge");
    badge.className = "dl-badge " + (d.Status === 4 ? "done" : d.Status === 2 ? "active" : "");
    badge.textContent = d.Status === 4 ? "✓" : ((d.FileName || "").split(".").pop().slice(0, 4).toUpperCase() || "FILE");
    row.querySelector(".dl-name").textContent = d.FileName || d.Url;

    const pct = d.TotalBytes > 0 ? Math.min(100, (d.DownloadedBytes / d.TotalBytes) * 100) : (d.Status === 4 ? 100 : 0);
    const st = I18N[lang].statusLabels[d.Status] || d.StatusDetail || "";
    const parts = [
      st,
      d.TotalBytes > 0 ? `${formatBytes(d.DownloadedBytes)} / ${formatBytes(d.TotalBytes)} (${pct.toFixed(1)}%)` : formatBytes(d.DownloadedBytes)
    ];
    if (d.Status === 2) {
      parts.push(formatSpeed(d.SpeedBps));
      const eta = formatEta(d.EtaSeconds);
      if (eta) parts.push(eta);
    } else if (d.Status === 5 && d.ErrorMessage) {
      parts.push(d.ErrorMessage);
    }
    row.querySelector(".dl-sub").textContent = parts.join(" · ");

    const tBtn = row.querySelector('[data-act="toggle"]');
    if (d.Status === 4) {
      tBtn.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/></svg>`;
    } else if (d.Status === 2 || d.Status === 1 || d.Status === 0) {
      tBtn.innerHTML = `<svg viewBox="0 0 24 24" fill="currentColor"><rect x="7" y="5" width="3.5" height="14" rx="1"/><rect x="13.5" y="5" width="3.5" height="14" rx="1"/></svg>`;
    } else {
      tBtn.innerHTML = `<svg viewBox="0 0 24 24" fill="currentColor"><polygon points="7 5 19 12 7 19 7 5"/></svg>`;
    }
    const fill = row.querySelector(".progress-fill");
    fill.style.width = pct.toFixed(2) + "%";
    fill.className = "progress-fill " + (d.Status === 4 ? "done" : d.Status === 3 ? "paused" : d.Status === 5 ? "error" : "");
  }

  function renderMiniMonitor() {
    const mini = document.getElementById("miniMonitor");
    const active = (appState.downloads || []).filter((d) => d.Status === 2 || d.Status === 1);
    if (active.length === 0) { mini.classList.add("hidden"); return; }
    mini.classList.remove("hidden");
    const primary = active[0];
    document.getElementById("miniTitle").textContent = primary.FileName || primary.Url;
    document.getElementById("miniSub").textContent = `${formatSpeed(appState.totalSpeedBps)} · ${active.length} active`;
    const pct = primary.TotalBytes > 0 ? Math.min(1, primary.DownloadedBytes / primary.TotalBytes) : 0;
    document.getElementById("miniRingFill").style.strokeDashoffset = (69.115 * (1 - pct)).toFixed(2);
    mini.onclick = (e) => {
      if (e.target.closest("#miniPauseBtn")) return;
      inspectorId = primary.Id; openSheet("inspectorSheet"); renderInspector();
    };
  }

  function renderInspector() {
    const d = (appState.downloads || []).find((x) => x.Id === inspectorId);
    if (!d) return;
    document.getElementById("inspTitle").textContent = d.FileName || "Inspector";
    const tBtn = document.getElementById("inspToggleBtn");
    tBtn.textContent = d.Status === 2 ? (lang === "fa" ? "توقف" : "Pause") : (lang === "fa" ? "ادامه" : "Resume");
    tBtn.onclick = () => sendHost(d.Status === 2 ? "pause" : "resume", { id: d.Id });

    const segs = (d.Segments || []).map((s) => {
      const total = s.EndByte >= s.StartByte ? (s.EndByte - s.StartByte + 1) : 0;
      const done = Math.max(0, s.CurrentByte - s.StartByte);
      const pct = total > 0 ? Math.min(100, (done / total) * 100) : 0;
      return `
        <div class="seg-cell">
          <div class="seg-cell-top"><span>#${s.Index + 1} ${I18N[lang].segLabels[s.Status] || ""}</span><span>${pct.toFixed(0)}%</span></div>
          <div class="progress-track"><div class="progress-fill ${s.Status === 6 ? "done" : ""}" style="width:${pct.toFixed(1)}%"></div></div>
          <div class="muted-label" style="margin-top:4px;">${formatSpeed(s.SpeedBps)}</div>
        </div>
      `;
    }).join("");

    document.getElementById("inspectorBody").innerHTML = `
      <div class="info-pill" style="flex-direction:column;gap:4px;">
        <div style="display:flex;justify-content:space-between;">
          <strong>${formatBytes(d.DownloadedBytes)} / ${formatBytes(d.TotalBytes)}</strong>
          <span>${formatSpeed(d.SpeedBps)}</span>
        </div>
        <div class="muted-label ltr-text" style="word-break:break-all;">${escapeHtml(d.Url)}</div>
      </div>
      <div class="row-gap">
        <input id="inspRefreshUrl" type="url" class="ios-input ltr-text" value="${escapeHtml(d.Url)}" />
        <button id="inspRefreshBtn" class="secondary-pill-btn pressable">${t("refreshUrl")}</button>
      </div>
      <div class="row-gap">
        <input id="inspSpeedCap" type="number" min="0" class="ios-input ltr-text" style="max-width:160px;" value="${d.SpeedLimitKBps || 0}" placeholder="KB/s (0=∞)" />
        <button id="inspSpeedBtn" class="secondary-pill-btn pressable">${lang === "fa" ? "اعمال سقف سرعت" : "Set Speed Cap"}</button>
        <button id="inspHashBtn" class="secondary-pill-btn pressable">${t("computeHash")}</button>
      </div>
      ${d.Sha256 ? `<div class="muted-label ltr-text">SHA256: ${d.Sha256}</div>` : ""}
      <div class="seg-grid">${segs}</div>
    `;
    document.getElementById("inspRefreshBtn").onclick = () => {
      const u = document.getElementById("inspRefreshUrl").value.trim();
      if (u) sendHost("refresh_address", { id: d.Id, url: u });
    };
    document.getElementById("inspSpeedBtn").onclick = () => {
      const kb = parseInt(document.getElementById("inspSpeedCap").value, 10) || 0;
      sendHost("set_item_speed_limit", { id: d.Id, kbps: kb });
    };
    document.getElementById("inspHashBtn").onclick = async () => {
      const r = await callHost("compute_checksum", { id: d.Id });
      if (r && r.sha256) showToast("SHA-256", r.sha256);
    };
  }

  function updateNdFolderForCategory() {
    if (!appState.settings) return;
    const base = appState.settings.DefaultSaveFolder || "";
    const cat = document.getElementById("ndCategory").value;
    if (appState.settings.AutoCategorizeFolders && cat && cat !== "Other") {
      document.getElementById("ndFolder").value = base.replace(/[\\\/]+$/, "") + "\\" + cat;
    } else {
      document.getElementById("ndFolder").value = base;
    }
  }

  function extractFirstHttpUrl(text) {
    if (!text) return "";
    const m = String(text).trim().match(/https?:\/\/[^\s"'<>\]\)]+/i);
    return m ? m[0] : "";
  }

  function openNewDownloadWithUrl(url, fileName, referer) {
    const cleanUrl = extractFirstHttpUrl(url) || (url || "").trim();
    ndLastProbedUrl = "";
    ndNameCustomized = Boolean(fileName && fileName.trim());
    document.getElementById("ndUrl").value = cleanUrl;
    document.getElementById("ndFileName").value = fileName || "";
    document.getElementById("ndReferer").value = referer || "";
    document.getElementById("ndCategory").value = "Other";
    updateNdFolderForCategory();
    document.getElementById("ndProbeCard").classList.add("hidden");
    openSheet("newDownloadSheet");
    if (cleanUrl) {
      probeCurrentUrl(true);
    } else {
      setTimeout(() => { const u = document.getElementById("ndUrl"); if (u) u.focus(); }, 60);
    }
  }

  async function probeCurrentUrl(force) {
    const url = document.getElementById("ndUrl").value.trim();
    if (!/^https?:\/\/\S+/i.test(url)) return;
    if (!force && url === ndLastProbedUrl) return;
    ndLastProbedUrl = url;
    const card = document.getElementById("ndProbeCard");
    card.classList.remove("hidden");
    card.innerHTML = `<span>${lang === "fa" ? "در حال بررسی لینک و نوع فایل..." : "Probing link & file type..."}</span>`;
    const r = await callHost("probe_url", { url, referer: document.getElementById("ndReferer").value.trim() });
    if (document.getElementById("ndUrl").value.trim() !== url) return;
    if (!r || !r.Success) { card.textContent = (r && r.Error) || (lang === "fa" ? "خطا در بررسی لینک" : "Failed"); return; }
    if (r.FileName && !ndNameCustomized) {
      document.getElementById("ndFileName").value = r.FileName;
    }
    if (r.Category) {
      document.getElementById("ndCategory").value = r.Category;
      updateNdFolderForCategory();
    }
    const mimeShort = (r.MimeType || "").split(";")[0] || r.Category || "File";
    card.innerHTML = `
      <div>
        <strong>${escapeHtml(r.FileName || "")}</strong>
        <div class="muted-label ltr-text">${escapeHtml(mimeShort)}</div>
      </div>
      <div class="probe-badges">
        <span class="probe-tag">${formatBytes(r.TotalBytes)}</span>
        <span class="probe-tag ${r.SupportsRange ? "ok" : ""}">${r.SupportsRange ? t("resumable") + " ✓" : t("singleStream")}</span>
      </div>
    `;
  }

  function scheduleAutoProbe(immediate) {
    if (ndProbeTimer) { clearTimeout(ndProbeTimer); ndProbeTimer = null; }
    const delay = immediate ? 30 : 420;
    ndProbeTimer = setTimeout(() => {
      const urlInput = document.getElementById("ndUrl");
      if (!urlInput) return;
      const val = urlInput.value.trim();
      const extracted = extractFirstHttpUrl(val);
      if (extracted && extracted !== val) urlInput.value = extracted;
      if (/^https?:\/\/[^\/\s]+/i.test(urlInput.value.trim())) {
        probeCurrentUrl(false);
      }
    }, delay);
  }

  function expandBatchInput(raw) {
    const urls = new Set();
    raw.split(/\r?\n/).forEach((line) => {
      const m = line.match(/\[(\d+)-(\d+)\]/);
      if (m) {
        const pad = m[1].length, a = parseInt(m[1], 10), b = parseInt(m[2], 10);
        for (let i = a; i <= Math.min(b, a + 200); i++) urls.add(line.replace(m[0], String(i).padStart(pad, "0")).trim());
      } else {
        const found = line.match(/https?:\/\/[^\s"'<>]+/gi);
        if (found) found.forEach((u) => urls.add(u));
      }
    });
    return Array.from(urls);
  }

  function showToast(title, msg) {
    const stack = document.getElementById("toastStack");
    const card = document.createElement("div");
    card.className = "toast-card";
    card.innerHTML = `<div><div style="font-weight:600;font-size:13px;">${escapeHtml(title)}</div><div class="muted-label">${escapeHtml(msg)}</div></div>`;
    stack.appendChild(card);
    setTimeout(() => { card.classList.add("leaving"); setTimeout(() => card.remove(), 220); }, 3800);
  }

  function showClipboardToast(url) {
    const stack = document.getElementById("toastStack");
    const card = document.createElement("div");
    card.className = "toast-card";
    card.innerHTML = `
      <div style="min-width:0;flex:1;">
        <div style="font-weight:600;font-size:12.5px;">${t("clipboardDetected")}</div>
        <div class="muted-label ltr-text" style="white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${escapeHtml(url)}</div>
      </div>
      <button class="sheet-primary-btn pressable">${t("downloadNow")}</button>
    `;
    card.querySelector("button").onclick = () => { card.remove(); openNewDownloadWithUrl(url, "", ""); };
    stack.appendChild(card);
    setTimeout(() => { if (card.isConnected) { card.classList.add("leaving"); setTimeout(() => card.remove(), 220); } }, 6500);
  }

  function renderProxyProfiles() {
    if (!appState.settings || !appState.settings.Proxy) return;
    const p = appState.settings.Proxy;
    const activeId = p.ActiveProfileId || (p.Mode === 0 ? "direct" : p.Mode === 1 ? "system" : "custom");
    const builtIns = [
      { Id: "system", Name: t("presetSystem"), Mode: 1, Host: "Windows", Port: 0, builtIn: true },
      { Id: "direct", Name: t("presetDirect"), Mode: 0, Host: "No Proxy", Port: 0, builtIn: true }
    ];
    const saved = (p.Profiles || []).map((x) => ({ ...x, builtIn: false }));
    const all = [...builtIns, ...saved];

    const listEl = document.getElementById("proxyProfileList");
    listEl.innerHTML = all.map((item) => {
      const isAct = activeId === item.Id;
      const addr = item.Mode === 0 ? "Direct" : item.Mode === 1 ? "System Proxy" : `${item.Mode === 3 ? "SOCKS5" : "HTTP"} ${item.Host}:${item.Port}`;
      return `
        <div class="proxy-item ${isAct ? "active" : ""}" data-pid="${escapeHtml(item.Id)}">
          <div class="proxy-meta">
            <span class="proxy-name">${escapeHtml(item.Name)} ${isAct ? "✓" : ""}</span>
            <span class="proxy-addr">${escapeHtml(addr)}</span>
          </div>
          ${item.builtIn ? "" : `<button type="button" class="dl-btn danger pressable" data-del-proxy="${escapeHtml(item.Id)}"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/></svg></button>`}
        </div>
      `;
    }).join("");

    listEl.querySelectorAll(".proxy-item").forEach((el) => {
      el.onclick = (e) => {
        const delBtn = e.target.closest("[data-del-proxy]");
        if (delBtn) {
          e.stopPropagation();
          const delId = delBtn.getAttribute("data-del-proxy");
          p.Profiles = (p.Profiles || []).filter((x) => x.Id !== delId);
          if (p.ActiveProfileId === delId) { p.ActiveProfileId = "system"; p.Mode = 1; }
          renderProxyProfiles();
          return;
        }
        const pid = el.getAttribute("data-pid");
        const chosen = all.find((x) => x.Id === pid);
        if (!chosen) return;
        p.ActiveProfileId = chosen.Id;
        p.Mode = chosen.Mode;
        if (!chosen.builtIn) {
          p.Host = chosen.Host; p.Port = chosen.Port;
          p.Username = chosen.Username || ""; p.Password = chosen.Password || "";
        }
        renderProxyProfiles();
      };
    });
  }

  function hydrateSettings() {
    if (!appState.settings) return;
    const s = appState.settings;
    document.getElementById("setFolder").value = s.DefaultSaveFolder || "";
    document.getElementById("setTheme").value = s.Theme || "auto";
    document.getElementById("setDefConns").value = s.DefaultConnections || 16;
    document.getElementById("setMaxConc").value = s.MaxConcurrentDownloads || 3;
    document.getElementById("setSpeedCap").value = s.GlobalSpeedLimitKBps || 0;
    document.getElementById("setAutoCat").checked = !!s.AutoCategorizeFolders;
    document.getElementById("setClipMon").checked = !!s.ClipboardMonitor;
    document.getElementById("setTray").checked = !!s.MinimizeToTray;
    document.getElementById("setReduceMotion").checked = !!s.ReduceMotion;
    document.getElementById("setReduceTrans").checked = !!s.ReduceTransparency;

    document.getElementById("setAutoResumeNet").checked = !!s.AutoResumeOnReconnect;
    document.getElementById("dohEnabled").checked = !!s.Doh.Enabled;
    document.getElementById("dohProvider").value = s.Doh.Endpoint || "https://1.1.1.1/dns-query";
    document.getElementById("irStallSec").value = s.StallTimeoutSeconds || 8;
    document.getElementById("irMinSpeed").value = s.MinStallSpeedKBps || 4;
    document.getElementById("irMaxRetries").value = s.MaxRetriesPerSegment || 24;

    document.getElementById("schNightEnabled").checked = !!s.Scheduler.NightModeEnabled;
    document.getElementById("schStart").value = s.Scheduler.StartTime || "02:00";
    document.getElementById("schEnd").value = s.Scheduler.EndTime || "07:00";
    document.getElementById("schPostAction").value = String(s.Scheduler.PostAction || 0);
    renderProxyProfiles();
  }

  function openSettingsTab(tab) {
    hydrateSettings();
    openSheet("settingsSheet");
    document.querySelectorAll(".s-tab").forEach((x) => x.classList.toggle("active", x.getAttribute("data-stab") === tab));
    document.querySelectorAll(".stab-pane").forEach((p) => p.classList.toggle("hidden", p.id !== "pane-" + tab));
  }

  function bindEvents() {
    // Custom Window Drag & Traffic Lights
    const dragStrip = document.getElementById("windowDragRegion");
    dragStrip.addEventListener("mousedown", (e) => { if (e.button === 0 && e.detail === 1) sendHost("window_drag"); });
    dragStrip.addEventListener("dblclick", () => sendHost("window_maximize"));
    document.getElementById("winCloseBtn").onclick = () => sendHost("window_close");
    document.getElementById("winMinBtn").onclick = () => sendHost("window_minimize");
    document.getElementById("winMaxBtn").onclick = () => sendHost("window_maximize");

    const stage = document.getElementById("scrollStage");
    stage.addEventListener("scroll", () => {
      document.getElementById("topBar").classList.toggle("scrolled", stage.scrollTop > 44);
    });

    document.getElementById("categoryChips").addEventListener("click", (e) => {
      const btn = e.target.closest(".cat-chip");
      if (!btn) return;
      activeCategory = btn.getAttribute("data-cat");
      document.querySelectorAll(".cat-chip").forEach((c) => c.classList.toggle("active", c === btn));
      renderDownloadList();
    });

    document.getElementById("bottomTabBar").addEventListener("click", (e) => {
      const btn = e.target.closest(".bottom-tab");
      if (!btn) return;
      selectBottomTab(btn.getAttribute("data-tab"));
    });
    setupTabBarDrag();

    const orb = document.getElementById("searchOrb");
    const searchInput = document.getElementById("searchInput");
    document.getElementById("searchToggleBtn").onclick = () => {
      orb.classList.toggle("expanded");
      if (orb.classList.contains("expanded")) searchInput.focus();
      else { searchInput.value = ""; searchQuery = ""; renderDownloadList(); }
    };
    searchInput.addEventListener("input", (e) => { searchQuery = e.target.value.trim(); renderDownloadList(); });

    document.getElementById("themeToggleBtn").onclick = () => {
      const order = ["auto", "light", "dark"];
      theme = order[(order.indexOf(theme) + 1) % order.length];
      applyTheme();
      if (appState.settings) {
        appState.settings.Theme = theme;
        callHost("save_settings", { settings: appState.settings });
      }
      showToast("Theme", theme.toUpperCase());
    };

    document.getElementById("langToggleBtn").onclick = () => {
      lang = lang === "fa" ? "en" : "fa";
      applyLanguage();
      if (appState.settings) {
        appState.settings.Language = lang;
        callHost("save_settings", { settings: appState.settings });
      }
      renderAll();
    };

    document.getElementById("newDownloadBtn").onclick = () => openNewDownloadWithUrl("", "", "");
    document.getElementById("emptyAddBtn").onclick = () => openNewDownloadWithUrl("", "", "");
    document.getElementById("resumeAllBtn").onclick = () => sendHost("resume_all");
    document.getElementById("pauseAllBtn").onclick = () => sendHost("pause_all");
    document.getElementById("miniPauseBtn").onclick = () => sendHost("pause_all");
    document.getElementById("settingsBtn").onclick = () => openSettingsTab("general");
    document.getElementById("quickProxyPill").onclick = () => openSettingsTab("network");
    document.getElementById("quickCapPill").onclick = () => openSettingsTab("general");

    document.querySelectorAll("[data-close-sheet]").forEach((b) => { b.onclick = () => closeSheet(b.getAttribute("data-close-sheet")); });
    document.getElementById("sheetBackdrop").onclick = () => { document.querySelectorAll(".glass-sheet.open").forEach((s) => closeSheet(s.id)); };

    // New Download Sheet
    document.querySelectorAll(".conn-pill").forEach((b) => {
      b.onclick = () => {
        ndConnections = parseInt(b.getAttribute("data-conn"), 10);
        document.querySelectorAll(".conn-pill").forEach((x) => x.classList.toggle("active", x === b));
      };
    });
    document.getElementById("ndCategory").addEventListener("change", updateNdFolderForCategory);
    document.getElementById("ndFileName").addEventListener("input", () => {
      ndNameCustomized = Boolean(document.getElementById("ndFileName").value.trim());
    });
    const ndUrlEl = document.getElementById("ndUrl");
    ndUrlEl.addEventListener("input", () => {
      if (!ndNameCustomized) document.getElementById("ndFileName").value = "";
      scheduleAutoProbe(false);
    });
    ndUrlEl.addEventListener("paste", () => {
      ndNameCustomized = false;
      ndLastProbedUrl = "";
      scheduleAutoProbe(true);
    });
    ndUrlEl.addEventListener("change", () => scheduleAutoProbe(true));
    document.getElementById("ndProbeBtn").onclick = () => probeCurrentUrl(true);
    document.getElementById("ndBrowseBtn").onclick = async () => {
      const r = await callHost("pick_folder");
      if (r && r.folder) document.getElementById("ndFolder").value = r.folder;
    };
    const submitNd = async (startNow) => {
      const url = document.getElementById("ndUrl").value.trim();
      if (!url) return;
      const mirrors = document.getElementById("ndMirrors").value.split(/\r?\n/).map((s) => s.trim()).filter(Boolean);
      await callHost("add_download", {
        url,
        fileName: document.getElementById("ndFileName").value.trim(),
        folder: document.getElementById("ndFolder").value.trim(),
        connections: ndConnections,
        startNow,
        iranResilient: true,
        mirrors,
        referer: document.getElementById("ndReferer").value.trim(),
        scheduledFor: document.getElementById("ndSchedule").value
      });
      closeSheet("newDownloadSheet");
    };
    document.getElementById("ndStartBtn").onclick = () => submitNd(true);
    document.getElementById("ndAddPausedBtn").onclick = () => submitNd(false);

    // Batch
    document.getElementById("batchParseBtn").onclick = () => {
      const urls = expandBatchInput(document.getElementById("batchInput").value);
      document.getElementById("batchCountLabel").textContent = `${urls.length} links`;
      document.getElementById("batchPreviewList").innerHTML = urls.map((u) =>
        `<label class="batch-row"><input type="checkbox" checked value="${escapeHtml(u)}" /><span>${escapeHtml(u)}</span></label>`
      ).join("");
    };
    document.getElementById("batchStartBtn").onclick = async () => {
      const urls = Array.from(document.querySelectorAll("#batchPreviewList input:checked")).map((i) => i.value);
      if (!urls.length) return;
      await callHost("batch_add", { urls, startNow: true });
      closeSheet("batchSheet");
    };

    // Settings
    document.getElementById("settingsTabs").addEventListener("click", (e) => {
      const btn = e.target.closest(".s-tab");
      if (!btn) return;
      const tab = btn.getAttribute("data-stab");
      document.querySelectorAll(".s-tab").forEach((x) => x.classList.toggle("active", x === btn));
      document.querySelectorAll(".stab-pane").forEach((p) => p.classList.toggle("hidden", p.id !== "pane-" + tab));
    });

    document.querySelectorAll(".cap-chip").forEach((c) => {
      c.onclick = () => { document.getElementById("setSpeedCap").value = c.getAttribute("data-cap"); };
    });

    document.getElementById("openExtFolderBtn").onclick = () => sendHost("open_ext_folder");

    document.getElementById("addProxyProfileBtn").onclick = () => {
      if (!appState.settings) return;
      const name = document.getElementById("pxName").value.trim() || "Custom Proxy";
      const mode = parseInt(document.getElementById("pxMode").value, 10);
      const host = document.getElementById("pxHost").value.trim() || "127.0.0.1";
      const port = parseInt(document.getElementById("pxPort").value, 10) || 10808;
      const user = document.getElementById("pxUser").value.trim();
      const pass = document.getElementById("pxPass").value;
      const prof = {
        Id: "px_" + Date.now().toString(36),
        Name: name, Mode: mode, Host: host, Port: port, Username: user, Password: pass
      };
      if (!appState.settings.Proxy.Profiles) appState.settings.Proxy.Profiles = [];
      appState.settings.Proxy.Profiles.push(prof);
      appState.settings.Proxy.ActiveProfileId = prof.Id;
      appState.settings.Proxy.Mode = mode;
      appState.settings.Proxy.Host = host;
      appState.settings.Proxy.Port = port;
      appState.settings.Proxy.Username = user;
      appState.settings.Proxy.Password = pass;
      document.getElementById("pxName").value = "";
      renderProxyProfiles();
    };

    document.getElementById("testProxyBtn").onclick = async () => {
      const el = document.getElementById("proxyTestResult");
      el.textContent = "Testing...";
      const r = await callHost("test_proxy", { proxy: appState.settings ? appState.settings.Proxy : {} });
      el.textContent = (r && r.ok) ? `${r.latencyMs}ms · ${r.loc} · ${r.ip}` : `Failed: ${(r && r.error) || "Timeout"}`;
    };

    document.getElementById("setBrowseBtn").onclick = async () => {
      const r = await callHost("pick_folder");
      if (r && r.folder) document.getElementById("setFolder").value = r.folder;
    };

    document.getElementById("setSaveBtn").onclick = async () => {
      if (!appState.settings) return;
      const s = appState.settings;
      s.DefaultSaveFolder = document.getElementById("setFolder").value.trim();
      theme = document.getElementById("setTheme").value || "auto";
      s.Theme = theme;
      s.DefaultConnections = parseInt(document.getElementById("setDefConns").value, 10) || 16;
      s.MaxConcurrentDownloads = parseInt(document.getElementById("setMaxConc").value, 10) || 3;
      s.GlobalSpeedLimitKBps = parseInt(document.getElementById("setSpeedCap").value, 10) || 0;
      s.AutoCategorizeFolders = document.getElementById("setAutoCat").checked;
      s.ClipboardMonitor = document.getElementById("setClipMon").checked;
      s.MinimizeToTray = document.getElementById("setTray").checked;
      s.ReduceMotion = document.getElementById("setReduceMotion").checked;
      s.ReduceTransparency = document.getElementById("setReduceTrans").checked;

      s.AutoResumeOnReconnect = document.getElementById("setAutoResumeNet").checked;
      s.Doh.Enabled = document.getElementById("dohEnabled").checked;
      s.Doh.Endpoint = document.getElementById("dohProvider").value;
      s.StallTimeoutSeconds = parseInt(document.getElementById("irStallSec").value, 10) || 8;
      s.MinStallSpeedKBps = parseInt(document.getElementById("irMinSpeed").value, 10) || 4;
      s.MaxRetriesPerSegment = parseInt(document.getElementById("irMaxRetries").value, 10) || 24;

      s.Scheduler.NightModeEnabled = document.getElementById("schNightEnabled").checked;
      s.Scheduler.StartTime = document.getElementById("schStart").value;
      s.Scheduler.EndTime = document.getElementById("schEnd").value;
      s.Scheduler.PostAction = parseInt(document.getElementById("schPostAction").value, 10);

      applyTheme();
      applyAccessibilityClasses();
      await callHost("save_settings", { settings: s });
      closeSheet("settingsSheet");
    };

    // Drag & Drop URL
    const root = document.getElementById("app");
    root.addEventListener("dragover", (e) => { e.preventDefault(); root.classList.add("drag-over"); });
    root.addEventListener("dragleave", () => root.classList.remove("drag-over"));
    root.addEventListener("drop", (e) => {
      e.preventDefault(); root.classList.remove("drag-over");
      const url = e.dataTransfer.getData("text/uri-list") || e.dataTransfer.getData("text/plain");
      if (url && /^https?:\/\//i.test(url.trim())) openNewDownloadWithUrl(url.trim(), "", "");
    });

    // Global Paste & Shortcuts including Ctrl+V IDM-style Probe & Add (works in both FA and EN keyboard layouts)
    async function triggerClipboardAdd(fallbackText) {
      const fromEvent = extractFirstHttpUrl(fallbackText);
      if (fromEvent) {
        openNewDownloadWithUrl(fromEvent, "", "");
        return;
      }
      const r = await callHost("get_clipboard_url");
      const found = (r && r.url) || extractFirstHttpUrl(r && r.raw);
      if (found) openNewDownloadWithUrl(found, "", "");
      else showToast(lang === "fa" ? "کلیپ‌بورد" : "Clipboard", lang === "fa" ? "لینک معتبری در کلیپ‌بورد نیست" : "No valid HTTP/HTTPS URL in clipboard");
    }

    document.addEventListener("paste", (e) => {
      const tag = (document.activeElement && document.activeElement.tagName) || "";
      const isEditing = tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT";
      if (isEditing) return;
      const txt = e.clipboardData ? e.clipboardData.getData("text") : "";
      const u = extractFirstHttpUrl(txt);
      if (u) {
        e.preventDefault();
        openNewDownloadWithUrl(u, "", "");
      }
    });

    window.addEventListener("keydown", async (e) => {
      const tag = (document.activeElement && document.activeElement.tagName) || "";
      const isEditing = tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT";
      const code = e.code || "";
      const key = (e.key || "").toLowerCase();

      if (e.ctrlKey && (code === "KeyV" || key === "v") && !isEditing) {
        e.preventDefault();
        await triggerClipboardAdd("");
      } else if (e.ctrlKey && (code === "KeyN" || key === "n")) {
        e.preventDefault(); openNewDownloadWithUrl("", "", "");
      } else if (e.ctrlKey && (code === "KeyF" || key === "f")) {
        e.preventDefault(); orb.classList.add("expanded"); searchInput.focus();
      } else if (e.ctrlKey && (code === "Comma" || key === ",")) {
        e.preventDefault(); openSettingsTab("general");
      } else if (e.key === "Escape") {
        document.querySelectorAll(".glass-sheet.open").forEach((s) => closeSheet(s.id));
      }
    });

    window.addEventListener("resize", () => requestAnimationFrame(() => kickPillSpring(true)));
  }

  bindEvents();
  setupSheetDrag();
  applyLanguage();
  applyTheme();
  requestAnimationFrame(() => kickPillSpring(true));
  sendHost("get_state");
})();
