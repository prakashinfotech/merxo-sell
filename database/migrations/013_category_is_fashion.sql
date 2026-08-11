-- ============================================================
-- 013_category_is_fashion.sql
-- Adds:
--   * Categories.IsFashion (BIT)   — drives the seller-side variant builder.
--                                    When true, the colour/size matrix is
--                                    enabled for products listed under this
--                                    category (or any of its children).
-- Seeds well-known fashion categories with IsFashion = 1.
-- Idempotent: safe to re-run.
-- ============================================================

USE MerxoSellDb;
GO

-- ── 1. Column ─────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('Categories') AND name = 'IsFashion')
BEGIN
    ALTER TABLE Categories ADD IsFashion BIT NOT NULL DEFAULT 0;
    PRINT 'Categories.IsFashion added.';
END
GO

-- ── 2. Seed obvious fashion roots + their children ────────────
-- Mark anything whose name suggests apparel / footwear / clothing / fashion.
UPDATE Categories
   SET IsFashion = 1
 WHERE IsFashion = 0
   AND (
        Name LIKE '%Fashion%'
     OR Name LIKE '%Cloth%'
     OR Name LIKE '%Apparel%'
     OR Name LIKE '%Shoe%'
     OR Name LIKE '%Footwear%'
     OR Name LIKE '%Men%'
     OR Name LIKE '%Women%'
     OR Name LIKE '%Kids%Wear%'
   );
GO

-- Propagate down: if a parent is fashion, mark its sub-categories too.
;WITH FashionRoots AS (
    SELECT CategoryId FROM Categories WHERE IsFashion = 1
)
UPDATE c
   SET IsFashion = 1
  FROM Categories c
  JOIN FashionRoots r ON c.ParentCategoryId = r.CategoryId
 WHERE c.IsFashion = 0;
GO

PRINT 'Categories.IsFashion seeding complete.';
GO
