using System.Linq;

namespace AFT.RegoV2.MemberApi.Interface.Common
{
    public class ValidationResult
    {
        public bool HasError => Errors.Any();

        public System.Collections.Generic.IDictionary<string, string> Errors { get; set; }
    }
}
