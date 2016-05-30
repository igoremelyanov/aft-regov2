using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data.Base
{
    public class ValidationResponseBase
    {
        public bool Success { get; set; }
        public IEnumerable<ValidationError> Errors { get; set; }
    }

    public class ValidationError
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
