using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AFT.RegoV2.Shared.Synchronization
{
    /// <summary>
    ///  Lock service based on DB lock
    /// </summary>
    public class SynchronizationService : ISynchronizationService
    {
        private readonly string _masterConnectionString;
        private const int SynchronizationTimeOut = 180;
        private readonly string _instanceName;

        public SynchronizationService()
        {
            _instanceName = ConfigurationManager.AppSettings["InstanceName"];

            if ( string.IsNullOrWhiteSpace(_instanceName))
                throw new RegoException("SynchronizationService is missing InstanceName.");

            var defaulConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            var connectionStringBuilder = new SqlConnectionStringBuilder(defaulConnectionString)
            {
                InitialCatalog = "Master",
            };
            _masterConnectionString = connectionStringBuilder.ConnectionString;
        }

        /// <summary>
        /// make sure you aware that total lock time can't be greater than SynchronizationTimeOut.
        /// default DbContext timeout 15 sec.
        /// </summary>
        public void Execute(string sectionName, Action action)
        {
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();

                using (var context = new DbContext(connection, contextOwnsConnection:false))
                {
                    context.Database.CommandTimeout = SynchronizationTimeOut;
                    
                    try
                    {
                        var getLockQuery = GetLockQuery(sectionName);
                        context.Database.ExecuteSqlCommand(getLockQuery);

                        action();
                    }
                    finally
                    {
                        var releaseLockQuery = GetReleaseLockQuery(sectionName);
                        context.Database.ExecuteSqlCommand(releaseLockQuery);
                    }
                }
            }
        }

        public async Task ExecuteAsync(string sectionName, Func<Task> action)
        {
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();

                using (var context = new DbContext(connection, contextOwnsConnection:false))
                {
                    context.Database.CommandTimeout = SynchronizationTimeOut;
                    
                    try
                    {
                        var getLockQuery = GetLockQuery(sectionName);
                        await context.Database.ExecuteSqlCommandAsync(getLockQuery);

                        await action();
                    }
                    finally
                    {
                        var releaseLockQuery = GetReleaseLockQuery(sectionName);
                        await context.Database.ExecuteSqlCommandAsync(releaseLockQuery);
                    }
                }
            }
        }

        private string GetLockQuery(string sectionName)
        {
            string resourceToLock = GetResourceToLock(sectionName);
            return string.Format("EXEC sp_getapplock @Resource = '{0}', @LockMode = 'Exclusive', @LockOwner = 'Session';", resourceToLock);
        }

        private string GetReleaseLockQuery(string sectionName)
        {
            string resourceToLock = GetResourceToLock(sectionName);
            return string.Format("EXEC sp_releaseapplock  @Resource = '{0}', @LockOwner = 'Session';", resourceToLock);
        }

        private string GetResourceToLock(string sectionName)
        {
            return string.Format("{0}_{1}", _instanceName, sectionName);
        }
    }
}
