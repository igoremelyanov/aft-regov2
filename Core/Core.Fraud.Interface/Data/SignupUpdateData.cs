using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices.Data
{
    public class SignupUpdateData
    {
        public IEnumerable<SignupUpdateItem> Data { get; set; }
        public SignupUpdateAction Action { get; set; }
        public SystemAction? Sanction { get; set; }
        public string Remarks { get; set; }
    }

    public class SignupUpdateItem
    {
        public Guid PlayerId { get; set; }
        public Guid FraudTypeId { get; set; }

    }

    public enum SignupUpdateAction
    {
        Apply = 0,
        Remove = 1,
        New = 2
    }
}