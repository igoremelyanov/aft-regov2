namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_TimeZoneId_To_Brand : DbMigration
    {
        public override void Up()
        {
            AddColumn("messaging.Brands", "TimezoneId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("messaging.Brands", "TimezoneId");
        }
    }
}
