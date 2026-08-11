-- ============================================================
-- 002_dummy_products.sql
-- Seeds dummy products with real Unsplash images and categories.
-- Provides a dedicated Seller account for testing.
-- ============================================================

USE MerxoSellDb;
GO

-- ── 1. Ensure Roles Exist ───────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'SuperAdmin') INSERT INTO Roles (RoleName) VALUES ('SuperAdmin');
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Seller')     INSERT INTO Roles (RoleName) VALUES ('Seller');
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Buyer')      INSERT INTO Roles (RoleName) VALUES ('Buyer');
GO

-- ── 2. Create Dummy Seller User ──────────────────────────────────────────────
-- Email:    seller@temuclone.com
-- Password: Seller@123
DECLARE @SellerRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Seller');
DECLARE @SellerUserId INT;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'seller@temuclone.com')
BEGIN
    INSERT INTO Users (RoleId, FullName, Email, PasswordHash, PreferredCurrency, IsActive)
    VALUES (
        @SellerRoleId, 
        'Demo Seller', 
        'seller@temuclone.com', 
        '$2a$11$mC7GlnmMWzT6Ic9vNfXoGeK3WvD0k8R4N7v8E9GjH8e7V2q.o.02K', 
        'CAD', 
        1
    );
    SET @SellerUserId = SCOPE_IDENTITY();

    INSERT INTO Sellers (UserId, StoreName, StoreDescription, IsVerified, IsActive)
    VALUES (@SellerUserId, 'Global Gadgets & Gear', 'Your one-stop shop for premium tech and lifestyle products.', 1, 1);
END
ELSE
BEGIN
    SET @SellerUserId = (SELECT UserId FROM Users WHERE Email = 'seller@temuclone.com');
END

DECLARE @SellerId INT = (SELECT SellerId FROM Sellers WHERE UserId = @SellerUserId);
GO

-- ── 3. Categories ────────────────────────────────────────────────────────────
MERGE INTO Categories AS target
USING (VALUES 
    ('Electronics', 'electronics', 1),
    ('Fashion', 'fashion', 2),
    ('Home & Decor', 'home-decor', 3),
    ('Beauty & Personal Care', 'beauty-care', 4),
    ('Sports & Fitness', 'sports-fitness', 5)
) AS source (Name, Slug, SortOrder)
ON target.Slug = source.Slug
WHEN NOT MATCHED THEN
    INSERT (Name, Slug, SortOrder, IsActive)
    VALUES (source.Name, source.Slug, source.SortOrder, 1);
GO

-- ── 4. Manufacturers ─────────────────────────────────────────────────────────
-- Note: Manufacturers table does not have Slug or Description columns.
MERGE INTO Manufacturers AS target
USING (VALUES 
    ('Apple', 'United States'),
    ('Samsung', 'South Korea'),
    ('Nike', 'United States'),
    ('IKEA', 'Sweden')
) AS source (Name, Country)
ON target.Name = source.Name
WHEN NOT MATCHED THEN
    INSERT (Name, Country, IsActive)
    VALUES (source.Name, source.Country, 1);
GO

-- ── 5. Dummy Products ────────────────────────────────────────────────────────
DECLARE @CatElectronics INT = (SELECT CategoryId FROM Categories WHERE Slug = 'electronics');
DECLARE @CatFashion     INT = (SELECT CategoryId FROM Categories WHERE Slug = 'fashion');
DECLARE @CatHome        INT = (SELECT CategoryId FROM Categories WHERE Slug = 'home-decor');
DECLARE @CatBeauty      INT = (SELECT CategoryId FROM Categories WHERE Slug = 'beauty-care');
DECLARE @CatSports      INT = (SELECT CategoryId FROM Categories WHERE Slug = 'sports-fitness');

DECLARE @MfrApple   INT = (SELECT ManufacturerId FROM Manufacturers WHERE Name = 'Apple');
DECLARE @MfrSamsung INT = (SELECT ManufacturerId FROM Manufacturers WHERE Name = 'Samsung');
DECLARE @MfrNike    INT = (SELECT ManufacturerId FROM Manufacturers WHERE Name = 'Nike');
DECLARE @MfrIkea    INT = (SELECT ManufacturerId FROM Manufacturers WHERE Name = 'IKEA');

DECLARE @SellerId INT = (SELECT SellerId FROM Sellers WHERE StoreName = 'Global Gadgets & Gear');

-- Only insert if they don't exist (avoiding duplicates on re-run)
IF NOT EXISTS (SELECT 1 FROM Products WHERE Slug = 'iphone-15-pro-128')
BEGIN
    INSERT INTO Products (CategoryId, SellerId, ManufacturerId, Name, Slug, Description, BasePrice, Stock, Status, IsActive)
    VALUES 
        (@CatElectronics, @SellerId, @MfrApple, 'iPhone 15 Pro - 128GB', 'iphone-15-pro-128', 'The latest flagship from Apple with Titanium design and A17 Pro chip.', 1099.00, 50, 'Approved', 1),
        (@CatElectronics, @SellerId, @MfrSamsung, 'Samsung Galaxy S23 Ultra', 'samsung-s23-ultra', 'Experience the ultimate smartphone with 200MP camera and S Pen.', 1199.00, 40, 'Approved', 1),
        (@CatFashion, @SellerId, @MfrNike, 'Nike Air Max 270', 'nike-air-max-270', 'Legendary Air Max cushioning and sleek design for everyday comfort.', 160.00, 100, 'Approved', 1),
        (@CatHome, @SellerId, @MfrIkea, 'MALM High Bed Frame', 'malm-bed-frame', 'Clean design that looks just as good from every side.', 249.00, 20, 'Approved', 1),
        (@CatBeauty, @SellerId, NULL, 'Hydrating Night Cream', 'hydrating-night-cream', 'Revitalize your skin with our premium overnight hydration formula.', 35.00, 200, 'Approved', 1),
        (@CatSports, @SellerId, NULL, 'Adjustable Dumbbell Set', 'adjustable-dumbbell-set', 'Versatile home workout equipment for all fitness levels.', 199.00, 30, 'Approved', 1);

    -- Product Images (Linked to the last 6 inserted IDs)
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    SELECT ProductId, 
           CASE 
             WHEN Slug = 'iphone-15-pro-128' THEN 'https://images.unsplash.com/photo-1696446701796-da61225697cc?q=80&w=600'
             WHEN Slug = 'samsung-s23-ultra' THEN 'https://images.unsplash.com/photo-1678911820864-e2c567c655d7?q=80&w=600'
             WHEN Slug = 'nike-air-max-270' THEN 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?q=80&w=600'
             WHEN Slug = 'malm-bed-frame' THEN 'https://images.unsplash.com/photo-1505693419148-de397065842c?q=80&w=600'
             WHEN Slug = 'hydrating-night-cream' THEN 'https://images.unsplash.com/photo-1556229010-6c3f2c9ca5f8?q=80&w=600'
             WHEN Slug = 'adjustable-dumbbell-set' THEN 'https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?q=80&w=600'
           END, 
           Name, 1, 1
    FROM Products WHERE Slug IN ('iphone-15-pro-128','samsung-s23-ultra','nike-air-max-270','malm-bed-frame','hydrating-night-cream','adjustable-dumbbell-set');
END
GO
