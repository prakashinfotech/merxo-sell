using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerxoSell.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferBanners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OfferBanners",
                columns: table => new
                {
                    BannerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DesktopImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MobileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RedirectUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OfferTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BannerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Hero"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferBanners", x => x.BannerId);
                    table.ForeignKey(
                        name: "FK_OfferBanners_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OfferBanners_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_BannerType",
                table: "OfferBanners",
                column: "BannerType");

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_CategoryId",
                table: "OfferBanners",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_DisplayOrder",
                table: "OfferBanners",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_IsActive",
                table: "OfferBanners",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_IsActive_StartDate_EndDate",
                table: "OfferBanners",
                columns: new[] { "IsActive", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_ProductId",
                table: "OfferBanners",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfferBanners");
        }
    }
}
