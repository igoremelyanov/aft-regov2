namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUnverifyDepositField : DbMigration
    {
        public override void Up()
        {
            AddColumn("payment.Deposits", "DateUnverified", c => c.DateTimeOffset(precision: 7));
            AddColumn("payment.Deposits", "UnverifiedBy", c => c.String());
            AddColumn("payment.Deposits", "UnverifyReason", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("payment.Deposits", "UnverifyReason");
            DropColumn("payment.Deposits", "UnverifiedBy");
            DropColumn("payment.Deposits", "DateUnverified");
        }
    }
}
