namespace VirtoCommerce.InventoryModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSkuIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Inventory", "Sku");
            DropColumn("dbo.Inventory", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Inventory", "Discriminator", c => c.String(maxLength: 128));
            DropIndex("dbo.Inventory", new[] { "Sku" });
        }
    }
}
