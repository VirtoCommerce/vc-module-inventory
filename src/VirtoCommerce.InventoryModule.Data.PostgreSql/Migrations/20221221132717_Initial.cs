using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.InventoryModule.Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FulfillmentCenter",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ShortDescription = table.Column<string>(type: "text", nullable: true),
                    Line1 = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Line2 = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    City = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StateProvince = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CountryName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RegionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RegionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DaytimePhoneNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OrganizationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    GeoLocation = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FulfillmentCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InStockQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReorderMinQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PreorderQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BackorderQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllowBackorder = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPreorder = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PreorderAvailabilityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BackorderAvailabilityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Sku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FulfillmentCenterId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventory_FulfillmentCenter_FulfillmentCenterId",
                        column: x => x.FulfillmentCenterId,
                        principalTable: "FulfillmentCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_FulfillmentCenterId",
                table: "Inventory",
                column: "FulfillmentCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Sku_ModifiedDate",
                table: "Inventory",
                columns: new[] { "Sku", "ModifiedDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "FulfillmentCenter");
        }
    }
}
