using System;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.ServiceBus;

namespace AFT.RegoV2.RegoBus.Providers
{
    public class ServiceBusConnectionStringProvider : IServiceBusConnectionStringProvider
    {

        private readonly IServiceBusSettingsProvider _settingsProvider;

        public ServiceBusConnectionStringProvider(IServiceBusSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public string GetConnectionString()
        {
            var httpPort = _settingsProvider.GetWsbHttpPort();
            var tcpPort = _settingsProvider.GetWsbTcpPort();
            var host = _settingsProvider.GetWsbHost();
            var @namespace = _settingsProvider.GetWsbNamespace();
            var username = _settingsProvider.GetWsbUsername();
            var passwordSecure = _settingsProvider.GetWsbPasswordSecure();
            var userDomain = _settingsProvider.GetWsbUserDomain();

            var connectionStringBuilder = new ServiceBusConnectionStringBuilder
            {
                ManagementPort = httpPort,
                RuntimePort = tcpPort,
                OAuthUsername = username,
                OAuthPassword = passwordSecure,
                OAuthDomain = userDomain
            };

            connectionStringBuilder.Endpoints.Add(new UriBuilder
            {
                Scheme = "sb",
                Host = host,
                Path = @namespace
            }.Uri);

            connectionStringBuilder.StsEndpoints.Add(new UriBuilder
            {
                Scheme = "https",
                Host = host,
                Port = httpPort,
                Path = @namespace
            }.Uri);

            return connectionStringBuilder.ToString();
        }
    }
}
