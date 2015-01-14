namespace HxAntenna.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AntennaRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Permission",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ControllerName = c.String(nullable: false),
                        ActionName = c.String(),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SerialNumber",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestResult",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SerialNumberId = c.Int(nullable: false),
                        Result = c.Boolean(nullable: false),
                        TestTime = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SerialNumber", t => t.SerialNumberId)
                .Index(t => t.SerialNumberId);
            
            CreateTable(
                "dbo.TestResultItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestResultId = c.Int(nullable: false),
                        ResultItem = c.Boolean(nullable: false),
                        TestItemId = c.Int(nullable: false),
                        AntennaUserId = c.String(maxLength: 128),
                        TestTimeItem = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                        IsLatestTest = c.Boolean(nullable: false),
                        IsEsc = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AntennaUser", t => t.AntennaUserId)
                .ForeignKey("dbo.TestItem", t => t.TestItemId)
                .ForeignKey("dbo.TestResult", t => t.TestResultId)
                .Index(t => t.AntennaUserId)
                .Index(t => t.TestItemId)
                .Index(t => t.TestResultId);
            
            CreateTable(
                "dbo.AntennaUser",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        JobNumber = c.String(maxLength: 20),
                        AntennaRoleId = c.Int(),
                        IsDeleted = c.Boolean(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AntennaRole", t => t.AntennaRoleId)
                .Index(t => t.AntennaRoleId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AntennaUser", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
                .ForeignKey("dbo.AntennaUser", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId)
                .ForeignKey("dbo.AntennaUser", t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TestResultItemDegree",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestResultItemId = c.Int(nullable: false),
                        Degree = c.Decimal(nullable: false, precision: 6, scale: 2),
                        Img = c.String(),
                        ResultItemDegree = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestResultItem", t => t.TestResultItemId)
                .Index(t => t.TestResultItemId);
            
            CreateTable(
                "dbo.TestResultItemDegreeVal",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestResultItemDegreeId = c.Int(nullable: false),
                        TestStandardId = c.Int(nullable: false),
                        Port = c.Int(nullable: false),
                        TestData = c.Decimal(nullable: false, precision: 8, scale: 2),
                        ResultItemDegreeVal = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestResultItemDegree", t => t.TestResultItemDegreeId)
                .ForeignKey("dbo.TestStandard", t => t.TestStandardId)
                .Index(t => t.TestResultItemDegreeId)
                .Index(t => t.TestStandardId);
            
            CreateTable(
                "dbo.TestStandard",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Symbol = c.Int(nullable: false),
                        StandardValue = c.Decimal(nullable: false, precision: 8, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AntennaRolePermission",
                c => new
                    {
                        AntennaRoleId = c.Int(nullable: false),
                        PermissionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AntennaRoleId, t.PermissionId })
                .ForeignKey("dbo.AntennaRole", t => t.AntennaRoleId)
                .ForeignKey("dbo.Permission", t => t.PermissionId)
                .Index(t => t.AntennaRoleId)
                .Index(t => t.PermissionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TestResultItemDegreeVal", "TestStandardId", "dbo.TestStandard");
            DropForeignKey("dbo.TestResultItemDegreeVal", "TestResultItemDegreeId", "dbo.TestResultItemDegree");
            DropForeignKey("dbo.TestResultItemDegree", "TestResultItemId", "dbo.TestResultItem");
            DropForeignKey("dbo.TestResultItem", "TestResultId", "dbo.TestResult");
            DropForeignKey("dbo.TestResultItem", "TestItemId", "dbo.TestItem");
            DropForeignKey("dbo.TestResultItem", "AntennaUserId", "dbo.AntennaUser");
            DropForeignKey("dbo.AspNetUserClaims", "User_Id", "dbo.AntennaUser");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AntennaUser");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AntennaUser");
            DropForeignKey("dbo.AntennaUser", "AntennaRoleId", "dbo.AntennaRole");
            DropForeignKey("dbo.TestResult", "SerialNumberId", "dbo.SerialNumber");
            DropForeignKey("dbo.AntennaRolePermission", "PermissionId", "dbo.Permission");
            DropForeignKey("dbo.AntennaRolePermission", "AntennaRoleId", "dbo.AntennaRole");
            DropIndex("dbo.TestResultItemDegreeVal", new[] { "TestStandardId" });
            DropIndex("dbo.TestResultItemDegreeVal", new[] { "TestResultItemDegreeId" });
            DropIndex("dbo.TestResultItemDegree", new[] { "TestResultItemId" });
            DropIndex("dbo.TestResultItem", new[] { "TestResultId" });
            DropIndex("dbo.TestResultItem", new[] { "TestItemId" });
            DropIndex("dbo.TestResultItem", new[] { "AntennaUserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "User_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AntennaUser", new[] { "AntennaRoleId" });
            DropIndex("dbo.TestResult", new[] { "SerialNumberId" });
            DropIndex("dbo.AntennaRolePermission", new[] { "PermissionId" });
            DropIndex("dbo.AntennaRolePermission", new[] { "AntennaRoleId" });
            DropTable("dbo.AntennaRolePermission");
            DropTable("dbo.TestStandard");
            DropTable("dbo.TestResultItemDegreeVal");
            DropTable("dbo.TestResultItemDegree");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AntennaUser");
            DropTable("dbo.TestResultItem");
            DropTable("dbo.TestResult");
            DropTable("dbo.TestItem");
            DropTable("dbo.SerialNumber");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Permission");
            DropTable("dbo.AntennaRole");
        }
    }
}
