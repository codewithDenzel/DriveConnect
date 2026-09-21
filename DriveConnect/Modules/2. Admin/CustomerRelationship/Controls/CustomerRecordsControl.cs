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
        private Label lblBreadcrumb = new Label(); // Repurposing top sub-tab panel as a page title

        // --- BUSINESS INTELLIGENCE PANELS ---
        private Panel panelBI_Dashboard = new Panel();
        private Panel panelBI_KPI = new Panel();
        private Panel panelBI_Reports = new Panel();
        private Panel panelBI_Graphs = new Panel();

        // --- DASHBOARD UI CONTROLS ---
        private Label lblTotalLeads = new Label();
        private Label lblPipelineValue = new Label();
        private Label lblConversionRate = new Label();
        private Label lblActivePromos = new Label();
        private DataGridView dgvPipelineSummary = new DataGridView();
        private DataGridView dgvTopSalespeople = new DataGridView();

        // --- KPI UI CONTROLS ---
        private Label lblKpiActiveLeads = new Label();
        private Label lblKpiClosedWon = new Label();
        private Label lblKpiClosedLost = new Label();
        private Label lblKpiAverageDeal = new Label();
        private Label lblKpiPipeline = new Label();
        private Label lblKpiActiveRepairs = new Label();
        private Label lblKpiRepaired = new Label();
        private Label lblKpiPickedUp = new Label();
        private DataGridView dgvKpiStages = new DataGridView();
        private DataGridView dgvKpiStaff = new DataGridView();

        // --- REPORT UI CONTROLS ---
        private ComboBox cbReportType = new ComboBox();
        private DateTimePicker dtReportFrom = new DateTimePicker();
        private DateTimePicker dtReportTo = new DateTimePicker();
        private Button btnGenerateReport = new Button();
        private DataGridView dgvReport = new DataGridView();
        private Label lblReportSummary = new Label();

        // --- CONTROLS ---
        private DataGridView gridView = new DataGridView();
        private TextBox txtSearch = new TextBox();
        private Button btnNewRecord = new Button();

        // --- STATE & DATA ---
        private string currentMainTab = "Car Sales and Leads";
        private string currentSubTab = "New Inquiry";
        private List<SalesLead> _allSales = new List<SalesLead>();
        private List<RepairTicket> _allRepairs = new List<RepairTicket>();
        private List<Button> _allAccordionButtons = new List<Button>();

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

            // --- ACCORDION SIDEBAR ---
            FlowLayoutPanel sidebarFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            sidebarFlow.Controls.Add(CreateAccordion("nav_bi", "📊 Business Intelligence",
                new[] { "Dashboard", "KPI", "Reports", "Graphs" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_sales", "🚗 Car Sales and Leads",
                new[] { "New Inquiry", "Test Drive Scheduled", "Negotiation", "Closed Deals" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_service", "🔧 Service and Repair",
                new[] { "New Diagnose", "In Repair", "Waiting for Parts", "Repaired", "Ready for Pickup", "Picked Up" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_promotions", "📢 Promotions",
                new[] { "Active Promos", "Drafts" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_history", "🕒 Customer History",
                new[] { "Interaction Logs" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_archived", "📁 Archived",
                new[] { "Archived Sales", "Archived Repairs" }));

            sidebarPanel.Controls.Add(sidebarFlow);
            sidebarFlow.BringToFront();

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

            lblBreadcrumb.AutoSize = true;
            lblBreadcrumb.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            lblBreadcrumb.ForeColor = Color.FromArgb(17, 24, 39);
            subTabPanel.Controls.Add(lblBreadcrumb);

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

            // Add BI Panels
            BuildDashboardView();
            BuildKpiView();
            BuildReportsView();
            BuildGraphsView();

            mainContentPanel.Controls.Add(panelBI_Dashboard);
            mainContentPanel.Controls.Add(panelBI_KPI);
            mainContentPanel.Controls.Add(panelBI_Reports);
            mainContentPanel.Controls.Add(panelBI_Graphs);
            mainContentPanel.Controls.Add(gridWrapper);
            mainContentPanel.Controls.Add(subTabPanel);
            mainContentPanel.Controls.Add(topActionBar);

            this.Controls.Add(mainContentPanel);
            this.Controls.Add(sidebarPanel);
            mainContentPanel.BringToFront();

            TriggerTabSwitch("Car Sales and Leads", "New Inquiry", null);
        }

        // --- ACCORDION GENERATOR ---
        private Panel CreateAccordion(string id, string mainTitle, string[] subTitles)
        {
            Panel container = new Panel { AutoSize = true, MinimumSize = new Size(270, 45), Width = 270, Margin = new Padding(0) };

            Button btnMain = new Button { Name = id, Text = "  " + mainTitle + " ˅", Width = 270, Height = 45, Dock = DockStyle.Top, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(107, 114, 128), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Cursor = Cursors.Hand };
            btnMain.FlatAppearance.BorderSize = 0;

            Panel subContainer = new Panel { AutoSize = true, Width = 270, Dock = DockStyle.Top, Visible = false };

            string cleanMainTitle = mainTitle.Replace("📊 ", "").Replace("🚗 ", "").Replace("🔧 ", "").Replace("📢 ", "").Replace("🕒 ", "").Replace("📁 ", "").Trim();

            for (int i = subTitles.Length - 1; i >= 0; i--)
            {
                string subTitle = subTitles[i];
                Button btnSub = new Button { Name = id + "_sub_" + i, Text = "      • " + subTitle, Width = 270, Height = 40, Dock = DockStyle.Top, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(156, 163, 175), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(35, 0, 0, 0), Cursor = Cursors.Hand };
                btnSub.FlatAppearance.BorderSize = 0;
                btnSub.Click += (s, e) => TriggerTabSwitch(cleanMainTitle, subTitle, (Button)s);

                _allAccordionButtons.Add(btnSub);
                subContainer.Controls.Add(btnSub);
            }

            btnMain.Click += (s, e) => {
                subContainer.Visible = !subContainer.Visible;
                btnMain.Text = subContainer.Visible ? "  " + mainTitle + " ˄" : "  " + mainTitle + " ˅";
            };

            container.Controls.Add(subContainer);
            container.Controls.Add(btnMain);
            return container;
        }

        private void TriggerTabSwitch(string mainTab, string subTab, Button? selectedBtn)
        {
            currentMainTab = mainTab;
            currentSubTab = subTab;
            lblBreadcrumb.Text = $"{mainTab} > {subTab}";

            foreach (var btn in _allAccordionButtons)
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(156, 163, 175);
            }
            if (selectedBtn != null)
            {
                selectedBtn.BackColor = Color.FromArgb(224, 231, 255);
                selectedBtn.ForeColor = Color.FromArgb(67, 56, 202);
            }

            // Hide everything first
            gridWrapper.Visible = false;
            panelBI_Dashboard.Visible = false;
            panelBI_KPI.Visible = false;
            panelBI_Graphs.Visible = false;
            panelBI_Reports.Visible = false;
            lblPlaceholderMessage.Visible = false;

            if (mainTab == "Business Intelligence")
            {
                topActionBar.Visible = false;

                if (subTab == "Dashboard")
                {
                    panelBI_Dashboard.Visible = true;
                    panelBI_Dashboard.BringToFront();
                    RefreshDashboardMetrics();
                }
                else if (subTab == "KPI")
                {
                    panelBI_KPI.Visible = true;
                    panelBI_KPI.BringToFront();
                    RefreshKpiView();
                }
                else if (subTab == "Graphs")
                {
                    panelBI_Graphs.Visible = true;
                    panelBI_Graphs.BringToFront();
                    RefreshGraphsView();
                }
                else if (subTab == "Reports")
                {
                    panelBI_Reports.Visible = true;
                    panelBI_Reports.BringToFront();
                }
            }
            else
            {
                topActionBar.Visible = true;
                gridWrapper.Visible = true;
                gridWrapper.BringToFront();

                if (mainTab == "Car Sales and Leads" || mainTab == "Service and Repair" || mainTab == "Archived")
                {
                    txtSearch.Visible = true;
                    gridView.Visible = true;
                    btnNewRecord.Visible = (mainTab != "Archived");
                    FilterAndBindGrid();
                }
                else
                {
                    txtSearch.Visible = false;
                    gridView.Visible = false;
                    btnNewRecord.Visible = false;
                    lblPlaceholderMessage.Text = $"{subTab} features are currently under development.";
                    lblPlaceholderMessage.Visible = true;
                }
            }
        }

        // --- DASHBOARD BUILDERS ---
        private void BuildDashboardView()
        {
            panelBI_Dashboard.Dock = DockStyle.Fill;
            panelBI_Dashboard.BackColor = Color.Transparent;

            TableLayoutPanel topCardsGrid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 130, ColumnCount = 4, RowCount = 1, Padding = new Padding(0, 0, 0, 15) };
            for (int i = 0; i < 4; i++) topCardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            topCardsGrid.Controls.Add(CreateStatCard("Active Sales Leads", lblTotalLeads, Color.FromArgb(124, 58, 237)), 0, 0);
            topCardsGrid.Controls.Add(CreateStatCard("Total Pipeline Value", lblPipelineValue, Color.FromArgb(16, 185, 129)), 1, 0);
            topCardsGrid.Controls.Add(CreateStatCard("Lead Conversion Rate", lblConversionRate, Color.FromArgb(59, 130, 246)), 2, 0);
            topCardsGrid.Controls.Add(CreateStatCard("Active Promotions", lblActivePromos, Color.FromArgb(245, 158, 11)), 3, 0);
            lblActivePromos.Text = "0"; // Static for now until Promos entity is built

            TableLayoutPanel bottomSplit = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            bottomSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            bottomSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Panel leftCard = CreateCardPanel();
            AddHeader(leftCard, "Dashboard Overview", "High-level CRM sales performance & active pipelines");
            dgvPipelineSummary = CreateDashboardGridView();
            leftCard.Controls.Add(dgvPipelineSummary);

            Panel rightCard = CreateCardPanel();
            AddHeader(rightCard, "Sales Team Leaderboard", "Top performing salespeople by closed deals");
            dgvTopSalespeople = CreateDashboardGridView();
            rightCard.Controls.Add(dgvTopSalespeople);

            bottomSplit.Controls.Add(leftCard, 0, 0);
            bottomSplit.Controls.Add(rightCard, 1, 0);

            panelBI_Dashboard.Controls.Add(bottomSplit);
            panelBI_Dashboard.Controls.Add(topCardsGrid);
        }

        private Panel CreateStatCard(string title, Label valLabel, Color accentColor)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);
            Label lblT = new Label { Text = title, Left = 15, Top = 15, AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(107, 114, 128) };
            valLabel.Left = 15; valLabel.Top = 45; valLabel.Width = 220; valLabel.Height = 40;
            valLabel.Text = "0"; valLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold); valLabel.ForeColor = accentColor;
            card.Controls.Add(lblT); card.Controls.Add(valLabel);
            return card;
        }

        private Panel CreateCardPanel()
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5), Padding = new Padding(15, 80, 15, 15) };
            p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);
            return p;
        }

        private Panel CreatePlaceholderPanel(string text)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5) };
            p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);
            Label l = new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14F, FontStyle.Italic), ForeColor = Color.FromArgb(156, 163, 175) };
            p.Controls.Add(l);
            return p;
        }

        private void AddHeader(Panel parent, string title, string subtitle)
        {
            Label lblT = new Label { Text = title, Left = 20, Top = 20, AutoSize = true, Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(124, 58, 237) };
            Label lblS = new Label { Text = subtitle, Left = 20, Top = 50, AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(107, 114, 128) };
            parent.Controls.Add(lblT); parent.Controls.Add(lblS);
        }

        private DataGridView CreateDashboardGridView()
        {
            var gv = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, GridColor = Color.FromArgb(243, 244, 246), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, RowHeadersVisible = false, AllowUserToAddRows = false, EnableHeadersVisualStyles = false, RowTemplate = { Height = 35 } };
            gv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(249, 250, 251), ForeColor = Color.FromArgb(75, 85, 99), Font = new Font("Segoe UI", 9F, FontStyle.Bold), Padding = new Padding(10, 5, 10, 5) };
            gv.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI", 9F), SelectionBackColor = Color.FromArgb(237, 233, 254), SelectionForeColor = Color.FromArgb(124, 58, 237), Padding = new Padding(10, 0, 10, 0) };
            return gv;
        }

        private void RefreshDashboardMetrics()
        {
            try
            {
                var activeSales = _allSales.Where(x => x.Status != "Archived").ToList();
                var activeLeads = activeSales.Where(x => x.Status != "Closed Won" && x.Status != "Closed Lost").ToList();

                lblTotalLeads.Text = activeLeads.Count.ToString();
                lblPipelineValue.Text = $"₱{activeLeads.Sum(x => x.EstimatedCost):N2}";

                int closedWon = activeSales.Count(x => x.Status == "Closed Won");
                int closedLost = activeSales.Count(x => x.Status == "Closed Lost");
                int resolvedLeads = closedWon + closedLost;
                lblConversionRate.Text = $"{(resolvedLeads > 0 ? Math.Round(((decimal)closedWon / resolvedLeads) * 100, 1) : 0)}%";

                var pipelineSummary = activeSales
                    .GroupBy(x => string.IsNullOrEmpty(x.Status) ? "New Inquiry" : x.Status)
                    .Select(g => new { SalesStage = g.Key, TotalLeads = g.Count(), PipelineValue = $"₱{g.Sum(item => item.EstimatedCost):N2}" }).ToList();

                dgvPipelineSummary.DataSource = pipelineSummary;

                var leaders = activeSales.Where(x => !string.IsNullOrEmpty(x.HandledBy))
                    .GroupBy(x => x.HandledBy)
                    .Select(g => new { Salesperson = g.Key, ActiveLeads = g.Count(c => c.Status != "Closed Won" && c.Status != "Closed Lost"), DealsWon = g.Count(c => c.Status == "Closed Won") })
                    .OrderByDescending(x => x.DealsWon).ThenByDescending(x => x.ActiveLeads).ToList();

                dgvTopSalespeople.DataSource = leaders;
            }
            catch { }
        }


        // --- KPI VIEW ---
        private void BuildKpiView()
        {
            panelBI_KPI.Dock = DockStyle.Fill;
            panelBI_KPI.BackColor = Color.Transparent;

            TableLayoutPanel cards = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 245,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(0, 0, 0, 10)
            };

            for (int i = 0; i < 4; i++)
                cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            cards.Controls.Add(CreateStatCard("Active Leads", lblKpiActiveLeads, Color.FromArgb(124, 58, 237)), 0, 0);
            cards.Controls.Add(CreateStatCard("Closed Won", lblKpiClosedWon, Color.FromArgb(16, 185, 129)), 1, 0);
            cards.Controls.Add(CreateStatCard("Closed Lost", lblKpiClosedLost, Color.FromArgb(239, 68, 68)), 2, 0);
            cards.Controls.Add(CreateStatCard("Average Closed Deal", lblKpiAverageDeal, Color.FromArgb(59, 130, 246)), 3, 0);
            cards.Controls.Add(CreateStatCard("Open Pipeline", lblKpiPipeline, Color.FromArgb(245, 158, 11)), 0, 1);
            cards.Controls.Add(CreateStatCard("Active Repairs", lblKpiActiveRepairs, Color.FromArgb(124, 58, 237)), 1, 1);
            cards.Controls.Add(CreateStatCard("Repaired", lblKpiRepaired, Color.FromArgb(16, 185, 129)), 2, 1);
            cards.Controls.Add(CreateStatCard("Picked Up", lblKpiPickedUp, Color.FromArgb(59, 130, 246)), 3, 1);

            TableLayoutPanel tables = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 0, 0, 0)
            };
            tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Panel stageCard = CreateCardPanel();
            AddHeader(stageCard, "Sales Stage KPI", "Number of leads in each sales stage");
            dgvKpiStages = CreateDashboardGridView();
            stageCard.Controls.Add(dgvKpiStages);

            Panel staffCard = CreateCardPanel();
            AddHeader(staffCard, "Staff KPI", "Sales activity grouped by handled staff");
            dgvKpiStaff = CreateDashboardGridView();
            staffCard.Controls.Add(dgvKpiStaff);

            tables.Controls.Add(stageCard, 0, 0);
            tables.Controls.Add(staffCard, 1, 0);

            panelBI_KPI.Controls.Add(tables);
            panelBI_KPI.Controls.Add(cards);
        }

        private void RefreshKpiView()
        {
            try
            {
                var activeSales = _allSales.Where(x => x.Status != "Archived").ToList();
                var activeLeads = activeSales.Where(x => x.Status != "Closed Won" && x.Status != "Closed Lost").ToList();
                var closedWon = activeSales.Where(x => x.Status == "Closed Won").ToList();
                var closedLost = activeSales.Where(x => x.Status == "Closed Lost").ToList();

                lblKpiActiveLeads.Text = activeLeads.Count.ToString();
                lblKpiClosedWon.Text = closedWon.Count.ToString();
                lblKpiClosedLost.Text = closedLost.Count.ToString();
                lblKpiAverageDeal.Text = closedWon.Count > 0
                    ? $"₱{closedWon.Average(x => x.EstimatedCost):N2}"
                    : "₱0.00";
                lblKpiPipeline.Text = $"₱{activeLeads.Sum(x => x.EstimatedCost):N2}";

                var activeRepairs = _allRepairs.Where(x => x.Status != "Archived").ToList();
                lblKpiActiveRepairs.Text = activeRepairs.Count.ToString();
                lblKpiRepaired.Text = activeRepairs.Count(x => x.Status == "Repaired").ToString();
                lblKpiPickedUp.Text = activeRepairs.Count(x => x.PickupStatus == "Picked Up").ToString();

                dgvKpiStages.DataSource = activeSales
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New Inquiry" : x.Status)
                    .Select(g => new
                    {
                        SalesStage = g.Key,
                        Leads = g.Count(),
                        Value = $"₱{g.Sum(x => x.EstimatedCost):N2}"
                    })
                    .OrderBy(x => x.SalesStage)
                    .ToList();

                dgvKpiStaff.DataSource = activeSales
                    .Where(x => !string.IsNullOrWhiteSpace(x.HandledBy))
                    .GroupBy(x => x.HandledBy)
                    .Select(g => new
                    {
                        Staff = g.Key,
                        ActiveLeads = g.Count(x => x.Status != "Closed Won" && x.Status != "Closed Lost"),
                        ClosedWon = g.Count(x => x.Status == "Closed Won")
                    })
                    .OrderByDescending(x => x.ClosedWon)
                    .ThenByDescending(x => x.ActiveLeads)
                    .ToList();
            }
            catch
            {
                // Keep the BI page usable if a data row is incomplete.
            }
        }

        // --- REPORTS VIEW ---
        private void BuildReportsView()
        {
            panelBI_Reports.Dock = DockStyle.Fill;
            panelBI_Reports.BackColor = Color.Transparent;

            Panel filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            filterBar.Paint += (s, e) => ControlPaint.DrawBorder(
                e.Graphics,
                filterBar.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);

            cbReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbReportType.Width = 160;
            cbReportType.Items.AddRange(new object[]
            {
                "Sales Report",
                "Lead Report",
                "Staff Performance",
                "Repair Report",
                "Archived Sales",
                "Archived Repairs"
            });
            cbReportType.SelectedIndex = 0;
            cbReportType.Location = new Point(15, 17);

            dtReportFrom.Format = DateTimePickerFormat.Short;
            dtReportFrom.Value = DateTime.Today.AddMonths(-1);
            dtReportFrom.Width = 110;
            dtReportFrom.Location = new Point(190, 17);

            dtReportTo.Format = DateTimePickerFormat.Short;
            dtReportTo.Value = DateTime.Today;
            dtReportTo.Width = 110;
            dtReportTo.Location = new Point(315, 17);

            btnGenerateReport.Text = "Generate Report";
            btnGenerateReport.Width = 135;
            btnGenerateReport.Height = 30;
            btnGenerateReport.Location = new Point(440, 15);
            btnGenerateReport.BackColor = Color.FromArgb(79, 70, 229);
            btnGenerateReport.ForeColor = Color.White;
            btnGenerateReport.FlatStyle = FlatStyle.Flat;
            btnGenerateReport.FlatAppearance.BorderSize = 0;
            btnGenerateReport.Click += (s, e) => GenerateSelectedReport();

            filterBar.Controls.Add(cbReportType);
            filterBar.Controls.Add(dtReportFrom);
            filterBar.Controls.Add(dtReportTo);
            filterBar.Controls.Add(btnGenerateReport);

            Panel reportCard = CreateCardPanel();
            reportCard.Padding = new Padding(15, 55, 15, 45);
            AddHeader(reportCard, "Business Intelligence Reports", "Select a report and date range, then generate the table below.");

            dgvReport = CreateDashboardGridView();
            reportCard.Controls.Add(dgvReport);

            lblReportSummary.AutoSize = false;
            lblReportSummary.Dock = DockStyle.Bottom;
            lblReportSummary.Height = 35;
            lblReportSummary.TextAlign = ContentAlignment.MiddleLeft;
            lblReportSummary.ForeColor = Color.FromArgb(107, 114, 128);
            reportCard.Controls.Add(lblReportSummary);

            panelBI_Reports.Controls.Add(reportCard);
            panelBI_Reports.Controls.Add(filterBar);

            GenerateSelectedReport();
        }

        private void GenerateSelectedReport()
        {
            DateTime from = dtReportFrom.Value.Date;
            DateTime to = dtReportTo.Value.Date.AddDays(1).AddTicks(-1);

            if (from > to)
            {
                MessageBox.Show("The From date cannot be after the To date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string report = cbReportType.SelectedItem?.ToString() ?? "Sales Report";

            if (report == "Sales Report")
            {
                var rows = _allSales
                    .Where(x => x.CreatedAt >= from && x.CreatedAt <= to && x.Status != "Archived")
                    .Select(x => new
                    {
                        ID = x.InquiryId,
                        Customer = ($"{x.FirstName} {x.LastName}").Trim(),
                        Model = x.CarModel,
                        Stage = x.Status,
                        DealValue = x.EstimatedCost,
                        HandledBy = x.HandledBy,
                        Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                    }).ToList();

                dgvReport.DataSource = rows;
                FormatCurrencyColumn("DealValue");
                lblReportSummary.Text = $"Sales records: {rows.Count} | Value: ₱{rows.Sum(x => x.DealValue):N2}";
            }
            else if (report == "Lead Report")
            {
                var rows = _allSales
                    .Where(x => x.CreatedAt >= from && x.CreatedAt <= to && x.Status != "Archived")
                    .Select(x => new
                    {
                        ID = x.InquiryId,
                        Customer = ($"{x.FirstName} {x.LastName}").Trim(),
                        Model = x.CarModel,
                        Stage = x.Status,
                        HandledBy = x.HandledBy,
                        Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                    }).ToList();

                dgvReport.DataSource = rows;
                lblReportSummary.Text = $"Leads: {rows.Count}";
            }
            else if (report == "Staff Performance")
            {
                var rows = _allSales
                    .Where(x => x.CreatedAt >= from && x.CreatedAt <= to && x.Status != "Archived" && !string.IsNullOrWhiteSpace(x.HandledBy))
                    .GroupBy(x => x.HandledBy)
                    .Select(g => new
                    {
                        Staff = g.Key,
                        Leads = g.Count(),
                        ClosedWon = g.Count(x => x.Status == "Closed Won"),
                        ClosedLost = g.Count(x => x.Status == "Closed Lost"),
                        ClosedValue = g.Where(x => x.Status == "Closed Won").Sum(x => x.EstimatedCost)
                    })
                    .OrderByDescending(x => x.ClosedWon)
                    .ToList();

                dgvReport.DataSource = rows;
                FormatCurrencyColumn("ClosedValue");
                lblReportSummary.Text = $"Staff records: {rows.Count}";
            }
            else if (report == "Repair Report")
            {
                var rows = _allRepairs
                    .Where(x => x.CreatedAt >= from && x.CreatedAt <= to && x.Status != "Archived")
                    .Select(x => new
                    {
                        ID = x.TicketId,
                        Customer = ($"{x.FirstName} {x.LastName}").Trim(),
                        Model = x.CarModel,
                        Issue = x.Concern,
                        Status = x.Status,
                        Pickup = x.PickupStatus,
                        EstimatedCost = x.EstimatedCost,
                        HandledBy = x.HandledBy,
                        Date = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                    }).ToList();

                dgvReport.DataSource = rows;
                FormatCurrencyColumn("EstimatedCost");
                lblReportSummary.Text = $"Repair records: {rows.Count} | Estimated cost: ₱{rows.Sum(x => x.EstimatedCost):N2}";
            }
            else if (report == "Archived Sales")
            {
                var rows = _allSales
                    .Where(x => x.Status == "Archived" && x.CompletedAt.HasValue && x.CompletedAt.Value >= from && x.CompletedAt.Value <= to)
                    .Select(x => new
                    {
                        ID = x.InquiryId,
                        Customer = ($"{x.FirstName} {x.LastName}").Trim(),
                        Model = x.CarModel,
                        DealValue = x.EstimatedCost,
                        ArchivedOn = x.CompletedAt.Value.ToLocalTime().ToString("MMM dd, yyyy"),
                        HandledBy = x.HandledBy
                    }).ToList();

                dgvReport.DataSource = rows;
                FormatCurrencyColumn("DealValue");
                lblReportSummary.Text = $"Archived sales: {rows.Count}";
            }
            else
            {
                var rows = _allRepairs
                    .Where(x => x.Status == "Archived" && x.CompletedAt.HasValue && x.CompletedAt.Value >= from && x.CompletedAt.Value <= to)
                    .Select(x => new
                    {
                        ID = x.TicketId,
                        Customer = ($"{x.FirstName} {x.LastName}").Trim(),
                        Model = x.CarModel,
                        Issue = x.Concern,
                        EstimatedCost = x.EstimatedCost,
                        ArchivedOn = x.CompletedAt.Value.ToLocalTime().ToString("MMM dd, yyyy"),
                        HandledBy = x.HandledBy
                    }).ToList();

                dgvReport.DataSource = rows;
                FormatCurrencyColumn("EstimatedCost");
                lblReportSummary.Text = $"Archived repairs: {rows.Count}";
            }
        }

        private void FormatCurrencyColumn(string columnName)
        {
            if (dgvReport.Columns[columnName] != null)
            {
                dgvReport.Columns[columnName].DefaultCellStyle.Format = "₱#,##0.00";
            }
        }

        // --- GRAPHS VIEW ---
        private void BuildGraphsView()
        {
            panelBI_Graphs.Dock = DockStyle.Fill;
            panelBI_Graphs.BackColor = Color.Transparent;

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            layout.Controls.Add(CreateGraphCard("Sales Pipeline by Stage", "graphPipeline"), 0, 0);
            layout.Controls.Add(CreateGraphCard("Closed Deals by Staff", "graphStaff"), 1, 0);
            layout.Controls.Add(CreateGraphCard("Repair Status", "graphRepair"), 0, 1);
            layout.Controls.Add(CreateGraphCard("Sales Value by Month", "graphMonthly"), 1, 1);

            panelBI_Graphs.Controls.Add(layout);
        }

        private Panel CreateGraphCard(string title, string graphName)
        {
            Panel card = CreateCardPanel();
            card.Padding = new Padding(15, 55, 15, 15);
            AddHeader(card, title, "Current CRM data");
            Panel graph = new Panel
            {
                Name = graphName,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };
            card.Controls.Add(graph);
            return card;
        }

        private void RefreshGraphsView()
        {
            foreach (Control card in panelBI_Graphs.Controls)
            {
                if (card is TableLayoutPanel layout)
                {
                    foreach (Control item in layout.Controls)
                    {
                        if (item is Panel graphCard)
                        {
                            Panel? graph = graphCard.Controls.OfType<Panel>().FirstOrDefault();
                            if (graph != null)
                                DrawSimpleBars(graph, GetGraphData(graph.Name));
                        }
                    }
                }
            }
        }

        private List<KeyValuePair<string, decimal>> GetGraphData(string graphName)
        {
            if (graphName == "graphPipeline")
            {
                return _allSales
                    .Where(x => x.Status != "Archived")
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New Inquiry" : x.Status)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(x => x.EstimatedCost)))
                    .ToList();
            }

            if (graphName == "graphStaff")
            {
                return _allSales
                    .Where(x => x.Status == "Closed Won" && !string.IsNullOrWhiteSpace(x.HandledBy))
                    .GroupBy(x => x.HandledBy)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key!, g.Count()))
                    .OrderByDescending(x => x.Value)
                    .ToList();
            }

            if (graphName == "graphRepair")
            {
                return _allRepairs
                    .Where(x => x.Status != "Archived")
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New Diagnose" : x.Status)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count()))
                    .ToList();
            }

            var monthly = _allSales
                .Where(x => x.Status == "Closed Won")
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .TakeLast(6)
                .Select(g => new KeyValuePair<string, decimal>(
                    new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    g.Sum(x => x.EstimatedCost)))
                .ToList();

            return monthly;
        }

        private void DrawSimpleBars(Panel graph, List<KeyValuePair<string, decimal>> data)
        {
            graph.Controls.Clear();

            if (data.Count == 0)
            {
                graph.Controls.Add(new Label
                {
                    Text = "No data available",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(156, 163, 175)
                });
                return;
            }

            decimal max = data.Max(x => x.Value);
            if (max <= 0) max = 1;

            int y = 10;
            foreach (var item in data)
            {
                Label label = new Label
                {
                    Text = item.Key,
                    Location = new Point(5, y),
                    Width = 125,
                    Height = 28,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.FromArgb(75, 85, 99)
                };

                Panel bar = new Panel
                {
                    Location = new Point(135, y + 5),
                    Width = Math.Max(5, (int)((graph.ClientSize.Width - 220) * (double)(item.Value / max))),
                    Height = 18,
                    BackColor = Color.FromArgb(124, 58, 237)
                };

                Label value = new Label
                {
                    Text = item.Value % 1 == 0 ? item.Value.ToString("N0") : $"₱{item.Value:N2}",
                    Location = new Point(bar.Right + 8, y),
                    AutoSize = true,
                    Height = 28,
                    ForeColor = Color.FromArgb(55, 65, 81)
                };

                graph.Controls.Add(label);
                graph.Controls.Add(bar);
                graph.Controls.Add(value);
                y += 40;
            }
        }

        // --- DATA BINDING ---
        private async Task LoadDataFromApiAsync()
        {
            try
            {
                _allSales = await _apiService.GetSalesAsync(CurrentCompanyId) ?? new List<SalesLead>();
                _allRepairs = await _apiService.GetRepairsAsync(CurrentCompanyId) ?? new List<RepairTicket>();

                if (currentMainTab == "Business Intelligence")
                {
                    RefreshDashboardMetrics();
                    RefreshKpiView();
                    RefreshGraphsView();
                }
                else
                {
                    FilterAndBindGrid();
                }
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

        // --- POPUP MODALS ---
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