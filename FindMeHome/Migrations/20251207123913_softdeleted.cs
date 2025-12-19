using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindMeHome.Migrations
{
    /// <inheritdoc />
    public partial class softdeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RealEstates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RealEstates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RealEstates",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RealEstates");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RealEstates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RealEstates");
        }
    }
}
