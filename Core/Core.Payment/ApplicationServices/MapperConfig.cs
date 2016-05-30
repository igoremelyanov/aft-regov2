using AutoMapper;
using Bank = AFT.RegoV2.Core.Payment.Interface.Data.Bank;
using BankAccount = AFT.RegoV2.Core.Payment.Interface.Data.BankAccount;
using BankAccountType = AFT.RegoV2.Core.Payment.Interface.Data.BankAccountType;
using OfflineDeposit = AFT.RegoV2.Core.Payment.Interface.Data.OfflineDeposit;
using PaymentLevel = AFT.RegoV2.Core.Payment.Interface.Data.PaymentLevel;
using PlayerBankAccount = AFT.RegoV2.Core.Payment.Interface.Data.PlayerBankAccount;
using PaymentGatewaySettings = AFT.RegoV2.Core.Payment.Interface.Data.PaymentGatewaySettings;
using OfflineWithdraw = AFT.RegoV2.Core.Payment.Interface.Data.OfflineWithdraw;
using Country = AFT.RegoV2.Core.Payment.Interface.Data.Country;
using PaymentSettings = AFT.RegoV2.Core.Payment.Interface.Data.PaymentSettings;
using VipLevel = AFT.RegoV2.Core.Payment.Interface.Data.VipLevel;
using Deposit = AFT.RegoV2.Core.Payment.Interface.Data.DepositDto;
using PlayerPaymentLevel = AFT.RegoV2.Core.Payment.Interface.Data.PlayerPaymentLevel;
using TransferSettings = AFT.RegoV2.Core.Payment.Interface.Data.TransferSettings;
using OfflineWithdrawalHistory = AFT.RegoV2.Core.Payment.Interface.Data.OfflineWithdrawalHistory;
using Currency = AFT.RegoV2.Core.Payment.Interface.Data.Currency;
using CurrencyExchange = AFT.RegoV2.Core.Payment.Interface.Data.CurrencyExchange;
using Licensee = AFT.RegoV2.Core.Payment.Interface.Data.Licensee;
using BrandCurrency = AFT.RegoV2.Core.Payment.Interface.Data.BrandCurrency;
namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    internal static class MapperConfig
    {
        internal static void CreateMap()
        {
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.Brand, AFT.RegoV2.Core.Payment.Interface.Data.Brand>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.Player, AFT.RegoV2.Core.Payment.Interface.Data.Player>()
                .ForMember(dest=>dest.FullName, opt =>opt.Ignore())
                .ForMember(dest => dest.CurrentBankAccount, opt =>
                {
                    opt.MapFrom(src => src.CurrentBankAccount);
                    opt.ExplicitExpansion();
                });

            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.PaymentGatewaySettings, PaymentGatewaySettings>()
                .ForMember(dest => dest.LicenseeId, opt => opt.MapFrom(data => data.Brand.LicenseeId))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(data => data.Brand.Id))
                .ForMember(dest => dest.PaymentLevels, opt =>
                {
                    opt.MapFrom(src => src.PaymentLevels);
                    opt.ExplicitExpansion();
                });

            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.PaymentLevel, PaymentLevel>()
                .ForMember(dest => dest.BankAccounts, opt =>
                {
                    opt.MapFrom(src => src.BankAccounts);
                    opt.ExplicitExpansion();
                })
                .ForMember(dest => dest.PaymentGatewaySettings, opt =>
                {
                    opt.MapFrom(src => src.PaymentGatewaySettings);
                    opt.ExplicitExpansion();
                });

            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.Bank, Bank>()
                .ForMember(dest=>dest.Accounts, opt =>
                {
                    opt.MapFrom(src => src.Accounts);
                    opt.ExplicitExpansion();
                });

            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.BankAccount, BankAccount>()
                .ForMember(dest => dest.PaymentLevels, opt =>
                {
                    opt.MapFrom(src => src.PaymentLevels);
                    opt.ExplicitExpansion();
                });

            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.BankAccountType, BankAccountType>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.PlayerBankAccount, PlayerBankAccount>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.OfflineDeposit, OfflineDeposit>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.OfflineWithdraw, OfflineWithdraw>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.OfflineWithdrawalHistory, OfflineWithdrawalHistory>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.Currency, Currency>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.CurrencyExchange, CurrencyExchange>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.Country, Country>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.PaymentSettings, PaymentSettings>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.PlayerPaymentLevel, PlayerPaymentLevel>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.TransferSettings, TransferSettings>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.VipLevel, VipLevel>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.Deposit, Deposit>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.Licensee, Licensee>();
            Mapper.CreateMap<AFT.RegoV2.Core.Payment.Data.BrandCurrency, BrandCurrency>();
        }
    }
}
