using Lift.Subject;

namespace Lift.State
{
    /// <summary>
    /// State pattern: every action a lift can receive is answered by the CURRENT state object,
    /// so the Elevator class needs no big "if the lift is moving ... else if ..." blocks.
    /// </summary>
    public interface IElevatorState
    {
        string Name { get; }
        void Enter(Elevator e);
        void Tick(Elevator e);
        void OnRequestAdded(Elevator e, Lift.Models.Floor floor);
        void OnOpenPressed(Elevator e);
        void OnClosePressed(Elevator e);
        void OnEmergencyPressed(Elevator e);

        /// <summary>Ticks until this state finishes what it is doing (used to estimate response times).</summary>
        double RemainingTicks(Elevator e);

        /// <summary>Text for the LED screen of the control panel while the lift is in this state.</summary>
        string StatusText(Elevator e);
    }
}
