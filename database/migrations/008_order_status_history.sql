-- ============================================================
-- 008_order_status_history.sql
-- Adds OrderStatusHistories table + CancellationReason and
-- ApprovedBy columns on Orders. Used by seller order management
-- and buyer "Track Order" timeline.
-- Idempotent.
-- ============================================================

USE MerxoSellDb;
GO

-- ── 1. Orders.CancellationReason / ApprovedBy ─────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('Orders') AND name = 'CancellationReason')
BEGIN
    ALTER TABLE Orders ADD CancellationReason NVARCHAR(1000) NULL;
    PRINT 'Orders.CancellationReason added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('Orders') AND name = 'ApprovedBy')
BEGIN
    ALTER TABLE Orders ADD ApprovedBy INT NULL;
    PRINT 'Orders.ApprovedBy added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_Orders_Status' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE INDEX IX_Orders_Status ON Orders(Status);
    PRINT 'IX_Orders_Status created.';
END
GO

-- ── 2. OrderStatusHistories ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderStatusHistories')
BEGIN
    CREATE TABLE OrderStatusHistories (
        OrderStatusHistoryId INT           IDENTITY(1,1) PRIMARY KEY,
        OrderId              INT           NOT NULL,
        FromStatus           NVARCHAR(30)  NOT NULL,
        ToStatus             NVARCHAR(30)  NOT NULL,
        Note                 NVARCHAR(500) NULL,
        ChangedBy            INT           NULL,
        ChangedByRole        NVARCHAR(30)  NULL,
        CreatedAt            DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT FK_OrderStatusHistory_Orders
            FOREIGN KEY (OrderId)   REFERENCES Orders(OrderId)  ON DELETE CASCADE,
        CONSTRAINT FK_OrderStatusHistory_Users
            FOREIGN KEY (ChangedBy) REFERENCES Users(UserId)    ON DELETE SET NULL
    );

    CREATE INDEX IX_OrderStatusHistory_OrderId    ON OrderStatusHistories(OrderId);
    CREATE INDEX IX_OrderStatusHistory_CreatedAt  ON OrderStatusHistories(CreatedAt DESC);
    PRINT 'OrderStatusHistories table created.';
END
ELSE
    PRINT 'OrderStatusHistories already exists.';
GO
