using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerxoSell.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeBrowsingHistoryUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_BrowsingHistory_User_Product' AND parent_object_id = OBJECT_ID('BrowsingHistories')) ALTER TABLE [BrowsingHistories] DROP CONSTRAINT [UQ_BrowsingHistory_User_Product]");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_BrowsingHistory_User_Product' AND object_id = OBJECT_ID('BrowsingHistories')) DROP INDEX [UQ_BrowsingHistory_User_Product] ON [BrowsingHistories]");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "BrowsingHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "UQ_BrowsingHistory_User_Product",
                table: "BrowsingHistories",
                columns: new[] { "UserId", "ProductId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "CouponUsageHistory");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CouponId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "UQ_BrowsingHistory_User_Product",
                table: "BrowsingHistories");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CouponId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "BrowsingHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_BrowsingHistory_User_Product",
                table: "BrowsingHistories",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }
    }
}
