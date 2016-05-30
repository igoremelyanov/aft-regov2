using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;

namespace AFT.RegoV2.RegoBus.Providers
{
    public class UgsServiceBusConnectionStringProvider : IServiceBusConnectionStringProvider
    {
        private readonly IUgsServiceBusSettingsProvider _settingsProvider;

        public UgsServiceBusConnectionStringProvider(IUgsServiceBusSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public string GetConnectionString()
        {
            return _settingsProvider.GetUgsBusConnectionString();
        }
    }
}
