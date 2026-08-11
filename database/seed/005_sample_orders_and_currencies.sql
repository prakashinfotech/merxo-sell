-- ============================================================
-- 005_sample_orders_and_currencies.sql
-- Adds sample currency rates and a handful of sample orders
-- with line items so the admin Orders / Currencies pages have
-- realistic content to display.
-- Idempotent: safe to re-run.
-- Run AFTER 004_sample_sellers_and_customers.sql.
-- ============================================================

USE MerxoSellDb;
GO

-- ── 1. Currency rates ─────────────────────────────────────────
-- CAD is the immutable base.  Other rates are illustrative — admin
-- can adjust them at any time from the Currencies admin page.
;WITH src AS (
    SELECT * FROM (VALUES
        ('CAD', N'Canadian Dollar',    1.000000,  N'CA$', 1),
        ('USD', N'US Dollar',          0.740000,  N'$',   1),
        ('EUR', N'Euro',               0.680000,  N'€',   1),
        ('GBP', N'British Pound',      0.590000,  N'£',   1),
        ('INR', N'Indian Rupee',      61.500000,  N'₹',   1),
        ('AUD', N'Australian Dollar',  1.130000,  N'A$',  1),
        ('JPY', N'Japanese Yen',     112.400000,  N'¥',   1),
        ('SGD', N'Singapore Dollar',   0.990000,  N'S$',  1)
    ) v(CurrencyCode, CurrencyName, RateToCad, Symbol, IsActive)
)
MERGE CurrencyRates AS tgt
USING src AS s
   ON tgt.CurrencyCode = s.CurrencyCode
WHEN MATCHED THEN
    UPDATE SET tgt.CurrencyName = s.CurrencyName,
               tgt.RateToCad    = s.RateToCad,
               tgt.Symbol       = s.Symbol,
               tgt.IsActive     = s.IsActive,
               tgt.LastUpdated  = GETUTCDATE()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (CurrencyCode, CurrencyName, RateToCad, Symbol, IsActive, LastUpdated)
    VALUES (s.CurrencyCode, s.CurrencyName, s.RateToCad, s.Symbol, s.IsActive, GETUTCDATE());

PRINT 'Currency rates seeded.';
GO

-- ── 2. Pick sample buyers + first available approved product ─
-- We only seed orders if there's at least one active buyer + an
-- active product to reference, otherwise FK constraints would fail.
DECLARE @ProductId INT = (
    SELECT TOP 1 p.ProductId
    FROM   Products p
    WHERE  p.IsActive = 1
    ORDER BY p.ProductId
);

IF @ProductId IS NULL
BEGIN
    PRINT 'No products found — skipping order seed.';
    RETURN;
END

DECLARE @ProductPrice DECIMAL(18,2) =
    (SELECT ISNULL(SalePrice, BasePrice) FROM Products WHERE ProductId = @ProductId);
DECLARE @ProductName NVARCHAR(300) =
    (SELECT Name FROM Products WHERE ProductId = @ProductId);

-- ── 3. Create one order per sample buyer (skip buyers who already have orders) ─
DECLARE @OrderSeed TABLE (
    UserId      INT,
    Status      NVARCHAR(30),
    CurrencyCd  NVARCHAR(10),
    Quantity    INT,
    DaysAgo     INT
);

INSERT INTO @OrderSeed
SELECT u.UserId, v.Status, u.PreferredCurrency, v.Quantity, v.DaysAgo
FROM   Users u
JOIN   Roles r ON r.RoleId = u.RoleId
CROSS APPLY (VALUES
    ('Pending',    1,  1),
    ('Confirmed',  2,  3),
    ('Processing', 1,  6),
    ('Shipped',    3, 10),
    ('Delivered',  2, 18),
    ('Cancelled',  1, 22),
    ('Refunded',   1, 30),
    ('Delivered',  4, 40)
) v(Status, Quantity, DaysAgo)
WHERE  r.RoleName = 'Buyer'
  AND  u.Email LIKE '%@example.com'
  AND  u.IsActive = 1
  AND  NOT EXISTS (SELECT 1 FROM Orders o WHERE o.UserId = u.UserId);

-- Resolve a default address per buyer (first one we find) for shipping
DECLARE @SampleOrders CURSOR;
SET @SampleOrders = CURSOR FAST_FORWARD FOR
    SELECT s.UserId, s.Status, s.CurrencyCd, s.Quantity, s.DaysAgo
    FROM   @OrderSeed s
    -- limit to ~3 orders per buyer at most for readability
    JOIN   (SELECT UserId,
                   ROW_NUMBER() OVER (PARTITION BY UserId ORDER BY DaysAgo) AS rn
            FROM   @OrderSeed) ranked
        ON ranked.UserId = s.UserId
    WHERE  ranked.rn <= 3;

DECLARE @UserId INT, @Status NVARCHAR(30), @Currency NVARCHAR(10),
        @Qty INT, @DaysAgo INT;

OPEN @SampleOrders;
FETCH NEXT FROM @SampleOrders INTO @UserId, @Status, @Currency, @Qty, @DaysAgo;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @AddressId INT =
        (SELECT TOP 1 AddressId FROM Addresses WHERE UserId = @UserId ORDER BY IsDefault DESC, AddressId);

    DECLARE @TotalCad DECIMAL(18,2) = @ProductPrice * @Qty;
    DECLARE @Rate DECIMAL(18,6) =
        ISNULL((SELECT RateToCad FROM CurrencyRates WHERE CurrencyCode = @Currency), 1);
    DECLARE @DisplayTotal DECIMAL(18,2) = @TotalCad * @Rate;
    DECLARE @CreatedAt DATETIME2 = DATEADD(DAY, -@DaysAgo, GETUTCDATE());

    INSERT INTO Orders (UserId, AddressId, Status, TotalAmountCAD, ShippingAmount, DiscountAmount,
                        CurrencyCode, DisplayTotal, CreatedAt, UpdatedAt)
    VALUES (@UserId, @AddressId, @Status, @TotalCad, 0, 0,
            @Currency, @DisplayTotal, @CreatedAt, @CreatedAt);

    DECLARE @OrderId INT = SCOPE_IDENTITY();

    INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPriceCAD)
    VALUES (@OrderId, @ProductId, @ProductName, @Qty, @ProductPrice);

    FETCH NEXT FROM @SampleOrders INTO @UserId, @Status, @Currency, @Qty, @DaysAgo;
END

CLOSE @SampleOrders;
DEALLOCATE @SampleOrders;

PRINT 'Sample orders seeded.';
GO

-- ── 4. Verification ───────────────────────────────────────────
SELECT 'Currencies (active)' AS [Set], COUNT(*) AS [Rows] FROM CurrencyRates WHERE IsActive = 1
UNION ALL
SELECT 'Currencies (total)',  COUNT(*) FROM CurrencyRates
UNION ALL
SELECT 'Orders (sample)',     COUNT(*) FROM Orders o
                              JOIN Users u ON u.UserId = o.UserId
                              WHERE u.Email LIKE '%@example.com'
UNION ALL
SELECT 'Order items',         COUNT(*) FROM OrderItems oi
                              JOIN Orders o  ON o.OrderId = oi.OrderId
                              JOIN Users u   ON u.UserId  = o.UserId
                              WHERE u.Email LIKE '%@example.com';
GO
