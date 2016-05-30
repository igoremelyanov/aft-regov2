using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Player.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class AddSecurityQuestionValidator : AbstractValidator<SecurityQuestion>
    {
        public AddSecurityQuestionValidator(IPlayerRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
            
            RuleFor(x => x.Question)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
        }
    }
}