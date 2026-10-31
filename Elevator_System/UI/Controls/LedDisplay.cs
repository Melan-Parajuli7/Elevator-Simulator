using System;
using Lift.Models;
using Lift.Observer;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Lift.UI.Controls
{
    /// <summary>Black LED screen: floor number, moving arrows and a status message.</summary>
    public class LedDisplay : Control
    {
        private string _floorText = "G";
        private string _statusMessage = "";
        private Direction _direction = Direction.None;
        private bool _moving;
        private bool _emergency;

        public LedDisplay()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(170, 300);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FloorText
        {
            get { return _floorText; }
            set { if (_floorText != value) { _floorText = value; Invalidate(); } }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { if (_statusMessage != value) { _statusMessage = value; Invalidate(); } }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Direction Direction
        {
            get { return _direction; }
            set { if (_direction != value) { _direction = value; Invalidate(); } }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Moving
        {
            get { return _moving; }
            set { if (_moving != value) { _moving = value; Invalidate(); } }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Emergency
        {
            get { return _emergency; }
            set { if (_emergency != value) { _emergency = value; Invalidate(); } }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            var outer = new RectangleF(1, 1, Width - 3, Height - 3);
            var screen = RectangleF.Inflate(outer, -5, -5);
            bool flashOn = _emergency && Gfx.Ticks(350) % 2 == 0;

            // bezel
            using (GraphicsPath path = Gfx.RoundedRect(outer, 7))
            using (var brush = new LinearGradientBrush(outer, Color.FromArgb(90, 90, 96), Color.FromArgb(20, 20, 22), 90f))
            {
                g.FillPath(brush, path);
                using (var pen = new Pen(Color.FromArgb(30, 30, 32), 1.5f)) g.DrawPath(pen, path);
            }

            // screen
            using (GraphicsPath path = Gfx.RoundedRect(screen, 4))
            using (var brush = new SolidBrush(flashOn ? Color.FromArgb(75, 0, 0) : Color.Black))
                g.FillPath(brush, path);

            var iconRect = new RectangleF(screen.X, screen.Y + 8, screen.Width, screen.Height * 0.52f);
            var messageRect = new RectangleF(screen.X + 6, iconRect.Bottom + 2, screen.Width - 12, screen.Bottom - iconRect.Bottom - 8);

            if (_emergency)
                DrawEmergency(g, iconRect);
            else if (_moving)
                DrawChevrons(g, iconRect);
            else
                DrawFloor(g, iconRect);

            // divider
            using (var pen = new Pen(Color.FromArgb(90, 160, 0, 0), 1f))
                g.DrawLine(pen, screen.X + 12, iconRect.Bottom, screen.Right - 12, iconRect.Bottom);

            DrawMessage(g, messageRect);

            // glass reflection
            var gloss = new RectangleF(screen.X, screen.Y, screen.Width, screen.Height * 0.32f);
            using (GraphicsPath path = Gfx.RoundedRect(gloss, 4))
            using (var brush = new LinearGradientBrush(gloss, Color.FromArgb(38, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
                g.FillPath(brush, path);
        }

        private void DrawFloor(Graphics g, RectangleF rect)
        {
            float size = rect.Height * 0.62f;
            float starSize = size * 0.7f;

            using (var font = new Font("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                SizeF text = g.MeasureString(_floorText, font);
                float labelWidth = text.Width * 0.75f;
                float totalWidth = starSize + 10 + labelWidth;
                float startX = rect.X + (rect.Width - totalWidth) / 2f;

                DrawStar(g, new PointF(startX + starSize / 2f, rect.Y + rect.Height / 2f), starSize / 2f);

                var textRect = new RectangleF(startX + starSize + 10 - (text.Width - labelWidth) / 2f, rect.Y, text.Width, rect.Height);
                using (var shadow = new SolidBrush(Color.FromArgb(90, 120, 0, 0)))
                    Gfx.DrawCentered(g, _floorText, font, shadow, Gfx.Offset(textRect, 2, 2));
                using (var brush = new SolidBrush(Color.FromArgb(255, 45, 45)))
                    Gfx.DrawCentered(g, _floorText, font, brush, textRect);
            }
        }

        private void DrawStar(Graphics g, PointF center, float radius)
        {
            var points = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI / 5;
                float r = i % 2 == 0 ? radius : radius * 0.42f;
                points[i] = new PointF(center.X + (float)(Math.Cos(angle) * r), center.Y + (float)(Math.Sin(angle) * r));
            }

            using (var brush = new SolidBrush(Color.FromArgb(235, 30, 35)))
                g.FillPolygon(brush, points);
        }

        private void DrawChevrons(Graphics g, RectangleF rect)
        {
            int phase = Gfx.Ticks(220) % 3;
            bool up = _direction != Direction.Down;
            int litIndex = up ? 2 - phase : phase;

            float width = rect.Width * 0.46f;
            float height = rect.Height * 0.20f;
            float gap = height * 0.55f;
            float total = 3 * height + 2 * gap;
            float startY = rect.Y + (rect.Height - total) / 2f;
            float centerX = rect.X + rect.Width / 2f;

            for (int i = 0; i < 3; i++)
            {
                float y = startY + i * (height + gap);
                int alpha = i == litIndex ? 255 : 70;

                PointF[] points = up
                    ? new PointF[] { new PointF(centerX - width / 2, y + height), new PointF(centerX, y), new PointF(centerX + width / 2, y + height) }
                    : new PointF[] { new PointF(centerX - width / 2, y), new PointF(centerX, y + height), new PointF(centerX + width / 2, y) };

                using (var pen = new Pen(Color.FromArgb(alpha, 255, 40, 40), Math.Max(3f, height * 0.45f)))
                {
                    pen.LineJoin = LineJoin.Round;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLines(pen, points);
                }
            }
        }

        private void DrawEmergency(Graphics g, RectangleF rect)
        {
            using (var font = new Font("Segoe UI", rect.Height * 0.42f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.FromArgb(255, 60, 60)))
                Gfx.DrawCentered(g, "SOS", font, brush, rect, true);
        }

        private void DrawMessage(Graphics g, RectangleF rect)
        {
            if (string.IsNullOrEmpty(_statusMessage)) return;

            float size = Math.Max(11f, Width * 0.092f);
            using (var font = new Font("Consolas", size, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                // soft glow
                using (var glow = new SolidBrush(Color.FromArgb(45, 255, 40, 40)))
                {
                    Gfx.DrawCentered(g, _statusMessage, font, glow, Gfx.Offset(rect, 1.5f, 0));
                    Gfx.DrawCentered(g, _statusMessage, font, glow, Gfx.Offset(rect, -1.5f, 0));
                    Gfx.DrawCentered(g, _statusMessage, font, glow, Gfx.Offset(rect, 0, 1.5f));
                    Gfx.DrawCentered(g, _statusMessage, font, glow, Gfx.Offset(rect, 0, -1.5f));
                }

                using (var brush = new SolidBrush(Color.FromArgb(255, 70, 70)))
                    Gfx.DrawCentered(g, _statusMessage, font, brush, rect);
            }
        }
    }
}
