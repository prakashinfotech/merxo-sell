-- =========================================================================
-- 014_search_moderation_analytics.sql
-- Adds Address.Label, Review moderation columns, and indexes that back the
-- new global product-search and seller-analytics queries.
--
-- Idempotent — safe to run multiple times.
-- Paired EF Core migration: 20260515045336_AddSearchModerationAnalytics.cs
-- =========================================================================

SET NOCOUNT ON;

-- ── Reviews moderation columns ───────────────────────────────────────────
IF COL_LENGTH('dbo.Reviews', 'Status') IS NULL
BEGIN
    ALTER TABLE dbo.Reviews
        ADD [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_Reviews_Status DEFAULT 'Approved';
END;

IF COL_LENGTH('dbo.Reviews', 'FlagReason') IS NULL
BEGIN
    ALTER TABLE dbo.Reviews ADD [FlagReason] NVARCHAR(500) NULL;
END;

IF COL_LENGTH('dbo.Reviews', 'ModeratedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Reviews ADD [ModeratedAt] DATETIME2 NULL;
END;

IF COL_LENGTH('dbo.Reviews', 'ModeratedBy') IS NULL
BEGIN
    ALTER TABLE dbo.Reviews ADD [ModeratedBy] INT NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_Status' AND object_id = OBJECT_ID('dbo.Reviews'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reviews_Status ON dbo.Reviews([Status]);
END;

-- ── Address label ────────────────────────────────────────────────────────
IF COL_LENGTH('dbo.Addresses', 'Label') IS NULL
BEGIN
    ALTER TABLE dbo.Addresses ADD [Label] NVARCHAR(50) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Addresses_UserId_IsDefault' AND object_id = OBJECT_ID('dbo.Addresses'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Addresses_UserId_IsDefault
        ON dbo.Addresses([UserId], [IsDefault]);
END;

-- ── Product search-supporting indexes ───────────────────────────────────
-- Products.Name needs a bounded length so it can sit in a NONCLUSTERED index
-- that powers fuzzy LIKE search and ORDER BY Name. NVARCHAR(MAX) cannot be
-- indexed.
IF EXISTS (
    SELECT 1
    FROM   sys.columns c
    JOIN   sys.types   t ON c.user_type_id = t.user_type_id
    WHERE  c.object_id = OBJECT_ID('dbo.Products')
      AND  c.name = 'Name'
      AND  t.name = 'nvarchar'
      AND  c.max_length = -1
)
BEGIN
    ALTER TABLE dbo.Products ALTER COLUMN [Name] NVARCHAR(450) NOT NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Name' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_Name ON dbo.Products([Name]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CreatedAt' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_CreatedAt ON dbo.Products([CreatedAt]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Status_IsActive_IsDeleted' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_Status_IsActive_IsDeleted
        ON dbo.Products([Status], [IsActive], [IsDeleted]);
END;
