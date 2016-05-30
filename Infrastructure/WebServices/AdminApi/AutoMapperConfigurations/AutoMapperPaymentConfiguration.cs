using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AutoMapper;

namespace AFT.RegoV2.AdminApi.AutoMapperConfigurations
{
    public static class AutoMapperPaymentConfiguration
    {
        public static void Configure()
        {
            //Map Core Data to Api Data
            Mapper.CreateMap<Core.Common.Data.Payment.DepositType, DepositType>();
            Mapper.CreateMap<Core.Payment.Interface.Data.DepositMethod, DepositMethod>();
            Mapper.CreateMap<Core.Payment.Interface.Data.TransferType, TransferType>();
            Mapper.CreateMap<Core.Common.Data.Payment.PaymentMethod, PaymentMethod>();
            Mapper.CreateMap<Core.Common.Data.Payment.OfflineDepositStatus, OfflineDepositStatus>();
            Mapper.CreateMap<Core.Payment.Interface.Data.PaymentGateway, PaymentGateway>();
            Mapper.CreateMap<Core.Payment.Interface.Data.Bank, BankDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.LicenseeId, opt => opt.MapFrom(src => src.Brand.LicenseeId))
                .ForMember(dest => dest.LicenseeName, opt => opt.MapFrom(src => src.Brand.LicenseeName))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.Name));


            Mapper.CreateMap<Core.Payment.Interface.Data.Brand, BrandDto>();
            Mapper.CreateMap<Core.Payment.Interface.Data.Player, PlayerDto>();
            Mapper.CreateMap<Core.Payment.Interface.Data.BankAccountType, BankAccountTypeDto>();
            Mapper.CreateMap<Core.Payment.Interface.Data.BankAccount, BankAccountDto>();
            Mapper.CreateMap<AFT.RegoV2.Core.Common.Data.BankAccountStatus, BankAccountStatus>();

            Mapper.CreateMap<Core.Payment.Interface.Data.OfflineDeposit, OfflineDepositDto>();

            Mapper.CreateMap<Core.Payment.Interface.Data.DepositDto, OnlineDepositViewDataResponse>()
                .ForMember(dest => dest.DateSubmitted, opt => opt.ResolveUsing(src => src.DateSubmitted.GetNormalizedDateTime()))
                .ForMember(dest => dest.DateVerified, opt => opt.ResolveUsing(src => src.DateVerified.GetNormalizedDateTime()))
                .ForMember(dest => dest.DateRejected, opt => opt.ResolveUsing(src => src.DateRejected.GetNormalizedDateTime()))
                .ForMember(dest => dest.DepositType, opt => opt.ResolveUsing(src => src.DepositType.ToString()));
            Mapper.CreateMap<Core.Payment.Interface.Data.PaymentLevel, PaymentLevel>();

            //Map Api Request to Core Command 
            Mapper.CreateMap<ApproveOfflineDepositRequest, OfflineDepositApprove>();
            Mapper.CreateMap<ConfirmOfflineDepositRequest, OfflineDepositConfirm>();
            Mapper.CreateMap<CreateOfflineDepositRequest, OfflineDepositRequest>();
            Mapper.CreateMap<AddBankRequest, AddBankData>();
            Mapper.CreateMap<EditBankRequest, EditBankData>();
            Mapper.CreateMap<AddBankAccountRequest, AddBankAccountData>();
            Mapper.CreateMap<EditBankAccountRequest, EditBankAccountData>();

            Mapper.CreateMap<VerifyOnlineDepositRequest, Core.Payment.Interface.Data.VerifyOnlineDepositRequest>();
            Mapper.CreateMap<UnverifyOnlineDepositRequest, Core.Payment.Interface.Data.UnverifyOnlineDepositRequest>();
            Mapper.CreateMap<ApproveOnlineDepositRequest, Core.Payment.Interface.Data.ApproveOnlineDepositRequest>();
            Mapper.CreateMap<RejectOnlineDepositRequest, Core.Payment.Interface.Data.RejectOnlineDepositRequest>();
        }
    }
}