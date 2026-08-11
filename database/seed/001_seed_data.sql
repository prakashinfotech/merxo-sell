-- ============================================================
-- Temu-Clone E-Commerce Platform — Seed Data
-- Run AFTER 001_create_tables.sql
-- NOTE: Replace the PasswordHash placeholder with a real
--       BCrypt hash before using in a shared environment.
--       Generate with: BCrypt.HashPassword("Admin@123", 11)
-- ============================================================

USE MerxoSellDb;
GO

-- ─────────────────────────────────────────────
-- Roles
-- ─────────────────────────────────────────────
INSERT INTO Roles (RoleName) VALUES
    ('SuperAdmin'),
    ('Seller'),
    ('Buyer');
GO

-- ─────────────────────────────────────────────
-- Admin user  (password: Admin@123)
-- Replace hash before deploying to shared environments
-- ─────────────────────────────────────────────
INSERT INTO Users (RoleId, FullName, Email, PasswordHash)
VALUES (
    (SELECT RoleId FROM Roles WHERE RoleName = 'SuperAdmin'),
    'Admin User',
    'admin@temuclone.com',
    '$2a$11$fOnD2zcbxOANoivufHxpuO7Vqx1a0z/FzwkG69PgxvVHXgkViGxi.'
);
GO

-- ─────────────────────────────────────────────
-- Root categories
-- ─────────────────────────────────────────────
INSERT INTO Categories (ParentCategoryId, Name, Slug, SortOrder) VALUES
    (NULL, 'Electronics',   'electronics',  1),
    (NULL, 'Fashion',       'fashion',      2),
    (NULL, 'Home & Garden', 'home-garden',  3),
    (NULL, 'Sports',        'sports',       4),
    (NULL, 'Beauty',        'beauty',       5);
GO

-- ─────────────────────────────────────────────
-- Subcategories
-- ─────────────────────────────────────────────
INSERT INTO Categories (ParentCategoryId, Name, Slug, SortOrder) VALUES
    (1, 'Phones',   'phones',  1),
    (1, 'Laptops',  'laptops', 2),
    (2, 'Men',      'men',     1),
    (2, 'Women',    'women',   2);
GO

-- ─────────────────────────────────────────────
-- Sample products
-- ─────────────────────────────────────────────
INSERT INTO Products (CategoryId, Name, Slug, Description, BasePrice, SalePrice, Stock) VALUES
    (6, 'Wireless Earbuds Pro',  'wireless-earbuds-pro',
        'High-quality wireless earbuds with active noise cancellation and 24-hour battery life.',
        29.99, 19.99, 150),
    (8, 'Classic White T-Shirt', 'classic-white-tshirt',
        '100% premium cotton, comfortable relaxed fit, machine washable.',
        12.99, NULL,  300);
GO

-- ─────────────────────────────────────────────
-- Product images
-- ─────────────────────────────────────────────
INSERT INTO ProductImages (ProductId, ImageUrl, AltText, IsPrimary, SortOrder) VALUES
    (1, '/images/products/earbuds-1.jpg', 'Wireless Earbuds Pro front view', 1, 1),
    (1, '/images/products/earbuds-2.jpg', 'Wireless Earbuds Pro charging case', 0, 2),
    (2, '/images/products/tshirt-1.jpg',  'White T-Shirt front view',          1, 1),
    (3, '/images/products/yogamat-1.jpg', 'Yoga Mat rolled up',                1, 1);
GO

-- ─────────────────────────────────────────────
-- Product variants
-- ─────────────────────────────────────────────
INSERT INTO ProductVariants (ProductId, Color, Size, PriceDelta, Stock, SKU) VALUES
    (1, 'Black', NULL, 0.00,  80, 'EAR-PRO-BLK'),
    (1, 'White', NULL, 2.00,  70, 'EAR-PRO-WHT'),
    (2, NULL, 'S',    0.00, 100, 'TSH-WHT-S'),
    (2, NULL, 'M',    0.00, 120, 'TSH-WHT-M'),
    (2, NULL, 'L',    0.00,  80, 'TSH-WHT-L');
GO
