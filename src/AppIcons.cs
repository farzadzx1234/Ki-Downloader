using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace KiDownloader
{
    public static class AppIcons
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public static Bitmap RenderLogoBitmap(int size)
        {
            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                float pad = size * 0.039f;
                float boxSize = size - pad * 2f;
                float radius = boxSize * 0.228f;

                using (GraphicsPath sq = CreateRoundedRect(pad, pad, boxSize, boxSize, radius))
                {
                    g.SetClip(sq);

                    using (LinearGradientBrush bg = new LinearGradientBrush(
                        new PointF(pad + boxSize * 0.15f, pad),
                        new PointF(pad + boxSize * 0.85f, pad + boxSize),
                        Color.FromArgb(26, 140, 255),
                        Color.FromArgb(10, 30, 94)))
                    {
                        ColorBlend cb = new ColorBlend(3);
                        cb.Colors = new Color[] {
                            Color.FromArgb(26, 140, 255),
                            Color.FromArgb(0, 92, 230),
                            Color.FromArgb(10, 30, 94)
                        };
                        cb.Positions = new float[] { 0f, 0.52f, 1f };
                        bg.InterpolationColors = cb;
                        g.FillPath(bg, sq);
                    }

                    using (GraphicsPath glowPath = new GraphicsPath())
                    {
                        glowPath.AddEllipse(-size * 0.08f, -size * 0.18f, size * 0.86f, size * 0.72f);
                        using (PathGradientBrush pgb = new PathGradientBrush(glowPath))
                        {
                            pgb.CenterColor = Color.FromArgb(105, 128, 212, 255);
                            pgb.SurroundColors = new Color[] { Color.FromArgb(0, 0, 92, 230) };
                            g.FillPath(pgb, glowPath);
                        }
                    }

                    float inset = Math.Max(1f, size * 0.008f);
                    using (GraphicsPath rim = CreateRoundedRect(pad + inset, pad + inset, boxSize - inset * 2f, boxSize - inset * 2f, radius - inset))
                    using (Pen rimPen = new Pen(Color.FromArgb(62, 255, 255, 255), Math.Max(1f, size * 0.01f)))
                    {
                        g.DrawPath(rimPen, rim);
                    }

                    float s = size / 512f;

                    // Translucent depth chevron behind main arrow
                    using (Pen glassWing = new Pen(Color.FromArgb(110, 175, 232, 255), 54f * s))
                    {
                        glassWing.StartCap = LineCap.Round;
                        glassWing.EndCap = LineCap.Round;
                        glassWing.LineJoin = LineJoin.Round;
                        g.DrawLines(glassWing, new PointF[] {
                            new PointF(186f * s, 210f * s),
                            new PointF(256f * s, 278f * s),
                            new PointF(326f * s, 210f * s)
                        });
                    }

                    // Primary crisp minimalist arrow
                    using (Pen arrowPen = new Pen(Color.FromArgb(250, 255, 255, 255), 60f * s))
                    {
                        arrowPen.StartCap = LineCap.Round;
                        arrowPen.EndCap = LineCap.Round;
                        arrowPen.LineJoin = LineJoin.Round;
                        g.DrawLine(arrowPen, 256f * s, 148f * s, 256f * s, 344f * s);
                        g.DrawLines(arrowPen, new PointF[] {
                            new PointF(166f * s, 268f * s),
                            new PointF(256f * s, 358f * s),
                            new PointF(346f * s, 268f * s)
                        });
                    }

                    g.ResetClip();
                }
            }
            return bmp;
        }

        public static Bitmap RenderTrayBitmap(int size)
        {
            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                float pad = Math.Max(1f, size * 0.04f);
                float boxSize = size - pad * 2f;
                float radius = boxSize * 0.28f;

                using (GraphicsPath sq = CreateRoundedRect(pad, pad, boxSize, boxSize, radius))
                {
                    g.SetClip(sq);

                    using (LinearGradientBrush bg = new LinearGradientBrush(
                        new PointF(pad, pad),
                        new PointF(pad + boxSize, pad + boxSize),
                        Color.FromArgb(40, 185, 255),
                        Color.FromArgb(11, 36, 108)))
                    {
                        ColorBlend cb = new ColorBlend(3);
                        cb.Colors = new Color[] {
                            Color.FromArgb(56, 189, 248),
                            Color.FromArgb(0, 102, 255),
                            Color.FromArgb(11, 36, 108)
                        };
                        cb.Positions = new float[] { 0f, 0.5f, 1f };
                        bg.InterpolationColors = cb;
                        g.FillPath(bg, sq);
                    }

                    // Top specular liquid-glass sheen
                    using (GraphicsPath sheen = new GraphicsPath())
                    {
                        sheen.AddEllipse(pad - boxSize * 0.1f, pad - boxSize * 0.35f, boxSize * 1.2f, boxSize * 0.78f);
                        using (LinearGradientBrush sheenBrush = new LinearGradientBrush(
                            new PointF(0, pad),
                            new PointF(0, pad + boxSize * 0.45f),
                            Color.FromArgb(115, 255, 255, 255),
                            Color.FromArgb(0, 255, 255, 255)))
                        {
                            g.FillPath(sheenBrush, sheen);
                        }
                    }

                    // Crisp glass inner rim
                    float inset = Math.Max(0.8f, size * 0.025f);
                    using (GraphicsPath rim = CreateRoundedRect(pad + inset, pad + inset, boxSize - inset * 2f, boxSize - inset * 2f, Math.Max(1f, radius - inset)))
                    using (Pen rimPen = new Pen(Color.FromArgb(110, 255, 255, 255), Math.Max(1f, size * 0.032f)))
                    {
                        g.DrawPath(rimPen, rim);
                    }

                    float s = size / 32f;
                    // Bold crisp download arrow
                    using (Pen arrowPen = new Pen(Color.FromArgb(255, 255, 255, 255), 3.1f * s))
                    {
                        arrowPen.StartCap = LineCap.Round;
                        arrowPen.EndCap = LineCap.Round;
                        arrowPen.LineJoin = LineJoin.Round;
                        g.DrawLine(arrowPen, 16f * s, 7.2f * s, 16f * s, 19.2f * s);
                        g.DrawLines(arrowPen, new PointF[] {
                            new PointF(10.5f * s, 14.8f * s),
                            new PointF(16f * s, 20.2f * s),
                            new PointF(21.5f * s, 14.8f * s)
                        });
                    }

                    // Sleek dock bar beneath arrow
                    using (Pen dockPen = new Pen(Color.FromArgb(230, 186, 230, 253), 2.5f * s))
                    {
                        dockPen.StartCap = LineCap.Round;
                        dockPen.EndCap = LineCap.Round;
                        g.DrawLine(dockPen, 9.8f * s, 24.4f * s, 22.2f * s, 24.4f * s);
                    }

                    g.ResetClip();
                }
            }
            return bmp;
        }

        public static Icon CreateTrayIcon(int size)
        {
            using (Bitmap bmp = RenderTrayBitmap(size))
            {
                IntPtr hIcon = bmp.GetHicon();
                try
                {
                    using (Icon temp = Icon.FromHandle(hIcon))
                    {
                        return (Icon)temp.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }

        public static Bitmap CreateMenuGlyph(string kind, bool darkMode)
        {
            int sz = 16;
            Bitmap bmp = new Bitmap(sz, sz, PixelFormat.Format32bppArgb);
            Color fg = darkMode ? Color.FromArgb(235, 240, 248) : Color.FromArgb(30, 41, 59);
            Color accent = Color.FromArgb(10, 132, 255);
            Color danger = Color.FromArgb(239, 68, 68);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                if (kind == "open")
                {
                    using (Pen p = new Pen(accent, 1.7f))
                    {
                        p.StartCap = LineCap.Round;
                        p.EndCap = LineCap.Round;
                        p.LineJoin = LineJoin.Round;
                        g.DrawRectangle(p, 2.5f, 3.2f, 11f, 9.6f);
                        g.DrawLine(p, 2.5f, 6.2f, 13.5f, 6.2f);
                    }
                }
                else if (kind == "resume")
                {
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(16, 185, 129)))
                    {
                        g.FillPolygon(br, new PointF[] {
                            new PointF(4.2f, 2.8f),
                            new PointF(12.8f, 8f),
                            new PointF(4.2f, 13.2f)
                        });
                    }
                }
                else if (kind == "pause")
                {
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(245, 158, 11)))
                    {
                        g.FillRectangle(br, 3.8f, 3f, 3f, 10f);
                        g.FillRectangle(br, 9.2f, 3f, 3f, 10f);
                    }
                }
                else if (kind == "exit")
                {
                    using (Pen p = new Pen(danger, 1.8f))
                    {
                        p.StartCap = LineCap.Round;
                        p.EndCap = LineCap.Round;
                        g.DrawLine(p, 4f, 4f, 12f, 12f);
                        g.DrawLine(p, 12f, 4f, 4f, 12f);
                    }
                }
                else
                {
                    using (Pen p = new Pen(fg, 1.6f))
                    {
                        g.DrawEllipse(p, 3f, 3f, 10f, 10f);
                    }
                }
            }
            return bmp;
        }

        public static Icon CreateAppIcon(int size)
        {
            using (Bitmap bmp = RenderLogoBitmap(size))
            {
                IntPtr hIcon = bmp.GetHicon();
                try
                {
                    using (Icon temp = Icon.FromHandle(hIcon))
                    {
                        return (Icon)temp.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }

        public static void SaveAssets(string assetsDir)
        {
            if (!Directory.Exists(assetsDir)) Directory.CreateDirectory(assetsDir);
            using (Bitmap b256 = RenderLogoBitmap(256))
            {
                b256.Save(Path.Combine(assetsDir, "ki-logo.png"), ImageFormat.Png);
            }
            SaveMultiResIco(Path.Combine(assetsDir, "ki-downloader.ico"), new int[] { 256, 64, 48, 32, 16 });
        }

        private static void SaveMultiResIco(string path, int[] sizes)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write((short)0);
                bw.Write((short)1);
                bw.Write((short)sizes.Length);

                List<byte[]> pngChunks = new List<byte[]>();
                foreach (int sz in sizes)
                {
                    using (Bitmap bmp = RenderLogoBitmap(sz))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        pngChunks.Add(ms.ToArray());
                    }
                }

                int offset = 6 + (16 * sizes.Length);
                for (int i = 0; i < sizes.Length; i++)
                {
                    int sz = sizes[i];
                    byte[] data = pngChunks[i];
                    bw.Write((byte)(sz >= 256 ? 0 : sz));
                    bw.Write((byte)(sz >= 256 ? 0 : sz));
                    bw.Write((byte)0);
                    bw.Write((byte)0);
                    bw.Write((short)1);
                    bw.Write((short)32);
                    bw.Write(data.Length);
                    bw.Write(offset);
                    offset += data.Length;
                }

                for (int i = 0; i < pngChunks.Count; i++)
                {
                    bw.Write(pngChunks[i]);
                }
            }
        }

        private static GraphicsPath CreateRoundedRect(float x, float y, float w, float h, float r)
        {
            GraphicsPath path = new GraphicsPath();
            float d = r * 2f;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
