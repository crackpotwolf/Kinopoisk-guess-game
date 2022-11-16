using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinopoiskGuessGame.Data.Migrations
{
    public partial class HotFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Answers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Answers");
        }
    }
}
