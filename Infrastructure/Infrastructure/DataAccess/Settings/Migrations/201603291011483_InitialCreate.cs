namespace AFT.RegoV2.Infrastructure.DataAccess.Settings.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "settings.Settings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Key = c.String(maxLength: 450),
                        Value = c.String(),
                        UpdatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Key, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("settings.Settings", new[] { "Key" });
            DropTable("settings.Settings");
        }
    }
}
