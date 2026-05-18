using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcopioModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "acopio");

            migrationBuilder.CreateTable(
                name: "Camiones",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camiones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Granjeros",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Finca = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Vereda = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Municipio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrecioLitro = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PromedioDiario = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Granjeros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asistentes",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CamionPreferidoId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistentes_Camiones_CamionPreferidoId",
                        column: x => x.CamionPreferidoId,
                        principalSchema: "acopio",
                        principalTable: "Camiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Conductores",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CamionPreferidoId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conductores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conductores_Camiones_CamionPreferidoId",
                        column: x => x.CamionPreferidoId,
                        principalSchema: "acopio",
                        principalTable: "Camiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Conductores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Rutas",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CamionId = table.Column<int>(type: "int", nullable: false),
                    ConductorId = table.Column<int>(type: "int", nullable: false),
                    AsistenteId = table.Column<int>(type: "int", nullable: true),
                    HoraSalida = table.Column<TimeSpan>(type: "time", nullable: true),
                    HoraLlegadaPlanta = table.Column<TimeSpan>(type: "time", nullable: true),
                    HoraDescargue = table.Column<TimeSpan>(type: "time", nullable: true),
                    TotalLitrosChofer = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalLitrosPlanta = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DiferenciaTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rutas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rutas_Asistentes_AsistenteId",
                        column: x => x.AsistenteId,
                        principalSchema: "acopio",
                        principalTable: "Asistentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Rutas_Camiones_CamionId",
                        column: x => x.CamionId,
                        principalSchema: "acopio",
                        principalTable: "Camiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rutas_Conductores_ConductorId",
                        column: x => x.ConductorId,
                        principalSchema: "acopio",
                        principalTable: "Conductores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recogidas",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RutaId = table.Column<int>(type: "int", nullable: false),
                    GranjeroId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CantinasChofer = table.Column<int>(type: "int", nullable: false),
                    SaldoChofer = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    LitrosChofer = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CantinasPlanta = table.Column<int>(type: "int", nullable: true),
                    SaldoPlanta = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    LitrosPlanta = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DiferenciaLitros = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MotivoDiferencia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RecogidoAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DescargadoAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperarioPlantaUserId = table.Column<int>(type: "int", nullable: true),
                    ClientUuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recogidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recogidas_Granjeros_GranjeroId",
                        column: x => x.GranjeroId,
                        principalSchema: "acopio",
                        principalTable: "Granjeros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recogidas_Rutas_RutaId",
                        column: x => x.RutaId,
                        principalSchema: "acopio",
                        principalTable: "Rutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recogidas_Users_OperarioPlantaUserId",
                        column: x => x.OperarioPlantaUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistentes_CamionPreferidoId",
                schema: "acopio",
                table: "Asistentes",
                column: "CamionPreferidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistentes_Cedula",
                schema: "acopio",
                table: "Asistentes",
                column: "Cedula",
                unique: true,
                filter: "[Cedula] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Camiones_Nombre",
                schema: "acopio",
                table: "Camiones",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conductores_CamionPreferidoId",
                schema: "acopio",
                table: "Conductores",
                column: "CamionPreferidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Conductores_Cedula",
                schema: "acopio",
                table: "Conductores",
                column: "Cedula",
                unique: true,
                filter: "[Cedula] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Conductores_UserId",
                schema: "acopio",
                table: "Conductores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Granjeros_Cedula",
                schema: "acopio",
                table: "Granjeros",
                column: "Cedula",
                unique: true,
                filter: "[Cedula] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Granjeros_Numero",
                schema: "acopio",
                table: "Granjeros",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recogidas_ClientUuid",
                schema: "acopio",
                table: "Recogidas",
                column: "ClientUuid",
                unique: true,
                filter: "[ClientUuid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Recogidas_Fecha_GranjeroId",
                schema: "acopio",
                table: "Recogidas",
                columns: new[] { "Fecha", "GranjeroId" });

            migrationBuilder.CreateIndex(
                name: "IX_Recogidas_GranjeroId",
                schema: "acopio",
                table: "Recogidas",
                column: "GranjeroId");

            migrationBuilder.CreateIndex(
                name: "IX_Recogidas_OperarioPlantaUserId",
                schema: "acopio",
                table: "Recogidas",
                column: "OperarioPlantaUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Recogidas_RutaId",
                schema: "acopio",
                table: "Recogidas",
                column: "RutaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rutas_AsistenteId",
                schema: "acopio",
                table: "Rutas",
                column: "AsistenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Rutas_CamionId_Fecha",
                schema: "acopio",
                table: "Rutas",
                columns: new[] { "CamionId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Rutas_ConductorId_Fecha",
                schema: "acopio",
                table: "Rutas",
                columns: new[] { "ConductorId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Rutas_Fecha",
                schema: "acopio",
                table: "Rutas",
                column: "Fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recogidas",
                schema: "acopio");

            migrationBuilder.DropTable(
                name: "Granjeros",
                schema: "acopio");

            migrationBuilder.DropTable(
                name: "Rutas",
                schema: "acopio");

            migrationBuilder.DropTable(
                name: "Asistentes",
                schema: "acopio");

            migrationBuilder.DropTable(
                name: "Conductores",
                schema: "acopio");

            migrationBuilder.DropTable(
                name: "Camiones",
                schema: "acopio");
        }
    }
}
