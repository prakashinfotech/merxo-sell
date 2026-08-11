-- ============================================================
-- Seed.sql — TemuClone Database Seed Data
-- ============================================================
-- Prerequisites (run once before this script):
--   dotnet ef migrations add AddCurrencySupport
--   dotnet ef database update
--
-- This script is idempotent — safe to run multiple times.
-- Each section checks for existing data before inserting.
--
-- IMPORTANT — Admin password:
--   The PasswordHash below is a placeholder.
--   Replace '$2a$11$PLACEHOLDER...' with a real BCrypt hash of
--   'Admin@123' before running in any environment.
--   Generate with: BCrypt.Net.BCrypt.HashPassword("Admin@123", 11)
-- ============================================================

USE MerxoSellDb;
GO

-- ============================================================
-- SECTION 1 — Roles
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'SuperAdmin')
    INSERT INTO Roles (RoleName) VALUES ('SuperAdmin');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Seller')
    INSERT INTO Roles (RoleName) VALUES ('Seller');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Buyer')
    INSERT INTO Roles (RoleName) VALUES ('Buyer');

DECLARE @SuperAdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'SuperAdmin');
DECLARE @SellerRoleId     INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Seller');
DECLARE @BuyerRoleId      INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Buyer');

PRINT 'Roles seeded — SuperAdminRoleId: ' + CAST(@SuperAdminRoleId AS NVARCHAR)
    + '  SellerRoleId: ' + CAST(@SellerRoleId AS NVARCHAR)
    + '  BuyerRoleId: ' + CAST(@BuyerRoleId AS NVARCHAR);
GO

-- ============================================================
-- SECTION 2 — CurrencyRates
-- (All monetary values in the app are stored in CAD.
--  These rates are used by the Angular display layer only.)
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM CurrencyRates WHERE CurrencyCode = 'CAD')
    INSERT INTO CurrencyRates (CurrencyCode, CurrencyName, RateToCad, Symbol, IsActive, LastUpdated)
    VALUES ('CAD', 'Canadian Dollar', 1.000000, '$', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM CurrencyRates WHERE CurrencyCode = 'USD')
    INSERT INTO CurrencyRates (CurrencyCode, CurrencyName, RateToCad, Symbol, IsActive, LastUpdated)
    VALUES ('USD', 'US Dollar', 0.740000, '$', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM CurrencyRates WHERE CurrencyCode = 'EUR')
    INSERT INTO CurrencyRates (CurrencyCode, CurrencyName, RateToCad, Symbol, IsActive, LastUpdated)
    VALUES ('EUR', 'Euro', 0.680000, '€', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM CurrencyRates WHERE CurrencyCode = 'GBP')
    INSERT INTO CurrencyRates (CurrencyCode, CurrencyName, RateToCad, Symbol, IsActive, LastUpdated)
    VALUES ('GBP', 'British Pound', 0.590000, '£', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM CurrencyRates WHERE CurrencyCode = 'INR')
    INSERT INTO CurrencyRates (CurrencyCode, CurrencyName, RateToCad, Symbol, IsActive, LastUpdated)
    VALUES ('INR', 'Indian Rupee', 61.500000, '₹', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM CurrencyRates WHERE CurrencyCode = 'AUD')
    INSERT INTO CurrencyRates (CurrencyCode, CurrencyName, RateToCad, Symbol, IsActive, LastUpdated)
    VALUES ('AUD', 'Australian Dollar', 1.130000, '$', 1, GETDATE());

PRINT 'CurrencyRates seeded — 6 currencies (CAD, USD, EUR, GBP, INR, AUD)';
GO

-- ============================================================
-- SECTION 3 — Admin User
-- ============================================================

DECLARE @SuperAdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'SuperAdmin');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@temuclone.com')
    INSERT INTO Users
        (RoleId, FullName, Email, PasswordHash, Phone, PreferredCurrency, IsActive, CreatedAt)
    VALUES
        (
            @SuperAdminRoleId,
            'Admin User',
            'admin@temuclone.com',
            -- BCrypt hash of 'Admin@123456' (work factor 11)
            '$2a$11$fOnD2zcbxOANoivufHxpuO7Vqx1a0z/FzwkG69PgxvVHXgkViGxi.',
            NULL,
            'CAD',
            1,
            GETDATE()
        );

DECLARE @AdminUserId INT = (SELECT UserId FROM Users WHERE Email = 'admin@temuclone.com');

PRINT 'Admin user seeded — UserId: ' + CAST(@AdminUserId AS NVARCHAR)
    + '  Email: admin@temuclone.com';
GO

-- ============================================================
-- SECTION 4 — Categories
-- Insert order: top-level parents first, children second.
-- ============================================================

-- ── Top-level categories ──────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'electronics')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (NULL, 'Electronics', 'electronics', NULL, 1, 1);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'fashion')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (NULL, 'Fashion', 'fashion', NULL, 2, 1);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'home-garden')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (NULL, 'Home & Garden', 'home-garden', NULL, 3, 1);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'sports')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (NULL, 'Sports', 'sports', NULL, 4, 1);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'beauty')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (NULL, 'Beauty', 'beauty', NULL, 5, 1);

-- Capture parent IDs before inserting children
DECLARE @ElectronicsId INT = (SELECT CategoryId FROM Categories WHERE Slug = 'electronics');
DECLARE @FashionId     INT = (SELECT CategoryId FROM Categories WHERE Slug = 'fashion');

-- ── Child categories — Electronics ───────────────────────
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'phones')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (@ElectronicsId, 'Phones', 'phones', NULL, 1, 1);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'laptops')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (@ElectronicsId, 'Laptops', 'laptops', NULL, 2, 1);

-- ── Child categories — Fashion ────────────────────────────
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'men')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (@FashionId, 'Men', 'men', NULL, 1, 1);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Slug = 'women')
    INSERT INTO Categories (ParentCategoryId, Name, Slug, ImageUrl, SortOrder, IsActive)
    VALUES (@FashionId, 'Women', 'women', NULL, 2, 1);

PRINT 'Categories seeded — 5 top-level, 4 child categories';
GO

-- ============================================================
-- SECTION 5 — Products  (all prices in CAD)
-- ============================================================

DECLARE @PhonesCatId  INT = (SELECT CategoryId FROM Categories WHERE Slug = 'phones');
DECLARE @MenCatId     INT = (SELECT CategoryId FROM Categories WHERE Slug = 'men');
DECLARE @SportsCatId  INT = (SELECT CategoryId FROM Categories WHERE Slug = 'sports');

-- ── Product 1: Wireless Earbuds Pro ──────────────────────
IF NOT EXISTS (SELECT 1 FROM Products WHERE Slug = 'wireless-earbuds-pro')
    INSERT INTO Products
        (CategoryId, Name, Slug, Description, BasePrice, SalePrice, Stock, IsActive, CreatedAt)
    VALUES
        (
            @PhonesCatId,
            'Wireless Earbuds Pro',
            'wireless-earbuds-pro',
            N'Premium wireless earbuds with active noise cancellation, 30-hour battery life, '
            + N'and IPX5 water resistance. Compatible with iOS and Android.',
            40.50,
            26.99,
            150,
            1,
            GETDATE()
        );

-- ── Product 2: Classic White T-Shirt ─────────────────────
IF NOT EXISTS (SELECT 1 FROM Products WHERE Slug = 'classic-white-tshirt')
    INSERT INTO Products
        (CategoryId, Name, Slug, Description, BasePrice, SalePrice, Stock, IsActive, CreatedAt)
    VALUES
        (
            @MenCatId,
            'Classic White T-Shirt',
            'classic-white-tshirt',
            N'100% premium cotton crew-neck tee. Preshrunk fabric, reinforced stitching, '
            + N'available in sizes S through XXL.',
            17.50,
            NULL,
            300,
            1,
            GETDATE()
        );

PRINT 'Products seeded — Wireless Earbuds Pro, Classic White T-Shirt';
GO

-- ============================================================
-- SECTION 6 — ProductImages
-- ============================================================

DECLARE @EarbudsId  INT = (SELECT ProductId FROM Products WHERE Slug = 'wireless-earbuds-pro');
DECLARE @TshirtId   INT = (SELECT ProductId FROM Products WHERE Slug = 'classic-white-tshirt');
DECLARE @YogaMatId  INT = (SELECT ProductId FROM Products WHERE Slug = 'yoga-mat-premium');

-- ── Earbuds images ────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @EarbudsId AND SortOrder = 1)
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    VALUES
        (@EarbudsId, '/images/products/earbuds-front.jpg',  'Wireless Earbuds Pro front view', 1, 1);

IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @EarbudsId AND SortOrder = 2)
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    VALUES
        (@EarbudsId, '/images/products/earbuds-case.jpg',   'Wireless Earbuds Pro charging case', 0, 2);

IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @EarbudsId AND SortOrder = 3)
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    VALUES
        (@EarbudsId, '/images/products/earbuds-worn.jpg',   'Wireless Earbuds Pro worn view', 0, 3);

-- ── T-Shirt images ────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @TshirtId AND SortOrder = 1)
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    VALUES
        (@TshirtId, '/images/products/tshirt-front.jpg',   'Classic White T-Shirt front', 1, 1);

IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @TshirtId AND SortOrder = 2)
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    VALUES
        (@TshirtId, '/images/products/tshirt-back.jpg',    'Classic White T-Shirt back', 0, 2);

-- ── Yoga Mat images ───────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @YogaMatId AND SortOrder = 1)
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    VALUES
        (@YogaMatId, '/images/products/yogamat-rolled.jpg', 'Yoga Mat Premium rolled up',  1, 1);

IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @YogaMatId AND SortOrder = 2)
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    VALUES
        (@YogaMatId, '/images/products/yogamat-flat.jpg',   'Yoga Mat Premium laid flat', 0, 2);

PRINT 'ProductImages seeded — 3 earbuds, 2 t-shirt, 2 yoga mat';
GO

-- ============================================================
-- SECTION 7 — ProductVariants  (earbuds only — color variants)
-- PriceDelta is in CAD relative to the product BasePrice.
-- ============================================================

DECLARE @EarbudsId INT = (SELECT ProductId FROM Products WHERE Slug = 'wireless-earbuds-pro');

-- Black — no price premium, 85 units
IF NOT EXISTS (SELECT 1 FROM ProductVariants WHERE ProductId = @EarbudsId AND Color = 'Black')
    INSERT INTO ProductVariants (ProductId, Color, Size, PriceDelta, Stock, SKU, IsActive)
    VALUES (@EarbudsId, 'Black', NULL, 0.00, 85, 'EAR-PRO-BLK', 1);

-- White — CA$2.00 premium (limited edition colour), 65 units
IF NOT EXISTS (SELECT 1 FROM ProductVariants WHERE ProductId = @EarbudsId AND Color = 'White')
    INSERT INTO ProductVariants (ProductId, Color, Size, PriceDelta, Stock, SKU, IsActive)
    VALUES (@EarbudsId, 'White', NULL, 2.00, 65, 'EAR-PRO-WHT', 1);

PRINT 'ProductVariants seeded — Earbuds Black (85 stock), Earbuds White (65 stock, +CA$2.00)';
GO

-- ============================================================
-- VERIFICATION QUERIES
-- Run these after seeding to confirm row counts.
-- ============================================================

SELECT 'Roles'           AS [Table], COUNT(*) AS [Rows] FROM Roles
UNION ALL
SELECT 'CurrencyRates',               COUNT(*)          FROM CurrencyRates
UNION ALL
SELECT 'Users',                        COUNT(*)          FROM Users
UNION ALL
SELECT 'Categories',                   COUNT(*)          FROM Categories
UNION ALL
SELECT 'Products',                     COUNT(*)          FROM Products
UNION ALL
SELECT 'ProductImages',                COUNT(*)          FROM ProductImages
UNION ALL
SELECT 'ProductVariants',              COUNT(*)          FROM ProductVariants;
GO

-- Expected results:
-- Roles           2
-- CurrencyRates   6
-- Users           1
-- Categories      9
-- Products        3
-- ProductImages   7
-- ProductVariants 2
