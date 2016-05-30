using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Common.Data.Player;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player.Mappings
{
    public class OnSiteMessageMap : EntityTypeConfiguration<OnSiteMessage>
    {
        public OnSiteMessageMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Player).WithMany().WillCascadeOnDelete(false);
        }
    }
}
