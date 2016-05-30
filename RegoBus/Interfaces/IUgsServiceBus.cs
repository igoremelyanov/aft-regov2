using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.RegoBus.Interfaces
{
    public interface IUgsServiceBus : IServiceBus
    {
        void PublishExternalMessage<T>(T message) where T : class;
    }
}
