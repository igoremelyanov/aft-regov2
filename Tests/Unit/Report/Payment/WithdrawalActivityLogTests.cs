using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Report.Payment
{
    public class WithdrawalActivityLogTests : AdminWebsiteUnitTestsBase
    {
        private IPlayerRepository _playerRepository;
        private IServiceBus _serviceBus;

        public override void BeforeEach()
        {
            base.BeforeEach();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();
            _playerRepository = Container.Resolve<IPlayerRepository>();
            _serviceBus = Container.Resolve<IServiceBus>();

            Container.Resolve<PlayerActivityLogWorker>().Start();
        }

        [Test]
        public void Can_log_withdrawal_wager_checked()
        {
            var @event = new WithdrawalWagerChecked();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand);
        }

        [Test]
        public void Can_log_withdrawal_created()
        {
            var @event = new WithdrawalCreated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand);
        }

        [Test]
        public void Can_log_withdrawal_investigated()
        {
            var @event = new WithdrawalInvestigated();
            _serviceBus.PublishMessage(@event);
            AssertAdminActivityLog(@event, AdminActivityLogCategory.Brand);
        }

        private void AssertAdminActivityLog(IDomainEvent @event, AdminActivityLogCategory category, string performedBy = "SuperAdmin")
        {
            Assert.AreEqual(1, _playerRepository.PlayerActivityLog.Count());
//            var record = _reportRepository.AdminActivityLog.Single();
//            Assert.AreEqual(category, record.Category);
//            Assert.AreEqual(performedBy, record.PerformedBy);
//            Assert.AreEqual(@event.EventCreated.Date, record.DatePerformed.Date);
//            Assert.AreEqual(@event.GetType().Name.SeparateWords(), record.ActivityDone);
        }
    }
}
