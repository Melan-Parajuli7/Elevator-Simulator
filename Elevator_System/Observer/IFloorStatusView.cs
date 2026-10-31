using Lift.Models;

namespace Lift.Observer
{
    /// <summary>
    /// A floor status display (the small screen and lamp above a lift door).
    /// The observer talks to this interface only, so it does not depend on WinForms.
    /// </summary>
    public interface IFloorStatusView
    {
        void ShowElevator(ElevatorEvent e);
    }
}
