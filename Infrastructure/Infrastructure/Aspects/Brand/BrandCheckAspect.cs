using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.AdminApi.Interface.Brand;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity.InterceptionExtension;
using BrandId = AFT.RegoV2.Core.Brand.Interface.Data.BrandId;
using EditBrandIpRegulationData = AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations.EditBrandIpRegulationData;
using Player = AFT.RegoV2.Core.Common.Data.Player.Player;
using PlayerId = AFT.RegoV2.Core.Common.Data.Player.PlayerId;
using VipLevel = AFT.RegoV2.Core.Common.Data.Player.VipLevel;
using VipLevelId = AFT.RegoV2.Core.Player.Interface.Data.VipLevelId;

namespace AFT.RegoV2.Infrastructure.Aspects
{
    public class BrandCheckAspect : IInterceptionBehavior
    {
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly ISecurityRepository _securityRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IFraudRepository _fraudRepository;
        private readonly IPlayerRepository _playerRepository;

        public BrandCheckAspect(
            IActorInfoProvider actorInfoProvider,
            ISecurityRepository securityRepository,
            IPaymentRepository paymentRepository,
            IFraudRepository fraudRepository,
            IPlayerRepository playerRepository)
        {
            _actorInfoProvider = actorInfoProvider;
            _securityRepository = securityRepository;
            _paymentRepository = paymentRepository;
            _fraudRepository = fraudRepository;
            _playerRepository = playerRepository;
        }

        private IList<Guid> GetBrandIdsOrNull(IEnumerable arguments)
        {
            var result = new List<Guid>();

            foreach (var argument in arguments)
            {
                if (argument == null)
                    continue;

                #region Payment
                //TODO:AFTREGO-4143 remove checking here
                //if (argument.GetType() == typeof(AddBankAccountData))
                //    if (_paymentRepository.Banks.Any(o => o.Id == ((AddBankAccountData)argument).Bank))
                //        result.Add(_paymentRepository.Banks.Single(o => o.Id == ((AddBankAccountData)argument).Bank).BrandId);

                //if (argument.GetType() == typeof(EditBankAccountData))
                //    if (_paymentRepository.Banks.Any(o => o.Id == ((EditBankAccountData)argument).Bank))
                //        result.Add(_paymentRepository.Banks.Single(o => o.Id == ((EditBankAccountData)argument).Bank).BrandId);

                //if (argument.GetType() == typeof(BankAccountId))
                //    if (_paymentRepository.BankAccounts.Any(b => b.Id == (BankAccountId)argument) &&
                //        _paymentRepository.BankAccounts.Single(b => b.Id == (BankAccountId)argument).Bank != null)
                //        result.Add(_paymentRepository.BankAccounts.Single(b => b.Id == (BankAccountId)argument).Bank.BrandId);

                if (argument.GetType() == typeof(EditPlayerBankAccountData))
                    if (_paymentRepository.Banks.Any(o => o.Id == ((EditPlayerBankAccountData)argument).Bank))
                        result.Add(_paymentRepository.Banks.Single(o => o.Id == ((EditPlayerBankAccountData)argument).Bank).BrandId);

                if (argument.GetType() == typeof(PlayerBankAccountId))
                    if (_paymentRepository.PlayerBankAccounts.Any(b => b.Id == (PlayerBankAccountId)argument) &&
                        _paymentRepository.PlayerBankAccounts.Single(b => b.Id == (PlayerBankAccountId)argument).Bank != null)
                        result.Add(_paymentRepository.PlayerBankAccounts.Single(b => b.Id == (PlayerBankAccountId)argument).Bank.BrandId);

                if (argument.GetType() == typeof(Exemption))
                    if (_paymentRepository.Players.Any(o => o.Id == ((Exemption)argument).PlayerId))
                        result.Add(_paymentRepository.Players.Single(o => o.Id == ((Exemption)argument).PlayerId).BrandId);

                if (argument.GetType() == typeof(OfflineWithdrawRequest))
                    if (_paymentRepository.PlayerBankAccounts.Any(b => b.Id == ((OfflineWithdrawRequest)argument).PlayerBankAccountId) &&
                        _paymentRepository.PlayerBankAccounts.Single(b => b.Id == ((OfflineWithdrawRequest)argument).PlayerBankAccountId).Bank != null)
                        result.Add(_paymentRepository.PlayerBankAccounts.Single(b => b.Id == ((OfflineWithdrawRequest)argument).PlayerBankAccountId).Bank.BrandId);

                if (argument.GetType() == typeof(OfflineWithdrawId))
                    if (_paymentRepository.OfflineWithdraws.Any(b => b.Id == (OfflineWithdrawId)argument))
                        result.Add(_paymentRepository.OfflineWithdraws.Include(x => x.PlayerBankAccount.Bank).Single(b => b.Id == ((OfflineWithdrawId)argument)).PlayerBankAccount.Bank.BrandId);

                if (argument.GetType() == typeof(OfflineDepositId))
                    if (_paymentRepository.OfflineDeposits.Any(o => o.Id == (OfflineDepositId)argument))
                        result.Add(_paymentRepository.OfflineDeposits.Single(o => o.Id == (OfflineDepositId)argument).BrandId);
                //TODO:AFTREGO-4143 remove checking here
                //if (argument.GetType() == typeof(OfflineDepositRequest))
                //    if (_paymentRepository.Players.Any(o => o.Id == ((OfflineDepositRequest)argument).PlayerId))
                //        result.Add(_paymentRepository.Players.Single(o => o.Id == ((OfflineDepositRequest)argument).PlayerId).BrandId);

                //if (argument.GetType() == typeof(OfflineDepositRequest))
                //    if (_paymentRepository.BankAccounts.Any(o => o.Id == ((OfflineDepositRequest)argument).BankAccountId) &&
                //        _paymentRepository.BankAccounts.Single(o => o.Id == ((OfflineDepositRequest)argument).BankAccountId).Bank != null)
                //        result.Add(_paymentRepository.BankAccounts.Single(o => o.Id == ((OfflineDepositRequest)argument).BankAccountId).Bank.BrandId);

                if (argument.GetType() == typeof(OfflineDepositConfirm))
                    if (_paymentRepository.Banks.Any(o => o.Id == ((OfflineDepositConfirm)argument).BankId))
                        result.Add(_paymentRepository.Banks.Single(o => o.Id == ((OfflineDepositConfirm)argument).BankId).BrandId);

                if (argument.GetType() == typeof(OfflineDepositApprove))
                    if (_paymentRepository.OfflineDeposits.Any(o => o.Id == ((OfflineDepositApprove)argument).Id))
                        result.Add(_paymentRepository.OfflineDeposits.Single(o => o.Id == ((OfflineDepositApprove)argument).Id).BrandId);

                if (argument.GetType() == typeof(EditPaymentLevel))
                    if (_paymentRepository.PaymentLevels.Any(o => o.Id == ((EditPaymentLevel)argument).Id))
                        result.Add(_paymentRepository.PaymentLevels.Single(o => o.Id == ((EditPaymentLevel)argument).Id).BrandId);

                if (argument.GetType() == typeof(PaymentSettingsId))
                    if (_paymentRepository.PaymentSettings.Any(o => o.Id == (PaymentSettingsId)argument))
                        result.Add(_paymentRepository.PaymentSettings.Single(o => o.Id == (PaymentSettingsId)argument).BrandId);

                if (argument.GetType() == typeof(SavePaymentSettingsCommand))
                    result.Add(((SavePaymentSettingsCommand)argument).Brand);
                
                if (argument.GetType() == typeof(TransferSettingsId))
                    if (_paymentRepository.TransferSettings.Any(o => o.Id == (TransferSettingsId)argument))
                        result.Add(_paymentRepository.TransferSettings.Single(o => o.Id == (TransferSettingsId)argument).BrandId);

                if (argument.GetType() == typeof(SaveTransferSettingsCommand))
                    result.Add(((SaveTransferSettingsCommand)argument).Brand);

                if (argument.GetType() == typeof (PlayerId))
                {
                   
                    if (_playerRepository.Players.Any(o => o.Id == (PlayerId)argument))
                        result.Add(_playerRepository.Players.Single(o => o.Id == (PlayerId)argument).BrandId);
                }
                if (argument.GetType() == typeof (Core.Payment.Interface.Data.PlayerId))
                {
                    if (_paymentRepository.Players.Any(o => o.Id == (Core.Payment.Interface.Data.PlayerId)argument))
                        result.Add(_paymentRepository.Players.Single(o => o.Id == (Core.Payment.Interface.Data.PlayerId)argument).BrandId);
                }

                if (argument.GetType() == typeof (Core.Fraud.Interface.Data.PlayerId))
                {
                    if(_paymentRepository.Players.Any(o => o.Id == (Core.Fraud.Interface.Data.PlayerId)argument))
                        result.Add(_paymentRepository.Players.Single(o => o.Id == (Core.Fraud.Interface.Data.PlayerId)argument).BrandId);
                }

                #endregion

                #region Fraud

                if (argument.GetType() == typeof(AVCConfigurationDTO))
                        result.Add(((AVCConfigurationDTO)argument).Brand);

                if (argument.GetType() == typeof(RiskLevelId))
                    if (_fraudRepository.RiskLevels.Any(o => o.Id == (RiskLevelId)argument))
                        result.Add(_fraudRepository.RiskLevels.Single(o => o.Id == (RiskLevelId)argument).BrandId);

                if (argument.GetType() == typeof(Core.Fraud.Interface.Data.RiskLevel))
                    result.Add(((Core.Fraud.Interface.Data.RiskLevel)argument).BrandId);

                if (argument.GetType() == typeof(WagerConfigurationId))
                    if (_fraudRepository.WagerConfigurations.Any(o => o.Id == (WagerConfigurationId)argument))
                        result.Add(_fraudRepository.WagerConfigurations.Single(o => o.Id == (WagerConfigurationId)argument).BrandId);

                if (argument.GetType() == typeof(WagerConfigurationDTO))
                    result.Add(((WagerConfigurationDTO)argument).BrandId);

                #endregion

                #region Brand

                if (argument.GetType() == typeof(BrandId))
                    result.Add(((BrandId)argument));

                if (argument.GetType() == typeof(ActivateBrandRequest))
                    result.Add(((ActivateBrandRequest)argument).BrandId);

                if (argument.GetType() == typeof(DeactivateBrandRequest))
                    result.Add(((DeactivateBrandRequest)argument).BrandId);

                if (argument.GetType() == typeof(VipLevelViewModel))
                    result.Add(((VipLevelViewModel)argument).Brand);

                #endregion

                #region Player

                if (argument.GetType() == typeof(PlayerId))
                {
                    if (_playerRepository.Players.Any(o => o.Id == (PlayerId)argument))
                        result.Add(_playerRepository.Players.Single(o => o.Id == (PlayerId)argument).BrandId);
                }

                if (argument.GetType() == typeof(Player))
                {
                    if (_playerRepository.Players.Any(o => o.Id == ((Player)argument).Id))
                        result.Add(_playerRepository.Players.Single(o => o.Id == ((Player)argument).Id).BrandId);
                }

                if (argument.GetType() == typeof(EditPlayerData))
                {
                    if (_playerRepository.Players.Any(o => o.Id == ((EditPlayerData)argument).PlayerId))
                        result.Add(_playerRepository.Players.Single(o => o.Id == ((EditPlayerData)argument).PlayerId).BrandId);
                }

                if (argument.GetType() == typeof(VipLevelId))
                {
                    if (_playerRepository.VipLevels.Any(o => o.Id == (VipLevelId)argument))
                        result.Add(_playerRepository.VipLevels.Single(o => o.Id == (VipLevelId)argument).BrandId);
                }

                if (argument.GetType() == typeof(VipLevel))
                {
                    if (_playerRepository.VipLevels.Any(o => o.Id == ((VipLevel)argument).Id))
                        result.Add(_playerRepository.VipLevels.Single(o => o.Id == ((VipLevel)argument).Id).BrandId);
                }

                if (argument.GetType() == typeof(RegistrationData))
                {
                    result.Add(new Guid(((RegistrationData)argument).BrandId));
                }

                #endregion Player

                #region Security

                if (argument.GetType() == typeof(AddBrandIpRegulationData))
                {
                    result.Add(((AddBrandIpRegulationData)argument).BrandId);
                }

                if (argument.GetType() == typeof(EditBrandIpRegulationData))
                {
                    result.Add(((EditBrandIpRegulationData)argument).BrandId);
                }

                if (argument.GetType() == typeof(BrandIpRegulationId))
                {
                    if (_securityRepository.BrandIpRegulations.Any(o => o.Id == (BrandIpRegulationId)argument))
                        result.Add(_securityRepository.BrandIpRegulations.Single(o => o.Id == ((BrandIpRegulationId)argument)).BrandId);
                }

                if (argument.GetType() == typeof(AddAdminData) && ((AddAdminData)argument).AllowedBrands != null)
                {
                    result.AddRange(((AddAdminData)argument).AllowedBrands);
                }

                if (argument.GetType() == typeof(EditAdminData) && ((EditAdminData)argument).AllowedBrands != null)
                {
                    result.AddRange(((EditAdminData)argument).AllowedBrands);
                }

                if (argument.GetType() == typeof(AdminId))
                {
                    if (_securityRepository.Admins.Any(o => o.Id == (AdminId)argument))
                        result.AddRange((_securityRepository.Admins.Single(o => o.Id == ((AdminId)argument)).AllowedBrands.Select(b => b.Id)));
                }

                #endregion
            }

            return result.Count > 0 ? result.Distinct().ToList() : null;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var brandIds = GetBrandIdsOrNull(input.Arguments);

            if (brandIds == null)
                return getNext()(input, getNext);

            if (!_actorInfoProvider.IsActorAvailable)
            {
                throw new RegoException("User must be logged in");
            }

            var admin = _securityRepository
                .Admins
                .Include(u => u.AllowedBrands)
                .SingleOrDefault(u => u.Id == _actorInfoProvider.Actor.Id);

            var allowed = admin.AllowedBrands.Count(brand => brandIds.Contains(brand.Id)) == brandIds.Count();

            if (!allowed)
            {
                throw new InsufficientPermissionsException(
                    string.Format("User \"{0}\" has insufficient permissions for the operation ",
                            _actorInfoProvider.Actor.UserName));
            }

            // Invoke the next behavior in the chain.
            var result = getNext()(input, getNext);

            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute
        {
            get { return true; }
        }
    }
}
