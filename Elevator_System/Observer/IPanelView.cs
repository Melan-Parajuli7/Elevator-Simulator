using Lift.Models;

namespace Lift.Observer
{
    /// <summary>A control panel (LED screen and button lights) inside a lift.</summary>
    public interface IPanelView
    {
        void ShowStatus(ElevatorEvent e);
    }
}
