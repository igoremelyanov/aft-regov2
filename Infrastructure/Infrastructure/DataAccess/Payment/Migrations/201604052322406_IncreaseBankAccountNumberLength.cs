namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncreaseBankAccountNumberLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("payment.BankAccounts", "AccountNumber", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("payment.BankAccounts", "AccountNumber", c => c.String(nullable: false, maxLength: 20));
        }
    }
}
