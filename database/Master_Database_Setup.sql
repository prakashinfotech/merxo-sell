-- =========================================================================
-- MerxoSell E-Commerce Platform — Master Database Initialization Script
-- Combines Database Schema Creation & Master Seed Initialization.
-- Target: SQL Server / LocalDB / SSMS
-- =========================================================================

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'MerxoSellDb')
BEGIN
    CREATE DATABASE MerxoSellDb;
    PRINT 'Created database MerxoSellDb.';
END
GO

USE MerxoSellDb;
GO

SET NOCOUNT ON;

-- ─────────────────────────────────────────────────────────────────────────
-- SECTION 1: DATABASE TABLES & SCHEMA
-- ─────────────────────────────────────────────────────────────────────────

-- 1. Roles
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RoleId   INT          NOT NULL IDENTITY(1,1),
        RoleName NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_Roles      PRIMARY KEY (RoleId),
        CONSTRAINT UQ_Roles_Name UNIQUE      (RoleName)
    );
END
GO

-- 2. Users
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId       INT           NOT NULL IDENTITY(1,1),
        RoleId       INT           NOT NULL,
        FullName     NVARCHAR(150) NOT NULL,
        Email        NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(512) NOT NULL,
        Phone        NVARCHAR(20)  NULL,
        Country      NVARCHAR(100) NULL,
        IsActive     BIT           NOT NULL DEFAULT 1,
        IsDelete     BIT           NOT NULL DEFAULT 0,
        CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt    DATETIME2     NULL,

        CONSTRAINT PK_Users       PRIMARY KEY (UserId),
        CONSTRAINT UQ_Users_Email UNIQUE      (Email),
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId)
    );
    CREATE INDEX IX_Users_RoleId ON dbo.Users (RoleId);
END
GO

-- 3. Sellers
IF OBJECT_ID('dbo.Sellers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sellers (
        SellerId         INT            NOT NULL IDENTITY(1,1),
        UserId           INT            NOT NULL,
        StoreName        NVARCHAR(200)  NOT NULL,
        StoreDescription NVARCHAR(1000) NULL,
        ContactEmail     NVARCHAR(200)  NULL,
        Phone            NVARCHAR(50)   NULL,
        IsVerified       BIT            NOT NULL DEFAULT 0,
        IsActive         BIT            NOT NULL DEFAULT 1,
        IsDelete         BIT            NOT NULL DEFAULT 0,
        CreatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt        DATETIME2      NULL,

        CONSTRAINT PK_Sellers       PRIMARY KEY (SellerId),
        CONSTRAINT UQ_Sellers_User  UNIQUE      (UserId),
        CONSTRAINT FK_Sellers_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId) ON DELETE CASCADE
    );
END
GO

-- 4. Manufacturers
IF OBJECT_ID('dbo.Manufacturers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Manufacturers (
        ManufacturerId INT           NOT NULL IDENTITY(1,1),
        Name           NVARCHAR(200) NOT NULL,
        ContactEmail   NVARCHAR(200) NULL,
        Phone          NVARCHAR(50)  NULL,
        Address        NVARCHAR(500) NULL,
        Country        NVARCHAR(100) NULL,
        Website        NVARCHAR(300) NULL,
        IsActive       BIT           NOT NULL DEFAULT 1,
        CreatedAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt      DATETIME2     NULL,

        CONSTRAINT PK_Manufacturers PRIMARY KEY (ManufacturerId)
    );
END
GO

-- 5. Categories
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryId       INT           NOT NULL IDENTITY(1,1),
        ParentCategoryId INT           NULL,
        Name             NVARCHAR(100) NOT NULL,
        Slug             NVARCHAR(120) NOT NULL,
        ImageUrl         NVARCHAR(500) NULL,
        SortOrder        INT           NOT NULL DEFAULT 0,
        IsFashion        BIT           NOT NULL DEFAULT 0,
        IsActive         BIT           NOT NULL DEFAULT 1,
        IsDelete         BIT           NOT NULL DEFAULT 0,

        CONSTRAINT PK_Categories        PRIMARY KEY (CategoryId),
        CONSTRAINT UQ_Categories_Slug   UNIQUE      (Slug),
        CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES dbo.Categories (CategoryId)
    );
END
GO

-- 6. Products
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        ProductId      INT            NOT NULL IDENTITY(1,1),
        CategoryId     INT            NOT NULL,
        SellerId       INT            NOT NULL,
        ManufacturerId INT            NULL,
        Name           NVARCHAR(450)  NOT NULL,
        Slug           NVARCHAR(320)  NOT NULL,
        Description    NVARCHAR(MAX)  NULL,
        BasePrice      DECIMAL(18,2)  NOT NULL,
        SalePrice      DECIMAL(18,2)  NULL,
        Stock          INT            NOT NULL DEFAULT 0,
        Status         NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
        ApprovalNote   NVARCHAR(1000) NULL,
        ViewCount      INT            NOT NULL DEFAULT 0,
        IsActive       BIT            NOT NULL DEFAULT 1,
        IsDelete       BIT            NOT NULL DEFAULT 0,
        CreatedAt      DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt      DATETIME2      NULL,

        CONSTRAINT PK_Products            PRIMARY KEY (ProductId),
        CONSTRAINT UQ_Products_Slug       UNIQUE      (Slug),
        CONSTRAINT CK_Products_BasePrice  CHECK       (BasePrice >= 0),
        CONSTRAINT CK_Products_Status     CHECK       (Status IN ('Pending','Approved','Rejected')),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)     REFERENCES dbo.Categories (CategoryId),
        CONSTRAINT FK_Products_Sellers    FOREIGN KEY (SellerId)       REFERENCES dbo.Sellers (SellerId),
        CONSTRAINT FK_Products_Mfg        FOREIGN KEY (ManufacturerId) REFERENCES dbo.Manufacturers (ManufacturerId) ON DELETE SET NULL
    );
    CREATE INDEX IX_Products_CategoryId ON dbo.Products (CategoryId);
    CREATE INDEX IX_Products_SellerId   ON dbo.Products (SellerId);
END
GO

-- 7. ProductVariants & Images
IF OBJECT_ID('dbo.ProductVariants', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariants (
        VariantId  INT           NOT NULL IDENTITY(1,1),
        ProductId  INT           NOT NULL,
        Color      NVARCHAR(50)  NULL,
        Size       NVARCHAR(50)  NULL,
        PriceDelta DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        Stock      INT           NOT NULL DEFAULT 0,
        SKU        NVARCHAR(100) NULL,
        IsDefault  BIT           NOT NULL DEFAULT 0,
        IsActive   BIT           NOT NULL DEFAULT 1,

        CONSTRAINT PK_ProductVariants          PRIMARY KEY (VariantId),
        CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.ProductImages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductImages (
        ImageId   INT           NOT NULL IDENTITY(1,1),
        ProductId INT           NOT NULL,
        VariantId INT           NULL,
        ImageUrl  NVARCHAR(500) NOT NULL,
        AltText   NVARCHAR(200) NULL,
        IsPrimary BIT           NOT NULL DEFAULT 0,
        SortOrder INT           NOT NULL DEFAULT 0,

        CONSTRAINT PK_ProductImages          PRIMARY KEY (ImageId),
        CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId) ON DELETE CASCADE,
        CONSTRAINT FK_ProductImages_Variants FOREIGN KEY (VariantId) REFERENCES dbo.ProductVariants (VariantId) ON DELETE NO ACTION
    );
END
GO

-- 8. OfferBanners
IF OBJECT_ID('dbo.OfferBanners', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OfferBanners (
        BannerId         INT           NOT NULL IDENTITY(1,1),
        Slot             NVARCHAR(20)  NOT NULL,
        Title            NVARCHAR(200) NOT NULL,
        Subtitle         NVARCHAR(300) NULL,
        BadgeText        NVARCHAR(40)  NULL,
        ImageUrl         NVARCHAR(500) NOT NULL,
        SideImageUrl     NVARCHAR(500) NULL,
        CtaLabel         NVARCHAR(60)  NULL,
        CtaUrl           NVARCHAR(500) NULL,
        SecondaryLabel   NVARCHAR(60)  NULL,
        SecondaryUrl     NVARCHAR(500) NULL,
        BackgroundColor  NVARCHAR(20)  NULL,
        TextColor        NVARCHAR(20)  NULL,
        LinkedProductId  INT           NULL,
        LinkedCategoryId INT           NULL,
        StartsAt         DATETIME2     NULL,
        EndsAt           DATETIME2     NULL,
        SortOrder        INT           NOT NULL DEFAULT 0,
        IsActive         BIT           NOT NULL DEFAULT 1,
        CreatedAt        DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt        DATETIME2     NULL,

        CONSTRAINT PK_OfferBanners PRIMARY KEY (BannerId),
        CONSTRAINT CK_OfferBanners_Slot CHECK (Slot IN ('Hero','MidLeft','MidRight','Strip')),
        CONSTRAINT FK_OfferBanners_Product  FOREIGN KEY (LinkedProductId)  REFERENCES dbo.Products (ProductId) ON DELETE SET NULL,
        CONSTRAINT FK_OfferBanners_Category FOREIGN KEY (LinkedCategoryId) REFERENCES dbo.Categories (CategoryId) ON DELETE SET NULL
    );
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- SECTION 2: MASTER SEED DATA
-- ─────────────────────────────────────────────────────────────────────────

-- 1. Roles
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'SuperAdmin')
    INSERT INTO dbo.Roles (RoleName) VALUES ('SuperAdmin');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Seller')
    INSERT INTO dbo.Roles (RoleName) VALUES ('Seller');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Buyer')
    INSERT INTO dbo.Roles (RoleName) VALUES ('Buyer');

-- 2. Base Admin & Seller Users
DECLARE @SuperAdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'SuperAdmin');
DECLARE @SellerRoleId     INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Seller');
DECLARE @PassHash         NVARCHAR(512) = '$2a$11$qJ5208qT/Y4dvhUe/S0tmeJvNnCq5v2K2zZlU1fN.p.4JtK5e5J6i';

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'Admin@merxosell.com')
    INSERT INTO dbo.Users (RoleId, FullName, Email, PasswordHash, Phone, IsActive, CreatedAt)
    VALUES (@SuperAdminRoleId, 'System SuperAdmin', 'Admin@merxosell.com', @PassHash, '+1-800-555-0100', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'seller@merxosell.com')
    INSERT INTO dbo.Users (RoleId, FullName, Email, PasswordHash, Phone, IsActive, CreatedAt)
    VALUES (@SellerRoleId, 'Official Store Seller', 'seller@merxosell.com', @PassHash, '+1-800-555-0200', 1, GETUTCDATE());

-- 3. Banners
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

PRINT 'Master Database Initialization Completed Successfully!';
GO
