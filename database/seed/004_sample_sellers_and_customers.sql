-- ============================================================
-- 004_sample_sellers_and_customers.sql
-- Adds sample Seller users + Seller profiles, and sample
-- Buyer users (customers) with addresses for the admin UI.
-- Idempotent: safe to re-run.
-- Run AFTER 003_add_sellers_and_approval.sql.
-- ============================================================

USE MerxoSellDb;
GO

-- BCrypt(workFactor 11) hash for the password 'Sample@123'.
-- Replace with a freshly-generated hash if you need a different password.
DECLARE @SamplePasswordHash NVARCHAR(512) =
    '$2a$11$U9qoyHzFpLNpzzs6hXBPL.QBJRjm4SDkDxeqAhqEsqZjUxlASjfLW';

-- Resolve role ids defensively.
DECLARE @SellerRoleId INT = (SELECT TOP 1 RoleId FROM Roles WHERE RoleName = 'Seller'   ORDER BY RoleId);
DECLARE @BuyerRoleId  INT = (SELECT TOP 1 RoleId FROM Roles WHERE RoleName = 'Buyer' ORDER BY RoleId);

IF @SellerRoleId IS NULL
BEGIN
    INSERT INTO Roles (RoleName) VALUES ('Seller');
    SET @SellerRoleId = SCOPE_IDENTITY();
    PRINT 'Seller role created.';
END

IF @BuyerRoleId IS NULL
BEGIN
    INSERT INTO Roles (RoleName) VALUES ('Buyer');
    SET @BuyerRoleId = SCOPE_IDENTITY();
    PRINT 'Buyer role created.';
END

-- ── 1. Sample Seller users + Seller profiles ──────────────────────────────────
DECLARE @Sellers TABLE (
    Email        NVARCHAR(256),
    FullName     NVARCHAR(150),
    Phone        NVARCHAR(20),
    StoreName    NVARCHAR(200),
    Description  NVARCHAR(1000),
    ContactEmail NVARCHAR(200),
    Country      NVARCHAR(100),
    IsVerified   BIT
);

INSERT INTO @Sellers VALUES
('alice@techvision.store',  'Alice Walker',   '+1 (416) 555-0101',
    'TechVision Store',
    'Premium consumer electronics, headphones and smart-home gadgets.',
    'orders@techvision.store',  'Canada',        1),
('bruno@fashionhub.shop',   'Bruno Costa',    '+1 (212) 555-0144',
    'FashionHub Boutique',
    'Trend-forward apparel for men and women — fast shipping worldwide.',
    'support@fashionhub.shop',  'United States', 1),
('chiara@activelife.fit',   'Chiara Romano',  '+39 02 5550 0188',
    'ActiveLife Sports',
    'Yoga, running and outdoor gear handpicked by athletes.',
    'hello@activelife.fit',     'Italy',         0),
('dev@greengoods.eco',      'Dev Patel',      '+44 20 5550 0177',
    'GreenGoods Co.',
    'Eco-friendly home & garden essentials, certified sustainable.',
    'team@greengoods.eco',      'United Kingdom',1);

-- Insert any seller user that doesn't already exist
INSERT INTO Users (RoleId, FullName, Email, PasswordHash, Phone, PreferredCurrency, IsActive, CreatedAt)
SELECT @SellerRoleId, s.FullName, s.Email, @SamplePasswordHash, s.Phone, 'CAD', 1, GETUTCDATE()
FROM   @Sellers s
WHERE  NOT EXISTS (SELECT 1 FROM Users u WHERE u.Email = s.Email);

-- Create Seller profile for each user (skip if already exists)
INSERT INTO Sellers (UserId, StoreName, StoreDescription, ContactEmail, Phone, IsVerified, IsActive, CreatedAt)
SELECT u.UserId, s.StoreName, s.Description, s.ContactEmail, s.Phone, s.IsVerified, 1, GETUTCDATE()
FROM   @Sellers s
JOIN   Users u ON u.Email = s.Email
WHERE  NOT EXISTS (SELECT 1 FROM Sellers x WHERE x.UserId = u.UserId);

PRINT 'Sample seller accounts ensured.';
GO

-- ── 2. Sample Buyer users + addresses ─────────────────────────────
DECLARE @BuyerRoleId INT = (SELECT TOP 1 RoleId FROM Roles WHERE RoleName = 'Buyer' ORDER BY RoleId);
DECLARE @SamplePasswordHash NVARCHAR(512) =
    '$2a$11$U9qoyHzFpLNpzzs6hXBPL.QBJRjm4SDkDxeqAhqEsqZjUxlASjfLW';

DECLARE @Customers TABLE (
    Email      NVARCHAR(256),
    FullName   NVARCHAR(150),
    Phone      NVARCHAR(20),
    Currency   NVARCHAR(10),
    IsActive   BIT,
    City       NVARCHAR(100),
    State      NVARCHAR(100),
    Postal     NVARCHAR(20),
    Country    NVARCHAR(100),
    Line1      NVARCHAR(250)
);

INSERT INTO @Customers VALUES
('emma.johnson@example.com',   'Emma Johnson',   '+1 (604) 555-0211', 'CAD', 1, 'Vancouver',  'BC', 'V6B 1A1', 'Canada',        '210 Granville St'),
('liam.smith@example.com',     'Liam Smith',     '+1 (212) 555-0277', 'USD', 1, 'New York',   'NY', '10001',   'United States', '550 Fifth Ave'),
('olivia.brown@example.com',   'Olivia Brown',   '+44 20 5550 0322',  'GBP', 1, 'London',     NULL, 'EC1A 1BB','United Kingdom','12 Old Bailey'),
('noah.garcia@example.com',    'Noah Garcia',    '+34 91 5550 0388',  'EUR', 1, 'Madrid',     NULL, '28013',   'Spain',         'Calle Gran Via 21'),
('ava.martinez@example.com',   'Ava Martinez',   '+91 11 5550 0411',  'INR', 1, 'Mumbai',     'MH', '400001',  'India',         '88 Marine Drive'),
('ethan.davis@example.com',    'Ethan Davis',    '+61 2 5550 0466',   'AUD', 1, 'Sydney',     'NSW','2000',    'Australia',     '12 George St'),
('isabella.miller@example.com','Isabella Miller','+1 (514) 555-0512', 'CAD', 1, 'Montreal',   'QC', 'H3B 1A1', 'Canada',        '1010 Sherbrooke St'),
('mason.wilson@example.com',   'Mason Wilson',   '+1 (213) 555-0533', 'USD', 0, 'Los Angeles','CA', '90001',   'United States', '4500 Sunset Blvd');

-- Insert buyer users (skip existing emails)
INSERT INTO Users (RoleId, FullName, Email, PasswordHash, Phone, PreferredCurrency, IsActive, CreatedAt)
SELECT @BuyerRoleId, c.FullName, c.Email, @SamplePasswordHash, c.Phone, c.Currency, c.IsActive, GETUTCDATE()
FROM   @Customers c
WHERE  NOT EXISTS (SELECT 1 FROM Users u WHERE u.Email = c.Email);

-- Insert one default address per customer (skip if any exists)
INSERT INTO Addresses (UserId, FullName, Phone, AddressLine1, City, State, PostalCode, Country, IsDefault, CreatedAt)
SELECT u.UserId, c.FullName, c.Phone, c.Line1, c.City, c.State, c.Postal, c.Country, 1, GETUTCDATE()
FROM   @Customers c
JOIN   Users u ON u.Email = c.Email
WHERE  NOT EXISTS (SELECT 1 FROM Addresses a WHERE a.UserId = u.UserId);

PRINT 'Sample buyer accounts ensured.';
GO

-- ── 3. Verification ───────────────────────────────────────────────────────────
SELECT 'Sellers (sample)'   AS [Set], COUNT(*) AS [Rows]
FROM   Sellers s
WHERE  s.StoreName IN ('TechVision Store','FashionHub Boutique','ActiveLife Sports','GreenGoods Co.')
UNION ALL
SELECT 'Buyers (sample)',   COUNT(*)
FROM   Users u
JOIN   Roles r ON r.RoleId = u.RoleId
WHERE  r.RoleName = 'Buyer'
  AND  u.Email LIKE '%@example.com';
GO
