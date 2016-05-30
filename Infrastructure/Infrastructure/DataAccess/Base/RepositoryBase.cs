using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace AFT.RegoV2.Infrastructure.DataAccess.Base
{
    public class RepositoryBase : IRepositoryBase
    {
        public bool IsDatabaseSeeded()
        {
            var defaultConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            var databaseName = new SqlConnectionStringBuilder(defaultConnectionString).InitialCatalog;
            var masterConnectionString = GetMasterConnectionString(defaultConnectionString);
            using (var connection = new SqlConnection(masterConnectionString))
            {
                using (var command = new SqlCommand(string.Format("SELECT db_id('{0}')", databaseName), connection))
                {
                    connection.Open();
                    return (command.ExecuteScalar() != DBNull.Value);
                }
            }
        }

        static string GetMasterConnectionString(string connectionString)
        {
            var defaulConnectionString = connectionString;
            var connectionStringBuilder = new SqlConnectionStringBuilder(defaulConnectionString)
            {
                InitialCatalog = "Master",
            };
            return connectionStringBuilder.ConnectionString;
        }

        public int ExecuteSqlCommand(string query, bool swallowExceptions = false)
        {
            using (var db = new DbContext(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                try
                {
                    return db.Database.ExecuteSqlCommand(query);
                }
                catch (SqlException)
                {
                    if (!swallowExceptions)
                    {
                        throw;
                    }

                    return 0;
                }
            }
        }
    }
}
