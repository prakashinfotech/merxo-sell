-- ============================================================
-- fix_admin_password.sql
-- Run this ONCE against MerxoSellDb to repair the admin user.
--
-- Credentials after running:
--   Email    : admin@temuclone.com
--   Password : Admin@123456
--
-- The hash below is BCrypt work-factor 11 of "Admin@123456".
-- ============================================================

USE MerxoSellDb;
GO

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'SuperAdmin')
    INSERT INTO Roles (RoleName) VALUES ('SuperAdmin');

DECLARE @SuperAdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'SuperAdmin');

-- Insert admin if missing, otherwise fix the password hash
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@temuclone.com')
BEGIN
    INSERT INTO Users (RoleId, FullName, Email, PasswordHash, PreferredCurrency, IsActive, CreatedAt)
    VALUES (
        @SuperAdminRoleId,
        'Administrator',
        'admin@temuclone.com',
        '$2a$11$fOnD2zcbxOANoivufHxpuO7Vqx1a0z/FzwkG69PgxvVHXgkViGxi.',
        'CAD',
        1,
        GETDATE()
    );
    PRINT 'Admin user inserted.';
END
ELSE
BEGIN
    UPDATE Users
    SET    PasswordHash = '$2a$11$fOnD2zcbxOANoivufHxpuO7Vqx1a0z/FzwkG69PgxvVHXgkViGxi.',
           IsActive     = 1,
           RoleId       = @SuperAdminRoleId
    WHERE  Email = 'admin@temuclone.com';
    PRINT 'Admin password hash updated.';
END
GO

-- Verify
SELECT UserId, RoleId, FullName, Email, IsActive,
       LEN(PasswordHash) AS HashLength
FROM   Users
WHERE  Email = 'admin@temuclone.com';
GO
