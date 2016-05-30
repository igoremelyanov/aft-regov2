using System;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class AvcChangeStatusCommand
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }
}
