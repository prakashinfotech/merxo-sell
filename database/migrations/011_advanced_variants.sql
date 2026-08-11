-- ============================================================
-- 011_advanced_variants.sql
-- Implements lookup tables for Colors and Sizes and improves Variant relationships.
-- ============================================================

USE MerxoSellDb;
GO

-- 1. ProductColors
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductColors')
BEGIN
    CREATE TABLE ProductColors (
        ColorId   INT            NOT NULL IDENTITY(1,1),
        ProductId INT            NOT NULL,
        ColorName NVARCHAR(50)   NOT NULL,
        HexCode   NVARCHAR(10)   NULL, -- For color swatches
        
        CONSTRAINT PK_ProductColors PRIMARY KEY (ColorId),
        CONSTRAINT FK_ProductColors_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProductColors_ProductId ON ProductColors(ProductId);
END

-- 2. ProductSizes
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductSizes')
BEGIN
    CREATE TABLE ProductSizes (
        SizeId    INT            NOT NULL IDENTITY(1,1),
        ProductId INT            NOT NULL,
        SizeName  NVARCHAR(50)   NOT NULL,
        
        CONSTRAINT PK_ProductSizes PRIMARY KEY (SizeId),
        CONSTRAINT FK_ProductSizes_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProductSizes_ProductId ON ProductSizes(ProductId);
END

-- 3. Update ProductVariants to use ColorId/SizeId (optional but cleaner)
-- However, we will keep existing Color/Size strings for simplicity but add indexes.
-- The user asked for these tables, so we will use them to populate the dynamic selections.

-- 4. ProductVariantImages (Join table for multiple variants sharing same images)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductVariantImages')
BEGIN
    CREATE TABLE ProductVariantImages (
        VariantId INT NOT NULL,
        ImageId   INT NOT NULL,
        
        CONSTRAINT PK_ProductVariantImages PRIMARY KEY (VariantId, ImageId),
        CONSTRAINT FK_PVI_Variant FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId) ON DELETE CASCADE,
        CONSTRAINT FK_PVI_Image   FOREIGN KEY (ImageId)   REFERENCES ProductImages(ImageId)     ON DELETE NO ACTION
    );
END
GO
