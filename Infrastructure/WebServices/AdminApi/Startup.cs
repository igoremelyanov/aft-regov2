using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi;
using AFT.RegoV2.AdminApi.AutoMapperConfigurations;
using AFT.RegoV2.AdminApi.Interface.Bonus;
using AFT.RegoV2.AdminApi.Provider;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.ApiDataFiltering;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Shared.Synchronization;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Owin;
using Unity.WebApi;
using ComparisonOperator = AFT.RegoV2.Shared.ApiDataFiltering.ComparisonOperator;

[assembly: OwinStartup(typeof(Startup))]

namespace AFT.RegoV2.AdminApi
{
    public class Startup
    {
        public static IUnityContainer Container { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);

            Container = GetUnityContainer();

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //app.Use<InvalidLoginOwinMiddleware>();

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AuthorizationCodeExpireTimeSpan = TimeSpan.FromHours(1),
                Provider = GetAuthorizationServerProvider(),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1),
                RefreshTokenProvider = new RefreshTokenProvider()
            });

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            var config = new HttpConfiguration { DependencyResolver = new UnityDependencyResolver(Container) };

            WebApiConfig.Register(config);

            app.UseWebApi(config);

            AutoMapperPaymentConfiguration.Configure();

            var synchronizationService = Container.Resolve<SynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                var repositoryBase = Container.Resolve<IRepositoryBase>();
                if (!repositoryBase.IsDatabaseSeeded())
                    throw new RegoException(Messages.DbExceptionMessage);
            });
        }

        protected virtual OAuthAuthorizationServerProvider GetAuthorizationServerProvider()
        {
            return new AuthServerProvider(GetUnityContainer());
        }

        protected virtual IUnityContainer GetUnityContainer()
        {
            RegisterMappings();

            return AdminApiDependencyResolver.Default.Container;
        }

        private void RegisterMappings()
        {
            Mapper.CreateMap<AddAdminData, Admin>()
                .ForMember(dest => dest.AllowedBrands, opt => opt.Ignore())
                .ForMember(dest => dest.Currencies, opt => opt.Ignore());

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

            Mapper.CreateMap<SearchPackage, FilteredDataRequest>()
                .ForMember(dest => dest.Filters, opt => opt.ResolveUsing(data =>
                {
                    var singleFilters = new List<SingleFilter> { data.SingleFilter };
                    if (data.AdvancedFilter?.Rules != null)
                        singleFilters.AddRange(data.AdvancedFilter.Rules);

                    return singleFilters.Select(r => new Filter
                    {
                        Field = r.Field,
                        Comparison = (ComparisonOperator)r.Comparison,
                        Data = r.Data
                    }).ToArray();
                }));

            Mapper.CreateMap<Bonus.Core.Models.Data.Bonus, Interface.Bonus.Bonus>()
                .ForMember(dest => dest.ActiveFrom, opt => opt.MapFrom(data => Format.FormatDate(data.ActiveFrom, false)))
                .ForMember(dest => dest.ActiveTo, opt => opt.MapFrom(data => Format.FormatDate(data.ActiveTo, false)))
                .ForMember(dest => dest.DurationDays, opt => opt.ResolveUsing(data =>
                {
                    var duration = data.DurationEnd - data.ActiveFrom;
                    return duration.Days;
                }))
                .ForMember(dest => dest.DurationHours, opt => opt.ResolveUsing(data =>
                {
                    var duration = data.DurationEnd - data.ActiveFrom;
                    return duration.Hours;
                }))
                .ForMember(dest => dest.DurationMinutes, opt => opt.ResolveUsing(data =>
                {
                    var duration = data.DurationEnd - data.ActiveFrom;
                    return duration.Minutes;
                }))
                .ForMember(dest => dest.DurationStart, opt => opt.MapFrom(data => Format.FormatDate(data.DurationStart, true)))
                .ForMember(dest => dest.DurationEnd, opt => opt.MapFrom(data => Format.FormatDate(data.DurationEnd, true)));

            Mapper.CreateMap<Bonus.Core.Models.Data.Template, Template>();
            Mapper.CreateMap<Bonus.Core.Models.Data.TemplateInfo, TemplateInfo>();
            Mapper.CreateMap<Bonus.Core.Models.Data.TemplateAvailability, TemplateAvailability>()
               .ForMember(dest => dest.PlayerRegistrationDateFrom, opt => opt.MapFrom(data => Format.FormatDate(data.PlayerRegistrationDateFrom, false)))
               .ForMember(dest => dest.PlayerRegistrationDateTo, opt => opt.MapFrom(data => Format.FormatDate(data.PlayerRegistrationDateTo, false)));
            Mapper.CreateMap<Bonus.Core.Models.Data.TemplateRules, TemplateRules>();
            Mapper.CreateMap<Bonus.Core.Models.Data.TemplateTier, TemplateTier>();
            Mapper.CreateMap<Bonus.Core.Models.Data.RewardTier, RewardTier>();
            Mapper.CreateMap<Bonus.Core.Models.Data.TemplateWagering, TemplateWagering>();
            Mapper.CreateMap<Bonus.Core.Models.Data.GameContribution, GameContribution>();
            Mapper.CreateMap<Bonus.Core.Models.Data.TemplateNotification, TemplateNotification>();

            Mapper.CreateMap<Bonus.Api.Interface.Responses.BonusTemplate, BonusTemplate>();
        }
    }
}
