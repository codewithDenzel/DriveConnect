/*
    DriveConnect: local tenant sample CRM data
    Target: (localdb)\MSSQLLocalDB
    Databases: DriveConnectTenantA, DriveConnectTenantB, DriveConnectTenantC

    This script ADDS three fictional records to each of the nine CRM tables
    in each tenant. It does not delete or update existing CRM records.
    Tenant A's existing records are preserved.

    Prerequisites:
    - Local Tenant A/B/C tenant migrations have been applied.
    - DriveConnectMaster company mappings are Company 1 -> TenantA,
      Company 2 -> TenantB, Company 3 -> TenantC.
    - Main Branch IDs are 1 for Tenant A, 3 for Tenant B, and 4 for Tenant C.
    - Run this whole script once, in one query window, on the same LocalDB instance.

    A marker check prevents accidentally running it a second time.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_ID(N'DriveConnectMaster') IS NULL
    THROW 52000, 'DriveConnectMaster was not found on this SQL Server instance.', 1;
IF DB_ID(N'DriveConnectTenantA') IS NULL
   OR DB_ID(N'DriveConnectTenantB') IS NULL
   OR DB_ID(N'DriveConnectTenantC') IS NULL
    THROW 52001, 'One or more tenant databases are missing.', 1;

IF NOT EXISTS (
    SELECT 1 FROM [DriveConnectMaster].dbo.CompanyDatabases
    WHERE CompanyId = 1 AND ServerName = N'(localdb)\mssqllocaldb'
      AND DatabaseName = N'DriveConnectTenantA' AND IsActive = 1
)
   OR NOT EXISTS (
    SELECT 1 FROM [DriveConnectMaster].dbo.CompanyDatabases
    WHERE CompanyId = 2 AND ServerName = N'(localdb)\mssqllocaldb'
      AND DatabaseName = N'DriveConnectTenantB' AND IsActive = 1
)
   OR NOT EXISTS (
    SELECT 1 FROM [DriveConnectMaster].dbo.CompanyDatabases
    WHERE CompanyId = 3 AND ServerName = N'(localdb)\mssqllocaldb'
      AND DatabaseName = N'DriveConnectTenantC' AND IsActive = 1
)
    THROW 52002, 'Master database mappings do not match the expected local tenant setup.', 1;

IF NOT EXISTS (
    SELECT 1 FROM [DriveConnectMaster].dbo.Branches
    WHERE CompanyId = 1 AND BranchId = 1 AND IsActive = 1
)
   OR NOT EXISTS (
    SELECT 1 FROM [DriveConnectMaster].dbo.Branches
    WHERE CompanyId = 2 AND BranchId = 3 AND IsActive = 1
)
   OR NOT EXISTS (
    SELECT 1 FROM [DriveConnectMaster].dbo.Branches
    WHERE CompanyId = 3 AND BranchId = 4 AND IsActive = 1
)
    THROW 52003, 'Expected active branch IDs were not found. Check the master Branches table first.', 1;

-- Unique marker leads stop accidental duplicate seeding.
IF EXISTS (SELECT 1 FROM [DriveConnectTenantA].dbo.SalesLeads WHERE EmailAddress = N'carlo.lim.a@example.com')
   OR EXISTS (SELECT 1 FROM [DriveConnectTenantB].dbo.SalesLeads WHERE EmailAddress = N'liam.cruz.b@example.com')
   OR EXISTS (SELECT 1 FROM [DriveConnectTenantC].dbo.SalesLeads WHERE EmailAddress = N'elias.navarro.c@example.com')
    THROW 52004, 'Seed marker found. This script may already have run; no new records were added.', 1;

IF OBJECT_ID('tempdb..#SeedPeople') IS NOT NULL
    DROP TABLE #SeedPeople;

CREATE TABLE #SeedPeople
(
    TenantCode char(1) NOT NULL,
    SeedRow int NOT NULL,
    FirstName nvarchar(100) NOT NULL,
    MiddleName nvarchar(100) NULL,
    LastName nvarchar(100) NOT NULL,
    PhoneNumber nvarchar(50) NOT NULL,
    EmailAddress nvarchar(150) NOT NULL,
    CarModel nvarchar(150) NOT NULL,
    CONSTRAINT PK_SeedPeople PRIMARY KEY (TenantCode, SeedRow)
);

INSERT INTO #SeedPeople
    (TenantCode, SeedRow, FirstName, MiddleName, LastName, PhoneNumber, EmailAddress, CarModel)
VALUES
    ('A', 1, N'Carlo',   N'Luis',   N'Lim',       N'0917-555-0101', N'carlo.lim.a@example.com',       N'Toyota Vios'),
    ('A', 2, N'Denise',  N'Mae',    N'Cruz',      N'0917-555-0102', N'denise.cruz.a@example.com',     N'Honda City'),
    ('A', 3, N'Joaquin', N'Rafael', N'Bautista',  N'0917-555-0103', N'joaquin.bautista.a@example.com',N'Mitsubishi Xpander'),

    ('B', 1, N'Liam',    N'Andres', N'Cruz',      N'0917-555-0201', N'liam.cruz.b@example.com',       N'Honda HR-V'),
    ('B', 2, N'Sofia',   N'Mae',    N'Castillo',  N'0917-555-0202', N'sofia.castillo.b@example.com',  N'Toyota Corolla Cross'),
    ('B', 3, N'Nathan',  N'Wei',    N'Chua',      N'0917-555-0203', N'nathan.chua.b@example.com',     N'Nissan Navara'),

    ('C', 1, N'Elias',   N'Gabriel',N'Navarro',   N'0917-555-0301', N'elias.navarro.c@example.com',   N'Mazda CX-5'),
    ('C', 2, N'Camille', N'Rosa',   N'Garcia',    N'0917-555-0302', N'camille.garcia.c@example.com',  N'Ford Territory'),
    ('C', 3, N'Jordan',  N'Miguel', N'Tan',       N'0917-555-0303', N'jordan.tan.c@example.com',      N'Hyundai Tucson');

DECLARE @Now datetime2(0) = SYSDATETIME();
DECLARE @Today date = CONVERT(date, SYSDATETIME());
DECLARE @AStaff nvarchar(100) = N'Denzel Roi Mon Sinio';
DECLARE @AAdmin nvarchar(100) = N'Denzel Roi Mon Sinio';
DECLARE @BStaff nvarchar(100) = N'Adrian Luis Mendoza';
DECLARE @BAdmin nvarchar(100) = N'Elena Marie Santos';
DECLARE @CStaff nvarchar(100) = N'Sophia Claire Navarro';
DECLARE @CAdmin nvarchar(100) = N'Miguel Reyes';

DECLARE @WarrantyMap TABLE
(
    TenantCode char(1) NOT NULL,
    SeedRow int NOT NULL,
    WarrantyId int NOT NULL,
    PRIMARY KEY (TenantCode, SeedRow)
);

BEGIN TRY
    BEGIN TRANSACTION;

    ---------------------------------------------------------
    -- Tenant A: DriveConnectTenantA (CompanyId 1, BranchId 1)
    ---------------------------------------------------------

    INSERT INTO [DriveConnectTenantA].dbo.SalesLeads
        (BranchId, FirstName, MiddleName, LastName, PhoneNumber, EmailAddress,
         CarModel, Status, EstimatedCost, HandledBy, CreatedAt, CompletedAt)
    SELECT 1, p.FirstName, p.MiddleName, p.LastName, p.PhoneNumber, p.EmailAddress,
           p.CarModel, CASE p.SeedRow WHEN 1 THEN N'New Inquiry' WHEN 2 THEN N'Test Drive Scheduled' ELSE N'Follow-up' END, CASE p.SeedRow WHEN 1 THEN CAST(850000.00 AS decimal(18,2)) WHEN 2 THEN CAST(1190000.00 AS decimal(18,2)) ELSE CAST(1420000.00 AS decimal(18,2)) END, N'Denzel Roi Mon Sinio',
           DATEADD(day, -p.SeedRow * 2, @Now), NULL
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A';

    INSERT INTO [DriveConnectTenantA].dbo.RepairTickets
        (BranchId, FirstName, MiddleName, LastName, PhoneNumber, EmailAddress,
         CarModel, Concern, Status, EstimatedCost, HandledBy, CreatedAt,
         CompletedAt, PickupStatus, PickedUpAt)
    SELECT 1, p.FirstName, p.MiddleName, p.LastName, p.PhoneNumber, p.EmailAddress,
           p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN N'Engine oil and filter service'
               WHEN 2 THEN N'Brake noise during low-speed stops'
               ELSE N'Battery replacement and electrical check'
           END,
           CASE p.SeedRow WHEN 1 THEN N'In Progress' WHEN 2 THEN N'Diagnose' ELSE N'Completed' END,
           CASE p.SeedRow WHEN 1 THEN 4500.00 WHEN 2 THEN 2800.00 ELSE 7200.00 END,
           N'Denzel Roi Mon Sinio', DATEADD(day, -p.SeedRow * 3, @Now),
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END,
           CASE WHEN p.SeedRow = 3 THEN N'Picked Up' ELSE N'Pending' END,
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A';

    INSERT INTO [DriveConnectTenantA].dbo.Promotions
        (BranchId, Title, Description, Reason, DiscountType, DiscountValue,
         StartDate, EndDate, CreatedBy, ApprovedBy, Status, CreatedAt)
    VALUES
        (1, N'Weekend Maintenance Package',
         N'Save on scheduled maintenance and basic vehicle inspection.',
         N'Encourage customers to keep up with routine vehicle care.',
         N'Percentage', 10.00, DATEADD(day, -3, @Today), DATEADD(day, 30, @Today),
         N'Denzel Roi Mon Sinio', N'Denzel Roi Mon Sinio', N'Active', @Now),
        (1, N'Trade-In Upgrade Offer',
         N'A fixed-value offer for eligible vehicle trade-ins.',
         N'Support customers who are planning to upgrade their vehicles.',
         N'Fixed Amount', 5000.00, DATEADD(day, 1, @Today), DATEADD(day, 35, @Today),
         N'Denzel Roi Mon Sinio', NULL, N'Draft', @Now),
        (1, N'Complimentary Vehicle Inspection',
         N'Includes a basic multi-point vehicle inspection.',
         N'Promote preventive care and early identification of service needs.',
         N'Percentage', 15.00, DATEADD(day, -1, @Today), DATEADD(day, 21, @Today),
         N'Denzel Roi Mon Sinio', N'Denzel Roi Mon Sinio', N'Active', @Now);

    INSERT INTO [DriveConnectTenantA].dbo.Feedback
        (BranchId, CustomerName, PhoneNumber, Type, Rating, Comment,
         HandledBy, Status, CreatedAt, ReviewedAt)
    SELECT 1, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Service' WHEN 2 THEN N'Sales' ELSE N'Vehicle Delivery' END,
           CASE p.SeedRow WHEN 1 THEN 5 WHEN 2 THEN 4 ELSE 3 END,
           CASE p.SeedRow
               WHEN 1 THEN N'The service was explained clearly and the vehicle was ready on time.'
               WHEN 2 THEN N'The staff answered my questions and explained the available options.'
               ELSE N'Delivery went well, although the handover could be a little quicker.'
           END,
           N'Denzel Roi Mon Sinio',
           CASE WHEN p.SeedRow = 2 THEN N'New' ELSE N'Reviewed' END,
           DATEADD(day, -p.SeedRow * 4, @Now),
           CASE WHEN p.SeedRow = 2 THEN NULL ELSE DATEADD(day, -p.SeedRow * 2, @Now) END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A';

    INSERT INTO [DriveConnectTenantA].dbo.Complaints
        (BranchId, CustomerName, PhoneNumber, Category, Description, Priority,
         HandledBy, Status, Resolution, CreatedAt, ResolvedAt)
    SELECT 1, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Service Delay' WHEN 2 THEN N'Vehicle Concern' ELSE N'Staff Assistance' END,
           CASE p.SeedRow
               WHEN 1 THEN N'The scheduled service took longer than the estimate provided.'
               WHEN 2 THEN N'A warning light appeared shortly after the vehicle was collected.'
               ELSE N'The customer would like a clearer explanation of the service package.'
           END,
           CASE p.SeedRow WHEN 1 THEN N'Medium' WHEN 2 THEN N'High' ELSE N'Low' END,
           N'Denzel Roi Mon Sinio',
           CASE p.SeedRow WHEN 1 THEN N'Resolved' WHEN 2 THEN N'Investigating' ELSE N'New' END,
           CASE WHEN p.SeedRow = 1 THEN N'The work was reviewed with the customer and the delay was explained.' ELSE NULL END,
           DATEADD(day, -p.SeedRow * 5, @Now),
           CASE WHEN p.SeedRow = 1 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A';

    INSERT INTO [DriveConnectTenantA].dbo.InteractionLogs
        (BranchId, CustomerName, PhoneNumber, InteractionType, Subject,
         Notes, HandledBy, CreatedAt)
    SELECT 1, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Phone Call' WHEN 2 THEN N'Email' ELSE N'Walk-in' END,
           CASE p.SeedRow
               WHEN 1 THEN N'Follow-up on vehicle inquiry'
               WHEN 2 THEN N'Service appointment confirmation'
               ELSE N'Warranty coverage discussion'
           END,
           CASE p.SeedRow
               WHEN 1 THEN N'Customer asked about availability and the next steps.'
               WHEN 2 THEN N'Appointment details and expected service time were confirmed.'
               ELSE N'Coverage details were reviewed and the customer received a copy of the information.'
           END,
           N'Denzel Roi Mon Sinio', DATEADD(day, -p.SeedRow, @Now)
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A';

    -- Insert one warranty at a time so each generated WarrantyId can be
    -- linked to the matching WarrantyClaim below.
    INSERT INTO [DriveConnectTenantA].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'A' AS char(1)), 1, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 1, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -90, @Now), DATEADD(day, -90, @Now),
           DATEADD(year, 3, DATEADD(day, -90, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A' AND p.SeedRow = 1;

    INSERT INTO [DriveConnectTenantA].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'A' AS char(1)), 2, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 1, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -45, @Now), DATEADD(day, -45, @Now),
           DATEADD(year, 3, DATEADD(day, -45, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A' AND p.SeedRow = 2;

    INSERT INTO [DriveConnectTenantA].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'A' AS char(1)), 3, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 1, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -15, @Now), DATEADD(day, -15, @Now),
           DATEADD(year, 3, DATEADD(day, -15, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A' AND p.SeedRow = 3;

    INSERT INTO [DriveConnectTenantA].dbo.WarrantyClaims
        (BranchId, WarrantyId, CustomerName, PhoneNumber, VehicleModel,
         Problem, DateReported, HandledBy, Status, Resolution, DateResolved)
    SELECT 1, w.WarrantyId, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN N'Battery does not consistently hold its charge.'
               WHEN 2 THEN N'Infotainment display restarts intermittently.'
               ELSE N'Central door lock actuator is not responding consistently.'
           END,
           DATEADD(day, -p.SeedRow * 2, @Now),
           N'Denzel Roi Mon Sinio',
           CASE p.SeedRow WHEN 1 THEN N'Pending' WHEN 2 THEN N'In Review' ELSE N'Resolved' END,
           CASE WHEN p.SeedRow = 3 THEN N'The actuator was inspected and replaced under warranty.' ELSE NULL END,
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    INNER JOIN @WarrantyMap AS w
        ON w.TenantCode = 'A' AND w.SeedRow = p.SeedRow
    WHERE p.TenantCode = 'A';

    INSERT INTO [DriveConnectTenantA].dbo.MaintenanceRecords
        (BranchId, CustomerName, PhoneNumber, VehicleModel, ServiceDate,
         ServiceType, PlanCoverage, AssignedStaff, Status, Notes)
    SELECT 1, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN DATEADD(day, -2, @Now)
               WHEN 2 THEN DATEADD(day, 4, @Now)
               ELSE DATEADD(day, -1, @Now)
           END,
           CASE p.SeedRow
               WHEN 1 THEN N'Oil Change'
               WHEN 2 THEN N'10,000 km Scheduled Service'
               ELSE N'Brake Inspection'
           END,
           CASE p.SeedRow WHEN 2 THEN N'Extended Service Plan' ELSE N'Standard Service Plan' END,
           N'Denzel Roi Mon Sinio',
           CASE p.SeedRow WHEN 1 THEN N'Completed' WHEN 2 THEN N'Scheduled' ELSE N'In Progress' END,
           CASE p.SeedRow
               WHEN 1 THEN N'Engine oil and filter were changed.'
               WHEN 2 THEN N'Customer requested a morning appointment.'
               ELSE N'Inspect front brake pads and report findings.'
           END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'A';


    ---------------------------------------------------------
    -- Tenant B: DriveConnectTenantB (CompanyId 2, BranchId 3)
    ---------------------------------------------------------

    INSERT INTO [DriveConnectTenantB].dbo.SalesLeads
        (BranchId, FirstName, MiddleName, LastName, PhoneNumber, EmailAddress,
         CarModel, Status, EstimatedCost, HandledBy, CreatedAt, CompletedAt)
    SELECT 3, p.FirstName, p.MiddleName, p.LastName, p.PhoneNumber, p.EmailAddress,
           p.CarModel, CASE p.SeedRow WHEN 1 THEN N'New Inquiry' WHEN 2 THEN N'Test Drive Scheduled' ELSE N'Follow-up' END, CASE p.SeedRow WHEN 1 THEN CAST(850000.00 AS decimal(18,2)) WHEN 2 THEN CAST(1190000.00 AS decimal(18,2)) ELSE CAST(1420000.00 AS decimal(18,2)) END, N'Adrian Luis Mendoza',
           DATEADD(day, -p.SeedRow * 2, @Now), NULL
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B';

    INSERT INTO [DriveConnectTenantB].dbo.RepairTickets
        (BranchId, FirstName, MiddleName, LastName, PhoneNumber, EmailAddress,
         CarModel, Concern, Status, EstimatedCost, HandledBy, CreatedAt,
         CompletedAt, PickupStatus, PickedUpAt)
    SELECT 3, p.FirstName, p.MiddleName, p.LastName, p.PhoneNumber, p.EmailAddress,
           p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN N'Engine oil and filter service'
               WHEN 2 THEN N'Brake noise during low-speed stops'
               ELSE N'Battery replacement and electrical check'
           END,
           CASE p.SeedRow WHEN 1 THEN N'In Progress' WHEN 2 THEN N'Diagnose' ELSE N'Completed' END,
           CASE p.SeedRow WHEN 1 THEN 4500.00 WHEN 2 THEN 2800.00 ELSE 7200.00 END,
           N'Adrian Luis Mendoza', DATEADD(day, -p.SeedRow * 3, @Now),
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END,
           CASE WHEN p.SeedRow = 3 THEN N'Picked Up' ELSE N'Pending' END,
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B';

    INSERT INTO [DriveConnectTenantB].dbo.Promotions
        (BranchId, Title, Description, Reason, DiscountType, DiscountValue,
         StartDate, EndDate, CreatedBy, ApprovedBy, Status, CreatedAt)
    VALUES
        (3, N'Weekend Maintenance Package',
         N'Save on scheduled maintenance and basic vehicle inspection.',
         N'Encourage customers to keep up with routine vehicle care.',
         N'Percentage', 10.00, DATEADD(day, -3, @Today), DATEADD(day, 30, @Today),
         N'Adrian Luis Mendoza', N'Elena Marie Santos', N'Active', @Now),
        (3, N'Trade-In Upgrade Offer',
         N'A fixed-value offer for eligible vehicle trade-ins.',
         N'Support customers who are planning to upgrade their vehicles.',
         N'Fixed Amount', 5000.00, DATEADD(day, 1, @Today), DATEADD(day, 35, @Today),
         N'Adrian Luis Mendoza', NULL, N'Draft', @Now),
        (3, N'Complimentary Vehicle Inspection',
         N'Includes a basic multi-point vehicle inspection.',
         N'Promote preventive care and early identification of service needs.',
         N'Percentage', 15.00, DATEADD(day, -1, @Today), DATEADD(day, 21, @Today),
         N'Adrian Luis Mendoza', N'Elena Marie Santos', N'Active', @Now);

    INSERT INTO [DriveConnectTenantB].dbo.Feedback
        (BranchId, CustomerName, PhoneNumber, Type, Rating, Comment,
         HandledBy, Status, CreatedAt, ReviewedAt)
    SELECT 3, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Service' WHEN 2 THEN N'Sales' ELSE N'Vehicle Delivery' END,
           CASE p.SeedRow WHEN 1 THEN 5 WHEN 2 THEN 4 ELSE 3 END,
           CASE p.SeedRow
               WHEN 1 THEN N'The service was explained clearly and the vehicle was ready on time.'
               WHEN 2 THEN N'The staff answered my questions and explained the available options.'
               ELSE N'Delivery went well, although the handover could be a little quicker.'
           END,
           N'Adrian Luis Mendoza',
           CASE WHEN p.SeedRow = 2 THEN N'New' ELSE N'Reviewed' END,
           DATEADD(day, -p.SeedRow * 4, @Now),
           CASE WHEN p.SeedRow = 2 THEN NULL ELSE DATEADD(day, -p.SeedRow * 2, @Now) END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B';

    INSERT INTO [DriveConnectTenantB].dbo.Complaints
        (BranchId, CustomerName, PhoneNumber, Category, Description, Priority,
         HandledBy, Status, Resolution, CreatedAt, ResolvedAt)
    SELECT 3, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Service Delay' WHEN 2 THEN N'Vehicle Concern' ELSE N'Staff Assistance' END,
           CASE p.SeedRow
               WHEN 1 THEN N'The scheduled service took longer than the estimate provided.'
               WHEN 2 THEN N'A warning light appeared shortly after the vehicle was collected.'
               ELSE N'The customer would like a clearer explanation of the service package.'
           END,
           CASE p.SeedRow WHEN 1 THEN N'Medium' WHEN 2 THEN N'High' ELSE N'Low' END,
           N'Adrian Luis Mendoza',
           CASE p.SeedRow WHEN 1 THEN N'Resolved' WHEN 2 THEN N'Investigating' ELSE N'New' END,
           CASE WHEN p.SeedRow = 1 THEN N'The work was reviewed with the customer and the delay was explained.' ELSE NULL END,
           DATEADD(day, -p.SeedRow * 5, @Now),
           CASE WHEN p.SeedRow = 1 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B';

    INSERT INTO [DriveConnectTenantB].dbo.InteractionLogs
        (BranchId, CustomerName, PhoneNumber, InteractionType, Subject,
         Notes, HandledBy, CreatedAt)
    SELECT 3, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Phone Call' WHEN 2 THEN N'Email' ELSE N'Walk-in' END,
           CASE p.SeedRow
               WHEN 1 THEN N'Follow-up on vehicle inquiry'
               WHEN 2 THEN N'Service appointment confirmation'
               ELSE N'Warranty coverage discussion'
           END,
           CASE p.SeedRow
               WHEN 1 THEN N'Customer asked about availability and the next steps.'
               WHEN 2 THEN N'Appointment details and expected service time were confirmed.'
               ELSE N'Coverage details were reviewed and the customer received a copy of the information.'
           END,
           N'Adrian Luis Mendoza', DATEADD(day, -p.SeedRow, @Now)
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B';

    -- Insert one warranty at a time so each generated WarrantyId can be
    -- linked to the matching WarrantyClaim below.
    INSERT INTO [DriveConnectTenantB].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'B' AS char(1)), 1, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 3, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -90, @Now), DATEADD(day, -90, @Now),
           DATEADD(year, 3, DATEADD(day, -90, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B' AND p.SeedRow = 1;

    INSERT INTO [DriveConnectTenantB].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'B' AS char(1)), 2, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 3, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -45, @Now), DATEADD(day, -45, @Now),
           DATEADD(year, 3, DATEADD(day, -45, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B' AND p.SeedRow = 2;

    INSERT INTO [DriveConnectTenantB].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'B' AS char(1)), 3, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 3, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -15, @Now), DATEADD(day, -15, @Now),
           DATEADD(year, 3, DATEADD(day, -15, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B' AND p.SeedRow = 3;

    INSERT INTO [DriveConnectTenantB].dbo.WarrantyClaims
        (BranchId, WarrantyId, CustomerName, PhoneNumber, VehicleModel,
         Problem, DateReported, HandledBy, Status, Resolution, DateResolved)
    SELECT 3, w.WarrantyId, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN N'Battery does not consistently hold its charge.'
               WHEN 2 THEN N'Infotainment display restarts intermittently.'
               ELSE N'Central door lock actuator is not responding consistently.'
           END,
           DATEADD(day, -p.SeedRow * 2, @Now),
           N'Adrian Luis Mendoza',
           CASE p.SeedRow WHEN 1 THEN N'Pending' WHEN 2 THEN N'In Review' ELSE N'Resolved' END,
           CASE WHEN p.SeedRow = 3 THEN N'The actuator was inspected and replaced under warranty.' ELSE NULL END,
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    INNER JOIN @WarrantyMap AS w
        ON w.TenantCode = 'B' AND w.SeedRow = p.SeedRow
    WHERE p.TenantCode = 'B';

    INSERT INTO [DriveConnectTenantB].dbo.MaintenanceRecords
        (BranchId, CustomerName, PhoneNumber, VehicleModel, ServiceDate,
         ServiceType, PlanCoverage, AssignedStaff, Status, Notes)
    SELECT 3, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN DATEADD(day, -2, @Now)
               WHEN 2 THEN DATEADD(day, 4, @Now)
               ELSE DATEADD(day, -1, @Now)
           END,
           CASE p.SeedRow
               WHEN 1 THEN N'Oil Change'
               WHEN 2 THEN N'10,000 km Scheduled Service'
               ELSE N'Brake Inspection'
           END,
           CASE p.SeedRow WHEN 2 THEN N'Extended Service Plan' ELSE N'Standard Service Plan' END,
           N'Adrian Luis Mendoza',
           CASE p.SeedRow WHEN 1 THEN N'Completed' WHEN 2 THEN N'Scheduled' ELSE N'In Progress' END,
           CASE p.SeedRow
               WHEN 1 THEN N'Engine oil and filter were changed.'
               WHEN 2 THEN N'Customer requested a morning appointment.'
               ELSE N'Inspect front brake pads and report findings.'
           END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'B';


    ---------------------------------------------------------
    -- Tenant C: DriveConnectTenantC (CompanyId 3, BranchId 4)
    ---------------------------------------------------------

    INSERT INTO [DriveConnectTenantC].dbo.SalesLeads
        (BranchId, FirstName, MiddleName, LastName, PhoneNumber, EmailAddress,
         CarModel, Status, EstimatedCost, HandledBy, CreatedAt, CompletedAt)
    SELECT 4, p.FirstName, p.MiddleName, p.LastName, p.PhoneNumber, p.EmailAddress,
           p.CarModel, CASE p.SeedRow WHEN 1 THEN N'New Inquiry' WHEN 2 THEN N'Test Drive Scheduled' ELSE N'Follow-up' END, CASE p.SeedRow WHEN 1 THEN CAST(850000.00 AS decimal(18,2)) WHEN 2 THEN CAST(1190000.00 AS decimal(18,2)) ELSE CAST(1420000.00 AS decimal(18,2)) END, N'Sophia Claire Navarro',
           DATEADD(day, -p.SeedRow * 2, @Now), NULL
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C';

    INSERT INTO [DriveConnectTenantC].dbo.RepairTickets
        (BranchId, FirstName, MiddleName, LastName, PhoneNumber, EmailAddress,
         CarModel, Concern, Status, EstimatedCost, HandledBy, CreatedAt,
         CompletedAt, PickupStatus, PickedUpAt)
    SELECT 4, p.FirstName, p.MiddleName, p.LastName, p.PhoneNumber, p.EmailAddress,
           p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN N'Engine oil and filter service'
               WHEN 2 THEN N'Brake noise during low-speed stops'
               ELSE N'Battery replacement and electrical check'
           END,
           CASE p.SeedRow WHEN 1 THEN N'In Progress' WHEN 2 THEN N'Diagnose' ELSE N'Completed' END,
           CASE p.SeedRow WHEN 1 THEN 4500.00 WHEN 2 THEN 2800.00 ELSE 7200.00 END,
           N'Sophia Claire Navarro', DATEADD(day, -p.SeedRow * 3, @Now),
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END,
           CASE WHEN p.SeedRow = 3 THEN N'Picked Up' ELSE N'Pending' END,
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C';

    INSERT INTO [DriveConnectTenantC].dbo.Promotions
        (BranchId, Title, Description, Reason, DiscountType, DiscountValue,
         StartDate, EndDate, CreatedBy, ApprovedBy, Status, CreatedAt)
    VALUES
        (4, N'Weekend Maintenance Package',
         N'Save on scheduled maintenance and basic vehicle inspection.',
         N'Encourage customers to keep up with routine vehicle care.',
         N'Percentage', 10.00, DATEADD(day, -3, @Today), DATEADD(day, 30, @Today),
         N'Sophia Claire Navarro', N'Miguel Reyes', N'Active', @Now),
        (4, N'Trade-In Upgrade Offer',
         N'A fixed-value offer for eligible vehicle trade-ins.',
         N'Support customers who are planning to upgrade their vehicles.',
         N'Fixed Amount', 5000.00, DATEADD(day, 1, @Today), DATEADD(day, 35, @Today),
         N'Sophia Claire Navarro', NULL, N'Draft', @Now),
        (4, N'Complimentary Vehicle Inspection',
         N'Includes a basic multi-point vehicle inspection.',
         N'Promote preventive care and early identification of service needs.',
         N'Percentage', 15.00, DATEADD(day, -1, @Today), DATEADD(day, 21, @Today),
         N'Sophia Claire Navarro', N'Miguel Reyes', N'Active', @Now);

    INSERT INTO [DriveConnectTenantC].dbo.Feedback
        (BranchId, CustomerName, PhoneNumber, Type, Rating, Comment,
         HandledBy, Status, CreatedAt, ReviewedAt)
    SELECT 4, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Service' WHEN 2 THEN N'Sales' ELSE N'Vehicle Delivery' END,
           CASE p.SeedRow WHEN 1 THEN 5 WHEN 2 THEN 4 ELSE 3 END,
           CASE p.SeedRow
               WHEN 1 THEN N'The service was explained clearly and the vehicle was ready on time.'
               WHEN 2 THEN N'The staff answered my questions and explained the available options.'
               ELSE N'Delivery went well, although the handover could be a little quicker.'
           END,
           N'Sophia Claire Navarro',
           CASE WHEN p.SeedRow = 2 THEN N'New' ELSE N'Reviewed' END,
           DATEADD(day, -p.SeedRow * 4, @Now),
           CASE WHEN p.SeedRow = 2 THEN NULL ELSE DATEADD(day, -p.SeedRow * 2, @Now) END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C';

    INSERT INTO [DriveConnectTenantC].dbo.Complaints
        (BranchId, CustomerName, PhoneNumber, Category, Description, Priority,
         HandledBy, Status, Resolution, CreatedAt, ResolvedAt)
    SELECT 4, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Service Delay' WHEN 2 THEN N'Vehicle Concern' ELSE N'Staff Assistance' END,
           CASE p.SeedRow
               WHEN 1 THEN N'The scheduled service took longer than the estimate provided.'
               WHEN 2 THEN N'A warning light appeared shortly after the vehicle was collected.'
               ELSE N'The customer would like a clearer explanation of the service package.'
           END,
           CASE p.SeedRow WHEN 1 THEN N'Medium' WHEN 2 THEN N'High' ELSE N'Low' END,
           N'Sophia Claire Navarro',
           CASE p.SeedRow WHEN 1 THEN N'Resolved' WHEN 2 THEN N'Investigating' ELSE N'New' END,
           CASE WHEN p.SeedRow = 1 THEN N'The work was reviewed with the customer and the delay was explained.' ELSE NULL END,
           DATEADD(day, -p.SeedRow * 5, @Now),
           CASE WHEN p.SeedRow = 1 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C';

    INSERT INTO [DriveConnectTenantC].dbo.InteractionLogs
        (BranchId, CustomerName, PhoneNumber, InteractionType, Subject,
         Notes, HandledBy, CreatedAt)
    SELECT 4, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber,
           CASE p.SeedRow WHEN 1 THEN N'Phone Call' WHEN 2 THEN N'Email' ELSE N'Walk-in' END,
           CASE p.SeedRow
               WHEN 1 THEN N'Follow-up on vehicle inquiry'
               WHEN 2 THEN N'Service appointment confirmation'
               ELSE N'Warranty coverage discussion'
           END,
           CASE p.SeedRow
               WHEN 1 THEN N'Customer asked about availability and the next steps.'
               WHEN 2 THEN N'Appointment details and expected service time were confirmed.'
               ELSE N'Coverage details were reviewed and the customer received a copy of the information.'
           END,
           N'Sophia Claire Navarro', DATEADD(day, -p.SeedRow, @Now)
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C';

    -- Insert one warranty at a time so each generated WarrantyId can be
    -- linked to the matching WarrantyClaim below.
    INSERT INTO [DriveConnectTenantC].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'C' AS char(1)), 1, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 4, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -90, @Now), DATEADD(day, -90, @Now),
           DATEADD(year, 3, DATEADD(day, -90, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C' AND p.SeedRow = 1;

    INSERT INTO [DriveConnectTenantC].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'C' AS char(1)), 2, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 4, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -45, @Now), DATEADD(day, -45, @Now),
           DATEADD(year, 3, DATEADD(day, -45, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C' AND p.SeedRow = 2;

    INSERT INTO [DriveConnectTenantC].dbo.VehicleWarranties
        (BranchId, CustomerName, PhoneNumber, VehicleModel, PurchaseDate,
         WarrantyStart, WarrantyEnd, Coverage, Status)
    OUTPUT CAST(N'C' AS char(1)), 3, inserted.WarrantyId
        INTO @WarrantyMap (TenantCode, SeedRow, WarrantyId)
    SELECT 4, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           DATEADD(day, -15, @Now), DATEADD(day, -15, @Now),
           DATEADD(year, 3, DATEADD(day, -15, @Now)),
           N'3-year limited warranty covering eligible components.', N'Active'
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C' AND p.SeedRow = 3;

    INSERT INTO [DriveConnectTenantC].dbo.WarrantyClaims
        (BranchId, WarrantyId, CustomerName, PhoneNumber, VehicleModel,
         Problem, DateReported, HandledBy, Status, Resolution, DateResolved)
    SELECT 4, w.WarrantyId, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN N'Battery does not consistently hold its charge.'
               WHEN 2 THEN N'Infotainment display restarts intermittently.'
               ELSE N'Central door lock actuator is not responding consistently.'
           END,
           DATEADD(day, -p.SeedRow * 2, @Now),
           N'Sophia Claire Navarro',
           CASE p.SeedRow WHEN 1 THEN N'Pending' WHEN 2 THEN N'In Review' ELSE N'Resolved' END,
           CASE WHEN p.SeedRow = 3 THEN N'The actuator was inspected and replaced under warranty.' ELSE NULL END,
           CASE WHEN p.SeedRow = 3 THEN DATEADD(day, -1, @Now) ELSE NULL END
    FROM #SeedPeople AS p
    INNER JOIN @WarrantyMap AS w
        ON w.TenantCode = 'C' AND w.SeedRow = p.SeedRow
    WHERE p.TenantCode = 'C';

    INSERT INTO [DriveConnectTenantC].dbo.MaintenanceRecords
        (BranchId, CustomerName, PhoneNumber, VehicleModel, ServiceDate,
         ServiceType, PlanCoverage, AssignedStaff, Status, Notes)
    SELECT 4, CONCAT_WS(N' ', p.FirstName, NULLIF(p.MiddleName, N''), p.LastName), p.PhoneNumber, p.CarModel,
           CASE p.SeedRow
               WHEN 1 THEN DATEADD(day, -2, @Now)
               WHEN 2 THEN DATEADD(day, 4, @Now)
               ELSE DATEADD(day, -1, @Now)
           END,
           CASE p.SeedRow
               WHEN 1 THEN N'Oil Change'
               WHEN 2 THEN N'10,000 km Scheduled Service'
               ELSE N'Brake Inspection'
           END,
           CASE p.SeedRow WHEN 2 THEN N'Extended Service Plan' ELSE N'Standard Service Plan' END,
           N'Sophia Claire Navarro',
           CASE p.SeedRow WHEN 1 THEN N'Completed' WHEN 2 THEN N'Scheduled' ELSE N'In Progress' END,
           CASE p.SeedRow
               WHEN 1 THEN N'Engine oil and filter were changed.'
               WHEN 2 THEN N'Customer requested a morning appointment.'
               ELSE N'Inspect front brake pads and report findings.'
           END
    FROM #SeedPeople AS p
    WHERE p.TenantCode = 'C';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

-- Verification: expect 3 seeded rows per table in A, B, and C
-- (Tenant A's previous records are still present).
SELECT N'DriveConnectTenantA' AS DatabaseName, N'SalesLeads' AS TableName,
       COUNT_BIG(*) AS RecordCount FROM [DriveConnectTenantA].dbo.SalesLeads
UNION ALL SELECT N'DriveConnectTenantA', N'RepairTickets', COUNT_BIG(*) FROM [DriveConnectTenantA].dbo.RepairTickets
UNION ALL SELECT N'DriveConnectTenantA', N'Promotions', COUNT_BIG(*) FROM [DriveConnectTenantA].dbo.Promotions
UNION ALL SELECT N'DriveConnectTenantA', N'Feedback', COUNT_BIG(*) FROM [DriveConnectTenantA].dbo.Feedback
UNION ALL SELECT N'DriveConnectTenantA', N'Complaints', COUNT_BIG(*) FROM [DriveConnectTenantA].dbo.Complaints
UNION ALL SELECT N'DriveConnectTenantA', N'InteractionLogs', COUNT_BIG(*) FROM [DriveConnectTenantA].dbo.InteractionLogs
UNION ALL SELECT N'DriveConnectTenantA', N'VehicleWarranties', COUNT_BIG(*) FROM [DriveConnectTenantA].dbo.VehicleWarranties
UNION ALL SELECT N'DriveConnectTenantA', N'WarrantyClaims', COUNT_BIG(*) FROM [DriveConnectTenantA].dbo.WarrantyClaims
UNION ALL SELECT N'DriveConnectTenantA', N'MaintenanceRecords', COUNT_BIG(*) FROM [DriveConnectTenantA].dbo.MaintenanceRecords
UNION ALL SELECT N'DriveConnectTenantB', N'SalesLeads', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.SalesLeads
UNION ALL SELECT N'DriveConnectTenantB', N'RepairTickets', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.RepairTickets
UNION ALL SELECT N'DriveConnectTenantB', N'Promotions', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.Promotions
UNION ALL SELECT N'DriveConnectTenantB', N'Feedback', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.Feedback
UNION ALL SELECT N'DriveConnectTenantB', N'Complaints', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.Complaints
UNION ALL SELECT N'DriveConnectTenantB', N'InteractionLogs', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.InteractionLogs
UNION ALL SELECT N'DriveConnectTenantB', N'VehicleWarranties', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.VehicleWarranties
UNION ALL SELECT N'DriveConnectTenantB', N'WarrantyClaims', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.WarrantyClaims
UNION ALL SELECT N'DriveConnectTenantB', N'MaintenanceRecords', COUNT_BIG(*) FROM [DriveConnectTenantB].dbo.MaintenanceRecords
UNION ALL SELECT N'DriveConnectTenantC', N'SalesLeads', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.SalesLeads
UNION ALL SELECT N'DriveConnectTenantC', N'RepairTickets', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.RepairTickets
UNION ALL SELECT N'DriveConnectTenantC', N'Promotions', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.Promotions
UNION ALL SELECT N'DriveConnectTenantC', N'Feedback', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.Feedback
UNION ALL SELECT N'DriveConnectTenantC', N'Complaints', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.Complaints
UNION ALL SELECT N'DriveConnectTenantC', N'InteractionLogs', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.InteractionLogs
UNION ALL SELECT N'DriveConnectTenantC', N'VehicleWarranties', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.VehicleWarranties
UNION ALL SELECT N'DriveConnectTenantC', N'WarrantyClaims', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.WarrantyClaims
UNION ALL SELECT N'DriveConnectTenantC', N'MaintenanceRecords', COUNT_BIG(*) FROM [DriveConnectTenantC].dbo.MaintenanceRecords
ORDER BY DatabaseName, TableName;
