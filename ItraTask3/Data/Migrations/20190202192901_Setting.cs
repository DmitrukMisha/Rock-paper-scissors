using Microsoft.EntityFrameworkCore.Migrations;

namespace ItraTask3.Data.Migrations
{
    public partial class Setting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Settings",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                            name: "Settings",
                            table: "AspNetUsers");
        }
    }
}
