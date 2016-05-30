using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Base
{
    [Category("Unit")]
    public abstract class UnitTestBase: BonusTestBase
    {
        protected BonusQueries BonusQueries { get; set; }
        protected Guid PlayerId { get; set; }

        protected List<BonusRedemption> BonusRedemptions => BonusRepository.GetLockedPlayer(PlayerId).Data.Wallets.First().BonusesRedeemed;

        public override void BeforeAll()
        {
            base.BeforeAll();

            BonusQueries = Container.Resolve<BonusQueries>();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();

            BonusRepository.Bonuses.ToList().ForEach(b => BonusRepository.Bonuses.Remove(b));
            BonusRepository.Templates.ToList().ForEach(b => BonusRepository.Templates.Remove(b));
            BonusRepository.Players.ToList().ForEach(b => BonusRepository.Players.Remove(b));
            BonusRepository.Brands.ToList().ForEach(b => BonusRepository.Brands.Remove(b));
            BonusRepository.Games.ToList().ForEach(b => BonusRepository.Games.Remove(b));

            CreateActiveBrandWithProducts();
            PlayerId = CreatePlayer().Id;
        }
    }
}
