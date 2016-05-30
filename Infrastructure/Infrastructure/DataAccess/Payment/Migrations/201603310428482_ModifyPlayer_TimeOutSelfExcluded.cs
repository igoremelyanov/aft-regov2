namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyPlayer_TimeOutSelfExcluded : DbMigration
    {
        public override void Up()
        {
            AddColumn("payment.Players", "IsTimeOut", c => c.Boolean(nullable: false));
            AddColumn("payment.Players", "TimeOutEndDate", c => c.DateTimeOffset(precision: 7));
            AddColumn("payment.Players", "IsSelfExclude", c => c.Boolean(nullable: false));
            AddColumn("payment.Players", "SelfExcludeEndDate", c => c.DateTimeOffset(precision: 7));
            CreateIndex("payment.PlayerPaymentLevel", "PlayerId");
            AddForeignKey("payment.PlayerPaymentLevel", "PlayerId", "payment.Players", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("payment.PlayerPaymentLevel", "PlayerId", "payment.Players");
            DropIndex("payment.PlayerPaymentLevel", new[] { "PlayerId" });
            DropColumn("payment.Players", "SelfExcludeEndDate");
            DropColumn("payment.Players", "IsSelfExclude");
            DropColumn("payment.Players", "TimeOutEndDate");
            DropColumn("payment.Players", "IsTimeOut");
        }
    }
}
