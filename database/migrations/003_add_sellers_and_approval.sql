-- ============================================================
-- 003_add_sellers_and_approval.sql
-- Adds: Sellers, ProductApprovalLogs, ProductViews tables.
-- Alters: Products (+ SellerId, Status, ApprovalNote, ViewCount).
-- Run against MerxoSellDb AFTER 002_add_manufacturers.sql.
-- ============================================================

USE MerxoSellDb;
GO

-- ── 1. Sellers ─────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Sellers')
BEGIN
    CREATE TABLE Sellers (
        SellerId         INT            IDENTITY(1,1) PRIMARY KEY,
        UserId           INT            NOT NULL,
        StoreName        NVARCHAR(200)  NOT NULL,
        StoreDescription NVARCHAR(1000) NULL,
        ContactEmail     NVARCHAR(200)  NULL,
        Phone            NVARCHAR(50)   NULL,
        IsVerified       BIT            NOT NULL DEFAULT 0,
        IsActive         BIT            NOT NULL DEFAULT 1,
        CreatedAt        DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt        DATETIME2      NULL,

        CONSTRAINT FK_Sellers_Users
            FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT UQ_Sellers_UserId UNIQUE (UserId)
    );

    CREATE INDEX IX_Sellers_UserId   ON Sellers(UserId);
    CREATE INDEX IX_Sellers_IsActive ON Sellers(IsActive);

    PRINT 'Sellers table created.';
END
ELSE
    PRINT 'Sellers table already exists — skipped.';
GO

-- ── 2. Add SellerId + Status columns to Products ───────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('Products') AND name = 'SellerId'
)
BEGIN
    -- Temporarily allow NULL so existing rows don't fail the NOT NULL constraint
    ALTER TABLE Products ADD SellerId INT NULL;

    PRINT 'Products.SellerId column added.';
END
ELSE
    PRINT 'Products.SellerId already exists — skipped.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('Products') AND name = 'Status'
)
BEGIN
    ALTER TABLE Products ADD
        Status       NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
        ApprovalNote NVARCHAR(1000) NULL,
        ViewCount    INT            NOT NULL DEFAULT 0;

    EXEC('ALTER TABLE Products ADD CONSTRAINT CK_Products_Status CHECK (Status IN (''Pending'',''Approved'',''Rejected''))');

    EXEC('CREATE INDEX IX_Products_Status   ON Products(Status)');
    EXEC('CREATE INDEX IX_Products_SellerId ON Products(SellerId)');

    PRINT 'Products.Status, ApprovalNote, ViewCount columns added.';
END
ELSE
    PRINT 'Products.Status already exists — skipped.';
GO

-- ── 3. Seed default Seller record and link existing products ───────────────────
-- Insert a placeholder admin/system seller so existing products can get a SellerId
DECLARE @SystemSellerId INT;

IF NOT EXISTS (SELECT 1 FROM Sellers WHERE StoreName = 'System Seller')
BEGIN
    -- Use the first SuperAdmin user as the system seller owner
    DECLARE @SystemUserId INT = (SELECT TOP 1 u.UserId FROM Users u
                                  JOIN Roles r ON u.RoleId = r.RoleId
                                  WHERE r.RoleName = 'SuperAdmin'
                                  ORDER BY u.UserId);

    IF @SystemUserId IS NOT NULL
    BEGIN
        INSERT INTO Sellers (UserId, StoreName, IsVerified, IsActive)
        VALUES (@SystemUserId, 'System Seller', 1, 1);

        SET @SystemSellerId = SCOPE_IDENTITY();
        PRINT 'System Seller created.';
    END
END
ELSE
    SET @SystemSellerId = (SELECT SellerId FROM Sellers WHERE StoreName = 'System Seller');

-- Assign all existing orphan products to the system seller
IF @SystemSellerId IS NOT NULL
BEGIN
    UPDATE Products SET SellerId = @SystemSellerId WHERE SellerId IS NULL;
    PRINT 'Orphan products assigned to System Seller.';
END
GO

-- Now enforce NOT NULL + FK on Products.SellerId
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Products') AND name = 'SellerId' AND is_nullable = 1
)
BEGIN
    -- Drop index if it exists to allow altering the column
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_SellerId' AND object_id = OBJECT_ID('Products'))
    BEGIN
        DROP INDEX IX_Products_SellerId ON Products;
    END

    ALTER TABLE Products ALTER COLUMN SellerId INT NOT NULL;

    -- Add the foreign key constraint
    ALTER TABLE Products ADD CONSTRAINT FK_Products_Sellers
        FOREIGN KEY (SellerId) REFERENCES Sellers(SellerId);

    -- Recreate the index
    CREATE INDEX IX_Products_SellerId ON Products(SellerId);

    PRINT 'Products.SellerId FK constraint added and index recreated.';
END
GO

-- ── 4. ProductApprovalLogs ─────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductApprovalLogs')
BEGIN
    CREATE TABLE ProductApprovalLogs (
        LogId       INT            IDENTITY(1,1) PRIMARY KEY,
        ProductId   INT            NOT NULL,
        ReviewedBy  INT            NULL,
        OldStatus   NVARCHAR(20)   NOT NULL,
        NewStatus   NVARCHAR(20)   NOT NULL,
        Note        NVARCHAR(1000) NULL,
        CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_ApprovalLogs_Products
            FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
        CONSTRAINT FK_ApprovalLogs_Users
            FOREIGN KEY (ReviewedBy) REFERENCES Users(UserId) ON DELETE SET NULL
    );

    CREATE INDEX IX_ApprovalLogs_ProductId ON ProductApprovalLogs(ProductId);

    PRINT 'ProductApprovalLogs table created.';
END
ELSE
    PRINT 'ProductApprovalLogs table already exists — skipped.';
GO

-- ── 5. ProductViews ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductViews')
BEGIN
    CREATE TABLE ProductViews (
        ViewId     INT          IDENTITY(1,1) PRIMARY KEY,
        ProductId  INT          NOT NULL,
        UserId     INT          NULL,
        ViewedAt   DATETIME2    NOT NULL DEFAULT GETUTCDATE(),
        IpAddress  NVARCHAR(45) NULL,

        CONSTRAINT FK_ProductViews_Products
            FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
        CONSTRAINT FK_ProductViews_Users
            FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL
    );

    CREATE INDEX IX_ProductViews_ProductId ON ProductViews(ProductId);
    CREATE INDEX IX_ProductViews_ViewedAt  ON ProductViews(ViewedAt);

    PRINT 'ProductViews table created.';
END
ELSE
    PRINT 'ProductViews table already exists — skipped.';
GO

-- ── 6. Verification ───────────────────────────────────────────────────────────
EXEC('
SELECT ''Sellers''             AS [Table], COUNT(*) AS [Rows] FROM Sellers
UNION ALL
SELECT ''Products (Pending)'', COUNT(*) FROM Products WHERE Status = ''Pending''
UNION ALL
SELECT ''ProductApprovalLogs'', COUNT(*) FROM ProductApprovalLogs
UNION ALL
SELECT ''ProductViews'',        COUNT(*) FROM ProductViews;
');
GO
