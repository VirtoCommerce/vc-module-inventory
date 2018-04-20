namespace VirtoCommerce.InventoryModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FulfillementCenter : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Inventory", "FulfillmentCenterId");
            AddForeignKey("dbo.Inventory", "FulfillmentCenterId", "dbo.FulfillmentCenter", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Inventory", "FulfillmentCenterId", "dbo.FulfillmentCenter");
            DropIndex("dbo.Inventory", new[] { "FulfillmentCenterId" });
        }
    }
}
