using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Base;

namespace AFT.RegoV2.AdminApi.Interface.Payment
{
    #region Dto
  
    #endregion

    #region Request/Response  
    public class VerifyPlayerBankAccountRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }

    public class RejectPlayerBankAccountRequest
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }
    public class VerifyPlayerBankAccountResponse : ValidationResponseBase
    {
    }

    public class RejectPlayerBankAccountResponse : ValidationResponseBase
    {
    }
    #endregion    
}
