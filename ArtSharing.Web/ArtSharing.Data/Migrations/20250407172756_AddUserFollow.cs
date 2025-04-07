using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtSharing.Web.Data.Migrations
{
    public partial class AddUserFollow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollow_AspNetUsers_FollowerId",
                table: "UserFollow");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFollow_AspNetUsers_FollowingId",
                table: "UserFollow");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFollow",
                table: "UserFollow");

            migrationBuilder.RenameTable(
                name: "UserFollow",
                newName: "UserFollows");

            migrationBuilder.RenameIndex(
                name: "IX_UserFollow_FollowingId",
                table: "UserFollows",
                newName: "IX_UserFollows_FollowingId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFollow_FollowerId",
                table: "UserFollows",
                newName: "IX_UserFollows_FollowerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFollows",
                table: "UserFollows",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollows_AspNetUsers_FollowerId",
                table: "UserFollows",
                column: "FollowerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollows_AspNetUsers_FollowingId",
                table: "UserFollows",
                column: "FollowingId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollows_AspNetUsers_FollowerId",
                table: "UserFollows");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFollows_AspNetUsers_FollowingId",
                table: "UserFollows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFollows",
                table: "UserFollows");

            migrationBuilder.RenameTable(
                name: "UserFollows",
                newName: "UserFollow");

            migrationBuilder.RenameIndex(
                name: "IX_UserFollows_FollowingId",
                table: "UserFollow",
                newName: "IX_UserFollow_FollowingId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFollows_FollowerId",
                table: "UserFollow",
                newName: "IX_UserFollow_FollowerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFollow",
                table: "UserFollow",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollow_AspNetUsers_FollowerId",
                table: "UserFollow",
                column: "FollowerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollow_AspNetUsers_FollowingId",
                table: "UserFollow",
                column: "FollowingId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
