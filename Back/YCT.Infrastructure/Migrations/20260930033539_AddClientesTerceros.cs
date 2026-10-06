using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientesTerceros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientesTerceros",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Municipio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrecioLitro = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesTerceros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntregasTerceros",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteTerceroId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    Cantinas = table.Column<int>(type: "int", nullable: false),
                    SaldoLitros = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Litros = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioLitro = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RutaId = table.Column<int>(type: "int", nullable: true),
                    RegistradoPorUserId = table.Column<int>(type: "int", nullable: true),
                    RegistradoPorNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ClientUuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregasTerceros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntregasTerceros_ClientesTerceros_ClienteTerceroId",
                        column: x => x.ClienteTerceroId,
                        principalSchema: "acopio",
                        principalTable: "ClientesTerceros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntregasTerceros_Rutas_RutaId",
                        column: x => x.RutaId,
                        principalSchema: "acopio",
                        principalTable: "Rutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTerceros_Cedula",
                schema: "acopio",
                table: "ClientesTerceros",
                column: "Cedula",
                unique: true,
                filter: "[Cedula] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesTerceros_NombreCompleto",
                schema: "acopio",
                table: "ClientesTerceros",
                column: "NombreCompleto");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasTerceros_ClienteTerceroId",
                schema: "acopio",
                table: "EntregasTerceros",
                column: "ClienteTerceroId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasTerceros_ClientUuid",
                schema: "acopio",
                table: "EntregasTerceros",
                column: "ClientUuid",
                unique: true,
                filter: "[ClientUuid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasTerceros_Fecha",
                schema: "acopio",
                table: "EntregasTerceros",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasTerceros_RutaId",
                schema: "acopio",
                table: "EntregasTerceros",
                column: "RutaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntregasTerceros",
                schema: "acopio");

            migrationBuilder.DropTable(
                name: "ClientesTerceros",
                schema: "acopio");
        }
    }
}
