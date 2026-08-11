-- ─────────────────────────────────────────────────────────────────────────────
-- DATABASE UPDATE SCRIPT: Soft Delete & Payment Methods
-- ─────────────────────────────────────────────────────────────────────────────

-- 1. Add IsDeleted column to core entities
ALTER TABLE [Users] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
ALTER TABLE [Products] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
ALTER TABLE [Sellers] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;

-- 2. Create UserPaymentMethods table
CREATE TABLE [UserPaymentMethods] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] INT NOT NULL,
    [Type] NVARCHAR(20) NOT NULL, -- 'Card' or 'UPI'
    [Label] NVARCHAR(100) NOT NULL,
    [SubLabel] NVARCHAR(100) NULL,
    [IsDefault] BIT NOT NULL DEFAULT 0,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_UserPaymentMethods] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserPaymentMethods_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

-- 3. Add indices for performance filtering
CREATE INDEX [IX_Users_IsDeleted] ON [Users] ([IsDeleted]);
CREATE INDEX [IX_Products_IsDeleted] ON [Products] ([IsDeleted]);
CREATE INDEX [IX_Sellers_IsDeleted] ON [Sellers] ([IsDeleted]);
CREATE INDEX [IX_UserPaymentMethods_UserId] ON [UserPaymentMethods] ([UserId]);
CREATE INDEX [IX_UserPaymentMethods_IsDeleted] ON [UserPaymentMethods] ([IsDeleted]);

-- 4. Set default value for existing entries (already handled by DEFAULT 0 in ALTER)
-- But ensuring all existing records are 0 explicitly if needed by the environment:
UPDATE [Users] SET [IsDeleted] = 0 WHERE [IsDeleted] IS NULL;
UPDATE [Products] SET [IsDeleted] = 0 WHERE [IsDeleted] IS NULL;
UPDATE [Sellers] SET [IsDeleted] = 0 WHERE [IsDeleted] IS NULL;
