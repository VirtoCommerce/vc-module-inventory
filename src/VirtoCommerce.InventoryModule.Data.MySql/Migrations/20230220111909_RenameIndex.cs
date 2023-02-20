using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.InventoryModule.Data.MySql.Migrations
{
    public partial class RenameIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ObjectType_ObjectId",
                table: "FulfillmentCenterDynamicPropertyObjectValue",
                newName: "IX_FulfillmentCenterDynamicPropertyObjectValue_ObjectType_ObjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_FulfillmentCenterDynamicPropertyObjectValue_ObjectType_ObjectId",
                table: "FulfillmentCenterDynamicPropertyObjectValue",
                newName: "IX_ObjectType_ObjectId");
        }
    }
}
