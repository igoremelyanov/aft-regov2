using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Validators.Bank;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class BankQueries : IBankQueries
    {
        private readonly IPaymentRepository _paymentRepository;

        static BankQueries()
        {
            MapperConfig.CreateMap();
        }

        public BankQueries(
            IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public ValidationResult ValidateCanAdd(AddBankData data)
        {
            return new AddBankValidator(_paymentRepository).Validate(data);
        }

        public ValidationResult ValidateCanEdit(EditBankData data)
        {
            return new EditBankValidator(_paymentRepository).Validate(data);
        }
    }
}