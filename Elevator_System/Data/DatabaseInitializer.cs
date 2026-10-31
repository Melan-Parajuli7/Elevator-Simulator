using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.SqlClient;

namespace Lift.Data
{
    /// <summary>Creates the Lift database and its tables the first time the program runs.</summary>
    public sealed class DatabaseInitializer
    {
        private readonly DatabaseSettings _settings;

        public DatabaseInitializer(DatabaseSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Finds a SQL Server that answers (LocalDB, SQL Express, localhost), creates the Lift database
        /// there if it is missing and runs the script that creates the tables. Safe to run every time.
        /// </summary>
        public void EnsureCreated()
        {
            // the script is located with a relative path, so check it first and explain clearly if it is missing
            if (!File.Exists(_settings.ScriptPath))
            {
                throw new DataAccessException(
                    "The database script was not found at: " + _settings.ScriptPath +
                    ". Make sure the Database folder is next to the program.", null);
            }

            var problems = new List<string>();

            foreach (string server in _settings.CandidateServers)
            {
                _settings.UseServer(server);
                try
                {
                    CreateDatabaseIfMissing();
                    RunScript();
                    return;                                 // this server works: stop here
                }
                catch (SqlException ex)
                {
                    problems.Add(server + "  ->  " + FirstLine(ex.Message));
                }
            }

            throw new DataAccessException(
                "No SQL Server could be reached. Tried:\n" + string.Join("\n", problems) +
                "\n\nInstall SQL Server Express LocalDB (it comes with Visual Studio), " +
                "or set the environment variable LIFT_DB_SERVER to your server name.", null);
        }

        private static string FirstLine(string text)
        {
            int end = text.IndexOf('\n');
            return end > 0 ? text.Substring(0, end).Trim() : text.Trim();
        }

        private void CreateDatabaseIfMissing()
        {
            using (var connection = new SqlConnection(_settings.MasterConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "IF DB_ID(@name) IS NULL " +
                        "BEGIN " +
                        "  DECLARE @sql NVARCHAR(400) = N'CREATE DATABASE ' + QUOTENAME(@name); " +
                        "  EXEC (@sql); " +
                        "END";
                    command.Parameters.AddWithValue("@name", _settings.DatabaseName);

                    // Creating a database is DDL, which a DataAdapter cannot do, so this one place
                    // uses ExecuteNonQuery. Every change to the log DATA goes through DataAdapter.Update().
                    command.ExecuteNonQuery();
                }
            }
        }

        private void RunScript()
        {
            string script = File.ReadAllText(_settings.ScriptPath);

            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                connection.Open();
                foreach (string batch in SqlScript.SplitBatches(script))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = batch;
                        command.ExecuteNonQuery();      // DDL again (CREATE TABLE ...)
                    }
                }
            }
        }
    }
}
