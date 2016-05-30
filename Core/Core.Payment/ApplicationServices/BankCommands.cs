using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;
using Bank = AFT.RegoV2.Core.Payment.Interface.Data.Bank;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{   
    public class BankCommands : IApplicationService, IBankCommands
    {
        private readonly IBankQueries _bankQueries;
        private readonly IPaymentRepository _repository;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;

        static BankCommands()
        {
            Mapper.CreateMap<AddBankData, AFT.RegoV2.Core.Payment.Data.Bank>();
            Mapper.CreateMap<EditBankData, AFT.RegoV2.Core.Payment.Data.Bank>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.Bank, Bank>();
        }

        public BankCommands(
            IBankQueries bankQueries,
            IPaymentRepository repository,
            IActorInfoProvider actorInfoProvider, 
            IEventBus eventBus)
        {
            _bankQueries = bankQueries;
            _repository = repository;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
        }

        public Guid Add(AddBankData data)
        {
            var validationResult = _bankQueries.ValidateCanAdd(data);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var bank = Mapper.Map<AFT.RegoV2.Core.Payment.Data.Bank>(data);
            bank.Id = data.Id ?? Guid.NewGuid();
            bank.Created = DateTime.Now;
            bank.CreatedBy = _actorInfoProvider.Actor.UserName;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.Banks.Add(bank);
                _repository.SaveChanges();

                var bankAdded = new BankAdded
                {
                    Id = bank.Id,
                    BankId = bank.BankId,
                    BrandId = bank.BrandId,
                    Name = bank.BankName,
                    CreatedDate = bank.Created
                };

                _eventBus.Publish(bankAdded);

                scope.Complete();
            }

            return bank.Id;
        }

        public void Edit(EditBankData data)
        {
            var validationResult = _bankQueries.ValidateCanEdit(data);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var bank = _repository.Banks.Single(x => x.Id == data.Id);
                bank = Mapper.Map(data, bank);
                bank.Updated = DateTime.Now;
                bank.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();

                var bankAdded = new BankEdited
                {
                    Id = bank.Id,
                    BankId = bank.BankId,
                    BankName = bank.BankName,
                    BrandId = bank.BrandId,
                    UpdatedDate = bank.Updated.GetValueOrDefault()
                };

                _eventBus.Publish(bankAdded);

                scope.Complete();
            }
        }
    }
}
