using System.Collections.Generic;
using System.Data;

namespace Lift.Data
{
    /// <summary>Everything the program needs from the database. Tests use a fake implementation.</summary>
    public interface IElevatorLogRepository
    {
        /// <summary>Stores new log entries.</summary>
        void Save(IEnumerable<LogEntry> entries);

        /// <summary>All stored entries, ready to show in a grid (newest first).</summary>
        DataTable LoadHistory();

        /// <summary>Deletes every stored entry.</summary>
        void ClearHistory();
    }
}
