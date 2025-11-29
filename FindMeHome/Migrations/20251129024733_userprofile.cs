using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMeHome.Migrations
{
    /// <inheritdoc />
    public partial class userprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RealEstates",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RealEstates_UserId",
                table: "RealEstates",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RealEstates_AspNetUsers_UserId",
                table: "RealEstates",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RealEstates_AspNetUsers_UserId",
                table: "RealEstates");

            migrationBuilder.DropIndex(
                name: "IX_RealEstates_UserId",
                table: "RealEstates");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RealEstates");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");
        }
    }
}
