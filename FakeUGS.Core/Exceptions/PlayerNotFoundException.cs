using AFT.RegoV2.Shared;

namespace FakeUGS.Core.Exceptions
{
    public class PlayerNotFoundException : RegoException
    {
        public PlayerNotFoundException(string message) : base(message) { }
    }
}