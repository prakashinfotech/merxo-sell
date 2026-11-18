using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerxoSell.API.Migrations
{
    public partial class MakeBrowsingHistoryUserIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The original migration expected BrowsingHistories to already
            // exist, but no previous migration creates it.
            // Create it when missing so the migration chain can continue.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT *
                    FROM sys.objects
                    WHERE object_id = OBJECT_ID(N'[BrowsingHistories]')
                      AND type = N'U'
                )
                BEGIN
                    CREATE TABLE [BrowsingHistories] (
                        [BrowsingHistoryId] INT NOT NULL IDENTITY(1,1),
                        [UserId] INT NULL,
                        [ProductId] INT NOT NULL,
                        [ViewedAt] DATETIME2 NOT NULL,

                        CONSTRAINT [PK_BrowsingHistories]
                            PRIMARY KEY ([BrowsingHistoryId]),

                        CONSTRAINT [FK_BrowsingHistories_Users_UserId]
                            FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([UserId])
                            ON DELETE SET NULL,

                        CONSTRAINT [FK_BrowsingHistories_Products_ProductId]
                            FOREIGN KEY ([ProductId])
                            REFERENCES [Products] ([ProductId])
                            ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_BrowsingHistories_ProductId]
                        ON [BrowsingHistories] ([ProductId]);

                    CREATE INDEX [IX_BrowsingHistories_ViewedAt]
                        ON [BrowsingHistories] ([ViewedAt]);

                    CREATE UNIQUE INDEX [UQ_BrowsingHistory_User_Product]
                        ON [BrowsingHistories] ([UserId], [ProductId])
                        WHERE [UserId] IS NOT NULL;
                END
            ");

            // If the table already existed, make UserId nullable.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT *
                    FROM sys.objects
                    WHERE object_id = OBJECT_ID(N'[BrowsingHistories]')
                      AND type = N'U'
                )
                AND EXISTS (
                    SELECT *
                    FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[BrowsingHistories]')
                      AND name = N'UserId'
                      AND is_nullable = 0
                )
                BEGIN
                    DECLARE @ConstraintName sysname;

                    SELECT @ConstraintName = d.name
                    FROM sys.default_constraints d
                    INNER JOIN sys.columns c
                        ON d.parent_object_id = c.object_id
                       AND d.parent_column_id = c.column_id
                    WHERE d.parent_object_id = OBJECT_ID(N'[BrowsingHistories]')
                      AND c.name = N'UserId';

                    IF @ConstraintName IS NOT NULL
                    BEGIN
                        EXEC(
                            N'ALTER TABLE [BrowsingHistories] DROP CONSTRAINT [' +
                            @ConstraintName + N']'
                        );
                    END;

                    ALTER TABLE [BrowsingHistories]
                    ALTER COLUMN [UserId] INT NULL;
                END
            ");

            // Remove the old non-filtered unique constraint/index if present.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT *
                    FROM sys.objects
                    WHERE name = 'UQ_BrowsingHistory_User_Product'
                      AND parent_object_id = OBJECT_ID('BrowsingHistories')
                )
                BEGIN
                    ALTER TABLE [BrowsingHistories]
                    DROP CONSTRAINT [UQ_BrowsingHistory_User_Product];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT *
                    FROM sys.indexes
                    WHERE name = 'UQ_BrowsingHistory_User_Product'
                      AND object_id = OBJECT_ID('BrowsingHistories')
                )
                BEGIN
                    DROP INDEX [UQ_BrowsingHistory_User_Product]
                    ON [BrowsingHistories];
                END
            ");

            // Create the filtered unique index if it doesn't already exist.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT *
                    FROM sys.indexes
                    WHERE name = 'UQ_BrowsingHistory_User_Product'
                      AND object_id = OBJECT_ID('BrowsingHistories')
                )
                BEGIN
                    CREATE UNIQUE INDEX [UQ_BrowsingHistory_User_Product]
                        ON [BrowsingHistories] ([UserId], [ProductId])
                        WHERE [UserId] IS NOT NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_BrowsingHistory_User_Product",
                table: "BrowsingHistories");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "BrowsingHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_BrowsingHistory_User_Product",
                table: "BrowsingHistories",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }
    }
}
