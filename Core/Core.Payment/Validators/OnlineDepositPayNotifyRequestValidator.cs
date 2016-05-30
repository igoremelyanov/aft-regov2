using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Helpers;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class OnlineDepositPayNotifyRequestValidator : AbstractValidator<OnlineDepositPayNotifyRequest>
    {
        public OnlineDepositPayNotifyRequestValidator(IPaymentRepository repository,string key)
        {
            RuleFor(x => x.OrderIdOfMerchant)
                .Must(x => x != null && repository.OnlineDeposits.FirstOrDefault(y=>x == y.TransactionNumber) != null)
                .WithMessage("{\"text\": \"app:payment.onlineDeposit.transactionNumberNotExist\"}");

            Custom(data =>
            {
                var signString = data.OrderIdOfMerchant + data.OrderIdOfRouter + data.OrderIdOfGateway + data.Language + key;
                var expect = EncryptHelper.GetMD5HashInHexadecimalFormat(signString);
                return expect != data.Signature
                ? new ValidationFailure("Signature", "{\"text\": \"app:payment.onlineDeposit.signatureNotMatch\"}")
                : null; 
            });
            
        }    
    }
}
