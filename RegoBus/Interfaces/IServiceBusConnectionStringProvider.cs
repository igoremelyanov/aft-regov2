using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.RegoBus.Interfaces
{
    public interface IServiceBusConnectionStringProvider
    {
        string GetConnectionString();
    }
}
