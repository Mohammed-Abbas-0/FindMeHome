using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMeHome.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppAndWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForSale",
                table: "RealEstates");

            migrationBuilder.DropColumn(
                name: "IsFurnished",
                table: "RealEstates");

            migrationBuilder.DropColumn(
                name: "ApartmentId",
                table: "Furnitures");

            migrationBuilder.RenameColumn(
                name: "ExtraPrice",
                table: "Furnitures",
                newName: "Price");

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "RealEstates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Furnitures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Wishlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealEstateId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wishlists_RealEstates_RealEstateId",
                        column: x => x.RealEstateId,
                        principalTable: "RealEstates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_RealEstateId",
                table: "Wishlists",
                column: "RealEstateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wishlists");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "RealEstates");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Furnitures");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Furnitures",
                newName: "ExtraPrice");

            migrationBuilder.AddColumn<bool>(
                name: "IsForSale",
                table: "RealEstates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFurnished",
                table: "RealEstates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ApartmentId",
                table: "Furnitures",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
