using Lift.Models;
using Lift.Subject;

namespace Lift.State
{
    /// <summary>Default behaviour shared by all states. A state overrides only what it does differently.</summary>
    public abstract class ElevatorStateBase : IElevatorState
    {
        public abstract string Name { get; }

        public virtual void Enter(Elevator e) { }
        public virtual void Tick(Elevator e) { }
        public virtual void OnRequestAdded(Elevator e, Floor floor) { }
        public virtual void OnOpenPressed(Elevator e) { }
        public virtual void OnClosePressed(Elevator e) { }

        /// <summary>The SOS button works in every state except the emergency state itself.</summary>
        public virtual void OnEmergencyPressed(Elevator e)
        {
            e.ChangeState(new EmergencyState());
        }

        public virtual double RemainingTicks(Elevator e)
        {
            return 0;
        }

        public virtual string StatusText(Elevator e)
        {
            return Name.ToUpper();
        }
    }
}
