using Lift.Models;
using Lift.Subject;

namespace Lift.Controller
{
    /// <summary>What the dispatcher decided about one request from outside the lifts.</summary>
    public sealed class HallCallResult
    {
        public HallCallResult(Floor floor, Elevator elevator, bool alreadyServed, double estimatedSeconds, string message)
        {
            Floor = floor;
            Elevator = elevator;
            AlreadyServed = alreadyServed;
            EstimatedSeconds = estimatedSeconds;
            Message = message;
        }

        public Floor Floor { get; private set; }

        /// <summary>The lift that will answer, or null when no lift could take the call.</summary>
        public Elevator Elevator { get; private set; }

        /// <summary>True when a lift was already going to that floor, so no second lift was sent.</summary>
        public bool AlreadyServed { get; private set; }

        public double EstimatedSeconds { get; private set; }
        public string Message { get; private set; }
        public bool Accepted { get { return Elevator != null; } }
    }
}
