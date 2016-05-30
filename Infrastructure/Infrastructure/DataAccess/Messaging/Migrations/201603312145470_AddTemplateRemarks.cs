namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTemplateRemarks : DbMigration
    {
        public override void Up()
        {
            AddColumn("messaging.MessageTemplates", "Remarks", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("messaging.MessageTemplates", "Remarks");
        }
    }
}
