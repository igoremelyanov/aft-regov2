using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using BankAccount = AFT.RegoV2.Core.Payment.Data.BankAccount;
using BankAccountType = AFT.RegoV2.Core.Payment.Interface.Data.BankAccountType;
namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class BankAccountCommands : IBankAccountCommands
    {
        private readonly IPaymentRepository _repository;
        private readonly IBrandRepository _brandRepository;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IDocumentService _documentsService;
        private readonly IEventBus _eventBus;

        public BankAccountCommands(
            IPaymentRepository repository,
            IBrandRepository brandRepository,
            IActorInfoProvider actorInfoProvider,
            IDocumentService documentsService,
            IEventBus eventBus)
        {
            _repository = repository;
            _brandRepository = brandRepository;
            _actorInfoProvider = actorInfoProvider;
            _documentsService = documentsService;
            _eventBus = eventBus;
        }

        public Guid Add(AddBankAccountData data)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AddBankAccountValidator(_repository, _brandRepository).Validate(data);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                    //throw validationResult.GetValidationError();
                }

                var brand = _repository.Brands.Single(x => x.Id == new Guid(data.BrandId));

                var bankAccount = new BankAccount
                {
                    Id = data.Id ?? Guid.NewGuid(),
                    Bank = _repository.Banks.Single(x => x.Id == data.Bank),
                    CurrencyCode = data.Currency,
                    AccountId = data.AccountId,
                    AccountName = data.AccountName,
                    AccountNumber = data.AccountNumber,
                    //AccountType = data.AccountType,
                    AccountType = _repository.BankAccountTypes.Single(x => x.Id == data.AccountType),
                    Province = data.Province,
                    Branch = data.Branch,
                    SupplierName = data.SupplierName,
                    ContactNumber = data.ContactNumber,
                    USBCode = data.USBCode,
                    //Temporrary solution for Bank Account seeding
                    PurchasedDate = DateTime.UtcNow,
                    Remarks = data.Remarks,
                    Status = BankAccountStatus.Pending,
                    Created = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                    CreatedBy = _actorInfoProvider.Actor.UserName
                };

                _repository.BankAccounts.Add(bankAccount);
                _repository.SaveChanges();

                var bankAccountAdded = new BankAccountAdded
                {
                    Id = bankAccount.Id,
                    AccountId = bankAccount.AccountId,
                    BankId = bankAccount.Bank.Id,
                    BankAccountStatus = bankAccount.Status,
                    CreatedBy = bankAccount.CreatedBy,
                    CreatedDate = bankAccount.Created,
                    EventCreated = bankAccount.Created,
                };

                _eventBus.Publish(bankAccountAdded);

                scope.Complete();

                return bankAccount.Id;
            }
        }

        public Guid AddWithFiles(AddBankAccountData data, byte[] idFrontImage, byte[] idBackImage,
            byte[] atmCardImage)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                //Server Validation
                //var validationResult = new AddBankAccountValidator(_repository, _brandRepository).Validate(data);
                //if (!validationResult.IsValid)
                //{
                //    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                //    //throw validationResult.GetValidationError();
                //}

                //var bank = _repository.Banks.Single(x => x.Id == data.Bank);
                var id = Guid.NewGuid();
                var brandId = Guid.Parse(data.BrandId);
                var licenseeId = Guid.Parse(data.LicenseeId);

                var frontImageId = SaveFile(data.IdFrontImage, idFrontImage, id, brandId, licenseeId);
                var backImageId = SaveFile(data.IdBackImage, idBackImage, id, brandId, licenseeId);
                var atmCardImageId = SaveFile(data.AtmCardImage, atmCardImage, id, brandId, licenseeId);

                var brand = _repository.Brands.Single(x => x.Id == new Guid(data.BrandId));

                var bankAccount = new BankAccount
                {
                    Id = id,
                    Bank = _repository.Banks.Single(x => x.Id == data.Bank),
                    CurrencyCode = data.Currency,
                    AccountId = data.AccountId,
                    AccountName = data.AccountName,
                    AccountNumber = data.AccountNumber,
                    AccountType = _repository.BankAccountTypes.Single(x => x.Id == data.AccountType),
                    Province = data.Province,
                    Branch = data.Branch,
                    Remarks = data.Remarks,
                    SupplierName = data.SupplierName,
                    ContactNumber = data.ContactNumber,
                    USBCode = data.USBCode,
                    PurchasedDate = DateTime.Parse(data.PurchasedDate),
                    UtilizationDate = DateTime.Parse(data.UtilizationDate),
                    ExpirationDate = DateTime.Parse(data.ExpirationDate),
                    IdFrontImage = frontImageId,
                    IdBackImage = backImageId,
                    ATMCardImage = atmCardImageId,
                    Status = BankAccountStatus.Pending,
                    Created = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId),
                    CreatedBy = _actorInfoProvider.Actor.UserName
                };

                _repository.BankAccounts.Add(bankAccount);
                _repository.SaveChanges();

                var bankAccountAdded = new BankAccountAdded
                {
                    Id = bankAccount.Id,
                    AccountId = bankAccount.AccountId,
                    BankId = bankAccount.Bank.Id,
                    BankAccountStatus = bankAccount.Status,
                    CreatedBy = bankAccount.CreatedBy,
                    CreatedDate = bankAccount.Created,
                    EventCreated = bankAccount.Created,
                    Remarks = bankAccount.Remarks
                };

                _eventBus.Publish(bankAccountAdded);

                scope.Complete();

                return bankAccount.Id;
            }
        }

        public void SaveChanges(
            EditBankAccountData bankAccountData,
            byte[] idFrontImage,
            byte[] idBackImage,
            byte[] atmCardImage
            )
        {
            //Server Validation

            var frontImageId = SaveFile(bankAccountData.IdFrontImage, idFrontImage, bankAccountData.Id);
            var backImageId = SaveFile(bankAccountData.IdBackImage, idBackImage, bankAccountData.Id);
            var atmCardImageId = SaveFile(bankAccountData.AtmCardImage, atmCardImage, bankAccountData.Id);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var bankAccount = _repository.BankAccounts
                    .Include(x => x.Bank.Brand)
                    .Single(x => x.Id == bankAccountData.Id);

                bankAccount.Bank = _repository.Banks.Single(x => x.Id == bankAccountData.Bank);
                bankAccount.CurrencyCode = bankAccountData.Currency;
                bankAccount.AccountId = bankAccountData.AccountId;
                bankAccount.AccountName = bankAccountData.AccountName;
                bankAccount.AccountNumber = bankAccountData.AccountNumber;
                //bankAccount.AccountType = bankAccountData.AccountType;
                bankAccount.AccountType = _repository.BankAccountTypes.Single(x => x.Id == bankAccountData.AccountType);
                bankAccount.Province = bankAccountData.Province;
                bankAccount.Branch = bankAccountData.Branch;
                bankAccount.Remarks = bankAccountData.Remarks;
                bankAccount.SupplierName = bankAccountData.SupplierName;
                bankAccount.ContactNumber = bankAccountData.ContactNumber;
                bankAccount.USBCode = bankAccountData.USBCode;
                bankAccount.PurchasedDate = DateTime.Parse(bankAccountData.PurchasedDate);
                bankAccount.UtilizationDate = DateTime.Parse(bankAccountData.UtilizationDate);
                bankAccount.ExpirationDate = DateTime.Parse(bankAccountData.ExpirationDate);
                //bankAccount.IdFrontImage = frontImageId;
                //bankAccount.IdBackImage = backImageId;
                //bankAccount.ATMCardImage = atmCardImageId;
                bankAccount.Updated = DateTimeOffset.Now.ToBrandOffset(bankAccount.Bank.Brand.TimezoneId);
                bankAccount.UpdatedBy = _actorInfoProvider.Actor.UserName;

                if (frontImageId != null)
                    bankAccount.IdFrontImage = frontImageId;
                if (backImageId != null)
                    bankAccount.IdBackImage = backImageId;
                if (atmCardImageId != null)
                    bankAccount.ATMCardImage = atmCardImageId;

                _repository.SaveChanges();

                var bankAccountChanged = new BankAccountEdited
                {
                    Id = bankAccount.Id,
                    AccountId = bankAccount.AccountId,
                    Name = bankAccount.AccountName,
                    Number = bankAccount.AccountNumber,
                    BankId = bankAccount.Bank.Id,
                    UpdatedBy = bankAccount.UpdatedBy,
                    UpdatedDate = bankAccount.Updated.Value,
                    BankAccountStatus = bankAccount.Status,
                    EventCreated = bankAccount.Updated.Value,
                    Remarks = bankAccount.Remarks
                };

                _eventBus.Publish(bankAccountChanged);

                scope.Complete();
            }            
        }

        public void Edit(EditBankAccountData data)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new EditBankAccountValidator(_repository, _brandRepository).Validate(data);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                    //throw validationResult.GetValidationError();
                }

                var bankAccount = _repository.BankAccounts
                                    .Include(x => x.Bank.Brand)
                                    .Single(x => x.Id == data.Id);

                bankAccount.Bank = _repository.Banks.Single(x => x.Id == data.Bank);
                bankAccount.CurrencyCode = data.Currency;
                bankAccount.AccountId = data.AccountId;
                bankAccount.AccountName = data.AccountName;
                bankAccount.AccountNumber = data.AccountNumber;
                bankAccount.AccountType = _repository.BankAccountTypes.Single(x => x.Id == data.AccountType);
                bankAccount.Province = data.Province;
                bankAccount.Branch = data.Branch;
                bankAccount.Remarks = data.Remarks;
                bankAccount.Updated = DateTimeOffset.Now.ToBrandOffset(bankAccount.Bank.Brand.TimezoneId);
                bankAccount.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();

                var bankAccountEdited = new BankAccountEdited
                {
                    Id = bankAccount.Id,
                    AccountId = bankAccount.AccountId,
                    Name = bankAccount.AccountName,
                    Number = bankAccount.AccountNumber,
                    BankId = bankAccount.Bank.Id,
                    UpdatedBy = bankAccount.UpdatedBy,
                    UpdatedDate = bankAccount.Updated.Value,
                    BankAccountStatus = bankAccount.Status,
                    EventCreated = bankAccount.Updated.Value,
                };
                _eventBus.Publish(bankAccountEdited);

                scope.Complete();
            }
        }

        public void Activate(Guid bankAccountId, string remarks)
        {
            var bankAccount = _repository.GetBankAccount(bankAccountId);

            bankAccount.Activate(_actorInfoProvider.Actor.UserName, remarks);

            _repository.SaveChanges();

            var account = _repository.BankAccounts
                //.Include(x => x.Bank.Brand)
                .Single(x => x.Id == bankAccountId);

            _eventBus.Publish(new BankAccountActivated
            {
                Id = account.Id,
                Name = account.AccountName,
                Number = account.AccountNumber,
                Remarks = remarks,
                ActivatedBy = account.UpdatedBy,
                ActivatedDate = account.Updated.Value,
                EventCreated = account.Updated.Value,
            });
        }

        public void Deactivate(Guid bankAccountId, string remarks)
        {
            var bankAccount = _repository.GetBankAccount(bankAccountId);
            bankAccount.Deactivate(_actorInfoProvider.Actor.UserName, remarks);
            _repository.SaveChanges();

            var account = _repository.BankAccounts
                //.Include(x => x.Bank.Brand)
                .Single(x => x.Id == bankAccountId);

            var bankAccountDeactivated = new BankAccountDeactivated
            {
                Id = account.Id,
                Name = account.AccountName,
                Number = account.AccountNumber,
                Remarks = remarks,
                DeactivatedBy = account.UpdatedBy,
                DeactivatedDate = account.Updated.Value,
                EventCreated = account.Updated.Value,
            };
            _eventBus.Publish(bankAccountDeactivated);
        }

        public Guid AddBankAccountType(BankAccountType bankAccountType)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AddBankAccountTypeValidator(_repository).Validate(bankAccountType);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                }

                if (bankAccountType.Id.Equals(Guid.Empty))
                {
                    bankAccountType.Id = Guid.NewGuid();
                }

                var bankAccountTypeEntity = new AFT.RegoV2.Core.Payment.Data.BankAccountType
                {
                    Id = bankAccountType.Id,
                    Name = bankAccountType.Name
                };

                _repository.BankAccountTypes.Add(bankAccountTypeEntity);
                _repository.SaveChanges();
                
                _eventBus.Publish(new BankAccountTypeAdded
                {
                    Id = bankAccountTypeEntity.Id,
                    Name = bankAccountTypeEntity.Name,
                });

                scope.Complete();

                return bankAccountType.Id;
            }
        }

        private Guid? SaveFile(string fileName, byte[] content, Guid bankAccountId)
        {
            if (content != null && content.Length > 0)
            {
                var bankAccount = _repository.BankAccounts
                    .Include(b => b.Bank)
                    .Include(br => br.Bank.Brand)
                    .Single(x => x.Id == bankAccountId);

                var streamId = _documentsService.SaveFile(fileName, content, bankAccountId, bankAccount.Bank.BrandId, bankAccount.Bank.Brand.LicenseeId);
                return streamId;
            }
            return null;
        }

        private Guid? SaveFile(string fileName, byte[] content, Guid bankAccountId, Guid brandId, Guid licenseeId)
        {
            if (content != null && content.Length > 0)
            {
                var streamId = _documentsService.SaveFile(fileName, content, bankAccountId, brandId, licenseeId);
                return streamId;
            }
            return null;
        }
    }
}
