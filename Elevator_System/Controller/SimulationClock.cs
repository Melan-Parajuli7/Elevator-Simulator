using System;
using System.Windows.Forms;
using Lift.Models;

namespace Lift.Controller
{
    /// <summary>
    /// The animation timer. Every tick it calls the delegate(s) subscribed to <see cref="Tick"/>,
    /// which is how the lifts, the doors and the displays are animated.
    /// </summary>
    public sealed class SimulationClock : IDisposable
    {
        private readonly Timer _timer = new Timer();

        public SimulationClock()
        {
            _timer.Interval = SimulationSettings.TickIntervalMs;
            _timer.Tick += OnTimerTick;
        }

        public event EventHandler Tick;

        public bool Running { get { return _timer.Enabled; } }

        public void Start() { _timer.Start(); }

        public void Stop() { _timer.Stop(); }

        private void OnTimerTick(object sender, EventArgs e)
        {
            EventHandler handler = Tick;
            if (handler != null) handler(this, e);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
