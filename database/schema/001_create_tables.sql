-- ============================================================
-- Temu-Clone E-Commerce Platform — Initial Schema
-- Run scripts in this order to satisfy foreign key dependencies.
-- Target: SQL Server (SSMS / Azure SQL)
-- ============================================================

USE MerxoSellDb;
GO

-- ─────────────────────────────────────────────
-- 1. Roles
-- ─────────────────────────────────────────────
CREATE TABLE Roles (
    RoleId   INT            NOT NULL IDENTITY(1,1),
    RoleName NVARCHAR(50)   NOT NULL,

    CONSTRAINT PK_Roles      PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Roles_Name UNIQUE      (RoleName)
);
GO

-- ─────────────────────────────────────────────
-- 2. Users
-- ─────────────────────────────────────────────
CREATE TABLE Users (
    UserId       INT             NOT NULL IDENTITY(1,1),
    RoleId       INT             NOT NULL,
    FullName     NVARCHAR(150)   NOT NULL,
    Email        NVARCHAR(256)   NOT NULL,
    PasswordHash NVARCHAR(512)   NOT NULL,
    Phone        NVARCHAR(20)    NULL,
    IsActive     BIT             NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME2       NULL,

    CONSTRAINT PK_Users          PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email    UNIQUE      (Email),
    CONSTRAINT FK_Users_Roles    FOREIGN KEY (RoleId)
        REFERENCES Roles (RoleId)
);
GO

-- ─────────────────────────────────────────────
-- 3. Addresses
-- ─────────────────────────────────────────────
CREATE TABLE Addresses (
    AddressId    INT            NOT NULL IDENTITY(1,1),
    UserId       INT            NOT NULL,
    FullName     NVARCHAR(150)  NOT NULL,
    Phone        NVARCHAR(20)   NOT NULL,
    AddressLine1 NVARCHAR(250)  NOT NULL,
    AddressLine2 NVARCHAR(250)  NULL,
    City         NVARCHAR(100)  NOT NULL,
    State        NVARCHAR(100)  NULL,
    PostalCode   NVARCHAR(20)   NOT NULL,
    Country      NVARCHAR(100)  NOT NULL DEFAULT 'US',
    IsDefault    BIT            NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2      NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Addresses       PRIMARY KEY (AddressId),
    CONSTRAINT FK_Addresses_Users FOREIGN KEY (UserId)
        REFERENCES Users (UserId) ON DELETE CASCADE
);
GO

-- ─────────────────────────────────────────────
-- 4. Categories  (self-referencing)
-- ─────────────────────────────────────────────
CREATE TABLE Categories (
    CategoryId       INT           NOT NULL IDENTITY(1,1),
    ParentCategoryId INT           NULL,
    Name             NVARCHAR(100) NOT NULL,
    Slug             NVARCHAR(120) NOT NULL,
    ImageUrl         NVARCHAR(500) NULL,
    SortOrder        INT           NOT NULL DEFAULT 0,
    IsActive         BIT           NOT NULL DEFAULT 1,

    CONSTRAINT PK_Categories        PRIMARY KEY (CategoryId),
    CONSTRAINT UQ_Categories_Slug   UNIQUE      (Slug),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryId)
        REFERENCES Categories (CategoryId)
);
GO

-- ─────────────────────────────────────────────
-- 5. Products
-- ─────────────────────────────────────────────
CREATE TABLE Products (
    ProductId   INT             NOT NULL IDENTITY(1,1),
    CategoryId  INT             NOT NULL,
    Name        NVARCHAR(300)   NOT NULL,
    Slug        NVARCHAR(320)   NOT NULL,
    Description NVARCHAR(MAX)   NULL,
    BasePrice   DECIMAL(18,2)   NOT NULL,
    SalePrice   DECIMAL(18,2)   NULL,
    Stock       INT             NOT NULL DEFAULT 0,
    IsActive    BIT             NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2       NOT NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME2       NULL,

    CONSTRAINT PK_Products            PRIMARY KEY (ProductId),
    CONSTRAINT UQ_Products_Slug       UNIQUE      (Slug),
    CONSTRAINT CK_Products_BasePrice  CHECK       (BasePrice >= 0),
    CONSTRAINT CK_Products_SalePrice  CHECK       (SalePrice IS NULL OR SalePrice >= 0),
    CONSTRAINT CK_Products_Stock      CHECK       (Stock >= 0),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)
        REFERENCES Categories (CategoryId)
);
GO

-- ─────────────────────────────────────────────
-- 6. ProductImages
-- ─────────────────────────────────────────────
CREATE TABLE ProductImages (
    ImageId    INT            NOT NULL IDENTITY(1,1),
    ProductId  INT            NOT NULL,
    ImageUrl   NVARCHAR(500)  NOT NULL,
    AltText    NVARCHAR(200)  NULL,
    IsPrimary  BIT            NOT NULL DEFAULT 0,
    SortOrder  INT            NOT NULL DEFAULT 0,

    CONSTRAINT PK_ProductImages          PRIMARY KEY (ImageId),
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId)
        REFERENCES Products (ProductId) ON DELETE CASCADE
);
GO

-- ─────────────────────────────────────────────
-- 7. ProductVariants
-- ─────────────────────────────────────────────
CREATE TABLE ProductVariants (
    VariantId   INT            NOT NULL IDENTITY(1,1),
    ProductId   INT            NOT NULL,
    Color       NVARCHAR(50)   NULL,
    Size        NVARCHAR(50)   NULL,
    PriceDelta  DECIMAL(18,2)  NOT NULL DEFAULT 0.00,
    Stock       INT            NOT NULL DEFAULT 0,
    SKU         NVARCHAR(100)  NULL,
    IsActive    BIT            NOT NULL DEFAULT 1,

    CONSTRAINT PK_ProductVariants            PRIMARY KEY (VariantId),
    CONSTRAINT UQ_ProductVariants_SKU        UNIQUE      (SKU),
    CONSTRAINT CK_ProductVariants_Stock      CHECK       (Stock >= 0),
    CONSTRAINT CK_ProductVariants_PriceDelta CHECK       (PriceDelta >= -9999),
    CONSTRAINT FK_ProductVariants_Products   FOREIGN KEY (ProductId)
        REFERENCES Products (ProductId) ON DELETE CASCADE
);
GO

-- ─────────────────────────────────────────────
-- 8. Cart  (one per user)
-- ─────────────────────────────────────────────
CREATE TABLE Cart (
    CartId    INT       NOT NULL IDENTITY(1,1),
    UserId    INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT PK_Cart       PRIMARY KEY (CartId),
    CONSTRAINT UQ_Cart_User  UNIQUE      (UserId),
    CONSTRAINT FK_Cart_Users FOREIGN KEY (UserId)
        REFERENCES Users (UserId) ON DELETE CASCADE
);
GO

-- ─────────────────────────────────────────────
-- 9. CartItems
-- ─────────────────────────────────────────────
CREATE TABLE CartItems (
    CartItemId INT       NOT NULL IDENTITY(1,1),
    CartId     INT       NOT NULL,
    ProductId  INT       NOT NULL,
    VariantId  INT       NULL,
    Quantity   INT       NOT NULL DEFAULT 1,
    AddedAt    DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_CartItems          PRIMARY KEY (CartItemId),
    CONSTRAINT CK_CartItems_Quantity CHECK       (Quantity > 0),
    CONSTRAINT UQ_CartItems_Product  UNIQUE      (CartId, ProductId, VariantId),
    CONSTRAINT FK_CartItems_Cart     FOREIGN KEY (CartId)
        REFERENCES Cart (CartId) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId)
        REFERENCES Products (ProductId),
    CONSTRAINT FK_CartItems_Variants FOREIGN KEY (VariantId)
        REFERENCES ProductVariants (VariantId)
);
GO

-- ─────────────────────────────────────────────
-- 10. Orders
-- ─────────────────────────────────────────────
CREATE TABLE Orders (
    OrderId        INT            NOT NULL IDENTITY(1,1),
    UserId         INT            NOT NULL,
    AddressId      INT            NULL,
    Status         NVARCHAR(30)   NOT NULL DEFAULT 'Pending',
    TotalAmount    DECIMAL(18,2)  NOT NULL,
    ShippingAmount DECIMAL(18,2)  NOT NULL DEFAULT 0.00,
    DiscountAmount DECIMAL(18,2)  NOT NULL DEFAULT 0.00,
    Notes          NVARCHAR(500)  NULL,
    CreatedAt      DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt      DATETIME2      NULL,

    CONSTRAINT PK_Orders             PRIMARY KEY (OrderId),
    CONSTRAINT CK_Orders_TotalAmount CHECK       (TotalAmount >= 0),
    CONSTRAINT CK_Orders_Status      CHECK       (Status IN (
        'Pending','Confirmed','Processing','Shipped','Delivered','Cancelled','Refunded'
    )),
    CONSTRAINT FK_Orders_Users       FOREIGN KEY (UserId)
        REFERENCES Users (UserId),
    CONSTRAINT FK_Orders_Addresses   FOREIGN KEY (AddressId)
        REFERENCES Addresses (AddressId) ON DELETE SET NULL
);
GO

-- ─────────────────────────────────────────────
-- 11. OrderItems  (price snapshot at purchase time)
-- ─────────────────────────────────────────────
CREATE TABLE OrderItems (
    OrderItemId INT            NOT NULL IDENTITY(1,1),
    OrderId     INT            NOT NULL,
    ProductId   INT            NOT NULL,
    VariantId   INT            NULL,
    ProductName NVARCHAR(300)  NOT NULL,
    VariantInfo NVARCHAR(100)  NULL,
    Quantity    INT            NOT NULL,
    UnitPrice   DECIMAL(18,2)  NOT NULL,

    CONSTRAINT PK_OrderItems           PRIMARY KEY (OrderItemId),
    CONSTRAINT CK_OrderItems_Quantity  CHECK       (Quantity > 0),
    CONSTRAINT CK_OrderItems_UnitPrice CHECK       (UnitPrice >= 0),
    CONSTRAINT FK_OrderItems_Orders    FOREIGN KEY (OrderId)
        REFERENCES Orders (OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products  FOREIGN KEY (ProductId)
        REFERENCES Products (ProductId),
    CONSTRAINT FK_OrderItems_Variants  FOREIGN KEY (VariantId)
        REFERENCES ProductVariants (VariantId) ON DELETE SET NULL
);
GO

-- ─────────────────────────────────────────────
-- 12. Reviews  (one per user per product)
-- ─────────────────────────────────────────────
CREATE TABLE Reviews (
    ReviewId  INT            NOT NULL IDENTITY(1,1),
    ProductId INT            NOT NULL,
    UserId    INT            NOT NULL,
    Rating    TINYINT        NOT NULL,
    Comment   NVARCHAR(2000) NULL,
    CreatedAt DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2      NULL,

    CONSTRAINT PK_Reviews             PRIMARY KEY (ReviewId),
    CONSTRAINT UQ_Reviews_UserProduct UNIQUE      (UserId, ProductId),
    CONSTRAINT CK_Reviews_Rating      CHECK       (Rating BETWEEN 1 AND 5),
    CONSTRAINT FK_Reviews_Products    FOREIGN KEY (ProductId)
        REFERENCES Products (ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_Reviews_Users       FOREIGN KEY (UserId)
        REFERENCES Users (UserId)
);
GO

-- ─────────────────────────────────────────────
-- Indexes
-- ─────────────────────────────────────────────

-- Products (most queried table)
CREATE INDEX IX_Products_CategoryId
    ON Products (CategoryId)
    INCLUDE (Name, BasePrice, SalePrice, Stock, IsActive);

CREATE INDEX IX_Products_IsActive_CreatedAt
    ON Products (IsActive, CreatedAt DESC);

CREATE INDEX IX_Products_BasePrice
    ON Products (BasePrice)
    WHERE IsActive = 1;

CREATE INDEX IX_Products_Name
    ON Products (Name);

-- ProductImages
CREATE INDEX IX_ProductImages_ProductId
    ON ProductImages (ProductId)
    INCLUDE (ImageUrl, IsPrimary, SortOrder);

-- ProductVariants
CREATE INDEX IX_ProductVariants_ProductId
    ON ProductVariants (ProductId)
    INCLUDE (Color, Size, PriceDelta, Stock, IsActive);

-- CartItems
CREATE INDEX IX_CartItems_CartId
    ON CartItems (CartId)
    INCLUDE (ProductId, VariantId, Quantity);

-- Orders
CREATE INDEX IX_Orders_UserId_CreatedAt
    ON Orders (UserId, CreatedAt DESC);

CREATE INDEX IX_Orders_Status
    ON Orders (Status)
    INCLUDE (UserId, TotalAmount, CreatedAt);

-- OrderItems
CREATE INDEX IX_OrderItems_OrderId
    ON OrderItems (OrderId)
    INCLUDE (ProductId, Quantity, UnitPrice);

-- Reviews
CREATE INDEX IX_Reviews_ProductId
    ON Reviews (ProductId)
    INCLUDE (Rating, UserId, CreatedAt);

CREATE INDEX IX_Reviews_UserId
    ON Reviews (UserId);

-- Addresses
CREATE INDEX IX_Addresses_UserId
    ON Addresses (UserId)
    INCLUDE (IsDefault);

-- Categories
CREATE INDEX IX_Categories_ParentCategoryId
    ON Categories (ParentCategoryId);
GO
