using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Infrastructure.DataAccess;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Shared.Synchronization;
using AFT.RegoV2.Shared.Utils;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using Topshelf;
using Topshelf.Logging;

using WinService.Workers;

namespace AFT.RegoV2.WinService
{
    internal class Program
    {
        private static IUnityContainer _container;
        private static ILog _logger;

        private const int WinServiceTimeoutInSeconds = 180;

        private static string WinServiceName
        {
            get { return ConfigurationManager.AppSettings["WinServiceName"]; }
        }

        private static int Main(string[] args)
        {
            _container = new WinServiceContainerFactory().CreateWithRegisteredTypes();
            
            _logger = _container.Resolve<ILog>();

            var exitCode = HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<CompositeWorker>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(x => _container.Resolve<CompositeWorker>());
                    serviceConfigurator.WhenStarted(x =>
                    {
                        InitializeAndSeedDatabases();
                        SetSystemIdentity();
                        x.Start();
                    });
                    serviceConfigurator.WhenStopped(x => x.Stop());
                });

                hostConfigurator.SetStartTimeout(TimeSpan.FromSeconds(WinServiceTimeoutInSeconds));
                hostConfigurator.UseNLog();
                hostConfigurator.StartAutomatically();
                hostConfigurator.SetDescription("AFT REGOv2 background tasks execution service.");
                hostConfigurator.SetDisplayName(WinServiceName);
                hostConfigurator.SetServiceName(WinServiceName);
            });

            return (int)exitCode;
        }

        private static void InitializeAndSeedDatabases()
        {
            _logger.Debug("Initializing databases ...");

            var synchronizationService = _container.Resolve<SynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                // Required for seeding purposes
                var adminIdentity = new ClaimsIdentity("WinService");
                adminIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, RoleIds.SuperAdminId.ToString()));
                adminIdentity.AddClaim(new Claim(ClaimTypes.Name, "SuperAdmin"));
                Thread.CurrentPrincipal = new ClaimsPrincipal(adminIdentity);

                InitializeAllRepositories();
                SeedIndividualRepositories();
                RunApplicationSeeder();
                RunApplicationSeederSeedSessionStore();
            });

            _logger.Debug("Initializing databases completed.");
        }

        // this method creates basic structure for all repositories by mostly creating empty tables, keys, indexes
        private static void InitializeAllRepositories()
        {
            _logger.Debug("Initializing repositories ...");

            AppDomain.CurrentDomain.GetRegoTypes()
                    .Where(t => t.IsClass && t.IsDescendentOf(typeof(DbContext)))
                    .ForEach(t =>
                    {
                        using (var repository = (DbContext)_container.Resolve(t))
                        {
                            _logger.Debug("Initializing " + repository.GetType().Name);
                            repository.Database.Initialize(false);
                        }
                    });

            _logger.Debug("Initializing repositories completed.");
        }

        //this step is responsible for creating basic entities so that system can function properly
        private static void RunApplicationSeeder()
        {
            _logger.Debug("Running ApplicationSeeder ...");

            _container.Resolve<ApplicationSeeder>().Seed();

            _logger.Debug("Running ApplicationSeeder completed.");
        }

        private static void RunApplicationSeederSeedSessionStore()
        {
            _logger.Debug("Running ApplicationSeeder (Initializing SQL Server Session Store) ...");

            _container.Resolve<ApplicationSeeder>().InitializeSqlServerSessionStore();

            _logger.Debug("Running ApplicationSeeder (Initializing SQL Server Session Store) completed.");
        }

        //each individual repository may have some custom seeding logic which is not possible to describe through Fluent interface
        private static void SeedIndividualRepositories()
        {
            _logger.Debug("Seeding databases ...");

            AppDomain.CurrentDomain.GetRegoTypes()
                .Where(t => t.IsClass && t.IsDescendentOf(typeof(ISeedable)))
                .OrderByDescending(x => x.AssemblyQualifiedName)
                .ForEach(seedableRepository =>
                {
                    var seedable = (ISeedable)_container.Resolve(seedableRepository);
                    _logger.Debug("Seeding " + seedableRepository.Name);
                    seedable.Seed();
                });
            _logger.Debug("Seeding databases completed.");
        }

        /// <summary>
        /// Login for the System actor
        /// </summary>
        private static void SetSystemIdentity()
        {
            _logger.Debug("Signing in System actor ...");

            //not using claims identity provider, because System actor is not seeded at this moment
            var systemIdentity = new ClaimsIdentity("WinService");
            systemIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, RoleIds.SystemId.ToString()));
            systemIdentity.AddClaim(new Claim(ClaimTypes.Name, "System"));
            AppDomain.CurrentDomain.SetThreadPrincipal(new ClaimsPrincipal(systemIdentity));

            _logger.Debug("System actor signed in");
        }
    }
}