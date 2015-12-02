namespace HxAntenna.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_models_for_pim : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Carrier",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SetFreq = c.Decimal(nullable: false, precision: 8, scale: 2),
                        StartFreq = c.Decimal(nullable: false, precision: 8, scale: 2),
                        StopFreq = c.Decimal(nullable: false, precision: 8, scale: 2),
                        FreqUnit = c.Int(nullable: false),
                        Power = c.Decimal(nullable: false, precision: 8, scale: 2),
                        ImUnit = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestResultPim",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestTime = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                        SerialNumberId = c.Int(nullable: false),
                        AntennaUserId = c.String(maxLength: 128),
                        TestEquipmentId = c.Int(),
                        ImOrderId = c.Int(nullable: false),
                        TestMeans = c.Int(nullable: false),
                        TestDescription = c.String(),
                        TestResult = c.Boolean(nullable: false),
                        TestImage = c.String(),
                        IsLatest = c.Boolean(nullable: false),
                        LimitLine = c.Decimal(nullable: false, precision: 8, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AntennaUser", t => t.AntennaUserId)
                .ForeignKey("dbo.ImOrder", t => t.ImOrderId)
                .ForeignKey("dbo.SerialNumber", t => t.SerialNumberId)
                .ForeignKey("dbo.TestEquipment", t => t.TestEquipmentId)
                .Index(t => t.AntennaUserId)
                .Index(t => t.ImOrderId)
                .Index(t => t.SerialNumberId)
                .Index(t => t.TestEquipmentId);
            
            CreateTable(
                "dbo.ImOrder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderNumber = c.Int(nullable: false),
                        ImUnit = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestEquipment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SerialNumber = c.String(),
                        isVna = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TestResultPimPoint",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TestResultPimId = c.Int(nullable: false),
                        CarrierOneFreq = c.Decimal(nullable: false, precision: 8, scale: 2),
                        CarrierTwoFreq = c.Decimal(nullable: false, precision: 8, scale: 2),
                        ImFreq = c.Decimal(nullable: false, precision: 8, scale: 2),
                        ImPower = c.Decimal(nullable: false, precision: 8, scale: 2),
                        isWorst = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TestResultPim", t => t.TestResultPimId)
                .Index(t => t.TestResultPimId);
            
            CreateTable(
                "dbo.TestResultPimCarrier",
                c => new
                    {
                        TestResultPim_Id = c.Int(nullable: false),
                        Carrier_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TestResultPim_Id, t.Carrier_Id })
                .ForeignKey("dbo.TestResultPim", t => t.TestResultPim_Id)
                .ForeignKey("dbo.Carrier", t => t.Carrier_Id)
                .Index(t => t.TestResultPim_Id)
                .Index(t => t.Carrier_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TestResultPimPoint", "TestResultPimId", "dbo.TestResultPim");
            DropForeignKey("dbo.TestResultPim", "TestEquipmentId", "dbo.TestEquipment");
            DropForeignKey("dbo.TestResultPim", "SerialNumberId", "dbo.SerialNumber");
            DropForeignKey("dbo.TestResultPim", "ImOrderId", "dbo.ImOrder");
            DropForeignKey("dbo.TestResultPimCarrier", "Carrier_Id", "dbo.Carrier");
            DropForeignKey("dbo.TestResultPimCarrier", "TestResultPim_Id", "dbo.TestResultPim");
            DropForeignKey("dbo.TestResultPim", "AntennaUserId", "dbo.AntennaUser");
            DropIndex("dbo.TestResultPimPoint", new[] { "TestResultPimId" });
            DropIndex("dbo.TestResultPim", new[] { "TestEquipmentId" });
            DropIndex("dbo.TestResultPim", new[] { "SerialNumberId" });
            DropIndex("dbo.TestResultPim", new[] { "ImOrderId" });
            DropIndex("dbo.TestResultPimCarrier", new[] { "Carrier_Id" });
            DropIndex("dbo.TestResultPimCarrier", new[] { "TestResultPim_Id" });
            DropIndex("dbo.TestResultPim", new[] { "AntennaUserId" });
            DropTable("dbo.TestResultPimCarrier");
            DropTable("dbo.TestResultPimPoint");
            DropTable("dbo.TestEquipment");
            DropTable("dbo.ImOrder");
            DropTable("dbo.TestResultPim");
            DropTable("dbo.Carrier");
        }
    }
}
