-- ============================================================
-- Migration 013 — Offer Banners (v2)
-- Replaces the old BannerType-based table with a Slot-based
-- design supporting Hero, MidLeft, MidRight, and Strip zones.
-- Run AFTER: 012_coupons_and_browsing_history.sql
-- ============================================================

-- Drop old table if it exists (dev environments)
IF OBJECT_ID('dbo.OfferBanners', 'U') IS NOT NULL
    DROP TABLE dbo.OfferBanners;

CREATE TABLE dbo.OfferBanners (
    BannerId         INT             NOT NULL IDENTITY(1,1),
    Slot             NVARCHAR(20)    NOT NULL,             -- 'Hero' | 'MidLeft' | 'MidRight' | 'Strip'
    Title            NVARCHAR(200)   NOT NULL,
    Subtitle         NVARCHAR(300)   NULL,
    BadgeText        NVARCHAR(40)    NULL,                 -- e.g. "50% OFF"
    ImageUrl         NVARCHAR(500)   NOT NULL,             -- main banner image
    SideImageUrl     NVARCHAR(500)   NULL,                 -- small thumbnail in Hero right column
    CtaLabel         NVARCHAR(60)    NULL,                 -- e.g. "Discover the Collection"
    CtaUrl           NVARCHAR(500)   NULL,                 -- /products/123 or /products?categoryId=8
    SecondaryLabel   NVARCHAR(60)    NULL,                 -- e.g. "Preview"
    SecondaryUrl     NVARCHAR(500)   NULL,
    BackgroundColor  NVARCHAR(20)    NULL,                 -- "#fff8eb" or NULL = default theme
    TextColor        NVARCHAR(20)    NULL,                 -- "#1a3a5e"
    LinkedProductId  INT             NULL,                 -- optional deep-link to a product
    LinkedCategoryId INT             NULL,                 -- optional deep-link to a category
    StartsAt         DATETIME2       NULL,
    EndsAt           DATETIME2       NULL,
    SortOrder        INT             NOT NULL DEFAULT 0,
    IsActive         BIT             NOT NULL DEFAULT 1,
    CreatedAt        DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2       NULL,

    CONSTRAINT PK_OfferBanners       PRIMARY KEY (BannerId),
    CONSTRAINT CK_OfferBanners_Slot  CHECK (Slot IN ('Hero','MidLeft','MidRight','Strip')),
    CONSTRAINT FK_OfferBanners_Product
        FOREIGN KEY (LinkedProductId)  REFERENCES dbo.Products  (ProductId)   ON DELETE SET NULL,
    CONSTRAINT FK_OfferBanners_Category
        FOREIGN KEY (LinkedCategoryId) REFERENCES dbo.Categories (CategoryId) ON DELETE SET NULL
);

CREATE INDEX IX_OfferBanners_Slot_Active_Sort
    ON dbo.OfferBanners (Slot, IsActive, SortOrder)
    INCLUDE (Title, ImageUrl, StartsAt, EndsAt);

CREATE INDEX IX_OfferBanners_Active_Window
    ON dbo.OfferBanners (IsActive, StartsAt, EndsAt);

GO
PRINT 'Migration 013_offer_banners applied successfully.';
