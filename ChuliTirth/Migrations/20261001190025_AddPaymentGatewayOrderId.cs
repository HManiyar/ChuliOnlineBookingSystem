using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChuliTirth.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentGatewayOrderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayOrderId",
                table: "Payments",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayOrderId",
                table: "Payments",
                column: "GatewayOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_GatewayOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "GatewayOrderId",
                table: "Payments");
        }
    }
}
