-- DriveConnect: add display name to user accounts
-- Run this on the MASTER database used by the local API.
-- Existing users are preserved; FullName starts as NULL.
-- Afterward, edit existing Admin/Staff accounts in User Management to enter their names.

IF COL_LENGTH('dbo.AppUsers', 'FullName') IS NULL
BEGIN
    ALTER TABLE dbo.AppUsers
        ADD FullName NVARCHAR(200) NULL;
END;
GO

SELECT
    COL_LENGTH('dbo.AppUsers', 'FullName') AS FullNameColumnBytes;
GO
