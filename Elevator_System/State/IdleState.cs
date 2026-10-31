using Lift.Models;
using Lift.Subject;

namespace Lift.State
{
    /// <summary>Standing at a floor with the doors closed, waiting for work.</summary>
    public sealed class IdleState : ElevatorStateBase
    {
        public override string Name { get { return "Idle"; } }

        public override void Enter(Elevator e)
        {
            e.Direction = Direction.None;
        }

        public override void Tick(Elevator e)
        {
            Floor next;
            if (!e.TryPeekRequest(out next))
            {
                return;
            }

            if (next == e.CurrentFloor)
            {
                e.CompleteRequestsAt(next);
                e.ChangeState(new DoorsOpeningState());
            }
            else
            {
                e.ChangeState(new MovingState(next));
            }
        }

        public override string StatusText(Elevator e)
        {
            Floor next;
            return e.TryPeekRequest(out next)
                ? "GOING TO " + next.DisplayName().ToUpper()
                : e.CurrentFloor.DisplayName().ToUpper() + " - READY";
        }

        public override void OnOpenPressed(Elevator e)
        {
            e.ChangeState(new DoorsOpeningState());
        }
    }
}
