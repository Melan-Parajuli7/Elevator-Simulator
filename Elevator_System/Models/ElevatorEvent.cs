using System;

namespace Lift.Models
{
    /// <summary>
    /// The message a Subject (Elevator) sends to its Observers. It is a snapshot: it holds
    /// plain values, so an observer never needs to call back into the lift.
    /// </summary>
    public sealed class ElevatorEvent
    {
        public ElevatorEvent(int elevatorId, string elevatorName, ElevatorEventKind kind,
            string message, DateTime timestamp)
        {
            ElevatorId = elevatorId;
            ElevatorName = elevatorName;
            Kind = kind;
            Message = message;
            Timestamp = timestamp;
        }

        public int ElevatorId { get; private set; }
        public string ElevatorName { get; private set; }
        public ElevatorEventKind Kind { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; private set; }

        // ---- snapshot of the lift at the moment of the event ----
        public Floor DisplayFloor { get; set; }
        public Floor? AtFloor { get; set; }          // exact floor when standing at one, otherwise null
        public Direction Direction { get; set; }
        public bool Moving { get; set; }
        public bool Emergency { get; set; }
        public Floor? TargetFloor { get; set; }
        public Floor? RelatedFloor { get; set; }     // the floor a button or a call was about
        public string StateName { get; set; }
        public string StatusText { get; set; }       // text for the LED screen of the control panel
        public bool PendingGround { get; set; }
        public bool PendingFirst { get; set; }

        public bool IsPending(Floor floor)
        {
            return floor == Floor.Ground ? PendingGround : PendingFirst;
        }
    }
}
