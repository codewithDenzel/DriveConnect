USE [DriveConnectTenant1];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /*
        DRIVE CONNECT - REALISTIC DEMO DATA RESET + SEED

        This script:
        1. Removes tenant demo/transaction rows only.
        2. Keeps Company, Branch, Staff, Admin, and Login records in DriveConnectMaster.
        3. Reads the two active Staff users from DriveConnectMaster automatically.
        4. Splits the seeded records 50/50 between those two Staff users.
        5. Uses each Staff user's BranchId so branch isolation still works.
        6. Inserts exactly 300 tenant rows across CRM modules.
        
        Expected total:
          Sales Leads       70
          Repair Tickets    50
          Interactions     40
          Feedback         30
          Complaints       20
          Warranties       30
          Warranty Claims  15
          Maintenance      30
          Promotions       15
          -------------------
          TOTAL            300

        All customer names below are synthetic but realistic-looking sample names.
    */

    /* ------------------------------------------------------------
       1. CLEAR EXISTING TENANT DATA
       ------------------------------------------------------------ */

    DELETE FROM WarrantyClaims;
    DELETE FROM VehicleWarranties;
    DELETE FROM MaintenanceRecords;
    DELETE FROM Complaints;
    DELETE FROM Feedback;
    DELETE FROM InteractionLogs;
    DELETE FROM RepairTickets;
    DELETE FROM SalesLeads;
    DELETE FROM Promotions;

    /* ------------------------------------------------------------
       2. LOAD THE TWO ACTIVE STAFF + ONE ADMIN FROM MASTER DB
       ------------------------------------------------------------ */

    IF OBJECT_ID('tempdb..#Staff') IS NOT NULL DROP TABLE #Staff;
    IF OBJECT_ID('tempdb..#Customers') IS NOT NULL DROP TABLE #Customers;
    IF OBJECT_ID('tempdb..#Models') IS NOT NULL DROP TABLE #Models;
    IF OBJECT_ID('tempdb..#WarrantyMap') IS NOT NULL DROP TABLE #WarrantyMap;

    CREATE TABLE #Staff
    (
        RowNo INT PRIMARY KEY,
        UserId INT NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        BranchId INT NOT NULL
    );

    INSERT INTO #Staff (RowNo, UserId, FullName, BranchId)
    SELECT
        ROW_NUMBER() OVER (ORDER BY u.UserId),
        u.UserId,
        LTRIM(RTRIM(CONCAT(
            u.FirstName,
            CASE WHEN NULLIF(LTRIM(RTRIM(u.MiddleName)), '') IS NULL THEN '' ELSE ' ' + LTRIM(RTRIM(u.MiddleName)) END,
            ' ',
            u.LastName
        ))),
        u.BranchId
    FROM DriveConnectMaster.dbo.AppUsers u
    WHERE u.CompanyId = 1
      AND u.Role = 'Staff'
      AND u.IsActive = 1
      AND u.BranchId IS NOT NULL;

    IF (SELECT COUNT(*) FROM #Staff) <> 2
    BEGIN
        THROW 51001, 'Expected exactly two active Staff users for the realistic seed. Check DriveConnectMaster.dbo.AppUsers first.', 1;
    END;

    DECLARE @AdminName NVARCHAR(200);

    SELECT TOP 1
        @AdminName = LTRIM(RTRIM(CONCAT(
            u.FirstName,
            CASE WHEN NULLIF(LTRIM(RTRIM(u.MiddleName)), '') IS NULL THEN '' ELSE ' ' + LTRIM(RTRIM(u.MiddleName)) END,
            ' ',
            u.LastName
        )))
    FROM DriveConnectMaster.dbo.AppUsers u
    WHERE u.CompanyId = 1
      AND u.Role IN ('Admin', 'Company Admin')
      AND u.IsActive = 1
    ORDER BY u.UserId;

    IF @AdminName IS NULL
    BEGIN
        THROW 51002, 'No active Company Admin/Admin was found in DriveConnectMaster.', 1;
    END;

    /* ------------------------------------------------------------
       3. SYNTHETIC CUSTOMER POOL
       ------------------------------------------------------------ */

    CREATE TABLE #Customers
    (
        RowNo INT PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        MiddleName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NOT NULL,
        FullName NVARCHAR(220) NOT NULL,
        PhoneNumber NVARCHAR(50) NOT NULL,
        EmailAddress NVARCHAR(150) NOT NULL
    );

    ;WITH FirstNames AS
    (
        SELECT FirstName
        FROM (VALUES
            (N'Adrian'), (N'Alex'), (N'Alyssa'), (N'Amanda'), (N'Andrea'),
            (N'Angela'), (N'Anthony'), (N'Beatrice'), (N'Benjamin'), (N'Bianca'),
            (N'Carlo'), (N'Christian'), (N'Clarisse'), (N'Cristian'), (N'Daniel'),
            (N'David'), (N'Denise'), (N'Dominic'), (N'Elaine'), (N'Elijah'),
            (N'Emmanuel'), (N'Erika'), (N'Francis'), (N'Gabriel'), (N'Gian'),
            (N'Grace'), (N'Hannah'), (N'Harold'), (N'Isabella'), (N'Jasmine'),
            (N'Jerome'), (N'John'), (N'Joshua'), (N'Julia'), (N'Justin'),
            (N'Karen'), (N'Katrina'), (N'Kevin'), (N'Kristine'), (N'Leonardo'),
            (N'Liam'), (N'Lorenzo'), (N'Mark'), (N'Maria'), (N'Marco'),
            (N'Martin'), (N'Mathew'), (N'Michael'), (N'Michelle'), (N'Monica'),
            (N'Nathan'), (N'Nicole'), (N'Noel'), (N'Patricia'), (N'Paul'),
            (N'Paolo'), (N'Rafael'), (N'Reina'), (N'Richard'), (N'Rina'),
            (N'Robert'), (N'Rose'), (N'Ryan'), (N'Samuel'), (N'Sarah'),
            (N'Sofia'), (N'Stephen'), (N'Teresa'), (N'Trisha'), (N'Vincent'),
            (N'William'), (N'Zachary'), (N'Zoe')
        ) AS x(FirstName)
    ),
    MiddleNames AS
    (
        SELECT MiddleName
        FROM (VALUES
            (N'Cruz'), (N'Reyes'), (N'Santos'), (N'David'), (N'Jose'),
            (N'Lopez'), (N'Manuel'), (N'Ramos'), (N'Garcia'), (N'Navarro'),
            (N'Villanueva'), (N'Castillo'), (N'Dela Cruz'), (N'Mendoza'),
            (N'Fernandez'), (N'Aguilar'), (N'Mercado'), (N'Torres'),
            (N'Bautista'), (N'Pascual')
        ) AS x(MiddleName)
    ),
    LastNames AS
    (
        SELECT LastName
        FROM (VALUES
            (N'Dela Cruz'), (N'Santos'), (N'Garcia'), (N'Reyes'), (N'Bautista'),
            (N'Navarro'), (N'Mendoza'), (N'Castillo'), (N'Ramos'), (N'Fernandez'),
            (N'Gonzales'), (N'Mercado'), (N'Torres'), (N'Rivera'), (N'Aguirre'),
            (N'Villanueva'), (N'Flores'), (N'Pascual'), (N'Lim'), (N'Tan'),
            (N'Chua'), (N'Pangilinan'), (N'Valdez'), (N'Martinez'), (N'Castro'),
            (N'Salazar'), (N'Acosta'), (N'Domingo'), (N'Alvarez'), (N'Soriano')
        ) AS x(LastName)
    ),
    NamePool AS
    (
        SELECT TOP (120)
            ROW_NUMBER() OVER (ORDER BY f.FirstName, m.MiddleName, l.LastName) AS RowNo,
            f.FirstName,
            m.MiddleName,
            l.LastName
        FROM FirstNames f
        CROSS JOIN MiddleNames m
        CROSS JOIN LastNames l
    )
    INSERT INTO #Customers
    (
        RowNo,
        FirstName,
        MiddleName,
        LastName,
        FullName,
        PhoneNumber,
        EmailAddress
    )
    SELECT
        RowNo,
        FirstName,
        MiddleName,
        LastName,
        CONCAT(FirstName, N' ', MiddleName, N' ', LastName),
        CONCAT(N'0917', RIGHT(N'0000000' + CAST(1000000 + RowNo AS NVARCHAR(20)), 7)),
        LOWER(CONCAT(
            REPLACE(FirstName, N' ', N'.'),
            N'.',
            REPLACE(REPLACE(LastName, N' ', N''), N'''', N''),
            CAST(RowNo AS NVARCHAR(10)),
            N'@gmail.com'
        ))
    FROM NamePool;

    CREATE TABLE #Models
    (
        RowNo INT PRIMARY KEY,
        Model NVARCHAR(150) NOT NULL
    );

    INSERT INTO #Models (RowNo, Model)
    VALUES
        (1, N'Toyota Vios'),
        (2, N'Toyota Innova'),
        (3, N'Toyota Fortuner'),
        (4, N'Toyota Hilux'),
        (5, N'Toyota Corolla Cross'),
        (6, N'Mitsubishi Xpander'),
        (7, N'Mitsubishi Montero Sport'),
        (8, N'Honda City'),
        (9, N'Honda CR-V'),
        (10, N'Nissan Navara'),
        (11, N'Nissan Terra'),
        (12, N'Isuzu D-Max'),
        (13, N'Isuzu mu-X'),
        (14, N'Suzuki Ertiga'),
        (15, N'Hyundai Stargazer'),
        (16, N'Kia Sonet'),
        (17, N'Ford Everest'),
        (18, N'Mazda CX-5');

    /* ------------------------------------------------------------
       4. SALES LEADS - 70
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (70)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    ),
    SeedRows AS
    (
        SELECT
            n.n,
            c.FirstName,
            c.MiddleName,
            c.LastName,
            c.PhoneNumber,
            c.EmailAddress,
            m.Model,
            s.BranchId,
            s.FullName AS HandledBy,
            DATEADD(DAY, -(n.n * 4 % 330), CAST(GETDATE() AS DATE)) AS CreatedAt
        FROM N n
        JOIN #Customers c ON c.RowNo = ((n.n - 1) % 120) + 1
        JOIN #Models m ON m.RowNo = ((n.n - 1) % 18) + 1
        JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1
    )
    INSERT INTO SalesLeads
    (
        BranchId, FirstName, MiddleName, LastName,
        PhoneNumber, EmailAddress, CarModel, Status,
        EstimatedCost, HandledBy, CreatedAt, CompletedAt
    )
    SELECT
        BranchId,
        FirstName,
        MiddleName,
        LastName,
        PhoneNumber,
        EmailAddress,
        Model,
        Status,
        EstimatedCost,
        HandledBy,
        CreatedAt,
        CASE
            WHEN Status IN (N'Closed Won', N'Closed Lost', N'Archived')
                THEN DATEADD(DAY, 3 + (n % 12), CreatedAt)
            ELSE NULL
        END
    FROM
    (
        SELECT
            sr.*,
            CASE
                WHEN sr.n % 14 IN (0, 1) THEN N'Closed Won'
                WHEN sr.n % 14 = 2 THEN N'Closed Lost'
                WHEN sr.n % 14 = 3 THEN N'Archived'
                WHEN sr.n % 4 = 0 THEN N'Negotiation'
                WHEN sr.n % 3 = 0 THEN N'Test Drive Scheduled'
                ELSE N'New Inquiry'
            END AS Status,
            CAST(780000 + ((sr.n * 47300) % 1250000) AS DECIMAL(18,2)) AS EstimatedCost
        FROM SeedRows sr
    ) x;

    /* ------------------------------------------------------------
       5. REPAIR TICKETS - 50
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (50)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    INSERT INTO RepairTickets
    (
        BranchId, FirstName, MiddleName, LastName,
        PhoneNumber, EmailAddress, CarModel, Concern,
        Status, EstimatedCost, HandledBy, CreatedAt,
        CompletedAt, PickupStatus, PickedUpAt
    )
    SELECT
        s.BranchId,
        c.FirstName,
        c.MiddleName,
        c.LastName,
        c.PhoneNumber,
        c.EmailAddress,
        m.Model,
        CASE n.n % 8
            WHEN 0 THEN N'Routine preventive maintenance and inspection'
            WHEN 1 THEN N'Engine oil and filter replacement'
            WHEN 2 THEN N'Brake inspection and front brake service'
            WHEN 3 THEN N'Air conditioning cooling issue'
            WHEN 4 THEN N'Battery and charging system check'
            WHEN 5 THEN N'Suspension noise during low-speed driving'
            WHEN 6 THEN N'Check engine light diagnosis'
            ELSE N'Wheel alignment and tire balancing'
        END AS Concern,
        CASE
            WHEN n.n % 11 = 0 THEN N'New Diagnose'
            WHEN n.n % 11 IN (1, 2, 3) THEN N'In Repair'
            WHEN n.n % 11 IN (4, 5) THEN N'Waiting for Parts'
            WHEN n.n % 11 IN (6, 7) THEN N'Repaired'
            WHEN n.n % 11 = 8 THEN N'Repaired'
            ELSE N'In Repair'
        END AS Status,
        CAST(1800 + ((n.n * 725) % 28500) AS DECIMAL(18,2)) AS EstimatedCost,
        s.FullName,
        DATEADD(DAY, -(n.n * 6 % 330), CAST(GETDATE() AS DATE)),
        CASE
            WHEN n.n % 11 IN (6, 7, 8)
                THEN DATEADD(DAY, 2 + (n.n % 7), DATEADD(DAY, -(n.n * 6 % 330), CAST(GETDATE() AS DATE)))
            ELSE NULL
        END AS CompletedAt,
        CASE
            WHEN n.n % 11 IN (7, 8) THEN N'Ready for Pickup'
            WHEN n.n % 11 = 9 THEN N'Picked Up'
            ELSE N'Pending'
        END AS PickupStatus,
        CASE
            WHEN n.n % 11 = 9
                THEN DATEADD(DAY, 5 + (n.n % 5), DATEADD(DAY, -(n.n * 6 % 330), CAST(GETDATE() AS DATE)))
            ELSE NULL
        END AS PickedUpAt
    FROM N n
    JOIN #Customers c ON c.RowNo = ((n.n + 14) % 120) + 1
    JOIN #Models m ON m.RowNo = ((n.n + 4) % 18) + 1
    JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1;

    /* ------------------------------------------------------------
       6. INTERACTION LOGS - 40
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (40)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    INSERT INTO InteractionLogs
    (
        BranchId, CustomerName, PhoneNumber, InteractionType,
        Subject, Notes, HandledBy, CreatedAt
    )
    SELECT
        s.BranchId,
        c.FullName,
        c.PhoneNumber,
        CASE n.n % 5
            WHEN 0 THEN N'Phone Call'
            WHEN 1 THEN N'Walk-in'
            WHEN 2 THEN N'Email'
            WHEN 3 THEN N'Follow-up'
            ELSE N'Message'
        END,
        CASE n.n % 5
            WHEN 0 THEN N'Sales follow-up'
            WHEN 1 THEN N'Vehicle inquiry'
            WHEN 2 THEN N'Warranty question'
            WHEN 3 THEN N'Service appointment follow-up'
            ELSE N'Promotion inquiry'
        END,
        CASE n.n % 6
            WHEN 0 THEN N'Customer requested an updated quotation and financing estimate.'
            WHEN 1 THEN N'Customer asked about vehicle availability and estimated release date.'
            WHEN 2 THEN N'Customer followed up on a previous service request.'
            WHEN 3 THEN N'Customer asked about warranty coverage and claim requirements.'
            WHEN 4 THEN N'Customer requested details about the current service promotion.'
            ELSE N'Customer confirmed the next appointment and preferred contact time.'
        END,
        s.FullName,
        DATEADD(DAY, -(n.n * 8 % 330), CAST(GETDATE() AS DATE))
    FROM N n
    JOIN #Customers c ON c.RowNo = ((n.n + 30) % 120) + 1
    JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1;

    /* ------------------------------------------------------------
       7. FEEDBACK - 30
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (30)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    INSERT INTO Feedback
    (
        BranchId, CustomerName, PhoneNumber, Type,
        Rating, Comment, HandledBy, Status, CreatedAt, ReviewedAt
    )
    SELECT
        s.BranchId,
        c.FullName,
        c.PhoneNumber,
        CASE n.n % 5
            WHEN 0 THEN N'Sales Experience'
            WHEN 1 THEN N'Test Drive Experience'
            WHEN 2 THEN N'Service Experience'
            WHEN 3 THEN N'Staff Service'
            ELSE N'General Suggestion'
        END,
        CASE
            WHEN n.n % 10 IN (0, 1) THEN 3
            WHEN n.n % 10 IN (2, 3, 4) THEN 4
            ELSE 5
        END,
        CASE n.n % 6
            WHEN 0 THEN N'The sales process was clear and the staff explained the vehicle well.'
            WHEN 1 THEN N'The test drive was smooth and the staff answered my questions.'
            WHEN 2 THEN N'The service team explained the repair findings clearly.'
            WHEN 3 THEN N'The staff was courteous and responsive throughout the transaction.'
            WHEN 4 THEN N'Good overall experience. Waiting time can still be improved.'
            ELSE N'The vehicle handover and documentation process was organized.'
        END,
        s.FullName,
        CASE WHEN n.n % 4 = 0 THEN N'New' ELSE N'Reviewed' END,
        DATEADD(DAY, -(n.n * 9 % 330), CAST(GETDATE() AS DATE)),
        CASE
            WHEN n.n % 4 = 0 THEN NULL
            ELSE DATEADD(DAY, 1 + (n.n % 4), DATEADD(DAY, -(n.n * 9 % 330), CAST(GETDATE() AS DATE)))
        END
    FROM N n
    JOIN #Customers c ON c.RowNo = ((n.n + 44) % 120) + 1
    JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1;

    /* ------------------------------------------------------------
       8. COMPLAINTS - 20
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (20)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    INSERT INTO Complaints
    (
        BranchId, CustomerName, PhoneNumber, Category,
        Description, Priority, HandledBy, Status,
        Resolution, CreatedAt, ResolvedAt
    )
    SELECT
        s.BranchId,
        c.FullName,
        c.PhoneNumber,
        CASE n.n % 5
            WHEN 0 THEN N'Service Delay'
            WHEN 1 THEN N'Billing'
            WHEN 2 THEN N'Staff Service'
            WHEN 3 THEN N'Vehicle Concern'
            ELSE N'Communication'
        END,
        CASE n.n % 5
            WHEN 0 THEN N'Customer reported that the service took longer than the expected release time.'
            WHEN 1 THEN N'Customer requested clarification regarding an item on the invoice.'
            WHEN 2 THEN N'Customer reported a concern about the way the service request was handled.'
            WHEN 3 THEN N'Customer noticed a concern after vehicle release and requested inspection.'
            ELSE N'Customer reported that an expected update was not received on time.'
        END,
        CASE
            WHEN n.n % 6 IN (0, 1) THEN N'High'
            WHEN n.n % 3 = 0 THEN N'Medium'
            ELSE N'Low'
        END,
        s.FullName,
        CASE
            WHEN n.n % 5 IN (0, 1) THEN N'New'
            WHEN n.n % 5 IN (2, 3) THEN N'Investigating'
            ELSE N'Resolved'
        END,
        CASE
            WHEN n.n % 5 IN (0, 1, 2, 3)
                THEN N'Issue reviewed with the assigned staff and customer was provided an update.'
            ELSE N'Concern resolved after service review and follow-up with the customer.'
        END,
        DATEADD(DAY, -(n.n * 13 % 330), CAST(GETDATE() AS DATE)),
        CASE
            WHEN n.n % 5 = 2
                THEN DATEADD(DAY, 2 + (n.n % 5), DATEADD(DAY, -(n.n * 13 % 330), CAST(GETDATE() AS DATE)))
            ELSE NULL
        END
    FROM N n
    JOIN #Customers c ON c.RowNo = ((n.n + 54) % 120) + 1
    JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1;

    /* ------------------------------------------------------------
       9. VEHICLE WARRANTIES - 30
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (30)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    INSERT INTO VehicleWarranties
    (
        BranchId, CustomerName, PhoneNumber, VehicleModel,
        PurchaseDate, WarrantyStart, WarrantyEnd, Coverage, Status
    )
    SELECT
        s.BranchId,
        c.FullName,
        c.PhoneNumber,
        m.Model,
        DATEADD(DAY, -(60 + (n.n * 11 % 540)), CAST(GETDATE() AS DATE)),
        DATEADD(DAY, -(45 + (n.n * 11 % 540)), CAST(GETDATE() AS DATE)),
        DATEADD(MONTH, 36, DATEADD(DAY, -(45 + (n.n * 11 % 540)), CAST(GETDATE() AS DATE))),
        CASE n.n % 4
            WHEN 0 THEN N'3-Year Basic Warranty'
            WHEN 1 THEN N'Engine and Transmission'
            WHEN 2 THEN N'Comprehensive Vehicle Warranty'
            ELSE N'Parts and Labor Coverage'
        END,
        CASE
            WHEN DATEADD(MONTH, 36, DATEADD(DAY, -(45 + (n.n * 11 % 540)), CAST(GETDATE() AS DATE))) >= CAST(GETDATE() AS DATE)
                THEN N'Active'
            ELSE N'Expired'
        END
    FROM N n
    JOIN #Customers c ON c.RowNo = ((n.n + 64) % 120) + 1
    JOIN #Models m ON m.RowNo = ((n.n + 8) % 18) + 1
    JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1;

    CREATE TABLE #WarrantyMap
    (
        RowNo INT PRIMARY KEY,
        WarrantyId INT NOT NULL
    );

    INSERT INTO #WarrantyMap (RowNo, WarrantyId)
    SELECT
        ROW_NUMBER() OVER (ORDER BY WarrantyId),
        WarrantyId
    FROM VehicleWarranties;

    /* ------------------------------------------------------------
       10. WARRANTY CLAIMS - 15
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (15)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    INSERT INTO WarrantyClaims
    (
        BranchId, WarrantyId, CustomerName, PhoneNumber,
        VehicleModel, Problem, DateReported, HandledBy,
        Status, Resolution, DateResolved
    )
    SELECT
        w.BranchId,
        w.WarrantyId,
        c.FullName,
        c.PhoneNumber,
        m.Model,
        CASE n.n % 5
            WHEN 0 THEN N'Air conditioning system stopped cooling properly.'
            WHEN 1 THEN N'Power window mechanism requires inspection.'
            WHEN 2 THEN N'Warning light appeared during normal driving.'
            WHEN 3 THEN N'Battery charging issue reported by customer.'
            ELSE N'Noise detected from the suspension during driving.'
        END,
        DATEADD(DAY, -(n.n * 17 % 300), CAST(GETDATE() AS DATE)),
        s.FullName,
        CASE
            WHEN n.n % 3 = 0 THEN N'Pending'
            WHEN n.n % 3 = 1 THEN N'Approved'
            ELSE N'Resolved'
        END,
        CASE
            WHEN n.n % 3 = 2 THEN N'Warranty coverage verified and repair completed by the service team.'
            ELSE NULL
        END,
        CASE
            WHEN n.n % 3 = 2
                THEN DATEADD(DAY, 3 + (n.n % 6), DATEADD(DAY, -(n.n * 17 % 300), CAST(GETDATE() AS DATE)))
            ELSE NULL
        END
    FROM N n
    JOIN #WarrantyMap wm ON wm.RowNo = n.n
    JOIN VehicleWarranties w ON w.WarrantyId = wm.WarrantyId
    JOIN #Customers c ON c.RowNo = ((n.n + 79) % 120) + 1
    JOIN #Models m ON m.RowNo = ((n.n + 11) % 18) + 1
    JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1;

    /* ------------------------------------------------------------
       11. MAINTENANCE - 30
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (30)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    INSERT INTO MaintenanceRecords
    (
        BranchId, CustomerName, PhoneNumber, VehicleModel,
        ServiceDate, ServiceType, PlanCoverage, AssignedStaff,
        Status, Notes
    )
    SELECT
        s.BranchId,
        c.FullName,
        c.PhoneNumber,
        m.Model,
        DATEADD(DAY, -(n.n * 10 % 330), CAST(GETDATE() AS DATE)),
        CASE n.n % 3
            WHEN 0 THEN N'Free Maintenance'
            WHEN 1 THEN N'Preventive Maintenance'
            ELSE N'General Service'
        END,
        CASE n.n % 3
            WHEN 0 THEN N'1-Year Free Maintenance'
            WHEN 1 THEN N'Customer Paid'
            ELSE N'Warranty Related'
        END,
        s.FullName,
        CASE
            WHEN n.n % 5 = 0 THEN N'Rescheduled'
            WHEN n.n % 3 = 0 THEN N'Completed'
            ELSE N'Scheduled'
        END,
        CASE n.n % 4
            WHEN 0 THEN N'Customer requested inspection of brakes and fluids.'
            WHEN 1 THEN N'Routine maintenance reminder and vehicle inspection.'
            WHEN 2 THEN N'Customer requested tire pressure and battery inspection.'
            ELSE N'Periodic service scheduled according to vehicle mileage.'
        END
    FROM N n
    JOIN #Customers c ON c.RowNo = ((n.n + 89) % 120) + 1
    JOIN #Models m ON m.RowNo = ((n.n + 3) % 18) + 1
    JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1;

    /* ------------------------------------------------------------
       12. PROMOTIONS - 15
       ------------------------------------------------------------ */

    ;WITH N AS
    (
        SELECT TOP (15)
            ROW_NUMBER() OVER (ORDER BY a.object_id, b.object_id) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    )
    INSERT INTO Promotions
    (
        BranchId, Title, Description, Reason, DiscountType,
        DiscountValue, StartDate, EndDate, CreatedBy,
        ApprovedBy, Status, CreatedAt
    )
    SELECT
        s.BranchId,
        CASE n.n % 5
            WHEN 0 THEN N'Weekend Service Savings'
            WHEN 1 THEN N'New Vehicle Buyer Offer'
            WHEN 2 THEN N'Maintenance Month Deal'
            WHEN 3 THEN N'Customer Loyalty Discount'
            ELSE N'Seasonal Service Package'
        END
        + CONCAT(N' - ', CAST(2026 + (n.n % 2) AS NVARCHAR(10))),
        CASE n.n % 5
            WHEN 0 THEN N'Service package promotion for weekend appointments.'
            WHEN 1 THEN N'Special offer for customers purchasing selected vehicle models.'
            WHEN 2 THEN N'Discounted preventive maintenance package for qualified vehicles.'
            WHEN 3 THEN N'Exclusive service savings for returning customers.'
            ELSE N'Limited-time service package covering selected maintenance items.'
        END,
        CASE n.n % 4
            WHEN 0 THEN N'Increase service appointments during lower-volume periods.'
            WHEN 1 THEN N'Support vehicle sales conversion and encourage follow-up visits.'
            WHEN 2 THEN N'Encourage preventive maintenance among existing customers.'
            ELSE N'Improve customer retention through targeted service offers.'
        END,
        CASE n.n % 3
            WHEN 0 THEN N'Percentage'
            WHEN 1 THEN N'Fixed Amount'
            ELSE N'None'
        END,
        CASE n.n % 3
            WHEN 0 THEN CAST(10 + ((n.n * 3) % 16) AS DECIMAL(18,2))
            WHEN 1 THEN CAST(500 + ((n.n * 250) % 2501) AS DECIMAL(18,2))
            ELSE CAST(0 AS DECIMAL(18,2))
        END,
        DATEADD(DAY, -(30 - (n.n * 9)), CAST(GETDATE() AS DATE)),
        DATEADD(DAY, 15 + (n.n * 14), CAST(GETDATE() AS DATE)),
        s.FullName,
        CASE
            WHEN n.n % 4 = 0 THEN NULL
            ELSE @AdminName
        END,
        CASE
            WHEN n.n % 4 = 0 THEN N'Draft'
            WHEN n.n % 5 = 0 THEN N'Expired'
            ELSE N'Active'
        END,
        DATEADD(DAY, -(45 - (n.n * 2)), CAST(GETDATE() AS DATE))
    FROM N n
    JOIN #Staff s ON s.RowNo = ((n.n - 1) % 2) + 1;

    /* ------------------------------------------------------------
       13. FINAL VALIDATION
       ------------------------------------------------------------ */

    DECLARE @TotalRows INT;

    SELECT @TotalRows =
        (SELECT COUNT(*) FROM SalesLeads)
        + (SELECT COUNT(*) FROM RepairTickets)
        + (SELECT COUNT(*) FROM InteractionLogs)
        + (SELECT COUNT(*) FROM Feedback)
        + (SELECT COUNT(*) FROM Complaints)
        + (SELECT COUNT(*) FROM VehicleWarranties)
        + (SELECT COUNT(*) FROM WarrantyClaims)
        + (SELECT COUNT(*) FROM MaintenanceRecords)
        + (SELECT COUNT(*) FROM Promotions);

    IF @TotalRows <> 300
    BEGIN
        THROW 51003, 'Realistic seed did not produce exactly 300 tenant rows.', 1;
    END;

    COMMIT TRANSACTION;

    SELECT
        'SalesLeads' AS TableName, COUNT(*) AS RowCount FROM SalesLeads
    UNION ALL SELECT 'RepairTickets', COUNT(*) FROM RepairTickets
    UNION ALL SELECT 'InteractionLogs', COUNT(*) FROM InteractionLogs
    UNION ALL SELECT 'Feedback', COUNT(*) FROM Feedback
    UNION ALL SELECT 'Complaints', COUNT(*) FROM Complaints
    UNION ALL SELECT 'VehicleWarranties', COUNT(*) FROM VehicleWarranties
    UNION ALL SELECT 'WarrantyClaims', COUNT(*) FROM WarrantyClaims
    UNION ALL SELECT 'MaintenanceRecords', COUNT(*) FROM MaintenanceRecords
    UNION ALL SELECT 'Promotions', COUNT(*) FROM Promotions;

    SELECT
        s.RowNo,
        s.FullName AS StaffName,
        s.BranchId,
        COUNT(*) AS ExpectedSeedShare
    FROM #Staff s
    CROSS JOIN (SELECT 1 AS Dummy) d
    GROUP BY s.RowNo, s.FullName, s.BranchId
    ORDER BY s.RowNo;

    PRINT 'DriveConnect realistic demo data reset + seed completed successfully. Total tenant rows: 300.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
