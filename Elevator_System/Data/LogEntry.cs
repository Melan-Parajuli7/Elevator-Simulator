using System;

namespace Lift.Data
{
    /// <summary>One row of the ElevatorLog table, as a plain object.</summary>
    public sealed class LogEntry
    {
        public LogEntry(int? elevatorId, int? floorId, string eventType, string message, DateTime loggedAt)
        {
            ElevatorId = elevatorId;
            FloorId = floorId;
            EventType = eventType;
            Message = message;
            LoggedAt = loggedAt;
        }

        public int? ElevatorId { get; private set; }
        public int? FloorId { get; private set; }
        public string EventType { get; private set; }
        public string Message { get; private set; }
        public DateTime LoggedAt { get; private set; }

        /// <summary>An entry that belongs to the whole system and not to one lift.</summary>
        public static LogEntry ForSystem(string eventType, string message)
        {
            return new LogEntry(null, null, eventType, message, DateTime.Now);
        }
    }
}
