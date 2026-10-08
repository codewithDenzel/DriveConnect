/*
    DriveConnect - Local BI Demo Data Seed
    --------------------------------------
    PURPOSE:
      Clean the LOCAL tenant CRM data and insert exactly 300 demo records.

    IMPORTANT:
      1. Run this ONLY against the LOCAL DriveConnectTenant1 database.
      2. This script deletes CRM data from the tenant tables listed below.
      3. It does NOT touch DriveConnectMaster, users, or branch definitions.
      4. Sync is currently disabled, so these records will NOT be sent to MonsterASP.
      5. Branch IDs below are the two branches already created for testing:
         @Branch1 = 1
         @Branch2 = 2

    SEEDED TOTAL = 300
      Sales Leads       100
      Repair Tickets     45
      Promotions         15
      Feedback           30
      Complaints         25
      Warranties         20
      Warranty Claims    15
      Maintenance        35
      Interactions       15
      ----------------------
      TOTAL              300
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Branch1 INT = 1;
DECLARE @Branch2 INT = 2;
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

DECLARE @StaffBranch1 NVARCHAR(100);
DECLARE @StaffBranch2 NVARCHAR(100);
DECLARE @AdminName NVARCHAR(100);

SELECT TOP (1)
    @StaffBranch1 = COALESCE(
        NULLIF(CONCAT_WS(' ',
            NULLIF(LTRIM(RTRIM(FirstName)), ''),
            NULLIF(LTRIM(RTRIM(MiddleName)), ''),
            NULLIF(LTRIM(RTRIM(LastName)), '')
        ), ''),
        Username
    )
FROM DriveConnectMaster.dbo.AppUsers
WHERE Role = 'Staff'
  AND IsActive = 1
  AND BranchId = @Branch1
ORDER BY UserId;

SELECT TOP (1)
    @StaffBranch2 = COALESCE(
        NULLIF(CONCAT_WS(' ',
            NULLIF(LTRIM(RTRIM(FirstName)), ''),
            NULLIF(LTRIM(RTRIM(MiddleName)), ''),
            NULLIF(LTRIM(RTRIM(LastName)), '')
        ), ''),
        Username
    )
FROM DriveConnectMaster.dbo.AppUsers
WHERE Role = 'Staff'
  AND IsActive = 1
  AND BranchId = @Branch2
ORDER BY UserId;

SELECT TOP (1)
    @AdminName = COALESCE(
        NULLIF(CONCAT_WS(' ',
            NULLIF(LTRIM(RTRIM(FirstName)), ''),
            NULLIF(LTRIM(RTRIM(MiddleName)), ''),
            NULLIF(LTRIM(RTRIM(LastName)), '')
        ), ''),
        Username
    )
FROM DriveConnectMaster.dbo.AppUsers
WHERE Role = 'Admin'
  AND IsActive = 1
ORDER BY UserId;

IF @StaffBranch1 IS NULL
    THROW 50003, 'No active Staff account was found for Branch 1.', 1;

IF @StaffBranch2 IS NULL
    THROW 50004, 'No active Staff account was found for Branch 2.', 1;

IF @AdminName IS NULL
    THROW 50005, 'No active Admin account was found.', 1;


-- Safety check: make sure the two branches supplied above are actually used by the system.
IF @Branch1 = @Branch2
    THROW 50001, 'Branch 1 and Branch 2 cannot be the same.', 1;

BEGIN TRANSACTION;

BEGIN TRY

    /* ============================================================
       1. CLEAN LOCAL TENANT CRM DATA
       ============================================================ */

    -- Child records first.
    DELETE FROM dbo.WarrantyClaims;
    DELETE FROM dbo.Promotions;
    DELETE FROM dbo.InteractionLogs;
    DELETE FROM dbo.MaintenanceRecords;
    DELETE FROM dbo.Complaints;
    DELETE FROM dbo.Feedback;
    DELETE FROM dbo.RepairTickets;
    DELETE FROM dbo.SalesLeads;
    DELETE FROM dbo.VehicleWarranties;

    /* ============================================================
       2. SALES LEADS - 100
       ============================================================ */

    DECLARE @i INT = 1;

    WHILE @i <= 100
    BEGIN
        DECLARE @SalesStatus NVARCHAR(50) =
            CASE
                WHEN @i % 20 = 0 THEN 'Archived'
                WHEN @i % 4 = 0 THEN 'Closed Won'
                WHEN @i % 7 = 0 THEN 'Closed Lost'
                WHEN @i % 3 = 0 THEN 'Negotiation'
                WHEN @i % 2 = 0 THEN 'Test Drive Scheduled'
                ELSE 'New Inquiry'
            END;

        DECLARE @SalesCreated DATETIME2 =
            DATEADD(DAY, -((@i * 5) % 180), @Now);

        INSERT INTO dbo.SalesLeads
        (
            BranchId,
            FirstName,
            MiddleName,
            LastName,
            PhoneNumber,
            EmailAddress,
            CarModel,
            Status,
            EstimatedCost,
            HandledBy,
            CreatedAt,
            CompletedAt
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            CASE @i % 10
                WHEN 0 THEN 'Mark'
                WHEN 1 THEN 'John'
                WHEN 2 THEN 'James'
                WHEN 3 THEN 'Daniel'
                WHEN 4 THEN 'Kevin'
                WHEN 5 THEN 'Michael'
                WHEN 6 THEN 'Ryan'
                WHEN 7 THEN 'Joshua'
                WHEN 8 THEN 'Adrian'
                ELSE 'Carlos'
            END,
            CASE @i % 4
                WHEN 0 THEN 'S.'
                WHEN 1 THEN 'M.'
                WHEN 2 THEN 'R.'
                ELSE NULL
            END,
            CONCAT('DemoCustomer', RIGHT('000' + CAST(@i AS VARCHAR(3)), 3)),
            CONCAT('0907', RIGHT('000000' + CAST(@i AS VARCHAR(6)), 6)),
            CONCAT('demo.sales.', @i, '@example.com'),
            CASE @i % 6
                WHEN 0 THEN 'Toyota Vios'
                WHEN 1 THEN 'Toyota Fortuner'
                WHEN 2 THEN 'Honda City'
                WHEN 3 THEN 'Mitsubishi Xpander'
                WHEN 4 THEN 'Ford Everest'
                ELSE 'Hyundai Creta'
            END,
            @SalesStatus,
            CAST(850000 + ((@i * 37500) % 2200000) AS DECIMAL(18,2)),
            CASE WHEN @i % 2 = 0 THEN @StaffBranch1 ELSE @StaffBranch2 END,
            @SalesCreated,
            CASE
                WHEN @SalesStatus IN ('Closed Won', 'Closed Lost', 'Archived')
                    THEN DATEADD(DAY, 3 + (@i % 12), @SalesCreated)
                ELSE NULL
            END
        );

        SET @i += 1;
    END;

    /* ============================================================
       3. REPAIR TICKETS - 45
       ============================================================ */

    SET @i = 1;

    WHILE @i <= 45
    BEGIN
        DECLARE @RepairStatus NVARCHAR(50) =
            CASE
                WHEN @i % 15 = 0 THEN 'Archived'
                WHEN @i % 5 = 0 THEN 'Repaired'
                WHEN @i % 4 = 0 THEN 'Waiting for Parts'
                WHEN @i % 3 = 0 THEN 'In Repair'
                ELSE 'New Diagnose'
            END;

        DECLARE @PickupStatus NVARCHAR(50) =
            CASE
                WHEN @RepairStatus = 'Repaired' AND @i % 2 = 0 THEN 'Picked Up'
                WHEN @RepairStatus = 'Archived' THEN 'Picked Up'
                WHEN @RepairStatus = 'Repaired' THEN 'Ready for Pickup'
                ELSE 'Pending'
            END;

        DECLARE @RepairCreated DATETIME2 =
            DATEADD(DAY, -((@i * 4) % 180), @Now);

        INSERT INTO dbo.RepairTickets
        (
            BranchId,
            FirstName,
            MiddleName,
            LastName,
            PhoneNumber,
            EmailAddress,
            CarModel,
            Concern,
            Status,
            EstimatedCost,
            HandledBy,
            CreatedAt,
            CompletedAt,
            PickupStatus,
            PickedUpAt
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            'RepairCustomer',
            NULL,
            RIGHT('000' + CAST(@i AS VARCHAR(3)), 3),
            CONCAT('0917', RIGHT('000000' + CAST(@i + 200 AS VARCHAR(6)), 6)),
            CONCAT('demo.repair.', @i, '@example.com'),
            CASE @i % 5
                WHEN 0 THEN 'Toyota Innova'
                WHEN 1 THEN 'Toyota Hilux'
                WHEN 2 THEN 'Honda CR-V'
                WHEN 3 THEN 'Mitsubishi Montero'
                ELSE 'Ford Ranger'
            END,
            CASE @i % 5
                WHEN 0 THEN 'Engine inspection'
                WHEN 1 THEN 'Brake service'
                WHEN 2 THEN 'Air-conditioning issue'
                WHEN 3 THEN 'Oil leak inspection'
                ELSE 'Battery replacement'
            END,
            @RepairStatus,
            CAST(3500 + ((@i * 725) % 42000) AS DECIMAL(18,2)),
            CASE WHEN @i % 2 = 0 THEN @StaffBranch1 ELSE @StaffBranch2 END,
            @RepairCreated,
            CASE
                WHEN @RepairStatus IN ('Repaired', 'Archived')
                    THEN DATEADD(DAY, 2 + (@i % 8), @RepairCreated)
                ELSE NULL
            END,
            @PickupStatus,
            CASE
                WHEN @PickupStatus = 'Picked Up'
                    THEN DATEADD(DAY, 1 + (@i % 5), @RepairCreated)
                ELSE NULL
            END
        );

        SET @i += 1;
    END;

    /* ============================================================
       4. PROMOTIONS - 15
       ============================================================ */

    SET @i = 1;

    WHILE @i <= 15
    BEGIN
        DECLARE @PromotionStatus NVARCHAR(30) =
            CASE
                WHEN @i % 5 = 0 THEN 'Expired'
                WHEN @i % 4 = 0 THEN 'Draft'
                ELSE 'Active'
            END;

        INSERT INTO dbo.Promotions
        (
            BranchId,
            Title,
            Description,
            Reason,
            DiscountType,
            DiscountValue,
            StartDate,
            EndDate,
            CreatedBy,
            ApprovedBy,
            Status,
            CreatedAt
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            CONCAT('Demo DriveConnect Promo ', @i),
            'Sample dealership promotion for BI and CRM testing.',
            CASE @i % 3
                WHEN 0 THEN 'Increase showroom inquiries'
                WHEN 1 THEN 'Support monthly sales target'
                ELSE 'Promote after-sales service'
            END,
            CASE WHEN @i % 2 = 0 THEN 'Percentage' ELSE 'Fixed' END,
            CAST(CASE WHEN @i % 2 = 0 THEN 5 + (@i % 11) ELSE 2000 + (@i * 250) END AS DECIMAL(18,2)),
            DATEADD(DAY, -30 + (@i * 2), @Now),
            DATEADD(DAY, 15 + (@i * 3), @Now),
            CASE WHEN @i % 2 = 0 THEN @StaffBranch1 ELSE @StaffBranch2 END,
            CASE
                WHEN @PromotionStatus IN ('Active', 'Expired') THEN @AdminName
                ELSE NULL
            END,
            @PromotionStatus,
            DATEADD(DAY, -((@i * 7) % 120), @Now)
        );

        SET @i += 1;
    END;

    /* ============================================================
       5. FEEDBACK - 30
       ============================================================ */

    SET @i = 1;

    WHILE @i <= 30
    BEGIN
        DECLARE @Rating INT =
            CASE
                WHEN @i % 10 = 0 THEN 2
                WHEN @i % 7 = 0 THEN 3
                WHEN @i % 4 = 0 THEN 4
                ELSE 5
            END;

        DECLARE @FeedbackStatus NVARCHAR(30) =
            CASE WHEN @i % 3 = 0 THEN 'New' ELSE 'Reviewed' END;

        DECLARE @FeedbackCreated DATETIME2 =
            DATEADD(DAY, -((@i * 6) % 180), @Now);

        INSERT INTO dbo.Feedback
        (
            BranchId,
            CustomerName,
            PhoneNumber,
            Type,
            Rating,
            Comment,
            HandledBy,
            Status,
            CreatedAt,
            ReviewedAt
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            CONCAT('Feedback Customer ', @i),
            CONCAT('0920', RIGHT('000000' + CAST(@i + 300 AS VARCHAR(6)), 6)),
            CASE @i % 3
                WHEN 0 THEN 'Sales'
                WHEN 1 THEN 'Service'
                ELSE 'Overall Experience'
            END,
            @Rating,
            CASE @Rating
                WHEN 5 THEN 'Excellent experience and helpful staff.'
                WHEN 4 THEN 'Good service with minor delays.'
                WHEN 3 THEN 'Acceptable service but room for improvement.'
                ELSE 'Customer reported concerns requiring follow-up.'
            END,
            CASE WHEN @i % 2 = 0 THEN @StaffBranch1 ELSE @StaffBranch2 END,
            @FeedbackStatus,
            @FeedbackCreated,
            CASE
                WHEN @FeedbackStatus = 'Reviewed'
                    THEN DATEADD(DAY, 1 + (@i % 4), @FeedbackCreated)
                ELSE NULL
            END
        );

        SET @i += 1;
    END;

    /* ============================================================
       6. COMPLAINTS - 25
       ============================================================ */

    SET @i = 1;

    WHILE @i <= 25
    BEGIN
        DECLARE @ComplaintStatus NVARCHAR(30) =
            CASE
                WHEN @i % 6 = 0 THEN 'Closed'
                WHEN @i % 4 = 0 THEN 'Resolved'
                WHEN @i % 3 = 0 THEN 'Investigating'
                ELSE 'New'
            END;

        DECLARE @Priority NVARCHAR(30) =
            CASE
                WHEN @i % 5 = 0 THEN 'High'
                WHEN @i % 2 = 0 THEN 'Medium'
                ELSE 'Low'
            END;

        DECLARE @ComplaintCreated DATETIME2 =
            DATEADD(DAY, -((@i * 7) % 180), @Now);

        INSERT INTO dbo.Complaints
        (
            BranchId,
            CustomerName,
            PhoneNumber,
            Category,
            Description,
            Priority,
            HandledBy,
            Status,
            Resolution,
            CreatedAt,
            ResolvedAt
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            CONCAT('Complaint Customer ', @i),
            CONCAT('0935', RIGHT('000000' + CAST(@i + 400 AS VARCHAR(6)), 6)),
            CASE @i % 4
                WHEN 0 THEN 'Service'
                WHEN 1 THEN 'Sales'
                WHEN 2 THEN 'Staff'
                ELSE 'Vehicle'
            END,
            'Sample complaint created for management reporting and follow-up testing.',
            @Priority,
            CASE WHEN @i % 2 = 0 THEN @StaffBranch1 ELSE @StaffBranch2 END,
            @ComplaintStatus,
            CASE
                WHEN @ComplaintStatus IN ('Resolved', 'Closed')
                    THEN 'Issue reviewed and addressed by dealership staff.'
                ELSE NULL
            END,
            @ComplaintCreated,
            CASE
                WHEN @ComplaintStatus IN ('Resolved', 'Closed')
                    THEN DATEADD(DAY, 2 + (@i % 6), @ComplaintCreated)
                ELSE NULL
            END
        );

        SET @i += 1;
    END;

    /* ============================================================
       7. VEHICLE WARRANTIES - 20
       ============================================================ */

    SET @i = 1;

    WHILE @i <= 20
    BEGIN
        DECLARE @WarrantyStart DATETIME2 =
            DATEADD(DAY, -((@i * 8) % 365), @Now);

        INSERT INTO dbo.VehicleWarranties
        (
            BranchId,
            CustomerName,
            PhoneNumber,
            VehicleModel,
            PurchaseDate,
            WarrantyStart,
            WarrantyEnd,
            Coverage,
            Status
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            CONCAT('Warranty Customer ', @i),
            CONCAT('0945', RIGHT('000000' + CAST(@i + 500 AS VARCHAR(6)), 6)),
            CASE @i % 5
                WHEN 0 THEN 'Toyota Veloz'
                WHEN 1 THEN 'Toyota Fortuner'
                WHEN 2 THEN 'Honda HR-V'
                WHEN 3 THEN 'Mitsubishi Xpander'
                ELSE 'Ford Territory'
            END,
            DATEADD(DAY, -30, @WarrantyStart),
            @WarrantyStart,
            DATEADD(MONTH, 3 + (@i % 7), @WarrantyStart),
            CASE @i % 3
                WHEN 0 THEN 'Engine and drivetrain'
                WHEN 1 THEN 'Parts and labor'
                ELSE 'General vehicle warranty'
            END,
            CASE
                WHEN @i % 5 = 0 THEN 'Expired'
                WHEN @i % 4 = 0 THEN 'Pending'
                ELSE 'Active'
            END
        );

        SET @i += 1;
    END;

    /* ============================================================
       8. WARRANTY CLAIMS - 15
       ============================================================ */

    SET @i = 1;

    WHILE @i <= 15
    BEGIN
        DECLARE @WarrantyId INT;

        SELECT @WarrantyId = WarrantyId
        FROM
        (
            SELECT WarrantyId, ROW_NUMBER() OVER (ORDER BY WarrantyId) AS RowNum
            FROM dbo.VehicleWarranties
        ) w
        WHERE w.RowNum = ((@i - 1) % 20) + 1;

        DECLARE @ClaimStatus NVARCHAR(30) =
            CASE
                WHEN @i % 5 = 0 THEN 'Rejected'
                WHEN @i % 3 = 0 THEN 'Resolved'
                ELSE 'Pending'
            END;

        DECLARE @ClaimDate DATETIME2 =
            DATEADD(DAY, -((@i * 9) % 150), @Now);

        INSERT INTO dbo.WarrantyClaims
        (
            BranchId,
            WarrantyId,
            CustomerName,
            PhoneNumber,
            VehicleModel,
            Problem,
            DateReported,
            HandledBy,
            Status,
            Resolution,
            DateResolved
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            @WarrantyId,
            CONCAT('Claim Customer ', @i),
            CONCAT('0956', RIGHT('000000' + CAST(@i + 600 AS VARCHAR(6)), 6)),
            CASE @i % 5
                WHEN 0 THEN 'Toyota Veloz'
                WHEN 1 THEN 'Toyota Fortuner'
                WHEN 2 THEN 'Honda HR-V'
                WHEN 3 THEN 'Mitsubishi Xpander'
                ELSE 'Ford Territory'
            END,
            CASE @i % 3
                WHEN 0 THEN 'Electrical issue'
                WHEN 1 THEN 'Engine concern'
                ELSE 'Parts failure'
            END,
            @ClaimDate,
            CASE WHEN @i % 2 = 0 THEN @StaffBranch1 ELSE @StaffBranch2 END,
            @ClaimStatus,
            CASE
                WHEN @ClaimStatus = 'Resolved' THEN 'Warranty claim approved and service completed.'
                WHEN @ClaimStatus = 'Rejected' THEN 'Claim did not meet warranty coverage requirements.'
                ELSE NULL
            END,
            CASE
                WHEN @ClaimStatus IN ('Resolved', 'Rejected')
                    THEN DATEADD(DAY, 2 + (@i % 5), @ClaimDate)
                ELSE NULL
            END
        );

        SET @i += 1;
    END;

    /* ============================================================
       9. MAINTENANCE - 35
       ============================================================ */

    SET @i = 1;

    WHILE @i <= 35
    BEGIN
        DECLARE @MaintenanceStatus NVARCHAR(30) =
            CASE
                WHEN @i % 8 = 0 THEN 'Completed'
                WHEN @i % 6 = 0 THEN 'Rescheduled'
                WHEN @i % 4 = 0 THEN 'Cancelled'
                ELSE 'Scheduled'
            END;

        INSERT INTO dbo.MaintenanceRecords
        (
            BranchId,
            CustomerName,
            PhoneNumber,
            VehicleModel,
            ServiceDate,
            ServiceType,
            PlanCoverage,
            AssignedStaff,
            Status,
            Notes
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            CONCAT('Maintenance Customer ', @i),
            CONCAT('0966', RIGHT('000000' + CAST(@i + 700 AS VARCHAR(6)), 6)),
            CASE @i % 5
                WHEN 0 THEN 'Toyota Vios'
                WHEN 1 THEN 'Toyota Innova'
                WHEN 2 THEN 'Honda City'
                WHEN 3 THEN 'Mitsubishi Xpander'
                ELSE 'Ford Territory'
            END,
            DATEADD(DAY, -((@i * 5) % 120) + CASE WHEN @i % 6 = 0 THEN 7 ELSE 0 END, @Now),
            CASE @i % 4
                WHEN 0 THEN 'Oil Change'
                WHEN 1 THEN 'Preventive Maintenance'
                WHEN 2 THEN 'Brake Inspection'
                ELSE 'General Checkup'
            END,
            CASE @i % 3
                WHEN 0 THEN 'Basic Plan'
                WHEN 1 THEN 'Premium Plan'
                ELSE 'Customer Paid'
            END,
            CASE WHEN @i % 2 = 0 THEN @StaffBranch1 ELSE @StaffBranch2 END,
            @MaintenanceStatus,
            'Demo maintenance record for operational and BI testing.'
        );

        SET @i += 1;
    END;

    /* ============================================================
       10. INTERACTIONS - 15
       ============================================================ */

    SET @i = 1;

    WHILE @i <= 15
    BEGIN
        INSERT INTO dbo.InteractionLogs
        (
            BranchId,
            CustomerName,
            PhoneNumber,
            InteractionType,
            Subject,
            Notes,
            HandledBy,
            CreatedAt
        )
        VALUES
        (
            CASE WHEN @i % 2 = 0 THEN @Branch1 ELSE @Branch2 END,
            CONCAT('Interaction Customer ', @i),
            CONCAT('0977', RIGHT('000000' + CAST(@i + 800 AS VARCHAR(6)), 6)),
            CASE @i % 4
                WHEN 0 THEN 'Call'
                WHEN 1 THEN 'Walk-in'
                WHEN 2 THEN 'Email'
                ELSE 'Follow-up'
            END,
            CASE @i % 3
                WHEN 0 THEN 'Sales follow-up'
                WHEN 1 THEN 'Service follow-up'
                ELSE 'Customer inquiry'
            END,
            'Demo interaction record for customer history testing.',
            CASE WHEN @i % 2 = 0 THEN @StaffBranch1 ELSE @StaffBranch2 END,
            DATEADD(DAY, -((@i * 10) % 180), @Now)
        );

        SET @i += 1;
    END;

    /* ============================================================
       11. VERIFY EXACT COUNTS
       ============================================================ */

    DECLARE @TotalRows INT;

    SELECT @TotalRows =
        (SELECT COUNT(*) FROM dbo.SalesLeads) +
        (SELECT COUNT(*) FROM dbo.RepairTickets) +
        (SELECT COUNT(*) FROM dbo.Promotions) +
        (SELECT COUNT(*) FROM dbo.Feedback) +
        (SELECT COUNT(*) FROM dbo.Complaints) +
        (SELECT COUNT(*) FROM dbo.VehicleWarranties) +
        (SELECT COUNT(*) FROM dbo.WarrantyClaims) +
        (SELECT COUNT(*) FROM dbo.MaintenanceRecords) +
        (SELECT COUNT(*) FROM dbo.InteractionLogs);

    IF @TotalRows <> 300
        THROW 50002, 'Seed verification failed: expected exactly 300 CRM records.', 1;

    COMMIT TRANSACTION;

    SELECT
        'SalesLeads' AS TableName, COUNT(*) AS RecordCount FROM dbo.SalesLeads
    UNION ALL
    SELECT 'RepairTickets', COUNT(*) FROM dbo.RepairTickets
    UNION ALL
    SELECT 'Promotions', COUNT(*) FROM dbo.Promotions
    UNION ALL
    SELECT 'Feedback', COUNT(*) FROM dbo.Feedback
    UNION ALL
    SELECT 'Complaints', COUNT(*) FROM dbo.Complaints
    UNION ALL
    SELECT 'VehicleWarranties', COUNT(*) FROM dbo.VehicleWarranties
    UNION ALL
    SELECT 'WarrantyClaims', COUNT(*) FROM dbo.WarrantyClaims
    UNION ALL
    SELECT 'MaintenanceRecords', COUNT(*) FROM dbo.MaintenanceRecords
    UNION ALL
    SELECT 'InteractionLogs', COUNT(*) FROM dbo.InteractionLogs
    ORDER BY TableName;

    SELECT
        'TOTAL' AS TableName,
        @TotalRows AS RecordCount;

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
