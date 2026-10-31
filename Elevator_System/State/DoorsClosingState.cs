using Lift.Models;
using Lift.Subject;

namespace Lift.State
{
    public sealed class DoorsClosingState : ElevatorStateBase
    {
        public override string Name { get { return "Doors closing"; } }

        public override void Tick(Elevator e)
        {
            if (e.ObstructionPresent)
            {
                e.Raise(ElevatorEventKind.ObstructionDetected,
                    "Object in the doorway at " + e.CurrentFloor.DisplayName() + " - " + e.Name + " doors reopening");
                e.ChangeState(new DoorsOpeningState());
                return;
            }

            e.DoorOpenness -= SimulationSettings.DoorFractionPerTick;
            if (e.DoorOpenness <= 0.0)
            {
                e.DoorOpenness = 0.0;
                e.Raise(ElevatorEventKind.DoorsClosed, "Doors closed at " + e.CurrentFloor.DisplayName() + " (" + e.Name + ")");
                e.ChangeState(new IdleState());
            }
        }

        public override void OnRequestAdded(Elevator e, Floor floor)
        {
            // asked for the floor we are standing at while the doors close: open again
            if (floor == e.CurrentFloor)
            {
                e.CompleteRequestsAt(floor);
                e.ChangeState(new DoorsOpeningState());
            }
        }

        public override void OnOpenPressed(Elevator e)
        {
            e.ChangeState(new DoorsOpeningState());
        }

        public override string StatusText(Elevator e) { return "DOORS CLOSING"; }

        public override double RemainingTicks(Elevator e)
        {
            return e.DoorOpenness / SimulationSettings.DoorFractionPerTick;
        }
    }
}
