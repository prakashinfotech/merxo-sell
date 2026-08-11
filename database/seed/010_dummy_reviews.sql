-- ============================================================
-- 010_dummy_reviews.sql
-- Adds sample reviews for existing active products and existing
-- Buyer users. Idempotent: skips user/product pairs that already
-- have reviews.
-- ============================================================

USE MerxoSellDb;
GO

DECLARE @Pairs TABLE (
    RowNo   INT IDENTITY(1,1),
    UserId  INT,
    ProductId INT
);

INSERT INTO @Pairs (UserId, ProductId)
SELECT TOP (24)
       u.UserId,
       p.ProductId
FROM Users u
JOIN Roles r ON r.RoleId = u.RoleId
CROSS JOIN Products p
WHERE r.RoleName = 'Buyer'
  AND u.IsActive = 1
  AND p.IsActive = 1
  AND NOT EXISTS (
      SELECT 1
      FROM Reviews existing
      WHERE existing.UserId = u.UserId
        AND existing.ProductId = p.ProductId
  )
ORDER BY u.UserId, p.ProductId;

INSERT INTO Reviews (ProductId, UserId, Rating, Comment, CreatedAt)
SELECT
    ProductId,
    UserId,
    CASE RowNo % 5
        WHEN 0 THEN 5
        WHEN 1 THEN 4
        WHEN 2 THEN 5
        WHEN 3 THEN 3
        ELSE 4
    END,
    CASE RowNo % 6
        WHEN 0 THEN 'Great value for the price and arrived in good condition.'
        WHEN 1 THEN 'Product quality matched the listing and packaging was neat.'
        WHEN 2 THEN 'Useful product overall. Delivery was quick and simple.'
        WHEN 3 THEN 'Good item, though the color looked slightly different in person.'
        WHEN 4 THEN 'Happy with the purchase. I would consider buying again.'
        ELSE 'Solid product for everyday use with no major issues.'
    END,
    DATEADD(DAY, -RowNo, GETUTCDATE())
FROM @Pairs;

SELECT COUNT(*) AS TotalReviews FROM Reviews;
GO
