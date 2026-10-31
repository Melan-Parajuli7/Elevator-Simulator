using Lift.Models;
using Lift.Subject;

namespace Lift.State
{
    public sealed class DoorsOpeningState : ElevatorStateBase
    {
        public override string Name { get { return "Doors opening"; } }

        public override void Tick(Elevator e)
        {
            e.DoorOpenness += SimulationSettings.DoorFractionPerTick;
            if (e.DoorOpenness >= 1.0)
            {
                e.DoorOpenness = 1.0;
                e.Raise(ElevatorEventKind.DoorsOpened, "Doors opened at " + e.CurrentFloor.DisplayName() + " (" + e.Name + ")");
                e.ChangeState(new DoorsOpenState());
            }
        }

        public override void OnRequestAdded(Elevator e, Floor floor)
        {
            // somebody asks for the floor where the doors are opening right now: nothing more to do
            if (floor == e.CurrentFloor) e.CompleteRequestsAt(floor);
        }

        public override void OnClosePressed(Elevator e)
        {
            e.ChangeState(new DoorsClosingState());
        }

        public override string StatusText(Elevator e) { return "DOORS OPENING"; }

        public override double RemainingTicks(Elevator e)
        {
            return (1.0 - e.DoorOpenness) / SimulationSettings.DoorFractionPerTick
                   + SimulationSettings.HoldOpenTicks
                   + 1.0 / SimulationSettings.DoorFractionPerTick;
        }
    }
}
