using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerxoSell.API.Migrations
{
    /// <summary>
    /// Adds Address.Label, Review moderation columns, and the indexes that back
    /// the new global product-search and seller-analytics queries.
    ///
    /// Implemented as raw idempotent SQL so it survives databases where the
    /// paired sql script (database/migrations/014_search_moderation_analytics.sql)
    /// was applied out-of-band by ops before EF caught up. Each statement is
    /// guarded by IF (NOT) EXISTS / COL_LENGTH IS NULL so re-running is safe.
    /// </summary>
    public partial class AddSearchModerationAnalyticsV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'Status') IS NULL
                    ALTER TABLE dbo.Reviews ADD [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_Reviews_Status DEFAULT 'Approved';
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'FlagReason') IS NULL
                    ALTER TABLE dbo.Reviews ADD [FlagReason] NVARCHAR(500) NULL;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'ModeratedAt') IS NULL
                    ALTER TABLE dbo.Reviews ADD [ModeratedAt] DATETIME2 NULL;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'ModeratedBy') IS NULL
                    ALTER TABLE dbo.Reviews ADD [ModeratedBy] INT NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_Status' AND object_id = OBJECT_ID('dbo.Reviews'))
                    CREATE NONCLUSTERED INDEX IX_Reviews_Status ON dbo.Reviews([Status]);
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Addresses', 'Label') IS NULL
                    ALTER TABLE dbo.Addresses ADD [Label] NVARCHAR(50) NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Addresses_UserId_IsDefault' AND object_id = OBJECT_ID('dbo.Addresses'))
                    CREATE NONCLUSTERED INDEX IX_Addresses_UserId_IsDefault ON dbo.Addresses([UserId], [IsDefault]);
            ");

            // Products.Name needs a bounded length so it can sit in a non-clustered
            // index that powers fuzzy LIKE search and ORDER BY Name.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM   sys.columns c
                    JOIN   sys.types   t ON c.user_type_id = t.user_type_id
                    WHERE  c.object_id = OBJECT_ID('dbo.Products')
                      AND  c.name = 'Name'
                      AND  t.name = 'nvarchar'
                      AND  c.max_length = -1
                )
                    ALTER TABLE dbo.Products ALTER COLUMN [Name] NVARCHAR(450) NOT NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Name' AND object_id = OBJECT_ID('dbo.Products'))
                    CREATE NONCLUSTERED INDEX IX_Products_Name ON dbo.Products([Name]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CreatedAt' AND object_id = OBJECT_ID('dbo.Products'))
                    CREATE NONCLUSTERED INDEX IX_Products_CreatedAt ON dbo.Products([CreatedAt]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Status_IsActive_IsDeleted' AND object_id = OBJECT_ID('dbo.Products'))
                    CREATE NONCLUSTERED INDEX IX_Products_Status_IsActive_IsDeleted ON dbo.Products([Status], [IsActive], [IsDeleted]);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Status_IsActive_IsDeleted' AND object_id = OBJECT_ID('dbo.Products')) DROP INDEX IX_Products_Status_IsActive_IsDeleted ON dbo.Products;");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CreatedAt' AND object_id = OBJECT_ID('dbo.Products')) DROP INDEX IX_Products_CreatedAt ON dbo.Products;");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Name' AND object_id = OBJECT_ID('dbo.Products')) DROP INDEX IX_Products_Name ON dbo.Products;");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Addresses_UserId_IsDefault' AND object_id = OBJECT_ID('dbo.Addresses')) DROP INDEX IX_Addresses_UserId_IsDefault ON dbo.Addresses;");
            migrationBuilder.Sql("IF COL_LENGTH('dbo.Addresses', 'Label') IS NOT NULL ALTER TABLE dbo.Addresses DROP COLUMN [Label];");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reviews_Status' AND object_id = OBJECT_ID('dbo.Reviews')) DROP INDEX IX_Reviews_Status ON dbo.Reviews;");
            migrationBuilder.Sql("IF COL_LENGTH('dbo.Reviews', 'ModeratedBy') IS NOT NULL ALTER TABLE dbo.Reviews DROP COLUMN [ModeratedBy];");
            migrationBuilder.Sql("IF COL_LENGTH('dbo.Reviews', 'ModeratedAt') IS NOT NULL ALTER TABLE dbo.Reviews DROP COLUMN [ModeratedAt];");
            migrationBuilder.Sql("IF COL_LENGTH('dbo.Reviews', 'FlagReason') IS NOT NULL ALTER TABLE dbo.Reviews DROP COLUMN [FlagReason];");
            migrationBuilder.Sql("IF COL_LENGTH('dbo.Reviews', 'Status') IS NOT NULL BEGIN ALTER TABLE dbo.Reviews DROP CONSTRAINT DF_Reviews_Status; ALTER TABLE dbo.Reviews DROP COLUMN [Status]; END;");
        }
    }
}
