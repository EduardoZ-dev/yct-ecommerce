using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRutaNovedades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RutaNovedades",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PlanillaUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RutaId = table.Column<int>(type: "int", nullable: true),
                    ConductorId = table.Column<int>(type: "int", nullable: false),
                    CamionId = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GranjeroCodigoId = table.Column<int>(type: "int", nullable: true),
                    ReportadoAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GpsLat = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    GpsLng = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutaNovedades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RutaNovedades_Camiones_CamionId",
                        column: x => x.CamionId,
                        principalSchema: "acopio",
                        principalTable: "Camiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RutaNovedades_Conductores_ConductorId",
                        column: x => x.ConductorId,
                        principalSchema: "acopio",
                        principalTable: "Conductores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RutaNovedades_GranjeroCodigos_GranjeroCodigoId",
                        column: x => x.GranjeroCodigoId,
                        principalSchema: "acopio",
                        principalTable: "GranjeroCodigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RutaNovedades_Rutas_RutaId",
                        column: x => x.RutaId,
                        principalSchema: "acopio",
                        principalTable: "Rutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RutaNovedades_CamionId",
                schema: "acopio",
                table: "RutaNovedades",
                column: "CamionId");

            migrationBuilder.CreateIndex(
                name: "IX_RutaNovedades_ConductorId",
                schema: "acopio",
                table: "RutaNovedades",
                column: "ConductorId");

            migrationBuilder.CreateIndex(
                name: "IX_RutaNovedades_GranjeroCodigoId",
                schema: "acopio",
                table: "RutaNovedades",
                column: "GranjeroCodigoId");

            migrationBuilder.CreateIndex(
                name: "IX_RutaNovedades_PlanillaUuid",
                schema: "acopio",
                table: "RutaNovedades",
                column: "PlanillaUuid");

            migrationBuilder.CreateIndex(
                name: "IX_RutaNovedades_RutaId",
                schema: "acopio",
                table: "RutaNovedades",
                column: "RutaId");

            migrationBuilder.CreateIndex(
                name: "IX_RutaNovedades_Uuid",
                schema: "acopio",
                table: "RutaNovedades",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RutaNovedades",
                schema: "acopio");
        }
    }
}
