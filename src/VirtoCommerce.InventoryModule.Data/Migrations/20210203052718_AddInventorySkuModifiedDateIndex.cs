using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.InventoryModule.Data.Migrations
{
    public partial class AddInventorySkuModifiedDateIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [IX_Inventory_Sku] ON [Inventory]");

            // Add index thru scripting because of no way to make indexing field DESC
            migrationBuilder.Sql(@"CREATE INDEX [IX_Inventory_Sku_ModifiedDate] ON [Inventory] ([Sku] ASC, [ModifiedDate] DESC)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventory_Sku_ModifiedDate",
                table: "Inventory");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Sku",
                table: "Inventory",
                column: "Sku");
        }
    }
}
