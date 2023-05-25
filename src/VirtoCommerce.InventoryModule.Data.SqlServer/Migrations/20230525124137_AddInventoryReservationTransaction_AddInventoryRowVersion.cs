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
                    OuterId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OuterType = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FulfillmentCenterId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ParentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
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
                name: "IX_InventoryReservationTransaction_OuterId_OuterType_FulfillmentCenterId_Type",
                table: "InventoryReservationTransaction",
                columns: new[] { "OuterId", "OuterType", "FulfillmentCenterId", "Type" },
                unique: true,
                filter: "[OuterId] IS NOT NULL AND [OuterType] IS NOT NULL AND [FulfillmentCenterId] IS NOT NULL");
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
