using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.InventoryModule.Data.SqlServer.Migrations
{
    public partial class AddInventoryReservationTransaction_AddInventoryRowVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Inventory",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryReservationTransaction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ParentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ItemType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ItemId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FulfillmentCenterId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReservationTransaction", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservationTransaction_ItemId_FulfillmentCenterId_ItemType_Type",
                table: "InventoryReservationTransaction",
                columns: new[] { "ItemId", "FulfillmentCenterId", "ItemType", "Type" },
                unique: true,
                filter: "[ItemId] IS NOT NULL AND [FulfillmentCenterId] IS NOT NULL AND [ItemType] IS NOT NULL AND [Type] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryReservationTransaction");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Inventory");
        }
    }
}
