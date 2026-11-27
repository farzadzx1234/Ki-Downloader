const KI = "http://127.0.0.1:45780";
const statusEl = document.getElementById("status");
const autoToggle = document.getElementById("autoToggle");

fetch(`${KI}/api/ping`)
  .then((r) => {
    if (r.ok) {
      statusEl.textContent = "Connected to Ki-Downloader";
      statusEl.classList.remove("off");
    }
  })
  .catch(() => { statusEl.textContent = "App Not Running"; });

chrome.storage.local.get({ autoIntercept: true }, (res) => {
  autoToggle.checked = !!res.autoIntercept;
});
autoToggle.onchange = () => {
  chrome.storage.local.set({ autoIntercept: autoToggle.checked });
};

document.getElementById("openBtn").onclick = () => {
  fetch(`${KI}/api/focus`).catch(() => {});
};

document.getElementById("grabBtn").onclick = async () => {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  if (!tab || !tab.id) return;
  const results = await chrome.scripting.executeScript({
    target: { tabId: tab.id },
    func: () => {
      const exts = /\.(zip|rar|7z|tar|gz|iso|exe|msi|apk|mp4|mkv|webm|mp3|flac|pdf)(\?.*)?$/i;
      const set = new Set();
      document.querySelectorAll("a[href], video source[src], audio source[src]").forEach((el) => {
        const u = el.href || el.src;
        if (u && u.startsWith("http") && exts.test(u)) set.add(u);
      });
      return Array.from(set);
    }
  });
  const links = (results && results[0] && results[0].result) || [];
  for (const url of links.slice(0, 25)) {
    await fetch(`${KI}/api/add`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ url, referer: tab.url || "", silent: true })
    }).catch(() => {});
  }
  statusEl.textContent = links.length ? `Sent ${Math.min(links.length, 25)} links!` : "No media/archive links found";
};
