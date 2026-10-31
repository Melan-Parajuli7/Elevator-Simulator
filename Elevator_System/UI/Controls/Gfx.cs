using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Lift.UI.Controls
{
    /// <summary>Small drawing helpers shared by the custom controls.</summary>
    public static class Gfx
    {
        /// <summary>Counter that increases every <paramref name="ms"/> milliseconds (used for blinking/animation).</summary>
        public static int Ticks(int ms)
        {
            return (int)((Environment.TickCount & 0x7FFFFFFF) / ms);
        }

        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
            if (d <= 1f)
            {
                path.AddRectangle(r);
                return path;
            }

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Brushed-steel gradient (light and dark bands).</summary>
        public static LinearGradientBrush MetalBrush(RectangleF r, LinearGradientMode mode)
        {
            if (r.Width < 1f) r.Width = 1f;
            if (r.Height < 1f) r.Height = 1f;

            var brush = new LinearGradientBrush(r, Color.Gray, Color.White, mode);
            var blend = new ColorBlend(5);
            blend.Positions = new float[] { 0f, 0.22f, 0.5f, 0.78f, 1f };
            blend.Colors = new Color[]
            {
                Color.FromArgb(120, 120, 126),
                Color.FromArgb(206, 206, 211),
                Color.FromArgb(160, 160, 166),
                Color.FromArgb(226, 226, 231),
                Color.FromArgb(132, 132, 138)
            };
            brush.InterpolationColors = blend;
            return brush;
        }

        public static void DrawCentered(Graphics g, string text, Font font, Brush brush, RectangleF rect, bool noWrap = false)
        {
            using (var format = new StringFormat())
            {
                if (noWrap) format.FormatFlags = StringFormatFlags.NoWrap;
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(text, font, brush, rect, format);
            }
        }

        /// <summary>Returns a copy of the rectangle moved by (dx, dy).</summary>
        public static RectangleF Offset(RectangleF r, float dx, float dy)
        {
            return new RectangleF(r.X + dx, r.Y + dy, r.Width, r.Height);
        }

        /// <summary>Draws a simple person standing on (footX, footY). walkPhase swings the legs and arms.</summary>
        public static void DrawPerson(Graphics g, float footX, float footY, float height, Color shirt, double walkPhase)
        {
            if (height < 8f) return;

            float swing = (float)Math.Sin(walkPhase) * height * 0.07f;
            float hipY = footY - height * 0.38f;
            float shoulderY = footY - height * 0.74f;
            float headRadius = height * 0.12f;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // shadow on the floor
            using (var shadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                g.FillEllipse(shadow, footX - height * 0.22f, footY - height * 0.03f, height * 0.44f, height * 0.07f);

            // legs
            using (var pen = new Pen(Color.FromArgb(45, 50, 68), Math.Max(2f, height * 0.08f)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, footX - height * 0.05f, hipY, footX - height * 0.07f + swing, footY);
                g.DrawLine(pen, footX + height * 0.05f, hipY, footX + height * 0.07f - swing, footY);
            }

            // arms
            Color sleeve = Color.FromArgb(Math.Max(0, shirt.R - 25), Math.Max(0, shirt.G - 25), Math.Max(0, shirt.B - 25));
            using (var pen = new Pen(sleeve, Math.Max(2f, height * 0.065f)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, footX - height * 0.15f, shoulderY + height * 0.05f, footX - height * 0.17f - swing * 0.7f, hipY + height * 0.02f);
                g.DrawLine(pen, footX + height * 0.15f, shoulderY + height * 0.05f, footX + height * 0.17f + swing * 0.7f, hipY + height * 0.02f);
            }

            // torso
            var torso = new RectangleF(footX - height * 0.14f, shoulderY, height * 0.28f, hipY - shoulderY + height * 0.03f);
            using (GraphicsPath path = RoundedRect(torso, height * 0.06f))
            using (var brush = new SolidBrush(shirt))
                g.FillPath(brush, path);

            // head and hair
            var head = new RectangleF(footX - headRadius, shoulderY - headRadius * 2.15f, headRadius * 2f, headRadius * 2f);
            using (var skin = new SolidBrush(Color.FromArgb(240, 196, 160)))
                g.FillEllipse(skin, head);
            using (var hair = new SolidBrush(Color.FromArgb(55, 40, 30)))
                g.FillPie(hair, head.X - 0.5f, head.Y - 0.5f, head.Width + 1f, head.Height + 1f, 180, 180);

            g.SmoothingMode = previous;
        }

        /// <summary>Shirt colour of a passenger: 0 = blue, 1 = orange, others = green.</summary>
        public static Color PassengerColor(int index)
        {
            switch (index)
            {
                case 0: return Color.FromArgb(45, 110, 215);
                case 1: return Color.FromArgb(235, 130, 35);
                default: return Color.FromArgb(60, 160, 90);
            }
        }

        public static float Lerp(float from, float to, double t)
        {
            return (float)(from + (to - from) * t);
        }

        public static Color Lighten(Color c, int amount)
        {
            return Color.FromArgb(
                c.A,
                Math.Min(255, c.R + amount),
                Math.Min(255, c.G + amount),
                Math.Min(255, c.B + amount));
        }
    }
}
