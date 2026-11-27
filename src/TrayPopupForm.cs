using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KiDownloader
{
    public sealed class TrayPopupForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        private readonly DownloadEngine _engine;
        private readonly bool _isDark;
        private readonly bool _isFa;
        private readonly Action _onOpenApp;
        private readonly Action _onNewDownload;
        private readonly Action _onExitApp;
        private readonly Bitmap _logoBmp;

        private readonly List<RowDef> _rows = new List<RowDef>();
        private int _hoverIndex = -1;
        private bool _headerHover;
        private Rectangle _headerRect;

        private sealed class RowDef
        {
            public string Id;
            public string Label;
            public string Shortcut;
            public string IconKind;
            public bool IsSeparator;
            public bool IsDanger;
            public bool IsToggle;
            public bool ToggleValue;
            public Rectangle Bounds;
            public Action OnClick;
        }

        public TrayPopupForm(
            DownloadEngine engine,
            bool isDark,
            Action onOpenApp,
            Action onNewDownload,
            Action onExitApp)
        {
            _engine = engine;
            _isDark = isDark;
            _isFa = engine != null && engine.Settings != null &&
                    string.Equals(engine.Settings.Language, "fa", StringComparison.OrdinalIgnoreCase);
            _onOpenApp = onOpenApp;
            _onNewDownload = onNewDownload;
            _onExitApp = onExitApp;
            _logoBmp = AppIcons.RenderTrayBitmap(36);

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            DoubleBuffered = true;
            BackColor = isDark ? Color.FromArgb(24, 25, 30) : Color.FromArgb(255, 255, 255);
            KeyPreview = true;

            BuildRows();
            LayoutCard();

            Deactivate += delegate { SafeClose(); };
            KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) SafeClose();
            };
            MouseMove += OnCardMouseMove;
            MouseLeave += delegate
            {
                if (_hoverIndex != -1 || _headerHover)
                {
                    _hoverIndex = -1;
                    _headerHover = false;
                    Invalidate();
                }
            };
            MouseUp += OnCardMouseUp;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                int dark = _isDark ? 1 : 0;
                DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
                int corner = 2; // DWMWCP_ROUND
                DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _logoBmp != null) _logoBmp.Dispose();
            base.Dispose(disposing);
        }

        public void ShowNearCursor()
        {
            Point cursor = Cursor.Position;
            Rectangle wa = Screen.FromPoint(cursor).WorkingArea;

            int x = cursor.X - (Width / 2);
            int y = cursor.Y - Height - 12;

            if (y < wa.Top + 8) y = wa.Bottom - Height - 10;
            if (y + Height > wa.Bottom - 8) y = wa.Bottom - Height - 10;
            if (x + Width > wa.Right - 10) x = wa.Right - Width - 10;
            if (x < wa.Left + 10) x = wa.Left + 10;

            Location = new Point(x, y);
            Show();
            Activate();
        }

        private void SafeClose()
        {
            try
            {
                if (!IsDisposed) Close();
            }
            catch { }
        }

        private void BuildRows()
        {
            _rows.Clear();
            bool clipOn = _engine != null && _engine.Settings != null && _engine.Settings.ClipboardMonitor;

            _rows.Add(new RowDef
            {
                Id = "new",
                Label = _isFa ? "دانلود جدید" : "New Download",
                IconKind = "plus",
                OnClick = delegate { SafeClose(); if (_onNewDownload != null) _onNewDownload(); }
            });
            _rows.Add(new RowDef
            {
                Id = "resume",
                Label = _isFa ? "ادامه همه" : "Resume All",
                IconKind = "play",
                OnClick = delegate { SafeClose(); if (_engine != null) _engine.ResumeAll(); }
            });
            _rows.Add(new RowDef
            {
                Id = "pause",
                Label = _isFa ? "توقف همه" : "Pause All",
                IconKind = "pause",
                OnClick = delegate { SafeClose(); if (_engine != null) _engine.PauseAll(); }
            });
            _rows.Add(new RowDef
            {
                Id = "clipboard",
                Label = _isFa ? "پایش کلیپ‌بورد" : "Clipboard Monitor",
                IconKind = "clip",
                IsToggle = true,
                ToggleValue = clipOn,
                OnClick = delegate
                {
                    if (_engine != null && _engine.Settings != null)
                    {
                        _engine.Settings.ClipboardMonitor = !_engine.Settings.ClipboardMonitor;
                        _engine.SaveState();
                        foreach (var r in _rows)
                        {
                            if (r.Id == "clipboard") r.ToggleValue = _engine.Settings.ClipboardMonitor;
                        }
                        Invalidate();
                    }
                }
            });
            _rows.Add(new RowDef
            {
                Id = "folder",
                Label = _isFa ? "پوشه دانلودها" : "Downloads Folder",
                IconKind = "folder",
                OnClick = delegate
                {
                    SafeClose();
                    try
                    {
                        string dir = (_engine != null && _engine.Settings != null) ? _engine.Settings.DefaultSaveFolder : "";
                        if (!string.IsNullOrEmpty(dir))
                        {
                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                            Process.Start("explorer.exe", "\"" + dir + "\"");
                        }
                    }
                    catch { }
                }
            });
            _rows.Add(new RowDef { IsSeparator = true });
            _rows.Add(new RowDef
            {
                Id = "exit",
                Label = _isFa ? "خروج" : "Exit",
                IconKind = "power",
                IsDanger = true,
                OnClick = delegate { SafeClose(); if (_onExitApp != null) _onExitApp(); }
            });
        }

        private void LayoutCard()
        {
            int w = 276;
            int y = 8;
            _headerRect = new Rectangle(8, y, w - 16, 58);
            y += 64;

            // Top divider space
            y += 8;

            foreach (RowDef r in _rows)
            {
                if (r.IsSeparator)
                {
                    r.Bounds = new Rectangle(14, y + 4, w - 28, 1);
                    y += 10;
                }
                else
                {
                    r.Bounds = new Rectangle(8, y, w - 16, 38);
                    y += 40;
                }
            }
            y += 6;
            ClientSize = new Size(w, y);
        }

        private void OnCardMouseMove(object sender, MouseEventArgs e)
        {
            bool newHeader = _headerRect.Contains(e.Location);
            int newHover = -1;
            for (int i = 0; i < _rows.Count; i++)
            {
                if (!_rows[i].IsSeparator && _rows[i].Bounds.Contains(e.Location))
                {
                    newHover = i;
                    break;
                }
            }

            if (newHeader != _headerHover || newHover != _hoverIndex)
            {
                _headerHover = newHeader;
                _hoverIndex = newHover;
                Cursor = (_headerHover || _hoverIndex >= 0) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        private void OnCardMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (_headerRect.Contains(e.Location))
            {
                SafeClose();
                if (_onOpenApp != null) _onOpenApp();
                return;
            }
            for (int i = 0; i < _rows.Count; i++)
            {
                if (!_rows[i].IsSeparator && _rows[i].Bounds.Contains(e.Location))
                {
                    if (_rows[i].OnClick != null) _rows[i].OnClick();
                    return;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color bg = _isDark ? Color.FromArgb(24, 25, 30) : Color.FromArgb(255, 255, 255);
            Color border = _isDark ? Color.FromArgb(46, 48, 57) : Color.FromArgb(220, 226, 234);
            Color hoverBg = _isDark ? Color.FromArgb(37, 39, 48) : Color.FromArgb(241, 245, 249);
            Color textPrimary = _isDark ? Color.FromArgb(244, 244, 245) : Color.FromArgb(15, 23, 42);
            Color textMuted = _isDark ? Color.FromArgb(161, 161, 170) : Color.FromArgb(100, 116, 139);
            Color dividerColor = _isDark ? Color.FromArgb(42, 44, 52) : Color.FromArgb(226, 232, 240);
            Color dangerColor = _isDark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38);

            g.Clear(bg);

            // Outer 1px border
            using (Pen bp = new Pen(border, 1f))
            {
                g.DrawRectangle(bp, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }

            // Header card
            if (_headerHover)
            {
                using (GraphicsPath hp = RoundedRect(_headerRect, 10))
                using (SolidBrush hb = new SolidBrush(hoverBg))
                {
                    g.FillPath(hb, hp);
                }
            }

            int logoSize = 36;
            int logoX = _isFa ? (_headerRect.Right - logoSize - 10) : (_headerRect.Left + 10);
            int logoY = _headerRect.Top + (_headerRect.Height - logoSize) / 2;
            if (_logoBmp != null)
            {
                g.DrawImage(_logoBmp, logoX, logoY, logoSize, logoSize);
            }

            // Status summary
            int activeCount = 0;
            double speedBps = 0;
            if (_engine != null && _engine.Downloads != null)
            {
                foreach (var d in _engine.Downloads)
                {
                    if (d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Probing)
                    {
                        activeCount++;
                        speedBps += d.SpeedBps;
                    }
                }
            }

            Color dotColor = activeCount > 0 ? Color.FromArgb(34, 197, 94) : Color.FromArgb(56, 189, 248);
            string statusText;
            if (activeCount > 0)
            {
                statusText = _isFa
                    ? string.Format("{0} دانلود فعال • {1}", activeCount, FormatSpeed(speedBps))
                    : string.Format("{0} active • {1}", activeCount, FormatSpeed(speedBps));
            }
            else
            {
                statusText = _isFa
                    ? string.Format("آماده به کار • v{0}", DownloadEngine.AppVersion)
                    : string.Format("Ready • v{0}", DownloadEngine.AppVersion);
            }

            using (Font titleFont = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            using (Font subFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (SolidBrush titleBrush = new SolidBrush(textPrimary))
            using (SolidBrush subBrush = new SolidBrush(textMuted))
            using (SolidBrush dotBrush = new SolidBrush(dotColor))
            {
                if (_isFa)
                {
                    Rectangle titleRect = new Rectangle(_headerRect.Left + 28, _headerRect.Top + 9, _headerRect.Width - 84, 22);
                    StringFormat sfR = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                    g.DrawString("Ki-Downloader", titleFont, titleBrush, titleRect, sfR);

                    SizeF subSize = g.MeasureString(statusText, subFont);
                    float dotX = logoX - 18;
                    float dotY = _headerRect.Top + 37;
                    g.FillEllipse(dotBrush, dotX, dotY, 7f, 7f);

                    RectangleF subRect = new RectangleF(_headerRect.Left + 28, _headerRect.Top + 31, (dotX - 6) - (_headerRect.Left + 28), 18);
                    g.DrawString(statusText, subFont, subBrush, subRect, sfR);

                    DrawChevron(g, _headerRect.Left + 14, _headerRect.Top + _headerRect.Height / 2f, true, textMuted);
                }
                else
                {
                    Rectangle titleRect = new Rectangle(logoX + logoSize + 12, _headerRect.Top + 9, _headerRect.Width - 84, 22);
                    StringFormat sfL = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                    g.DrawString("Ki-Downloader", titleFont, titleBrush, titleRect, sfL);

                    float dotX = logoX + logoSize + 13;
                    float dotY = _headerRect.Top + 37;
                    g.FillEllipse(dotBrush, dotX, dotY, 7f, 7f);

                    RectangleF subRect = new RectangleF(dotX + 12, _headerRect.Top + 31, _headerRect.Right - 28 - (dotX + 12), 18);
                    g.DrawString(statusText, subFont, subBrush, subRect, sfL);

                    DrawChevron(g, _headerRect.Right - 14, _headerRect.Top + _headerRect.Height / 2f, false, textMuted);
                }
            }

            // Header bottom divider
            using (Pen dp = new Pen(dividerColor, 1f))
            {
                g.DrawLine(dp, 14, _headerRect.Bottom + 5, ClientSize.Width - 14, _headerRect.Bottom + 5);
            }

            // Menu rows
            using (Font rowFont = new Font("Segoe UI", 9.5f, FontStyle.Regular))
            {
                for (int i = 0; i < _rows.Count; i++)
                {
                    RowDef r = _rows[i];
                    if (r.IsSeparator)
                    {
                        using (Pen dp = new Pen(dividerColor, 1f))
                        {
                            g.DrawLine(dp, r.Bounds.Left, r.Bounds.Top, r.Bounds.Right, r.Bounds.Top);
                        }
                        continue;
                    }

                    if (i == _hoverIndex)
                    {
                        using (GraphicsPath rp = RoundedRect(r.Bounds, 8))
                        using (SolidBrush hb = new SolidBrush(hoverBg))
                        {
                            g.FillPath(hb, rp);
                        }
                    }

                    Color fg = r.IsDanger ? dangerColor : textPrimary;
                    Color iconCol = r.IsDanger ? dangerColor : textMuted;

                    int iconX = _isFa ? (r.Bounds.Right - 28) : (r.Bounds.Left + 12);
                    int iconY = r.Bounds.Top + (r.Bounds.Height - 16) / 2;
                    DrawOutlineIcon(g, r.IconKind, new Rectangle(iconX, iconY, 16, 16), iconCol);

                    using (SolidBrush tb = new SolidBrush(fg))
                    {
                        if (_isFa)
                        {
                            Rectangle textRect = new Rectangle(r.Bounds.Left + (r.IsToggle ? 54 : 12), r.Bounds.Top, r.Bounds.Width - (r.IsToggle ? 92 : 50), r.Bounds.Height);
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                            g.DrawString(r.Label, rowFont, tb, textRect, sf);
                        }
                        else
                        {
                            Rectangle textRect = new Rectangle(r.Bounds.Left + 38, r.Bounds.Top, r.Bounds.Width - (r.IsToggle ? 92 : 50), r.Bounds.Height);
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                            g.DrawString(r.Label, rowFont, tb, textRect, sf);
                        }
                    }

                    if (r.IsToggle)
                    {
                        int swW = 36, swH = 20;
                        int swX = _isFa ? (r.Bounds.Left + 12) : (r.Bounds.Right - swW - 12);
                        int swY = r.Bounds.Top + (r.Bounds.Height - swH) / 2;
                        DrawToggleSwitch(g, new Rectangle(swX, swY, swW, swH), r.ToggleValue, _isDark);
                    }
                }
            }
        }

        private static void DrawChevron(Graphics g, float cx, float cy, bool pointLeft, Color color)
        {
            using (Pen p = new Pen(color, 1.5f))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                p.LineJoin = LineJoin.Round;
                float dx = pointLeft ? -3.5f : 3.5f;
                g.DrawLines(p, new PointF[] {
                    new PointF(cx - dx, cy - 4.5f),
                    new PointF(cx + dx, cy),
                    new PointF(cx - dx, cy + 4.5f)
                });
            }
        }

        private static void DrawToggleSwitch(Graphics g, Rectangle rect, bool on, bool isDark)
        {
            Color track = on
                ? Color.FromArgb(10, 132, 255)
                : (isDark ? Color.FromArgb(63, 63, 70) : Color.FromArgb(203, 213, 225));
            using (GraphicsPath path = RoundedRect(rect, rect.Height / 2))
            using (SolidBrush br = new SolidBrush(track))
            {
                g.FillPath(br, path);
            }
            int knob = rect.Height - 4;
            int kx = on ? (rect.Right - knob - 2) : (rect.Left + 2);
            using (SolidBrush kb = new SolidBrush(Color.White))
            {
                g.FillEllipse(kb, kx, rect.Top + 2, knob, knob);
            }
        }

        private static void DrawOutlineIcon(Graphics g, string kind, Rectangle b, Color c)
        {
            using (Pen p = new Pen(c, 1.5f))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                p.LineJoin = LineJoin.Round;

                float x = b.X, y = b.Y, w = b.Width, h = b.Height;
                if (kind == "plus")
                {
                    g.DrawEllipse(p, x + 1.5f, y + 1.5f, w - 3f, h - 3f);
                    g.DrawLine(p, x + w / 2f, y + 4.8f, x + w / 2f, y + h - 4.8f);
                    g.DrawLine(p, x + 4.8f, y + h / 2f, x + w - 4.8f, y + h / 2f);
                }
                else if (kind == "play")
                {
                    g.DrawEllipse(p, x + 1.5f, y + 1.5f, w - 3f, h - 3f);
                    g.DrawPolygon(p, new PointF[] {
                        new PointF(x + 6.4f, y + 5f),
                        new PointF(x + 11.4f, y + 8f),
                        new PointF(x + 6.4f, y + 11f)
                    });
                }
                else if (kind == "pause")
                {
                    g.DrawEllipse(p, x + 1.5f, y + 1.5f, w - 3f, h - 3f);
                    g.DrawLine(p, x + 6.4f, y + 5.2f, x + 6.4f, y + 10.8f);
                    g.DrawLine(p, x + 9.6f, y + 5.2f, x + 9.6f, y + 10.8f);
                }
                else if (kind == "clip")
                {
                    g.DrawRoundedRectCompat(p, x + 3f, y + 2.5f, w - 6f, h - 4f, 2f);
                    g.DrawLine(p, x + 5.5f, y + 1.5f, x + w - 5.5f, y + 1.5f);
                    g.DrawLine(p, x + 5.5f, y + 7f, x + w - 5.5f, y + 7f);
                    g.DrawLine(p, x + 5.5f, y + 10f, x + w - 7f, y + 10f);
                }
                else if (kind == "folder")
                {
                    g.DrawLines(p, new PointF[] {
                        new PointF(x + 1.8f, y + 3.5f),
                        new PointF(x + 6.2f, y + 3.5f),
                        new PointF(x + 7.6f, y + 5.2f),
                        new PointF(x + 14.2f, y + 5.2f),
                        new PointF(x + 14.2f, y + 12.8f),
                        new PointF(x + 1.8f, y + 12.8f),
                        new PointF(x + 1.8f, y + 3.5f)
                    });
                }
                else if (kind == "power")
                {
                    g.DrawArc(p, x + 2.2f, y + 2.6f, w - 4.4f, h - 4.4f, -55, 290);
                    g.DrawLine(p, x + w / 2f, y + 1.5f, x + w / 2f, y + 7.2f);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = Math.Max(2, radius * 2);
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static string FormatSpeed(double bps)
        {
            if (bps < 1024) return ((int)bps) + " B/s";
            if (bps < 1024 * 1024) return (bps / 1024.0).ToString("0.0") + " KB/s";
            return (bps / (1024.0 * 1024.0)).ToString("0.0") + " MB/s";
        }
    }

    internal static class GraphicsExt
    {
        public static void DrawRoundedRectCompat(this Graphics g, Pen p, float x, float y, float w, float h, float r)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                float d = r * 2f;
                path.AddArc(x, y, d, d, 180, 90);
                path.AddArc(x + w - d, y, d, d, 270, 90);
                path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
                path.AddArc(x, y + h - d, d, d, 90, 90);
                path.CloseFigure();
                g.DrawPath(p, path);
            }
        }
    }
}
