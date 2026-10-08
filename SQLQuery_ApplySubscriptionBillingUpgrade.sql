USE [DriveConnectMaster];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH('dbo.Subscriptions', 'BillingCycle') IS NULL
    BEGIN
        ALTER TABLE dbo.Subscriptions
        ADD BillingCycle nvarchar(20) NOT NULL
            CONSTRAINT DF_Subscriptions_BillingCycle DEFAULT ('Monthly');
    END;

    IF COL_LENGTH('dbo.Subscriptions', 'BillingAmount') IS NULL
    BEGIN
        ALTER TABLE dbo.Subscriptions
        ADD BillingAmount decimal(18,2) NOT NULL
            CONSTRAINT DF_Subscriptions_BillingAmount DEFAULT (0);
    END;

    UPDATE dbo.Subscriptions
    SET BillingAmount = MonthlyFee
    WHERE BillingAmount = 0 AND MonthlyFee > 0;

    UPDATE dbo.Subscriptions
    SET BillingCycle = 'Monthly'
    WHERE BillingCycle IS NULL OR LTRIM(RTRIM(BillingCycle)) = '';

    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM dbo.__EFMigrationsHistory
           WHERE MigrationId = N'20261009000000_AddSubscriptionBillingFields'
       )
    BEGIN
        INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES (N'20261009000000_AddSubscriptionBillingFields', N'10.0.11');
    END;

    COMMIT TRANSACTION;

    SELECT
        'Subscription billing schema updated successfully.' AS Result,
        COUNT(*) AS SubscriptionCount
    FROM dbo.Subscriptions;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
