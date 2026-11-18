using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerxoSell.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndPaymentMethodsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Users: Add IsDeleted
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT *
                    FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[Users]')
                      AND name = 'IsDeleted'
                )
                BEGIN
                    ALTER TABLE [Users]
                    ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
                END
            ");

            // Sellers: Create table if it does not exist.
            // This table was missing from the original InitialCreate migration.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT *
                    FROM sys.objects
                    WHERE object_id = OBJECT_ID(N'[Sellers]')
                      AND type = N'U'
                )
                BEGIN
                    CREATE TABLE [Sellers] (
                        [SellerId] INT NOT NULL IDENTITY(1,1),
                        [UserId] INT NOT NULL,
                        [StoreName] NVARCHAR(200) NOT NULL,
                        [StoreDescription] NVARCHAR(1000) NULL,
                        [ContactEmail] NVARCHAR(200) NULL,
                        [Phone] NVARCHAR(50) NULL,
                        [IsVerified] BIT NOT NULL DEFAULT 0,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        [IsDeleted] BIT NOT NULL DEFAULT 0,
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        [UpdatedAt] DATETIME2 NULL,

                        CONSTRAINT [PK_Sellers]
                            PRIMARY KEY ([SellerId]),

                        CONSTRAINT [FK_Sellers_Users_UserId]
                            FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([UserId])
                            ON DELETE CASCADE
                    );

                    CREATE UNIQUE INDEX [IX_Sellers_UserId]
                        ON [Sellers] ([UserId]);

                    CREATE INDEX [IX_Sellers_IsDeleted]
                        ON [Sellers] ([IsDeleted]);
                END
            ");

            // Products: Add IsDeleted
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT *
                    FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[Products]')
                      AND name = 'IsDeleted'
                )
                BEGIN
                    ALTER TABLE [Products]
                    ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
                END
            ");

            // UserPaymentMethods: Create Table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT *
                    FROM sys.objects
                    WHERE object_id = OBJECT_ID(N'[UserPaymentMethods]')
                      AND type = N'U'
                )
                BEGIN
                    CREATE TABLE [UserPaymentMethods] (
                        [Id] INT NOT NULL IDENTITY(1,1),
                        [UserId] INT NOT NULL,
                        [Type] NVARCHAR(50) NOT NULL,
                        [Label] NVARCHAR(100) NOT NULL,
                        [SubLabel] NVARCHAR(100) NULL,
                        [IsDefault] BIT NOT NULL DEFAULT 0,
                        [IsDeleted] BIT NOT NULL DEFAULT 0,
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

                        CONSTRAINT [PK_UserPaymentMethods]
                            PRIMARY KEY ([Id]),

                        CONSTRAINT [FK_UserPaymentMethods_Users_UserId]
                            FOREIGN KEY ([UserId])
                            REFERENCES [Users] ([UserId])
                            ON DELETE CASCADE
                    );
                END
            ");

            // Indices
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT *
                    FROM sys.indexes
                    WHERE name = 'IX_Users_IsDeleted'
                      AND object_id = OBJECT_ID('Users')
                )
                CREATE INDEX [IX_Users_IsDeleted]
                    ON [Users] ([IsDeleted]);

                IF NOT EXISTS (
                    SELECT *
                    FROM sys.indexes
                    WHERE name = 'IX_Products_IsDeleted'
                      AND object_id = OBJECT_ID('Products')
                )
                CREATE INDEX [IX_Products_IsDeleted]
                    ON [Products] ([IsDeleted]);

                IF NOT EXISTS (
                    SELECT *
                    FROM sys.indexes
                    WHERE name = 'IX_Sellers_IsDeleted'
                      AND object_id = OBJECT_ID('Sellers')
                )
                CREATE INDEX [IX_Sellers_IsDeleted]
                    ON [Sellers] ([IsDeleted]);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPaymentMethods");

            // Sellers was created conditionally in Up().
            // Do not drop it here automatically because this migration
            // may be applied to an existing database where Sellers
            // already existed.
        }
    }
}
