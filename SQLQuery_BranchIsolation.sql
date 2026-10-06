-- DriveConnect tenant branch isolation
-- Run this script in LOCAL DriveConnectTenant1 and in the cloud tenant database.
-- Existing records keep BranchId = NULL. Admin can still see them; Staff will only see records assigned to their branch.

IF COL_LENGTH('dbo.SalesLeads', 'BranchId') IS NULL
    ALTER TABLE dbo.SalesLeads ADD BranchId INT NULL;
GO

IF COL_LENGTH('dbo.RepairTickets', 'BranchId') IS NULL
    ALTER TABLE dbo.RepairTickets ADD BranchId INT NULL;
GO

IF COL_LENGTH('dbo.Promotions', 'BranchId') IS NULL
    ALTER TABLE dbo.Promotions ADD BranchId INT NULL;
GO

IF COL_LENGTH('dbo.Feedback', 'BranchId') IS NULL
    ALTER TABLE dbo.Feedback ADD BranchId INT NULL;
GO

IF COL_LENGTH('dbo.Complaints', 'BranchId') IS NULL
    ALTER TABLE dbo.Complaints ADD BranchId INT NULL;
GO

IF COL_LENGTH('dbo.InteractionLogs', 'BranchId') IS NULL
    ALTER TABLE dbo.InteractionLogs ADD BranchId INT NULL;
GO

IF COL_LENGTH('dbo.VehicleWarranties', 'BranchId') IS NULL
    ALTER TABLE dbo.VehicleWarranties ADD BranchId INT NULL;
GO

IF COL_LENGTH('dbo.WarrantyClaims', 'BranchId') IS NULL
    ALTER TABLE dbo.WarrantyClaims ADD BranchId INT NULL;
GO

IF COL_LENGTH('dbo.MaintenanceRecords', 'BranchId') IS NULL
    ALTER TABLE dbo.MaintenanceRecords ADD BranchId INT NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_SalesLeads_BranchId'
      AND object_id = OBJECT_ID('dbo.SalesLeads')
)
    CREATE INDEX IX_SalesLeads_BranchId ON dbo.SalesLeads(BranchId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_RepairTickets_BranchId'
      AND object_id = OBJECT_ID('dbo.RepairTickets')
)
    CREATE INDEX IX_RepairTickets_BranchId ON dbo.RepairTickets(BranchId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Promotions_BranchId'
      AND object_id = OBJECT_ID('dbo.Promotions')
)
    CREATE INDEX IX_Promotions_BranchId ON dbo.Promotions(BranchId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Feedback_BranchId'
      AND object_id = OBJECT_ID('dbo.Feedback')
)
    CREATE INDEX IX_Feedback_BranchId ON dbo.Feedback(BranchId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Complaints_BranchId'
      AND object_id = OBJECT_ID('dbo.Complaints')
)
    CREATE INDEX IX_Complaints_BranchId ON dbo.Complaints(BranchId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_InteractionLogs_BranchId'
      AND object_id = OBJECT_ID('dbo.InteractionLogs')
)
    CREATE INDEX IX_InteractionLogs_BranchId ON dbo.InteractionLogs(BranchId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_VehicleWarranties_BranchId'
      AND object_id = OBJECT_ID('dbo.VehicleWarranties')
)
    CREATE INDEX IX_VehicleWarranties_BranchId ON dbo.VehicleWarranties(BranchId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_WarrantyClaims_BranchId'
      AND object_id = OBJECT_ID('dbo.WarrantyClaims')
)
    CREATE INDEX IX_WarrantyClaims_BranchId ON dbo.WarrantyClaims(BranchId);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_MaintenanceRecords_BranchId'
      AND object_id = OBJECT_ID('dbo.MaintenanceRecords')
)
    CREATE INDEX IX_MaintenanceRecords_BranchId ON dbo.MaintenanceRecords(BranchId);
GO
