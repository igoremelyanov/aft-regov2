using System.Linq;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class DepositReportEventHandlers
    {
        private readonly IUnityContainer _container;
        private const string DepositRecordNotFoundMessage = "deposit record '{0}' was not found";

        public DepositReportEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(DepositSubmitted submittedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.DepositRecords.SingleOrDefault(r => r.DepositId == submittedEvent.DepositId);
            if (record != null)
                throw new RegoException(string.Format("deposit record '{0}' already exists", submittedEvent.DepositId));

            var player = _container.Resolve<PlayerQueries>().GetPlayer(submittedEvent.PlayerId);
            var brand = _container.Resolve<BrandQueries>().GetBrandOrNull(player.BrandId);
            record = new DepositRecord
            {
                Licensee = brand != null ? brand.Licensee.Name : null,
                Brand = brand != null ? brand.Name : null,
                Username = player != null ? player.Username : null,
                IsInternalAccount = player != null ? player.InternalAccount : true,
                VipLevel = player != null && player.VipLevel != null ? player.VipLevel.Code : null,
                TransactionId = submittedEvent.TransactionNumber,
                DepositId = submittedEvent.DepositId,
                PaymentMethod = submittedEvent.PaymentMethod,
                Currency = submittedEvent.CurrencyCode,
                Amount = submittedEvent.Amount,
                Status = submittedEvent.DepositType == DepositType.Offline ? OfflineDepositStatus.New.ToString() : OnlineDepositStatus.Processing.ToString(),
                Submitted = submittedEvent.Submitted,
                SubmittedBy = submittedEvent.SubmittedBy,
                DepositType = submittedEvent.DepositType.ToString(),
                BankAccountName = submittedEvent.BankAccountName,
                BankAccountId = submittedEvent.BankAccountId,
                BankName = submittedEvent.BankName,
                BankProvince = submittedEvent.BankProvince,
                BankBranch = submittedEvent.BankBranch,
                BankAccountNumber = submittedEvent.BankAccountNumber
            };
            repository.DepositRecords.Add(record);
            repository.SaveChanges();
        }

        public void Handle(DepositConfirmed confirmedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.DepositRecords.SingleOrDefault(r => r.DepositId == confirmedEvent.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, confirmedEvent.DepositId));

            record.Amount = confirmedEvent.Amount;
            record.Status = OfflineDepositStatus.Processing.ToString();
            repository.SaveChanges();
        }

        public void Handle(DepositVerified verifiedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.DepositRecords.SingleOrDefault(r => r.DepositId == verifiedEvent.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, verifiedEvent.DepositId));
            
            if(record.Status != OfflineDepositStatus.Processing.ToString())
                throw new RegoException(string.Format("deposit record '{0}' was not in 'Processing' state", verifiedEvent.DepositId));

            record.Verified = verifiedEvent.Verified;
            record.VerifiedBy = verifiedEvent.VerifiedBy;
            record.Status = OfflineDepositStatus.Verified.ToString();
            repository.SaveChanges();
        }

        public void Handle(DepositApproved approvedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.DepositRecords.SingleOrDefault(r => r.DepositId == approvedEvent.DepositId);
            if (record == null || 
                (record.DepositType==DepositType.Offline.ToString() && record.Status != OfflineDepositStatus.Verified.ToString()) ||
                (record.DepositType == DepositType.Online.ToString() &&(record.Status == OfflineDepositStatus.Approved.ToString() || record.Status == OfflineDepositStatus.Rejected.ToString())) 
                )
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, approvedEvent.DepositId));

            record.ActualAmount = approvedEvent.ActualAmount;
            record.Fee = approvedEvent.Fee;
            record.Approved = approvedEvent.Approved;
            record.ApprovedBy = approvedEvent.ApprovedBy;
            record.Status = OfflineDepositStatus.Approved.ToString();
            repository.SaveChanges();
        }

        public void Handle(DepositUnverified cancelledEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.DepositRecords.SingleOrDefault(r => r.DepositId == cancelledEvent.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, cancelledEvent.DepositId));

            record.Rejected = cancelledEvent.Unverified;
            record.RejectedBy = cancelledEvent.UnverifiedBy;
            record.Status = cancelledEvent.Status.ToString();
            repository.SaveChanges();
        }

        public void Handle(DepositRejected rejectedEvent)
        {
            var repository = _container.Resolve<IReportRepository>();
            var record = repository.DepositRecords.SingleOrDefault(r => r.DepositId == rejectedEvent.DepositId);
            if (record == null)
                throw new RegoException(string.Format(DepositRecordNotFoundMessage, rejectedEvent.DepositId));

            record.Rejected = rejectedEvent.Rejected;
            record.RejectedBy = rejectedEvent.RejectedBy;
            record.Status = OfflineDepositStatus.Rejected.ToString();
            repository.SaveChanges();
        }
    }
}
