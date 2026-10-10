using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntregaTerceroMedicionPlanta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantinasPlanta",
                schema: "acopio",
                table: "EntregasTerceros",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LitrosPlanta",
                schema: "acopio",
                table: "EntregasTerceros",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionPlanta",
                schema: "acopio",
                table: "EntregasTerceros",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoPlanta",
                schema: "acopio",
                table: "EntregasTerceros",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantinasPlanta",
                schema: "acopio",
                table: "EntregasTerceros");

            migrationBuilder.DropColumn(
                name: "LitrosPlanta",
                schema: "acopio",
                table: "EntregasTerceros");

            migrationBuilder.DropColumn(
                name: "ObservacionPlanta",
                schema: "acopio",
                table: "EntregasTerceros");

            migrationBuilder.DropColumn(
                name: "SaldoPlanta",
                schema: "acopio",
                table: "EntregasTerceros");
        }
    }
}
