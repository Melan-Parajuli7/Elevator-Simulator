using Lift.Data;
using Lift.Models;

namespace Lift.Observer
{
    /// <summary>Writes the important events of a lift to the log (the database), through an ILogWriter.</summary>
    public sealed class LogObserver : IElevatorObserver
    {
        private readonly ILogWriter _writer;

        public LogObserver(ILogWriter writer)
        {
            _writer = writer;
        }

        public void Update(ElevatorEvent e)
        {
            // FloorChanged and StateChanged only exist to refresh displays; they are not operations to record.
            if (e.Kind == ElevatorEventKind.FloorChanged || e.Kind == ElevatorEventKind.StateChanged)
            {
                return;
            }

            int floorId = (int)(e.RelatedFloor ?? e.DisplayFloor);
            _writer.Write(new LogEntry(e.ElevatorId, floorId, e.Kind.ToString(), e.Message, e.Timestamp));
        }
    }
}
