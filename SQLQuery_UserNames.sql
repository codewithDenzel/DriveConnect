-- DriveConnect: add first, middle, and last names to user accounts
-- Run this on the MASTER database used by the local API.
-- Existing users are preserved; the new name fields start as NULL/empty.
-- Afterward, edit the existing Admin/Staff accounts in User Management to enter their names.

IF COL_LENGTH('dbo.AppUsers', 'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.AppUsers
        ADD FirstName NVARCHAR(100) NULL;
END;
GO

IF COL_LENGTH('dbo.AppUsers', 'MiddleName') IS NULL
BEGIN
    ALTER TABLE dbo.AppUsers
        ADD MiddleName NVARCHAR(100) NULL;
END;
GO

IF COL_LENGTH('dbo.AppUsers', 'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.AppUsers
        ADD LastName NVARCHAR(100) NULL;
END;
GO

SELECT
    COL_LENGTH('dbo.AppUsers', 'FirstName') AS FirstNameColumnBytes,
    COL_LENGTH('dbo.AppUsers', 'MiddleName') AS MiddleNameColumnBytes,
    COL_LENGTH('dbo.AppUsers', 'LastName') AS LastNameColumnBytes;
GO
