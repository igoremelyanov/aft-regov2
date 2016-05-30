using System.Linq;
using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Messaging.Data;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class MessagingTestsBase : AdminWebsiteUnitTestsBase
    {
        protected IMessagingRepository MessagingRepository { get; set; }
        protected IMessageTemplateQueries MessageTemplateQueries { get; set; }
        protected IMessageTemplateCommands MessageTemplateCommands { get; set; }
        protected IMessageTemplateService MessageTemplateService { get; set; }
        protected MessagingTestHelper MessagingTestHelper { get; set; }
        protected BrandTestHelper BrandTestHelper { get; set; }
        protected Brand Brand { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();

            MessagingRepository = Container.Resolve<IMessagingRepository>();
            MessageTemplateQueries = Container.Resolve<IMessageTemplateQueries>();
            MessageTemplateCommands = Container.Resolve<IMessageTemplateCommands>();
            MessageTemplateService = Container.Resolve<IMessageTemplateService>();
            MessagingTestHelper = Container.Resolve<MessagingTestHelper>();
            BrandTestHelper = Container.Resolve<BrandTestHelper>();
            
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            BrandTestHelper.CreateBrand(isActive: true);
            Brand = MessagingRepository.Brands.First();
        }
    }
}
