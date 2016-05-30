using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace AFT.RegoV2.Core.Common.Extensions
{
    public static class DbUpdateExceptionExtensions
    {
        public static bool HasDuplicatedUniqueValues(this DbUpdateException exception)
        {
            if (exception.InnerException == null || exception.InnerException.InnerException == null)
            {
                return false;
            }

            var errorNumber = ((SqlException)exception.InnerException.InnerException).Number;
            return errorNumber == 2601 || errorNumber == 2627;
        }
    }
}