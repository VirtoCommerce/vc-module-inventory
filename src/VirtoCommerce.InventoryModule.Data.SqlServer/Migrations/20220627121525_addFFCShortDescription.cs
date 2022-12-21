using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.InventoryModule.Data.SqlServer.Migrations
{
    public partial class addFFCShortDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "FulfillmentCenter",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "FulfillmentCenter");
        }
    }
}
