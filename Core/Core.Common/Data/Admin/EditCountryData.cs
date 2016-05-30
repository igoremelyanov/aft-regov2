using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Common.Data.Admin
{
    public class EditCountryData
    {
        public string OldCode { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}")] 
        [MaxLength(2, ErrorMessage = "{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{1}\"}}}}")]
        [RegularExpression(@"^[a-zA-Z0-9-_]+$", ErrorMessage = "{{\"text\": \"app:payment.codeCharError\"}}")]
        public string Code { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}")]
        [MaxLength(50, ErrorMessage = "{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{1}\"}}}}")]
        [RegularExpression(@"^[a-zA-Z0-9-_ ]+$", ErrorMessage = "{{\"text\": \"app:payment.nameCharError\"}}")]
        public string Name { get; set; }
    }
}