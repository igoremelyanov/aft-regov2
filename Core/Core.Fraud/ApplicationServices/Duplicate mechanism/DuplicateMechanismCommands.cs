using System;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class DuplicateMechanismCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IFraudRepository _repository;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;

        public DuplicateMechanismCommands(IFraudRepository repository,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus)
        {
            _repository = repository;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
        }

        [Permission(Permissions.Create, Module = Modules.DuplicateConfiguration)]
        public void Create(DuplicateMechanismDTO data)
        {
            var config = AutoMapper.Mapper.DynamicMap<DuplicateMechanismConfiguration>(data);

            config.Id = Guid.NewGuid();
            config.BrandId = data.Brand;
            config.Brand = null;
            config.CreatedDate = DateTimeOffset.Now;
            config.CreatedBy = _actorInfoProvider.Actor.UserName;

            _repository.DuplicateMechanismConfigurations.Add(config);
            _repository.SaveChanges();

            _eventBus.Publish(new DuplicateMechanismConfigurationCreated(config));
        }

        [Permission(Permissions.Update, Module = Modules.DuplicateConfiguration)]
        public void Update(DuplicateMechanismDTO data)
        {
            var configuration = _repository.DuplicateMechanismConfigurations
                .Single(o => o.Id == data.Id);

            AutoMapper.Mapper.DynamicMap(data, configuration, typeof(DuplicateMechanismDTO), typeof(DuplicateMechanismConfiguration));
            configuration.BrandId = data.Brand;
            configuration.Brand = null;
            configuration.UpdatedDate = DateTimeOffset.Now;
            configuration.UpdatedBy = _actorInfoProvider.Actor.UserName;

            _repository.DuplicateMechanismConfigurations.AddOrUpdate(configuration);
            _repository.SaveChanges();

            _eventBus.Publish(new DuplicateMechanismConfigurationUpdated(configuration));
        }
    }
}
