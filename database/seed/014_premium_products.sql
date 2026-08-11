-- ============================================================
-- 014_premium_products.sql
-- Seeds 10 premium, AI-generated products to elevate UI.
-- ============================================================

USE MerxoSellDb;
GO

DECLARE @CatElectronics INT = (SELECT CategoryId FROM Categories WHERE Slug = 'electronics');
DECLARE @CatFashion     INT = (SELECT CategoryId FROM Categories WHERE Slug = 'fashion');
DECLARE @CatHome        INT = (SELECT CategoryId FROM Categories WHERE Slug = 'home-decor');
DECLARE @CatSports      INT = (SELECT CategoryId FROM Categories WHERE Slug = 'sports-fitness');

DECLARE @SellerId INT = (SELECT SellerId FROM Sellers WHERE StoreName = 'Global Gadgets & Gear');

-- Only insert if they don't exist
IF NOT EXISTS (SELECT 1 FROM Products WHERE Slug = 'aura-noise-canceling-headphones')
BEGIN
    INSERT INTO Products (CategoryId, SellerId, ManufacturerId, Name, Slug, Description, BasePrice, SalePrice, Stock, Status, IsActive, CreatedAt)
    VALUES 
        (@CatElectronics, @SellerId, NULL, 'Aura Noise-Canceling Headphones', 'aura-noise-canceling-headphones', 'Experience pure audio bliss with our premium over-ear headphones featuring active noise cancellation and 40-hour battery life.', 299.99, 249.99, 120, 'Approved', 1, GETUTCDATE()),
        (@CatHome, @SellerId, NULL, 'Lumina Smart Desk Lamp', 'lumina-smart-desk-lamp', 'Sleek matte black LED desk lamp with adjustable color temperature and smart home integration.', 89.00, NULL, 50, 'Approved', 1, GETUTCDATE()),
        (@CatElectronics, @SellerId, NULL, 'Zenith Mechanical Keyboard', 'zenith-mechanical-keyboard', 'Custom minimalist mechanical keyboard with tactile switches and soft pastel keycaps.', 145.00, NULL, 75, 'Approved', 1, GETUTCDATE()),
        (@CatHome, @SellerId, NULL, 'Ceramic Pour-Over Coffee Set', 'ceramic-pour-over-coffee', 'Elevate your morning routine with this beautifully crafted matte white ceramic pour-over set.', 65.00, 55.00, 200, 'Approved', 1, GETUTCDATE()),
        (@CatHome, @SellerId, NULL, 'Velvet Lounge Accent Chair', 'velvet-lounge-chair', 'Mid-century modern accent chair draped in rich emerald green velvet. Perfect for any contemporary living space.', 450.00, 399.00, 15, 'Approved', 1, GETUTCDATE()),
        (@CatFashion, @SellerId, NULL, 'Chronos Minimalist Watch', 'chronos-minimalist-watch', 'Timeless elegance. Features a clean white dial, stainless steel mesh strap, and Swiss quartz movement.', 180.00, NULL, 80, 'Approved', 1, GETUTCDATE()),
        (@CatFashion, @SellerId, NULL, 'Nomad Canvas Weekender Bag', 'nomad-canvas-weekender', 'Your perfect travel companion. Durable canvas construction with premium brown leather accents.', 120.00, 95.00, 150, 'Approved', 1, GETUTCDATE()),
        (@CatHome, @SellerId, NULL, 'Artisan Stone Planter', 'artisan-stone-planter', 'Textured indoor stone planter. Includes a self-watering base ideal for Monsteras and large houseplants.', 45.00, NULL, 300, 'Approved', 1, GETUTCDATE()),
        (@CatElectronics, @SellerId, NULL, 'Echo Portable Bluetooth Speaker', 'echo-portable-speaker', 'Compact, fabric-wrapped speaker delivering 360-degree sound with deep bass and waterproof rating.', 110.00, 89.99, 400, 'Approved', 1, GETUTCDATE()),
        (@CatSports, @SellerId, NULL, 'Aero Running Sneakers', 'aero-running-sneakers', 'Lightweight athletic sneakers engineered for maximum comfort and speed. Modern aesthetic in white and gray.', 130.00, NULL, 250, 'Approved', 1, GETUTCDATE());

    -- Get the inserted product IDs
    DECLARE @IdHeadphones INT = (SELECT ProductId FROM Products WHERE Slug = 'aura-noise-canceling-headphones');
    DECLARE @IdLamp INT       = (SELECT ProductId FROM Products WHERE Slug = 'lumina-smart-desk-lamp');
    DECLARE @IdKeyboard INT   = (SELECT ProductId FROM Products WHERE Slug = 'zenith-mechanical-keyboard');
    DECLARE @IdCoffee INT     = (SELECT ProductId FROM Products WHERE Slug = 'ceramic-pour-over-coffee');
    DECLARE @IdChair INT      = (SELECT ProductId FROM Products WHERE Slug = 'velvet-lounge-chair');
    DECLARE @IdWatch INT      = (SELECT ProductId FROM Products WHERE Slug = 'chronos-minimalist-watch');
    DECLARE @IdBag INT        = (SELECT ProductId FROM Products WHERE Slug = 'nomad-canvas-weekender');
    DECLARE @IdPlanter INT    = (SELECT ProductId FROM Products WHERE Slug = 'artisan-stone-planter');
    DECLARE @IdSpeaker INT    = (SELECT ProductId FROM Products WHERE Slug = 'echo-portable-speaker');
    DECLARE @IdSneakers INT   = (SELECT ProductId FROM Products WHERE Slug = 'aero-running-sneakers');

    -- Insert Product Images
    INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder)
    VALUES 
        (@IdHeadphones, 'http://localhost:5000/uploads/premium-headphones.png', 'Aura Headphones', 1, 1),
        (@IdHeadphones, 'http://localhost:5000/uploads/premium-headphones-alt.png', 'Aura Headphones Lifestyle', 0, 2),
        
        (@IdLamp, 'http://localhost:5000/uploads/premium-lamp.png', 'Lumina Desk Lamp', 1, 1),
        
        (@IdKeyboard, 'http://localhost:5000/uploads/premium-keyboard.png', 'Zenith Keyboard', 1, 1),
        (@IdKeyboard, 'http://localhost:5000/uploads/premium-keyboard-alt.png', 'Zenith Keyboard Close-up', 0, 2),
        
        (@IdCoffee, 'http://localhost:5000/uploads/premium-coffee.png', 'Ceramic Pour-Over', 1, 1),
        (@IdChair, 'http://localhost:5000/uploads/premium-chair.png', 'Emerald Velvet Chair', 1, 1),
        (@IdWatch, 'http://localhost:5000/uploads/premium-watch.png', 'Chronos Minimalist Watch', 1, 1),
        (@IdBag, 'http://localhost:5000/uploads/premium-bag.png', 'Nomad Weekender Bag', 1, 1),
        (@IdPlanter, 'http://localhost:5000/uploads/premium-planter.png', 'Artisan Stone Planter', 1, 1),
        (@IdSpeaker, 'http://localhost:5000/uploads/premium-speaker.png', 'Echo Bluetooth Speaker', 1, 1),
        (@IdSneakers, 'http://localhost:5000/uploads/premium-sneakers.png', 'Aero Running Sneakers', 1, 1);
        
    -- Generate some dummy reviews to boost rating so they appear in Top Rated and Best Sellers
    -- We'll just give them random 5-star ratings from the buyer user
    DECLARE @BuyerUserId INT = (SELECT TOP 1 UserId FROM Users WHERE Email = 'buyer@temuclone.com');
    IF @BuyerUserId IS NULL SET @BuyerUserId = (SELECT TOP 1 UserId FROM Users WHERE RoleId = (SELECT RoleId FROM Roles WHERE RoleName = 'Buyer'));
    
    IF @BuyerUserId IS NOT NULL
    BEGIN
        INSERT INTO Reviews (ProductId, UserId, Rating, Comment, Status, CreatedAt)
        VALUES 
            (@IdHeadphones, @BuyerUserId, 5, 'Absolutely incredible sound quality and the noise cancellation is magic!', 'Approved', GETUTCDATE()),
            (@IdKeyboard, @BuyerUserId, 5, 'Typing on this is a dream. The pastel keycaps are gorgeous.', 'Approved', GETUTCDATE()),
            (@IdChair, @BuyerUserId, 5, 'Looks stunning in my living room. Very high quality velvet.', 'Approved', GETUTCDATE()),
            (@IdBag, @BuyerUserId, 5, 'Perfect size for a weekend getaway. The leather feels premium.', 'Approved', GETUTCDATE()),
            (@IdSneakers, @BuyerUserId, 4, 'Very comfortable and stylish, though they run slightly small.', 'Approved', GETUTCDATE());
    END
END
GO
