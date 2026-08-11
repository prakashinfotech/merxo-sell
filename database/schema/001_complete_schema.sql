-- =========================================================================
-- MerxoSell E-Commerce Platform — Complete Unified Database Schema
-- Combines initial table creation and all migration scripts (001 - 014).
-- Target: Microsoft SQL Server (SSMS / LocalDB / Azure SQL)
-- =========================================================================

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'MerxoSellDb')
BEGIN
    CREATE DATABASE MerxoSellDb;
END
GO

USE MerxoSellDb;
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 1. Roles
-- ─────────────────────────────────────────────────────────────────────────
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

-- ─────────────────────────────────────────────────────────────────────────
-- 2. Users
-- ─────────────────────────────────────────────────────────────────────────
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
    CREATE INDEX IX_Users_IsActive_IsDelete ON dbo.Users (IsActive, IsDelete);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 3. Sellers
-- ─────────────────────────────────────────────────────────────────────────
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
    CREATE INDEX IX_Sellers_UserId   ON dbo.Sellers (UserId);
    CREATE INDEX IX_Sellers_IsActive ON dbo.Sellers (IsActive);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 4. Manufacturers
-- ─────────────────────────────────────────────────────────────────────────
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

-- ─────────────────────────────────────────────────────────────────────────
-- 5. Categories
-- ─────────────────────────────────────────────────────────────────────────
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
    CREATE INDEX IX_Categories_ParentCategoryId ON dbo.Categories (ParentCategoryId);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 6. Products
-- ─────────────────────────────────────────────────────────────────────────
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
        CONSTRAINT CK_Products_SalePrice  CHECK       (SalePrice IS NULL OR SalePrice >= 0),
        CONSTRAINT CK_Products_Stock      CHECK       (Stock >= 0),
        CONSTRAINT CK_Products_Status     CHECK       (Status IN ('Pending','Approved','Rejected')),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)     REFERENCES dbo.Categories (CategoryId),
        CONSTRAINT FK_Products_Sellers    FOREIGN KEY (SellerId)       REFERENCES dbo.Sellers (SellerId),
        CONSTRAINT FK_Products_Mfg        FOREIGN KEY (ManufacturerId) REFERENCES dbo.Manufacturers (ManufacturerId) ON DELETE SET NULL
    );

    CREATE INDEX IX_Products_CategoryId   ON dbo.Products (CategoryId);
    CREATE INDEX IX_Products_SellerId     ON dbo.Products (SellerId);
    CREATE INDEX IX_Products_Status       ON dbo.Products (Status);
    CREATE INDEX IX_Products_Name         ON dbo.Products (Name);
    CREATE INDEX IX_Products_CreatedAt    ON dbo.Products (CreatedAt DESC);
    CREATE INDEX IX_Products_Status_Active ON dbo.Products (Status, IsActive, IsDelete);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 7. ProductColors & ProductSizes
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.ProductColors', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductColors (
        ColorId   INT          NOT NULL IDENTITY(1,1),
        ProductId INT          NOT NULL,
        ColorName NVARCHAR(50) NOT NULL,
        HexCode   NVARCHAR(10) NULL,

        CONSTRAINT PK_ProductColors PRIMARY KEY (ColorId),
        CONSTRAINT FK_ProductColors_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProductColors_ProductId ON dbo.ProductColors (ProductId);
END
GO

IF OBJECT_ID('dbo.ProductSizes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductSizes (
        SizeId    INT          NOT NULL IDENTITY(1,1),
        ProductId INT          NOT NULL,
        SizeName  NVARCHAR(50) NOT NULL,

        CONSTRAINT PK_ProductSizes PRIMARY KEY (SizeId),
        CONSTRAINT FK_ProductSizes_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProductSizes_ProductId ON dbo.ProductSizes (ProductId);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 8. ProductVariants & ProductVariantImages
-- ─────────────────────────────────────────────────────────────────────────
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

        CONSTRAINT PK_ProductVariants            PRIMARY KEY (VariantId),
        CONSTRAINT UQ_ProductVariants_SKU        UNIQUE      (SKU),
        CONSTRAINT CK_ProductVariants_Stock      CHECK       (Stock >= 0),
        CONSTRAINT FK_ProductVariants_Products   FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProductVariants_ProductId ON dbo.ProductVariants (ProductId);
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
    CREATE INDEX IX_ProductImages_ProductId ON dbo.ProductImages (ProductId);
END
GO

IF OBJECT_ID('dbo.ProductVariantImages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariantImages (
        VariantId INT NOT NULL,
        ImageId   INT NOT NULL,

        CONSTRAINT PK_ProductVariantImages PRIMARY KEY (VariantId, ImageId),
        CONSTRAINT FK_PVI_Variant FOREIGN KEY (VariantId) REFERENCES dbo.ProductVariants (VariantId) ON DELETE CASCADE,
        CONSTRAINT FK_PVI_Image   FOREIGN KEY (ImageId)   REFERENCES dbo.ProductImages (ImageId)     ON DELETE NO ACTION
    );
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 9. Addresses
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Addresses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Addresses (
        AddressId    INT           NOT NULL IDENTITY(1,1),
        UserId       INT           NOT NULL,
        Label        NVARCHAR(50)  NULL,
        FullName     NVARCHAR(150) NOT NULL,
        Phone        NVARCHAR(20)  NOT NULL,
        AddressLine1 NVARCHAR(250) NOT NULL,
        AddressLine2 NVARCHAR(250) NULL,
        City         NVARCHAR(100) NOT NULL,
        State        NVARCHAR(100) NULL,
        PostalCode   NVARCHAR(20)  NOT NULL,
        Country      NVARCHAR(100) NOT NULL DEFAULT 'US',
        IsDefault    BIT           NOT NULL DEFAULT 0,
        CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT PK_Addresses       PRIMARY KEY (AddressId),
        CONSTRAINT FK_Addresses_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId) ON DELETE CASCADE
    );
    CREATE INDEX IX_Addresses_UserId ON dbo.Addresses (UserId, IsDefault);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 10. Cart & CartItems
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Cart', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cart (
        CartId    INT       NOT NULL IDENTITY(1,1),
        UserId    INT       NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,

        CONSTRAINT PK_Cart       PRIMARY KEY (CartId),
        CONSTRAINT UQ_Cart_User  UNIQUE      (UserId),
        CONSTRAINT FK_Cart_Users FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems (
        CartItemId INT       NOT NULL IDENTITY(1,1),
        CartId     INT       NOT NULL,
        ProductId  INT       NOT NULL,
        VariantId  INT       NULL,
        Quantity   INT       NOT NULL DEFAULT 1,
        AddedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT PK_CartItems          PRIMARY KEY (CartItemId),
        CONSTRAINT CK_CartItems_Quantity CHECK       (Quantity > 0),
        CONSTRAINT UQ_CartItems_Product  UNIQUE      (CartId, ProductId, VariantId),
        CONSTRAINT FK_CartItems_Cart     FOREIGN KEY (CartId)    REFERENCES dbo.Cart (CartId) ON DELETE CASCADE,
        CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId),
        CONSTRAINT FK_CartItems_Variants FOREIGN KEY (VariantId) REFERENCES dbo.ProductVariants (VariantId)
    );
    CREATE INDEX IX_CartItems_CartId ON dbo.CartItems (CartId);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 11. Coupons & Usage History
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Coupons', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Coupons (
        CouponId              INT           IDENTITY(1,1) NOT NULL,
        CouponCode            NVARCHAR(50)  NOT NULL,
        Title                 NVARCHAR(150) NOT NULL,
        Description           NVARCHAR(1000) NULL,
        DiscountType          NVARCHAR(30)  NOT NULL,
        DiscountValue         DECIMAL(18,2) NOT NULL,
        MinimumPurchaseAmount DECIMAL(18,2) NULL,
        MaximumDiscountAmount DECIMAL(18,2) NULL,
        UsageLimit            INT           NULL,
        UsedCount             INT           NOT NULL DEFAULT 0,
        StartDate             DATETIME2     NOT NULL,
        ExpiryDate            DATETIME2     NULL,
        IsActive              BIT           NOT NULL DEFAULT 1,
        IsDeleted             BIT           NOT NULL DEFAULT 0,
        CreatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt             DATETIME2     NULL,

        CONSTRAINT PK_Coupons PRIMARY KEY (CouponId),
        CONSTRAINT CK_Coupons_DiscountType CHECK (DiscountType IN ('FixedAmount', 'Percentage')),
        CONSTRAINT CK_Coupons_DiscountValue CHECK (DiscountValue > 0)
    );
    CREATE UNIQUE INDEX UX_Coupons_CouponCode ON dbo.Coupons (CouponCode);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 12. Orders & OrderItems
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        OrderId            INT           NOT NULL IDENTITY(1,1),
        UserId             INT           NOT NULL,
        AddressId          INT           NULL,
        CouponId           INT           NULL,
        CouponCode         NVARCHAR(50)  NULL,
        Status             NVARCHAR(30)  NOT NULL DEFAULT 'Pending',
        TotalAmount        DECIMAL(18,2) NOT NULL,
        ShippingAmount     DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        DiscountAmount     DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        Notes              NVARCHAR(500) NULL,
        CancellationReason NVARCHAR(1000) NULL,
        ApprovedBy         INT           NULL,
        CreatedAt          DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt          DATETIME2     NULL,

        CONSTRAINT PK_Orders             PRIMARY KEY (OrderId),
        CONSTRAINT CK_Orders_TotalAmount CHECK       (TotalAmount >= 0),
        CONSTRAINT FK_Orders_Users       FOREIGN KEY (UserId)    REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_Orders_Addresses   FOREIGN KEY (AddressId) REFERENCES dbo.Addresses (AddressId) ON DELETE SET NULL,
        CONSTRAINT FK_Orders_Coupons     FOREIGN KEY (CouponId)  REFERENCES dbo.Coupons (CouponId) ON DELETE SET NULL
    );
    CREATE INDEX IX_Orders_UserId_CreatedAt ON dbo.Orders (UserId, CreatedAt DESC);
    CREATE INDEX IX_Orders_Status           ON dbo.Orders (Status);
END
GO

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemId INT           NOT NULL IDENTITY(1,1),
        OrderId     INT           NOT NULL,
        ProductId   INT           NOT NULL,
        VariantId   INT           NULL,
        ProductName NVARCHAR(300) NOT NULL,
        VariantInfo NVARCHAR(100) NULL,
        Quantity    INT           NOT NULL,
        UnitPrice   DECIMAL(18,2) NOT NULL,

        CONSTRAINT PK_OrderItems           PRIMARY KEY (OrderItemId),
        CONSTRAINT CK_OrderItems_Quantity  CHECK       (Quantity > 0),
        CONSTRAINT CK_OrderItems_UnitPrice CHECK       (UnitPrice >= 0),
        CONSTRAINT FK_OrderItems_Orders    FOREIGN KEY (OrderId)   REFERENCES dbo.Orders (OrderId) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products  FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId),
        CONSTRAINT FK_OrderItems_Variants  FOREIGN KEY (VariantId) REFERENCES dbo.ProductVariants (VariantId) ON DELETE SET NULL
    );
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems (OrderId);
END
GO

IF OBJECT_ID('dbo.OrderStatusHistories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderStatusHistories (
        OrderStatusHistoryId INT          IDENTITY(1,1) PRIMARY KEY,
        OrderId              INT          NOT NULL,
        FromStatus           NVARCHAR(30) NOT NULL,
        ToStatus             NVARCHAR(30) NOT NULL,
        Note                 NVARCHAR(500) NULL,
        ChangedBy            INT          NULL,
        ChangedByRole        NVARCHAR(30) NULL,
        CreatedAt            DATETIME2    NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_OrderStatusHistory_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders (OrderId) ON DELETE CASCADE,
        CONSTRAINT FK_OrderStatusHistory_Users  FOREIGN KEY (ChangedBy) REFERENCES dbo.Users (UserId) ON DELETE SET NULL
    );
    CREATE INDEX IX_OrderStatusHistory_OrderId ON dbo.OrderStatusHistories (OrderId);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 13. OfferBanners
-- ─────────────────────────────────────────────────────────────────────────
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
    CREATE INDEX IX_OfferBanners_Slot_Active ON dbo.OfferBanners (Slot, IsActive, SortOrder);
END
GO

-- ─────────────────────────────────────────────────────────────────────────
-- 14. Reviews & Browsing History
-- ─────────────────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Reviews', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews (
        ReviewId    INT            NOT NULL IDENTITY(1,1),
        ProductId   INT            NOT NULL,
        UserId      INT            NOT NULL,
        Rating      TINYINT        NOT NULL,
        Comment     NVARCHAR(2000) NULL,
        Status      NVARCHAR(20)   NOT NULL DEFAULT 'Approved',
        FlagReason  NVARCHAR(500)  NULL,
        ModeratedAt DATETIME2      NULL,
        ModeratedBy INT            NULL,
        CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt   DATETIME2      NULL,

        CONSTRAINT PK_Reviews          PRIMARY KEY (ReviewId),
        CONSTRAINT UQ_Reviews_UserProd UNIQUE      (UserId, ProductId),
        CONSTRAINT CK_Reviews_Rating   CHECK       (Rating BETWEEN 1 AND 5),
        CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId) ON DELETE CASCADE,
        CONSTRAINT FK_Reviews_Users    FOREIGN KEY (UserId)    REFERENCES dbo.Users (UserId)
    );
    CREATE INDEX IX_Reviews_ProductId ON dbo.Reviews (ProductId);
    CREATE INDEX IX_Reviews_Status    ON dbo.Reviews (Status);
END
GO

IF OBJECT_ID('dbo.BrowsingHistories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BrowsingHistories (
        BrowsingHistoryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId            INT NOT NULL,
        ProductId         INT NOT NULL,
        ViewedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_BrowsingHistories_Users    FOREIGN KEY (UserId)    REFERENCES dbo.Users (UserId) ON DELETE CASCADE,
        CONSTRAINT FK_BrowsingHistories_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UX_BrowsingHistories_User_Product ON dbo.BrowsingHistories (UserId, ProductId);
END
GO

PRINT 'Complete Database Schema creation script completed successfully.';
GO
