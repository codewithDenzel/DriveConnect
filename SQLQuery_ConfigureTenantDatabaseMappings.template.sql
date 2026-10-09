-- TEMPLATE ONLY: run against the MASTER database after creating the tenant databases
-- and ensuring the Company rows exist. Replace the NULL IDs and placeholders first.
-- This changes routing metadata only; it does not create databases or copy CRM records.

SET XACT_ABORT ON;

DECLARE @TenantACompanyId int = NULL; -- Replace with the real CompanyId for Tenant A
DECLARE @TenantBCompanyId int = NULL; -- Replace with the real CompanyId for Tenant B
DECLARE @TenantCCompanyId int = NULL; -- Replace with the real CompanyId for Tenant C

DECLARE @ServerName nvarchar(200) = N'YOUR_SQL_SERVER';
DECLARE @TenantADatabaseName nvarchar(200) = N'DriveConnectTenantA';
DECLARE @TenantBDatabaseName nvarchar(200) = N'DriveConnectTenantB';
DECLARE @TenantCDatabaseName nvarchar(200) = N'DriveConnectTenantC';

IF @TenantACompanyId IS NULL OR @TenantBCompanyId IS NULL OR @TenantCCompanyId IS NULL
    THROW 50001, 'Replace the three tenant CompanyId values before running this script.', 1;

IF @TenantACompanyId = @TenantBCompanyId
   OR @TenantACompanyId = @TenantCCompanyId
   OR @TenantBCompanyId = @TenantCCompanyId
    THROW 50002, 'Tenant CompanyId values must be different.', 1;

IF @TenantADatabaseName = @TenantBDatabaseName
   OR @TenantADatabaseName = @TenantCDatabaseName
   OR @TenantBDatabaseName = @TenantCDatabaseName
    THROW 50003, 'Each tenant must use a different database name.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyId = @TenantACompanyId)
   OR NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyId = @TenantBCompanyId)
   OR NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyId = @TenantCCompanyId)
    THROW 50004, 'Create the Tenant A, B, and C company rows before running this script.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE dbo.CompanyDatabases
       SET ServerName = @ServerName, DatabaseName = @TenantADatabaseName, IsActive = 1
     WHERE CompanyId = @TenantACompanyId;
    IF @@ROWCOUNT = 0
        INSERT dbo.CompanyDatabases (CompanyId, ServerName, DatabaseName, IsActive)
        VALUES (@TenantACompanyId, @ServerName, @TenantADatabaseName, 1);

    UPDATE dbo.CompanyDatabases
       SET ServerName = @ServerName, DatabaseName = @TenantBDatabaseName, IsActive = 1
     WHERE CompanyId = @TenantBCompanyId;
    IF @@ROWCOUNT = 0
        INSERT dbo.CompanyDatabases (CompanyId, ServerName, DatabaseName, IsActive)
        VALUES (@TenantBCompanyId, @ServerName, @TenantBDatabaseName, 1);

    UPDATE dbo.CompanyDatabases
       SET ServerName = @ServerName, DatabaseName = @TenantCDatabaseName, IsActive = 1
     WHERE CompanyId = @TenantCCompanyId;
    IF @@ROWCOUNT = 0
        INSERT dbo.CompanyDatabases (CompanyId, ServerName, DatabaseName, IsActive)
        VALUES (@TenantCCompanyId, @ServerName, @TenantCDatabaseName, 1);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT c.CompanyId, c.CompanyName, d.ServerName, d.DatabaseName, d.IsActive
FROM dbo.Companies c
JOIN dbo.CompanyDatabases d ON d.CompanyId = c.CompanyId
WHERE c.CompanyId IN (@TenantACompanyId, @TenantBCompanyId, @TenantCCompanyId)
ORDER BY c.CompanyId;
