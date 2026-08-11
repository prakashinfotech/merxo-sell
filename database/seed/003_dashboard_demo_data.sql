-- ============================================================
-- 003_dashboard_demo_data.sql
-- Generates dynamic sales and traffic data for dashboard testing.
-- Seeds: Orders, OrderItems, ProductViews.
-- ============================================================

USE MerxoSellDb;
GO

-- ── 1. Preparation: Get valid IDs ───────────────────────────────────────────
DECLARE @ProductCount INT = (SELECT COUNT(*) FROM Products);
DECLARE @UserCount    INT = (SELECT COUNT(*) FROM Users);

IF @ProductCount = 0 OR @UserCount = 0
BEGIN
    PRINT 'Error: No products or users found. Please run previous seed scripts first.';
    RETURN;
END

-- ── 2. Seed Product Views (Last 30 Days) ────────────────────────────────────
PRINT 'Seeding ProductViews...';
DELETE FROM ProductViews; -- Clear old data for a fresh demo

DECLARE @v_day INT = 0;
WHILE @v_day < 30
BEGIN
    DECLARE @v_date DATETIME2 = DATEADD(DAY, -@v_day, GETUTCDATE());
    DECLARE @v_count INT = ABS(CHECKSUM(NEWID())) % 50 + 20; -- 20-70 views per day
    
    DECLARE @i INT = 0;
    WHILE @i < @v_count
    BEGIN
        DECLARE @p_id INT = (SELECT TOP 1 ProductId FROM Products ORDER BY NEWID());
        DECLARE @u_id INT = (SELECT TOP 1 UserId FROM Users ORDER BY NEWID());
        
        INSERT INTO ProductViews (ProductId, UserId, ViewedAt, IpAddress)
        VALUES (@p_id, @u_id, @v_date, '127.0.0.1');
        
        SET @i = @i + 1;
    END
    SET @v_day = @v_day + 1;
END
PRINT 'ProductViews seeded.';

-- ── 3. Seed Orders (Last 6 Months) ───────────────────────────────────────────
PRINT 'Seeding Orders and OrderItems...';
-- We keep existing orders but add new ones for history
DECLARE @m_idx INT = 0;
WHILE @m_idx < 6
BEGIN
    DECLARE @m_date DATETIME2 = DATEADD(MONTH, -@m_idx, GETUTCDATE());
    DECLARE @o_count INT = ABS(CHECKSUM(NEWID())) % 15 + 10; -- 10-25 orders per month
    
    DECLARE @j INT = 0;
    WHILE @j < @o_count
    BEGIN
        DECLARE @buyer_id INT = (SELECT TOP 1 UserId FROM Users ORDER BY NEWID());
        DECLARE @order_date DATETIME2 = DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 28, @m_date);
        
        -- Insert Order
        INSERT INTO Orders (UserId, Status, TotalAmountCAD, ShippingAmount, DiscountAmount, CreatedAt)
        VALUES (@buyer_id, 'Delivered', 0, 5.00, 0, @order_date);
        
        DECLARE @new_order_id INT = SCOPE_IDENTITY();
        
        -- Insert 1-3 random items per order
        DECLARE @items_count INT = ABS(CHECKSUM(NEWID())) % 3 + 1;
        DECLARE @k INT = 0;
        DECLARE @running_total DECIMAL(18,2) = 0;
        
        WHILE @k < @items_count
        BEGIN
            DECLARE @prod_id INT = (SELECT TOP 1 ProductId FROM Products ORDER BY NEWID());
            DECLARE @prod_name NVARCHAR(300) = (SELECT Name FROM Products WHERE ProductId = @prod_id);
            DECLARE @price DECIMAL(18,2) = (SELECT BasePrice FROM Products WHERE ProductId = @prod_id);
            DECLARE @qty INT = ABS(CHECKSUM(NEWID())) % 2 + 1;
            
            INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice)
            VALUES (@new_order_id, @prod_id, @prod_name, @qty, @price);
            
            SET @running_total = @running_total + (@price * @qty);
            SET @k = @k + 1;
        END
        
        -- Update total amount (including $5 shipping)
        UPDATE Orders SET TotalAmountCAD = @running_total + 5.00 WHERE OrderId = @new_order_id;
        
        SET @j = @j + 1;
    END
    SET @m_idx = @m_idx + 1;
END
PRINT 'Orders and OrderItems seeded.';

-- ── 4. Verification ──────────────────────────────────────────────────────────
SELECT 'Orders' AS [Table], COUNT(*) AS [Rows] FROM Orders
UNION ALL
SELECT 'OrderItems', COUNT(*) FROM OrderItems
UNION ALL
SELECT 'ProductViews', COUNT(*) FROM ProductViews;
GO
