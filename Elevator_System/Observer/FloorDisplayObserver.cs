using System.Collections.Generic;
using Lift.Models;

namespace Lift.Observer
{
    /// <summary>
    /// Keeps the floor status displays of ONE lift up to date (one display per floor).
    /// Every event carries the lift's current location, so every display shows it.
    /// </summary>
    public sealed class FloorDisplayObserver : IElevatorObserver
    {
        private readonly List<IFloorStatusView> _views = new List<IFloorStatusView>();

        public FloorDisplayObserver(params IFloorStatusView[] views)
        {
            _views.AddRange(views);
        }

        public void Update(ElevatorEvent e)
        {
            foreach (IFloorStatusView view in _views)
            {
                view.ShowElevator(e);
            }
        }
    }
}
