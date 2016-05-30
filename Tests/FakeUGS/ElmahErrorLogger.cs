using System.Collections;
using AFT.RegoV2.Shared.Logging;
using Elmah;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.FakeUGS
{
    public class ElmahErrorLogger : ErrorLog
    {
        public ElmahErrorLogger(IDictionary config)
        {
        }

        public override string Log(Error error)
        {
            var logger = WebApiApplication.Container.Resolve<ILog>();
            logger.Error(string.Format("Unhandled exception in FakeUGS application: '{0}'", error.Message), error.Exception);

            return error.Message;
        }

        public override ErrorLogEntry GetError(string id)
        {
            return null;
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            return errorEntryList.Count;
        }
    }
}