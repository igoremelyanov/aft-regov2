using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.ViewModels.GameIntegration;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Data.Users;
using AutoMapper;
using Microsoft.Practices.Unity;
using Unity.Mvc4;
using Brand = AFT.RegoV2.Core.Brand.Interface.Data.Brand;

namespace AFT.RegoV2.AdminWebsite
{
    public static class Bootstrapper
    {
        public static IUnityContainer Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
           
            return container;
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new AdminWebsiteContainerFactory().CreateWithRegisteredTypes();

            RegisterMappings();

            return container;
        }

        public static void RegisterMappings()
        {
            Mapper.CreateMap<EditBrandRequest, Brand>().ForMember(b => b.Licensee, opt => opt.Ignore());
            
            RegisterUserMappings();

            RegisterIpRegulationMappings();

            RegisterGameProviderMappings();
        }

        public static void RegisterUserMappings()
        {
            Mapper.CreateMap<AddAdminData, Admin>()
                .ForMember(dest => dest.AllowedBrands, opt => opt.Ignore())
                .ForMember(dest => dest.Currencies, opt => opt.Ignore());

            Mapper.CreateMap<Admin, AddAdminData>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(data => data.Role.Id))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(data => data.Role.Name))
                .ForMember(dest => dest.AllowedBrands, opt => opt.Ignore())
                .ForMember(dest => dest.Currencies, opt => opt.Ignore());
        }

        public static void RegisterIpRegulationMappings()
        {
            Mapper.CreateMap<EditAdminIpRegulationData, AdminIpRegulation>();
            Mapper.CreateMap<AdminIpRegulation, EditAdminIpRegulationData>();

            Mapper.CreateMap<AddBrandIpRegulationData, BrandIpRegulation>();
            Mapper.CreateMap<BrandIpRegulation, AddBrandIpRegulationData>();

            Mapper.CreateMap<EditBrandIpRegulationData, BrandIpRegulation>();
            Mapper.CreateMap<BrandIpRegulation, EditBrandIpRegulationData>();
        }
        
        private static void RegisterGameProviderMappings()
        {
            Mapper.CreateMap<EditGameProviderModel, GameProvider>();
        }

    }
}