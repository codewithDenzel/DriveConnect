using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DriveConnect.winforms.Services;
using DriveConnect.domain.Entities; // Required for SalesLead and RepairTicket

namespace DriveConnect.winforms.Modules.Admin.CustomerRelationship.Controls
{
    public partial class CustomerRecordsControl : UserControl
    {
        // SIDE NAVIGATION BUTTONS
        private Button btnNavDashboard = null!;
        private Button btnNavSales = null!;
        private Button btnNavService = null!;
        private Button btnNavPromotions = null!;
        private Button btnNavHistory = null!;
        private Button btnNavReports = null!;
        private Button btnNavArchived = null!;

        // PANELS
        private Panel contentPanel = null!;
        private Panel panelDashboard = null!;
        private Panel panelSales = null!;
        private Panel panelService = null!;
        private Panel panelPromotions = null!;
        private Panel panelHistory = null!;
        private Panel panelReports = null!;
        private Panel panelArchived = null!;

        // DASHBOARD CONTROLS
        private Label lblTotalLeads = null!;
        private Label lblPipelineValue = null!;
        private Label lblConversionRate = null!;
        private Label lblActivePromos = null!;
        private DataGridView dgvPipelineSummary = null!;
        private DataGridView dgvTopSalespeople = null!;

        // SALES TAB CONTROLS
        private TextBox txtSalesFirstName = null!;
        private TextBox txtSalesMiddleName = null!;
        private TextBox txtSalesLastName = null!;
        private TextBox txtSalesPhone = null!;
        private TextBox txtSalesEmail = null!;
        private TextBox txtSalesCarModel = null!;
        private TextBox txtSalesCost = null!;
        private TextBox txtSalesHandledBy = null!;
        private ComboBox cbSalesLeadStatus = null!;
        private Button btnSalesCreate = null!;
        private Button btnSalesUpdate = null!;
        private Button btnSalesArchive = null!;
        private Button btnSalesClear = null!;
        private DataGridView dgvSalesLeads = null!;

        // REPAIR TAB CONTROLS
        private TextBox txtRepairFirstName = null!;
        private TextBox txtRepairMiddleName = null!;
        private TextBox txtRepairLastName = null!;
        private TextBox txtRepairPhone = null!;
        private TextBox txtRepairEmail = null!;
        private TextBox txtRepairCarModel = null!;
        private TextBox txtRepairConcern = null!;
        private TextBox txtRepairCost = null!;
        private ComboBox cbRepairStatus = null!;
        private ComboBox cbPickupStatus = null!;
        private TextBox txtHandledBy = null!;
        private Button btnRepairCreate = null!;
        private Button btnRepairUpdate = null!;
        private Button btnRepairArchive = null!;
        private DataGridView dgvRepairTickets = null!;

        // ARCHIVED TAB CONTROLS
        private DataGridView dgvArchived = null!;

        private readonly CrmApiService _apiService = new CrmApiService();
        private const int CurrentCompanyId = 1;

        // Trackers for Updates/Deletes
        private int selectedRepairId = 0;
        private int selectedSalesId = 0;

        // Separated Lists
        private List<SalesLead> _allSales = new List<SalesLead>();
        private List<RepairTicket> _allRepairs = new List<RepairTicket>();

        // DTO for binding combined archived records
        private class ArchivedRecordDto
        {
            public int ID { get; set; }
            public string Type { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string CarModel { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string ArchivedOn { get; set; } = string.Empty;
        }

        public CustomerRecordsControl()
        {
            InitializeComponent();
            BuildResponsiveUI();
            LoadData();
        }

        private void BuildResponsiveUI()
        {
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Dock = DockStyle.Fill;

            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 270, BackColor = Color.FromArgb(17, 24, 39) };
            Label lblAppLogo = new Label { Text = "DriveConnect CRM", Dock = DockStyle.Top, Height = 70, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            sidebar.Controls.Add(lblAppLogo);

            btnNavDashboard = CreateNavButton("📊  Dashboard", 80);
            btnNavSales = CreateNavButton("🚗  Car Sales & Leads", 135);
            btnNavService = CreateNavButton("🔧  Service & Repair", 190);
            btnNavPromotions = CreateNavButton("📢  Promotions", 245);
            btnNavHistory = CreateNavButton("🕒  Customer History", 300);
            btnNavReports = CreateNavButton("📄  Reports", 355);
            btnNavArchived = CreateNavButton("📁  Archived", 410);

            btnNavDashboard.Click += (s, e) => SwitchTab(panelDashboard, btnNavDashboard);
            btnNavSales.Click += (s, e) => SwitchTab(panelSales, btnNavSales);
            btnNavService.Click += (s, e) => SwitchTab(panelService, btnNavService);
            btnNavPromotions.Click += (s, e) => SwitchTab(panelPromotions, btnNavPromotions);
            btnNavHistory.Click += (s, e) => SwitchTab(panelHistory, btnNavHistory);
            btnNavReports.Click += (s, e) => SwitchTab(panelReports, btnNavReports);
            btnNavArchived.Click += (s, e) => SwitchTab(panelArchived, btnNavArchived);

            sidebar.Controls.AddRange(new Control[] { btnNavDashboard, btnNavSales, btnNavService, btnNavPromotions, btnNavHistory, btnNavReports, btnNavArchived });

            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 244, 246), Padding = new Padding(20) };

            panelDashboard = BuildDashboardView();
            panelSales = BuildSalesView();
            panelService = BuildServiceView();
            panelPromotions = BuildPromotionsView();
            panelHistory = BuildHistoryView();
            panelReports = BuildReportsView();
            panelArchived = BuildArchivedView();

            contentPanel.Controls.Add(panelDashboard);
            contentPanel.Controls.Add(panelSales);
            contentPanel.Controls.Add(panelService);
            contentPanel.Controls.Add(panelPromotions);
            contentPanel.Controls.Add(panelHistory);
            contentPanel.Controls.Add(panelReports);
            contentPanel.Controls.Add(panelArchived);

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);

            SwitchTab(panelDashboard, btnNavDashboard);
        }

        private Button CreateNavButton(string text, int top)
        {
            var btn = new Button
            {
                Text = text,
                Left = 10,
                Top = top,
                Width = 250,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                UseMnemonic = false
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SwitchTab(Panel selectedPanel, Button selectedNavBtn)
        {
            panelDashboard.Visible = false; panelSales.Visible = false; panelService.Visible = false;
            panelPromotions.Visible = false; panelHistory.Visible = false; panelReports.Visible = false; panelArchived.Visible = false;

            btnNavDashboard.BackColor = Color.Transparent; btnNavDashboard.ForeColor = Color.FromArgb(156, 163, 175);
            btnNavSales.BackColor = Color.Transparent; btnNavSales.ForeColor = Color.FromArgb(156, 163, 175);
            btnNavService.BackColor = Color.Transparent; btnNavService.ForeColor = Color.FromArgb(156, 163, 175);
            btnNavPromotions.BackColor = Color.Transparent; btnNavPromotions.ForeColor = Color.FromArgb(156, 163, 175);
            btnNavHistory.BackColor = Color.Transparent; btnNavHistory.ForeColor = Color.FromArgb(156, 163, 175);
            btnNavReports.BackColor = Color.Transparent; btnNavReports.ForeColor = Color.FromArgb(156, 163, 175);
            btnNavArchived.BackColor = Color.Transparent; btnNavArchived.ForeColor = Color.FromArgb(156, 163, 175);

            selectedPanel.Visible = true;
            selectedNavBtn.BackColor = Color.FromArgb(124, 58, 237);
            selectedNavBtn.ForeColor = Color.White;
        }

        private Panel BuildDashboardView()
        {
            Panel main = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            TableLayoutPanel topCardsGrid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 130, ColumnCount = 4, RowCount = 1, Padding = new Padding(0, 0, 0, 15) };
            for (int i = 0; i < 4; i++) topCardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            topCardsGrid.Controls.Add(CreateStatCard("Active Sales Leads", lblTotalLeads = new Label { Text = "0" }, Color.FromArgb(124, 58, 237)), 0, 0);
            topCardsGrid.Controls.Add(CreateStatCard("Total Pipeline Value", lblPipelineValue = new Label { Text = "₱0.00" }, Color.FromArgb(16, 185, 129)), 1, 0);
            topCardsGrid.Controls.Add(CreateStatCard("Lead Conversion Rate", lblConversionRate = new Label { Text = "0%" }, Color.FromArgb(59, 130, 246)), 2, 0);
            topCardsGrid.Controls.Add(CreateStatCard("Active Promotions", lblActivePromos = new Label { Text = "0" }, Color.FromArgb(245, 158, 11)), 3, 0);

            TableLayoutPanel bottomSplit = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            bottomSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            bottomSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Panel leftCard = CreateCardPanel();
            AddHeader(leftCard, "Dashboard Overview", "High-level CRM sales performance & active pipelines");

            dgvPipelineSummary = CreateGridView();
            dgvPipelineSummary.Dock = DockStyle.Fill;
            leftCard.Controls.Add(dgvPipelineSummary);

            Panel rightCard = CreateCardPanel();
            AddHeader(rightCard, "Sales Team Leaderboard", "Top performing salespeople by closed deals");

            dgvTopSalespeople = CreateGridView();
            dgvTopSalespeople.Dock = DockStyle.Fill;
            rightCard.Controls.Add(dgvTopSalespeople);

            bottomSplit.Controls.Add(leftCard, 0, 0);
            bottomSplit.Controls.Add(rightCard, 1, 0);
            main.Controls.Add(bottomSplit);
            main.Controls.Add(topCardsGrid);
            return main;
        }

        private Panel CreateStatCard(string title, Label valLabel, Color accentColor)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);
            Label lblT = new Label { Text = title, Left = 15, Top = 15, AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(107, 114, 128) };
            valLabel.Left = 15; valLabel.Top = 45; valLabel.Width = 220; valLabel.Height = 40;
            valLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold); valLabel.ForeColor = accentColor;
            card.Controls.Add(lblT); card.Controls.Add(valLabel);
            return card;
        }

        private Panel BuildSalesView()
        {
            TableLayoutPanel split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            Panel formPanel = CreateCardPanel();
            AddHeader(formPanel, "New Car Sales Inquiry", "Capture prospective car buyers and leads");

            int top = 85;
            AddLabelAndControl(formPanel, "First Name", txtSalesFirstName = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Middle Name", txtSalesMiddleName = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Last Name", txtSalesLastName = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Phone Number", txtSalesPhone = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Email Address", txtSalesEmail = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Interested Model", txtSalesCarModel = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Est. Deal Value (₱)", txtSalesCost = CreateTextBox("0.00"), ref top);

            cbSalesLeadStatus = CreateComboBox(new[] { "New Inquiry", "Test Drive Scheduled", "Negotiation", "Closed Won", "Closed Lost" });
            AddLabelAndControl(formPanel, "Sales Stage", cbSalesLeadStatus, ref top);

            AddLabelAndControl(formPanel, "Handled By", txtSalesHandledBy = CreateTextBox("Staff name diri"), ref top);

            top += 15;
            btnSalesCreate = CreateButton("Save Lead", Color.FromArgb(124, 58, 237), 20, top, 95);
            btnSalesCreate.Click += BtnSalesCreate_Click;

            btnSalesUpdate = CreateButton("Update", Color.FromArgb(79, 70, 229), 125, top, 85);
            btnSalesUpdate.Click += BtnSalesUpdate_Click;

            btnSalesArchive = CreateButton("Archive", Color.FromArgb(220, 38, 38), 220, top, 85);
            btnSalesArchive.Click += BtnSalesArchive_Click;

            btnSalesClear = CreateButton("Clear", Color.FromArgb(107, 114, 128), 315, top, 85);
            btnSalesClear.Click += (s, e) => ClearSalesInputs();

            formPanel.Controls.AddRange(new Control[] { btnSalesCreate, btnSalesUpdate, btnSalesArchive, btnSalesClear });

            Panel tablePanel = CreateCardPanel();
            AddHeader(tablePanel, "Queue", "Active car purchasing opportunities");

            dgvSalesLeads = CreateGridView();
            dgvSalesLeads.Dock = DockStyle.Fill;
            dgvSalesLeads.CellClick += DgvSalesLeads_CellClick;
            tablePanel.Controls.Add(dgvSalesLeads);

            split.Controls.Add(formPanel, 0, 0);
            split.Controls.Add(tablePanel, 1, 0);
            return split;
        }

        private Panel BuildServiceView()
        {
            TableLayoutPanel split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };

            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 450F));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            Panel formPanel = CreateCardPanel();
            AddHeader(formPanel, "Service & Repair Ticket", "Track vehicle maintenance and customer issues");

            int top = 85;
            AddLabelAndControl(formPanel, "First Name", txtRepairFirstName = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Middle Name", txtRepairMiddleName = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Last Name", txtRepairLastName = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Phone Number", txtRepairPhone = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Email Address", txtRepairEmail = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Vehicle Model", txtRepairCarModel = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Issue / Concern", txtRepairConcern = CreateTextBox(), ref top);
            AddLabelAndControl(formPanel, "Est. Cost (₱)", txtRepairCost = CreateTextBox("0.00"), ref top);

            cbRepairStatus = CreateComboBox(new[] { "Diagnose", "In Repair", "Waiting for Parts", "Repaired" });
            AddLabelAndControl(formPanel, "Repair Status", cbRepairStatus, ref top);

            cbPickupStatus = CreateComboBox(new[] { "Pending", "Ready for Pickup", "Picked Up" });
            AddLabelAndControl(formPanel, "Pickup Status", cbPickupStatus, ref top);

            AddLabelAndControl(formPanel, "Handled By", txtHandledBy = CreateTextBox("ngan sa Sales Staff"), ref top);

            top += 15;
            btnRepairCreate = CreateButton("Save Ticket", Color.FromArgb(124, 58, 237), 20, top, 95);
            btnRepairCreate.Click += BtnRepairCreate_Click;

            btnRepairUpdate = CreateButton("Update", Color.FromArgb(79, 70, 229), 125, top, 85);
            btnRepairUpdate.Click += BtnRepairUpdate_Click;

            btnRepairArchive = CreateButton("Archive", Color.FromArgb(220, 38, 38), 220, top, 85);
            btnRepairArchive.Click += BtnRepairArchive_Click;

            Button btnClear = CreateButton("Clear", Color.FromArgb(107, 114, 128), 315, top, 85);
            btnClear.Click += (s, e) => ClearRepairInputs();

            formPanel.Controls.AddRange(new Control[] { btnRepairCreate, btnRepairUpdate, btnRepairArchive, btnClear });

            Panel tablePanel = CreateCardPanel();
            AddHeader(tablePanel, "Active Repair Work Orders", "Current vehicles undergoing maintenance");

            dgvRepairTickets = CreateGridView();
            dgvRepairTickets.Dock = DockStyle.Fill;
            dgvRepairTickets.CellClick += DgvRepairTickets_CellClick;
            tablePanel.Controls.Add(dgvRepairTickets);

            split.Controls.Add(formPanel, 0, 0);
            split.Controls.Add(tablePanel, 1, 0);
            return split;
        }

        private Panel BuildArchivedView()
        {
            Panel main = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Panel card = CreateCardPanel();
            AddHeader(card, "Archived Records", "Historical log of archived sales leads and repair tickets");

            dgvArchived = CreateGridView();
            dgvArchived.Dock = DockStyle.Fill;
            card.Controls.Add(dgvArchived);

            main.Controls.Add(card);
            return main;
        }

        private Panel BuildPromotionsView() { Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }; Panel c = CreateCardPanel(); AddHeader(c, "Promotions & Offers", "Manage discount campaigns."); p.Controls.Add(c); return p; }
        private Panel BuildHistoryView() { Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }; Panel c = CreateCardPanel(); AddHeader(c, "Customer History", "View past interactions."); p.Controls.Add(c); return p; }
        private Panel BuildReportsView() { Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }; Panel c = CreateCardPanel(); AddHeader(c, "System Reports", "Generate analytics."); p.Controls.Add(c); return p; }

        private Panel CreateCardPanel()
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5), Padding = new Padding(15, 80, 15, 15) };
            p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);
            return p;
        }

        private void AddHeader(Panel parent, string title, string subtitle)
        {
            Label lblT = new Label { Text = title, Left = 20, Top = 20, AutoSize = true, Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(124, 58, 237) };
            Label lblS = new Label { Text = subtitle, Left = 20, Top = 50, AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(107, 114, 128) };
            parent.Controls.Add(lblT); parent.Controls.Add(lblS);
        }

        private TextBox CreateTextBox(string def = "") => new TextBox { Text = def, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(250, 250, 250) };
        private ComboBox CreateComboBox(string[] items) { var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F), FlatStyle = FlatStyle.Flat }; cb.Items.AddRange(items); cb.SelectedIndex = 0; return cb; }
        private Button CreateButton(string text, Color bg, int left, int top, int width) => new Button { Text = text, Left = left, Top = top, Width = width, Height = 38, BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), Cursor = Cursors.Hand };

        private DataGridView CreateGridView()
        {
            var gv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(243, 244, 246),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 35 }
            };
            gv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(249, 250, 251), ForeColor = Color.FromArgb(75, 85, 99), Font = new Font("Segoe UI", 9F, FontStyle.Bold), Padding = new Padding(10, 5, 10, 5) };
            gv.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI", 9F), SelectionBackColor = Color.FromArgb(237, 233, 254), SelectionForeColor = Color.FromArgb(124, 58, 237), Padding = new Padding(10, 0, 10, 0) };
            return gv;
        }

        private void AddLabelAndControl(Panel parent, string text, Control ctrl, ref int top)
        {
            Label lbl = new Label { Text = text, Left = 20, Top = top, AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 81) };
            ctrl.Left = 20;
            ctrl.Top = top + 22;
            ctrl.Width = 380;
            parent.Controls.Add(lbl);
            parent.Controls.Add(ctrl);
            top += 60;
        }

        private bool ConfirmAction(string message, string title)
        {
            DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        private async void LoadData()
        {
            try
            {
                _allSales = await _apiService.GetSalesAsync(CurrentCompanyId);
                _allRepairs = await _apiService.GetRepairsAsync(CurrentCompanyId);

                // --- DATA FILTERING ---
                var activeSales = _allSales.FindAll(x => x.Status != "Archived");
                var archivedSales = _allSales.FindAll(x => x.Status == "Archived");

                var activeRepairs = _allRepairs.FindAll(x => x.Status != "Archived");
                var archivedRepairs = _allRepairs.FindAll(x => x.Status == "Archived");

                // --- BIND SALES GRID ---
                var salesDisplayList = activeSales.Select(x => new {
                    x.InquiryId,
                    FullName = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                    x.PhoneNumber,
                    x.EmailAddress,
                    x.CarModel,
                    x.Status,
                    EstimatedCost = $"₱{x.EstimatedCost:N2}",
                    x.HandledBy,
                    CreatedAt = x.CreatedAt.ToString("MMM dd, yyyy hh:mm tt"),
                    CompletedAt = x.CompletedAt?.ToString("MMM dd, yyyy hh:mm tt") ?? "-"
                }).ToList();

                dgvSalesLeads.DataSource = null;
                dgvSalesLeads.DataSource = salesDisplayList;
                if (dgvSalesLeads.Columns.Count > 0)
                {
                    dgvSalesLeads.Columns["InquiryId"].HeaderText = "ID";
                    dgvSalesLeads.Columns["FullName"].HeaderText = "Full Name";
                    dgvSalesLeads.Columns["PhoneNumber"].HeaderText = "Phone";
                    dgvSalesLeads.Columns["EmailAddress"].HeaderText = "Email";
                    dgvSalesLeads.Columns["CarModel"].HeaderText = "Car Model";
                    dgvSalesLeads.Columns["Status"].HeaderText = "Status";
                    dgvSalesLeads.Columns["EstimatedCost"].HeaderText = "Est. Cost";
                    dgvSalesLeads.Columns["HandledBy"].HeaderText = "Handled By";
                    dgvSalesLeads.Columns["CreatedAt"].HeaderText = "Created At";
                    dgvSalesLeads.Columns["CompletedAt"].HeaderText = "Completed At";
                }

                // --- BIND REPAIR GRID ---
                var repairDisplayList = activeRepairs.Select(x => new {
                    x.TicketId,
                    FullName = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                    x.PhoneNumber,
                    x.EmailAddress,
                    x.CarModel,
                    x.Concern,
                    x.Status,
                    EstimatedCost = $"₱{x.EstimatedCost:N2}",
                    x.HandledBy,
                    CreatedAt = x.CreatedAt.ToString("MMM dd, yyyy hh:mm tt"),
                    CompletedAt = x.CompletedAt?.ToString("MMM dd, yyyy hh:mm tt") ?? "-",
                    x.PickupStatus,
                    PickedUpAt = x.PickedUpAt?.ToString("MMM dd, yyyy hh:mm tt") ?? "-"
                }).ToList();

                dgvRepairTickets.DataSource = null;
                dgvRepairTickets.DataSource = repairDisplayList;
                if (dgvRepairTickets.Columns.Count > 0)
                {
                    dgvRepairTickets.Columns["TicketId"].HeaderText = "ID";
                    dgvRepairTickets.Columns["FullName"].HeaderText = "Full Name";
                    dgvRepairTickets.Columns["PhoneNumber"].HeaderText = "Phone";
                    dgvRepairTickets.Columns["EmailAddress"].HeaderText = "Email";
                    dgvRepairTickets.Columns["CarModel"].HeaderText = "Car Model";
                    dgvRepairTickets.Columns["Concern"].HeaderText = "Concern";
                    dgvRepairTickets.Columns["Status"].HeaderText = "Status";
                    dgvRepairTickets.Columns["EstimatedCost"].HeaderText = "Est. Cost";
                    dgvRepairTickets.Columns["HandledBy"].HeaderText = "Handled By";
                    dgvRepairTickets.Columns["CreatedAt"].HeaderText = "Created At";
                    dgvRepairTickets.Columns["CompletedAt"].HeaderText = "Completed At";
                    dgvRepairTickets.Columns["PickupStatus"].HeaderText = "Pickup Status";
                    dgvRepairTickets.Columns["PickedUpAt"].HeaderText = "Picked Up At";
                }

                // --- BIND ARCHIVED GRID ---
                var archivedCombinedList = new List<ArchivedRecordDto>();

                archivedCombinedList.AddRange(archivedSales.Select(x => new ArchivedRecordDto
                {
                    ID = x.InquiryId,
                    Type = "Sales",
                    FullName = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                    CarModel = x.CarModel ?? "",
                    Status = x.Status ?? "",
                    ArchivedOn = x.CompletedAt?.ToString("MMM dd, yyyy hh:mm tt") ?? "-"
                }));

                archivedCombinedList.AddRange(archivedRepairs.Select(x => new ArchivedRecordDto
                {
                    ID = x.TicketId,
                    Type = "Repair",
                    FullName = $"{x.FirstName} {x.MiddleName} {x.LastName}".Replace("  ", " ").Trim(),
                    CarModel = x.CarModel ?? "",
                    Status = x.Status ?? "",
                    ArchivedOn = x.CompletedAt?.ToString("MMM dd, yyyy hh:mm tt") ?? "-"
                }));

                dgvArchived.DataSource = null;
                dgvArchived.DataSource = archivedCombinedList;
                if (dgvArchived.Columns.Count > 0)
                {
                    dgvArchived.Columns["ID"].HeaderText = "ID";
                    dgvArchived.Columns["Type"].HeaderText = "Record Type";
                    dgvArchived.Columns["FullName"].HeaderText = "Full Name";
                    dgvArchived.Columns["CarModel"].HeaderText = "Car Model";
                    dgvArchived.Columns["Status"].HeaderText = "Status";
                    dgvArchived.Columns["ArchivedOn"].HeaderText = "Archived On";
                }

                // --- DASHBOARD METRICS ---
                var activeSalesLeads = activeSales.Where(x => x.Status != "Closed Won" && x.Status != "Closed Lost").ToList();
                lblTotalLeads.Text = activeSalesLeads.Count.ToString();
                lblPipelineValue.Text = $"₱{activeSalesLeads.Sum(x => x.EstimatedCost):N2}";

                int closedWon = activeSales.Count(x => x.Status == "Closed Won");
                lblConversionRate.Text = $"{(activeSales.Count > 0 ? Math.Round(((decimal)closedWon / activeSales.Count) * 100, 1) : 0)}%";
                lblActivePromos.Text = "0";

                var pipelineSummary = activeSales
                    .GroupBy(x => string.IsNullOrEmpty(x.Status) ? "New Inquiry" : x.Status)
                    .Select(g => new {
                        SalesStage = g.Key,
                        TotalLeads = g.Count(),
                        PipelineValue = $"₱{g.Sum(item => item.EstimatedCost):N2}"
                    })
                    .ToList();

                dgvPipelineSummary.DataSource = null;
                dgvPipelineSummary.DataSource = pipelineSummary;

                var leaders = activeSales.Where(x => !string.IsNullOrEmpty(x.HandledBy))
                    .GroupBy(x => x.HandledBy)
                    .Select(g => new {
                        Salesperson = g.Key,
                        ActiveLeads = g.Count(c => c.Status != "Closed Won" && c.Status != "Closed Lost"),
                        DealsWon = g.Count(c => c.Status == "Closed Won")
                    })
                    .OrderByDescending(x => x.DealsWon).ThenByDescending(x => x.ActiveLeads).ToList();

                dgvTopSalespeople.DataSource = null;
                dgvTopSalespeople.DataSource = leaders;
            }
            catch { }
        }

        private bool ValidateSalesInputs()
        {
            if (string.IsNullOrWhiteSpace(txtSalesFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtSalesLastName.Text) ||
                string.IsNullOrWhiteSpace(txtSalesPhone.Text) ||
                string.IsNullOrWhiteSpace(txtSalesEmail.Text) ||
                string.IsNullOrWhiteSpace(txtSalesCarModel.Text) ||
                string.IsNullOrWhiteSpace(txtSalesCost.Text) ||
                string.IsNullOrWhiteSpace(txtSalesHandledBy.Text))
            {
                MessageBox.Show("Please fill in all required fields to proceed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool ValidateRepairInputs()
        {
            if (string.IsNullOrWhiteSpace(txtRepairFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtRepairLastName.Text) ||
                string.IsNullOrWhiteSpace(txtRepairPhone.Text) ||
                string.IsNullOrWhiteSpace(txtRepairEmail.Text) ||
                string.IsNullOrWhiteSpace(txtRepairCarModel.Text) ||
                string.IsNullOrWhiteSpace(txtRepairConcern.Text) ||
                string.IsNullOrWhiteSpace(txtRepairCost.Text) ||
                string.IsNullOrWhiteSpace(txtHandledBy.Text))
            {
                MessageBox.Show("Please fill in all required fields to proceed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private async void BtnSalesCreate_Click(object? sender, EventArgs e)
        {
            if (!ValidateSalesInputs()) return;
            if (!ConfirmAction("Are you sure you want to save this new lead?", "Save Lead")) return;

            decimal.TryParse(txtSalesCost.Text, out decimal cost);
            string currentStatus = "New Inquiry";

            var lead = new SalesLead
            {
                FirstName = txtSalesFirstName.Text,
                MiddleName = txtSalesMiddleName.Text,
                LastName = txtSalesLastName.Text,
                PhoneNumber = txtSalesPhone.Text,
                EmailAddress = txtSalesEmail.Text,
                CarModel = txtSalesCarModel.Text,
                EstimatedCost = cost,
                Status = currentStatus,
                HandledBy = txtSalesHandledBy.Text,
                CreatedAt = DateTime.UtcNow
            };

            if ((await _apiService.CreateSalesAsync(CurrentCompanyId, lead)).IsSuccessStatusCode)
            {
                MessageBox.Show("Lead created!", "DriveConnect CRM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearSalesInputs(); LoadData();
            }
        }

        private async void BtnSalesUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedSalesId == 0) return;
            if (!ValidateSalesInputs()) return;
            if (!ConfirmAction("Are you sure you want to update this lead?", "Update Lead")) return;

            decimal.TryParse(txtSalesCost.Text, out decimal cost);
            string currentStatus = cbSalesLeadStatus.SelectedItem?.ToString() ?? "New Inquiry";
            DateTime? completedAtDate = (currentStatus == "Closed Won" || currentStatus == "Closed Lost") ? DateTime.UtcNow : (DateTime?)null;

            var update = new SalesLead
            {
                InquiryId = selectedSalesId,
                FirstName = txtSalesFirstName.Text,
                MiddleName = txtSalesMiddleName.Text,
                LastName = txtSalesLastName.Text,
                PhoneNumber = txtSalesPhone.Text,
                EmailAddress = txtSalesEmail.Text,
                CarModel = txtSalesCarModel.Text,
                EstimatedCost = cost,
                Status = currentStatus,
                HandledBy = txtSalesHandledBy.Text,
                CompletedAt = completedAtDate
            };

            if ((await _apiService.UpdateSalesAsync(CurrentCompanyId, selectedSalesId, update)).IsSuccessStatusCode)
            {
                MessageBox.Show("Lead updated!", "DriveConnect CRM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearSalesInputs(); LoadData();
            }
        }

        private async void BtnSalesArchive_Click(object? sender, EventArgs e)
        {
            if (selectedSalesId == 0) return;
            if (!ConfirmAction("Are you sure you want to ARCHIVE this lead? It will be moved to the Archived tab.", "Archive Lead")) return;

            var existingRecord = _allSales.Find(x => x.InquiryId == selectedSalesId);
            if (existingRecord == null) return;

            existingRecord.Status = "Archived";
            existingRecord.CompletedAt = DateTime.UtcNow;

            if ((await _apiService.UpdateSalesAsync(CurrentCompanyId, selectedSalesId, existingRecord)).IsSuccessStatusCode)
            {
                MessageBox.Show("Lead archived successfully!", "DriveConnect CRM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearSalesInputs(); LoadData();
            }
        }

        private void DgvSalesLeads_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var idObj = dgvSalesLeads.Rows[e.RowIndex].Cells["InquiryId"].Value;
                if (idObj != null && int.TryParse(idObj.ToString(), out int id))
                {
                    selectedSalesId = id;
                    var item = _allSales.Find(x => x.InquiryId == id);
                    if (item != null)
                    {
                        txtSalesFirstName.Text = item.FirstName ?? "";
                        txtSalesMiddleName.Text = item.MiddleName ?? "";
                        txtSalesLastName.Text = item.LastName ?? "";
                        txtSalesPhone.Text = item.PhoneNumber ?? "";
                        txtSalesEmail.Text = item.EmailAddress ?? "";
                        txtSalesCarModel.Text = item.CarModel ?? "";
                        txtSalesCost.Text = item.EstimatedCost.ToString("F2");
                        txtSalesHandledBy.Text = item.HandledBy ?? "";
                        if (cbSalesLeadStatus.Items.Contains(item.Status ?? "")) cbSalesLeadStatus.SelectedItem = item.Status;
                    }
                }
            }
        }

        private void ClearSalesInputs()
        {
            selectedSalesId = 0;
            txtSalesFirstName.Text = ""; txtSalesMiddleName.Text = ""; txtSalesLastName.Text = ""; txtSalesPhone.Text = ""; txtSalesEmail.Text = "";
            txtSalesCarModel.Text = ""; txtSalesCost.Text = "0.00"; cbSalesLeadStatus.SelectedIndex = 0;
        }

        private async void BtnRepairCreate_Click(object? sender, EventArgs e)
        {
            if (!ValidateRepairInputs()) return;
            if (!ConfirmAction("Are you sure you want to create this new service ticket?", "Save Ticket")) return;

            decimal.TryParse(txtRepairCost.Text, out decimal cost);
            string currentStatus = "Diagnose";
            string currentPickup = "Pending";

            var repair = new RepairTicket
            {
                FirstName = txtRepairFirstName.Text,
                MiddleName = txtRepairMiddleName.Text,
                LastName = txtRepairLastName.Text,
                PhoneNumber = txtRepairPhone.Text,
                EmailAddress = txtRepairEmail.Text,
                CarModel = txtRepairCarModel.Text,
                Concern = txtRepairConcern.Text,
                EstimatedCost = cost,
                Status = currentStatus,
                PickupStatus = currentPickup,
                HandledBy = txtHandledBy.Text,
                CreatedAt = DateTime.UtcNow
            };

            if ((await _apiService.CreateRepairAsync(CurrentCompanyId, repair)).IsSuccessStatusCode)
            {
                MessageBox.Show("Ticket created!", "DriveConnect CRM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearRepairInputs(); LoadData();
            }
        }

        private async void BtnRepairUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedRepairId == 0) return;
            if (!ValidateRepairInputs()) return;
            if (!ConfirmAction("Are you sure you want to update this service ticket?", "Update Ticket")) return;

            decimal.TryParse(txtRepairCost.Text, out decimal cost);

            string currentStatus = cbRepairStatus.SelectedItem?.ToString() ?? "Diagnose";
            DateTime? completedAtDate = (currentStatus == "Repaired") ? DateTime.UtcNow : (DateTime?)null;

            string currentPickup = cbPickupStatus.SelectedItem?.ToString() ?? "Pending";
            DateTime? pickedUpAtDate = (currentPickup == "Picked Up") ? DateTime.UtcNow : (DateTime?)null;

            var update = new RepairTicket
            {
                TicketId = selectedRepairId,
                FirstName = txtRepairFirstName.Text,
                MiddleName = txtRepairMiddleName.Text,
                LastName = txtRepairLastName.Text,
                PhoneNumber = txtRepairPhone.Text,
                EmailAddress = txtRepairEmail.Text,
                CarModel = txtRepairCarModel.Text,
                Concern = txtRepairConcern.Text,
                EstimatedCost = cost,
                Status = currentStatus,
                PickupStatus = currentPickup,
                HandledBy = txtHandledBy.Text,
                CompletedAt = completedAtDate,
                PickedUpAt = pickedUpAtDate
            };

            if ((await _apiService.UpdateRepairAsync(CurrentCompanyId, selectedRepairId, update)).IsSuccessStatusCode)
            {
                MessageBox.Show("Ticket updated!", "DriveConnect CRM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearRepairInputs(); LoadData();
            }
        }

        private async void BtnRepairArchive_Click(object? sender, EventArgs e)
        {
            if (selectedRepairId == 0) return;
            if (!ConfirmAction("Are you sure you want to ARCHIVE this ticket? It will be moved to the Archived tab.", "Archive Ticket")) return;

            var existingRecord = _allRepairs.Find(x => x.TicketId == selectedRepairId);
            if (existingRecord == null) return;

            existingRecord.Status = "Archived";
            existingRecord.CompletedAt = DateTime.UtcNow;

            if ((await _apiService.UpdateRepairAsync(CurrentCompanyId, selectedRepairId, existingRecord)).IsSuccessStatusCode)
            {
                MessageBox.Show("Ticket archived successfully!", "DriveConnect CRM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearRepairInputs(); LoadData();
            }
        }

        private void DgvRepairTickets_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var idObj = dgvRepairTickets.Rows[e.RowIndex].Cells["TicketId"].Value;
                if (idObj != null && int.TryParse(idObj.ToString(), out int id))
                {
                    selectedRepairId = id;
                    var item = _allRepairs.Find(x => x.TicketId == id);
                    if (item != null)
                    {
                        txtRepairFirstName.Text = item.FirstName ?? "";
                        txtRepairMiddleName.Text = item.MiddleName ?? "";
                        txtRepairLastName.Text = item.LastName ?? "";
                        txtRepairPhone.Text = item.PhoneNumber ?? "";
                        txtRepairEmail.Text = item.EmailAddress ?? "";
                        txtRepairCarModel.Text = item.CarModel ?? "";
                        txtRepairConcern.Text = item.Concern ?? "";
                        txtRepairCost.Text = item.EstimatedCost.ToString("F2");
                        txtHandledBy.Text = item.HandledBy ?? "";
                        if (cbRepairStatus.Items.Contains(item.Status ?? "")) cbRepairStatus.SelectedItem = item.Status;
                        if (cbPickupStatus.Items.Contains(item.PickupStatus ?? "")) cbPickupStatus.SelectedItem = item.PickupStatus;
                    }
                }
            }
        }

        private void ClearRepairInputs()
        {
            selectedRepairId = 0; txtRepairFirstName.Text = ""; txtRepairMiddleName.Text = ""; txtRepairLastName.Text = ""; txtRepairPhone.Text = ""; txtRepairEmail.Text = "";
            txtRepairCarModel.Text = ""; txtRepairConcern.Text = ""; txtRepairCost.Text = "0.00";
            cbRepairStatus.SelectedIndex = 0; cbPickupStatus.SelectedIndex = 0;
        }
    }
}