using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DriveConnect.domain.Entities;
using DriveConnect.winforms.Services;

namespace DriveConnect.winforms.Modules.Admin.CustomerRelationship.Controls
{
    public partial class CustomerRecordsControl : UserControl
    {
        // --- CORE UI PANELS ---
        private Panel sidebarPanel = new Panel();
        private Panel mainContentPanel = new Panel();
        private Panel topActionBar = new Panel();
        private FlowLayoutPanel subTabPanel = new FlowLayoutPanel();
        private Panel gridWrapper = new Panel();
        private Label lblPlaceholderMessage = new Label();

        // --- CONTROLS ---
        private DataGridView gridView = new DataGridView();
        private TextBox txtSearch = new TextBox();
        private Button btnNewRecord = new Button();

        // --- STATE & DATA ---
        private string currentMainTab = "Car Sales and Leads";
        private string currentSubTab = "New Inquiry";
        private List<SalesLead> _allSales = new List<SalesLead>();
        private List<RepairTicket> _allRepairs = new List<RepairTicket>();

        private readonly CrmApiService _apiService = new CrmApiService();
        private const int CurrentCompanyId = 1;

        public CustomerRecordsControl()
        {
            InitializeComponent();
            this.Controls.Clear();
            SetupLightModernUI();
            _ = LoadDataFromApiAsync();
        }

        private void SetupLightModernUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 260;
            sidebarPanel.BackColor = Color.FromArgb(255, 255, 255);
            sidebarPanel.Padding = new Padding(0, 20, 0, 0);

            Panel sidebarBorder = new Panel { Dock = DockStyle.Right, Width = 1, BackColor = Color.FromArgb(229, 231, 235) };
            sidebarPanel.Controls.Add(sidebarBorder);

            Label lblLogo = new Label { Text = "DriveConnect CRM", Dock = DockStyle.Top, Height = 60, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), TextAlign = ContentAlignment.MiddleCenter };
            sidebarPanel.Controls.Add(lblLogo);

            AddSidebarMenuButton("Archived", 430);
            AddSidebarMenuButton("Reports", 370);
            AddSidebarMenuButton("Customer History", 310);
            AddSidebarMenuButton("Promotions", 250);
            AddSidebarMenuButton("Service and Repair", 190);
            AddSidebarMenuButton("Car Sales and Leads", 130);
            AddSidebarMenuButton("Dashboard", 70);

            mainContentPanel.Dock = DockStyle.Fill;
            mainContentPanel.Padding = new Padding(30);

            topActionBar.Dock = DockStyle.Top;
            topActionBar.Height = 50;

            btnNewRecord.Text = "+ New Record";
            btnNewRecord.Width = 140;
            btnNewRecord.Height = 40;
            btnNewRecord.Location = new Point(0, 0);
            btnNewRecord.BackColor = Color.FromArgb(79, 70, 229);
            btnNewRecord.ForeColor = Color.White;
            btnNewRecord.FlatStyle = FlatStyle.Flat;
            btnNewRecord.FlatAppearance.BorderSize = 0;
            btnNewRecord.Font = new Font("Segoe UI Semibold", 9.5F);
            btnNewRecord.Cursor = Cursors.Hand;
            btnNewRecord.Click += BtnNewRecord_Click;

            txtSearch.PlaceholderText = "Search by Name, Phone, or Model...";
            txtSearch.Width = 350;
            txtSearch.Height = 40;
            txtSearch.Location = new Point(160, 8);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.TextChanged += (s, e) => FilterAndBindGrid();

            topActionBar.Controls.Add(btnNewRecord);
            topActionBar.Controls.Add(txtSearch);

            subTabPanel.Dock = DockStyle.Top;
            subTabPanel.Height = 60;
            subTabPanel.Padding = new Padding(0, 15, 0, 0);

            gridWrapper.Dock = DockStyle.Fill;
            gridWrapper.BackColor = Color.White;
            gridWrapper.Padding = new Padding(1);
            gridWrapper.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, gridWrapper.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);

            lblPlaceholderMessage.Dock = DockStyle.Fill;
            lblPlaceholderMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblPlaceholderMessage.Font = new Font("Segoe UI", 12F, FontStyle.Italic);
            lblPlaceholderMessage.ForeColor = Color.FromArgb(156, 163, 175);
            lblPlaceholderMessage.Visible = false;
            gridWrapper.Controls.Add(lblPlaceholderMessage);

            gridView.Dock = DockStyle.Fill;
            gridView.BackgroundColor = Color.White;
            gridView.BorderStyle = BorderStyle.None;
            gridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridView.GridColor = Color.FromArgb(243, 244, 246);
            gridView.AllowUserToAddRows = false;
            gridView.ReadOnly = true;
            gridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridView.RowHeadersVisible = false;
            gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridView.CellDoubleClick += GridView_CellDoubleClick;

            gridView.DefaultCellStyle = new DataGridViewCellStyle { Padding = new Padding(12, 8, 12, 8), Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(55, 65, 81), SelectionBackColor = Color.FromArgb(238, 242, 255), SelectionForeColor = Color.FromArgb(79, 70, 229) };
            gridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(249, 250, 251), Font = new Font("Segoe UI Semibold", 9.5F), ForeColor = Color.FromArgb(107, 114, 128), Padding = new Padding(12) };
            gridView.EnableHeadersVisualStyles = false;
            gridView.RowTemplate.Height = 45;

            gridWrapper.Controls.Add(gridView);

            mainContentPanel.Controls.Add(gridWrapper);
            mainContentPanel.Controls.Add(subTabPanel);
            mainContentPanel.Controls.Add(topActionBar);

            gridWrapper.BringToFront();
            subTabPanel.SendToBack();
            topActionBar.SendToBack();

            this.Controls.Add(mainContentPanel);
            this.Controls.Add(sidebarPanel);
            mainContentPanel.BringToFront();

            SwitchMainTab("Car Sales and Leads");
        }

        private void AddSidebarMenuButton(string text, int top)
        {
            Button btn = new Button { Text = "   " + text, Top = top, Left = 15, Width = 230, Height = 45, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(107, 114, 128), Font = new Font("Segoe UI Semibold", 10.5F), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => SwitchMainTab(text);
            sidebarPanel.Controls.Add(btn);
        }

        private void SwitchMainTab(string tabName)
        {
            currentMainTab = tabName;

            foreach (Control c in sidebarPanel.Controls)
            {
                if (c is Button b)
                {
                    if (b.Text.Trim() == tabName) { b.BackColor = Color.FromArgb(243, 244, 246); b.ForeColor = Color.FromArgb(79, 70, 229); }
                    else { b.BackColor = Color.White; b.ForeColor = Color.FromArgb(107, 114, 128); }
                }
            }

            if (currentMainTab == "Car Sales and Leads") currentSubTab = "New Inquiry";
            else if (currentMainTab == "Service and Repair") currentSubTab = "New Diagnose";
            else if (currentMainTab == "Archived") currentSubTab = "Archived Sales";

            BuildSubTabs();
        }

        private void BuildSubTabs()
        {
            subTabPanel.Controls.Clear();
            txtSearch.Clear();
            gridView.DataSource = null;

            if (currentMainTab == "Car Sales and Leads")
            {
                btnNewRecord.Visible = true;
                txtSearch.Visible = true;
                gridView.Visible = true;
                lblPlaceholderMessage.Visible = false;

                AddSubTab("New Inquiry");
                AddSubTab("Test Drive Scheduled");
                AddSubTab("Negotiation");
                AddSubTab("Closed Deals");

                FilterAndBindGrid();
            }
            else if (currentMainTab == "Service and Repair")
            {
                btnNewRecord.Visible = true;
                txtSearch.Visible = true;
                gridView.Visible = true;
                lblPlaceholderMessage.Visible = false;

                AddSubTab("New Diagnose");
                AddSubTab("In Repair");
                AddSubTab("Waiting for Parts");
                AddSubTab("Repaired");
                AddSubTab("Ready for Pickup");
                AddSubTab("Picked Up");

                FilterAndBindGrid();
            }
            else if (currentMainTab == "Archived")
            {
                btnNewRecord.Visible = false;
                txtSearch.Visible = true;
                gridView.Visible = true;
                lblPlaceholderMessage.Visible = false;

                AddSubTab("Archived Sales");
                AddSubTab("Archived Repairs");

                FilterAndBindGrid();
            }
            else
            {
                currentSubTab = "";
                btnNewRecord.Visible = false;
                txtSearch.Visible = false;
                gridView.Visible = false;

                lblPlaceholderMessage.Text = $"{currentMainTab} features are currently under development.";
                lblPlaceholderMessage.Visible = true;
            }
        }

        private void AddSubTab(string text)
        {
            Button btn = new Button { Text = text, AutoSize = true, MinimumSize = new Size(120, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9.5F), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 10, 0) };
            btn.FlatAppearance.BorderSize = 0;

            if (text == currentSubTab) { btn.BackColor = Color.FromArgb(224, 231, 255); btn.ForeColor = Color.FromArgb(67, 56, 202); }
            else { btn.BackColor = Color.Transparent; btn.ForeColor = Color.FromArgb(107, 114, 128); }

            btn.Click += (s, e) => {
                currentSubTab = text;
                BuildSubTabs();
            };
            subTabPanel.Controls.Add(btn);
        }

        private async Task LoadDataFromApiAsync()
        {
            try
            {
                _allSales = await _apiService.GetSalesAsync(CurrentCompanyId) ?? new List<SalesLead>();
                _allRepairs = await _apiService.GetRepairsAsync(CurrentCompanyId) ?? new List<RepairTicket>();
                FilterAndBindGrid();
            }
            catch (Exception)
            {
                // Ignore silent API fetch fail on initial load if API isn't booted yet
            }
        }

        private void FilterAndBindGrid()
        {
            string q = txtSearch.Text.ToLower();
            gridView.DataSource = null;

            if (currentMainTab == "Car Sales and Leads")
            {
                var query = _allSales.Where(x => x.Status != "Archived").AsQueryable();

                // Match the exact sub-tab statuses
                if (currentSubTab == "New Inquiry") query = query.Where(x => x.Status == "New Inquiry");
                else if (currentSubTab == "Test Drive Scheduled") query = query.Where(x => x.Status == "Test Drive Scheduled");
                else if (currentSubTab == "Negotiation") query = query.Where(x => x.Status == "Negotiation");
                else if (currentSubTab == "Closed Deals") query = query.Where(x => x.Status == "Closed Won" || x.Status == "Closed Lost");

                if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => (x.FirstName != null && x.FirstName.ToLower().Contains(q)) || (x.LastName != null && x.LastName.ToLower().Contains(q)) || (x.CarModel != null && x.CarModel.ToLower().Contains(q)));

                gridView.DataSource = query.Select(x => new {
                    ID = x.InquiryId,
                    Name = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                    Phone = x.PhoneNumber,
                    Model = x.CarModel,
                    Stage = x.Status,
                    Value = $"₱{x.EstimatedCost:N2}",
                    HandledBy = x.HandledBy,
                    DateAdded = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt")
                }).ToList();

                if (gridView.Columns["DateAdded"] != null) gridView.Columns["DateAdded"].HeaderText = "Date & Time Added";
            }
            else if (currentMainTab == "Service and Repair")
            {
                var query = _allRepairs.Where(x => x.Status != "Archived").AsQueryable();

                // Match the exact sub-tab statuses
                if (currentSubTab == "New Diagnose") query = query.Where(x => x.Status == "New Diagnose" || x.Status == "Diagnose");
                else if (currentSubTab == "In Repair") query = query.Where(x => x.Status == "In Repair");
                else if (currentSubTab == "Waiting for Parts") query = query.Where(x => x.Status == "Waiting for Parts");
                else if (currentSubTab == "Repaired") query = query.Where(x => x.Status == "Repaired" && x.PickupStatus != "Ready for Pickup" && x.PickupStatus != "Picked Up");
                else if (currentSubTab == "Ready for Pickup") query = query.Where(x => x.PickupStatus == "Ready for Pickup");
                else if (currentSubTab == "Picked Up") query = query.Where(x => x.PickupStatus == "Picked Up");

                if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => (x.FirstName != null && x.FirstName.ToLower().Contains(q)) || (x.LastName != null && x.LastName.ToLower().Contains(q)) || (x.CarModel != null && x.CarModel.ToLower().Contains(q)));

                gridView.DataSource = query.Select(x => new {
                    ID = x.TicketId,
                    Name = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                    Model = x.CarModel,
                    Issue = x.Concern,
                    Value = $"₱{x.EstimatedCost:N2}",
                    Status = x.Status,
                    Pickup = x.PickupStatus,
                    HandledBy = x.HandledBy,
                    DateAdded = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt")
                }).ToList();

                if (gridView.Columns["DateAdded"] != null) gridView.Columns["DateAdded"].HeaderText = "Date & Time Added";
                if (gridView.Columns["Value"] != null) gridView.Columns["Value"].HeaderText = "Estimated Cost";
            }
            else if (currentMainTab == "Archived")
            {
                if (currentSubTab == "Archived Sales")
                {
                    gridView.DataSource = _allSales.Where(x => x.Status == "Archived").Select(x => new {
                        ID = x.InquiryId,
                        Name = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                        Model = x.CarModel,
                        Value = $"₱{x.EstimatedCost:N2}",
                        ArchivedOn = x.CompletedAt?.ToLocalTime().ToString("MMM dd, yyyy") ?? "-",
                        Time = x.CompletedAt?.ToLocalTime().ToString("hh:mm tt") ?? "-"
                    }).ToList();
                }
                else
                {
                    gridView.DataSource = _allRepairs.Where(x => x.Status == "Archived").Select(x => new {
                        ID = x.TicketId,
                        Name = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                        Model = x.CarModel,
                        Issue = x.Concern,
                        Value = $"₱{x.EstimatedCost:N2}",
                        ArchivedOn = x.CompletedAt?.ToLocalTime().ToString("MMM dd, yyyy") ?? "-",
                        Time = x.CompletedAt?.ToLocalTime().ToString("hh:mm tt") ?? "-"
                    }).ToList();
                }

                if (gridView.Columns["ArchivedOn"] != null) gridView.Columns["ArchivedOn"].HeaderText = "Date Archived";
                if (gridView.Columns["Time"] != null) gridView.Columns["Time"].HeaderText = "Time";
                if (gridView.Columns["Value"] != null) gridView.Columns["Value"].HeaderText = "Estimated Cost";
            }
        }

        private void BtnNewRecord_Click(object? sender, EventArgs e)
        {
            if (currentMainTab == "Car Sales and Leads") ShowSalesModal(null);
            else if (currentMainTab == "Service and Repair") ShowRepairModal(null);
        }

        private void GridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = (int)gridView.Rows[e.RowIndex].Cells["ID"].Value;

            if (currentMainTab == "Car Sales and Leads") ShowSalesModal(_allSales.FirstOrDefault(x => x.InquiryId == id));
            else if (currentMainTab == "Service and Repair") ShowRepairModal(_allRepairs.FirstOrDefault(x => x.TicketId == id));
        }

        private async void ShowSalesModal(SalesLead? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "New Sales Lead" : "Edit Sales Lead", 720);

            int y = 70;
            TextBox txtF = AddFormField(f, "First Name", existing?.FirstName, ref y);
            TextBox txtM = AddFormField(f, "Middle Name", existing?.MiddleName, ref y);
            TextBox txtL = AddFormField(f, "Last Name", existing?.LastName, ref y);
            TextBox txtP = AddFormField(f, "Phone Number", existing?.PhoneNumber, ref y);
            TextBox txtE = AddFormField(f, "Email Address", existing?.EmailAddress, ref y);
            TextBox txtModel = AddFormField(f, "Interested Model", existing?.CarModel, ref y);
            TextBox txtC = AddFormField(f, "Est. Deal Value (₱)", existing?.EstimatedCost.ToString("F2") ?? "0.00", ref y);
            TextBox txtH = AddFormField(f, "Handled By", existing?.HandledBy, ref y);
            ComboBox cbS = AddFormCombo(f, "Sales Stage", new[] { "New Inquiry", "Test Drive Scheduled", "Negotiation", "Closed Won", "Closed Lost", "Archived" }, existing?.Status ?? "New Inquiry", ref y);

            Button btnSave = AddFormSubmitButton(f, existing == null ? "Save Lead" : "Update Lead", y);
            btnSave.Click += async (s, ev) => {
                if (existing == null && cbS.SelectedItem?.ToString() != "New Inquiry")
                {
                    MessageBox.Show("Adding data should be new inquiry", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtF.Text) || !System.Text.RegularExpressions.Regex.IsMatch(txtF.Text, @"^[a-zA-Z\s]+$")) { MessageBox.Show("Valid First Name is required (letters only).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!string.IsNullOrWhiteSpace(txtM.Text) && !System.Text.RegularExpressions.Regex.IsMatch(txtM.Text, @"^[a-zA-Z\s]+$")) { MessageBox.Show("Middle Name must contain letters only.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtL.Text) || !System.Text.RegularExpressions.Regex.IsMatch(txtL.Text, @"^[a-zA-Z\s]+$")) { MessageBox.Show("Valid Last Name is required (letters only).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtP.Text) || !System.Text.RegularExpressions.Regex.IsMatch(txtP.Text, @"^\d{11}$")) { MessageBox.Show("Phone Number must be exactly 11 digits (numbers only).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!string.IsNullOrWhiteSpace(txtE.Text) && !System.Text.RegularExpressions.Regex.IsMatch(txtE.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtH.Text)) { MessageBox.Show("Handled By is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                decimal.TryParse(txtC.Text, out decimal cost);

                var payload = existing ?? new SalesLead { CreatedAt = DateTime.UtcNow };
                payload.FirstName = txtF.Text;
                payload.MiddleName = txtM.Text;
                payload.LastName = txtL.Text;
                payload.PhoneNumber = txtP.Text;
                payload.EmailAddress = txtE.Text;
                payload.CarModel = txtModel.Text;
                payload.EstimatedCost = cost;
                payload.HandledBy = txtH.Text;
                payload.Status = cbS.SelectedItem?.ToString();

                if (payload.Status == "Closed Won" || payload.Status == "Closed Lost" || payload.Status == "Archived") payload.CompletedAt = DateTime.UtcNow;

                if (existing == null) await _apiService.CreateSalesAsync(CurrentCompanyId, payload);
                else await _apiService.UpdateSalesAsync(CurrentCompanyId, payload.InquiryId, payload);

                f.DialogResult = DialogResult.OK;
            };

            if (f.ShowDialog() == DialogResult.OK) await LoadDataFromApiAsync();
        }

        private async void ShowRepairModal(RepairTicket? existing)
        {
            using Form f = CreateBaseModal(existing == null ? "New Repair Ticket" : "Edit Repair Ticket", 830);

            int y = 70;
            TextBox txtF = AddFormField(f, "First Name", existing?.FirstName, ref y);
            TextBox txtM = AddFormField(f, "Middle Name", existing?.MiddleName, ref y);
            TextBox txtL = AddFormField(f, "Last Name", existing?.LastName, ref y);
            TextBox txtP = AddFormField(f, "Phone Number", existing?.PhoneNumber, ref y);
            TextBox txtModel = AddFormField(f, "Vehicle Model", existing?.CarModel, ref y);
            TextBox txtConcern = AddFormField(f, "Issue / Concern", existing?.Concern, ref y);
            TextBox txtC = AddFormField(f, "Estimated Cost", existing?.EstimatedCost.ToString("F2") ?? "0.00", ref y);
            TextBox txtH = AddFormField(f, "Handled By", existing?.HandledBy, ref y);
            ComboBox cbS = AddFormCombo(f, "Repair Status", new[] { "New Diagnose", "In Repair", "Waiting for Parts", "Repaired", "Archived" }, existing?.Status ?? "New Diagnose", ref y);
            ComboBox cbP = AddFormCombo(f, "Pickup Status", new[] { "Pending", "Ready for Pickup", "Picked Up" }, existing?.PickupStatus ?? "Pending", ref y);

            Button btnSave = AddFormSubmitButton(f, existing == null ? "Save Ticket" : "Update Ticket", y);
            btnSave.Click += async (s, ev) => {
                if (existing == null && cbS.SelectedItem?.ToString() != "New Diagnose")
                {
                    MessageBox.Show("New tickets should be Diagnosed first", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (existing == null && cbP.SelectedItem?.ToString() != "Pending")
                {
                    MessageBox.Show("New tickets can only be pending.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtF.Text) || !System.Text.RegularExpressions.Regex.IsMatch(txtF.Text, @"^[a-zA-Z\s]+$")) { MessageBox.Show("Valid First Name is required (letters only).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!string.IsNullOrWhiteSpace(txtM.Text) && !System.Text.RegularExpressions.Regex.IsMatch(txtM.Text, @"^[a-zA-Z\s]+$")) { MessageBox.Show("Middle Name must contain letters only.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtL.Text) || !System.Text.RegularExpressions.Regex.IsMatch(txtL.Text, @"^[a-zA-Z\s]+$")) { MessageBox.Show("Valid Last Name is required (letters only).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtP.Text) || !System.Text.RegularExpressions.Regex.IsMatch(txtP.Text, @"^\d{11}$")) { MessageBox.Show("Phone Number must be exactly 11 digits (numbers only).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtConcern.Text)) { MessageBox.Show("Issue / Concern is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtH.Text)) { MessageBox.Show("Handled By is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                decimal.TryParse(txtC.Text, out decimal cost);

                var payload = existing ?? new RepairTicket { CreatedAt = DateTime.UtcNow };
                payload.FirstName = txtF.Text;
                payload.MiddleName = txtM.Text;
                payload.LastName = txtL.Text;
                payload.PhoneNumber = txtP.Text;
                payload.CarModel = txtModel.Text;
                payload.Concern = txtConcern.Text;
                payload.EstimatedCost = cost;
                payload.HandledBy = txtH.Text;
                payload.Status = cbS.SelectedItem?.ToString();
                payload.PickupStatus = cbP.SelectedItem?.ToString();

                if (payload.Status == "Repaired" || payload.Status == "Archived") payload.CompletedAt = DateTime.UtcNow;
                if (payload.PickupStatus == "Picked Up") payload.PickedUpAt = DateTime.UtcNow;

                if (existing == null) await _apiService.CreateRepairAsync(CurrentCompanyId, payload);
                else await _apiService.UpdateRepairAsync(CurrentCompanyId, payload.TicketId, payload);

                f.DialogResult = DialogResult.OK;
            };

            if (f.ShowDialog() == DialogResult.OK) await LoadDataFromApiAsync();
        }

        private Form CreateBaseModal(string title, int clientHeight)
        {
            Form f = new Form
            {
                Text = title,
                ClientSize = new Size(450, clientHeight),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White,
                AutoScroll = true
            };
            Label lbl = new Label { Text = title, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), AutoSize = true, Location = new Point(30, 20) };
            f.Controls.Add(lbl);
            return f;
        }

        private TextBox AddFormField(Form parent, string labelText, string? val, ref int yPos)
        {
            Label l = new Label { Text = labelText, Location = new Point(30, yPos), AutoSize = true, Font = new Font("Segoe UI Semibold", 9F), ForeColor = Color.FromArgb(75, 85, 99) };
            TextBox t = new TextBox { Text = val, Location = new Point(30, yPos + 22), Width = 370, Font = new Font("Segoe UI", 10F), BorderStyle = BorderStyle.FixedSingle };
            parent.Controls.Add(l); parent.Controls.Add(t);
            yPos += 60; return t;
        }

        private ComboBox AddFormCombo(Form parent, string labelText, string[] items, string val, ref int yPos)
        {
            Label l = new Label { Text = labelText, Location = new Point(30, yPos), AutoSize = true, Font = new Font("Segoe UI Semibold", 9F), ForeColor = Color.FromArgb(75, 85, 99) };
            ComboBox c = new ComboBox { Location = new Point(30, yPos + 22), Width = 370, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            c.Items.AddRange(items); c.SelectedItem = val;
            parent.Controls.Add(l); parent.Controls.Add(c);
            yPos += 60; return c;
        }

        private Button AddFormSubmitButton(Form parent, string text, int yPos)
        {
            Button b = new Button { Text = text, Location = new Point(30, yPos + 10), Width = 370, Height = 45, BackColor = Color.FromArgb(79, 70, 229), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 10F), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            parent.Controls.Add(b);
            return b;
        }
    }
}