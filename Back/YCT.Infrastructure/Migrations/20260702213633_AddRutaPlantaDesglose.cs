using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRutaPlantaDesglose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantinasPlanta",
                schema: "acopio",
                table: "Rutas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LitrosSueltosPlanta",
                schema: "acopio",
                table: "Rutas",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantinasPlanta",
                schema: "acopio",
                table: "Rutas");

            migrationBuilder.DropColumn(
                name: "LitrosSueltosPlanta",
                schema: "acopio",
                table: "Rutas");
        }
    }
}
