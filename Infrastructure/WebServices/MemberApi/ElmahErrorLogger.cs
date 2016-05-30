using System;
using System.Collections;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Interface.Security;
using Microsoft.Practices.Unity;
using AutoMapper;
using Elmah;

namespace AFT.RegoV2.MemberApi
{
    public class ElmahErrorLogger : ErrorLog
    {
        public ElmahErrorLogger(IDictionary config)
        {
        }

        public override string Log(Error error)
        {
            var logger = Startup.Container.Resolve<ILog>();
            logger.Error(string.Format("Unhandled exception in MemberApi application: '{0}'", error.Message), error.Exception);

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