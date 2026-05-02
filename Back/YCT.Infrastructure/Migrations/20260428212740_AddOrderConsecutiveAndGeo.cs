using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YCT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderConsecutiveAndGeo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Consecutive",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingLat",
                table: "Orders",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingLng",
                table: "Orders",
                type: "decimal(10,7)",
                nullable: true);

            // Asignar consecutivos a órdenes existentes según orden de creación
            migrationBuilder.Sql(@"
;WITH Ordered AS (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS rn
    FROM Orders
)
UPDATE o SET Consecutive = Ordered.rn
FROM Orders o INNER JOIN Ordered ON o.Id = Ordered.Id;");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Consecutive",
                table: "Orders",
                column: "Consecutive",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Consecutive",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Consecutive",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingLat",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingLng",
                table: "Orders");
        }
    }
}
