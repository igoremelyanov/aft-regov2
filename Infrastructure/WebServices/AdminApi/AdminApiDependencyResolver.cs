using AFT.RegoV2.Infrastructure.DependencyResolution;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminApi
{
    public interface IAdminApiDependencyResolver
    {
        IUnityContainer Container { get; }
    }

    public class AdminApiDependencyResolver : IAdminApiDependencyResolver
    {
        public static readonly IAdminApiDependencyResolver Default = new AdminApiDependencyResolver();

        private AdminApiDependencyResolver()
        {
        }

        IUnityContainer IAdminApiDependencyResolver.Container { get; } = new AdminApiContainerFactory().CreateWithRegisteredTypes();
    }

    public class AdminApiContainerFactory : ApplicationContainerFactory
    {
        public override void RegisterTypes(IUnityContainer container)
        {
            base.RegisterTypes(container);
        }
    }
}