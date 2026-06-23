using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGranjeroCodigos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GranjeroCodigoId",
                schema: "acopio",
                table: "Recogidas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GranjeroCodigos",
                schema: "acopio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GranjeroId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Finca = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GranjeroCodigos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GranjeroCodigos_Granjeros_GranjeroId",
                        column: x => x.GranjeroId,
                        principalSchema: "acopio",
                        principalTable: "Granjeros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recogidas_GranjeroCodigoId",
                schema: "acopio",
                table: "Recogidas",
                column: "GranjeroCodigoId");

            migrationBuilder.CreateIndex(
                name: "IX_GranjeroCodigos_Codigo",
                schema: "acopio",
                table: "GranjeroCodigos",
                column: "Codigo");

            migrationBuilder.CreateIndex(
                name: "IX_GranjeroCodigos_GranjeroId",
                schema: "acopio",
                table: "GranjeroCodigos",
                column: "GranjeroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recogidas_GranjeroCodigos_GranjeroCodigoId",
                schema: "acopio",
                table: "Recogidas",
                column: "GranjeroCodigoId",
                principalSchema: "acopio",
                principalTable: "GranjeroCodigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recogidas_GranjeroCodigos_GranjeroCodigoId",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropTable(
                name: "GranjeroCodigos",
                schema: "acopio");

            migrationBuilder.DropIndex(
                name: "IX_Recogidas_GranjeroCodigoId",
                schema: "acopio",
                table: "Recogidas");

            migrationBuilder.DropColumn(
                name: "GranjeroCodigoId",
                schema: "acopio",
                table: "Recogidas");
        }
    }
}
