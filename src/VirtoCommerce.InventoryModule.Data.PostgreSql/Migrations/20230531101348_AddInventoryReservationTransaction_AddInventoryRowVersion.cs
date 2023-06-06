using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.InventoryModule.Data.PostgreSql.Migrations
{
    public partial class AddInventoryReservationTransaction_AddInventoryRowVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Inventory",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryReservationTransaction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ParentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ItemType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FulfillmentCenterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProductId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReservationTransaction", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservationTransaction_ItemId_FulfillmentCenterId_~",
                table: "InventoryReservationTransaction",
                columns: new[] { "ItemId", "FulfillmentCenterId", "ItemType", "Type" },
                unique: true);
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
