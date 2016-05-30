using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices.Duplicate_mechanism
{
    public interface IFraudTypeCommands
    {
        void UpdatePlayer(SignupUpdateData data);
    }
}