using System;
using System.Net;

namespace AFT.RegoV2.MemberApi.Interface.Exceptions
{
    public class MemberApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public MemberApiException(string message) : base(message) { }
        public MemberApiException(string message, HttpStatusCode code) : base(message)
        {
            StatusCode = code;
        }
        public MemberApiException(string message, Exception inner) : base(message, inner) { }
    }
}
