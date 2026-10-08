using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DriveConnect.domain.Entities;
using DriveConnect.winforms.Services;
using DriveConnect.winforms.Modules.SystemAdministration.Controls;

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
        private Label lblKpiActiveWarranties = new Label();
        private Label lblKpiOpenClaims = new Label();
        private Label lblKpiOpenComplaints = new Label();
        private Label lblKpiScheduledMaintenance = new Label();
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
        private readonly ComboBox cbAnalyticsBranch = new ComboBox();
        private readonly ComboBox cbReportBranch = new ComboBox();
        private readonly Panel analyticsContentPanel = new Panel();
        private bool _analyticsBranchLoading;

        // --- STATE and DATA ---
        private string currentMainTab = "Car Sales and Leads";
        private string currentSubTab = "New Inquiry";
        private List<SalesLead> _allSales = new List<SalesLead>();
        private List<RepairTicket> _allRepairs = new List<RepairTicket>();
        private List<Branch> _allBranches = new List<Branch>();
        private List<Button> _allAccordionButtons = new List<Button>();
        private UserManagementControl? userManagementControl;

        private readonly CrmApiService _apiService = new CrmApiService();
        private const int CurrentCompanyId = 1;

        public CustomerRecordsControl()
        {
            InitializeComponent();
            this.Controls.Clear();
            SetupLightModernUI();
            _ = LoadDataFromApiAsync();
        }

        private bool IsAdminRole()
        {
            return string.Equals(
                UserSession.Role,
                "Admin",
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsStaffRole()
        {
            return string.Equals(
                UserSession.Role,
                "Staff",
                StringComparison.OrdinalIgnoreCase);
        }

        private Image? LoadDriveConnectLogo()
        {
            string logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "driveconnect-logo.png");
            if (!File.Exists(logoPath))
                return null;

            try
            {
                using var source = new Bitmap(logoPath);
                return new Bitmap(source);
            }
            catch
            {
                return null;
            }
        }

        private void SetupLightModernUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 260;
            sidebarPanel.BackColor = Color.FromArgb(255, 255, 255);
            sidebarPanel.Padding = new Padding(0);

            Panel logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 112,
                BackColor = Color.White,
                Padding = new Padding(24, 10, 24, 8)
            };

            var logoImage = LoadDriveConnectLogo();
            if (logoImage != null)
            {
                PictureBox logoBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Image = logoImage,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };
                logoPanel.Controls.Add(logoBox);
            }
            else
            {
                Label fallbackLogo = new Label
                {
                    Text = "DriveConnect CRM",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(17, 24, 39),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                logoPanel.Controls.Add(fallbackLogo);
            }

            // --- ACCORDION SIDEBAR ---
            FlowLayoutPanel sidebarFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            if (IsAdminRole())
            {
                sidebarFlow.Controls.Add(CreateAccordion("nav_bi", "📊 Business Intelligence",
                    new[] { "Analytics", "Reports" }));

                sidebarFlow.Controls.Add(CreateAccordion("nav_users", "👤 Staff Management",
                    new[] { "Manage Staff" }));

                sidebarFlow.Controls.Add(CreateAccordion("nav_branching", "🏢 Branching",
                    new[] { "Manage Branches" }));
            }

            if (IsStaffRole())
            {
                sidebarFlow.Controls.Add(CreateAccordion("nav_sales", "🚗 Car Sales and Leads",
                    new[] { "New Inquiry", "Test Drive Scheduled", "Negotiation", "Closed Deals" }));

                sidebarFlow.Controls.Add(CreateAccordion("nav_service", "🔧 Service and Repair",
                    new[] { "New Diagnose", "In Repair", "Waiting for Parts", "Repaired", "Ready for Pickup", "Picked Up" }));
            }

            sidebarFlow.Controls.Add(CreateAccordion(
                "nav_promotions2",
                "📢 Promotions",
                IsAdminRole()
                    ? new[] { "Pending Approval", "Active Promos" }
                    : new[] { "Active Promos", "Drafts" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_history", "🕒 Customer History",
                new[] { "Customer Profile", "Sales and Lead History", "Service and Repair History", "Interaction History", "Feedback History", "Complaint History", "Warranty History", "Maintenance History" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_feedback", "💬 Feedback",
                new[] { "New Feedback", "Reviewed Feedback" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_complaints", "⚠ Complaints",
                new[] { "New Complaints", "Investigating", "Resolved" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_warranty", "🛡 Warranty",
                new[] { "Active Warranties", "Warranty Claims" }));

            sidebarFlow.Controls.Add(CreateAccordion("nav_maintenance", "🧰 Maintenance",
                new[] { "Scheduled Maintenance", "Completed Maintenance" }));

            if (IsAdminRole())
            {
                sidebarFlow.Controls.Add(CreateAccordion("nav_archived", "📁 Archived",
                    new[] { "Archived Sales", "Archived Repairs" }));
            }

            Panel sidebarBorder = new Panel
            {
                Dock = DockStyle.Right,
                Width = 1,
                BackColor = Color.FromArgb(229, 231, 235)
            };

            // Add the fill area first, then the top logo and right border so the
            // accordion cannot cover the logo area.
            sidebarPanel.Controls.Add(sidebarFlow);
            sidebarPanel.Controls.Add(logoPanel);
            sidebarPanel.Controls.Add(sidebarBorder);

            mainContentPanel.Dock = DockStyle.Fill;
            mainContentPanel.Padding = new Padding(30);

            topActionBar.Dock = DockStyle.Top;
            topActionBar.Height = 50;

            btnNewRecord.Text = "+ New Record";
            btnNewRecord.Width = 140;
            btnNewRecord.Visible = true;
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
            subTabPanel.Height = 70;
            subTabPanel.Padding = new Padding(0, 18, 0, 0);

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

            userManagementControl = new UserManagementControl
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            mainContentPanel.Controls.Add(userManagementControl);

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

            string cleanMainTitle = mainTitle.Replace("📊 ", "").Replace("👤 ", "").Replace("🏢 ", "").Replace("🚗 ", "").Replace("🔧 ", "").Replace("📢 ", "").Replace("🕒 ", "").Replace("💬 ", "").Replace("⚠ ", "").Replace("🛡 ", "").Replace("🧰 ", "").Replace("📁 ", "").Trim();

            for (int i = subTitles.Length - 1; i >= 0; i--)
            {
                string subTitle = subTitles[i];
                Button btnSub = new Button { Name = id + "_sub_" + i, Text = "      • " + subTitle, Width = 270, Height = 40, Dock = DockStyle.Top, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(156, 163, 175), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(35, 0, 0, 0), Cursor = Cursors.Hand };
                btnSub.FlatAppearance.BorderSize = 0;
                btnSub.Click += (s, e) =>
{
    if (s is Button button)
        TriggerTabSwitch(cleanMainTitle, subTitle, button);
};

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
            // Keep the original DriveConnect menu names even if an older UI passes '&' versions.
            mainTab = mainTab
                .Replace("Car Sales & Leads", "Car Sales and Leads")
                .Replace("Service & Repair", "Service and Repair");
            subTab = subTab
                .Replace("Sales & Lead History", "Sales and Lead History")
                .Replace("Service & Repair History", "Service and Repair History");

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
            if (userManagementControl != null)
                userManagementControl.Visible = false;

            if (mainTab == "User Management" || mainTab == "Staff Management")
            {
                if (!IsAdminRole())
                    return;

                topActionBar.Visible = false;
                txtSearch.Visible = false;
                btnNewRecord.Visible = false;
                gridWrapper.Visible = false;
                userManagementControl?.BringToFront();
                if (userManagementControl != null)
                    userManagementControl.Visible = true;
                return;
            }

            if (mainTab == "Branching")
            {
                if (!IsAdminRole())
                    return;

                topActionBar.Visible = true;
                gridWrapper.Visible = true;
                gridWrapper.BringToFront();
                txtSearch.Visible = true;
                txtSearch.PlaceholderText = "Search by branch code or name...";
                gridView.Visible = true;
                btnNewRecord.Visible = true;
                lblPlaceholderMessage.Visible = false;
                BindBranchGrid();
            }
            else if (mainTab == "Business Intelligence")
            {
                if (!IsAdminRole())
                    return;

                topActionBar.Visible = false;
                txtSearch.Visible = false;
                txtSearch.PlaceholderText = "Search by Name, Phone, or Model...";

                if (subTab == "Analytics")
                {
                    panelBI_KPI.Visible = true;
                    panelBI_KPI.BringToFront();
                    RefreshAnalyticsView();
                }
                else if (subTab == "Reports")
                {
                    panelBI_Reports.Visible = true;
                    panelBI_Reports.BringToFront();
                }
            }
            else if (IsAdditionalDataTab(mainTab))
            {
                txtSearch.PlaceholderText = "Search by Name, Phone, or Model...";
                // These are implemented CRM modules, so they must never fall through
                // to the generic "under development" message.
                topActionBar.Visible = true;
                gridWrapper.Visible = true;
                gridWrapper.BringToFront();
                txtSearch.Visible = true;
                gridView.Visible = true;
                btnNewRecord.Visible = CanCreateCurrentTab();
                lblPlaceholderMessage.Visible = false;
                FilterAndBindGrid();
            }
            else
            {
                topActionBar.Visible = mainTab == "Car Sales and Leads" || mainTab == "Service and Repair";
                gridWrapper.Visible = true;
                gridWrapper.BringToFront();

                if (mainTab == "Car Sales and Leads" || mainTab == "Service and Repair" || mainTab == "Archived")
                {
                    txtSearch.Visible = true;
                    gridView.Visible = true;
                    btnNewRecord.Visible = IsStaffRole();
                    lblPlaceholderMessage.Visible = false;
                    FilterAndBindGrid();
                }
                else
                {
                    topActionBar.Visible = false;
                    txtSearch.Visible = false;
                    gridView.Visible = false;
                    btnNewRecord.Visible = false;
                    lblPlaceholderMessage.Text = $"{subTab} features are currently under development.";
                    lblPlaceholderMessage.Visible = true;
                    lblPlaceholderMessage.BringToFront();
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

            TableLayoutPanel bottomSplit = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            bottomSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            bottomSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Panel leftCard = CreateCardPanel();
            AddHeader(leftCard, "Dashboard Overview", "High-level CRM sales performance and active pipelines");
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
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(5),
                Padding = new Padding(10)
            };

            card.Paint += (s, e) => ControlPaint.DrawBorder(
                e.Graphics,
                card.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);

            TableLayoutPanel content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));

            Label lblT = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(107, 114, 128),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            valLabel.Dock = DockStyle.Fill;
            valLabel.Text = "0";
            valLabel.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
            valLabel.ForeColor = accentColor;
            valLabel.TextAlign = ContentAlignment.MiddleLeft;
            valLabel.AutoEllipsis = true;
            valLabel.Margin = new Padding(0);
            valLabel.Padding = new Padding(0);

            content.Controls.Add(lblT, 0, 0);
            content.Controls.Add(valLabel, 0, 1);
            card.Controls.Add(content);

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

                lblActivePromos.Text = _allPromotions.Count(x => x.Status == "Active").ToString();
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
            panelBI_KPI.BackColor = Color.FromArgb(243, 244, 246);
            panelBI_KPI.AutoScroll = true;
            panelBI_KPI.Padding = new Padding(0);
            panelBI_KPI.Controls.Clear();

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 92,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(18, 10, 18, 10),
                Margin = new Padding(0)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));

            var titlePanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            var title = new Label
            {
                Text = "Analytics",
                AutoSize = true,
                Location = new Point(0, 0),
                Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39)
            };
            var subtitle = new Label
            {
                Text = "Interactive KPIs and charts for company and branch performance.",
                AutoSize = true,
                Location = new Point(2, 36),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(107, 114, 128)
            };
            titlePanel.Controls.Add(title);
            titlePanel.Controls.Add(subtitle);

            var branchPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8, 8, 0, 8),
                Margin = new Padding(0)
            };
            branchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82F));
            branchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var scopeLabel = new Label
            {
                Text = "Branch",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = Color.FromArgb(75, 85, 99),
                Margin = new Padding(0, 0, 10, 0)
            };

            cbAnalyticsBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAnalyticsBranch.Dock = DockStyle.Fill;
            cbAnalyticsBranch.Font = new Font("Segoe UI", 9.5F);
            cbAnalyticsBranch.Margin = new Padding(0);
            cbAnalyticsBranch.SelectedIndexChanged -= AnalyticsBranch_SelectedIndexChanged;
            cbAnalyticsBranch.SelectedIndexChanged += AnalyticsBranch_SelectedIndexChanged;

            branchPanel.Controls.Add(scopeLabel, 0, 0);
            branchPanel.Controls.Add(cbAnalyticsBranch, 1, 0);
            header.Controls.Add(titlePanel, 0, 0);
            header.Controls.Add(branchPanel, 1, 0);

            analyticsContentPanel.Dock = DockStyle.Fill;
            analyticsContentPanel.BackColor = Color.FromArgb(243, 244, 246);
            analyticsContentPanel.AutoScroll = true;
            analyticsContentPanel.Padding = new Padding(12);
            analyticsContentPanel.Margin = new Padding(0);

            panelBI_KPI.Controls.Add(analyticsContentPanel);
            panelBI_KPI.Controls.Add(header);
        }

        private void AnalyticsBranch_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_analyticsBranchLoading)
                return;

            RefreshAnalyticsView();
        }

        private void RefreshAnalyticsView()
        {
            if (cbAnalyticsBranch.Items.Count == 0 && _allBranches.Count > 0)
            {
                _analyticsBranchLoading = true;
                cbAnalyticsBranch.Items.Clear();
                cbAnalyticsBranch.Items.Add(new AnalyticsBranchChoice(null, "All Branches"));
                foreach (var branch in _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName))
                    cbAnalyticsBranch.Items.Add(new AnalyticsBranchChoice(branch.BranchId, branch.BranchName));
                cbAnalyticsBranch.SelectedIndex = 0;
                _analyticsBranchLoading = false;
            }

            int? branchId = null;
            if (cbAnalyticsBranch.SelectedItem is AnalyticsBranchChoice selected)
                branchId = selected.Id;

            IEnumerable<SalesLead> sales = _allSales;
            IEnumerable<RepairTicket> repairs = _allRepairs;
            IEnumerable<Feedback> feedback = _allFeedback;
            IEnumerable<Complaint> complaints = _allComplaints;
            IEnumerable<VehicleWarranty> warranties = _allWarranties;
            IEnumerable<WarrantyClaim> claims = _allWarrantyClaims;
            IEnumerable<MaintenanceRecord> maintenance = _allMaintenance;

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

            var scopedSales = sales.Where(x => x.Status != "Archived").ToList();
            var scopedRepairs = repairs.Where(x => x.Status != "Archived").ToList();
            var scopedFeedback = feedback.ToList();
            var scopedComplaints = complaints.ToList();
            var scopedWarranties = warranties.ToList();
            var scopedClaims = claims.ToList();
            var scopedMaintenance = maintenance.ToList();

            var activeLeads = scopedSales.Where(x => x.Status != "Closed Won" && x.Status != "Closed Lost").ToList();
            var closedWon = scopedSales.Where(x => x.Status == "Closed Won").ToList();
            var closedLost = scopedSales.Where(x => x.Status == "Closed Lost").ToList();
            var activeRepairs = scopedRepairs;
            var repaired = activeRepairs.Count(x => x.Status == "Repaired");
            var pickedUp = activeRepairs.Count(x => x.PickupStatus == "Picked Up");
            var activeWarranties = scopedWarranties.Count(x => x.Status == "Active");
            var openClaims = scopedClaims.Count(x => x.Status != "Resolved" && x.Status != "Rejected");
            var openComplaints = scopedComplaints.Count(x => x.Status != "Resolved" && x.Status != "Closed");
            var scheduledMaintenance = scopedMaintenance.Count(x => x.Status == "Scheduled" || x.Status == "Rescheduled");

            analyticsContentPanel.Controls.Clear();

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(4),
                BackColor = Color.FromArgb(243, 244, 246)
            };

            flow.Controls.Add(CreateAnalyticsMetricCard(
                "Sales Pipeline",
                $"{activeLeads.Count:N0} Active Leads",
                $"₱{activeLeads.Sum(x => x.EstimatedCost):N2} Pipeline",
                "Click for sales-stage details",
                Color.FromArgb(124, 58, 237),
                () => ShowAnalyticsDetails("Sales Pipeline", new[]
                {
                    ("Active Leads", activeLeads.Count.ToString("N0")),
                    ("Pipeline Value", $"₱{activeLeads.Sum(x => x.EstimatedCost):N2}"),
                    ("New Inquiry", activeLeads.Count(x => x.Status == "New Inquiry").ToString("N0")),
                    ("Test Drive Scheduled", activeLeads.Count(x => x.Status == "Test Drive Scheduled").ToString("N0")),
                    ("Negotiation", activeLeads.Count(x => x.Status == "Negotiation").ToString("N0"))
                })));

            flow.Controls.Add(CreateAnalyticsMetricCard(
                "Sales Results",
                $"{closedWon.Count:N0} Won • {closedLost.Count:N0} Lost",
                closedWon.Count > 0 ? $"₱{closedWon.Average(x => x.EstimatedCost):N2} Avg Deal" : "₱0.00 Avg Deal",
                "Click for closed-sale details",
                Color.FromArgb(16, 185, 129),
                () => ShowAnalyticsDetails("Sales Results", new[]
                {
                    ("Closed Won", closedWon.Count.ToString("N0")),
                    ("Closed Lost", closedLost.Count.ToString("N0")),
                    ("Average Closed Deal", closedWon.Count > 0 ? $"₱{closedWon.Average(x => x.EstimatedCost):N2}" : "₱0.00"),
                    ("Won Value", $"₱{closedWon.Sum(x => x.EstimatedCost):N2}"),
                    ("Lost Value", $"₱{closedLost.Sum(x => x.EstimatedCost):N2}")
                })));

            flow.Controls.Add(CreateAnalyticsMetricCard(
                "Service Performance",
                $"{activeRepairs.Count:N0} Active Repairs",
                $"{repaired:N0} Repaired • {pickedUp:N0} Picked Up",
                "Click for repair details",
                Color.FromArgb(59, 130, 246),
                () => ShowAnalyticsDetails("Service Performance", new[]
                {
                    ("Active Repairs", activeRepairs.Count.ToString("N0")),
                    ("Repaired", repaired.ToString("N0")),
                    ("Picked Up", pickedUp.ToString("N0")),
                    ("Waiting for Parts", activeRepairs.Count(x => x.Status == "Waiting for Parts").ToString("N0")),
                    ("In Repair", activeRepairs.Count(x => x.Status == "In Repair").ToString("N0"))
                })));

            flow.Controls.Add(CreateAnalyticsMetricCard(
                "After-Sales",
                $"{activeWarranties:N0} Active Warranties",
                $"{openClaims:N0} Claims • {openComplaints:N0} Complaints",
                $"{scheduledMaintenance:N0} Scheduled Maintenance",
                Color.FromArgb(245, 158, 11),
                () => ShowAnalyticsDetails("After-Sales", new[]
                {
                    ("Active Warranties", activeWarranties.ToString("N0")),
                    ("Open Warranty Claims", openClaims.ToString("N0")),
                    ("Open Complaints", openComplaints.ToString("N0")),
                    ("Scheduled Maintenance", scheduledMaintenance.ToString("N0"))
                })));

            var branchRows = BuildBranchPerformanceRows();
            string topBranch = branchRows.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "-";
            flow.Controls.Add(CreateAnalyticsMetricCard(
                "Branch Performance",
                $"{_allBranches.Count(x => x.IsActive):N0} Active Branches",
                $"Top Sales: {topBranch}",
                "Click for branch comparison",
                Color.FromArgb(79, 70, 229),
                () => ShowBranchPerformanceDetails()));

            flow.Controls.Add(CreateAnalyticsChartCard(
                "Sales Pipeline",
                "Funnel shows movement through the active sales process",
                CreateFunnelChart(new[]
                {
                    ("New Inquiry", activeLeads.Count(x => x.Status == "New Inquiry")),
                    ("Test Drive Scheduled", activeLeads.Count(x => x.Status == "Test Drive Scheduled")),
                    ("Negotiation", activeLeads.Count(x => x.Status == "Negotiation")),
                    ("Closed Won", closedWon.Count)
                }),
                () => ShowAnalyticsDetails("Sales Pipeline", new[]
                {
                    ("New Inquiry", activeLeads.Count(x => x.Status == "New Inquiry").ToString("N0")),
                    ("Test Drive Scheduled", activeLeads.Count(x => x.Status == "Test Drive Scheduled").ToString("N0")),
                    ("Negotiation", activeLeads.Count(x => x.Status == "Negotiation").ToString("N0")),
                    ("Closed Won", closedWon.Count.ToString("N0"))
                })));

            flow.Controls.Add(CreateAnalyticsChartCard(
                "Sales Results",
                "Distribution of current sales outcomes",
                CreatePieChart(new[]
                {
                    ("Active", activeLeads.Count),
                    ("Closed Won", closedWon.Count),
                    ("Closed Lost", closedLost.Count)
                }),
                () => ShowAnalyticsDetails("Sales Results", new[]
                {
                    ("Active", activeLeads.Count.ToString("N0")),
                    ("Closed Won", closedWon.Count.ToString("N0")),
                    ("Closed Lost", closedLost.Count.ToString("N0"))
                })));

            flow.Controls.Add(CreateAnalyticsChartCard(
                "Monthly Sales Growth",
                "Closed-won sales value across the last 6 months",
                CreateLineChart(BuildMonthlySalesRows(scopedSales)),
                () => ShowAnalyticsDetails("Monthly Sales Growth", BuildMonthlySalesRows(scopedSales).Select(x => (x.Key, $"₱{x.Value:N2}")))));

            flow.Controls.Add(CreateAnalyticsChartCard(
                "Branch Sales Comparison",
                "Closed-won sales value by active branch",
                CreateHorizontalBarChart(branchRows, true),
                () => ShowBranchPerformanceDetails()));

            var staffRows = scopedSales
                .Where(x => !string.IsNullOrWhiteSpace(x.HandledBy))
                .GroupBy(x => x.HandledBy!)
                .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count(x => x.Status == "Closed Won")))
                .OrderByDescending(x => x.Value)
                .Take(8)
                .ToList();
            flow.Controls.Add(CreateAnalyticsChartCard(
                "Staff Performance",
                "Closed-won deals by handled staff",
                CreateHorizontalBarChart(staffRows, false),
                () => ShowAnalyticsDetails("Staff Performance", staffRows.Select(x => (x.Key, x.Value.ToString("N0"))))));

            var repairStatus = activeRepairs
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New Diagnose" : x.Status)
                .Select(g => (g.Key, g.Count()))
                .ToArray();
            flow.Controls.Add(CreateAnalyticsChartCard(
                "Repair Status",
                "Current distribution of repair tickets",
                CreatePieChart(repairStatus),
                () => ShowAnalyticsDetails("Repair Status", repairStatus.Select(x => (x.Key, x.Item2.ToString("N0"))))));

            var feedbackRows = scopedFeedback
                .GroupBy(x => x.Rating)
                .OrderBy(g => g.Key)
                .Select(g => new KeyValuePair<string, decimal>($"{g.Key} Star", g.Count()))
                .ToList();
            flow.Controls.Add(CreateAnalyticsChartCard(
                "Customer Feedback",
                "Feedback rating distribution",
                CreateColumnChart(feedbackRows),
                () => ShowAnalyticsDetails("Customer Feedback", feedbackRows.Select(x => (x.Key, x.Value.ToString("N0"))))));

            var complaintStatus = scopedComplaints
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New" : x.Status)
                .Select(g => (g.Key, g.Count()))
                .ToArray();
            flow.Controls.Add(CreateAnalyticsChartCard(
                "Complaint Status",
                "Current complaint distribution",
                CreatePieChart(complaintStatus),
                () => ShowAnalyticsDetails("Complaint Status", complaintStatus.Select(x => (x.Key, x.Item2.ToString("N0"))))));

            var warrantyStatus = scopedWarranties
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "Unknown" : x.Status)
                .Select(g => (g.Key, g.Count()))
                .ToArray();
            flow.Controls.Add(CreateAnalyticsChartCard(
                "Warranty Status",
                "Warranty distribution by status",
                CreatePieChart(warrantyStatus),
                () => ShowAnalyticsDetails("Warranty Status", warrantyStatus.Select(x => (x.Key, x.Item2.ToString("N0"))))));

            var maintenanceRows = scopedMaintenance
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "Scheduled" : x.Status)
                .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count()))
                .ToList();
            flow.Controls.Add(CreateAnalyticsChartCard(
                "Maintenance Activity",
                "Maintenance records by status",
                CreateColumnChart(maintenanceRows),
                () => ShowAnalyticsDetails("Maintenance Activity", maintenanceRows.Select(x => (x.Key, x.Value.ToString("N0"))))));

            analyticsContentPanel.Controls.Add(flow);
        }

        private Panel CreateAnalyticsMetricCard(string title, string value, string secondary, string hint, Color accent, Action click)
        {
            var card = new Panel
            {
                Width = 315,
                Height = 142,
                Margin = new Padding(6),
                Padding = new Padding(14),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
            {
                using var border = new Pen(Color.FromArgb(229, 231, 235));
                e.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
                using var accentPen = new Pen(accent, 4);
                e.Graphics.DrawLine(accentPen, 0, 0, 0, card.Height);
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Color.FromArgb(75, 85, 99),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Margin = new Padding(0)
            };
            var lblValue = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = false,
                Margin = new Padding(0)
            };
            var lblSecondary = new Label
            {
                Text = secondary,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(55, 65, 81),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Margin = new Padding(0)
            };
            var lblHint = new Label
            {
                Text = hint,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(156, 163, 175),
                TextAlign = ContentAlignment.BottomLeft,
                AutoEllipsis = true,
                Margin = new Padding(0)
            };

            layout.Controls.Add(lblTitle, 0, 0);
            layout.Controls.Add(lblValue, 0, 1);
            layout.Controls.Add(lblSecondary, 0, 2);
            layout.Controls.Add(lblHint, 0, 3);
            card.Controls.Add(layout);
            WireAnalyticsClick(card, click);
            return card;
        }

        private Panel CreateAnalyticsChartCard(string title, string subtitle, Control chart, Action click)
        {
            var card = new Panel
            {
                Width = 520,
                Height = 340,
                Margin = new Padding(6),
                Padding = new Padding(12),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);

            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(79, 70, 229),
                AutoEllipsis = true,
                Margin = new Padding(0)
            };
            var lblSubtitle = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(107, 114, 128),
                AutoEllipsis = true,
                Margin = new Padding(0)
            };
            titlePanel.Controls.Add(lblSubtitle);
            titlePanel.Controls.Add(lblTitle);

            chart.Dock = DockStyle.Fill;
            chart.Margin = new Padding(0);
            chart.BackColor = Color.White;

            card.Controls.Add(chart);
            card.Controls.Add(titlePanel);
            WireAnalyticsClick(card, click);
            return card;
        }

        private void WireAnalyticsClick(Control root, Action click)
        {
            root.Click += (s, e) => click();
            foreach (Control child in root.Controls)
            {
                child.Cursor = Cursors.Hand;
                WireAnalyticsClick(child, click);
            }
        }

        private Panel CreateFunnelChart(IEnumerable<(string Label, int Value)> source)
        {
            var data = source.ToList();
            var p = new Panel { BackColor = Color.White };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.Clear(Color.White);
                if (data.Count == 0) { DrawNoData(e.Graphics, p.ClientSize); return; }

                int max = Math.Max(1, data.Max(x => x.Value));
                int labelW = 150;
                int valueW = 48;
                int availableBar = Math.Max(80, p.ClientSize.Width - labelW - valueW - 24);
                int rowH = Math.Max(36, (p.ClientSize.Height - 12) / data.Count);
                Color[] fills =
                {
                    Color.FromArgb(124,58,237),
                    Color.FromArgb(139,92,246),
                    Color.FromArgb(168,85,247),
                    Color.FromArgb(192,132,252)
                };

                for (int i = 0; i < data.Count; i++)
                {
                    int y = 6 + i * rowH;
                    int width = Math.Max(data[i].Value > 0 ? 8 : 0, (int)(availableBar * (data[i].Value / (double)max)));

                    TextRenderer.DrawText(
                        e.Graphics,
                        data[i].Label,
                        new Font("Segoe UI", 8.5F),
                        new Rectangle(0, y, labelW - 8, 28),
                        Color.FromArgb(55,65,81),
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                    if (width > 0)
                    {
                        using var brush = new SolidBrush(fills[i % fills.Length]);
                        e.Graphics.FillRectangle(brush, labelW, y + 4, width, 20);
                    }

                    TextRenderer.DrawText(
                        e.Graphics,
                        data[i].Value.ToString("N0"),
                        new Font("Segoe UI", 8.5F, FontStyle.Bold),
                        new Rectangle(labelW + availableBar + 8, y, valueW, 28),
                        Color.FromArgb(55,65,81),
                        TextFormatFlags.VerticalCenter);
                }
            };
            return p;
        }

        private Panel CreatePieChart(IEnumerable<(string Label, int Value)> source)
        {
            var data = source.Where(x => x.Value > 0).ToList();
            var p = new Panel { BackColor = Color.White };
            p.Paint += (s,e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.Clear(Color.White);
                if (data.Count == 0) { DrawNoData(e.Graphics,p.ClientSize); return; }
                int total = Math.Max(1,data.Sum(x=>x.Value));
                int size = Math.Min(p.ClientSize.Height-28, 160);
                var pieRect = new Rectangle(18, Math.Max(18,(p.ClientSize.Height-size)/2), size, size);
                Color[] colors={Color.FromArgb(124,58,237),Color.FromArgb(16,185,129),Color.FromArgb(239,68,68),Color.FromArgb(59,130,246),Color.FromArgb(245,158,11),Color.FromArgb(107,114,128)};
                float start=0;
                for(int i=0;i<data.Count;i++)
                {
                    float sweep=360f*data[i].Value/total;
                    using var b=new SolidBrush(colors[i%colors.Length]);
                    e.Graphics.FillPie(b,pieRect,start,sweep);
                    start+=sweep;
                }
                int y=18;
                for(int i=0;i<data.Count;i++)
                {
                    using var b=new SolidBrush(colors[i%colors.Length]);
                    e.Graphics.FillRectangle(b,pieRect.Right+22,y,12,12);
                    TextRenderer.DrawText(e.Graphics,$"{data[i].Label}  {data[i].Value:N0}",new Font("Segoe UI",8.5F),new Rectangle(pieRect.Right+42,y-4,p.ClientSize.Width-pieRect.Right-45,24),Color.FromArgb(55,65,81),TextFormatFlags.VerticalCenter);
                    y+=28;
                }
            };
            return p;
        }

        private Panel CreateLineChart(List<KeyValuePair<string, decimal>> data)
        {
            var p=new Panel{BackColor=Color.White};
            p.Paint+=(s,e)=>
            {
                e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.Clear(Color.White);
                if(data.Count==0){DrawNoData(e.Graphics,p.ClientSize);return;}
                var plot=new Rectangle(42,18,Math.Max(120,p.ClientSize.Width-68),Math.Max(100,p.ClientSize.Height-52));
                decimal max=data.Max(x=>x.Value); if(max<=0)max=1;
                using var axis=new Pen(Color.FromArgb(209,213,219)); e.Graphics.DrawLine(axis,plot.Left,plot.Bottom,plot.Right,plot.Bottom); e.Graphics.DrawLine(axis,plot.Left,plot.Top,plot.Left,plot.Bottom);
                using var pen=new Pen(Color.FromArgb(124,58,237),3);
                PointF? prev=null;
                for(int i=0;i<data.Count;i++)
                {
                    float x=plot.Left+(data.Count==1?0:(plot.Width-10f)*i/(data.Count-1));
                    float y=plot.Bottom-(float)(data[i].Value/max)*(plot.Height-10);
                    if(prev.HasValue)e.Graphics.DrawLine(pen,prev.Value,new PointF(x,y));
                    using var dot=new SolidBrush(Color.FromArgb(124,58,237)); e.Graphics.FillEllipse(dot,x-4,y-4,8,8);
                    TextRenderer.DrawText(e.Graphics,data[i].Key,new Font("Segoe UI",7.5F),new Rectangle((int)x-35,plot.Bottom+4,70,20),Color.FromArgb(75,85,99),TextFormatFlags.HorizontalCenter);
                    prev=new PointF(x,y);
                }
            };
            return p;
        }

        private Panel CreateHorizontalBarChart(List<KeyValuePair<string, decimal>> data, bool currency)
        {
            var p = new Panel { BackColor = Color.White };
            p.Paint += (s, e) =>
            {
                e.Graphics.Clear(Color.White);
                if (data.Count == 0)
                {
                    DrawNoData(e.Graphics, p.ClientSize);
                    return;
                }

                decimal max = Math.Max(1, data.Max(x => x.Value));
                int rowH = Math.Max(28, (p.ClientSize.Height - 10) / Math.Max(1, data.Count));
                int labelW = 130;
                int valueW = 120;
                int gap = 10;
                int barAreaW = Math.Max(50, p.ClientSize.Width - labelW - valueW - gap);

                for (int i = 0; i < data.Count; i++)
                {
                    int y = 8 + i * rowH;
                    int barW = data[i].Value <= 0
                        ? 0
                        : Math.Max(5, (int)(barAreaW * (double)(data[i].Value / max)));

                    TextRenderer.DrawText(
                        e.Graphics,
                        data[i].Key,
                        new Font("Segoe UI", 8.5F),
                        new Rectangle(0, y, labelW - 8, rowH),
                        Color.FromArgb(75, 85, 99),
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                    if (barW > 0)
                    {
                        using var b = new SolidBrush(Color.FromArgb(124, 58, 237));
                        e.Graphics.FillRectangle(b, labelW, y + 7, barW, 16);
                    }

                    string val = currency ? $"₱{data[i].Value:N0}" : data[i].Value.ToString("N0");
                    TextRenderer.DrawText(
                        e.Graphics,
                        val,
                        new Font("Segoe UI", 8.5F, FontStyle.Bold),
                        new Rectangle(labelW + barAreaW + gap, y, valueW, rowH),
                        Color.FromArgb(55, 65, 81),
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            };
            return p;
        }

        private Panel CreateColumnChart(List<KeyValuePair<string, decimal>> data)
        {
            var p=new Panel{BackColor=Color.White};
            p.Paint+=(s,e)=>
            {
                e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.Clear(Color.White);
                if(data.Count==0){DrawNoData(e.Graphics,p.ClientSize);return;}
                decimal max=Math.Max(1,data.Max(x=>x.Value)); var plot=new Rectangle(36,12,Math.Max(140,p.ClientSize.Width-52),Math.Max(120,p.ClientSize.Height-50));
                using var axis=new Pen(Color.FromArgb(209,213,219)); e.Graphics.DrawLine(axis,plot.Left,plot.Bottom,plot.Right,plot.Bottom); e.Graphics.DrawLine(axis,plot.Left,plot.Top,plot.Left,plot.Bottom);
                int gap=12; int barW=Math.Max(18,(plot.Width-gap*Math.Max(0,data.Count-1)-10)/Math.Max(1,data.Count));
                for(int i=0;i<data.Count;i++)
                {
                    int h=(int)((plot.Height-14)*(double)(data[i].Value/max));
                    int x=plot.Left+8+i*(barW+gap); int y=plot.Bottom-h;
                    using var b=new SolidBrush(Color.FromArgb(124,58,237)); e.Graphics.FillRectangle(b,x,y,barW,h);
                    TextRenderer.DrawText(e.Graphics,data[i].Value.ToString("N0"),new Font("Segoe UI",7.5F,FontStyle.Bold),new Rectangle(x,y-20,barW,18),Color.FromArgb(55,65,81),TextFormatFlags.HorizontalCenter);
                    TextRenderer.DrawText(e.Graphics,data[i].Key,new Font("Segoe UI",7.5F),new Rectangle(x,plot.Bottom+4,barW,30),Color.FromArgb(75,85,99),TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak);
                }
            };
            return p;
        }

        private void DrawNoData(Graphics g, Size size)
        {
            TextRenderer.DrawText(g,"No data available",new Font("Segoe UI",9F,FontStyle.Italic),new Rectangle(Point.Empty,size),Color.FromArgb(156,163,175),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
        }

        private List<KeyValuePair<string, decimal>> BuildMonthlySalesRows(IEnumerable<SalesLead> source)
        {
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
            var rows = new List<KeyValuePair<string, decimal>>();
            for(int i=0;i<6;i++)
            {
                var month = monthStart.AddMonths(i);
                decimal value = source.Where(x => x.Status == "Closed Won" && x.CreatedAt.ToLocalTime().Year == month.Year && x.CreatedAt.ToLocalTime().Month == month.Month).Sum(x => x.EstimatedCost);
                rows.Add(new KeyValuePair<string, decimal>(month.ToString("MMM"),value));
            }
            return rows;
        }

        private List<KeyValuePair<string, decimal>> BuildBranchPerformanceRows()
        {
            return _allBranches
                .Where(x => x.IsActive)
                .OrderBy(x => x.BranchName)
                .Select(branch => new KeyValuePair<string, decimal>(
                    branch.BranchName,
                    _allSales.Where(x => x.BranchId == branch.BranchId && x.Status == "Closed Won").Sum(x => x.EstimatedCost)))
                .ToList();
        }

        private void ShowBranchPerformanceDetails()
        {
            var rows = _allBranches
                .Where(x => x.IsActive)
                .OrderBy(x => x.BranchName)
                .Select(branch =>
                {
                    var sales = _allSales.Where(x => x.BranchId == branch.BranchId && x.Status != "Archived").ToList();
                    int won = sales.Count(x => x.Status == "Closed Won");
                    int lost = sales.Count(x => x.Status == "Closed Lost");
                    decimal value = sales.Where(x => x.Status == "Closed Won").Sum(x => x.EstimatedCost);
                    decimal conversion = won + lost == 0 ? 0 : Math.Round((decimal)won / (won + lost) * 100, 1);
                    return (branch.BranchName, $"{sales.Count:N0} leads | {won:N0} won | {lost:N0} lost | ₱{value:N2} | {conversion:N1}% conversion");
                })
                .ToList();
            ShowAnalyticsDetails("Branch Performance", rows);
        }

        private void ShowAnalyticsDetails(string title, IEnumerable<(string Label, string Value)> rows)
        {
            using var form = new Form
            {
                Text = title,
                ClientSize = new Size(720, 430),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            var heading = new Label
            {
                Text = title,
                AutoSize = true,
                Location = new Point(24, 20),
                Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(79, 70, 229)
            };
            form.Controls.Add(heading);

            var grid = CreateDashboardGridView();
            grid.Location = new Point(24, 62);
            grid.Size = new Size(672, 300);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grid.DataSource = rows.Select(x => new { Metric = x.Label, Value = x.Value }).ToList();
            form.Controls.Add(grid);

            var close = new Button
            {
                Text = "Close",
                Width = 100,
                Height = 34,
                Location = new Point(596, 375),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            close.FlatAppearance.BorderSize = 0;
            form.Controls.Add(close);
            form.AcceptButton = close;
            form.ShowDialog(FindForm());
        }

        private sealed record AnalyticsBranchChoice(int? Id, string Name)
        {
            public override string ToString() => Name;
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
                lblKpiActiveWarranties.Text = _allWarranties.Count(x => x.Status == "Active").ToString();
                lblKpiOpenClaims.Text = _allWarrantyClaims.Count(x => x.Status != "Resolved" && x.Status != "Rejected").ToString();
                lblKpiOpenComplaints.Text = _allComplaints.Count(x => x.Status != "Resolved" && x.Status != "Closed").ToString();
                lblKpiScheduledMaintenance.Text = _allMaintenance.Count(x => x.Status == "Scheduled" || x.Status == "Rescheduled").ToString();

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
            panelBI_Reports.BackColor = Color.FromArgb(243, 244, 246);
            panelBI_Reports.Padding = new Padding(20);
            panelBI_Reports.Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.FromArgb(243, 244, 246)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(18, 10, 18, 8),
                Margin = new Padding(0, 0, 0, 8)
            };
            header.Paint += (s, e) => ControlPaint.DrawBorder(
                e.Graphics,
                header.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);

            header.Controls.Add(new Label
            {
                Text = "Business Intelligence Reports",
                AutoSize = true,
                Location = new Point(18, 10),
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(79, 70, 229)
            });

            header.Controls.Add(new Label
            {
                Text = "Select a report, branch, and date range, then generate the management report preview.",
                AutoSize = true,
                Location = new Point(20, 39),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(107, 114, 128)
            });

            var filterBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Height = 54,
                BackColor = Color.White,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(12, 10, 12, 8),
                Margin = new Padding(0, 0, 0, 8)
            };
            filterBar.Paint += (s, e) => ControlPaint.DrawBorder(
                e.Graphics,
                filterBar.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);

            cbReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbReportType.Width = 180;
            cbReportType.Height = 34;
            cbReportType.Margin = new Padding(0, 2, 10, 0);
            cbReportType.Items.Clear();
            cbReportType.Items.AddRange(new object[]
            {
                "Sales Report",
                "Lead Report",
                "Staff Performance",
                "Repair Report",
                "Feedback Report",
                "Complaint Report",
                "Warranty Report",
                "Maintenance Report",
                "Archived Sales",
                "Archived Repairs"
            });
            cbReportType.SelectedIndex = 0;

            cbReportBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cbReportBranch.Width = 180;
            cbReportBranch.Height = 34;
            cbReportBranch.Margin = new Padding(0, 2, 10, 0);
            RefreshReportBranchOptions();
            cbReportBranch.BackColor = Color.White;
            cbReportBranch.ForeColor = Color.FromArgb(55, 65, 81);

            dtReportFrom.Format = DateTimePickerFormat.Short;
            dtReportFrom.Width = 120;
            dtReportFrom.Height = 34;
            dtReportFrom.Value = DateTime.Today.AddMonths(-1);
            dtReportFrom.Margin = new Padding(0, 2, 10, 0);

            dtReportTo.Format = DateTimePickerFormat.Short;
            dtReportTo.Width = 120;
            dtReportTo.Height = 34;
            dtReportTo.Value = DateTime.Today;
            dtReportTo.Margin = new Padding(0, 2, 10, 0);

            btnGenerateReport.Text = "Generate & Preview";
            btnGenerateReport.Width = 150;
            btnGenerateReport.Height = 34;
            btnGenerateReport.Margin = new Padding(0, 2, 0, 0);
            btnGenerateReport.BackColor = Color.FromArgb(79, 70, 229);
            btnGenerateReport.ForeColor = Color.White;
            btnGenerateReport.FlatStyle = FlatStyle.Flat;
            btnGenerateReport.FlatAppearance.BorderSize = 0;
            btnGenerateReport.Font = new Font("Segoe UI Semibold", 9F);
            btnGenerateReport.Cursor = Cursors.Hand;
            btnGenerateReport.Click -= (s, e) => GenerateSelectedReport();
            btnGenerateReport.Click += (s, e) => GenerateSelectedReport();

            filterBar.Controls.Add(cbReportType);
            filterBar.Controls.Add(cbReportBranch);
            filterBar.Controls.Add(dtReportFrom);
            filterBar.Controls.Add(dtReportTo);
            filterBar.Controls.Add(btnGenerateReport);

            var instructionCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0),
                Padding = new Padding(25)
            };
            instructionCard.Paint += (s, e) => ControlPaint.DrawBorder(
                e.Graphics,
                instructionCard.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);

            var instructionLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            instructionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            instructionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            instructionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            instructionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            instructionLayout.Controls.Add(new Panel(), 0, 0);
            instructionLayout.Controls.Add(new Label
            {
                Text = "Generate a Management Report",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Margin = new Padding(0)
            }, 0, 1);
            instructionLayout.Controls.Add(new Label
            {
                Text = "The full report opens in a separate preview window with KPIs, charts, key findings, detailed records, and Export PDF.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(107, 114, 128),
                Margin = new Padding(0)
            }, 0, 2);
            instructionLayout.Controls.Add(new Panel(), 0, 3);

            instructionCard.Controls.Add(instructionLayout);

            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(filterBar, 0, 1);
            layout.Controls.Add(instructionCard, 0, 2);

            panelBI_Reports.Controls.Add(layout);
        }

        private void RefreshReportBranchOptions()
        {
            var currentId = (cbReportBranch.SelectedItem as ReportBranchChoice)?.Id;
            cbReportBranch.Items.Clear();
            cbReportBranch.Items.Add(new ReportBranchChoice(null, "All Branches"));

            foreach (var branch in _allBranches.Where(x => x.IsActive).OrderBy(x => x.BranchName))
                cbReportBranch.Items.Add(new ReportBranchChoice(branch.BranchId, branch.BranchName));

            var selected = cbReportBranch.Items
                .OfType<ReportBranchChoice>()
                .FirstOrDefault(x => x.Id == currentId);

            cbReportBranch.SelectedItem = selected ?? cbReportBranch.Items[0];
        }

        private void GenerateSelectedReport()
        {
            DateTime from = dtReportFrom.Value.Date;
            DateTime to = dtReportTo.Value.Date;

            if (from > to)
            {
                MessageBox.Show("The From date cannot be after the To date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reportType = cbReportType.SelectedItem?.ToString() ?? "Sales Report";
            int? branchId = cbReportBranch.SelectedItem is ReportBranchChoice choice ? choice.Id : null;
            var report = BuildManagementReport(reportType, from, to, branchId);

            using var preview = new DriveConnect.winforms.Report.ReportPreviewForm(report);
            preview.ShowDialog(FindForm());
        }

        private void FormatCurrencyColumn(string columnName)
        {
            var column = dgvReport.Columns[columnName];
            if (column != null)
            {
                column.DefaultCellStyle.Format = "₱#,##0.00";
            }
        }

        // --- GRAPHS VIEW ---
        private void BuildGraphsView()
        {
            panelBI_Graphs.Dock = DockStyle.Fill;
            panelBI_Graphs.BackColor = Color.FromArgb(243, 244, 246);
            panelBI_Graphs.AutoScroll = true;
            panelBI_Graphs.Padding = new Padding(8);
            panelBI_Graphs.Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 4,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 4; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 280F));

            layout.Controls.Add(CreateGraphCard("Sales Pipeline by Stage", "graphPipeline"), 0, 0);
            layout.Controls.Add(CreateGraphCard("Closed Deals by Staff", "graphStaff"), 1, 0);
            layout.Controls.Add(CreateGraphCard("Repair Status", "graphRepair"), 0, 1);
            layout.Controls.Add(CreateGraphCard("Sales Value by Month", "graphMonthly"), 1, 1);
            layout.Controls.Add(CreateGraphCard("Customer Feedback Ratings", "graphFeedback"), 0, 2);
            layout.Controls.Add(CreateGraphCard("Complaint Status", "graphComplaint"), 1, 2);
            layout.Controls.Add(CreateGraphCard("Warranty Status", "graphWarranty"), 0, 3);
            layout.Controls.Add(CreateGraphCard("Maintenance Status", "graphMaintenance"), 1, 3);

            panelBI_Graphs.Controls.Add(layout);
        }

        private Panel CreateGraphCard(string title, string graphName)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                Padding = new Padding(12),
                BackColor = Color.White
            };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(229,231,235), ButtonBorderStyle.Solid);

            var header = new Panel { Dock = DockStyle.Top, Height = 48, Margin = new Padding(0) };
            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(79,70,229),
                AutoEllipsis = true
            };
            var subtitle = new Label
            {
                Text = "Current CRM data",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(107,114,128),
                AutoEllipsis = true
            };
            header.Controls.Add(subtitle);
            header.Controls.Add(titleLabel);

            Panel graph = new Panel
            {
                Name = graphName,
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true,
                Margin = new Padding(0)
            };
            graph.Resize += (s, e) =>
            {
                if (graph.ClientSize.Width > 0 && graph.ClientSize.Height > 0)
                    DrawSimpleBars(graph, GetGraphData(graph.Name));
            };

            card.Controls.Add(graph);
            card.Controls.Add(header);
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

            if (graphName == "graphFeedback")
            {
                return _allFeedback
                    .GroupBy(x => x.Rating)
                    .OrderBy(g => g.Key)
                    .Select(g => new KeyValuePair<string, decimal>($"{g.Key} Star", g.Count()))
                    .ToList();
            }

            if (graphName == "graphComplaint")
            {
                return _allComplaints
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "New" : x.Status)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count()))
                    .ToList();
            }

            if (graphName == "graphWarranty")
            {
                return _allWarranties
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "Unknown" : x.Status)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Count()))
                    .ToList();
            }

            if (graphName == "graphMaintenance")
            {
                return _allMaintenance
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.Status) ? "Scheduled" : x.Status)
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
            bool currencyGraph = graph.Name == "graphPipeline" || graph.Name == "graphMonthly";

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
                    Text = currencyGraph ? $"₱{item.Value:N2}" : item.Value.ToString("N0"),
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

            graph.AutoScrollMinSize = new Size(0, y + 10);
        }

        // --- DATA BINDING ---
        private async Task LoadDataFromApiAsync()
        {
            try
            {
                _allSales = await _apiService.GetSalesAsync(CurrentCompanyId) ?? new List<SalesLead>();
                _allRepairs = await _apiService.GetRepairsAsync(CurrentCompanyId) ?? new List<RepairTicket>();
                await LoadBranchesAsync();
                RefreshReportBranchOptions();
                await LoadAdditionalDataAsync();

                if (currentMainTab == "Business Intelligence")
                {
                    if (currentSubTab == "Analytics")
                        RefreshAnalyticsView();
                    else
                    {
                        RefreshDashboardMetrics();
                        RefreshKpiView();
                        RefreshGraphsView();
                    }
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

            if (currentMainTab == "Branching")
            {
                BindBranchGrid();
                return;
            }

            if (IsAdditionalDataTab(currentMainTab))
            {
                BindAdditionalGrid();
                return;
            }

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

                var dateAddedColumn = gridView.Columns["DateAdded"];
                if (dateAddedColumn != null) dateAddedColumn.HeaderText = "Date and Time Added";
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

                var dateAddedColumn = gridView.Columns["DateAdded"];
                if (dateAddedColumn != null) dateAddedColumn.HeaderText = "Date and Time Added";
                var valueColumn = gridView.Columns["Value"];
                if (valueColumn != null) valueColumn.HeaderText = "Estimated Cost";
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

                var archivedOnColumn = gridView.Columns["ArchivedOn"];
                if (archivedOnColumn != null) archivedOnColumn.HeaderText = "Date Archived";
                var timeColumn = gridView.Columns["Time"];
                if (timeColumn != null) timeColumn.HeaderText = "Time";
                var valueHeaderColumn = gridView.Columns["Value"];
                if (valueHeaderColumn != null) valueHeaderColumn.HeaderText = "Estimated Cost";
            }
        }

        private async void BtnNewRecord_Click(object? sender, EventArgs e)
        {
            if (IsAdminRole() &&
                currentMainTab != "Branching")
            {
                return;
            }

            if (currentMainTab == "Branching")
            {
                await ShowBranchModalAsync(null);
            }
            else if (currentMainTab == "Car Sales and Leads") ShowSalesModal(null);
            else if (currentMainTab == "Service and Repair") ShowRepairModal(null);
            else if (CanCreateCurrentTab() && currentMainTab == "Promotions") await ShowPromotionModalAsync(null);
            else if (CanCreateCurrentTab() && currentMainTab == "Customer History" && currentSubTab == "Interaction History") await ShowInteractionModalAsync(null);
            else if (CanCreateCurrentTab() && currentMainTab == "Feedback") await ShowFeedbackModalAsync(null);
            else if (CanCreateCurrentTab() && currentMainTab == "Complaints") await ShowComplaintModalAsync(null);
            else if (CanCreateCurrentTab() && currentMainTab == "Warranty")
            {
                if (currentSubTab == "Active Warranties") await ShowWarrantyModalAsync(null);
                else await ShowWarrantyClaimModalAsync(null);
            }
            else if (CanCreateCurrentTab() && currentMainTab == "Maintenance") await ShowMaintenanceModalAsync(null);
        }

        private async void GridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (IsAdminRole() &&
                (currentMainTab == "Car Sales and Leads" ||
                 currentMainTab == "Service and Repair"))
            {
                return;
            }

            if (currentMainTab == "Branching")
            {
                var branchIdValue = gridView.Rows[e.RowIndex].Cells["ID"].Value;
                if (branchIdValue == null)
                    return;

                int branchId = Convert.ToInt32(branchIdValue);
                await ShowBranchModalAsync(_allBranches.FirstOrDefault(x => x.BranchId == branchId));
                return;
            }

            if (IsAdditionalDataTab(currentMainTab))
            {
                if (gridView.Columns["ID"] == null || gridView.Rows[e.RowIndex].Cells["ID"].Value == null) return;
                int additionalId = Convert.ToInt32(gridView.Rows[e.RowIndex].Cells["ID"].Value);
                await HandleAdditionalRecordDoubleClickAsync(additionalId);
                return;
            }

            var idValue = gridView.Rows[e.RowIndex].Cells["ID"].Value;
            if (idValue == null)
                return;

            int id = Convert.ToInt32(idValue);

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

                bool archive = existing != null && existing.Status != "Archived" && cbS.SelectedItem?.ToString() == "Archived";
                if (!ConfirmAction(existing == null ? "Confirm adding this lead?" : archive ? "Confirm archiving this lead?" : "Confirm updating this lead?")) return;

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

                var response = existing == null
                    ? await _apiService.CreateSalesAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdateSalesAsync(CurrentCompanyId, payload.InquiryId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The sales lead could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

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

                bool archive = existing != null && existing.Status != "Archived" && cbS.SelectedItem?.ToString() == "Archived";
                if (!ConfirmAction(existing == null ? "Confirm adding this repair ticket?" : archive ? "Confirm archiving this repair ticket?" : "Confirm updating this repair ticket?")) return;

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

                var response = existing == null
                    ? await _apiService.CreateRepairAsync(CurrentCompanyId, payload)
                    : await _apiService.UpdateRepairAsync(CurrentCompanyId, payload.TicketId, payload);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("The repair ticket could not be saved.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

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