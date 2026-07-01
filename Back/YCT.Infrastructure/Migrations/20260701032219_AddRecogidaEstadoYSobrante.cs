using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecogidaEstadoYSobrante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoOlor",
                schema: "acopio",
                table: "Recogidas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoSabor",
                schema: "acopio",
                table: "Recogidas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoVista",
                schema: "acopio",
                table: "Recogidas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LitrosRegaladosChofer",
                schema: "acopio",
                table: "Recogidas",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                schema: "acopio",
                table: "Recogidas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoOlor",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropColumn(
                name: "EstadoSabor",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropColumn(
                name: "EstadoVista",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropColumn(
                name: "LitrosRegaladosChofer",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropColumn(
                name: "Observacion",
                schema: "acopio",
                table: "Recogidas");
        }
    }
}
