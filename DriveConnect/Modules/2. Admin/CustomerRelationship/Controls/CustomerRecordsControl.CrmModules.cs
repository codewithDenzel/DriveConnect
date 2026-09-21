using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DriveConnect.domain.Entities;

namespace DriveConnect.winforms.Modules.Admin.CustomerRelationship.Controls
{
    public partial class CustomerRecordsControl
    {
        private List<Promotion> _allPromotions = new List<Promotion>();
        private List<Feedback> _allFeedback = new List<Feedback>();
        private List<Complaint> _allComplaints = new List<Complaint>();
        private List<InteractionLog> _allInteractions = new List<InteractionLog>();
        private List<VehicleWarranty> _allWarranties = new List<VehicleWarranty>();
        private List<WarrantyClaim> _allWarrantyClaims = new List<WarrantyClaim>();
        private List<MaintenanceRecord> _allMaintenance = new List<MaintenanceRecord>();

        private class CustomerHistoryProfileRow
        {
            public string Customer { get; set; } = "";
            public string Phone { get; set; } = "";
            public int SalesLeads { get; set; }
            public int Repairs { get; set; }
            public int Feedback { get; set; }
            public int Complaints { get; set; }
            public int Warranties { get; set; }
            public int Maintenance { get; set; }
        }

        private bool IsAdditionalDataTab(string mainTab)
        {
            return mainTab == "Promotions" ||
                   mainTab == "Customer History" ||
                   mainTab == "Feedback" ||
                   mainTab == "Complaints" ||
                   mainTab == "Warranty" ||
                   mainTab == "Maintenance";
        }

        private bool CanCreateCurrentTab()
        {
            if (currentMainTab == "Customer History")
                return currentSubTab == "Interaction History";

            return IsAdditionalDataTab(currentMainTab);
        }

        private void BindAdditionalGrid()
        {
            if (currentMainTab == "Promotions")
            {
                var query = _allPromotions.AsEnumerable();
                if (currentSubTab == "Active Promos") query = query.Where(x => x.Status == "Active");
                else if (currentSubTab == "Drafts") query = query.Where(x => x.Status == "Draft");

                string q = txtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(q))
                    query = query.Where(x => (x.Title ?? "").ToLower().Contains(q) || (x.Description ?? "").ToLower().Contains(q) || (x.CreatedBy ?? "").ToLower().Contains(q));

                gridView.DataSource = query.Select(x => new
                {
                    ID = x.PromotionId,
                    Title = x.Title,
                    Description = x.Description,
                    Discount = x.DiscountType == "Percentage" ? $"{x.DiscountValue:N2}%" : $"₱{x.DiscountValue:N2}",
                    StartDate = x.StartDate.ToLocalTime().ToString("MMM dd, yyyy"),
                    EndDate = x.EndDate.ToLocalTime().ToString("MMM dd, yyyy"),
                    CreatedBy = x.CreatedBy,
                    ApprovedBy = x.ApprovedBy,
                    Status = x.Status
                }).ToList();
                return;
            }

            if (currentMainTab == "Feedback")
            {
                var query = _allFeedback.AsEnumerable();
                if (currentSubTab == "New Feedback") query = query.Where(x => x.Status == "New");
                else if (currentSubTab == "Reviewed Feedback") query = query.Where(x => x.Status == "Reviewed");

                string q = txtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(q))
                    query = query.Where(x => (FullName(x.FirstName, x.MiddleName, x.LastName) ?? "").ToLower().Contains(q) || (x.PhoneNumber ?? "").Contains(q) || (x.Comment ?? "").ToLower().Contains(q));

                gridView.DataSource = query.Select(x => new
                {
                    ID = x.FeedbackId,
                    Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                    Phone = x.PhoneNumber,
                    Type = x.Type,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    HandledBy = x.HandledBy,
                    Status = x.Status,
                    Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                }).ToList();
                return;
            }

            if (currentMainTab == "Complaints")
            {
                var query = _allComplaints.AsEnumerable();
                if (currentSubTab == "New Complaints") query = query.Where(x => x.Status == "New");
                else if (currentSubTab == "Investigating") query = query.Where(x => x.Status == "Investigating");
                else if (currentSubTab == "Resolved") query = query.Where(x => x.Status == "Resolved" || x.Status == "Closed");

                string q = txtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(q))
                    query = query.Where(x => (FullName(x.FirstName, x.MiddleName, x.LastName) ?? "").ToLower().Contains(q) || (x.PhoneNumber ?? "").Contains(q) || (x.Description ?? "").ToLower().Contains(q));

                gridView.DataSource = query.Select(x => new
                {
                    ID = x.ComplaintId,
                    Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                    Phone = x.PhoneNumber,
                    Category = x.Category,
                    Priority = x.Priority,
                    Description = x.Description,
                    HandledBy = x.HandledBy,
                    Status = x.Status,
                    Resolution = x.Resolution,
                    Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                }).ToList();
                return;
            }

            if (currentMainTab == "Warranty")
            {
                if (currentSubTab == "Active Warranties")
                {
                    var query = _allWarranties.Where(x => x.Status == "Active").AsEnumerable();
                    string q = txtSearch.Text.Trim().ToLower();
                    if (!string.IsNullOrWhiteSpace(q))
                        query = query.Where(x => (FullName(x.FirstName, x.MiddleName, x.LastName) ?? "").ToLower().Contains(q) || (x.PhoneNumber ?? "").Contains(q) || (x.VehicleModel ?? "").ToLower().Contains(q));

                    gridView.DataSource = query.Select(x => new
                    {
                        ID = x.WarrantyId,
                        Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                        Phone = x.PhoneNumber,
                        Vehicle = x.VehicleModel,
                        PurchaseDate = x.PurchaseDate.ToLocalTime().ToString("MMM dd, yyyy"),
                        WarrantyStart = x.WarrantyStart.ToLocalTime().ToString("MMM dd, yyyy"),
                        WarrantyEnd = x.WarrantyEnd.ToLocalTime().ToString("MMM dd, yyyy"),
                        Coverage = x.Coverage,
                        Status = x.Status
                    }).ToList();
                }
                else
                {
                    var query = _allWarrantyClaims.AsEnumerable();
                    string q = txtSearch.Text.Trim().ToLower();
                    if (!string.IsNullOrWhiteSpace(q))
                        query = query.Where(x => (FullName(x.FirstName, x.MiddleName, x.LastName) ?? "").ToLower().Contains(q) || (x.PhoneNumber ?? "").Contains(q) || (x.VehicleModel ?? "").ToLower().Contains(q));

                    gridView.DataSource = query.Select(x => new
                    {
                        ID = x.ClaimId,
                        WarrantyID = x.WarrantyId,
                        Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                        Phone = x.PhoneNumber,
                        Vehicle = x.VehicleModel,
                        Problem = x.Problem,
                        Reported = x.DateReported.ToLocalTime().ToString("MMM dd, yyyy"),
                        HandledBy = x.HandledBy,
                        Status = x.Status,
                        Resolution = x.Resolution
                    }).ToList();
                }
                return;
            }

            if (currentMainTab == "Maintenance")
            {
                var query = _allMaintenance.AsEnumerable();
                if (currentSubTab == "Scheduled Maintenance") query = query.Where(x => x.Status == "Scheduled" || x.Status == "Rescheduled");
                else if (currentSubTab == "Completed Maintenance") query = query.Where(x => x.Status == "Completed");

                string q = txtSearch.Text.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(q))
                    query = query.Where(x => (FullName(x.FirstName, x.MiddleName, x.LastName) ?? "").ToLower().Contains(q) || (x.PhoneNumber ?? "").Contains(q) || (x.VehicleModel ?? "").ToLower().Contains(q));

                gridView.DataSource = query.Select(x => new
                {
                    ID = x.MaintenanceId,
                    Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                    Phone = x.PhoneNumber,
                    Vehicle = x.VehicleModel,
                    ServiceDate = x.ServiceDate.ToLocalTime().ToString("MMM dd, yyyy"),
                    ServiceType = x.ServiceType,
                    PlanCoverage = x.PlanCoverage,
                    AssignedStaff = x.AssignedStaff,
                    Status = x.Status,
                    Notes = x.Notes
                }).ToList();
                return;
            }

            if (currentMainTab == "Customer History")
            {
                string q = txtSearch.Text.Trim().ToLower();

                if (currentSubTab == "Customer Profile")
                {
                    var sources = new List<CustomerHistoryProfileRow>();

                    sources.AddRange(_allSales.Select(x => new CustomerHistoryProfileRow
                    {
                        Customer = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                        Phone = x.PhoneNumber ?? "",
                        SalesLeads = 1
                    }));
                    sources.AddRange(_allRepairs.Select(x => new CustomerHistoryProfileRow
                    {
                        Customer = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                        Phone = x.PhoneNumber ?? "",
                        Repairs = 1
                    }));
                    sources.AddRange(_allFeedback.Select(x => new CustomerHistoryProfileRow { Customer = FullName(x.FirstName, x.MiddleName, x.LastName) ?? "", Phone = x.PhoneNumber ?? "", Feedback = 1 }));
                    sources.AddRange(_allComplaints.Select(x => new CustomerHistoryProfileRow { Customer = FullName(x.FirstName, x.MiddleName, x.LastName) ?? "", Phone = x.PhoneNumber ?? "", Complaints = 1 }));
                    sources.AddRange(_allWarranties.Select(x => new CustomerHistoryProfileRow { Customer = FullName(x.FirstName, x.MiddleName, x.LastName) ?? "", Phone = x.PhoneNumber ?? "", Warranties = 1 }));
                    sources.AddRange(_allMaintenance.Select(x => new CustomerHistoryProfileRow { Customer = FullName(x.FirstName, x.MiddleName, x.LastName) ?? "", Phone = x.PhoneNumber ?? "", Maintenance = 1 }));
                    sources.AddRange(_allInteractions.Select(x => new CustomerHistoryProfileRow { Customer = FullName(x.FirstName, x.MiddleName, x.LastName) ?? "", Phone = x.PhoneNumber ?? "" }));

                    var grouped = sources
                        .Where(x => !string.IsNullOrWhiteSpace(x.Customer))
                        .GroupBy(x => string.IsNullOrWhiteSpace(x.Phone) ? x.Customer.ToLower() : x.Phone)
                        .Select(g => new
                        {
                            Customer = g.OrderByDescending(x => !string.IsNullOrWhiteSpace(x.Customer)).First().Customer,
                            Phone = g.First().Phone,
                            SalesLeads = g.Sum(x => x.SalesLeads),
                            Repairs = g.Sum(x => x.Repairs),
                            Feedback = g.Sum(x => x.Feedback),
                            Complaints = g.Sum(x => x.Complaints),
                            Warranties = g.Sum(x => x.Warranties),
                            Maintenance = g.Sum(x => x.Maintenance)
                        })
                        .Where(x => string.IsNullOrWhiteSpace(q) || x.Customer.ToLower().Contains(q) || x.Phone.Contains(q))
                        .ToList();

                    gridView.DataSource = grouped;
                    return;
                }

                if (currentSubTab == "Sales and Lead History")
                {
                    gridView.DataSource = _allSales.Select(x => new
                    {
                        ID = x.InquiryId,
                        Customer = $"{x.FirstName} {x.LastName}".Trim(),
                        Phone = x.PhoneNumber,
                        Model = x.CarModel,
                        Stage = x.Status,
                        Value = $"₱{x.EstimatedCost:N2}",
                        HandledBy = x.HandledBy,
                        Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                    }).ToList();
                    return;
                }

                if (currentSubTab == "Service and Repair History")
                {
                    gridView.DataSource = _allRepairs.Select(x => new
                    {
                        ID = x.TicketId,
                        Customer = $"{x.FirstName} {x.LastName}".Trim(),
                        Phone = x.PhoneNumber,
                        Vehicle = x.CarModel,
                        Issue = x.Concern,
                        Status = x.Status,
                        Pickup = x.PickupStatus,
                        HandledBy = x.HandledBy,
                        Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                    }).ToList();
                    return;
                }

                if (currentSubTab == "Interaction History")
                {
                    gridView.DataSource = _allInteractions
                        .Where(x => string.IsNullOrWhiteSpace(q) || (FullName(x.FirstName, x.MiddleName, x.LastName) ?? "").ToLower().Contains(q) || (x.PhoneNumber ?? "").Contains(q) || (x.Subject ?? "").ToLower().Contains(q))
                        .Select(x => new
                        {
                            ID = x.InteractionId,
                            Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                            Phone = x.PhoneNumber,
                            Type = x.InteractionType,
                            Subject = x.Subject,
                            Notes = x.Notes,
                            HandledBy = x.HandledBy,
                            Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                        }).ToList();
                    return;
                }

                if (currentSubTab == "Feedback History")
                {
                    gridView.DataSource = _allFeedback
                        .Select(x => new
                        {
                            ID = x.FeedbackId,
                            Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                            Phone = x.PhoneNumber,
                            Type = x.Type,
                            Rating = x.Rating,
                            Comment = x.Comment,
                            Status = x.Status,
                            Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                        }).ToList();
                    return;
                }

                if (currentSubTab == "Complaint History")
                {
                    gridView.DataSource = _allComplaints
                        .Select(x => new
                        {
                            ID = x.ComplaintId,
                            Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                            Phone = x.PhoneNumber,
                            Category = x.Category,
                            Priority = x.Priority,
                            Description = x.Description,
                            Status = x.Status,
                            Resolution = x.Resolution,
                            Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                        }).ToList();
                    return;
                }

                if (currentSubTab == "Warranty History")
                {
                    gridView.DataSource = _allWarranties
                        .Select(x => new
                        {
                            ID = x.WarrantyId,
                            Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                            Phone = x.PhoneNumber,
                            Vehicle = x.VehicleModel,
                            WarrantyStart = x.WarrantyStart.ToLocalTime().ToString("MMM dd, yyyy"),
                            WarrantyEnd = x.WarrantyEnd.ToLocalTime().ToString("MMM dd, yyyy"),
                            Coverage = x.Coverage,
                            Status = x.Status,
                            Claims = _allWarrantyClaims.Count(c => c.WarrantyId == x.WarrantyId)
                        }).ToList();
                    return;
                }

                if (currentSubTab == "Maintenance History")
                {
                    gridView.DataSource = _allMaintenance
                        .Select(x => new
                        {
                            ID = x.MaintenanceId,
                            Customer = FullName(x.FirstName, x.MiddleName, x.LastName),
                            Phone = x.PhoneNumber,
                            Vehicle = x.VehicleModel,
                            ServiceDate = x.ServiceDate.ToLocalTime().ToString("MMM dd, yyyy"),
                            ServiceType = x.ServiceType,
                            PlanCoverage = x.PlanCoverage,
                            AssignedStaff = x.AssignedStaff,
                            Status = x.Status
                        }).ToList();
                }
            }
        }

        private async Task LoadAdditionalDataAsync()
        {
            try { _allPromotions = await _apiService.GetPromotionsAsync(CurrentCompanyId); } catch { _allPromotions = new List<Promotion>(); }
            try { _allFeedback = await _apiService.GetFeedbackAsync(CurrentCompanyId); } catch { _allFeedback = new List<Feedback>(); }
            try { _allComplaints = await _apiService.GetComplaintsAsync(CurrentCompanyId); } catch { _allComplaints = new List<Complaint>(); }
            try { _allInteractions = await _apiService.GetInteractionsAsync(CurrentCompanyId); } catch { _allInteractions = new List<InteractionLog>(); }
            try { _allWarranties = await _apiService.GetWarrantiesAsync(CurrentCompanyId); } catch { _allWarranties = new List<VehicleWarranty>(); }
            try { _allWarrantyClaims = await _apiService.GetWarrantyClaimsAsync(CurrentCompanyId); } catch { _allWarrantyClaims = new List<WarrantyClaim>(); }
            try { _allMaintenance = await _apiService.GetMaintenanceAsync(CurrentCompanyId); } catch { _allMaintenance = new List<MaintenanceRecord>(); }
        }

        private async Task RefreshAdditionalDataAndGridAsync()
        {
            await LoadAdditionalDataAsync();
            if (currentMainTab == "Business Intelligence")
            {
                RefreshDashboardMetrics();
            }
            else
            {
                FilterAndBindGrid();
            }
        }

        private async Task HandleAdditionalRecordDoubleClickAsync(int id)
        {
            if (currentMainTab == "Customer History")
            {
                if (currentSubTab == "Interaction History")
                    await ShowInteractionModalAsync(_allInteractions.FirstOrDefault(x => x.InteractionId == id));
                return;
            }

            if (currentMainTab == "Promotions")
                await ShowPromotionModalAsync(_allPromotions.FirstOrDefault(x => x.PromotionId == id));
            else if (currentMainTab == "Feedback")
                await ShowFeedbackModalAsync(_allFeedback.FirstOrDefault(x => x.FeedbackId == id));
            else if (currentMainTab == "Complaints")
                await ShowComplaintModalAsync(_allComplaints.FirstOrDefault(x => x.ComplaintId == id));
            else if (currentMainTab == "Warranty")
            {
                if (currentSubTab == "Active Warranties")
                    await ShowWarrantyModalAsync(_allWarranties.FirstOrDefault(x => x.WarrantyId == id));
                else
                    await ShowWarrantyClaimModalAsync(_allWarrantyClaims.FirstOrDefault(x => x.ClaimId == id));
            }
            else if (currentMainTab == "Maintenance")
                await ShowMaintenanceModalAsync(_allMaintenance.FirstOrDefault(x => x.MaintenanceId == id));
        }

        private static string FullName(string? firstName, string? middleName, string? lastName)
        {
            return $"{firstName} {middleName} {lastName}".Replace("  ", " ").Trim();
        }

        private bool ConfirmAction(string message)
        {
            return MessageBox.Show(message, "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private async Task ShowPromotionModalAsync(Promotion? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "New Promotion" : "Edit Promotion", 650);
            int y = 70;
            TextBox title = AddFormField(f, "Promotion Title", existing?.Title, ref y);
            TextBox desc = AddFormField(f, "Description", existing?.Description, ref y);
            ComboBox discountType = AddFormCombo(f, "Discount Type", new[] { "Percentage", "Fixed Amount", "None" }, existing?.DiscountType ?? "Percentage", ref y);
            TextBox discount = AddFormField(f, "Discount Value", existing?.DiscountValue.ToString("F2") ?? "0.00", ref y);
            DateTimePicker start = AddFormDateField(f, "Start Date", existing?.StartDate ?? DateTime.Today, ref y);
            DateTimePicker end = AddFormDateField(f, "End Date", existing?.EndDate ?? DateTime.Today.AddMonths(1), ref y);
            TextBox createdBy = AddFormField(f, "Created By", existing?.CreatedBy, ref y);
            TextBox approvedBy = AddFormField(f, "Approved By", existing?.ApprovedBy, ref y);
            ComboBox status = AddFormCombo(f, "Status", new[] { "Draft", "Active", "Expired", "Archived" }, existing?.Status ?? "Draft", ref y);

            Button save = AddFormSubmitButton(f, existing == null ? "Save Promotion" : "Update Promotion", y);
            save.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(title.Text) || string.IsNullOrWhiteSpace(desc.Text) || string.IsNullOrWhiteSpace(createdBy.Text))
                {
                    MessageBox.Show("Title, Description, and Created By are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (end.Value.Date < start.Value.Date)
                {
                    MessageBox.Show("End Date cannot be before Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal.TryParse(discount.Text, out decimal discountValue);
                bool archive = existing != null && existing.Status != "Archived" && status.Text == "Archived";
                if (!ConfirmAction(existing == null ? "Confirm adding this promotion?" : archive ? "Confirm archiving this promotion?" : "Confirm updating this promotion?")) return;

                var payload = existing ?? new Promotion { CreatedAt = DateTime.UtcNow };
                payload.Title = title.Text;
                payload.Description = desc.Text;
                payload.DiscountType = discountType.Text;
                payload.DiscountValue = discountValue;
                payload.StartDate = start.Value.Date;
                payload.EndDate = end.Value.Date;
                payload.CreatedBy = createdBy.Text;
                payload.ApprovedBy = string.IsNullOrWhiteSpace(approvedBy.Text) ? null : approvedBy.Text;
                payload.Status = status.Text;

                var response = existing == null
                    ? await _apiService.CreatePromotionAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdatePromotionAsync(CurrentCompanyId, payload.PromotionId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The promotion could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                f.DialogResult = DialogResult.OK;
            };

            if (f.ShowDialog() == DialogResult.OK) await RefreshAdditionalDataAndGridAsync();
        }

        private async Task ShowFeedbackModalAsync(Feedback? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "New Feedback" : "Edit Feedback", 680);
            int y = 70;
            TextBox first = AddFormField(f, "First Name", existing?.FirstName, ref y);
            TextBox middle = AddFormField(f, "Middle Name (Optional)", existing?.MiddleName, ref y);
            TextBox last = AddFormField(f, "Last Name", existing?.LastName, ref y);
            TextBox phone = AddFormField(f, "Phone Number", existing?.PhoneNumber, ref y);
            ComboBox type = AddFormCombo(f, "Feedback Type", new[] { "Sales Experience", "Test Drive Experience", "Service Experience", "Staff Service", "General Suggestion" }, existing?.Type ?? "General Suggestion", ref y);
            ComboBox rating = AddFormCombo(f, "Rating", new[] { "1", "2", "3", "4", "5" }, (existing?.Rating ?? 5).ToString(), ref y);
            TextBox comment = AddFormField(f, "Comment", existing?.Comment, ref y);
            TextBox handledBy = AddFormField(f, "Handled By", existing?.HandledBy, ref y);
            ComboBox status = AddFormCombo(f, "Status", new[] { "New", "Reviewed" }, existing?.Status ?? "New", ref y);

            Button save = AddFormSubmitButton(f, existing == null ? "Save Feedback" : "Update Feedback", y);
            save.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(last.Text) || string.IsNullOrWhiteSpace(phone.Text) || string.IsNullOrWhiteSpace(comment.Text))
                {
                    MessageBox.Show("First Name, Last Name, Phone Number, and Comment are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!ConfirmAction(existing == null ? "Confirm adding this feedback?" : "Confirm updating this feedback?")) return;

                var payload = existing ?? new Feedback { CreatedAt = DateTime.UtcNow };
                payload.FirstName = first.Text;
                payload.MiddleName = middle.Text;
                payload.LastName = last.Text;
                payload.PhoneNumber = phone.Text;
                payload.Type = type.Text;
                payload.Rating = int.TryParse(rating.Text, out int r) ? r : 5;
                payload.Comment = comment.Text;
                payload.HandledBy = handledBy.Text;
                payload.Status = status.Text;

                var response = existing == null
                    ? await _apiService.CreateFeedbackAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdateFeedbackAsync(CurrentCompanyId, payload.FeedbackId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The feedback could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                f.DialogResult = DialogResult.OK;
            };
            if (f.ShowDialog() == DialogResult.OK) await RefreshAdditionalDataAndGridAsync();
        }

        private async Task ShowComplaintModalAsync(Complaint? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "New Complaint" : "Edit Complaint", 820);
            int y = 70;
            TextBox first = AddFormField(f, "First Name", existing?.FirstName, ref y);
            TextBox middle = AddFormField(f, "Middle Name (Optional)", existing?.MiddleName, ref y);
            TextBox last = AddFormField(f, "Last Name", existing?.LastName, ref y);
            TextBox phone = AddFormField(f, "Phone Number", existing?.PhoneNumber, ref y);
            ComboBox category = AddFormCombo(f, "Category", new[] { "Vehicle", "Service", "Staff", "Sales", "Other" }, existing?.Category ?? "Service", ref y);
            ComboBox priority = AddFormCombo(f, "Priority", new[] { "Low", "Medium", "High" }, existing?.Priority ?? "Medium", ref y);
            TextBox description = AddFormField(f, "Complaint", existing?.Description, ref y);
            TextBox handledBy = AddFormField(f, "Handled By", existing?.HandledBy, ref y);
            ComboBox status = AddFormCombo(f, "Status", new[] { "New", "Investigating", "Resolved", "Closed" }, existing?.Status ?? "New", ref y);
            TextBox resolution = AddFormField(f, "Resolution", existing?.Resolution, ref y);

            Button save = AddFormSubmitButton(f, existing == null ? "Save Complaint" : "Update Complaint", y);
            save.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(last.Text) || string.IsNullOrWhiteSpace(phone.Text) || string.IsNullOrWhiteSpace(description.Text) || string.IsNullOrWhiteSpace(handledBy.Text))
                {
                    MessageBox.Show("First Name, Last Name, Phone Number, Complaint, and Handled By are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if ((status.Text == "Resolved" || status.Text == "Closed") && string.IsNullOrWhiteSpace(resolution.Text))
                {
                    MessageBox.Show("Resolution is required when the complaint is resolved or closed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ConfirmAction(existing == null ? "Confirm adding this complaint?" : "Confirm updating this complaint?")) return;

                var payload = existing ?? new Complaint { CreatedAt = DateTime.UtcNow };
                payload.FirstName = first.Text;
                payload.MiddleName = middle.Text;
                payload.LastName = last.Text;
                payload.PhoneNumber = phone.Text;
                payload.Category = category.Text;
                payload.Priority = priority.Text;
                payload.Description = description.Text;
                payload.HandledBy = handledBy.Text;
                payload.Status = status.Text;
                payload.Resolution = string.IsNullOrWhiteSpace(resolution.Text) ? null : resolution.Text;

                var response = existing == null
                    ? await _apiService.CreateComplaintAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdateComplaintAsync(CurrentCompanyId, payload.ComplaintId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The complaint could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                f.DialogResult = DialogResult.OK;
            };
            if (f.ShowDialog() == DialogResult.OK) await RefreshAdditionalDataAndGridAsync();
        }

        private async Task ShowInteractionModalAsync(InteractionLog? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "New Interaction Log" : "Edit Interaction Log", 680);
            int y = 70;
            TextBox first = AddFormField(f, "First Name", existing?.FirstName, ref y);
            TextBox middle = AddFormField(f, "Middle Name (Optional)", existing?.MiddleName, ref y);
            TextBox last = AddFormField(f, "Last Name", existing?.LastName, ref y);
            TextBox phone = AddFormField(f, "Phone Number", existing?.PhoneNumber, ref y);
            ComboBox type = AddFormCombo(f, "Interaction Type", new[] { "Call", "Message", "Meeting", "Appointment", "Follow-up" }, existing?.InteractionType ?? "Call", ref y);
            TextBox subject = AddFormField(f, "Subject", existing?.Subject, ref y);
            TextBox notes = AddFormField(f, "Notes", existing?.Notes, ref y);
            TextBox handledBy = AddFormField(f, "Handled By", existing?.HandledBy, ref y);

            Button save = AddFormSubmitButton(f, existing == null ? "Save Interaction" : "Update Interaction", y);
            save.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(last.Text) || string.IsNullOrWhiteSpace(phone.Text) || string.IsNullOrWhiteSpace(subject.Text) || string.IsNullOrWhiteSpace(handledBy.Text))
                {
                    MessageBox.Show("First Name, Last Name, Phone Number, Subject, and Handled By are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ConfirmAction(existing == null ? "Confirm adding this interaction?" : "Confirm updating this interaction?")) return;

                var payload = existing ?? new InteractionLog { CreatedAt = DateTime.UtcNow };
                payload.FirstName = first.Text;
                payload.MiddleName = middle.Text;
                payload.LastName = last.Text;
                payload.PhoneNumber = phone.Text;
                payload.InteractionType = type.Text;
                payload.Subject = subject.Text;
                payload.Notes = notes.Text;
                payload.HandledBy = handledBy.Text;

                var response = existing == null
                    ? await _apiService.CreateInteractionAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdateInteractionAsync(CurrentCompanyId, payload.InteractionId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The interaction could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                f.DialogResult = DialogResult.OK;
            };
            if (f.ShowDialog() == DialogResult.OK) await RefreshAdditionalDataAndGridAsync();
        }

        private async Task ShowWarrantyModalAsync(VehicleWarranty? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "Register Vehicle Warranty" : "Edit Vehicle Warranty", 760);
            int y = 70;
            TextBox first = AddFormField(f, "First Name", existing?.FirstName, ref y);
            TextBox middle = AddFormField(f, "Middle Name (Optional)", existing?.MiddleName, ref y);
            TextBox last = AddFormField(f, "Last Name", existing?.LastName, ref y);
            TextBox phone = AddFormField(f, "Phone Number", existing?.PhoneNumber, ref y);
            TextBox vehicle = AddFormField(f, "Vehicle Model", existing?.VehicleModel, ref y);
            DateTimePicker purchase = AddFormDateField(f, "Purchase Date", existing?.PurchaseDate ?? DateTime.Today, ref y);
            DateTimePicker start = AddFormDateField(f, "Warranty Start", existing?.WarrantyStart ?? DateTime.Today, ref y);
            DateTimePicker end = AddFormDateField(f, "Warranty End", existing?.WarrantyEnd ?? DateTime.Today.AddYears(1), ref y);
            TextBox coverage = AddFormField(f, "Coverage", existing?.Coverage, ref y);
            ComboBox status = AddFormCombo(f, "Status", new[] { "Active", "Expired", "Cancelled" }, existing?.Status ?? "Active", ref y);

            Button save = AddFormSubmitButton(f, existing == null ? "Save Warranty" : "Update Warranty", y);
            save.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(last.Text) || string.IsNullOrWhiteSpace(phone.Text) || string.IsNullOrWhiteSpace(vehicle.Text) || string.IsNullOrWhiteSpace(coverage.Text))
                {
                    MessageBox.Show("First Name, Last Name, Phone Number, Vehicle Model, and Coverage are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (end.Value.Date < start.Value.Date)
                {
                    MessageBox.Show("Warranty End cannot be before Warranty Start.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ConfirmAction(existing == null ? "Confirm adding this warranty?" : "Confirm updating this warranty?")) return;

                var payload = existing ?? new VehicleWarranty();
                payload.FirstName = first.Text;
                payload.MiddleName = middle.Text;
                payload.LastName = last.Text;
                payload.PhoneNumber = phone.Text;
                payload.VehicleModel = vehicle.Text;
                payload.PurchaseDate = purchase.Value.Date;
                payload.WarrantyStart = start.Value.Date;
                payload.WarrantyEnd = end.Value.Date;
                payload.Coverage = coverage.Text;
                payload.Status = status.Text;

                var response = existing == null
                    ? await _apiService.CreateWarrantyAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdateWarrantyAsync(CurrentCompanyId, payload.WarrantyId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The warranty could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                f.DialogResult = DialogResult.OK;
            };
            if (f.ShowDialog() == DialogResult.OK) await RefreshAdditionalDataAndGridAsync();
        }

        private async Task ShowWarrantyClaimModalAsync(WarrantyClaim? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "New Warranty Claim" : "Edit Warranty Claim", 880);
            int y = 70;
            TextBox warrantyId = AddFormField(f, "Warranty ID", existing?.WarrantyId.ToString(), ref y);
            TextBox first = AddFormField(f, "First Name", existing?.FirstName, ref y);
            TextBox middle = AddFormField(f, "Middle Name (Optional)", existing?.MiddleName, ref y);
            TextBox last = AddFormField(f, "Last Name", existing?.LastName, ref y);
            TextBox phone = AddFormField(f, "Phone Number", existing?.PhoneNumber, ref y);
            TextBox vehicle = AddFormField(f, "Vehicle Model", existing?.VehicleModel, ref y);
            TextBox problem = AddFormField(f, "Problem", existing?.Problem, ref y);
            DateTimePicker reported = AddFormDateField(f, "Date Reported", existing?.DateReported ?? DateTime.Today, ref y);
            TextBox handledBy = AddFormField(f, "Handled By", existing?.HandledBy, ref y);
            ComboBox status = AddFormCombo(f, "Claim Status", new[] { "Pending", "Under Review", "Approved", "Rejected", "Resolved" }, existing?.Status ?? "Pending", ref y);
            TextBox resolution = AddFormField(f, "Resolution", existing?.Resolution, ref y);

            Button save = AddFormSubmitButton(f, existing == null ? "Save Claim" : "Update Claim", y);
            save.Click += async (s, e) =>
            {
                if (!int.TryParse(warrantyId.Text, out int wid) || wid <= 0)
                {
                    MessageBox.Show("Warranty ID must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(last.Text) || string.IsNullOrWhiteSpace(phone.Text) || string.IsNullOrWhiteSpace(vehicle.Text) || string.IsNullOrWhiteSpace(problem.Text) || string.IsNullOrWhiteSpace(handledBy.Text))
                {
                    MessageBox.Show("First Name, Last Name, Phone Number, Vehicle Model, Problem, and Handled By are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (status.Text == "Resolved" && string.IsNullOrWhiteSpace(resolution.Text))
                {
                    MessageBox.Show("Resolution is required when the claim is resolved.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ConfirmAction(existing == null ? "Confirm adding this warranty claim?" : "Confirm updating this warranty claim?")) return;

                var payload = existing ?? new WarrantyClaim();
                payload.WarrantyId = wid;
                payload.FirstName = first.Text;
                payload.MiddleName = middle.Text;
                payload.LastName = last.Text;
                payload.PhoneNumber = phone.Text;
                payload.VehicleModel = vehicle.Text;
                payload.Problem = problem.Text;
                payload.DateReported = reported.Value.Date;
                payload.HandledBy = handledBy.Text;
                payload.Status = status.Text;
                payload.Resolution = string.IsNullOrWhiteSpace(resolution.Text) ? null : resolution.Text;

                var response = existing == null
                    ? await _apiService.CreateWarrantyClaimAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdateWarrantyClaimAsync(CurrentCompanyId, payload.ClaimId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The warranty claim could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                f.DialogResult = DialogResult.OK;
            };
            if (f.ShowDialog() == DialogResult.OK) await RefreshAdditionalDataAndGridAsync();
        }

        private async Task ShowMaintenanceModalAsync(MaintenanceRecord? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "New Maintenance Record" : "Edit Maintenance Record", 860);
            int y = 70;
            TextBox first = AddFormField(f, "First Name", existing?.FirstName, ref y);
            TextBox middle = AddFormField(f, "Middle Name (Optional)", existing?.MiddleName, ref y);
            TextBox last = AddFormField(f, "Last Name", existing?.LastName, ref y);
            TextBox phone = AddFormField(f, "Phone Number", existing?.PhoneNumber, ref y);
            TextBox vehicle = AddFormField(f, "Vehicle Model", existing?.VehicleModel, ref y);
            DateTimePicker serviceDate = AddFormDateField(f, "Service Date", existing?.ServiceDate ?? DateTime.Today, ref y);
            ComboBox serviceType = AddFormCombo(f, "Service Type", new[] { "Free Maintenance", "Preventive Maintenance", "General Service" }, existing?.ServiceType ?? "Free Maintenance", ref y);
            ComboBox planCoverage = AddFormCombo(f, "Plan Coverage", new[] { "1-Year Free Maintenance", "Customer Paid", "Warranty Related" }, existing?.PlanCoverage ?? "1-Year Free Maintenance", ref y);
            TextBox staff = AddFormField(f, "Assigned Staff", existing?.AssignedStaff, ref y);
            ComboBox status = AddFormCombo(f, "Status", new[] { "Scheduled", "Rescheduled", "Completed" }, existing?.Status ?? "Scheduled", ref y);
            TextBox notes = AddFormField(f, "Notes", existing?.Notes, ref y);

            Button save = AddFormSubmitButton(f, existing == null ? "Save Maintenance" : "Update Maintenance", y);
            save.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(last.Text) || string.IsNullOrWhiteSpace(phone.Text) || string.IsNullOrWhiteSpace(vehicle.Text) || string.IsNullOrWhiteSpace(staff.Text))
                {
                    MessageBox.Show("First Name, Last Name, Phone Number, Vehicle Model, and Assigned Staff are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ConfirmAction(existing == null ? "Confirm adding this maintenance record?" : "Confirm updating this maintenance record?")) return;

                var payload = existing ?? new MaintenanceRecord();
                payload.FirstName = first.Text;
                payload.MiddleName = middle.Text;
                payload.LastName = last.Text;
                payload.PhoneNumber = phone.Text;
                payload.VehicleModel = vehicle.Text;
                payload.ServiceDate = serviceDate.Value.Date;
                payload.ServiceType = serviceType.Text;
                payload.PlanCoverage = planCoverage.Text;
                payload.AssignedStaff = staff.Text;
                payload.Status = status.Text;
                payload.Notes = notes.Text;

                var response = existing == null
                    ? await _apiService.CreateMaintenanceAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdateMaintenanceAsync(CurrentCompanyId, payload.MaintenanceId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The maintenance record could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                f.DialogResult = DialogResult.OK;
            };
            if (f.ShowDialog() == DialogResult.OK) await RefreshAdditionalDataAndGridAsync();
        }

        private DateTimePicker AddFormDateField(Form parent, string labelText, DateTime value, ref int yPos)
        {
            Label l = new Label
            {
                Text = labelText,
                Location = new Point(30, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Color.FromArgb(75, 85, 99)
            };

            DateTimePicker picker = new DateTimePicker
            {
                Value = value == default ? DateTime.Today : value,
                Location = new Point(30, yPos + 22),
                Width = 370,
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short
            };

            parent.Controls.Add(l);
            parent.Controls.Add(picker);
            yPos += 60;
            return picker;
        }
    }
}