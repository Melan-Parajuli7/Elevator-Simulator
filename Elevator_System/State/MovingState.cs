using System;
using Lift.Models;
using Lift.Subject;

namespace Lift.State
{
    /// <summary>
    /// Travelling between the floors. New requests are only queued: the trip in progress is never cut.
    /// </summary>
    public sealed class MovingState : ElevatorStateBase
    {
        private readonly Floor _target;

        public MovingState(Floor target)
        {
            _target = target;
        }

        public override string Name { get { return "Moving"; } }

        public Floor Target { get { return _target; } }

        public override void Enter(Elevator e)
        {
            e.MovingTarget = _target;
            e.Direction = (int)_target > e.Position ? Direction.Up : Direction.Down;
            e.Raise(ElevatorEventKind.MovementStarted,
                string.Format("{0} moving {1} from {2} to {3}", e.Name,
                    e.Direction == Direction.Up ? "up" : "down", e.CurrentFloor.DisplayName(), _target.DisplayName()),
                _target);
        }

        public override void Tick(Elevator e)
        {
            double goal = (int)_target;
            double step = SimulationSettings.FloorsPerTick;
            double distance = goal - e.Position;

            if (Math.Abs(distance) <= step)
            {
                e.MoveTo(goal);
                e.Direction = Direction.None;
                e.MovingTarget = null;
                e.CompleteRequestsAt(_target);
                e.Raise(ElevatorEventKind.FloorArrived,
                    string.Format("{0} arrived at {1}", e.Name, _target.DisplayName()), _target);
                e.ChangeState(new DoorsOpeningState());
                return;
            }

            e.MoveTo(e.Position + (distance > 0 ? step : -step));
        }

        public override string StatusText(Elevator e)
        {
            return "MOVING TO " + _target.DisplayName().ToUpper();
        }

        /// <summary>The trip itself is counted by the estimate plan in Elevator, so nothing is added here.</summary>
        public override double RemainingTicks(Elevator e)
        {
            return 0;
        }
    }
}
