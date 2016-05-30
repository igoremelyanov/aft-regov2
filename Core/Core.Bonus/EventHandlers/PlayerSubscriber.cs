using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using Microsoft.Practices.Unity;
using Player = AFT.RegoV2.Bonus.Core.Data.Player;

namespace AFT.RegoV2.Bonus.Core.EventHandlers
{
    public class PlayerSubscriber
    {
        private readonly IUnityContainer _container;

        public PlayerSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(PlayerContactVerified @event)
        {
            var repository = _container.Resolve<IBonusRepository>();

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = repository.GetLockedPlayer(@event.PlayerId);
                if (@event.ContactType == ContactType.Mobile) 
                    player.VerifyMobileNumber();
                if (@event.ContactType == ContactType.Email) 
                    player.VerifyEmailAddress();

                if (player.Data.IsEmailVerified && player.Data.IsMobileVerified)
                {
                    var bonusCommands = _container.Resolve<BonusCommands>();
                    bonusCommands.ProcessFirstBonusRedemptionOfType(player, BonusType.MobilePlusEmailVerification);
                }

                repository.SaveChanges();
                scope.Complete();
            }
        }

        public void Handle(PlayerRegistered @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();
            var bonusCommands = _container.Resolve<BonusCommands>();
            var bonusQueries = _container.Resolve<BonusQueries>();

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = bonusRepository.Players.SingleOrDefault(p => p.Id == @event.PlayerId);
                if (player != null)
                    throw new RegoException($"Player is already saved. Id: {@event.PlayerId}");

                var brand = bonusRepository.Brands.SingleOrDefault(b => b.Id == @event.BrandId);
                if (brand == null)
                    throw new RegoException($"Unable to find a brand with Id: {@event.BrandId}");

                player = new Player(@event, brand);
                if (@event.ReferralId.HasValue)
                {
                    var referrer = bonusRepository.Players.Single(p => p.ReferralId == @event.ReferralId.Value);
                    player.ReferredBy = referrer.Id;
                    var referralBonus =
                        bonusQueries.GetQualifiedBonuses(referrer.Id, BonusType.ReferFriend).FirstOrDefault();
                    if (referralBonus != null)
                    {
                        player.ReferredWith =
                            bonusRepository.Bonuses.Single(
                                b => b.Id == referralBonus.Id && b.Version == referralBonus.Version);
                        bonusCommands.RedeemBonus(referrer.Id, referralBonus.Id);
                    }
                }

                bonusRepository.Players.Add(player);
                bonusRepository.SaveChanges();

                var verificationBonus =
                    bonusQueries.GetQualifiedBonuses(player.Id, BonusType.MobilePlusEmailVerification)
                        .FirstOrDefault();
                if (verificationBonus != null)
                    bonusCommands.RedeemBonus(player.Id, verificationBonus.Id, new RedemptionParams());

                scope.Complete();
            }
        }
    }
}