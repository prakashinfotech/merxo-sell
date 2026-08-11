-- ============================================================
-- update_missing_product_images_v2.sql
-- Robustly updates or inserts primary images for catalog items.
-- ============================================================

USE MerxoSellDb;
GO

-- Create a temporary mapping for easier updates
CREATE TABLE #ImageMap (
    NamePattern NVARCHAR(300),
    ImageUrl    NVARCHAR(500)
);

INSERT INTO #ImageMap (NamePattern, ImageUrl) VALUES
('%Classic White T-Shirt%', '/assets/images/white-tshirt.png'),
('%Wireless Earbuds Pro%', '/assets/images/white-tshirt.png'),
('%iPhone 15 Pro%',       'https://images.unsplash.com/photo-1696446701796-da61225697cc?q=80&w=800'),
('%Samsung Galaxy S23%',  'https://images.unsplash.com/photo-1678911820864-e2c567c655d7?q=80&w=800'),
('%Nike Air Max%',        'https://images.unsplash.com/photo-1542291026-7eec264c27ff?q=80&w=800');

-- Loop through the map
DECLARE @Pattern NVARCHAR(300), @Url NVARCHAR(500);
DECLARE img_cursor CURSOR FOR SELECT NamePattern, ImageUrl FROM #ImageMap;

OPEN img_cursor;
FETCH NEXT FROM img_cursor INTO @Pattern, @Url;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- 1. Identify Product IDs matching the pattern
    DECLARE @Pid INT;
    DECLARE prod_cursor CURSOR FOR SELECT ProductId FROM Products WHERE Name LIKE @Pattern;
    
    OPEN prod_cursor;
    FETCH NEXT FROM prod_cursor INTO @Pid;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- 2. Check if this product already has a primary image
        IF EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @Pid AND IsPrimary = 1)
        BEGIN
            UPDATE ProductImages SET ImageUrl = @Url WHERE ProductId = @Pid AND IsPrimary = 1;
        END
        ELSE IF EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = @Pid)
        BEGIN
            -- Product has images but none marked primary? Set the first one to primary and update URL
            UPDATE ProductImages SET ImageUrl = @Url, IsPrimary = 1 WHERE ImageId = (SELECT TOP 1 ImageId FROM ProductImages WHERE ProductId = @Pid);
        END
        ELSE
        BEGIN
            -- No images at all, insert new primary image
            INSERT INTO ProductImages (ProductId, ImageUrl, IsPrimary, AltText)
            VALUES (@Pid, @Url, 1, 'Product Image');
        END
        
        FETCH NEXT FROM prod_cursor INTO @Pid;
    END
    CLOSE prod_cursor;
    DEALLOCATE prod_cursor;

    FETCH NEXT FROM img_cursor INTO @Pattern, @Url;
END

CLOSE img_cursor;
DEALLOCATE img_cursor;
DROP TABLE #ImageMap;

-- Final verification
SELECT p.Name, pi.ImageUrl, pi.IsPrimary
FROM Products p
JOIN ProductImages pi ON p.ProductId = pi.ProductId
WHERE pi.IsPrimary = 1
ORDER BY p.Name;
GO
