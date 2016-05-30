using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using AFT.UGS.Builds.UgsDatabaseHelper.Services;
using CommandLine;

namespace AFT.UGS.Builds.UgsDatabaseHelper
{
    public class Program
    {
        private static readonly IDatabaseHelperLogger Log = new DatabaseHelperLogger();

        private const int Online = 0;

        public static void Main(string[] args)
        {
            Log.CreateLog();

            try
            {
                var options = new Options();

                var parser = new Parser(s => { s.HelpWriter = Console.Error; });

                if (!parser.ParseArguments(args, options)) return;

                if (string.Equals(options.Command, "DropCreateAagDatabase", StringComparison.InvariantCultureIgnoreCase))
                {
                    DropCreateDatabases(options);
                }
                else if (string.Equals(options.Command, "Backup", StringComparison.InvariantCultureIgnoreCase))
                {
                    Backup(options);
                }
                else if (string.Equals(options.Command, "Restore", StringComparison.InvariantCultureIgnoreCase))
                {
                    Restore(options);
                }
                else
                {
                    Log.LogError("Unknown command: {0}", options.Command);
                }
            }
            catch (Exception e)
            {
                Log.LogError("Error: {0}", e);
                throw;
            }
        }

        private static void Backup(Options options)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var username = options.Username ?? appSettings["username"];
            var password = options.Password ?? appSettings["password"];
            var dbName = options.DatabaseName ?? appSettings["databaseName"];
            var filePath = options.FilePath ?? appSettings["file"];
            var host = options.Host ?? appSettings["host"];

            var connStr = string.Format("Server={0};Persist Security Info=True;integrated security=false;User Id={1}; Password={2};", host, username, password);
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                Log.LogInfo("Creating backup.");
                RunParameterLessNonQuery(conn, string.Format("BACKUP DATABASE [{0}] TO DISK = '{1}' WITH FORMAT", dbName, filePath));
            }
        }

        private static void Restore(Options options)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var username = options.Username ?? appSettings["username"];
            var password = options.Password ?? appSettings["password"];
            var dbName = options.DatabaseName ?? appSettings["databaseName"];
            var filePath = options.FilePath ?? appSettings["file"];
            var host = options.Host ?? appSettings["host"];

            var connStr = string.Format("Server={0};Persist Security Info=True;integrated security=false;User Id={1}; Password={2};", host, username, password);
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                Log.LogInfo("Restoring database.");
                RunParameterLessNonQuery(conn, string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", dbName));
                RunParameterLessNonQuery(conn, string.Format("RESTORE DATABASE [{0}] FROM DISK = '{1}' WITH REPLACE", dbName, filePath));
            }
        }

        private static void DropCreateDatabases(Options options)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var username = options.Username ?? appSettings["username"];
            var password = options.Password ?? appSettings["password"];
            var agName = options.AvailabilityGroupName ?? appSettings["availabilityGroupName"];
            var dbName = options.DatabaseName ?? appSettings["databaseName"];
            var fullBackupPath = Path.Combine(options.BackupPath ?? appSettings["backupPath"], dbName + ".bak");
            var primaryReplica = options.PrimaryReplica ?? appSettings["primaryReplica"];
            var secondaryReplicaStr = options.SecondaryReplicas ?? appSettings["secondaryReplicas"];
            var secondaryReplicas = secondaryReplicaStr.Split(',');
            var isStaging = string.Equals(dbName, "Rego-Staging", StringComparison.InvariantCultureIgnoreCase);

            if (!isStaging)
            {
                Log.LogError("Database name can only be Rego-Staging.");
                return;
            }

            var secondaryReplicaInfos = new List<SecondaryReplicaInfo>();
            try
            {
                foreach (var replicaHost in secondaryReplicas)
                {
                    var info = new SecondaryReplicaInfo
                    {
                        Host = replicaHost,
                        Connection = new SqlConnection(
                            string.Format(
                                "Server={0};Persist Security Info=True;integrated security=false;User Id={1}; Password={2};",
                                replicaHost, username, password))
                    };
                    secondaryReplicaInfos.Add(info);
                }

                var primaryConnStr =
                string.Format(
                    "Server={0};Persist Security Info=True;integrated security=false;User Id={1}; Password={2};",
                    primaryReplica, username, password);

                using (var primaryConn = new SqlConnection(primaryConnStr))
                {
                    primaryConn.Open();

                    foreach (var info in secondaryReplicaInfos)
                    {
                        var secondaryConn = info.Connection;

                        secondaryConn.Open();

                        var secondaryDbInfo = GetDatabaseInfo(secondaryConn, dbName);
                        if (secondaryDbInfo == null) continue;

                        if (secondaryDbInfo.GroupDatabaseId.HasValue)
                        {
                            Log.LogInfo("Removing secondary database at {0} from availability group.", info.Host);
                            RunParameterLessNonQuery(secondaryConn, string.Format("ALTER DATABASE [{0}] SET HADR OFF", dbName));
                        }

                        Log.LogInfo("Dropping secondary database at {0}.", info.Host);
                        secondaryDbInfo = GetDatabaseInfo(secondaryConn, dbName);
                        if (secondaryDbInfo.State == Online) {
                            RunParameterLessNonQuery(secondaryConn, string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", dbName));
                        }
                        RunParameterLessNonQuery(secondaryConn, string.Format("DROP DATABASE [{0}]", dbName));
                    }

                    var primaryDbInfo = GetDatabaseInfo(primaryConn, dbName);
                    if (primaryDbInfo != null)
                    {
                        if (primaryDbInfo.GroupDatabaseId.HasValue)
                        {
                            Log.LogInfo("Removing primary database from availability group.");
                            RunParameterLessNonQuery(primaryConn, string.Format("ALTER AVAILABILITY GROUP [{0}] REMOVE DATABASE [{1}]", agName, dbName));
                        }

                        Log.LogInfo("Dropping primary database.");
                        primaryDbInfo = GetDatabaseInfo(primaryConn, dbName);
                        if (primaryDbInfo.State == Online)
                        {
                            RunParameterLessNonQuery(primaryConn, string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", dbName));
                        }
                        RunParameterLessNonQuery(primaryConn, string.Format("DROP DATABASE [{0}]", dbName));
                    }

                    Log.LogInfo("Creating primary database.");
                    RunParameterLessNonQuery(primaryConn, string.Format("CREATE DATABASE [{0}] ON  PRIMARY ( NAME = N'{1}', FILENAME = N'C:\\Program Files\\Microsoft SQL Server\\RegoDB\\{2}.mdf') LOG ON ( NAME = N'{3}_log', FILENAME = N'C:\\Program Files\\Microsoft SQL Server\\RegoDB\\{4}.ldf')", dbName, dbName, dbName, dbName, dbName));

                    Log.LogInfo("Setting recovery mode to full.");
                    RunParameterLessNonQuery(primaryConn,
                        string.Format("ALTER DATABASE [{0}] SET RECOVERY FULL", dbName));

                    Log.LogInfo("Creating backup.");
                    RunParameterLessNonQuery(primaryConn,
                        string.Format("BACKUP DATABASE [{0}] TO DISK = '{1}' WITH FORMAT", dbName, fullBackupPath));

                    Log.LogInfo("Adding database to availability group.");
                    RunParameterLessNonQuery(primaryConn,
                        string.Format("ALTER AVAILABILITY GROUP [{0}] ADD DATABASE [{1}]", agName, dbName));

                    foreach (var info in secondaryReplicaInfos)
                    {
                        var secondaryConn = info.Connection;

                        Log.LogInfo("Restoring database on secondary replica {0}.", info.Host);
                        RunParameterLessNonQuery(secondaryConn,
                            string.Format("RESTORE DATABASE [{0}] FROM DISK = '{1}' WITH NORECOVERY", dbName, fullBackupPath));

                        Log.LogInfo("Adding secondary database at {0} to availability group.", info.Host);
                        RunParameterLessNonQuery(secondaryConn,
                            string.Format("ALTER DATABASE [{0}] SET HADR AVAILABILITY GROUP = [{1}]", dbName, agName));
                    }

                    var dbUsername = "regostaging";
                    Log.LogInfo("Creating user.");
                    RunParameterLessNonQuery(primaryConn, string.Format("USE [{0}] CREATE USER [{1}] FOR LOGIN [{2}] WITH DEFAULT_SCHEMA=[dbo]", dbName, dbUsername, dbUsername));
                    RunParameterLessNonQuery(primaryConn, string.Format("USE [{0}] ALTER ROLE [db_owner] ADD MEMBER [{1}]", dbName, dbUsername));
                }
            }
            finally
            {
                foreach (var info in secondaryReplicaInfos)
                {
                    if (info.Connection != null) info.Connection.Dispose();
                    info.Connection = null;
                }
            }
        }

        private static DatabaseInfo GetDatabaseInfo(SqlConnection conn, string dbName)
        {
            DatabaseInfo databaseInfo = null;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT database_id, group_database_id, state FROM sys.databases WHERE name = @name";
                cmd.Parameters.AddWithValue("@name", dbName);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var groupDatabaseId = DBNull.Value.Equals(reader[1]) ? null : (Guid?)reader[1];
                        databaseInfo = new DatabaseInfo { GroupDatabaseId = groupDatabaseId, State = (byte)reader[2] };
                    }
                }
            }

            return databaseInfo;
        }

        private static int RunParameterLessNonQuery(SqlConnection conn, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
