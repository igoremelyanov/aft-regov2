using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings;
using LocalBrand = AFT.RegoV2.Core.Fraud.Interface.Data.Brand;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud
{
    public class FraudRepository : DbContext, IFraudRepository, ISeedable
    {
        #region Constructors

        static FraudRepository()
        {
            Database.SetInitializer(new FraudRepositoryRepositoryInitializer());
        }

        public FraudRepository()
            : base("name=Default")
        {
        }

        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new BrandMap());
            modelBuilder.Configurations.Add(new RiskLevelMap());
            modelBuilder.Configurations.Add(new PaymentLevelMap());
            modelBuilder.Configurations.Add(new WinningRuleMap());
            modelBuilder.Configurations.Add(new PlayerRiskLevelMap());
            modelBuilder.Configurations.Add(new WagerConfigurationMap());
            modelBuilder.Configurations.Add(new BonusMap());
            modelBuilder.Configurations.Add(new PaymentMethodMap());
            modelBuilder.Configurations.Add(new RiskProfileConfigurationMap());
            modelBuilder.Configurations.Add(new AutoVerificationCheckConfigurationMap());
            modelBuilder.Configurations.Add(new DuplicateMechanismMap());
            modelBuilder.Configurations.Add(new PlayerMap());
            modelBuilder.Configurations.Add(new MatchingResultMap());
            modelBuilder.Configurations.Add(new WithdrawalVerificationLogMap());
            modelBuilder.Configurations.Add(new SignUpFraudTypeMap());
            modelBuilder.Configurations.Add(new MatchingCriteriaMap());
            modelBuilder.Configurations.Add(new VipLevelMap());
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        public void Seed()
        {
            #region PaymentMethods

            var enumType = typeof(Core.Common.Data.Payment.PaymentMethod);
            foreach (var value in Enum.GetValues(enumType))
            {
                var id = (int)value;
                if (PaymentMethods.All(o => o.Id != id))
                {
                    PaymentMethods.Add(new PaymentMethod
                    {
                        Key = Guid.NewGuid(),
                        Id = id,
                        Code = Enum.GetName(enumType, value)
                    });
                }
            }
            SaveChanges();

            #endregion

            #region MatchingCriterias

            enumType = typeof(MatchingCriteriaEnum);
            foreach (var value in Enum.GetValues(enumType))
            {
                var id = (int)value;
                if (PaymentMethods.All(o => o.Id != id))
                {
                    MatchingCriterias.Add(new MatchingCriteria
                    {
                        Key = Guid.NewGuid(),
                        Id = (MatchingCriteriaEnum)id,
                        Code = Enum.GetName(enumType, value)
                    });
                }
            }
            
            SaveChanges();

            #endregion
        }

        #region IFraudRepository Members

        public virtual IDbSet<RiskLevel> RiskLevels { get; set; }
        public virtual IDbSet<WinningRule> WinningRules { get; set; }
        public virtual IDbSet<LocalBrand> Brands { get; set; }
        public virtual IDbSet<VipLevel> VipLevels { get; set; }
        public virtual IDbSet<PlayerRiskLevel> PlayerRiskLevels { get; set; }
        public virtual IDbSet<WithdrawalVerificationLog> WithdrawalVerificationLogs { get; set; }
        public virtual IDbSet<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations { get; set; }
        public IDbSet<WagerConfiguration> WagerConfigurations { get; set; }
        public IDbSet<Core.Fraud.Interface.Data.Bonus> Bonuses { get; set; }
        public IDbSet<PaymentMethod> PaymentMethods { get; set; }
        public IDbSet<MatchingCriteria> MatchingCriterias { get; set; }
        public IDbSet<RiskProfileConfiguration> RiskProfileConfigurations { get; set; }
        public virtual IDbSet<PaymentLevel> PaymentLevels { get; set; }
        public virtual IDbSet<DuplicateMechanismConfiguration> DuplicateMechanismConfigurations { get; set; }
        public virtual IDbSet<Core.Fraud.Data.Player> Players { get; set; }
        public virtual IDbSet<MatchingResult> MatchingResults { get; set; }
        public virtual IDbSet<SignUpFraudType> SignUpFraudTypes { get; set; }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        #endregion
    }
}