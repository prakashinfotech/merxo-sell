-- ============================================================
-- 009_normalize_roles.sql
-- Normalize roles to the only supported set:
--   SuperAdmin, Seller, Buyer
-- Moves legacy Admin users to SuperAdmin and legacy Customer
-- users to Buyer, then removes the old roles when possible.
-- ============================================================

USE MerxoSellDb;
GO

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'SuperAdmin')
    INSERT INTO Roles (RoleName) VALUES ('SuperAdmin');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Seller')
    INSERT INTO Roles (RoleName) VALUES ('Seller');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Buyer')
    INSERT INTO Roles (RoleName) VALUES ('Buyer');

DECLARE @SuperAdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'SuperAdmin');
DECLARE @BuyerRoleId      INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Buyer');
DECLARE @AdminRoleId      INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin');
DECLARE @CustomerRoleId   INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Customer');

IF @AdminRoleId IS NOT NULL
BEGIN
    UPDATE Users SET RoleId = @SuperAdminRoleId WHERE RoleId = @AdminRoleId;
    IF NOT EXISTS (SELECT 1 FROM Users WHERE RoleId = @AdminRoleId)
        DELETE FROM Roles WHERE RoleId = @AdminRoleId;
END

IF @CustomerRoleId IS NOT NULL
BEGIN
    UPDATE Users SET RoleId = @BuyerRoleId WHERE RoleId = @CustomerRoleId;
    IF NOT EXISTS (SELECT 1 FROM Users WHERE RoleId = @CustomerRoleId)
        DELETE FROM Roles WHERE RoleId = @CustomerRoleId;
END

SELECT RoleId, RoleName FROM Roles ORDER BY RoleId;
GO
