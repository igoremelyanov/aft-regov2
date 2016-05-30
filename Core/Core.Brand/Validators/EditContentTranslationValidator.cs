using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    class EditContentTranslationValidator : AbstractValidator<EditContentTranslationData>
    {
        public EditContentTranslationValidator(IBrandRepository repository)
        {

        }
    }
}
