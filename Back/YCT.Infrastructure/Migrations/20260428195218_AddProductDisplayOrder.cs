using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Inicializa el orden con el Id actual para preservar el orden existente
            migrationBuilder.Sql("UPDATE Products SET DisplayOrder = Id;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Products");
        }
    }
}
