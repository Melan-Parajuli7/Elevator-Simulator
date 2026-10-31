using Lift.Models;
using Lift.Subject;

namespace Lift.State
{
    /// <summary>Doors fully open. They close by themselves after a short hold, or when Close is pressed.</summary>
    public sealed class DoorsOpenState : ElevatorStateBase
    {
        private int _heldTicks;

        public override string Name { get { return "Doors open"; } }

        public override void Tick(Elevator e)
        {
            if (e.ObstructionPresent)
            {
                // object in the doorway: keep the doors open and start the countdown again
                _heldTicks = SimulationSettings.HoldOpenTicks - SimulationSettings.HoldAfterObstructionTicks;
                return;
            }

            _heldTicks++;

            bool someoneWaitingElsewhere = e.HasPendingRequests;
            if (_heldTicks >= SimulationSettings.HoldOpenTicks ||
                (someoneWaitingElsewhere && _heldTicks >= SimulationSettings.HoldOpenWhenCalledTicks))
            {
                e.ChangeState(new DoorsClosingState());
            }
        }

        public override void OnRequestAdded(Elevator e, Floor floor)
        {
            if (floor == e.CurrentFloor)
            {
                e.CompleteRequestsAt(floor);
                _heldTicks = 0;                    // keep the doors open a little longer
            }
        }

        public override void OnOpenPressed(Elevator e)
        {
            _heldTicks = 0;
        }

        public override void OnClosePressed(Elevator e)
        {
            e.ChangeState(new DoorsClosingState());
        }

        public override string StatusText(Elevator e) { return "DOORS OPEN"; }

        public override double RemainingTicks(Elevator e)
        {
            return System.Math.Max(0, SimulationSettings.HoldOpenTicks - _heldTicks)
                   + 1.0 / SimulationSettings.DoorFractionPerTick;
        }
    }
}
