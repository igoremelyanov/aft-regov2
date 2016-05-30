using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeActorInfoProvider : IActorInfoProvider
    {
        public FakeActorInfoProvider()
        {
            Actor = new ActorInfo();
        }

        public ActorInfo Actor { get; set; }
        public bool IsActorAvailable
        {
            get { return !string.IsNullOrWhiteSpace(Actor.UserName); }
        }
    }
}