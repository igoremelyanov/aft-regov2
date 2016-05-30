namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Unverified : DbMigration
    {
        public override void Up()
        {
            AddColumn("payment.OnlineDeposits", "Unverified", c => c.DateTimeOffset(precision: 7));
            AddColumn("payment.OnlineDeposits", "UnverifiedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("payment.OnlineDeposits", "UnverifiedBy");
            DropColumn("payment.OnlineDeposits", "Unverified");
        }
    }
}
