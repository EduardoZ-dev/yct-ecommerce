using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntregaTerceroConfirmacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmadaEnPlantaAt",
                schema: "acopio",
                table: "EntregasTerceros",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmadaPorNombre",
                schema: "acopio",
                table: "EntregasTerceros",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmadaPorUserId",
                schema: "acopio",
                table: "EntregasTerceros",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntregasTerceros_ConfirmadaEnPlantaAt",
                schema: "acopio",
                table: "EntregasTerceros",
                column: "ConfirmadaEnPlantaAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntregasTerceros_ConfirmadaEnPlantaAt",
                schema: "acopio",
                table: "EntregasTerceros");

            migrationBuilder.DropColumn(
                name: "ConfirmadaEnPlantaAt",
                schema: "acopio",
                table: "EntregasTerceros");

            migrationBuilder.DropColumn(
                name: "ConfirmadaPorNombre",
                schema: "acopio",
                table: "EntregasTerceros");

            migrationBuilder.DropColumn(
                name: "ConfirmadaPorUserId",
                schema: "acopio",
                table: "EntregasTerceros");
        }
    }
}
