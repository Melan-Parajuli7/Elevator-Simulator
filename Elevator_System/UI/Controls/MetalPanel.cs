using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lift.UI.Controls
{
    /// <summary>Brushed-steel control panel that hosts the LED display and the buttons.</summary>
    public class MetalPanel : Panel
    {
        private Bitmap _cache;

        public MetalPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Width < 2 || Height < 2) return;

            if (_cache == null || _cache.Width != Width || _cache.Height != Height)
            {
                if (_cache != null) _cache.Dispose();
                _cache = BuildBackground(Width, Height);
            }

            e.Graphics.DrawImageUnscaled(_cache, 0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _cache != null)
            {
                _cache.Dispose();
                _cache = null;
            }
            base.Dispose(disposing);
        }

        private static Bitmap BuildBackground(int width, int height)
        {
            var bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                var rect = new Rectangle(0, 0, width, height);

                // base steel gradient (left to right)
                using (var brush = new LinearGradientBrush(rect, Color.Gray, Color.White, LinearGradientMode.Horizontal))
                {
                    var blend = new ColorBlend(6);
                    blend.Positions = new float[] { 0f, 0.18f, 0.38f, 0.58f, 0.82f, 1f };
                    blend.Colors = new Color[]
                    {
                        Color.FromArgb(112, 112, 118),
                        Color.FromArgb(176, 176, 182),
                        Color.FromArgb(146, 146, 152),
                        Color.FromArgb(206, 206, 212),
                        Color.FromArgb(158, 158, 164),
                        Color.FromArgb(108, 108, 114)
                    };
                    brush.InterpolationColors = blend;
                    g.FillRectangle(brush, rect);
                }

                // brushed streaks
                var random = new Random(11);
                for (int x = 0; x < width; x++)
                {
                    if (random.Next(3) != 0) continue;
                    int alpha = random.Next(8, 30);
                    Color color = random.Next(2) == 0 ? Color.FromArgb(alpha, 255, 255, 255) : Color.FromArgb(alpha, 0, 0, 0);
                    using (var pen = new Pen(color))
                        g.DrawLine(pen, x, 0, x, height);
                }

                // bevelled border
                using (var dark = new Pen(Color.FromArgb(70, 70, 76), 3f))
                    g.DrawRectangle(dark, 1, 1, width - 3, height - 3);
                using (var light = new Pen(Color.FromArgb(200, 255, 255, 255), 1f))
                    g.DrawRectangle(light, 4, 4, width - 9, height - 9);
            }

            return bitmap;
        }
    }
}
