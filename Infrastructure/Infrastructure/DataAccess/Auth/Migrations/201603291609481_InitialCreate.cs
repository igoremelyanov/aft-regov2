namespace AFT.RegoV2.Infrastructure.DataAccess.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "auth.Actors",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Username = c.String(),
                        EncryptedPassword = c.String(),
                        Role_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("auth.Roles", t => t.Role_Id)
                .Index(t => t.Role_Id);
            
            CreateTable(
                "auth.Roles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "auth.Permissions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Module = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "auth.RolePermissions",
                c => new
                    {
                        Role_Id = c.Guid(nullable: false),
                        Permission_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Role_Id, t.Permission_Id })
                .ForeignKey("auth.Roles", t => t.Role_Id, cascadeDelete: true)
                .ForeignKey("auth.Permissions", t => t.Permission_Id, cascadeDelete: true)
                .Index(t => t.Role_Id)
                .Index(t => t.Permission_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("auth.Actors", "Role_Id", "auth.Roles");
            DropForeignKey("auth.RolePermissions", "Permission_Id", "auth.Permissions");
            DropForeignKey("auth.RolePermissions", "Role_Id", "auth.Roles");
            DropIndex("auth.RolePermissions", new[] { "Permission_Id" });
            DropIndex("auth.RolePermissions", new[] { "Role_Id" });
            DropIndex("auth.Actors", new[] { "Role_Id" });
            DropTable("auth.RolePermissions");
            DropTable("auth.Permissions");
            DropTable("auth.Roles");
            DropTable("auth.Actors");
        }
    }
}
