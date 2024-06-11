using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjekWebShop.Data.Migrations
{
    public partial class CartModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Price",
                table: "CartItem",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "CartItem");
        }
    }
}
