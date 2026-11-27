const KI_ENDPOINT = "http://127.0.0.1:45780";

chrome.runtime.onInstalled.addListener(() => {
  chrome.storage.local.get({ autoIntercept: true }, (res) => {
    chrome.storage.local.set({ autoIntercept: res.autoIntercept });
  });
  chrome.contextMenus.create({
    id: "send-to-ki",
    title: "Download with Ki-Downloader",
    contexts: ["link", "video", "audio", "image"]
  });
});

chrome.contextMenus.onClicked.addListener(async (info, tab) => {
  if (info.menuItemId !== "send-to-ki") return;
  const targetUrl = info.linkUrl || info.srcUrl;
  if (!targetUrl) return;
  await sendToKi(targetUrl, "", tab ? tab.url : "");
});

chrome.downloads.onCreated.addListener(async (item) => {
  if (!item || !item.url || !item.url.startsWith("http")) return;
  const cfg = await chrome.storage.local.get({ autoIntercept: true });
  if (!cfg.autoIntercept) return;

  try {
    const ping = await fetch(`${KI_ENDPOINT}/api/ping`);
    if (!ping.ok) return;
    await chrome.downloads.cancel(item.id);
    await chrome.downloads.erase({ id: item.id });
    await sendToKi(item.url, item.filename || "", item.referrer || "");
  } catch {
    // Ki-Downloader is not running; let browser continue normally
  }
});

async function sendToKi(url, fileName, referer) {
  try {
    await fetch(`${KI_ENDPOINT}/api/add`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ url, fileName, referer, silent: false })
    });
  } catch {}
}
