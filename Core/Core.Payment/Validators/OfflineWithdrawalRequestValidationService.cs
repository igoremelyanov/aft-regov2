using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared;
using FluentValidation;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class OfflineWithdrawalRequestValidator : AbstractValidator<OfflineWithdrawRequest>
    {
        private readonly IUnityContainer _container;
        private readonly IPaymentRepository _repository;

        public OfflineWithdrawalRequestValidator(
            IUnityContainer container, 
            IPaymentRepository repository)
        {
            _container = container;
            _repository = repository;
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("app:payment.amountMustBeGreaterThanZero");
            RuleFor(x => x)
                .Must(x =>
                {
                    var bankAccount =
                        _repository.PlayerBankAccounts.Include(y => y.Player)
                            .Include(y => y.Bank)
                            .SingleOrDefault(y=> y.Id == x.PlayerBankAccountId);
                    return bankAccount != null;
                })
                .WithMessage("Player does not have a current and verified bank account.")
                .WithName("Amount");
            RuleFor(x => x).Must(x =>
            {
                var validationService = _container.Resolve<IOfflineWithdrawalValidationService>();
                try
                {
                    Task.Run(async () => await validationService.Validate(x)).Wait();
                }
                catch (Exception e)
                {
                    throw new RegoValidationException(e.InnerException.Message);
                }

                return true;
            }).WithName("Amount");
        }
    }
}
