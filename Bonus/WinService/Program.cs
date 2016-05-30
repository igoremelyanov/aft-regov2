using System;
using System.Configuration;
using Topshelf;

namespace AFT.RegoV2.Bonus.WinService
{
    class Program
    {
        private class DummyPerformer
        {
            public void Start() { }
            public void Stop() { }
        }

        private static string WinServiceName => ConfigurationManager.AppSettings["BonusWinServiceName"];

        private static int Main(string[] args)
        {
            var exitCode = HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<DummyPerformer>(s =>
                {
                    s.ConstructUsing(name => new DummyPerformer());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                hostConfigurator.SetStartTimeout(TimeSpan.FromSeconds(45));
                hostConfigurator.UseNLog();
                hostConfigurator.StartAutomatically();
                hostConfigurator.SetDescription("AFT REGOv2 bonus background tasks execution service.");
                hostConfigurator.SetDisplayName(WinServiceName);
                hostConfigurator.SetServiceName(WinServiceName);
            });

            return (int) exitCode;
        }
    }
}
