using AFT.RegoV2.AdminApi.Provider;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Data.Users;
using AutoMapper;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Core
{
    public class TestStartup : Startup
    {
        public static new IUnityContainer Container;
        protected override IUnityContainer GetUnityContainer()
        {
            return Container;
        }

        protected override OAuthAuthorizationServerProvider GetAuthorizationServerProvider()
        {
            RegisterMappings();
            Container.RegisterType<IActorInfoProvider, ActorInfoProvider>();
            return new AuthServerProvider(Container);
        }

        private void RegisterMappings()
        {
            Mapper.CreateMap<Admin, AddAdminData>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(data => data.Role.Id))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(data => data.Role.Name))
                .ForMember(dest => dest.AssignedLicensees, opt => opt.Ignore())
                .ForMember(dest => dest.AllowedBrands, opt => opt.Ignore())
                .ForMember(dest => dest.Currencies, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordConfirmation, opt => opt.Ignore());

            Mapper.CreateMap<EditAdminIpRegulationData, AdminIpRegulation>();
            Mapper.CreateMap<AdminIpRegulation, EditAdminIpRegulationData>();

            Mapper.CreateMap<AddBrandIpRegulationData, BrandIpRegulation>();
            Mapper.CreateMap<BrandIpRegulation, AddBrandIpRegulationData>();

            Mapper.CreateMap<EditBrandIpRegulationData, BrandIpRegulation>();
            Mapper.CreateMap<BrandIpRegulation, EditBrandIpRegulationData>();
        }
    }
}
