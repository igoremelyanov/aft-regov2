using FakeUGS.Core.Classes;

namespace FakeUGS.Core.Interfaces
{
    public interface IGameApiErrorDetails
    {
        GameApiErrorCode ErrorCode { get; set; }
        string ErrorDescription { get; set; }
    }
}
