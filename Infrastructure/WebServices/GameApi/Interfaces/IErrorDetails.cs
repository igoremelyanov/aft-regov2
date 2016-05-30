namespace AFT.RegoV2.GameApi.Interfaces
{
    using Classes;

    public interface IGameApiErrorDetails
    {
        GameApiErrorCode ErrorCode { get; set; }
        string ErrorDescription { get; set; }
    }
}
