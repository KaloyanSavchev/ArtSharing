using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtSharing.Web.Data.Migrations
{
    public partial class AddStatusProgress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Feedbacks");
        }
    }
}
