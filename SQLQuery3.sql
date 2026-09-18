USE [DriveConnectMaster];
GO

-- 1. Insert Master Company Record
IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyId = 1)
BEGIN
    SET IDENTITY_INSERT Companies ON;
    INSERT INTO Companies (CompanyId, CompanyCode, CompanyName, IsActive, CreatedAt)
    VALUES (1, 'TENANT1', 'Tenant One Auto Care', 1, GETUTCDATE());
    SET IDENTITY_INSERT Companies OFF;
END

-- 2. Insert Active Tenant Database Mapping
IF NOT EXISTS (SELECT 1 FROM CompanyDatabases WHERE CompanyId = 1)
BEGIN
    INSERT INTO CompanyDatabases (CompanyId, ServerName, DatabaseName, IsActive)
    VALUES (1, '(localdb)\mssqllocaldb', 'DriveConnectTenant1', 1);
END
ELSE
BEGIN
    UPDATE CompanyDatabases 
    SET IsActive = 1, ServerName = '(localdb)\mssqllocaldb', DatabaseName = 'DriveConnectTenant1'
    WHERE CompanyId = 1;
END
GO


SELECT * FROM CompanyDatabases WHERE CompanyId = 1;