-- ============================================================
-- 011_add_soft_delete_flags.sql
-- Adds: IsDelete column to Users, Products, Categories, and Sellers.
-- Sets default value 0 for existing and new entries.
-- ============================================================

USE MerxoSellDb;
GO

-- 1. Users Table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Users') AND name = 'IsDelete'
)
BEGIN
    ALTER TABLE Users ADD IsDelete BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDelete column to Users table.';
END
ELSE
BEGIN
    PRINT 'IsDelete column already exists in Users table.';
END
GO

-- 2. Products Table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Products') AND name = 'IsDelete'
)
BEGIN
    ALTER TABLE Products ADD IsDelete BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDelete column to Products table.';
END
ELSE
BEGIN
    PRINT 'IsDelete column already exists in Products table.';
END
GO

-- 3. Categories Table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Categories') AND name = 'IsDelete'
)
BEGIN
    ALTER TABLE Categories ADD IsDelete BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDelete column to Categories table.';
END
ELSE
BEGIN
    PRINT 'IsDelete column already exists in Categories table.';
END
GO

-- 4. Sellers Table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Sellers') AND name = 'IsDelete'
)
BEGIN
    ALTER TABLE Sellers ADD IsDelete BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDelete column to Sellers table.';
END
ELSE
BEGIN
    PRINT 'IsDelete column already exists in Sellers table.';
END
GO

-- 5. Set default value for existing entries (redundant due to DEFAULT constraint, but good for clarity)
UPDATE Users SET IsDelete = 0 WHERE IsDelete IS NULL;
UPDATE Products SET IsDelete = 0 WHERE IsDelete IS NULL;
UPDATE Categories SET IsDelete = 0 WHERE IsDelete IS NULL;
UPDATE Sellers SET IsDelete = 0 WHERE IsDelete IS NULL;
GO

PRINT 'Soft-delete flags migration completed successfully.';
