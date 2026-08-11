IF OBJECT_ID('dbo.Coupons', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Coupons (
        CouponId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Coupons PRIMARY KEY,
        CouponCode NVARCHAR(50) NOT NULL,
        Title NVARCHAR(150) NOT NULL,
        Description NVARCHAR(1000) NULL,
        DiscountType NVARCHAR(30) NOT NULL,
        DiscountValue DECIMAL(18,2) NOT NULL,
        MinimumPurchaseAmount DECIMAL(18,2) NULL,
        MaximumDiscountAmount DECIMAL(18,2) NULL,
        UsageLimit INT NULL,
        UsedCount INT NOT NULL CONSTRAINT DF_Coupons_UsedCount DEFAULT 0,
        StartDate DATETIME2 NOT NULL,
        ExpiryDate DATETIME2 NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Coupons_IsActive DEFAULT 1,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Coupons_IsDeleted DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Coupons_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT CK_Coupons_DiscountType CHECK (DiscountType IN ('FixedAmount', 'Percentage')),
        CONSTRAINT CK_Coupons_DiscountValue CHECK (DiscountValue > 0),
        CONSTRAINT CK_Coupons_PercentageValue CHECK (DiscountType <> 'Percentage' OR DiscountValue <= 100),
        CONSTRAINT CK_Coupons_UsageLimit CHECK (UsageLimit IS NULL OR UsageLimit > 0),
        CONSTRAINT CK_Coupons_DateRange CHECK (ExpiryDate IS NULL OR ExpiryDate > StartDate)
    );

    CREATE UNIQUE INDEX UX_Coupons_CouponCode ON dbo.Coupons(CouponCode);
    CREATE INDEX IX_Coupons_Available ON dbo.Coupons(IsActive, StartDate, ExpiryDate) INCLUDE (UsedCount, UsageLimit);
    CREATE INDEX IX_Coupons_IsDeleted ON dbo.Coupons(IsDeleted);
END
GO

IF COL_LENGTH('dbo.Orders', 'CouponId') IS NULL
BEGIN
    ALTER TABLE dbo.Orders ADD CouponId INT NULL, CouponCode NVARCHAR(50) NULL;
    ALTER TABLE dbo.Orders WITH CHECK ADD CONSTRAINT FK_Orders_Coupons_CouponId
        FOREIGN KEY (CouponId) REFERENCES dbo.Coupons(CouponId) ON DELETE SET NULL;
    CREATE INDEX IX_Orders_CouponId ON dbo.Orders(CouponId);
END
GO

IF OBJECT_ID('dbo.CouponUsageHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CouponUsageHistory (
        CouponUsageHistoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CouponUsageHistory PRIMARY KEY,
        CouponId INT NOT NULL,
        UserId INT NOT NULL,
        OrderId INT NULL,
        OrderAmount DECIMAL(18,2) NOT NULL,
        DiscountAmount DECIMAL(18,2) NOT NULL,
        UsedAt DATETIME2 NOT NULL CONSTRAINT DF_CouponUsageHistory_UsedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CouponUsageHistory_Coupons_CouponId FOREIGN KEY (CouponId) REFERENCES dbo.Coupons(CouponId) ON DELETE CASCADE,
        CONSTRAINT FK_CouponUsageHistory_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_CouponUsageHistory_Orders_OrderId FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX UX_CouponUsageHistory_Coupon_User ON dbo.CouponUsageHistory(CouponId, UserId);
    CREATE INDEX IX_CouponUsageHistory_UserId ON dbo.CouponUsageHistory(UserId);
    CREATE INDEX IX_CouponUsageHistory_UsedAt ON dbo.CouponUsageHistory(UsedAt);
END
GO

IF OBJECT_ID('dbo.BrowsingHistories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BrowsingHistories (
        BrowsingHistoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BrowsingHistories PRIMARY KEY,
        UserId INT NOT NULL,
        ProductId INT NOT NULL,
        ViewedAt DATETIME2 NOT NULL CONSTRAINT DF_BrowsingHistories_ViewedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_BrowsingHistories_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_BrowsingHistories_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX UX_BrowsingHistories_User_Product ON dbo.BrowsingHistories(UserId, ProductId);
    CREATE INDEX IX_BrowsingHistories_ViewedAt ON dbo.BrowsingHistories(ViewedAt);
END
GO
