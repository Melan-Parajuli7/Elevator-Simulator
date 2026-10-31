using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.SqlClient;

namespace Lift.Data
{
    /// <summary>
    /// Where the database is and where its creation script is. The script is found with a RELATIVE path
    /// (next to the program), so the project works on any computer and in any folder.
    /// The program tries several common SQL Server addresses, so it connects without any set-up.
    /// </summary>
    public sealed class DatabaseSettings
    {
        public const string DefaultDatabase = "ElevatorDb";

        private readonly List<string> _candidateServers = new List<string>();

        public DatabaseSettings(IEnumerable<string> candidateServers, string databaseName, string scriptPath)
        {
            _candidateServers.AddRange(candidateServers);
            if (_candidateServers.Count == 0) throw new ArgumentException("At least one server is needed.", "candidateServers");

            Server = _candidateServers[0];
            DatabaseName = databaseName;
            ScriptPath = scriptPath;
        }

        /// <summary>The server in use. It changes to the first server that answers.</summary>
        public string Server { get; private set; }

        public string DatabaseName { get; private set; }
        public string ScriptPath { get; private set; }

        /// <summary>Servers to try, in order.</summary>
        public IList<string> CandidateServers { get { return _candidateServers; } }

        /// <summary>Connection to the Lift database on the current server.</summary>
        public string ConnectionString { get { return Build(DatabaseName); } }

        /// <summary>Connection to the server's master database (only used to create the Lift database).</summary>
        public string MasterConnectionString { get { return Build("master"); } }

        public void UseServer(string server)
        {
            Server = server;
        }

        public static DatabaseSettings CreateDefault()
        {
            var servers = new List<string>();

            // LIFT_DB_SERVER lets another computer choose its own server without changing the code
            string custom = Environment.GetEnvironmentVariable("LIFT_DB_SERVER");
            if (!string.IsNullOrWhiteSpace(custom))
            {
                servers.Add(custom.Trim());
            }
            else
            {
                servers.Add(@"(localdb)\MSSQLLocalDB");   // installed with Visual Studio
                servers.Add(@".\SQLEXPRESS");              // SQL Server Express
                servers.Add(@"localhost");                 // a full SQL Server on this computer
            }

            string script = Path.Combine(AppContext.BaseDirectory, "Database", "Elevator.sql");
            return new DatabaseSettings(servers, DefaultDatabase, script);
        }

        private string Build(string database)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = Server;
            builder.InitialCatalog = database;
            builder.IntegratedSecurity = true;
            builder.ConnectTimeout = 20;
            builder.TrustServerCertificate = true;
            return builder.ConnectionString;
        }
    }
}
