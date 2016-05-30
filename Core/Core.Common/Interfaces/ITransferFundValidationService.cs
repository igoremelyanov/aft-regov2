using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface ITransferFundValidationService
    {
        Task<TransferFundValidationDTO> Validate(TransferFundRequest request);
    }
}
