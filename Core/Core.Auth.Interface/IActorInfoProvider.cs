using AFT.RegoV2.Core.Auth.Interface.Data;

namespace AFT.RegoV2.Core.Auth.Interface
{
    public interface IActorInfoProvider
    {
        ActorInfo Actor { get; }
        bool IsActorAvailable { get; }
    }
}