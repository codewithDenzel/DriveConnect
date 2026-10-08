using System.Drawing;
using System.Windows.Forms;
using DriveConnect.winforms.Services;

namespace DriveConnect.winforms.Modules.SystemAdministration.Controls;

public sealed class SuperAdminControl : UserControl
{
    private readonly SuperAdminApiService _api = new();
    private readonly Panel _workspace = new();
    private readonly Panel _subscriptionPanel = new();
    private readonly Panel _companiesPanel = new();
    private readonly Panel _statusPanel = new();
    private readonly UserManagementControl _adminAccountsControl = new();

    private readonly List<Button> _navButtons = new();
    private Button? _activeNavButton;

    private Label _pageTitle = new();
    private Label _pageSubtitle = new();
    private Button _refreshButton = new();

    private Label _planValue = new();
    private Label _feeValue = new();
    private Label _startValue = new();
    private Label _endValue = new();
    private Label _subscriptionStatusValue = new();

    private Label _companyCountValue = new();
    private Label _activeCompanyCountValue = new();
    private Label _activeSubscriptionCountValue = new();
    private Label _systemStatusValue = new();

    private Label _statusValueLarge = new();
    private Label _statusService = new();
    private Label _statusRole = new();
    private Label _statusUser = new();
    private Label _statusUpdated = new();

    private DataGridView _companiesGrid = new();

    public SuperAdminControl()
    {
        BuildUi();
        _ = LoadDashboardAsync();
    }

    private void BuildUi()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(243, 244, 246);
        Font = new Font("Segoe UI", 10F);

        var sidebar = BuildSidebar();

        _workspace.Dock = DockStyle.Fill;
        _workspace.BackColor = Color.FromArgb(243, 244, 246);

        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 76,
            BackColor = Color.White,
            Padding = new Padding(28, 14, 28, 12)
        };

        _pageTitle = new Label
        {
            Text = "Company Management",
            AutoSize = true,
            Location = new Point(28, 12),
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };

        _pageSubtitle = new Label
        {
            Text = "Manage company-level system information.",
            AutoSize = true,
            Location = new Point(30, 43),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        _refreshButton = new Button
        {
            Text = "↻  Refresh",
            Size = new Size(105, 34),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(75, 85, 99),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9F),
            Cursor = Cursors.Hand,
            Location = new Point(Math.Max(100, ClientSize.Width - 145), 20)
        };
        _refreshButton.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
        _refreshButton.FlatAppearance.BorderSize = 1;
        _refreshButton.Click += async (_, _) =>
        {
            _refreshButton.Enabled = false;
            await LoadDashboardAsync();
            _refreshButton.Enabled = true;
        };

        topBar.Controls.Add(_pageTitle);
        topBar.Controls.Add(_pageSubtitle);
        topBar.Controls.Add(_refreshButton);

        var contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28),
            BackColor = Color.FromArgb(243, 244, 246)
        };

        BuildSubscriptionPanel();
        BuildCompaniesPanel();
        BuildStatusPanel();

        _adminAccountsControl.Dock = DockStyle.Fill;
        _adminAccountsControl.Visible = false;

        contentHost.Controls.Add(_adminAccountsControl);
        contentHost.Controls.Add(_statusPanel);
        contentHost.Controls.Add(_companiesPanel);
        contentHost.Controls.Add(_subscriptionPanel);

        _workspace.Controls.Add(contentHost);
        _workspace.Controls.Add(topBar);

        Controls.Add(_workspace);
        Controls.Add(sidebar);

        Resize += (_, _) =>
        {
            _refreshButton.Left = Math.Max(20, _workspace.ClientSize.Width - _refreshButton.Width - 28);
        };

        ShowPanel(_companiesPanel, "Company Management", "Manage company-level system information.");
    }

    private Panel BuildSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 228,
            BackColor = Color.White,
            Padding = new Padding(0)
        };

        var divider = new Panel
        {
            Dock = DockStyle.Right,
            Width = 1,
            BackColor = Color.FromArgb(229, 231, 235)
        };
        sidebar.Controls.Add(divider);

        var brandPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 112,
            Padding = new Padding(24, 20, 18, 10)
        };

        var logo = new Label
        {
            Text = "DriveConnect",
            AutoSize = true,
            Location = new Point(24, 18),
            Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };

        var eyebrow = new Label
        {
            Text = "SYSTEM ADMINISTRATION",
            AutoSize = true,
            Location = new Point(26, 52),
            Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(124, 58, 237)
        };

        var userName = new Label
        {
            Text = string.IsNullOrWhiteSpace(UserSession.FullName)
                ? "Super Admin"
                : UserSession.FullName,
            AutoEllipsis = true,
            Size = new Size(172, 20),
            Location = new Point(26, 73),
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(55, 65, 81)
        };

        brandPanel.Controls.Add(logo);
        brandPanel.Controls.Add(eyebrow);
        brandPanel.Controls.Add(userName);

        var navPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 8, 12, 8)
        };

        AddNavButton(navPanel, "Company Management", _companiesPanel, "System-level company overview.");
        AddNavButton(navPanel, "Subscription", _subscriptionPanel, "Plan and subscription details.");
        AddNavButton(navPanel, "Admin Accounts", _adminAccountsControl, "Manage company Admin accounts.");
        AddNavButton(navPanel, "System Status", _statusPanel, "Connection and system status.");

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 78,
            Padding = new Padding(18, 10, 18, 14),
            BackColor = Color.FromArgb(249, 250, 251)
        };

        var footerRole = new Label
        {
            Text = "SIGNED IN AS",
            AutoSize = true,
            Location = new Point(18, 10),
            Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        var footerValue = new Label
        {
            Text = "Super Admin",
            AutoSize = true,
            Location = new Point(18, 32),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(124, 58, 237)
        };

        footer.Controls.Add(footerRole);
        footer.Controls.Add(footerValue);

        sidebar.Controls.Add(navPanel);
        sidebar.Controls.Add(brandPanel);
        sidebar.Controls.Add(footer);

        return sidebar;
    }

    private void AddNavButton(
        Panel parent,
        string text,
        Control target,
        string subtitle)
    {
        var button = new Button
        {
            Text = text,
            Tag = new NavItem(target, text, subtitle),
            Dock = DockStyle.Top,
            Height = 46,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(75, 85, 99),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 8, 0),
            Font = new Font("Segoe UI Semibold", 9.5F),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) =>
        {
            if (button.Tag is NavItem item)
                ShowPanel(item.Target, item.Title, item.Subtitle);
        };

        parent.Controls.Add(button);
        _navButtons.Add(button);
    }

    private void UpdateActiveNav(Button? activeButton)
    {
        foreach (var button in _navButtons)
        {
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(75, 85, 99);
            button.FlatAppearance.BorderColor = Color.Transparent;
        }

        if (activeButton == null)
            return;

        activeButton.BackColor = Color.FromArgb(245, 243, 255);
        activeButton.ForeColor = Color.FromArgb(109, 40, 217);
        activeButton.FlatAppearance.BorderColor = Color.FromArgb(124, 58, 237);
        _activeNavButton = activeButton;
    }

    private void ShowPanel(Control panel, string title, string subtitle)
    {
        if (panel == null)
            return;

        foreach (Control control in GetPanelHostControls())
            control.Visible = false;

        panel.Visible = true;
        panel.BringToFront();

        _pageTitle.Text = title;
        _pageSubtitle.Text = subtitle;

        var active = _navButtons.FirstOrDefault(button =>
            button.Tag is NavItem item && ReferenceEquals(item.Target, panel));

        UpdateActiveNav(active);
    }

    private IEnumerable<Control> GetPanelHostControls()
    {
        foreach (Control control in _workspace.Controls)
        {
            if (control is not Panel workspacePanel)
                continue;

            foreach (Control child in workspacePanel.Controls)
            {
                if (ReferenceEquals(child, _subscriptionPanel) ||
                    ReferenceEquals(child, _companiesPanel) ||
                    ReferenceEquals(child, _statusPanel) ||
                    ReferenceEquals(child, _adminAccountsControl))
                {
                    yield return child;
                }
            }
        }
    }

    private void BuildCompaniesPanel()
    {
        _companiesPanel.Dock = DockStyle.Fill;
        _companiesPanel.BackColor = Color.Transparent;

        var summary = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 108,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent
        };

        for (int i = 0; i < 4; i++)
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        summary.Controls.Add(
            CreateMetricCard("Companies", "Total companies registered in DriveConnect.", out _companyCountValue),
            0, 0);

        summary.Controls.Add(
            CreateMetricCard("Active Companies", "Companies currently active.", out _activeCompanyCountValue),
            1, 0);

        summary.Controls.Add(
            CreateMetricCard("Active Plans", "Companies with an active subscription.", out _activeSubscriptionCountValue),
            2, 0);

        summary.Controls.Add(
            CreateMetricCard("System", "Current API/system state.", out _systemStatusValue),
            3, 0);

        var tableHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 68,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 18, 0, 0)
        };

        var tableTitle = new Label
        {
            Text = "Companies",
            AutoSize = true,
            Location = new Point(0, 17),
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };

        var tableSubtitle = new Label
        {
            Text = "Company profile, account status, and subscription summary.",
            AutoSize = true,
            Location = new Point(0, 40),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        tableHeader.Controls.Add(tableTitle);
        tableHeader.Controls.Add(tableSubtitle);

        _companiesGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            GridColor = Color.FromArgb(243, 244, 246),
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            RowTemplate = { Height = 48 },
            ColumnHeadersHeight = 42
        };

        _companiesGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            Padding = new Padding(12, 7, 12, 7),
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(55, 65, 81),
            SelectionBackColor = Color.FromArgb(245, 243, 255),
            SelectionForeColor = Color.FromArgb(79, 70, 229)
        };

        _companiesGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(249, 250, 251),
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(75, 85, 99),
            Padding = new Padding(12),
            SelectionBackColor = Color.FromArgb(249, 250, 251),
            SelectionForeColor = Color.FromArgb(75, 85, 99)
        };

        _companiesGrid.EnableHeadersVisualStyles = false;

        _companiesGrid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var column = _companiesGrid.Columns[e.ColumnIndex].Name;

            if (column == "Status")
            {
                var status = e.Value?.ToString() ?? string.Empty;
                e.CellStyle.Font = new Font("Segoe UI Semibold", 9F);
                e.CellStyle.ForeColor = status == "Active"
                    ? Color.FromArgb(22, 101, 52)
                    : Color.FromArgb(153, 27, 27);
            }
        };

        var tableCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };

        tableCard.Paint += (_, e) =>
        {
            ControlPaint.DrawBorder(
                e.Graphics,
                tableCard.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);
        };

        tableCard.Controls.Add(_companiesGrid);

        _companiesPanel.Controls.Add(tableCard);
        _companiesPanel.Controls.Add(tableHeader);
        _companiesPanel.Controls.Add(summary);
    }

    private void BuildSubscriptionPanel()
    {
        _subscriptionPanel.Dock = DockStyle.Fill;
        _subscriptionPanel.BackColor = Color.Transparent;

        var card = new Panel
        {
            Dock = DockStyle.Top,
            Height = 310,
            BackColor = Color.White,
            Padding = new Padding(28)
        };

        card.Paint += (_, e) =>
        {
            ControlPaint.DrawBorder(
                e.Graphics,
                card.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);
        };

        _planValue = CreateValueLabel();
        _feeValue = CreateValueLabel();
        _startValue = CreateValueLabel();
        _endValue = CreateValueLabel();
        _subscriptionStatusValue = CreateValueLabel();

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.White
        };

        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66F));

        AddValueRow(grid, 0, "Plan", _planValue);
        AddValueRow(grid, 1, "Monthly Fee", _feeValue);
        AddValueRow(grid, 2, "Start Date", _startValue);
        AddValueRow(grid, 3, "End Date", _endValue);
        AddValueRow(grid, 4, "Status", _subscriptionStatusValue);

        card.Controls.Add(grid);

        var note = new Label
        {
            Text = "Super Admin view: subscription information is shown at the system level and does not expose CRM records.",
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(2, 14, 0, 0),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        _subscriptionPanel.Controls.Add(note);
        _subscriptionPanel.Controls.Add(card);
    }

    private void BuildStatusPanel()
    {
        _statusPanel.Dock = DockStyle.Fill;
        _statusPanel.BackColor = Color.Transparent;

        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 150,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent
        };

        for (int i = 0; i < 4; i++)
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        cards.Controls.Add(
            CreateStatusMetricCard("System Status", out _statusValueLarge),
            0, 0);

        cards.Controls.Add(
            CreateStatusMetricCard("Service", out _statusService),
            1, 0);

        cards.Controls.Add(
            CreateStatusMetricCard("Role", out _statusRole),
            2, 0);

        cards.Controls.Add(
            CreateStatusMetricCard("User ID", out _statusUser),
            3, 0);

        var detailCard = new Panel
        {
            Dock = DockStyle.Top,
            Height = 105,
            BackColor = Color.White,
            Padding = new Padding(22)
        };

        detailCard.Paint += (_, e) =>
        {
            ControlPaint.DrawBorder(
                e.Graphics,
                detailCard.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);
        };

        _statusUpdated = new Label
        {
            Text = "Last checked: -",
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(75, 85, 99)
        };

        var detail = new Label
        {
            Text = "This page checks only system-level connectivity. It does not expose customer or CRM transaction data.",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(107, 114, 128),
            Padding = new Padding(0, 6, 0, 0)
        };

        detailCard.Controls.Add(detail);
        detailCard.Controls.Add(_statusUpdated);

        _statusPanel.Controls.Add(detailCard);
        _statusPanel.Controls.Add(cards);
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            var companies = await _api.GetCompaniesAsync();
            BindCompanies(companies);
        }
        catch
        {
            _companiesGrid.DataSource = Array.Empty<object>();
            _companyCountValue.Text = "—";
            _activeCompanyCountValue.Text = "—";
            _activeSubscriptionCountValue.Text = "—";
        }

        try
        {
            var subscription = await _api.GetSubscriptionAsync(UserSession.CompanyId);
            BindSubscription(subscription);
        }
        catch
        {
            BindSubscription(null, true);
        }

        try
        {
            var systemStatus = await _api.GetSystemStatusAsync();
            BindSystemStatus(systemStatus);
        }
        catch
        {
            BindSystemStatus(null, true);
        }
    }

    private void BindCompanies(List<CompanyOverview> companies)
    {
        _companyCountValue.Text = companies.Count.ToString();
        _activeCompanyCountValue.Text = companies.Count(x => x.IsActive).ToString();
        _activeSubscriptionCountValue.Text = companies.Count(x =>
            x.IsActive && !string.IsNullOrWhiteSpace(x.ActivePlanName)).ToString();

        var rows = companies.Select(x => new
        {
            ID = x.CompanyId,
            Code = x.CompanyCode,
            Company = x.CompanyName,
            Status = x.IsActive ? "Active" : "Inactive",
            Plan = x.ActivePlanName ?? "None",
            MonthlyFee = x.ActiveMonthlyFee.HasValue
                ? $"₱{x.ActiveMonthlyFee.Value:N2}"
                : "-",
            SubscriptionEnd = x.SubscriptionEndDate.HasValue
                ? x.SubscriptionEndDate.Value.ToLocalTime().ToString("MMM dd, yyyy")
                : "-"
        }).ToList();

        _companiesGrid.DataSource = rows;

        var idColumn = _companiesGrid.Columns["ID"];
        if (idColumn != null)
            idColumn.Visible = false;
    }

    private void BindSubscription(
        SubscriptionOverview? subscription,
        bool failed = false)
    {
        if (subscription == null)
        {
            _planValue.Text = failed ? "Unavailable" : "No subscription";
            _feeValue.Text = "-";
            _startValue.Text = "-";
            _endValue.Text = "-";
            _subscriptionStatusValue.Text = failed
                ? "API unavailable"
                : "Not configured";
            return;
        }

        _planValue.Text = subscription.PlanName;
        _feeValue.Text = $"₱{subscription.MonthlyFee:N2} / month";
        _startValue.Text = subscription.StartDate.ToLocalTime().ToString("MMM dd, yyyy");
        _endValue.Text = subscription.EndDate.ToLocalTime().ToString("MMM dd, yyyy");
        _subscriptionStatusValue.Text = subscription.IsActive ? "Active" : "Inactive";

        _subscriptionStatusValue.ForeColor = subscription.IsActive
            ? Color.FromArgb(22, 101, 52)
            : Color.FromArgb(153, 27, 27);
    }

    private void BindSystemStatus(
        SystemStatusOverview? systemStatus,
        bool failed = false)
    {
        if (systemStatus == null)
        {
            var status = failed ? "Unavailable" : "Unknown";

            _systemStatusValue.Text = status;
            _statusValueLarge.Text = status;
            _statusService.Text = "DriveConnect API";
            _statusRole.Text = UserSession.Role;
            _statusUser.Text = UserSession.UserId.ToString();
            _statusUpdated.Text = $"Last checked: {DateTime.Now:MMM dd, yyyy h:mm:ss tt}";
            return;
        }

        _systemStatusValue.Text = systemStatus.Status;
        _statusValueLarge.Text = systemStatus.Status;
        _statusService.Text = systemStatus.Service;
        _statusRole.Text = systemStatus.Role;
        _statusUser.Text = systemStatus.UserId;
        _statusUpdated.Text = $"Last checked: {DateTime.Now:MMM dd, yyyy h:mm:ss tt}";

        var statusIsHealthy = string.Equals(
            systemStatus.Status,
            "Online",
            StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                systemStatus.Status,
                "Healthy",
                StringComparison.OrdinalIgnoreCase);

        var statusColor = statusIsHealthy
            ? Color.FromArgb(22, 101, 52)
            : Color.FromArgb(153, 27, 27);

        _systemStatusValue.ForeColor = statusColor;
        _statusValueLarge.ForeColor = statusColor;
    }

    private static Panel CreateMetricCard(
        string title,
        string subtitle,
        out Label valueLabel)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0),
            BackColor = Color.White,
            Padding = new Padding(16)
        };

        card.Paint += (_, e) =>
        {
            ControlPaint.DrawBorder(
                e.Graphics,
                card.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);
        };

        var titleLabel = new Label
        {
            Text = title,
            AutoSize = true,
            Location = new Point(16, 13),
            Font = new Font("Segoe UI Semibold", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        valueLabel = new Label
        {
            Text = "—",
            AutoSize = true,
            Location = new Point(16, 38),
            Font = new Font("Segoe UI Semibold", 19F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };

        var subtitleLabel = new Label
        {
            Text = subtitle,
            AutoEllipsis = true,
            Size = new Size(220, 30),
            Location = new Point(16, 68),
            Font = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(156, 163, 175)
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        card.Controls.Add(subtitleLabel);

        return card;
    }

    private static Panel CreateStatusMetricCard(
        string title,
        out Label valueLabel)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0),
            BackColor = Color.White,
            Padding = new Padding(18)
        };

        card.Paint += (_, e) =>
        {
            ControlPaint.DrawBorder(
                e.Graphics,
                card.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);
        };

        var titleLabel = new Label
        {
            Text = title,
            AutoSize = true,
            Location = new Point(18, 16),
            Font = new Font("Segoe UI Semibold", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        valueLabel = new Label
        {
            Text = "—",
            AutoSize = false,
            Size = new Size(180, 45),
            Location = new Point(18, 46),
            Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            AutoEllipsis = true
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);

        return card;
    }

    private static Label CreateValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 11F),
            ForeColor = Color.FromArgb(31, 41, 55),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static void AddValueRow(
        TableLayoutPanel grid,
        int row,
        string labelText,
        Label valueLabel)
    {
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = Color.FromArgb(107, 114, 128),
            TextAlign = ContentAlignment.MiddleLeft
        };

        grid.Controls.Add(label, 0, row);
        grid.Controls.Add(valueLabel, 1, row);
    }

    private sealed record NavItem(
        Control Target,
        string Title,
        string Subtitle);
}
