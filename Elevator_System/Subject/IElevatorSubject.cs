using Lift.Models;
using Lift.Observer;

namespace Lift.Subject
{
    /// <summary>Subject of the Observer pattern: one lift, many observers.</summary>
    public interface IElevatorSubject
    {
        void Attach(IElevatorObserver observer);
        void Detach(IElevatorObserver observer);
        void Notify(ElevatorEvent e);
    }
}
