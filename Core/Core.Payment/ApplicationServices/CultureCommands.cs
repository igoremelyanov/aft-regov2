using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class CultureCommands : ICultureCommands, IApplicationService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly BrandQueries _queries;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;

        public CultureCommands(
            BrandQueries queries,
            IEventBus eventBus,
            IBrandRepository brandRepository,
            IActorInfoProvider actorInfoProvider)
        {
            _queries = queries;
            _eventBus = eventBus;
            _brandRepository = brandRepository;
            _actorInfoProvider = actorInfoProvider;
        }

        [Permission(Permissions.Create, Module = Modules.LanguageManager)]
        public string Save(EditCultureData model)
        {
            var oldCode = model.OldCode;

            if (_queries.GetCultures().Any(c => c.Code == model.Code && c.Code != oldCode))
                throw new RegoException("app:common.codeUnique");

            var username = _actorInfoProvider.Actor.UserName;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var culture = new Culture
                {
                    Code = model.Code,
                    CreatedBy = username,
                    DateCreated = DateTimeOffset.UtcNow,
                    Name = model.Name,
                    NativeName = model.NativeName
                };

                _brandRepository.Cultures.Add(culture);
                _brandRepository.SaveChanges();
                _eventBus.Publish(new LanguageCreated(culture));

                scope.Complete();
            }

            return "app:language.created";
        }

        [Permission(Permissions.Update, Module = Modules.LanguageManager)]
        public string Edit(EditCultureData model)
        {
            var oldCode = model.OldCode;

            var culture = _brandRepository.Cultures.SingleOrDefault(c => c.Code == oldCode);
            if (culture == null)
                throw new RegoException("app:common.invalidId");

            if (_queries.GetCultures().Any(c => c.Code == model.Code && c.Code != oldCode))
                throw new RegoException("app:common.codeUnique");

            var username = _actorInfoProvider.Actor.UserName;
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                culture.UpdatedBy = username;
                culture.DateUpdated = DateTimeOffset.UtcNow;
                culture.Name = model.Name;
                culture.NativeName = model.NativeName;

                _brandRepository.SaveChanges();

                _eventBus.Publish(new LanguageUpdated(culture));

                scope.Complete();
            }

            return "app:language.updated";
        }
    }
}
