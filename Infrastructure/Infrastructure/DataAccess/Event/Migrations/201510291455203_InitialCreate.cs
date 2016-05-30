namespace AFT.RegoV2.Infrastructure.DataAccess.Event.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "event.Events",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AggregateId = c.Guid(nullable: false),
                        DataType = c.String(),
                        Data = c.String(),
                        State = c.Int(nullable: false),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        Published = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .Index(t => t.Created, clustered: true);
            
        }
        
        public override void Down()
        {
            DropIndex("event.Events", new[] { "Created" });
            DropTable("event.Events");
        }
    }
}
