using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public class DuplicateScoreService : IDuplicateScoreService
    {
        #region Fields

        private readonly DuplicateMechanismQueries _duplicateMechanismQueries;
        private readonly IFraudRepository _fraudRepository;

        #endregion

        #region Constructors

        public DuplicateScoreService(
            IFraudRepository fraudRepository,
            DuplicateMechanismQueries duplicateMechanismQueries)
        {
            _fraudRepository = fraudRepository;
            _duplicateMechanismQueries = duplicateMechanismQueries;
        }

        #endregion

        #region Public methods

        public int ScorePlayer(Guid playerId)
        {
            var player = _fraudRepository.Players.FirstOrDefault(x => x.Id == playerId);
            if (player == null)
                throw new RegoException("Can't find required player");

            var configuration = _duplicateMechanismQueries
                .GetConfigurations()
                .FirstOrDefault(x => x.BrandId == player.BrandId);
            if (configuration == null)
                return 0;

            var matchingResults = _fraudRepository
                .MatchingResults
                .Include(o => o.MatchingCriterias)
                .AsNoTracking()
                .Where(x => x.FirstPlayerId == playerId)
                .ToList();

            var totalScore = matchingResults.Max(o => Score(o.MatchingCriterias, configuration));

            return totalScore;
        }

        public int ScorePlayer(Guid basePlayerId, Guid secondaryPlayerId, IExactScoreConfiguration configuration = null)
        {
            var basePlayer = _fraudRepository.Players.FirstOrDefault(x => x.Id == basePlayerId);
            var secondaryPlayer = _fraudRepository.Players.FirstOrDefault(x => x.Id == secondaryPlayerId);
            if (basePlayer == null || secondaryPlayer == null)
                throw new RegoException("Can't find required player");

            if (configuration == null)
                configuration = _duplicateMechanismQueries
                    .GetConfigurations()
                    .FirstOrDefault(x => x.BrandId == basePlayer.BrandId);

            if (configuration == null)
                return 0;

            var matching = _fraudRepository
                .MatchingResults
                .Include(o => o.MatchingCriterias)
                .AsNoTracking()
                .SingleOrDefault(x => (x.FirstPlayerId == basePlayerId && x.SecondPlayerId == secondaryPlayerId)
                            || (x.FirstPlayerId == secondaryPlayerId && x.SecondPlayerId == basePlayerId));

            if (matching == null)
                return 0;

            return Score(matching.MatchingCriterias, configuration);
        }

        #endregion

        #region Private methods

        private static int Score(IEnumerable<MatchingCriteria> criterias, IExactScoreConfiguration configuration)
        {
            var score = 0;
            foreach (var criteria in criterias)
            {
                if (criteria.Id == MatchingCriteriaEnum.FirstName)
                    score += configuration.FirstNameExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.LastName)
                    score += configuration.LastNameExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.Username)
                    score += configuration.UsernameExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.Address)
                    score += configuration.AddressExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.DateOfBirth)
                    score += configuration.DateOfBirthExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.Email)
                    score += configuration.EmailAddressExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.FullName)
                    score += configuration.FullNameExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.IPAddress)
                    score += configuration.SignUpIpExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.PhoneNumber)
                    score += configuration.MobilePhoneExactScore/**criteria.Count*/;
                if (criteria.Id == MatchingCriteriaEnum.Zipcode)
                    score += configuration.ZipCodeExactScore/**criteria.Count*/;
            }

            return score;
        }

        #endregion
    }

    internal class MatchingResultAggregate
    {
        #region Properties

        public MatchingCriteriaEnum Criteria { get; set; }
        public int Count { get; set; }

        #endregion
    }
}