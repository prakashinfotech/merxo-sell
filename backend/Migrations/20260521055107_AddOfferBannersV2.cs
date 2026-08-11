using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerxoSell.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferBannersV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferBanners_Categories_CategoryId",
                table: "OfferBanners");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferBanners_Products_ProductId",
                table: "OfferBanners");

            migrationBuilder.DropIndex(
                name: "IX_OfferBanners_BannerType",
                table: "OfferBanners");

            migrationBuilder.DropIndex(
                name: "IX_OfferBanners_DisplayOrder",
                table: "OfferBanners");

            migrationBuilder.DropIndex(
                name: "IX_OfferBanners_IsActive",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "BannerType",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "ButtonText",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "OfferTag",
                table: "OfferBanners");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "OfferBanners",
                newName: "StartsAt");

            migrationBuilder.RenameColumn(
                name: "RedirectUrl",
                table: "OfferBanners",
                newName: "SideImageUrl");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "OfferBanners",
                newName: "LinkedProductId");

            migrationBuilder.RenameColumn(
                name: "MobileImageUrl",
                table: "OfferBanners",
                newName: "SecondaryUrl");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "OfferBanners",
                newName: "EndsAt");

            migrationBuilder.RenameColumn(
                name: "DesktopImageUrl",
                table: "OfferBanners",
                newName: "CtaUrl");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "OfferBanners",
                newName: "LinkedCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_OfferBanners_ProductId",
                table: "OfferBanners",
                newName: "IX_OfferBanners_LinkedProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OfferBanners_IsActive_StartDate_EndDate",
                table: "OfferBanners",
                newName: "IX_OfferBanners_IsActive_StartsAt_EndsAt");

            migrationBuilder.RenameIndex(
                name: "IX_OfferBanners_CategoryId",
                table: "OfferBanners",
                newName: "IX_OfferBanners_LinkedCategoryId");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "OfferBanners",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BadgeText",
                table: "OfferBanners",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CtaLabel",
                table: "OfferBanners",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "OfferBanners",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryLabel",
                table: "OfferBanners",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slot",
                table: "OfferBanners",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Hero");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "OfferBanners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TextColor",
                table: "OfferBanners",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_Slot_IsActive_SortOrder",
                table: "OfferBanners",
                columns: new[] { "Slot", "IsActive", "SortOrder" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OfferBanners_Slot",
                table: "OfferBanners",
                sql: "[Slot] IN ('Hero','MidLeft','MidRight','Strip')");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferBanners_Categories_LinkedCategoryId",
                table: "OfferBanners",
                column: "LinkedCategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferBanners_Products_LinkedProductId",
                table: "OfferBanners",
                column: "LinkedProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferBanners_Categories_LinkedCategoryId",
                table: "OfferBanners");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferBanners_Products_LinkedProductId",
                table: "OfferBanners");

            migrationBuilder.DropIndex(
                name: "IX_OfferBanners_Slot_IsActive_SortOrder",
                table: "OfferBanners");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OfferBanners_Slot",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "BadgeText",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "CtaLabel",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "SecondaryLabel",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "OfferBanners");

            migrationBuilder.DropColumn(
                name: "TextColor",
                table: "OfferBanners");

            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "OfferBanners",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "SideImageUrl",
                table: "OfferBanners",
                newName: "RedirectUrl");

            migrationBuilder.RenameColumn(
                name: "SecondaryUrl",
                table: "OfferBanners",
                newName: "MobileImageUrl");

            migrationBuilder.RenameColumn(
                name: "LinkedProductId",
                table: "OfferBanners",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "LinkedCategoryId",
                table: "OfferBanners",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "EndsAt",
                table: "OfferBanners",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "CtaUrl",
                table: "OfferBanners",
                newName: "DesktopImageUrl");

            migrationBuilder.RenameIndex(
                name: "IX_OfferBanners_LinkedProductId",
                table: "OfferBanners",
                newName: "IX_OfferBanners_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OfferBanners_LinkedCategoryId",
                table: "OfferBanners",
                newName: "IX_OfferBanners_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_OfferBanners_IsActive_StartsAt_EndsAt",
                table: "OfferBanners",
                newName: "IX_OfferBanners_IsActive_StartDate_EndDate");

            migrationBuilder.AddColumn<string>(
                name: "BannerType",
                table: "OfferBanners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Hero");

            migrationBuilder.AddColumn<string>(
                name: "ButtonText",
                table: "OfferBanners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "OfferBanners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "OfferBanners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OfferTag",
                table: "OfferBanners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_BannerType",
                table: "OfferBanners",
                column: "BannerType");

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_DisplayOrder",
                table: "OfferBanners",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_OfferBanners_IsActive",
                table: "OfferBanners",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferBanners_Categories_CategoryId",
                table: "OfferBanners",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferBanners_Products_ProductId",
                table: "OfferBanners",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
