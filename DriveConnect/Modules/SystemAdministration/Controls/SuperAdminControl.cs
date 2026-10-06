using System.Drawing;
using System.Windows.Forms;
using DriveConnect.winforms.Services;

namespace DriveConnect.winforms.Modules.SystemAdministration.Controls;

public sealed class SuperAdminControl : UserControl
{
    private readonly SuperAdminApiService _api = new();
    private readonly Panel _contentPanel = new();
    private readonly Panel _subscriptionPanel = new();
    private readonly Panel _companiesPanel = new();
    private readonly Panel _statusPanel = new();
    private readonly UserManagementControl _adminAccountsControl = new();

    private Label _planValue = new();
    private Label _feeValue = new();
    private Label _startValue = new();
    private Label _endValue = new();
    private Label _subscriptionStatusValue = new();

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

        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 260,
            BackColor = Color.White,
            Padding = new Padding(0, 20, 0, 0)
        };

        var border = new Panel
        {
            Dock = DockStyle.Right,
            Width = 1,
            BackColor = Color.FromArgb(229, 231, 235)
        };
        sidebar.Controls.Add(border);

        var logo = new Label
        {
            Text = "DriveConnect",
            Dock = DockStyle.Top,
            Height = 55,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            TextAlign = ContentAlignment.MiddleCenter
        };
        sidebar.Controls.Add(logo);

        var subtitle = new Label
        {
            Text = "Super Admin",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(124, 58, 237),
            TextAlign = ContentAlignment.MiddleCenter
        };
        sidebar.Controls.Add(subtitle);

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        nav.Controls.Add(CreateNavButton("Subscription", () => ShowPanel(_subscriptionPanel)));
        nav.Controls.Add(CreateNavButton("Company Management", () => ShowPanel(_companiesPanel)));
        nav.Controls.Add(CreateNavButton("Admin Accounts", () => ShowPanel(_adminAccountsControl)));
        nav.Controls.Add(CreateNavButton("System Status", () => ShowPanel(_statusPanel)));

        sidebar.Controls.Add(nav);
        nav.BringToFront();

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.Padding = new Padding(30);
        _contentPanel.BackColor = Color.FromArgb(243, 244, 246);

        BuildSubscriptionPanel();
        BuildCompaniesPanel();
        BuildStatusPanel();

        _adminAccountsControl.Dock = DockStyle.Fill;
        _adminAccountsControl.Visible = false;

        _contentPanel.Controls.Add(_adminAccountsControl);
        _contentPanel.Controls.Add(_statusPanel);
        _contentPanel.Controls.Add(_companiesPanel);
        _contentPanel.Controls.Add(_subscriptionPanel);

        Controls.Add(_contentPanel);
        Controls.Add(sidebar);

        ShowPanel(_subscriptionPanel);
    }

    private Button CreateNavButton(string text, Action action)
    {
        var button = new Button
        {
            Text = "  " + text,
            Width = 260,
            Height = 48,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(75, 85, 99),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();

        return button;
    }

    private void ShowPanel(Control panel)
    {
        foreach (Control control in _contentPanel.Controls)
            control.Visible = false;

        panel.Visible = true;
        panel.BringToFront();
    }

    private void BuildSubscriptionPanel()
    {
        _subscriptionPanel.Dock = DockStyle.Fill;
        _subscriptionPanel.BackColor = Color.Transparent;

        var title = CreateTitle(
            "Subscription",
            "View the current subscription for the company using DriveConnect.");

        _planValue = CreateValueLabel();
        _feeValue = CreateValueLabel();
        _startValue = CreateValueLabel();
        _endValue = CreateValueLabel();
        _subscriptionStatusValue = CreateValueLabel();

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 310,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.White,
            Padding = new Padding(25)
        };

        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

        AddValueRow(grid, 0, "Plan", _planValue);
        AddValueRow(grid, 1, "Monthly Fee", _feeValue);
        AddValueRow(grid, 2, "Start Date", _startValue);
        AddValueRow(grid, 3, "End Date", _endValue);
        AddValueRow(grid, 4, "Status", _subscriptionStatusValue);

        _subscriptionPanel.Controls.Add(grid);
        _subscriptionPanel.Controls.Add(title);
    }

    private void BuildCompaniesPanel()
    {
        _companiesPanel.Dock = DockStyle.Fill;
        _companiesPanel.BackColor = Color.Transparent;

        var title = CreateTitle(
            "Company Management",
            "System-level company information only. No customer or CRM records are shown here.");

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
            GridColor = Color.FromArgb(243, 244, 246)
        };

        _companiesGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            Padding = new Padding(10, 7, 10, 7),
            Font = new Font("Segoe UI", 9.5F)
        };

        _companiesGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(249, 250, 251),
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = Color.FromArgb(75, 85, 99),
            Padding = new Padding(10)
        };

        _companiesGrid.EnableHeadersVisualStyles = false;

        var wrapper = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };
        wrapper.Controls.Add(_companiesGrid);

        _companiesPanel.Controls.Add(wrapper);
        _companiesPanel.Controls.Add(title);
    }

    private void BuildStatusPanel()
    {
        _statusPanel.Dock = DockStyle.Fill;
        _statusPanel.BackColor = Color.Transparent;

        var title = CreateTitle(
            "System Status",
            "System-level status only. Company CRM data is intentionally excluded.");

        var card = new Panel
        {
            Dock = DockStyle.Top,
            Height = 220,
            BackColor = Color.White,
            Padding = new Padding(25)
        };

        _systemStatusValue = new Label
        {
            Text = "Loading...",
            AutoSize = true,
            Location = new Point(25, 35),
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(124, 58, 237)
        };

        _systemStatusDetail = new Label
        {
            Text = "Checking the DriveConnect API...",
            AutoSize = true,
            Location = new Point(25, 75),
            ForeColor = Color.FromArgb(75, 85, 99)
        };

        card.Controls.Add(_systemStatusValue);
        card.Controls.Add(_systemStatusDetail);
        _statusPanel.Controls.Add(card);
        _statusPanel.Controls.Add(title);
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            var subscription = await _api.GetSubscriptionAsync(UserSession.CompanyId);

            if (subscription != null)
            {
                _planValue.Text = subscription.PlanName;
                _feeValue.Text = $"₱{subscription.MonthlyFee:N2} / month";
                _startValue.Text = subscription.StartDate.ToLocalTime().ToString("MMM dd, yyyy");
                _endValue.Text = subscription.EndDate.ToLocalTime().ToString("MMM dd, yyyy");
                _subscriptionStatusValue.Text = subscription.IsActive ? "Active" : "Inactive";
            }
            else
            {
                _planValue.Text = "No subscription";
                _feeValue.Text = "-";
                _startValue.Text = "-";
                _endValue.Text = "-";
                _subscriptionStatusValue.Text = "Not configured";
            }
        }
        catch
        {
            _planValue.Text = "Unavailable";
            _feeValue.Text = "-";
            _startValue.Text = "-";
            _endValue.Text = "-";
            _subscriptionStatusValue.Text = "API unavailable";
        }

        try
        {
            var companies = await _api.GetCompaniesAsync();
            _companiesGrid.DataSource = companies.Select(x => new
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

            var idColumn = _companiesGrid.Columns["ID"];
            if (idColumn != null)
                idColumn.Visible = false;
        }
        catch
        {
            _companiesGrid.DataSource = Array.Empty<object>();
        }

        try
        {
            var systemStatus = await _api.GetSystemStatusAsync();

            if (systemStatus != null)
            {
                _systemStatusValue.Text = systemStatus.Status;
                _systemStatusDetail.Text =
                    $"{systemStatus.Service} • Logged in as {systemStatus.Role}";
            }
        }
        catch
        {
            _systemStatusValue.Text = "Unavailable";
            _systemStatusDetail.Text = "Unable to reach the DriveConnect API.";
        }
    }

    private static Panel CreateTitle(string titleText, string subtitleText)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 68,
            BackColor = Color.Transparent
        };

        var title = new Label
        {
            Text = titleText,
            AutoSize = true,
            Location = new Point(0, 0),
            Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };

        var subtitle = new Label
        {
            Text = subtitleText,
            AutoSize = true,
            Location = new Point(2, 40),
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);

        return panel;
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
}
