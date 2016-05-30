using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using FluentValidation.Results;
using OfflineDeposit = AFT.RegoV2.Core.Payment.Interface.Data.OfflineDeposit;
using AutoMapper;
namespace AFT.RegoV2.Core.Payment.ApplicationServices
{   
    public class OfflineDepositQueries : IApplicationService, IOfflineDepositQueries
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IPlayerQueries _playerQueries;

        static OfflineDepositQueries()
        {
            MapperConfig.CreateMap();
        }

        public OfflineDepositQueries(IPaymentRepository paymentRepository, IPaymentQueries paymentQueries, IPlayerQueries playerQueries)
        {
            _paymentRepository = paymentRepository;
            _paymentQueries = paymentQueries;
            _playerQueries = playerQueries;
        }

        public IEnumerable<OfflineDeposit> GetPendingDeposits(Guid playerId)
        {
           var query = _paymentRepository.OfflineDeposits
                .Where(offlineDeposit => offlineDeposit.PlayerId == playerId)
                .Where(
                    offlineDeposit =>
                        offlineDeposit.Status == OfflineDepositStatus.New || offlineDeposit.Status == OfflineDepositStatus.Processing ||
                        offlineDeposit.Status == OfflineDepositStatus.Unverified || offlineDeposit.Status == OfflineDepositStatus.Verified);
           return Mapper.Map<IEnumerable<OfflineDeposit>>(query);
        }

        public OfflineDeposit GetOfflineDeposit(Guid id)
        {
            var query = _paymentRepository.OfflineDeposits
                .Single(offlineDeposit => offlineDeposit.Id == id);
            return Mapper.Map<OfflineDeposit>(query);
        }
        
        public ValidationResult GetValidationResult(OfflineDepositRequest request)
        {
            var validator = new OfflineDepositValidator(_paymentRepository);

            return validator.Validate(request);
        }
    }
}
