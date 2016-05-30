using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Admin
{
    class AdminQueriesTests : SecurityTestsBase
    {
        [Test]
        public void Can_validate_login()
        {
            var password = TestDataGenerator.GetRandomString();
            var user = SecurityTestHelper.CreateAdmin(password: password);

            var authQueries = Container.Resolve<IAuthQueries>();

            var validationResult1 = authQueries.GetValidationResult(new LoginActor { ActorId = user.Id, Password = TestDataGenerator.GetRandomString(6) });
            var validationResult2 = authQueries.GetValidationResult(new LoginActor { ActorId = user.Id, Password = password });

            Assert.False(validationResult1.IsValid);
            Assert.True(validationResult2.IsValid);
        }
    }
}