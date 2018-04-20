namespace VirtoCommerce.InventoryModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DescriptionNewSize : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.FulfillmentCenter", "Description", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.FulfillmentCenter", "Description", c => c.String(maxLength: 256));
        }
    }
}
