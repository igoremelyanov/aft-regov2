using AFT.RegoV2.Bonus.Infrastructure.DataAccess;
using AFT.RegoV2.Bonus.Core;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Bonus.Infrastructure
{
    public class BonusContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IBonusRepository, BonusRepository>(new PerHttpRequestLifetime());
        }
    }
}