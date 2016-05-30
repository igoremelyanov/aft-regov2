namespace AFT.RegoV2.GameApi.Interface
{
    using Classes;

    public interface IGameApiErrorDetails
    {
        GameApiErrorCode ErrorCode { get; set; }
        string ErrorDescription { get; set; }
    }
}
