namespace AFT.RegoV2.Infrastructure.DataAccess.Base
{
    public interface IRepositoryBase
    {
        bool IsDatabaseSeeded();

        int ExecuteSqlCommand(string query, bool swallowExceptions = false);
    }
}
