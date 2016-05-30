using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    class AddContentTranslationValidator : AbstractValidator<AddContentTranslationData>
    {
        public AddContentTranslationValidator(IBrandRepository repository)
        {

        }
    }
}
