-- DriveConnect Task 7: add Promotion Reason
-- Run this on the local tenant database and the MonsterASP tenant database.
-- Safe to run more than once.

IF COL_LENGTH('dbo.Promotions', 'Reason') IS NULL
BEGIN
    ALTER TABLE dbo.Promotions
        ADD Reason NVARCHAR(500) NULL;
END;
GO

SELECT
    COL_LENGTH('dbo.Promotions', 'Reason') AS ReasonColumnExists;
GO
