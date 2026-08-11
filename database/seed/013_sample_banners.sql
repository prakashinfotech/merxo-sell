-- ============================================================
-- Seed 013 — Sample Offer Banners
-- Inserts 1 Hero + 1 MidLeft + 1 MidRight banner pointing to
-- existing seeded products. Run AFTER 013_offer_banners.sql.
-- ============================================================

-- Remove any stale sample rows so this script is re-runnable.
DELETE FROM dbo.OfferBanners
WHERE Title IN (
    'Summer Polo Sale',
    'Seasonal Shirt Sale',
    'Complete Bundles',
    'Free Shipping on all orders over $50'
);


-- Helper: grab the first two approved active products
DECLARE @ProductId1 INT, @ProductId2 INT, @ProductId3 INT;
SELECT TOP 1 @ProductId1 = ProductId FROM dbo.Products
    WHERE IsActive = 1 AND IsDeleted = 0 AND Status = 'Approved' ORDER BY CreatedAt DESC;
SELECT TOP 1 @ProductId2 = ProductId FROM dbo.Products
    WHERE IsActive = 1 AND IsDeleted = 0 AND Status = 'Approved' AND ProductId <> @ProductId1 ORDER BY CreatedAt DESC;
SELECT TOP 1 @ProductId3 = ProductId FROM dbo.Products
    WHERE IsActive = 1 AND IsDeleted = 0 AND Status = 'Approved' AND ProductId NOT IN (@ProductId1, ISNULL(@ProductId2, 0)) ORDER BY CreatedAt DESC;

-- Hero banner
INSERT INTO dbo.OfferBanners
    (Slot, Title, Subtitle, BadgeText, ImageUrl, SideImageUrl,
     CtaLabel, CtaUrl, SecondaryLabel, SecondaryUrl,
     BackgroundColor, TextColor, LinkedProductId, LinkedCategoryId,
     StartsAt, EndsAt, SortOrder, IsActive, CreatedAt)
VALUES
    ('Hero',
     'Summer Polo Sale',
     'Premium quality polos at unbeatable prices',
     'UP TO 40% OFF',
     'http://localhost:5000/uploads/sample-hero-banner.jpg',
     NULL,
     'Discover the Collection',
     CASE WHEN @ProductId1 IS NOT NULL THEN '/products/' + CAST(@ProductId1 AS NVARCHAR) ELSE '/products' END,
     'Preview',
     '/products',
     '#fff8eb',
     '#1a3a5e',
     @ProductId1,
     NULL,
     NULL,
     NULL,
     1,
     1,
     GETUTCDATE());

-- MidLeft banner
INSERT INTO dbo.OfferBanners
    (Slot, Title, Subtitle, BadgeText, ImageUrl, SideImageUrl,
     CtaLabel, CtaUrl, SecondaryLabel, SecondaryUrl,
     BackgroundColor, TextColor, LinkedProductId, LinkedCategoryId,
     StartsAt, EndsAt, SortOrder, IsActive, CreatedAt)
VALUES
    ('MidLeft',
     'Seasonal Shirt Sale',
     'GET 50% OFF TODAY',
     '50% OFF',
     'http://localhost:5000/uploads/sample-mid-left-banner.jpg',
     NULL,
     'Shop Shirts',
     CASE WHEN @ProductId2 IS NOT NULL THEN '/products/' + CAST(@ProductId2 AS NVARCHAR) ELSE '/products' END,
     NULL,
     NULL,
     '#e8f5e9',
     '#1b5e20',
     @ProductId2,
     NULL,
     NULL,
     NULL,
     1,
     1,
     GETUTCDATE());

-- MidRight banner
INSERT INTO dbo.OfferBanners
    (Slot, Title, Subtitle, BadgeText, ImageUrl, SideImageUrl,
     CtaLabel, CtaUrl, SecondaryLabel, SecondaryUrl,
     BackgroundColor, TextColor, LinkedProductId, LinkedCategoryId,
     StartsAt, EndsAt, SortOrder, IsActive, CreatedAt)
VALUES
    ('MidRight',
     'Complete Bundles',
     'AND GET $300 OFF!',
     '$300 OFF',
     'http://localhost:5000/uploads/sample-mid-right-banner.jpg',
     NULL,
     'Shop Bundles',
     CASE WHEN @ProductId3 IS NOT NULL THEN '/products/' + CAST(@ProductId3 AS NVARCHAR) ELSE '/products' END,
     NULL,
     NULL,
     '#1a3a5e',
     '#ffffff',
     @ProductId3,
     NULL,
     NULL,
     NULL,
     1,
     1,
     GETUTCDATE());

-- Strip banner
INSERT INTO dbo.OfferBanners
    (Slot, Title, Subtitle, BadgeText, ImageUrl, SideImageUrl,
     CtaLabel, CtaUrl, SecondaryLabel, SecondaryUrl,
     BackgroundColor, TextColor, LinkedProductId, LinkedCategoryId,
     StartsAt, EndsAt, SortOrder, IsActive, CreatedAt)
VALUES
    ('Strip',
     'Free Shipping on all orders over $50',
     NULL,
     NULL,
     'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=',
     NULL,
     'Shop Now',
     '/products',
     NULL,
     NULL,
     '#f8f9fa',
     '#212529',
     NULL,
     NULL,
     NULL,
     NULL,
     1,
     1,
     GETUTCDATE());


GO
PRINT 'Seed 013_sample_banners applied successfully.';
