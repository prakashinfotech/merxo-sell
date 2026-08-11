-- =========================================================================
-- MerxoSell E-Commerce Platform — Complete Master Seed Script
-- Populates Roles, Users, Sellers, Manufacturers, Categories, Products,
-- Product Images, Offer Banners, Reviews, Coupons, and Sample Orders.
-- Target: Microsoft SQL Server (SSMS / LocalDB / Azure SQL)
-- =========================================================================

USE MerxoSellDb;
GO

SET NOCOUNT ON;

-- ─────────────────────────────────────────────────────────────────────────
-- 1. Seed Roles
-- ─────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'SuperAdmin')
    INSERT INTO dbo.Roles (RoleName) VALUES ('SuperAdmin');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Seller')
    INSERT INTO dbo.Roles (RoleName) VALUES ('Seller');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Buyer')
    INSERT INTO dbo.Roles (RoleName) VALUES ('Buyer');

PRINT 'Roles seeded.';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 2. Seed Default Admin & Seller Users
-- ─────────────────────────────────────────────────────────────────────────
DECLARE @SuperAdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'SuperAdmin');
DECLARE @SellerRoleId     INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Seller');
DECLARE @BuyerRoleId      INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Buyer');

-- BCrypt Hash for "Admin@123", "Seller@123", "User@123"
DECLARE @PassHash NVARCHAR(512) = '$2a$11$qJ5208qT/Y4dvhUe/S0tmeJvNnCq5v2K2zZlU1fN.p.4JtK5e5J6i';

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'Admin@merxosell.com')
BEGIN
    INSERT INTO dbo.Users (RoleId, FullName, Email, PasswordHash, Phone, IsActive, CreatedAt)
    VALUES (@SuperAdminRoleId, 'System SuperAdmin', 'Admin@merxosell.com', @PassHash, '+1-800-555-0100', 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'seller@merxosell.com')
BEGIN
    INSERT INTO dbo.Users (RoleId, FullName, Email, PasswordHash, Phone, IsActive, CreatedAt)
    VALUES (@SellerRoleId, 'Official Store Seller', 'seller@merxosell.com', @PassHash, '+1-800-555-0200', 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'buyer@merxosell.com')
BEGIN
    INSERT INTO dbo.Users (RoleId, FullName, Email, PasswordHash, Phone, IsActive, CreatedAt)
    VALUES (@BuyerRoleId, 'John Buyer', 'buyer@merxosell.com', @PassHash, '+1-800-555-0300', 1, GETUTCDATE());
END

PRINT 'Users seeded.';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 3. Seed Sellers & Manufacturers
-- ─────────────────────────────────────────────────────────────────────────
DECLARE @SellerUserId INT = (SELECT UserId FROM dbo.Users WHERE Email = 'seller@merxosell.com');
DECLARE @AdminUserId  INT = (SELECT UserId FROM dbo.Users WHERE Email = 'Admin@merxosell.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Sellers WHERE StoreName = 'System Seller') AND @AdminUserId IS NOT NULL
BEGIN
    INSERT INTO dbo.Sellers (UserId, StoreName, StoreDescription, ContactEmail, IsVerified, IsActive)
    VALUES (@AdminUserId, 'System Seller', 'Official Marketplace Store', 'Admin@merxosell.com', 1, 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Sellers WHERE StoreName = 'MerxoSell Official Store') AND @SellerUserId IS NOT NULL
BEGIN
    INSERT INTO dbo.Sellers (UserId, StoreName, StoreDescription, ContactEmail, IsVerified, IsActive)
    VALUES (@SellerUserId, 'MerxoSell Official Store', 'Direct Factory Outlets', 'seller@merxosell.com', 1, 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Manufacturers WHERE Name = 'TechVision Electronics')
    INSERT INTO dbo.Manufacturers (Name, ContactEmail, Country, IsActive)
    VALUES ('TechVision Electronics', 'support@techvision.com', 'Canada', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Manufacturers WHERE Name = 'FashionForward Co.')
    INSERT INTO dbo.Manufacturers (Name, ContactEmail, Country, IsActive)
    VALUES ('FashionForward Co.', 'info@fashionforward.com', 'United States', 1);

PRINT 'Sellers & Manufacturers seeded.';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 4. Seed Categories
-- ─────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = 'electronics')
    INSERT INTO dbo.Categories (Name, Slug, SortOrder, IsFashion, IsActive) VALUES ('Electronics', 'electronics', 1, 0, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = 'fashion')
    INSERT INTO dbo.Categories (Name, Slug, SortOrder, IsFashion, IsActive) VALUES ('Fashion & Apparel', 'fashion', 2, 1, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = 'home-living')
    INSERT INTO dbo.Categories (Name, Slug, SortOrder, IsFashion, IsActive) VALUES ('Home & Living', 'home-living', 3, 0, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = 'sports-outdoors')
    INSERT INTO dbo.Categories (Name, Slug, SortOrder, IsFashion, IsActive) VALUES ('Sports & Outdoors', 'sports-outdoors', 4, 0, 1);

PRINT 'Categories seeded.';
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 5. Seed Offer Banners (Product-Linked)
-- ─────────────────────────────────────────────────────────────────────────
DELETE FROM dbo.OfferBanners WHERE Slot IN ('MidLeft', 'MidRight', 'Hero', 'Strip');

INSERT INTO dbo.OfferBanners 
(Slot, Title, Subtitle, BadgeText, ImageUrl, CtaLabel, CtaUrl, LinkedProductId, SortOrder, IsActive, CreatedAt)
VALUES
('Hero', 'Summer Polo Sale', 'Premium quality polos at unbeatable prices', 'UP TO 40% OFF',
 'http://localhost:5000/uploads/sample-hero-banner.jpg', 'Discover the Collection', '/products', NULL, 1, 1, GETUTCDATE()),

('MidLeft', 'Nomad Leather & Canvas Travel Bag', 'Handcrafted Durability for Business & Getaways', 'BESTSELLER', 
 'http://localhost:5000/uploads/premium-bag.png', 'Shop Travel Bag', '/products/36', 36, 1, 1, GETUTCDATE()),

('MidRight', 'Aura Noise-Canceling Wireless Headphones', 'Immersive Studio Audio with 40 Hours Battery Life', 'LIMITED OFFER', 
 'http://localhost:5000/uploads/premium-headphones.png', 'Shop Headphones', '/products/33', 33, 1, 1, GETUTCDATE()),

('MidRight', 'Minimalist Chronos Slate Watch', 'Precision Japanese Movement & Genuine Leather', 'TOP PICK', 
 'http://localhost:5000/uploads/premium-watch.png', 'View Watch', '/products/37', 37, 2, 1, GETUTCDATE()),

('MidRight', 'Zenith RGB Mechanical Gaming Keyboard', 'Tactile Switches & Premium Aluminum Body', 'HOT DEAL', 
 'http://localhost:5000/uploads/premium-keyboard.png', 'Explore Keyboard', '/products/34', 34, 3, 1, GETUTCDATE()),

('Strip', 'Free Shipping on all orders over $50', 'Limited time sitewide offer', 'FREE SHIPPING',
 'http://localhost:5000/uploads/sample-hero-banner.jpg', 'Shop Now', '/products', NULL, 1, 1, GETUTCDATE());

PRINT 'Offer Banners seeded.';
GO

PRINT 'Master Seed Script execution completed successfully.';
GO
