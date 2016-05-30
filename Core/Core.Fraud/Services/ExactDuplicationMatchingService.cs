using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class ExactDuplicationMatchingService : IDuplicationMatchingService
    {
        private readonly IUnityContainer _container;

        public ExactDuplicationMatchingService(IUnityContainer container)
        {
            _container = container;
        }

        public void Match(Guid id)
        {
            var repository = _container.Resolve<IFraudRepository>();

            var player = repository.Players.SingleOrDefault(x => x.Id == id);
            if (player == null)
                return;

            var allMatchedPlayers = repository
                .Players
                .AsNoTracking()
                .Where(x => string.Compare(x.FirstName, player.FirstName, StringComparison.OrdinalIgnoreCase) == 0
                            || string.Compare(x.LastName, player.LastName, StringComparison.OrdinalIgnoreCase) == 0
                            || string.Compare(x.Username, player.Username, StringComparison.OrdinalIgnoreCase) == 0
                            || string.Compare(x.Phone, player.Phone, StringComparison.OrdinalIgnoreCase) == 0
                            || string.Compare(x.IPAddress, player.IPAddress, StringComparison.OrdinalIgnoreCase) == 0
                            || string.Compare(x.Email, player.Email, StringComparison.OrdinalIgnoreCase) == 0
                            || string.Compare(x.Address, player.Address, StringComparison.OrdinalIgnoreCase) == 0
                            || x.DateRegistered == player.DateRegistered
                            || string.Compare(x.ZipCode, player.ZipCode, StringComparison.OrdinalIgnoreCase) == 0)
                .Where(x => x.Id != id)
                .ToList();

            if (allMatchedPlayers.Any())
            {
                foreach (var existedPlayer in allMatchedPlayers)
                {
                    var result = repository.MatchingResults
                        .SingleOrDefault(o => o.FirstPlayerId == id
                                              && o.SecondPlayerId == existedPlayer.Id);

                    if (result == null)
                        result = new MatchingResult
                        {
                            FirstPlayerId = id,
                            SecondPlayerId = existedPlayer.Id
                        };

                    var criteries = new List<MatchingCriteriaEnum>();

                    if (FieldsAreEqual(existedPlayer.FirstName, player.FirstName))
                        criteries.Add(MatchingCriteriaEnum.FirstName);
                    if (FieldsAreEqual(existedPlayer.LastName, player.LastName))
                        criteries.Add(MatchingCriteriaEnum.LastName);
                    if (FieldsAreEqual(existedPlayer.Username, player.Username))
                        criteries.Add(MatchingCriteriaEnum.Username);
                    if (existedPlayer.DateRegistered == player.DateRegistered)
                        criteries.Add(MatchingCriteriaEnum.DateRegitered);
                    if (FieldsAreEqual(existedPlayer.IPAddress, player.IPAddress))
                        criteries.Add(MatchingCriteriaEnum.IPAddress);
                    if (FieldsAreEqual(existedPlayer.Email, player.Email))
                        criteries.Add(MatchingCriteriaEnum.Email);
                    if (FieldsAreEqual(existedPlayer.ZipCode, player.ZipCode))
                        criteries.Add(MatchingCriteriaEnum.Zipcode);
                    if (FieldsAreEqual(existedPlayer.Phone, player.Phone))
                        criteries.Add(MatchingCriteriaEnum.PhoneNumber);
                    if (FieldsAreEqual(existedPlayer.Address, player.Address))
                        criteries.Add(MatchingCriteriaEnum.Address);

                    result.MatchingCriterias = new List<MatchingCriteria>();
                    criteries.ForEach(criteriaEnum =>
                    {
                        result.MatchingCriterias.Add(new MatchingCriteria
                        {
                            Key = Guid.NewGuid(),
                            Id = criteriaEnum,
                            Code = criteriaEnum.ToString()
                        });
                    });

                    repository.MatchingResults.AddOrUpdate(result);
                }
            }

            repository.SaveChanges();
        }

        private static bool FieldsAreEqual(string field1, string field2)
        {
            if (string.IsNullOrEmpty(field1) && string.IsNullOrEmpty(field2))
                return false;

            return string.Equals(field1, field2, StringComparison.OrdinalIgnoreCase);
        }

        public void ScoreById(Guid id)
        {
            var repository = _container.Resolve<IFraudRepository>();

            var matchingCriterias = repository
                .MatchingResults
                .AsNoTracking()
                .Where(x => x.FirstPlayerId == id || x.SecondPlayerId == id);
            //ToDo: calculate score && throw event into _bus
        }
    }
}

