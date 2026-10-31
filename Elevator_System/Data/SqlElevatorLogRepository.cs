using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Lift.Data
{
    /// <summary>
    /// Reads and writes the ElevatorLog table with the DISCONNECTED model:
    ///   DataAdapter.Fill() -> change the DataTable in memory -> DataAdapter.Update().
    /// Saving and clearing share one method (ModifyLog), so no database code is repeated.
    /// </summary>
    public sealed class SqlElevatorLogRepository : IElevatorLogRepository
    {
        private const string TableQuery =
            "SELECT LogId, ElevatorId, FloorId, EventType, Message, LoggedAt FROM dbo.ElevatorLog";

        // The grid shows names instead of ids, newest entry first.
        private const string HistoryQuery =
            "SELECT l.LogId AS [ID], " +
            "       CONVERT(VARCHAR(10), l.LoggedAt, 103) AS [Date], " +
            "       CONVERT(VARCHAR(8), l.LoggedAt, 108) AS [Time], " +
            "       ISNULL(e.ElevatorName, N'System') AS [Elevator], " +
            "       ISNULL(f.FloorName, N'') AS [Floor], " +
            "       l.EventType AS [Event], " +
            "       l.Message AS [Action] " +
            "FROM dbo.ElevatorLog l " +
            "LEFT JOIN dbo.Elevators e ON e.ElevatorId = l.ElevatorId " +
            "LEFT JOIN dbo.Floors f ON f.FloorId = l.FloorId " +
            "ORDER BY l.LogId DESC";

        private readonly DatabaseSettings _settings;

        public SqlElevatorLogRepository(DatabaseSettings settings)
        {
            _settings = settings;
        }

        // read at every call: the initializer may switch to another server before the first save
        private string ConnectionString { get { return _settings.ConnectionString; } }

        public void Save(IEnumerable<LogEntry> entries)
        {
            // "WHERE 1 = 0" loads the table structure only, so saving never reads the old rows
            ModifyLog(TableQuery + " WHERE 1 = 0", table =>
            {
                foreach (LogEntry entry in entries)
                {
                    table.Rows.Add(ToRow(table, entry));
                }
            });
        }

        public void ClearHistory()
        {
            ModifyLog(TableQuery, table =>
            {
                foreach (DataRow row in table.Rows)
                {
                    row.Delete();
                }
            });
        }

        public DataTable LoadHistory()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                using (var adapter = new SqlDataAdapter(HistoryQuery, connection))
                {
                    var table = new DataTable("History");
                    adapter.Fill(table);            // Fill opens and closes the connection by itself
                    return table;
                }
            }
            catch (SqlException ex)
            {
                throw new DataAccessException("Could not read the log from the database: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// The one place that changes the database: fill a DataTable, let the caller change it,
        /// then send ALL the changes back with adapter.Update().
        /// </summary>
        private void ModifyLog(string selectQuery, Action<DataTable> change)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                using (var adapter = new SqlDataAdapter(selectQuery, connection))
                using (var builder = new SqlCommandBuilder(adapter))
                {
                    builder.ConflictOption = ConflictOption.OverwriteChanges;   // match rows by key only
                    adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                    var table = new DataTable("ElevatorLog");
                    adapter.Fill(table);
                    change(table);
                    adapter.Update(table);          // INSERT / DELETE commands are generated automatically
                }
            }
            catch (SqlException ex)
            {
                throw new DataAccessException("Could not update the log in the database: " + ex.Message, ex);
            }
        }

        private static DataRow ToRow(DataTable table, LogEntry entry)
        {
            DataRow row = table.NewRow();
            row["ElevatorId"] = entry.ElevatorId.HasValue ? (object)entry.ElevatorId.Value : DBNull.Value;
            row["FloorId"] = entry.FloorId.HasValue ? (object)entry.FloorId.Value : DBNull.Value;
            row["EventType"] = entry.EventType;
            row["Message"] = entry.Message.Length > 200 ? entry.Message.Substring(0, 200) : entry.Message;
            row["LoggedAt"] = entry.LoggedAt;
            return row;
        }
    }
}
