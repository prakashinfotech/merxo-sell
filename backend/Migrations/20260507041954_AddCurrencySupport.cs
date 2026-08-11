using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerxoSell.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Orders",
                newName: "TotalAmountCAD");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "OrderItems",
                newName: "UnitPriceCAD");

            migrationBuilder.AddColumn<string>(
                name: "PreferredCurrency",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "CAD");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DisplayTotal",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CurrencyRates",
                columns: table => new
                {
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CurrencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RateToCad = table.Column<decimal>(type: "decimal(18,6)", nullable: false, defaultValue: 1.000000m),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "$"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRates", x => x.CurrencyCode);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PreferredCurrency",
                table: "Users",
                column: "PreferredCurrency");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_CurrencyRates_PreferredCurrency",
                table: "Users",
                column: "PreferredCurrency",
                principalTable: "CurrencyRates",
                principalColumn: "CurrencyCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_CurrencyRates_PreferredCurrency",
                table: "Users");

            migrationBuilder.DropTable(
                name: "CurrencyRates");

            migrationBuilder.DropIndex(
                name: "IX_Users_PreferredCurrency",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredCurrency",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DisplayTotal",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "TotalAmountCAD",
                table: "Orders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "UnitPriceCAD",
                table: "OrderItems",
                newName: "UnitPrice");
        }
    }
}
