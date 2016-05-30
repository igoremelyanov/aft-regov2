namespace FakeUGS.Core.Interfaces
{
    public interface IAuthTokenHolder
    {
        string AuthToken { get; set; }

        string PlayerIpAddress { get; set; }
    }
    
    public interface IGameApiBatchRequest
    {
    }

}
