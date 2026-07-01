using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecogidaRecorrido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CapturadoAt",
                schema: "acopio",
                table: "Recogidas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GpsLat",
                schema: "acopio",
                table: "Recogidas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GpsLng",
                schema: "acopio",
                table: "Recogidas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                schema: "acopio",
                table: "Recogidas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapturadoAt",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropColumn(
                name: "GpsLat",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropColumn(
                name: "GpsLng",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropColumn(
                name: "Orden",
                schema: "acopio",
                table: "Recogidas");
        }
    }
}
