using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTinaGestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TinaMovimientos",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GranjeroCodigoId = table.Column<int>(type: "int", nullable: true),
                    EsPlanta = table.Column<bool>(type: "bit", nullable: false),
                    CantidadAnterior = table.Column<int>(type: "int", nullable: false),
                    CantidadNueva = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinaMovimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TinaMovimientos_GranjeroCodigos_GranjeroCodigoId",
                        column: x => x.GranjeroCodigoId,
                        principalSchema: "acopio",
                        principalTable: "GranjeroCodigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TinaPlanta",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinaPlanta", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "acopio",
                table: "TinaPlanta",
                columns: new[] { "Id", "CreatedAt", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.CreateIndex(
                name: "IX_TinaMovimientos_CreatedAt",
                schema: "acopio",
                table: "TinaMovimientos",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TinaMovimientos_GranjeroCodigoId",
                schema: "acopio",
                table: "TinaMovimientos",
                column: "GranjeroCodigoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TinaMovimientos",
                schema: "acopio");

            migrationBuilder.DropTable(
                name: "TinaPlanta",
                schema: "acopio");
        }
    }
}
