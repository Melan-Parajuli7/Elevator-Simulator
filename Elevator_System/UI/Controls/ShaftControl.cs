using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Lift.UI.Controls
{
    /// <summary>
    /// The lift shaft seen from the side: a metal car (the "container") hangs on a cable and slides up and down
    /// between the Ground and the First floor level. A passenger standing in the car is visible through its window.
    /// The car sits exactly at the position the lift reports, so it stops in the same instant the lift stops.
    /// </summary>
    public class ShaftControl : Control
    {
        private const float CarHeight = 124f;

        private double _position = 1.0;
        private bool _moving;
        private bool _emergency;
        private int _passengerColor = -1;
        private int _firstLevelY = 175;
        private int _groundLevelY = 525;

        public ShaftControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Size = new Size(80, 672);
        }

        /// <summary>1.0 = Ground floor, 2.0 = First floor, in between while travelling.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Position
        {
            get { return _position; }
            set
            {
                value = Math.Max(1.0, Math.Min(2.0, value));
                if (Math.Abs(_position - value) > 0.00001) { _position = value; Invalidate(); }
            }
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

        /// <summary>Colour index of the passenger inside the car, or -1 when the car is empty.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PassengerColor
        {
            get { return _passengerColor; }
            set { if (_passengerColor != value) { _passengerColor = value; Invalidate(); } }
        }

        /// <summary>Height (inside this control) of the middle of the First floor door.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int FirstLevelY
        {
            get { return _firstLevelY; }
            set { _firstLevelY = value; Invalidate(); }
        }

        /// <summary>Height (inside this control) of the middle of the Ground floor door.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int GroundLevelY
        {
            get { return _groundLevelY; }
            set { _groundLevelY = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            var shaft = new RectangleF(6, 0, Width - 12, Height - 1);
            DrawShaft(g, shaft);

            float centerY = Gfx.Lerp(_groundLevelY, _firstLevelY, _position - 1.0);
            float carWidth = shaft.Width - 14f;
            var car = new RectangleF(shaft.X + 7f, centerY - CarHeight / 2f, carWidth, CarHeight);

            // cable from the top of the shaft to the roof of the car
            float cableX = car.X + car.Width / 2f;
            using (var pen = new Pen(Color.FromArgb(170, 170, 176), 2f))
                g.DrawLine(pen, cableX, 0, cableX, car.Y);
            using (var pen = new Pen(Color.FromArgb(120, 120, 126), 1f))
                g.DrawLine(pen, cableX + 3f, 0, cableX + 3f, car.Y);

            DrawCar(g, car);
        }

        private void DrawShaft(Graphics g, RectangleF shaft)
        {
            using (var brush = new LinearGradientBrush(shaft, Color.FromArgb(52, 52, 58), Color.FromArgb(22, 22, 26), LinearGradientMode.Horizontal))
                g.FillRectangle(brush, shaft);

            // guide rails on both sides
            using (var rail = new Pen(Color.FromArgb(105, 105, 112), 3f))
            {
                g.DrawLine(rail, shaft.X + 4, 0, shaft.X + 4, shaft.Bottom);
                g.DrawLine(rail, shaft.Right - 4, 0, shaft.Right - 4, shaft.Bottom);
            }

            // cross bars every 36 px
            using (var bar = new Pen(Color.FromArgb(40, 255, 255, 255), 1f))
            {
                for (float y = 18f; y < shaft.Bottom; y += 36f)
                    g.DrawLine(bar, shaft.X + 4, y, shaft.Right - 4, y);
            }

            // floor levels
            DrawLevel(g, shaft, _firstLevelY, "1");
            DrawLevel(g, shaft, _groundLevelY, "G");

            using (var pen = new Pen(Color.FromArgb(80, 80, 86), 1.5f))
                g.DrawRectangle(pen, shaft.X, shaft.Y, shaft.Width, shaft.Height);
        }

        private void DrawLevel(Graphics g, RectangleF shaft, int y, string label)
        {
            float half = CarHeight / 2f + 6f;
            using (var pen = new Pen(Color.FromArgb(150, 255, 215, 90), 1.5f))
            {
                g.DrawLine(pen, shaft.X, y - half, shaft.X + 9, y - half);
                g.DrawLine(pen, shaft.X, y + half, shaft.X + 9, y + half);
                g.DrawLine(pen, shaft.Right - 9, y - half, shaft.Right, y - half);
                g.DrawLine(pen, shaft.Right - 9, y + half, shaft.Right, y + half);
            }

            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(200, 255, 215, 90)))
                g.DrawString(label, font, brush, shaft.X + 10, y - half - 1);
        }

        private void DrawCar(Graphics g, RectangleF car)
        {
            bool blink = Gfx.Ticks(300) % 2 == 0;

            // drop shadow
            using (GraphicsPath shadow = Gfx.RoundedRect(Gfx.Offset(car, 2, 3), 6))
            using (var brush = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                g.FillPath(brush, shadow);

            // metal body
            using (GraphicsPath body = Gfx.RoundedRect(car, 6))
            {
                using (var brush = Gfx.MetalBrush(car, LinearGradientMode.Horizontal))
                    g.FillPath(brush, body);

                Color outline = _emergency ? (blink ? Color.FromArgb(255, 60, 60) : Color.FromArgb(150, 20, 20)) : Color.FromArgb(70, 70, 76);
                using (var pen = new Pen(outline, _emergency ? 3f : 1.6f))
                    g.DrawPath(pen, body);
            }

            // lamp on the roof: green = standing, amber = moving, red = emergency
            Color lamp = _emergency ? (blink ? Color.FromArgb(255, 70, 70) : Color.FromArgb(120, 20, 20))
                       : _moving ? Color.FromArgb(255, 190, 40)
                       : Color.FromArgb(70, 220, 90);
            var lampRect = new RectangleF(car.X + car.Width / 2f - 4f, car.Y + 4f, 8f, 8f);
            using (var brush = new SolidBrush(lamp))
                g.FillEllipse(brush, lampRect);

            // window: the lit inside of the car
            var window = new RectangleF(car.X + 6f, car.Y + 17f, car.Width - 12f, car.Height - 25f);
            using (var brush = new LinearGradientBrush(window, Color.FromArgb(250, 240, 200), Color.FromArgb(150, 140, 110), 90f))
                g.FillRectangle(brush, window);

            if (_passengerColor >= 0)
            {
                GraphicsState state = g.Save();
                g.SetClip(window);
                Gfx.DrawPerson(g, window.X + window.Width / 2f, window.Bottom - 3f, window.Height * 0.95f,
                    Gfx.PassengerColor(_passengerColor), 0);
                g.Restore(state);
            }

            // door seam and frame of the window
            using (var pen = new Pen(Color.FromArgb(120, 60, 60, 66), 1.2f))
            {
                float mid = window.X + window.Width / 2f;
                g.DrawLine(pen, mid, window.Y, mid, window.Bottom);
            }
            using (var pen = new Pen(Color.FromArgb(150, 70, 70, 76), 1.5f))
                g.DrawRectangle(pen, window.X, window.Y, window.Width, window.Height);
        }
    }
}
