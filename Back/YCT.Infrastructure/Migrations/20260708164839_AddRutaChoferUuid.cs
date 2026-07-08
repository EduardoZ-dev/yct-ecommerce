using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRutaChoferUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LitrosSueltosPlanta",
                schema: "acopio",
                table: "Rutas",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChoferUuid",
                schema: "acopio",
                table: "Rutas",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rutas_ChoferUuid",
                schema: "acopio",
                table: "Rutas",
                column: "ChoferUuid");

            // Backfill: las rutas existentes guardan el UUID del chofer dentro de
            // Observaciones ("[Enviado por chofer · UUID <guid>]"). Lo extraemos a la
            // columna nueva para que la idempotencia funcione también con envíos previos
            // (resuelve tablets que quedaron con planillas "pegadas" sin duplicar más).
            migrationBuilder.Sql(@"
                UPDATE acopio.Rutas
                SET ChoferUuid = SUBSTRING(Observaciones, CHARINDEX('UUID ', Observaciones) + 5, 36)
                WHERE ChoferUuid IS NULL AND Observaciones LIKE '%UUID %';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rutas_ChoferUuid",
                schema: "acopio",
                table: "Rutas");

            migrationBuilder.DropColumn(
                name: "ChoferUuid",
                schema: "acopio",
                table: "Rutas");

            migrationBuilder.AlterColumn<decimal>(
                name: "LitrosSueltosPlanta",
                schema: "acopio",
                table: "Rutas",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);
        }
    }
}
