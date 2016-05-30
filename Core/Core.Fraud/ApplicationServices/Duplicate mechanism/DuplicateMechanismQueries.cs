using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class DuplicateMechanismQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IFraudRepository _repository;

        public DuplicateMechanismQueries(IFraudRepository repository)
        {
            _repository = repository;
        }

        public IQueryable<DuplicateMechanismConfiguration> GetConfigurations()
        {
            return _repository.DuplicateMechanismConfigurations;
        }

        [Permission(Permissions.View, Module = Modules.DuplicateConfiguration)]
        public DuplicateMechanismDTO GetConfiguration(Guid id)
        {
            var configuration = _repository.DuplicateMechanismConfigurations
                .Include(o => o.Brand)
                .Single(o => o.Id == id);

            var dto = AutoMapper.Mapper.DynamicMap<DuplicateMechanismDTO>(configuration);
            dto.Brand = configuration.BrandId;
            dto.Licensee = configuration.Brand.LicenseeId;

            return dto;
        }

        public IEnumerable<Guid> GetUsedBrands(Guid licenseeId, Guid? configId)
        {
            var mechanismConfigurations = _repository.DuplicateMechanismConfigurations
                .Include(o => o.Brand.LicenseeId);

            var filteredConfigurations = configId.HasValue
                ? mechanismConfigurations.Where(o => o.Brand.LicenseeId == licenseeId && o.Id != configId.Value)
                : mechanismConfigurations.Where(o => o.Brand.LicenseeId == licenseeId);

            return filteredConfigurations.Select(o => o.BrandId);
        }
		
		public IQueryable<MatchingResult> GetMatchingResults(Guid playerId)
        {
            return _repository.MatchingResults
                .Where(o => o.FirstPlayerId == playerId)
                .Include(o => o.FirstPlayer)
                .Include(o => o.SecondPlayer)
                .Include(o => o.MatchingCriterias);
        }
    }
}
