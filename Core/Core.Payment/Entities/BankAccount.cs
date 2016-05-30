using System;
using System.Collections.ObjectModel;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Entities
{
    public class BankAccount
    {
        private readonly Data.BankAccount _data;

        public BankAccount(Data.BankAccount data)
        {
            _data = data;
        }

        public void Activate(string user, string remarks)
        {
            _data.Updated = DateTimeOffset.Now.ToBrandOffset(_data.Bank.Brand.TimezoneId);
            _data.UpdatedBy = user;
            _data.Remarks = remarks;
            _data.Status = BankAccountStatus.Active;
        }

        public void Deactivate(string user, string remarks)
        {
            _data.PaymentLevels = new Collection<PaymentLevel>();
            _data.Updated = DateTimeOffset.Now.ToBrandOffset(_data.Bank.Brand.TimezoneId);
            _data.UpdatedBy = user;
            _data.Remarks = remarks;
            _data.Status = BankAccountStatus.Pending;
        }
    }
}