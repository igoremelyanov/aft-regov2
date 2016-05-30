using AFT.RegoV2.Infrastructure.DataAccess.Base;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeRepositoryBase : IRepositoryBase
    {
        public bool IsDatabaseSeeded()
        {
            // prevent UT to fail on checking database seeding state
            return true;
        }

        public int ExecuteSqlCommand(string query, bool swallowExceptions = false)
        {
            throw new System.NotImplementedException();
        }
    }
}