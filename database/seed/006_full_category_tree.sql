-- ============================================================
-- 006_full_category_tree.sql
-- Replaces the demo category set with the full marketplace
-- taxonomy used by the storefront (Featured + 13 main groups
-- with 5 sub-categories each).  Idempotent — safe to re-run.
-- ============================================================

USE MerxoSellDb;
GO

SET NOCOUNT ON;

-- ── Helper: upsert a category by slug ──────────────────────────
-- Uses a temp table because T-SQL has no CTE-based upsert that
-- returns the IDENTITY of inserted-or-existing rows in one shot.

DECLARE @tree TABLE (
    SortOrder INT,
    Slug      NVARCHAR(140),
    Name      NVARCHAR(120),
    ParentSlug NVARCHAR(140) NULL
);

-- Top-level groups
INSERT INTO @tree VALUES
( 1, 'featured',           N'Featured',            NULL),
( 2, 'home-and-kitchen',   N'Home & Kitchen',      NULL),
( 3, 'womens-clothing',    N'Women''s Clothing',   NULL),
( 4, 'womens-shoes',       N'Women''s Shoes',      NULL),
( 5, 'mens-clothing',      N'Men''s Clothing',     NULL),
( 6, 'mens-shoes',         N'Men''s Shoes',        NULL),
( 7, 'sports-and-outdoors',N'Sports & Outdoors',   NULL),
( 8, 'jewelry-accessories',N'Jewelry & Accessories',NULL),
( 9, 'beauty-and-health',  N'Beauty & Health',     NULL),
(10, 'toys-and-games',     N'Toys & Games',        NULL),
(11, 'electronics',        N'Electronics',         NULL),
(12, 'appliances',         N'Appliances',          NULL),
(13, 'health-and-household',N'Health & Household', NULL),
(14, 'furniture',          N'Furniture',           NULL);

-- Home & Kitchen
INSERT INTO @tree VALUES
( 1, 'bakeware',                 N'Bakeware',                  'home-and-kitchen'),
( 2, 'rugs-and-mats',             N'Rugs & Mats',               'home-and-kitchen'),
( 3, 'home-decor-products',       N'Home Decor Products',       'home-and-kitchen'),
( 4, 'home-storage-organization', N'Home Storage & Organization','home-and-kitchen'),
( 5, 'cookware',                  N'Cookware',                  'home-and-kitchen');

-- Women's Clothing
INSERT INTO @tree VALUES
( 1, 'womens-tshirts',     N'Women''s T-Shirts',  'womens-clothing'),
( 2, 'womens-dress',       N'Women''s Dress',     'womens-clothing'),
( 3, 'womens-pants',       N'Women''s Pants',     'womens-clothing'),
( 4, 'womens-blazers',     N'Women''s Blazers',   'womens-clothing'),
( 5, 'womens-sweatshirts', N'Women''s Sweatshirts','womens-clothing');

-- Women's Shoes
INSERT INTO @tree VALUES
( 1, 'womens-fashion-sneakers', N'Women''s Fashion Sneakers', 'womens-shoes'),
( 2, 'womens-slippers',          N'Women''s Slippers',         'womens-shoes'),
( 3, 'womens-heeled-sandals',    N'Women''s Heeled Sandals',   'womens-shoes'),
( 4, 'womens-flat-sandals',      N'Women''s Flat Sandals',     'womens-shoes'),
( 5, 'womens-flip-flops',        N'Women''s Flip Flops',       'womens-shoes');

-- Men's Clothing
INSERT INTO @tree VALUES
( 1, 'mens-shirts',        N'Men''s Shirts',       'mens-clothing'),
( 2, 'mens-shorts',        N'Men''s Shorts',       'mens-clothing'),
( 3, 'mens-polos',         N'Men''s Polos',        'mens-clothing'),
( 4, 'mens-jeans',         N'Men''s Jeans',        'mens-clothing'),
( 5, 'mens-casual-pants',  N'Men''s Casual Pants', 'mens-clothing');

-- Men's Shoes
INSERT INTO @tree VALUES
( 1, 'mens-casual-shoes',  N'Men''s Casual Shoes', 'mens-shoes'),
( 2, 'mens-slippers',      N'Men''s Slippers',     'mens-shoes'),
( 3, 'mens-sandals',       N'Men''s Sandals',      'mens-shoes'),
( 4, 'mens-loafers',       N'Men''s Loafers',      'mens-shoes'),
( 5, 'mens-canvas-shoes',  N'Men''s Canvas Shoes', 'mens-shoes');

-- Sports & Outdoors
INSERT INTO @tree VALUES
( 1, 'exercise-fitness-items',     N'Exercise & Fitness Items',    'sports-and-outdoors'),
( 2, 'fishing',                    N'Fishing',                     'sports-and-outdoors'),
( 3, 'outdoor-lights',             N'Outdoors Lights',             'sports-and-outdoors'),
( 4, 'sports-electronics-gadgets', N'Sports & Electronics Gadgets','sports-and-outdoors'),
( 5, 'yoga-fitness',               N'Yoga & Fitness',              'sports-and-outdoors');

-- Jewelry & Accessories
INSERT INTO @tree VALUES
( 1, 'personalized-products', N'Personalized Products', 'jewelry-accessories'),
( 2, 'womens-eyewear',        N'Women''s Eyewear',      'jewelry-accessories'),
( 3, 'mens-watches',          N'Men''s Watches',        'jewelry-accessories'),
( 4, 'hand-fans',             N'Hand Fans',             'jewelry-accessories'),
( 5, 'cigarette-cases',       N'Cigarette Cases',       'jewelry-accessories');

-- Beauty & Health
INSERT INTO @tree VALUES
( 1, 'makeup',                  N'Makeup',                  'beauty-and-health'),
( 2, 'wigs-hair-extensions',    N'Wigs & Hair Extensions',  'beauty-and-health'),
( 3, 'facial-care',             N'Facial Care',             'beauty-and-health'),
( 4, 'personal-care',           N'Personal Care',           'beauty-and-health'),
( 5, 'nail-polish',             N'Nail Polish',             'beauty-and-health');

-- Toys & Games
INSERT INTO @tree VALUES
( 1, 'building-toys',           N'Building Toys',           'toys-and-games'),
( 2, 'dolls-accessories',       N'Dolls & Accessories',     'toys-and-games'),
( 3, 'baby-toddler-toys',       N'Baby & Toddler Toys',     'toys-and-games'),
( 4, 'toy-figures-playsets',    N'Toy Figures & Playsets',  'toys-and-games'),
( 5, 'puzzles',                 N'Puzzles',                 'toys-and-games');

-- Electronics
INSERT INTO @tree VALUES
( 1, 'headphones-earbuds',      N'Headphones, Earbuds & Accessories', 'electronics'),
( 2, 'photos-optics',           N'Photos & Optics',                   'electronics'),
( 3, 'data-storage',            N'Data Storage',                      'electronics'),
( 4, 'audio-radio',             N'Audio & Radio',                     'electronics'),
( 5, 'lighting',                N'Lighting',                          'electronics');

-- Appliances
INSERT INTO @tree VALUES
( 1, 'electric-massagers',      N'Electric Massagers',         'appliances'),
( 2, 'fans-air-conditioners',   N'Fans & Air Conditioners',    'appliances'),
( 3, 'juicers-food-processors', N'Juicers & Food Processors',  'appliances'),
( 4, 'coffee-tea-appliances',   N'Coffee & Tea Appliances',    'appliances'),
( 5, 'air-quality',             N'Air Quality',                'appliances');

-- Health & Household
INSERT INTO @tree VALUES
( 1, 'oral-care-products',      N'Oral Care Products',         'health-and-household'),
( 2, 'household-cleaning-supplies', N'Household & Cleaning Supplies', 'health-and-household'),
( 3, 'massage-tools',           N'Massage Tools',              'health-and-household'),
( 4, 'paper-plastic',           N'Paper & Plastic',            'health-and-household'),
( 5, 'diabetes-care',           N'Diabetes Care',              'health-and-household');

-- Furniture
INSERT INTO @tree VALUES
( 1, 'living-room-furniture',   N'Living Room Furniture',      'furniture'),
( 2, 'bedroom-furniture',       N'Bedroom Furniture',          'furniture'),
( 3, 'kitchen-dining-furniture',N'Kitchen & Dining Furniture', 'furniture'),
( 4, 'office-furniture',        N'Office Furniture',           'furniture'),
( 5, 'bathroom-furniture',      N'Bathroom Furniture',         'furniture');

-- ── 1. Upsert top-level rows (no parent) ─────────────────────────
MERGE Categories AS tgt
USING (SELECT Slug, Name, SortOrder FROM @tree WHERE ParentSlug IS NULL) AS s
   ON tgt.Slug = s.Slug
WHEN MATCHED THEN
    UPDATE SET tgt.Name = s.Name, tgt.SortOrder = s.SortOrder,
               tgt.ParentCategoryId = NULL, tgt.IsActive = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ParentCategoryId, Name, Slug, SortOrder, IsActive)
    VALUES (NULL, s.Name, s.Slug, s.SortOrder, 1);

-- ── 2. Upsert sub-categories (resolve parent by slug) ───────────
MERGE Categories AS tgt
USING (
    SELECT s.Slug, s.Name, s.SortOrder, parent.CategoryId AS ParentId
    FROM   @tree s
    JOIN   Categories parent ON parent.Slug = s.ParentSlug
    WHERE  s.ParentSlug IS NOT NULL
) AS src
   ON tgt.Slug = src.Slug
WHEN MATCHED THEN
    UPDATE SET tgt.Name = src.Name, tgt.SortOrder = src.SortOrder,
               tgt.ParentCategoryId = src.ParentId, tgt.IsActive = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ParentCategoryId, Name, Slug, SortOrder, IsActive)
    VALUES (src.ParentId, src.Name, src.Slug, src.SortOrder, 1);

-- ── Verification ─────────────────────────────────────────────────
SELECT 'Top-level categories'  AS [Set], COUNT(*) AS [Rows]
FROM   Categories WHERE ParentCategoryId IS NULL
UNION ALL
SELECT 'Sub-categories',        COUNT(*)
FROM   Categories WHERE ParentCategoryId IS NOT NULL;
GO
