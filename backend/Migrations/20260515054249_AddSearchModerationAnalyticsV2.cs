using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerxoSell.API.Migrations
{
    /// <summary>
    /// Adds Address.Label, Review moderation columns, Product moderation/search
    /// columns, and supporting indexes.
    ///
    /// Implemented as idempotent SQL so it can safely run against databases
    /// where parts of the schema were already applied out-of-band.
    /// </summary>
    public partial class AddSearchModerationAnalyticsV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ------------------------------------------------------------
            // Reviews
            // ------------------------------------------------------------

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'Status') IS NULL
                    ALTER TABLE dbo.Reviews
                    ADD [Status] NVARCHAR(20) NOT NULL
                        CONSTRAINT DF_Reviews_Status DEFAULT 'Approved';
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'FlagReason') IS NULL
                    ALTER TABLE dbo.Reviews
                    ADD [FlagReason] NVARCHAR(500) NULL;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'ModeratedAt') IS NULL
                    ALTER TABLE dbo.Reviews
                    ADD [ModeratedAt] DATETIME2 NULL;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'ModeratedBy') IS NULL
                    ALTER TABLE dbo.Reviews
                    ADD [ModeratedBy] INT NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Reviews_Status'
                      AND object_id = OBJECT_ID('dbo.Reviews')
                )
                    CREATE NONCLUSTERED INDEX IX_Reviews_Status
                    ON dbo.Reviews([Status]);
            ");

            // ------------------------------------------------------------
            // Addresses
            // ------------------------------------------------------------

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Addresses', 'Label') IS NULL
                    ALTER TABLE dbo.Addresses
                    ADD [Label] NVARCHAR(50) NULL;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Addresses_UserId_IsDefault'
                      AND object_id = OBJECT_ID('dbo.Addresses')
                )
                    CREATE NONCLUSTERED INDEX IX_Addresses_UserId_IsDefault
                    ON dbo.Addresses([UserId], [IsDefault]);
            ");

            // ------------------------------------------------------------
            // Products
            // ------------------------------------------------------------

            // Product.Name needs a bounded length for indexing.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    JOIN sys.types t
                        ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID('dbo.Products')
                      AND c.name = 'Name'
                      AND t.name = 'nvarchar'
                      AND c.max_length = -1
                )
                    ALTER TABLE dbo.Products
                    ALTER COLUMN [Name] NVARCHAR(450) NOT NULL;
            ");

            // Product moderation status.
            // Existing products are treated as Pending.
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Products', 'Status') IS NULL
                    ALTER TABLE dbo.Products
                    ADD [Status] NVARCHAR(20) NOT NULL
                        CONSTRAINT DF_Products_Status DEFAULT 'Pending';
            ");

            // Product approval/moderation note.
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Products', 'ApprovalNote') IS NULL
                    ALTER TABLE dbo.Products
                    ADD [ApprovalNote] NVARCHAR(1000) NULL;
            ");

            // Product view counter.
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Products', 'ViewCount') IS NULL
                    ALTER TABLE dbo.Products
                    ADD [ViewCount] INT NOT NULL
                        CONSTRAINT DF_Products_ViewCount DEFAULT 0;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Products_Name'
                      AND object_id = OBJECT_ID('dbo.Products')
                )
                    CREATE NONCLUSTERED INDEX IX_Products_Name
                    ON dbo.Products([Name]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Products_CreatedAt'
                      AND object_id = OBJECT_ID('dbo.Products')
                )
                    CREATE NONCLUSTERED INDEX IX_Products_CreatedAt
                    ON dbo.Products([CreatedAt]);
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Products_Status_IsActive_IsDeleted'
                      AND object_id = OBJECT_ID('dbo.Products')
                )
                    CREATE NONCLUSTERED INDEX IX_Products_Status_IsActive_IsDeleted
                    ON dbo.Products([Status], [IsActive], [IsDeleted]);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Products_Status_IsActive_IsDeleted'
                      AND object_id = OBJECT_ID('dbo.Products')
                )
                    DROP INDEX IX_Products_Status_IsActive_IsDeleted
                    ON dbo.Products;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Products_CreatedAt'
                      AND object_id = OBJECT_ID('dbo.Products')
                )
                    DROP INDEX IX_Products_CreatedAt
                    ON dbo.Products;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Products_Name'
                      AND object_id = OBJECT_ID('dbo.Products')
                )
                    DROP INDEX IX_Products_Name
                    ON dbo.Products;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Products', 'ViewCount') IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.default_constraints
                        WHERE name = 'DF_Products_ViewCount'
                    )
                        ALTER TABLE dbo.Products
                        DROP CONSTRAINT DF_Products_ViewCount;

                    ALTER TABLE dbo.Products
                    DROP COLUMN [ViewCount];
                END;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Products', 'ApprovalNote') IS NOT NULL
                    ALTER TABLE dbo.Products
                    DROP COLUMN [ApprovalNote];
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Products', 'Status') IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.default_constraints
                        WHERE name = 'DF_Products_Status'
                    )
                        ALTER TABLE dbo.Products
                        DROP CONSTRAINT DF_Products_Status;

                    ALTER TABLE dbo.Products
                    DROP COLUMN [Status];
                END;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Addresses_UserId_IsDefault'
                      AND object_id = OBJECT_ID('dbo.Addresses')
                )
                    DROP INDEX IX_Addresses_UserId_IsDefault
                    ON dbo.Addresses;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Addresses', 'Label') IS NOT NULL
                    ALTER TABLE dbo.Addresses
                    DROP COLUMN [Label];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_Reviews_Status'
                      AND object_id = OBJECT_ID('dbo.Reviews')
                )
                    DROP INDEX IX_Reviews_Status
                    ON dbo.Reviews;
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'ModeratedBy') IS NOT NULL
                    ALTER TABLE dbo.Reviews
                    DROP COLUMN [ModeratedBy];
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'ModeratedAt') IS NOT NULL
                    ALTER TABLE dbo.Reviews
                    DROP COLUMN [ModeratedAt];
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'FlagReason') IS NOT NULL
                    ALTER TABLE dbo.Reviews
                    DROP COLUMN [FlagReason];
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Reviews', 'Status') IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.default_constraints
                        WHERE name = 'DF_Reviews_Status'
                    )
                        ALTER TABLE dbo.Reviews
                        DROP CONSTRAINT DF_Reviews_Status;

                    ALTER TABLE dbo.Reviews
                    DROP COLUMN [Status];
                END;
            ");
        }
    }
}
