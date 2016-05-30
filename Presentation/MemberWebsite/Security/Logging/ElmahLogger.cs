using System;
using System.Collections;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Interface.Security;
using AutoMapper;
using Elmah;
using System.Threading.Tasks;
using AFT.RegoV2.Shared.Logging;

namespace AFT.RegoV2.MemberWebsite.Security.Logging
{
    public class ElmahErrorLogger : ErrorLog
    {
        private readonly MemberApiProxy _serviceProxy;

        static ElmahErrorLogger()
        {
            Mapper.CreateMap<Error, ApplicationErrorRequest>();
        }

        public ElmahErrorLogger(IDictionary config)
        {
            var appSettings = new AppSettings();
            _serviceProxy = new MemberApiProxy(appSettings.MemberApiUrl.ToString(), Guid.NewGuid().ToString());
        }

        public override string Log(Error error)
        {
            var logger = new LogDecorator();
            logger.Error(string.Format("Unhandled exception in MemberWebsite application: '{0}'", error.Message), error.Exception);

            var request = Mapper.Map<ApplicationErrorRequest>(error);
            Task.Run(() => _serviceProxy.ApplicationErrorAsync(request));
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