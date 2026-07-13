using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNovedadRevisada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Revisada",
                schema: "acopio",
                table: "RutaNovedades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevisadaAt",
                schema: "acopio",
                table: "RutaNovedades",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevisadaPor",
                schema: "acopio",
                table: "RutaNovedades",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Revisada",
                schema: "acopio",
                table: "RutaNovedades");

            migrationBuilder.DropColumn(
                name: "RevisadaAt",
                schema: "acopio",
                table: "RutaNovedades");

            migrationBuilder.DropColumn(
                name: "RevisadaPor",
                schema: "acopio",
                table: "RutaNovedades");
        }
    }
}
