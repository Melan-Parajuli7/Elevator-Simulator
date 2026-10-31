using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lift.UI.Controls
{
    /// <summary>The waiting area of one floor, next to the call button: it shows the people waiting for a lift.</summary>
    public class LobbyControl : Control
    {
        private readonly List<int> _people = new List<int>();
        private string _floorLabel = "";

        public LobbyControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Size = new Size(80, 84);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string FloorLabel
        {
            get { return _floorLabel; }
            set { _floorLabel = value; Invalidate(); }
        }

        /// <summary>Colour indexes of the people standing in this lobby.</summary>
        public void SetPeople(IList<int> people)
        {
            bool same = people.Count == _people.Count;
            for (int i = 0; same && i < people.Count; i++)
            {
                if (people[i] != _people[i]) same = false;
            }
            if (same) return;

            _people.Clear();
            _people.AddRange(people);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float floorY = Height - 14f;

            // floor strip
            using (var brush = new LinearGradientBrush(new RectangleF(4, floorY, Width - 8, 10),
                Color.FromArgb(200, 200, 205), Color.FromArgb(150, 150, 156), 90f))
                g.FillRectangle(brush, 4, floorY, Width - 8, 10);

            using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(105, 105, 110)))
                g.DrawString(_floorLabel, font, brush, 6, floorY + 0.5f);

            float spacing = _people.Count > 1 ? 30f : 0f;
            float startX = Width / 2f - spacing * (_people.Count - 1) / 2f;
            for (int i = 0; i < _people.Count; i++)
            {
                Gfx.DrawPerson(g, startX + i * spacing, floorY + 4f, 58f, Gfx.PassengerColor(_people[i]), 0);
            }
        }
    }
}
