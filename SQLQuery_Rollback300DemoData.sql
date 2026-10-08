/*
    DriveConnect - Roll back the 300-record local BI demo seed

    IMPORTANT:
      1. Run ONLY against local DriveConnectTenant1.
      2. This removes records created by SQLQuery_Seed300DemoData.sql.
      3. It does not touch DriveConnectMaster, users, branches, or logins.
      4. It does not restore the older test records that existed BEFORE the 300-row seed.
         The original seed intentionally deleted those records before inserting the 300 demo rows.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

BEGIN TRY

    DELETE FROM dbo.WarrantyClaims
    WHERE CustomerName LIKE 'Claim Customer %'
      AND PhoneNumber LIKE '0956%';

    DELETE FROM dbo.Promotions
    WHERE Title LIKE 'Demo DriveConnect Promo %';

    DELETE FROM dbo.InteractionLogs
    WHERE CustomerName LIKE 'Interaction Customer %'
      AND PhoneNumber LIKE '0977%';

    DELETE FROM dbo.MaintenanceRecords
    WHERE CustomerName LIKE 'Maintenance Customer %'
      AND PhoneNumber LIKE '0966%';

    DELETE FROM dbo.Complaints
    WHERE CustomerName LIKE 'Complaint Customer %'
      AND PhoneNumber LIKE '0935%';

    DELETE FROM dbo.Feedback
    WHERE CustomerName LIKE 'Feedback Customer %'
      AND PhoneNumber LIKE '0920%';

    DELETE FROM dbo.RepairTickets
    WHERE FirstName = 'RepairCustomer'
      AND LastName LIKE '___'
      AND PhoneNumber LIKE '0917%';

    DELETE FROM dbo.SalesLeads
    WHERE EmailAddress LIKE 'demo.sales.%@example.com';

    DELETE FROM dbo.VehicleWarranties
    WHERE CustomerName LIKE 'Warranty Customer %'
      AND PhoneNumber LIKE '0945%';

    COMMIT TRANSACTION;

    SELECT 'SalesLeads' AS TableName, COUNT(*) AS RemainingDemoRows
    FROM dbo.SalesLeads
    WHERE EmailAddress LIKE 'demo.sales.%@example.com'
    UNION ALL
    SELECT 'RepairTickets', COUNT(*)
    FROM dbo.RepairTickets
    WHERE FirstName = 'RepairCustomer'
      AND LastName LIKE '___'
      AND PhoneNumber LIKE '0917%'
    UNION ALL
    SELECT 'Promotions', COUNT(*)
    FROM dbo.Promotions
    WHERE Title LIKE 'Demo DriveConnect Promo %'
    UNION ALL
    SELECT 'Feedback', COUNT(*)
    FROM dbo.Feedback
    WHERE CustomerName LIKE 'Feedback Customer %'
      AND PhoneNumber LIKE '0920%'
    UNION ALL
    SELECT 'Complaints', COUNT(*)
    FROM dbo.Complaints
    WHERE CustomerName LIKE 'Complaint Customer %'
      AND PhoneNumber LIKE '0935%'
    UNION ALL
    SELECT 'VehicleWarranties', COUNT(*)
    FROM dbo.VehicleWarranties
    WHERE CustomerName LIKE 'Warranty Customer %'
      AND PhoneNumber LIKE '0945%'
    UNION ALL
    SELECT 'WarrantyClaims', COUNT(*)
    FROM dbo.WarrantyClaims
    WHERE CustomerName LIKE 'Claim Customer %'
      AND PhoneNumber LIKE '0956%'
    UNION ALL
    SELECT 'MaintenanceRecords', COUNT(*)
    FROM dbo.MaintenanceRecords
    WHERE CustomerName LIKE 'Maintenance Customer %'
      AND PhoneNumber LIKE '0966%'
    UNION ALL
    SELECT 'InteractionLogs', COUNT(*)
    FROM dbo.InteractionLogs
    WHERE CustomerName LIKE 'Interaction Customer %'
      AND PhoneNumber LIKE '0977%';

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
