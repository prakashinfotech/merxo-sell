-- ============================================================
-- 002_add_manufacturers.sql
-- Adds Manufacturers table and ManufacturerId FK on Products.
-- Run against MerxoSellDb after 001_create_tables.sql.
-- ============================================================

USE MerxoSellDb;
GO

-- ── Manufacturers ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Manufacturers')
BEGIN
    CREATE TABLE Manufacturers (
        ManufacturerId INT           IDENTITY(1,1) PRIMARY KEY,
        Name           NVARCHAR(200) NOT NULL,
        ContactEmail   NVARCHAR(200) NULL,
        Phone          NVARCHAR(50)  NULL,
        Address        NVARCHAR(500) NULL,
        Country        NVARCHAR(100) NULL,
        Website        NVARCHAR(300) NULL,
        IsActive       BIT           NOT NULL DEFAULT 1,
        CreatedAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt      DATETIME2     NULL
    );
    PRINT 'Manufacturers table created.';
END
ELSE
    PRINT 'Manufacturers table already exists — skipped.';
GO

-- ── Add ManufacturerId FK to Products ─────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('Products') AND name = 'ManufacturerId'
)
BEGIN
    ALTER TABLE Products
        ADD ManufacturerId INT NULL
            CONSTRAINT FK_Products_Manufacturers
            FOREIGN KEY REFERENCES Manufacturers(ManufacturerId)
            ON DELETE SET NULL;
    PRINT 'ManufacturerId column added to Products.';
END
ELSE
    PRINT 'ManufacturerId already exists on Products — skipped.';
GO

-- ── Seed sample manufacturers ─────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM Manufacturers WHERE Name = 'TechVision Electronics')
    INSERT INTO Manufacturers (Name, ContactEmail, Phone, Country, Website, IsActive)
    VALUES ('TechVision Electronics', 'orders@techvision.com', '+1-800-555-0101', 'Canada', 'https://techvision.example.com', 1);

IF NOT EXISTS (SELECT 1 FROM Manufacturers WHERE Name = 'FashionForward Co.')
    INSERT INTO Manufacturers (Name, ContactEmail, Phone, Country, Website, IsActive)
    VALUES ('FashionForward Co.', 'supply@fashionforward.com', '+1-800-555-0202', 'United States', 'https://fashionforward.example.com', 1);

IF NOT EXISTS (SELECT 1 FROM Manufacturers WHERE Name = 'ActiveLife Sports')
    INSERT INTO Manufacturers (Name, ContactEmail, Phone, Country, Website, IsActive)
    VALUES ('ActiveLife Sports', 'b2b@activelife.com', '+1-800-555-0303', 'Canada', 'https://activelife.example.com', 1);

PRINT 'Sample manufacturers seeded.';
GO

-- ── Link existing products to manufacturers ───────────────────────────────────
DECLARE @TechId   INT = (SELECT ManufacturerId FROM Manufacturers WHERE Name = 'TechVision Electronics');
DECLARE @FashId   INT = (SELECT ManufacturerId FROM Manufacturers WHERE Name = 'FashionForward Co.');
DECLARE @SportId  INT = (SELECT ManufacturerId FROM Manufacturers WHERE Name = 'ActiveLife Sports');

UPDATE Products SET ManufacturerId = @TechId  WHERE Slug = 'wireless-earbuds-pro'  AND ManufacturerId IS NULL;
UPDATE Products SET ManufacturerId = @FashId  WHERE Slug = 'classic-white-tshirt'  AND ManufacturerId IS NULL;
UPDATE Products SET ManufacturerId = @SportId WHERE Slug = 'yoga-mat-premium'       AND ManufacturerId IS NULL;

PRINT 'Products linked to manufacturers.';
GO

-- ── Verification ──────────────────────────────────────────────────────────────
SELECT 'Manufacturers' AS [Table], COUNT(*) AS [Rows] FROM Manufacturers
UNION ALL
SELECT 'Products with manufacturer', COUNT(*) FROM Products WHERE ManufacturerId IS NOT NULL;
GO
