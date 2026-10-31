using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Lift.UI.Controls
{
    public enum PanelGlyph
    {
        Text,
        ArrowUp,
        ArrowDown,
        OpenDoors,
        CloseDoors,
        Emergency
    }

    public enum PanelLook
    {
        Metal,   // grey floor / door buttons on the panel
        Dark,    // black call buttons next to the doors
        Alarm    // red emergency button
    }

    /// <summary>Round-cornered lift button with a metal bezel that glows when "lit".</summary>
    public class PanelButton : Control
    {
        private PanelGlyph _glyph = PanelGlyph.Text;
        private PanelLook _look = PanelLook.Metal;
        private bool _lit;
        private bool _hover;
        private bool _pressed;

        public PanelButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Size = new Size(72, 72);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public PanelGlyph Glyph
        {
            get { return _glyph; }
            set { if (_glyph != value) { _glyph = value; Invalidate(); } }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public PanelLook Look
        {
            get { return _look; }
            set { if (_look != value) { _look = value; Invalidate(); } }
        }

        /// <summary>True = button is illuminated (request pending / active).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Lit
        {
            get { return _lit; }
            set { if (_lit != value) { _lit = value; Invalidate(); } }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }

        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            var outer = new RectangleF(2, 2, Width - 5, Height - 5);
            float radius = Math.Min(outer.Width, outer.Height) * 0.16f;

            // drop shadow
            using (GraphicsPath shadow = Gfx.RoundedRect(new RectangleF(outer.X + 1.5f, outer.Y + 2.5f, outer.Width, outer.Height), radius))
            using (var brush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                g.FillPath(brush, shadow);

            // metal bezel
            using (GraphicsPath bezel = Gfx.RoundedRect(outer, radius))
            {
                using (var brush = new LinearGradientBrush(outer, Color.FromArgb(246, 246, 246), Color.FromArgb(118, 118, 124), 60f))
                    g.FillPath(brush, bezel);

                if (_lit)
                {
                    Color glow = _look == PanelLook.Alarm ? Color.FromArgb(255, 150, 40) : Color.FromArgb(90, 190, 255);
                    using (var pen = new Pen(glow, 3f)) g.DrawPath(pen, bezel);
                }
                else
                {
                    using (var pen = new Pen(Color.FromArgb(90, 90, 96), 1f)) g.DrawPath(pen, bezel);
                }
            }

            // face
            RectangleF face = RectangleF.Inflate(outer, -4.5f, -4.5f);
            if (_pressed) face.Offset(0, 1.2f);

            Color top, bottom;
            GetFaceColors(out top, out bottom);

            using (GraphicsPath facePath = Gfx.RoundedRect(face, radius * 0.7f))
            {
                using (var brush = new LinearGradientBrush(face, top, bottom, 90f))
                    g.FillPath(brush, facePath);

                // gloss on the upper half
                var gloss = new RectangleF(face.X, face.Y, face.Width, face.Height * 0.5f);
                using (GraphicsPath glossPath = Gfx.RoundedRect(gloss, radius * 0.7f))
                using (var brush = new LinearGradientBrush(gloss, Color.FromArgb(60, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
                    g.FillPath(brush, glossPath);
            }

            DrawGlyph(g, face);
        }

        private void GetFaceColors(out Color top, out Color bottom)
        {
            switch (_look)
            {
                case PanelLook.Dark:
                    top = _lit ? Color.FromArgb(70, 100, 130) : Color.FromArgb(66, 66, 70);
                    bottom = _lit ? Color.FromArgb(30, 50, 70) : Color.FromArgb(26, 26, 28);
                    break;

                case PanelLook.Alarm:
                    bool bright = _lit && Gfx.Ticks(300) % 2 == 0;
                    top = bright ? Color.FromArgb(255, 120, 100) : Color.FromArgb(232, 58, 58);
                    bottom = bright ? Color.FromArgb(210, 20, 20) : Color.FromArgb(140, 0, 0);
                    break;

                default:
                    top = _lit ? Color.FromArgb(205, 222, 235) : Color.FromArgb(182, 182, 188);
                    bottom = _lit ? Color.FromArgb(120, 150, 175) : Color.FromArgb(118, 118, 124);
                    break;
            }

            if (_hover && !_pressed)
            {
                top = Gfx.Lighten(top, 22);
                bottom = Gfx.Lighten(bottom, 22);
            }
        }

        private void DrawGlyph(Graphics g, RectangleF face)
        {
            float cx = face.X + face.Width / 2f;
            float cy = face.Y + face.Height / 2f;

            using (var white = new SolidBrush(Color.FromArgb(250, 250, 250)))
            using (var shadow = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            {
                switch (_glyph)
                {
                    case PanelGlyph.Text:
                        using (var font = new Font("Segoe UI", face.Height * 0.55f, FontStyle.Regular, GraphicsUnit.Pixel))
                        {
                            Gfx.DrawCentered(g, Text, font, shadow, Gfx.Offset(face, 1.2f, 1.5f));
                            Gfx.DrawCentered(g, Text, font, white, face);
                        }
                        break;

                    case PanelGlyph.ArrowUp:
                    case PanelGlyph.ArrowDown:
                        float halfW = face.Width * 0.22f;
                        float halfH = face.Height * 0.17f;
                        PointF[] triangle = _glyph == PanelGlyph.ArrowUp
                            ? new PointF[] { new PointF(cx - halfW, cy + halfH), new PointF(cx + halfW, cy + halfH), new PointF(cx, cy - halfH) }
                            : new PointF[] { new PointF(cx - halfW, cy - halfH), new PointF(cx + halfW, cy - halfH), new PointF(cx, cy + halfH) };
                        g.FillPolygon(white, triangle);
                        break;

                    case PanelGlyph.OpenDoors:
                    case PanelGlyph.CloseDoors:
                        DrawDoorSymbol(g, white, cx, cy, face, _glyph == PanelGlyph.OpenDoors);
                        break;

                    case PanelGlyph.Emergency:
                        using (var font = new Font("Segoe UI", face.Height * 0.36f, FontStyle.Bold, GraphicsUnit.Pixel))
                        {
                            Gfx.DrawCentered(g, "SOS", font, shadow, Gfx.Offset(face, 1.2f, 1.5f), true);
                            Gfx.DrawCentered(g, "SOS", font, white, face, true);
                        }
                        break;
                }
            }
        }

        // Open = arrows point away from the centre bar, Close = arrows point towards it.
        private static void DrawDoorSymbol(Graphics g, Brush brush, float cx, float cy, RectangleF face, bool open)
        {
            float barHalfH = face.Height * 0.22f;
            float triHalfH = face.Height * 0.15f;
            float triW = face.Width * 0.20f;
            float gap = face.Width * 0.07f;

            g.FillRectangle(brush, cx - 1.6f, cy - barHalfH, 3.2f, barHalfH * 2f);

            float leftTip = open ? cx - gap - triW : cx - gap;
            float leftBase = open ? cx - gap : cx - gap - triW;
            g.FillPolygon(brush, new PointF[]
            {
                new PointF(leftBase, cy - triHalfH),
                new PointF(leftBase, cy + triHalfH),
                new PointF(leftTip, cy)
            });

            float rightTip = open ? cx + gap + triW : cx + gap;
            float rightBase = open ? cx + gap : cx + gap + triW;
            g.FillPolygon(brush, new PointF[]
            {
                new PointF(rightBase, cy - triHalfH),
                new PointF(rightBase, cy + triHalfH),
                new PointF(rightTip, cy)
            });
        }
    }
}
