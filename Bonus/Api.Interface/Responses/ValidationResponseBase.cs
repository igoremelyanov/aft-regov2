using System.Collections.Generic;

namespace AFT.RegoV2.Bonus.Api.Interface.Responses
{
    public class ValidationResponseBase
    {
        public bool Success { get; set; }
        public List<ValidationError> Errors { get; set; }
    }

    public class ValidationError
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
    }
}