using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMeHome.Migrations
{
    /// <inheritdoc />
    public partial class isSeller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSellerRequest",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSellerRequest",
                table: "AspNetUsers");
        }
    }
}
