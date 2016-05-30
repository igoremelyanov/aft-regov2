using System;
using System.Net;

namespace AFT.RegoV2.MemberApi.Interface.Proxy
{
    public class MemberApiProxyException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public MemberApiException Exception { get; private set; }

        public MemberApiProxyException(MemberApiException exception, HttpStatusCode code) : base(exception.ErrorMessage)
        {
            StatusCode = code;
            Exception = exception;
        }
    }
}
