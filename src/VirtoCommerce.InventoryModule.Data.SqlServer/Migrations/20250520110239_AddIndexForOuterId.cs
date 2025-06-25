using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.InventoryModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForOuterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Inventory_OuterId",
                table: "Inventory",
                column: "OuterId");

            migrationBuilder.CreateIndex(
                name: "IX_FulfillmentCenter_OuterId",
                table: "FulfillmentCenter",
                column: "OuterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventory_OuterId",
                table: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_FulfillmentCenter_OuterId",
                table: "FulfillmentCenter");
        }
    }
}
