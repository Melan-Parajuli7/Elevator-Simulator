using Lift.Models;

namespace Lift.Observer
{
    /// <summary>Keeps the control panel display window of ONE lift up to date.</summary>
    public sealed class ControlPanelObserver : IElevatorObserver
    {
        private readonly IPanelView _panel;

        public ControlPanelObserver(IPanelView panel)
        {
            _panel = panel;
        }

        public void Update(ElevatorEvent e)
        {
            _panel.ShowStatus(e);
        }
    }
}
