using System;
using System.Collections.Generic;
using System.Linq;
using DriveConnect.domain.Entities;
using DriveConnect.winforms.Report;

namespace DriveConnect.winforms.Modules.Admin.CustomerRelationship.Controls;

public partial class CustomerRecordsControl
{
    private sealed record ReportBranchChoice(int? Id, string Name)
    {
        public override string ToString() => Name;
    }

    private ManagementReportData BuildManagementReport(
        string reportType,
        DateTime from,
        DateTime to,
        int? branchId)
    {
        string period = $"{from:MMM dd, yyyy} - {to:MMM dd, yyyy}";
        string branchName = branchId.HasValue
            ? _allBranches.FirstOrDefault(x => x.BranchId == branchId)?.BranchName ?? "Selected Branch"
            : "All Branches";

        var sales = _allSales
            .Where(x => x.CreatedAt.ToLocalTime().Date >= from.Date &&
                        x.CreatedAt.ToLocalTime().Date <= to.Date &&
                        x.Status != "Archived");
        var repairs = _allRepairs
            .Where(x => x.CreatedAt.ToLocalTime().Date >= from.Date &&
                        x.CreatedAt.ToLocalTime().Date <= to.Date &&
                        x.Status != "Archived");
        var feedback = _allFeedback
            .Where(x => x.CreatedAt.ToLocalTime().Date >= from.Date &&
                        x.CreatedAt.ToLocalTime().Date <= to.Date);
        var complaints = _allComplaints
            .Where(x => x.CreatedAt.ToLocalTime().Date >= from.Date &&
                        x.CreatedAt.ToLocalTime().Date <= to.Date);
        var warranties = _allWarranties
            .Where(x => x.WarrantyStart.ToLocalTime().Date >= from.Date &&
                        x.WarrantyStart.ToLocalTime().Date <= to.Date);
        var claims = _allWarrantyClaims
            .Where(x => x.DateReported.ToLocalTime().Date >= from.Date &&
                        x.DateReported.ToLocalTime().Date <= to.Date);
        var maintenance = _allMaintenance
            .Where(x => x.ServiceDate.ToLocalTime().Date >= from.Date &&
                        x.ServiceDate.ToLocalTime().Date <= to.Date);

        if (branchId.HasValue)
        {
            sales = sales.Where(x => x.BranchId == branchId);
            repairs = repairs.Where(x => x.BranchId == branchId);
            feedback = feedback.Where(x => x.BranchId == branchId);
            complaints = complaints.Where(x => x.BranchId == branchId);
            warranties = warranties.Where(x => x.BranchId == branchId);
            claims = claims.Where(x => x.BranchId == branchId);
            maintenance = maintenance.Where(x => x.BranchId == branchId);
        }

        return reportType switch
        {
            "Sales Report" => BuildSalesReport(sales.ToList(), period, branchName),
            "Lead Report" => BuildLeadReport(sales.ToList(), period, branchName),
            "Staff Performance" => BuildStaffReport(sales.ToList(), period, branchName),
            "Repair Report" => BuildRepairReport(repairs.ToList(), period, branchName),
            "Feedback Report" => BuildFeedbackReport(feedback.ToList(), period, branchName),
            "Complaint Report" => BuildComplaintReport(complaints.ToList(), period, branchName),
            "Warranty Report" => BuildWarrantyReport(warranties.ToList(), claims.ToList(), period, branchName),
            "Maintenance Report" => BuildMaintenanceReport(maintenance.ToList(), period, branchName),
            "Archived Sales" => BuildArchivedSalesReport(
                _allSales.Where(x => x.Status == "Archived" &&
                    x.CompletedAt.HasValue &&
                    x.CompletedAt.Value.ToLocalTime().Date >= from.Date &&
                    x.CompletedAt.Value.ToLocalTime().Date <= to.Date &&
                    (!branchId.HasValue || x.BranchId == branchId)).ToList(),
                period, branchName),
            _ => BuildArchivedRepairReport(
                _allRepairs.Where(x => x.Status == "Archived" &&
                    x.CompletedAt.HasValue &&
                    x.CompletedAt.Value.ToLocalTime().Date >= from.Date &&
                    x.CompletedAt.Value.ToLocalTime().Date <= to.Date &&
                    (!branchId.HasValue || x.BranchId == branchId)).ToList(),
                period, branchName)
        };
    }

    private ManagementReportData BuildSalesReport(List<SalesLead> rows, string period, string branch)
    {
        var active = rows.Where(x => x.Status != "Closed Won" && x.Status != "Closed Lost").ToList();
        var won = rows.Where(x => x.Status == "Closed Won").ToList();
        var lost = rows.Where(x => x.Status == "Closed Lost").ToList();
        decimal pipeline = active.Sum(x => x.EstimatedCost);
        decimal wonValue = won.Sum(x => x.EstimatedCost);
        decimal conversion = won.Count + lost.Count == 0 ? 0 : (decimal)won.Count / (won.Count + lost.Count) * 100;

        var stage = new List<KeyValuePair<string, decimal>>
        {
            new("New Inquiry", rows.Count(x => x.Status == "New Inquiry")),
            new("Test Drive", rows.Count(x => x.Status == "Test Drive Scheduled")),
            new("Negotiation", rows.Count(x => x.Status == "Negotiation")),
            new("Closed Won", won.Count)
        };

        var branchComparison = BuildBranchSalesComparison();
        var monthly = BuildMonthlySales(rows);

        var findings = new List<string>();
        if (branchComparison.Count > 0)
        {
            var best = branchComparison.OrderByDescending(x => x.Value).First();
            findings.Add($"{best.Key} has the highest closed sales value at ₱{best.Value:N2}.");
        }
        if (active.Count > 0)
            findings.Add($"{active.Count:N0} opportunities remain active; ₱{pipeline:N2} is still in the open pipeline.");
        if (won.Count + lost.Count > 0)
            findings.Add($"Current closed-deal conversion is {conversion:N1}%.");
        if (findings.Count == 0)
            findings.Add("There is not enough closed-deal activity in this period to determine a clear sales trend.");

        return new ManagementReportData
        {
            Title = "Sales Performance Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"This report covers {rows.Count:N0} sales records. The current open pipeline is worth ₱{pipeline:N2}, while closed-won sales total ₱{wonValue:N2}.",
            Metrics = new()
            {
                new("Total Leads", rows.Count.ToString("N0"), "Sales records in period"),
                new("Closed Won", won.Count.ToString("N0"), "Successful deals"),
                new("Conversion", $"{conversion:N1}%", "Won ÷ closed decisions"),
                new("Open Pipeline", $"₱{pipeline:N2}", "Active opportunity value"),
                new("Closed Sales", $"₱{wonValue:N2}", "Closed-won value")
            },
            Charts = new()
            {
                new() { Title = "Sales Pipeline Funnel", Subtitle = "Movement through major sales stages", Type = ManagementReportChartType.Funnel, Data = stage },
                new() { Title = "Sales Growth", Subtitle = "Closed-won sales value by month", Type = ManagementReportChartType.Line, Currency = true, Data = monthly },
                new() { Title = "Branch Sales Comparison", Subtitle = "Closed-won sales value by branch", Type = ManagementReportChartType.HorizontalBar, Currency = true, Data = branchComparison }
            },
            Findings = findings,
            DetailHeaders = new() { "Customer", "Model", "Stage", "Deal Value", "Handled By", "Date", "Branch" },
            DetailRows = rows
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new List<string>
                {
                    $"{x.FirstName} {x.LastName}".Trim(),
                    x.CarModel ?? "-",
                    x.Status ?? "-",
                    $"₱{x.EstimatedCost:N2}",
                    x.HandledBy ?? "-",
                    x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy"),
                    GetBranchName(x.BranchId)
                }).ToList()
        };
    }

    private ManagementReportData BuildLeadReport(List<SalesLead> rows, string period, string branch)
    {
        var stage = rows
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New Inquiry" : x.Status)
            .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        var branchLeads = BuildBranchLeadComparison();
        int active = rows.Count(x => x.Status != "Closed Won" && x.Status != "Closed Lost");
        int won = rows.Count(x => x.Status == "Closed Won");
        int lost = rows.Count(x => x.Status == "Closed Lost");
        decimal conversion = won + lost == 0 ? 0 : (decimal)won / (won + lost) * 100;

        return new ManagementReportData
        {
            Title = "Lead Status Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"The period contains {rows.Count:N0} leads. {active:N0} remain active and the current closed-decision conversion is {conversion:N1}%.",
            Metrics = new()
            {
                new("Total Leads", rows.Count.ToString("N0"), "Leads created in period"),
                new("Active Leads", active.ToString("N0"), "Not yet closed"),
                new("Closed Won", won.ToString("N0"), "Successful outcomes"),
                new("Closed Lost", lost.ToString("N0"), "Unsuccessful outcomes"),
                new("Conversion", $"{conversion:N1}%", "Won ÷ closed decisions")
            },
            Charts = new()
            {
                new() { Title = "Lead Status Distribution", Subtitle = "Current lead volume by stage", Type = ManagementReportChartType.Bar, Data = stage },
                new() { Title = "Lead Volume by Branch", Subtitle = "Lead count across active branches", Type = ManagementReportChartType.HorizontalBar, Data = branchLeads }
            },
            Findings = BuildLeadFindings(rows, branchLeads),
            DetailHeaders = new() { "Customer", "Model", "Stage", "Handled By", "Date", "Branch" },
            DetailRows = rows
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new List<string>
                {
                    $"{x.FirstName} {x.LastName}".Trim(),
                    x.CarModel ?? "-",
                    x.Status ?? "-",
                    x.HandledBy ?? "-",
                    x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy"),
                    GetBranchName(x.BranchId)
                }).ToList()
        };
    }

    private ManagementReportData BuildStaffReport(List<SalesLead> rows, string period, string branch)
    {
        var staff = rows
            .Where(x => !string.IsNullOrWhiteSpace(x.HandledBy))
            .GroupBy(x => x.HandledBy!)
            .Select(g => new
            {
                Name = g.Key,
                Leads = g.Count(),
                Won = g.Count(x => x.Status == "Closed Won"),
                Lost = g.Count(x => x.Status == "Closed Lost"),
                Value = g.Where(x => x.Status == "Closed Won").Sum(x => x.EstimatedCost)
            })
            .OrderByDescending(x => x.Won)
            .ThenByDescending(x => x.Value)
            .ToList();

        var chart = staff.Select(x => new KeyValuePair<string, decimal>(x.Name, x.Won)).ToList();
        string top = staff.FirstOrDefault()?.Name ?? "None";

        return new ManagementReportData
        {
            Title = "Staff Performance Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"{staff.Count:N0} staff members handled sales activity in this period. {top} currently leads the report by closed-won deals.",
            Metrics = new()
            {
                new("Staff Count", staff.Count.ToString("N0"), "Staff with handled sales"),
                new("Leads", rows.Count.ToString("N0"), "Sales activity handled"),
                new("Closed Won", rows.Count(x => x.Status == "Closed Won").ToString("N0"), "Successful deals"),
                new("Closed Lost", rows.Count(x => x.Status == "Closed Lost").ToString("N0"), "Lost deals"),
                new("Won Value", $"₱{rows.Where(x => x.Status == "Closed Won").Sum(x => x.EstimatedCost):N2}", "Closed-won sales value")
            },
            Charts = new()
            {
                new() { Title = "Closed-Won Deals by Staff", Subtitle = "Ranking by successful sales", Type = ManagementReportChartType.HorizontalBar, Data = chart }
            },
            Findings = new()
            {
                staff.Count == 0 ? "No staff-handled sales were found in this period." : $"{top} is the top performer by closed-won deals.",
                staff.Count > 1 ? $"{staff.OrderByDescending(x => x.Lost).First().Name} has the highest number of closed-lost deals; this may require follow-up review." : "More staff activity is needed before a broader comparison can be made."
            },
            DetailHeaders = new() { "Staff", "Leads", "Won", "Lost", "Closed Value" },
            DetailRows = staff.Select(x => new List<string>
            {
                x.Name,
                x.Leads.ToString("N0"),
                x.Won.ToString("N0"),
                x.Lost.ToString("N0"),
                $"₱{x.Value:N2}"
            }).ToList()
        };
    }

    private ManagementReportData BuildRepairReport(List<RepairTicket> rows, string period, string branch)
    {
        var statuses = rows.GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New Diagnose" : x.Status)
            .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count())).ToList();

        int open = rows.Count(x => x.Status != "Repaired" && x.PickupStatus != "Picked Up");
        int repaired = rows.Count(x => x.Status == "Repaired");
        int pickedUp = rows.Count(x => x.PickupStatus == "Picked Up");
        decimal estimated = rows.Sum(x => x.EstimatedCost);

        return new ManagementReportData
        {
            Title = "Service and Repair Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"{rows.Count:N0} repair tickets were recorded. {open:N0} remain in active service flow and the estimated repair value is ₱{estimated:N2}.",
            Metrics = new()
            {
                new("Repair Tickets", rows.Count.ToString("N0"), "Tickets in period"),
                new("Active Repairs", open.ToString("N0"), "Not yet picked up"),
                new("Repaired", repaired.ToString("N0"), "Repair work completed"),
                new("Picked Up", pickedUp.ToString("N0"), "Customer pickup completed"),
                new("Estimated Cost", $"₱{estimated:N2}", "Estimated repair value")
            },
            Charts = new()
            {
                new() { Title = "Repair Status", Subtitle = "Current repair workload by status", Type = ManagementReportChartType.Pie, Data = statuses },
                new() { Title = "Repairs by Branch", Subtitle = "Repair volume across branches", Type = ManagementReportChartType.HorizontalBar, Data = BuildBranchRepairComparison() }
            },
            Findings = new()
            {
                open > 0 ? $"{open:N0} repair tickets are still active and may require operational follow-up." : "No active repair backlog remains in this period.",
                rows.Count > 0 ? $"{rows.GroupBy(x => x.Status).OrderByDescending(g => g.Count()).First().Key ?? "Unknown"} is the most common repair status." : "No repair activity was recorded."
            },
            DetailHeaders = new() { "Customer", "Vehicle", "Issue", "Status", "Pickup", "Handled By", "Date" },
            DetailRows = rows.OrderByDescending(x => x.CreatedAt).Select(x => new List<string>
            {
                $"{x.FirstName} {x.LastName}".Trim(),
                x.CarModel ?? "-",
                x.Concern ?? "-",
                x.Status ?? "-",
                x.PickupStatus ?? "-",
                x.HandledBy ?? "-",
                x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
            }).ToList()
        };
    }

    private ManagementReportData BuildFeedbackReport(List<Feedback> rows, string period, string branch)
    {
        decimal average = rows.Count == 0 ? 0 : rows.Average(x => x.Rating);
        var ratings = rows.GroupBy(x => x.Rating).OrderBy(g => g.Key)
            .Select(g => new KeyValuePair<string, decimal>($"{g.Key} Star", g.Count())).ToList();

        return new ManagementReportData
        {
            Title = "Customer Feedback Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"The period contains {rows.Count:N0} feedback records with an average rating of {average:N1}/5. The report highlights customer sentiment and areas requiring attention.",
            Metrics = new()
            {
                new("Feedback", rows.Count.ToString("N0"), "Customer responses"),
                new("Average Rating", $"{average:N1}/5", "Overall rating"),
                new("5-Star", rows.Count(x => x.Rating == 5).ToString("N0"), "Highest rating"),
                new("New", rows.Count(x => x.Status == "New").ToString("N0"), "Not yet reviewed"),
                new("Reviewed", rows.Count(x => x.Status == "Reviewed").ToString("N0"), "Reviewed responses")
            },
            Charts = new()
            {
                new() { Title = "Feedback Rating Distribution", Subtitle = "Customer rating volume", Type = ManagementReportChartType.Bar, Data = ratings },
                new() { Title = "Feedback by Branch", Subtitle = "Feedback volume across branches", Type = ManagementReportChartType.HorizontalBar, Data = BuildBranchFeedbackComparison() }
            },
            Findings = new()
            {
                rows.Count == 0 ? "No customer feedback was recorded in this period." : $"Average customer rating is {average:N1}/5.",
                rows.Count(x => x.Status == "New") > 0 ? $"{rows.Count(x => x.Status == "New"):N0} feedback records are still marked New and may need review." : "All feedback in this period has been reviewed."
            },
            DetailHeaders = new() { "Customer", "Type", "Rating", "Status", "Handled By", "Date" },
            DetailRows = rows.OrderByDescending(x => x.CreatedAt).Select(x => new List<string>
            {
                x.CustomerName ?? "-",
                x.Type ?? "-",
                x.Rating.ToString(),
                x.Status ?? "-",
                x.HandledBy ?? "-",
                x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
            }).ToList()
        };
    }

    private ManagementReportData BuildComplaintReport(List<Complaint> rows, string period, string branch)
    {
        int open = rows.Count(x => x.Status != "Resolved" && x.Status != "Closed");
        int high = rows.Count(x => string.Equals(x.Priority, "High", StringComparison.OrdinalIgnoreCase));
        var status = rows.GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New" : x.Status)
            .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count())).ToList();

        return new ManagementReportData
        {
            Title = "Complaint Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"{rows.Count:N0} complaints were recorded. {open:N0} remain open and {high:N0} are marked High priority.",
            Metrics = new()
            {
                new("Complaints", rows.Count.ToString("N0"), "Customer complaints"),
                new("Open", open.ToString("N0"), "Not resolved or closed"),
                new("High Priority", high.ToString("N0"), "High-priority cases"),
                new("Resolved", rows.Count(x => x.Status == "Resolved").ToString("N0"), "Resolved complaints"),
                new("Closed", rows.Count(x => x.Status == "Closed").ToString("N0"), "Closed complaints")
            },
            Charts = new()
            {
                new() { Title = "Complaint Status", Subtitle = "Open vs resolved workload", Type = ManagementReportChartType.Pie, Data = status },
                new() { Title = "Complaints by Branch", Subtitle = "Complaint volume across branches", Type = ManagementReportChartType.HorizontalBar, Data = BuildBranchComplaintComparison() }
            },
            Findings = new()
            {
                open > 0 ? $"{open:N0} complaints remain open and may require management follow-up." : "No open complaints remain in this period.",
                high > 0 ? $"{high:N0} complaints are High priority." : "No High-priority complaints were recorded."
            },
            DetailHeaders = new() { "Customer", "Category", "Priority", "Status", "Handled By", "Date" },
            DetailRows = rows.OrderByDescending(x => x.CreatedAt).Select(x => new List<string>
            {
                x.CustomerName ?? "-",
                x.Category ?? "-",
                x.Priority ?? "-",
                x.Status ?? "-",
                x.HandledBy ?? "-",
                x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
            }).ToList()
        };
    }

    private ManagementReportData BuildWarrantyReport(List<VehicleWarranty> rows, List<WarrantyClaim> claims, string period, string branch)
    {
        var status = rows.GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "Unknown" : x.Status)
            .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count())).ToList();
        int active = rows.Count(x => x.Status == "Active");

        return new ManagementReportData
        {
            Title = "Warranty Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"{rows.Count:N0} warranties fall within the selected period. {active:N0} are currently Active, with {claims.Count:N0} warranty claims reported in the same period.",
            Metrics = new()
            {
                new("Warranties", rows.Count.ToString("N0"), "Warranty records"),
                new("Active", active.ToString("N0"), "Currently active"),
                new("Expired", rows.Count(x => x.Status == "Expired").ToString("N0"), "Expired warranties"),
                new("Cancelled", rows.Count(x => x.Status == "Cancelled").ToString("N0"), "Cancelled warranties"),
                new("Claims", claims.Count.ToString("N0"), "Claims reported")
            },
            Charts = new()
            {
                new() { Title = "Warranty Status", Subtitle = "Warranty distribution by status", Type = ManagementReportChartType.Pie, Data = status },
                new() { Title = "Warranties by Branch", Subtitle = "Warranty volume across branches", Type = ManagementReportChartType.HorizontalBar, Data = BuildBranchWarrantyComparison() }
            },
            Findings = new()
            {
                active > 0 ? $"{active:N0} warranties are currently Active and require ongoing after-sales support." : "No Active warranties were recorded for this period.",
                claims.Count > 0 ? $"{claims.Count:N0} warranty claims were reported and should be monitored with service teams." : "No warranty claims were reported in this period."
            },
            DetailHeaders = new() { "Customer", "Vehicle", "Start", "End", "Status", "Coverage" },
            DetailRows = rows.OrderByDescending(x => x.WarrantyStart).Select(x => new List<string>
            {
                x.CustomerName ?? "-",
                x.VehicleModel ?? "-",
                x.WarrantyStart.ToLocalTime().ToString("MMM dd, yyyy"),
                x.WarrantyEnd.ToLocalTime().ToString("MMM dd, yyyy"),
                x.Status ?? "-",
                x.Coverage ?? "-"
            }).ToList()
        };
    }

    private ManagementReportData BuildMaintenanceReport(List<MaintenanceRecord> rows, string period, string branch)
    {
        var status = rows.GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "Scheduled" : x.Status)
            .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count())).ToList();
        int scheduled = rows.Count(x => x.Status == "Scheduled" || x.Status == "Rescheduled");
        int completed = rows.Count(x => x.Status == "Completed");
        decimal completionRate = rows.Count == 0 ? 0 : (decimal)completed / rows.Count * 100;

        return new ManagementReportData
        {
            Title = "Maintenance Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"{rows.Count:N0} maintenance records were scheduled. {completed:N0} are Completed, giving a completion rate of {completionRate:N1}%.",
            Metrics = new()
            {
                new("Maintenance", rows.Count.ToString("N0"), "Maintenance records"),
                new("Scheduled", scheduled.ToString("N0"), "Scheduled or rescheduled"),
                new("Completed", completed.ToString("N0"), "Completed services"),
                new("Completion", $"{completionRate:N1}%", "Completed ÷ total"),
                new("Free Maintenance", rows.Count(x => x.ServiceType == "Free Maintenance").ToString("N0"), "Free maintenance services")
            },
            Charts = new()
            {
                new() { Title = "Maintenance Status", Subtitle = "Maintenance workload by status", Type = ManagementReportChartType.Bar, Data = status },
                new() { Title = "Maintenance by Branch", Subtitle = "Maintenance activity across branches", Type = ManagementReportChartType.HorizontalBar, Data = BuildBranchMaintenanceComparison() }
            },
            Findings = new()
            {
                scheduled > 0 ? $"{scheduled:N0} maintenance records are still scheduled or rescheduled." : "No scheduled maintenance backlog remains.",
                completionRate < 70 && rows.Count > 0 ? "Completion rate is below 70%; management may need to review scheduling or service capacity." : "Completion rate is currently at or above 70%."
            },
            DetailHeaders = new() { "Customer", "Vehicle", "Service Type", "Plan", "Assigned Staff", "Status", "Date" },
            DetailRows = rows.OrderByDescending(x => x.ServiceDate).Select(x => new List<string>
            {
                x.CustomerName ?? "-",
                x.VehicleModel ?? "-",
                x.ServiceType ?? "-",
                x.PlanCoverage ?? "-",
                x.AssignedStaff ?? "-",
                x.Status ?? "-",
                x.ServiceDate.ToLocalTime().ToString("MMM dd, yyyy")
            }).ToList()
        };
    }

    private ManagementReportData BuildArchivedSalesReport(List<SalesLead> rows, string period, string branch)
    {
        decimal value = rows.Sum(x => x.EstimatedCost);
        return new ManagementReportData
        {
            Title = "Archived Sales Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"{rows.Count:N0} archived sales records were completed in the selected period, representing ₱{value:N2} in recorded deal value.",
            Metrics = new()
            {
                new("Archived Sales", rows.Count.ToString("N0"), "Completed and archived"),
                new("Archived Value", $"₱{value:N2}", "Recorded deal value"),
                new("Avg Deal", rows.Count == 0 ? "₱0.00" : $"₱{rows.Average(x => x.EstimatedCost):N2}", "Average archived deal"),
                new("Models", rows.Select(x => x.CarModel).Distinct().Count().ToString("N0"), "Distinct models"),
                new("Staff", rows.Where(x => !string.IsNullOrWhiteSpace(x.HandledBy)).Select(x => x.HandledBy).Distinct().Count().ToString("N0"), "Staff involved")
            },
            Charts = new()
            {
                new() { Title = "Archived Sales by Branch", Subtitle = "Completed sales value by branch", Type = ManagementReportChartType.HorizontalBar, Currency = true, Data = BuildBranchArchivedSalesComparison() }
            },
            Findings = new()
            {
                rows.Count > 0 ? $"Archived sales in this period total ₱{value:N2}." : "No archived sales were found in this period."
            },
            DetailHeaders = new() { "Customer", "Model", "Deal Value", "Archived On", "Handled By", "Branch" },
            DetailRows = rows.OrderByDescending(x => x.CompletedAt).Select(x => new List<string>
            {
                $"{x.FirstName} {x.LastName}".Trim(),
                x.CarModel ?? "-",
                $"₱{x.EstimatedCost:N2}",
                x.CompletedAt?.ToLocalTime().ToString("MMM dd, yyyy") ?? "-",
                x.HandledBy ?? "-",
                GetBranchName(x.BranchId)
            }).ToList()
        };
    }

    private ManagementReportData BuildArchivedRepairReport(List<RepairTicket> rows, string period, string branch)
    {
        decimal value = rows.Sum(x => x.EstimatedCost);
        return new ManagementReportData
        {
            Title = "Archived Repairs Report",
            Period = period,
            Branch = branch,
            ExecutiveSummary = $"{rows.Count:N0} archived repair records were completed in the selected period, with an estimated value of ₱{value:N2}.",
            Metrics = new()
            {
                new("Archived Repairs", rows.Count.ToString("N0"), "Completed and archived"),
                new("Estimated Value", $"₱{value:N2}", "Archived repair estimate"),
                new("Avg Repair", rows.Count == 0 ? "₱0.00" : $"₱{rows.Average(x => x.EstimatedCost):N2}", "Average repair estimate"),
                new("Models", rows.Select(x => x.CarModel).Distinct().Count().ToString("N0"), "Distinct vehicles"),
                new("Staff", rows.Where(x => !string.IsNullOrWhiteSpace(x.HandledBy)).Select(x => x.HandledBy).Distinct().Count().ToString("N0"), "Staff involved")
            },
            Charts = new()
            {
                new() { Title = "Archived Repairs by Branch", Subtitle = "Estimated repair value by branch", Type = ManagementReportChartType.HorizontalBar, Currency = true, Data = BuildBranchArchivedRepairComparison() }
            },
            Findings = new()
            {
                rows.Count > 0 ? $"Archived repairs in this period have an estimated value of ₱{value:N2}." : "No archived repairs were found in this period."
            },
            DetailHeaders = new() { "Customer", "Vehicle", "Issue", "Estimate", "Archived On", "Handled By", "Branch" },
            DetailRows = rows.OrderByDescending(x => x.CompletedAt).Select(x => new List<string>
            {
                $"{x.FirstName} {x.LastName}".Trim(),
                x.CarModel ?? "-",
                x.Concern ?? "-",
                $"₱{x.EstimatedCost:N2}",
                x.CompletedAt?.ToLocalTime().ToString("MMM dd, yyyy") ?? "-",
                x.HandledBy ?? "-",
                GetBranchName(x.BranchId)
            }).ToList()
        };
    }

    private List<KeyValuePair<string, decimal>> BuildMonthlySales(IEnumerable<SalesLead> rows)
    {
        var monthStart = DateTime.Today.AddMonths(-5);
        monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
        var result = new List<KeyValuePair<string, decimal>>();
        for (int i = 0; i < 6; i++)
        {
            var month = monthStart.AddMonths(i);
            decimal value = rows.Where(x =>
                x.Status == "Closed Won" &&
                x.CreatedAt.ToLocalTime().Year == month.Year &&
                x.CreatedAt.ToLocalTime().Month == month.Month)
                .Sum(x => x.EstimatedCost);
            result.Add(new(month.ToString("MMM"), value));
        }
        return result;
    }

    private List<KeyValuePair<string, decimal>> BuildBranchSalesComparison()
    {
        return _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(
                b.BranchName,
                _allSales.Where(x => x.BranchId == b.BranchId && x.Status == "Closed Won").Sum(x => x.EstimatedCost)))
            .ToList();
    }

    private List<KeyValuePair<string, decimal>> BuildBranchLeadComparison()
        => _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(b.BranchName, _allSales.Count(x => x.BranchId == b.BranchId && x.Status != "Archived")))
            .ToList();

    private List<KeyValuePair<string, decimal>> BuildBranchRepairComparison()
        => _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(b.BranchName, _allRepairs.Count(x => x.BranchId == b.BranchId && x.Status != "Archived")))
            .ToList();

    private List<KeyValuePair<string, decimal>> BuildBranchFeedbackComparison()
        => _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(b.BranchName, _allFeedback.Count(x => x.BranchId == b.BranchId)))
            .ToList();

    private List<KeyValuePair<string, decimal>> BuildBranchComplaintComparison()
        => _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(b.BranchName, _allComplaints.Count(x => x.BranchId == b.BranchId)))
            .ToList();

    private List<KeyValuePair<string, decimal>> BuildBranchWarrantyComparison()
        => _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(b.BranchName, _allWarranties.Count(x => x.BranchId == b.BranchId)))
            .ToList();

    private List<KeyValuePair<string, decimal>> BuildBranchMaintenanceComparison()
        => _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(b.BranchName, _allMaintenance.Count(x => x.BranchId == b.BranchId)))
            .ToList();

    private List<KeyValuePair<string, decimal>> BuildBranchArchivedSalesComparison()
        => _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(
                b.BranchName,
                _allSales.Where(x => x.BranchId == b.BranchId && x.Status == "Archived").Sum(x => x.EstimatedCost)))
            .ToList();

    private List<KeyValuePair<string, decimal>> BuildBranchArchivedRepairComparison()
        => _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName)
            .Select(b => new KeyValuePair<string, decimal>(
                b.BranchName,
                _allRepairs.Where(x => x.BranchId == b.BranchId && x.Status == "Archived").Sum(x => x.EstimatedCost)))
            .ToList();

    private List<string> BuildLeadFindings(List<SalesLead> rows, List<KeyValuePair<string, decimal>> branchLeads)
    {
        var findings = new List<string>();
        if (branchLeads.Count > 0)
        {
            var top = branchLeads.OrderByDescending(x => x.Value).First();
            findings.Add($"{top.Key} currently holds the highest lead volume at {top.Value:N0}.");
        }
        var stage = rows.GroupBy(x => x.Status ?? "New Inquiry").OrderByDescending(g => g.Count()).FirstOrDefault();
        if (stage != null)
            findings.Add($"{stage.Key} is the largest lead stage with {stage.Count():N0} records.");
        findings.Add(rows.Count == 0
            ? "No leads were recorded in this period."
            : "Use the stage distribution to decide where follow-up effort should be concentrated.");
        return findings;
    }

    private string GetBranchName(int? branchId)
        => branchId.HasValue
            ? _allBranches.FirstOrDefault(x => x.BranchId == branchId)?.BranchName ?? $"Branch {branchId}"
            : "Unassigned";
}
