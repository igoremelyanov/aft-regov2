namespace AFT.RegoV2.Infrastructure.DataAccess.Documents.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "doc.FileMetadatas",
                c => new
                    {
                        FileId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                        LicenseeId = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.FileId);
            
        }
        
        public override void Down()
        {
            DropTable("doc.FileMetadatas");
        }
    }
}
