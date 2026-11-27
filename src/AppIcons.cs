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
