-- ============================================================
-- 009_product_variants_v2.sql
-- Adds:
--   * ProductVariants.IsDefault (BIT)        — the variant pre-selected on the buyer detail page
--   * ProductImages.VariantId  (INT NULL)    — image belongs to a specific variant when set
-- Idempotent: safe to re-run.
-- ============================================================

USE MerxoSellDb;
GO

-- ── 1. ProductVariants.IsDefault ──────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('ProductVariants') AND name = 'IsDefault')
BEGIN
    ALTER TABLE ProductVariants ADD IsDefault BIT NOT NULL DEFAULT 0;
    PRINT 'ProductVariants.IsDefault added.';
END

-- ── 2. ProductImages.VariantId ────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('ProductImages') AND name = 'VariantId')
BEGIN
    ALTER TABLE ProductImages ADD VariantId INT NULL;
    
    -- We use EXEC for the following because the column VariantId was just added 
    -- and won't be visible to the parser in the same batch otherwise.
    EXEC('ALTER TABLE ProductImages
          ADD CONSTRAINT FK_ProductImages_Variants
          FOREIGN KEY (VariantId) REFERENCES ProductVariants(VariantId) ON DELETE NO ACTION;');

    EXEC('CREATE INDEX IX_ProductImages_VariantId
          ON ProductImages(VariantId) WHERE VariantId IS NOT NULL;');

    PRINT 'ProductImages.VariantId added with FK + filtered index.';
END
GO
