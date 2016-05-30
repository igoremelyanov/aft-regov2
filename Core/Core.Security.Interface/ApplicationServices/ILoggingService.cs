using System;
using System.Linq;
using AFT.RegoV2.Core.Security.Interface.Data;

namespace AFT.RegoV2.Core.Security.Interface.ApplicationServices
{
    public interface ILoggingService
    {
        void Log(Error error);
        Error GetError(Guid id);
        IQueryable<Error> GetErrors();
    }
}
