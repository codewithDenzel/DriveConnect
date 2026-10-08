using System.Drawing;
using System.Globalization;
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

    private Label _pageTitle = new();
    private Label _pageSubtitle = new();
    private Button _refreshButton = new();

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

    // Subscription editor
    private ComboBox _subscriptionCompanyCombo = new();
    private ComboBox _subscriptionPlanCombo = new();
    private ComboBox _subscriptionBillingCycleCombo = new();
    private TextBox _subscriptionAmountTextBox = new();
    private DateTimePicker _subscriptionStartDatePicker = new();
    private CheckBox _subscriptionActiveCheckBox = new();
    private Button _subscriptionSaveButton = new();
    private Button _subscriptionDeactivateButton = new();
    private Label _subscriptionEndDateValue = new();
    private Label _subscriptionMonthlyEquivalentValue = new();

    // Subscription summary
    private Label _summaryCompanyValue = new();
    private Label _summaryPlanValue = new();
    private Label _summaryCycleValue = new();
    private Label _summaryAmountValue = new();
    private Label _summaryDatesValue = new();
    private Label _summaryStatusValue = new();

    private List<CompanyOverview> _loadedCompanies = new();
    private bool _suppressSubscriptionCompanyChanged;

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
            BackColor = Color.White
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
            Text = "Refresh",
            Size = new Size(105, 34),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(75, 85, 99),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9F),
            Cursor = Cursors.Hand,
            Location = new Point(0, 20)
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
            _refreshButton.Left = Math.Max(
                20,
                _workspace.ClientSize.Width - _refreshButton.Width - 28);
        };

        _refreshButton.Left = Math.Max(
            20,
            _workspace.ClientSize.Width - _refreshButton.Width - 28);

        ShowPanel(
            _companiesPanel,
            "Company Management",
            "Manage company-level system information.");
    }

    private Panel BuildSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 228,
            BackColor = Color.White
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
            Height = 112
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

        AddNavButton(
            navPanel,
            "Company Management",
            _companiesPanel,
            "System-level company overview.");

        AddNavButton(
            navPanel,
            "Subscription",
            _subscriptionPanel,
            "Assign and manage company subscriptions.");

        AddNavButton(
            navPanel,
            "Admin Accounts",
            _adminAccountsControl,
            "Manage company Admin accounts.");

        AddNavButton(
            navPanel,
            "System Status",
            _statusPanel,
            "Connection and system status.");

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
            button.FlatAppearance.BorderSize = 0;
        }

        if (activeButton == null)
            return;

        activeButton.BackColor = Color.FromArgb(245, 243, 255);
        activeButton.ForeColor = Color.FromArgb(109, 40, 217);
        activeButton.FlatAppearance.BorderSize = 1;
        activeButton.FlatAppearance.BorderColor = Color.FromArgb(124, 58, 237);
    }

    private void ShowPanel(Control panel, string title, string subtitle)
    {
        foreach (Control control in GetPanelHostControls())
            control.Visible = false;

        panel.Visible = true;
        panel.BringToFront();

        _pageTitle.Text = title;
        _pageSubtitle.Text = subtitle;

        var active = _navButtons.FirstOrDefault(button =>
            button.Tag is NavItem item &&
            ReferenceEquals(item.Target, panel));

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
            CreateMetricCard(
                "Companies",
                "Total companies registered in DriveConnect.",
                out _companyCountValue),
            0, 0);

        summary.Controls.Add(
            CreateMetricCard(
                "Active Companies",
                "Companies currently active.",
                out _activeCompanyCountValue),
            1, 0);

        summary.Controls.Add(
            CreateMetricCard(
                "Active Subscriptions",
                "Companies with an active subscription.",
                out _activeSubscriptionCountValue),
            2, 0);

        summary.Controls.Add(
            CreateMetricCard(
                "System",
                "Current API/system state.",
                out _systemStatusValue),
            3, 0);

        var tableHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 68,
            BackColor = Color.Transparent
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

                e.CellStyle.Font = new Font(
                    "Segoe UI Semibold",
                    9F);

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

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));

        var editorCard = CreateCard();
        editorCard.Dock = DockStyle.Fill;
        editorCard.Padding = new Padding(24);

        var editorTitle = new Label
        {
            Text = "Subscription Setup",
            AutoSize = true,
            Location = new Point(24, 20),
            Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };

        var editorSubtitle = new Label
        {
            Text = "Set the plan, billing cycle, price, and subscription period for a company.",
            AutoSize = true,
            Location = new Point(26, 47),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        editorCard.Controls.Add(editorTitle);
        editorCard.Controls.Add(editorSubtitle);

        var form = new TableLayoutPanel
        {
            Location = new Point(24, 82),
            Size = new Size(570, 355),
            ColumnCount = 2,
            RowCount = 7,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.White
        };

        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155F));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        AddFormLabel(form, 0, "Company");
        _subscriptionCompanyCombo = CreateComboBox();
        _subscriptionCompanyCombo.SelectedIndexChanged += async (_, _) =>
        {
            if (_suppressSubscriptionCompanyChanged)
                return;

            if (_subscriptionCompanyCombo.SelectedItem is CompanyChoice choice)
                await LoadSubscriptionForCompanyAsync(choice.CompanyId);
        };
        form.Controls.Add(_subscriptionCompanyCombo, 1, 0);

        AddFormLabel(form, 1, "Plan Name");
        _subscriptionPlanCombo = CreateComboBox();
        _subscriptionPlanCombo.DropDownStyle = ComboBoxStyle.DropDown;
        _subscriptionPlanCombo.Items.AddRange(new object[]
        {
            "Starter",
            "Professional",
            "Enterprise",
            "Custom"
        });
        form.Controls.Add(_subscriptionPlanCombo, 1, 1);

        AddFormLabel(form, 2, "Billing Cycle");
        _subscriptionBillingCycleCombo = CreateComboBox();
        _subscriptionBillingCycleCombo.Items.AddRange(new object[]
        {
            "Monthly",
            "Annual"
        });
        _subscriptionBillingCycleCombo.SelectedIndex = 0;
        _subscriptionBillingCycleCombo.SelectedIndexChanged += (_, _) => UpdateSubscriptionPreview();
        form.Controls.Add(_subscriptionBillingCycleCombo, 1, 2);

        AddFormLabel(form, 3, "Billing Amount");
        _subscriptionAmountTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            BorderStyle = BorderStyle.FixedSingle
        };
        _subscriptionAmountTextBox.TextChanged += (_, _) => UpdateSubscriptionPreview();
        form.Controls.Add(_subscriptionAmountTextBox, 1, 3);

        AddFormLabel(form, 4, "Start Date");
        _subscriptionStartDatePicker = new DateTimePicker
        {
            Dock = DockStyle.Left,
            Width = 190,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "MMM dd, yyyy"
        };
        _subscriptionStartDatePicker.ValueChanged += (_, _) => UpdateSubscriptionPreview();
        form.Controls.Add(_subscriptionStartDatePicker, 1, 4);

        AddFormLabel(form, 5, "End Date");
        _subscriptionEndDateValue = CreateFormValueLabel();
        form.Controls.Add(_subscriptionEndDateValue, 1, 5);

        AddFormLabel(form, 6, "Monthly Equivalent");
        _subscriptionMonthlyEquivalentValue = CreateFormValueLabel();
        form.Controls.Add(_subscriptionMonthlyEquivalentValue, 1, 6);

        editorCard.Controls.Add(form);

        _subscriptionActiveCheckBox = new CheckBox
        {
            Text = "Subscription is active",
            AutoSize = true,
            Location = new Point(24, 450),
            Checked = true,
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(55, 65, 81)
        };
        editorCard.Controls.Add(_subscriptionActiveCheckBox);

        _subscriptionSaveButton = CreatePrimaryButton(
            "Save Subscription",
            24,
            488,
            175,
            40);
        _subscriptionSaveButton.Click += async (_, _) => await SaveSubscriptionAsync();

        _subscriptionDeactivateButton = CreateSecondaryButton(
            "Deactivate",
            210,
            488,
            115,
            40);
        _subscriptionDeactivateButton.Click += async (_, _) => await DeactivateSubscriptionAsync();

        editorCard.Controls.Add(_subscriptionSaveButton);
        editorCard.Controls.Add(_subscriptionDeactivateButton);

        var editorNote = new Label
        {
            Text = "The billing amount is the amount the company pays for the selected billing cycle. Annual subscriptions also show a monthly equivalent for reference.",
            Location = new Point(24, 540),
            Size = new Size(570, 52),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };
        editorCard.Controls.Add(editorNote);

        var summaryCard = CreateCard();
        summaryCard.Dock = DockStyle.Fill;
        summaryCard.Margin = new Padding(12, 0, 0, 0);
        summaryCard.Padding = new Padding(22);

        var summaryTitle = new Label
        {
            Text = "Current Subscription",
            AutoSize = true,
            Location = new Point(22, 20),
            Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };

        var summarySubtitle = new Label
        {
            Text = "What is currently assigned to the selected company.",
            AutoSize = true,
            Location = new Point(24, 47),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        summaryCard.Controls.Add(summaryTitle);
        summaryCard.Controls.Add(summarySubtitle);

        int sy = 92;
        AddSummaryRow(summaryCard, "Company", ref sy, out _summaryCompanyValue);
        AddSummaryRow(summaryCard, "Plan", ref sy, out _summaryPlanValue);
        AddSummaryRow(summaryCard, "Billing", ref sy, out _summaryCycleValue);
        AddSummaryRow(summaryCard, "Amount", ref sy, out _summaryAmountValue);
        AddSummaryRow(summaryCard, "Period", ref sy, out _summaryDatesValue);
        AddSummaryRow(summaryCard, "Status", ref sy, out _summaryStatusValue);

        var summaryHint = new Label
        {
            Text = "Use this page when a company purchases, renews, changes, or cancels its DriveConnect subscription.",
            Location = new Point(22, sy + 20),
            Size = new Size(300, 80),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };
        summaryCard.Controls.Add(summaryHint);

        layout.Controls.Add(editorCard, 0, 0);
        layout.Controls.Add(summaryCard, 1, 0);

        _subscriptionPanel.Controls.Add(layout);
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

        var detailCard = CreateCard();
        detailCard.Dock = DockStyle.Top;
        detailCard.Height = 105;
        detailCard.Padding = new Padding(22);

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
            _loadedCompanies = new();
        }

        await LoadSubscriptionForCompanyAsync(UserSession.CompanyId);

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
        _loadedCompanies = companies;

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
            Billing = x.ActiveBillingAmount.HasValue
                ? $"₱{x.ActiveBillingAmount.Value:N2} / {x.ActiveBillingCycle?.ToLowerInvariant() ?? "cycle"}"
                : "-",
            SubscriptionEnd = x.SubscriptionEndDate.HasValue
                ? x.SubscriptionEndDate.Value.ToLocalTime().ToString("MMM dd, yyyy")
                : "-"
        }).ToList();

        _companiesGrid.DataSource = rows;

        var idColumn = _companiesGrid.Columns["ID"];
        if (idColumn != null)
            idColumn.Visible = false;

        PopulateSubscriptionCompanyCombo();
    }

    private void PopulateSubscriptionCompanyCombo()
    {
        var selectedCompanyId = UserSession.CompanyId;

        _suppressSubscriptionCompanyChanged = true;

        _subscriptionCompanyCombo.Items.Clear();

        foreach (var company in _loadedCompanies)
        {
            _subscriptionCompanyCombo.Items.Add(
                new CompanyChoice(
                    company.CompanyId,
                    $"{company.CompanyName} ({company.CompanyCode})"));
        }

        var selected = _subscriptionCompanyCombo.Items
            .OfType<CompanyChoice>()
            .FirstOrDefault(x => x.CompanyId == selectedCompanyId);

        if (selected != null)
            _subscriptionCompanyCombo.SelectedItem = selected;
        else if (_subscriptionCompanyCombo.Items.Count > 0)
            _subscriptionCompanyCombo.SelectedIndex = 0;

        _suppressSubscriptionCompanyChanged = false;
    }

    private async Task LoadSubscriptionForCompanyAsync(int companyId)
    {
        if (companyId <= 0)
            return;

        try
        {
            var subscription = await _api.GetSubscriptionAsync(companyId);
            BindSubscription(subscription, false);
        }
        catch
        {
            ClearSubscriptionEditor();
            SetSubscriptionSummary(
                _loadedCompanies.FirstOrDefault(x => x.CompanyId == companyId)?.CompanyName
                ?? "-",
                "Unavailable",
                "Unavailable",
                "-",
                "-",
                "API unavailable");
        }
    }

    private void BindSubscription(
        SubscriptionOverview? subscription,
        bool failed)
    {
        var companyId = subscription?.CompanyId
            ?? (_subscriptionCompanyCombo.SelectedItem as CompanyChoice)?.CompanyId
            ?? UserSession.CompanyId;

        var company = _loadedCompanies.FirstOrDefault(x => x.CompanyId == companyId);

        if (subscription == null)
        {
            _subscriptionPlanCombo.Text = "";
            _subscriptionBillingCycleCombo.SelectedItem = "Monthly";
            _subscriptionAmountTextBox.Text = "";
            _subscriptionStartDatePicker.Value = DateTime.Today;
            _subscriptionActiveCheckBox.Checked = false;
            _subscriptionDeactivateButton.Enabled = false;

            SetSubscriptionSummary(
                company?.CompanyName ?? "-",
                "No subscription",
                "-",
                "-",
                "-",
                failed ? "API unavailable" : "Not configured");

            UpdateSubscriptionPreview();
            return;
        }

        _subscriptionPlanCombo.Text = subscription.PlanName;
        _subscriptionBillingCycleCombo.SelectedItem =
            subscription.BillingCycle == "Annual"
                ? "Annual"
                : "Monthly";

        _subscriptionAmountTextBox.Text =
            subscription.BillingAmount.ToString("N2");

        var localStart = subscription.StartDate.ToLocalTime().Date;
        if (localStart < _subscriptionStartDatePicker.MinDate)
            localStart = _subscriptionStartDatePicker.MinDate;

        if (localStart > _subscriptionStartDatePicker.MaxDate)
            localStart = _subscriptionStartDatePicker.MaxDate;

        _subscriptionStartDatePicker.Value = localStart;
        _subscriptionActiveCheckBox.Checked = subscription.IsActive;
        _subscriptionDeactivateButton.Enabled = subscription.IsActive;

        SetSubscriptionSummary(
            company?.CompanyName ?? "-",
            subscription.PlanName,
            subscription.BillingCycle,
            $"₱{subscription.BillingAmount:N2}",
            $"{subscription.StartDate.ToLocalTime():MMM dd, yyyy} - {subscription.EndDate.ToLocalTime():MMM dd, yyyy}",
            subscription.IsActive ? "Active" : "Inactive");

        UpdateSubscriptionPreview();
    }

    private void ClearSubscriptionEditor()
    {
        _subscriptionPlanCombo.Text = "";
        _subscriptionBillingCycleCombo.SelectedItem = "Monthly";
        _subscriptionAmountTextBox.Text = "";
        _subscriptionStartDatePicker.Value = DateTime.Today;
        _subscriptionActiveCheckBox.Checked = false;
        _subscriptionDeactivateButton.Enabled = false;
        UpdateSubscriptionPreview();
    }

    private async Task SaveSubscriptionAsync()
    {
        var choice = _subscriptionCompanyCombo.SelectedItem as CompanyChoice;

        if (choice == null)
        {
            MessageBox.Show(
                "Select a company first.",
                "Subscription",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var planName = _subscriptionPlanCombo.Text.Trim();
        var billingCycle = _subscriptionBillingCycleCombo.SelectedItem?.ToString() ?? "Monthly";

        if (string.IsNullOrWhiteSpace(planName))
        {
            MessageBox.Show(
                "Enter a plan name.",
                "Subscription",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(
                _subscriptionAmountTextBox.Text.Trim(),
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var billingAmount) ||
            billingAmount <= 0)
        {
            MessageBox.Show(
                "Enter a valid billing amount greater than zero.",
                "Subscription",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var request = new SubscriptionRequest(
            planName,
            billingCycle,
            decimal.Round(billingAmount, 2, MidpointRounding.AwayFromZero),
            _subscriptionStartDatePicker.Value.Date,
            _subscriptionActiveCheckBox.Checked);

        _subscriptionSaveButton.Enabled = false;
        _subscriptionDeactivateButton.Enabled = false;

        try
        {
            var response = await _api.SaveSubscriptionAsync(
                choice.CompanyId,
                request);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    string.IsNullOrWhiteSpace(message)
                        ? "The subscription could not be saved."
                        : message.Trim('"'),
                    "Subscription Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "The company subscription has been saved.",
                "Subscription Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadDashboardAsync();

            _suppressSubscriptionCompanyChanged = true;
            var selected = _subscriptionCompanyCombo.Items
                .OfType<CompanyChoice>()
                .FirstOrDefault(x => x.CompanyId == choice.CompanyId);

            if (selected != null)
                _subscriptionCompanyCombo.SelectedItem = selected;

            _suppressSubscriptionCompanyChanged = false;

            await LoadSubscriptionForCompanyAsync(choice.CompanyId);
        }
        catch
        {
            MessageBox.Show(
                "Unable to save the subscription. Make sure the API is running and the Master database migration has been applied.",
                "Subscription Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _subscriptionSaveButton.Enabled = true;
            _subscriptionDeactivateButton.Enabled =
                _subscriptionActiveCheckBox.Checked;
        }
    }

    private async Task DeactivateSubscriptionAsync()
    {
        var choice = _subscriptionCompanyCombo.SelectedItem as CompanyChoice;

        if (choice == null)
            return;

        var confirm = MessageBox.Show(
            "Deactivate this company's DriveConnect subscription?",
            "Confirm Deactivation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
            return;

        _subscriptionDeactivateButton.Enabled = false;

        try
        {
            var response = await _api.DeactivateSubscriptionAsync(choice.CompanyId);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    string.IsNullOrWhiteSpace(message)
                        ? "The subscription could not be deactivated."
                        : message.Trim('"'),
                    "Subscription Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "The company subscription is now inactive.",
                "Subscription Updated",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadDashboardAsync();
        }
        catch
        {
            MessageBox.Show(
                "Unable to deactivate the subscription. Make sure the API is running.",
                "Subscription Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _subscriptionDeactivateButton.Enabled = _subscriptionActiveCheckBox.Checked;
        }
    }

    private void UpdateSubscriptionPreview()
    {
        var cycle = _subscriptionBillingCycleCombo.SelectedItem?.ToString() ?? "Monthly";
        var start = _subscriptionStartDatePicker.Value.Date;

        var end = cycle == "Annual"
            ? start.AddYears(1).AddDays(-1)
            : start.AddMonths(1).AddDays(-1);

        _subscriptionEndDateValue.Text =
            end.ToString("MMM dd, yyyy");

        if (decimal.TryParse(
                _subscriptionAmountTextBox.Text.Trim(),
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var amount) &&
            amount > 0)
        {
            var monthlyEquivalent = cycle == "Annual"
                ? decimal.Round(amount / 12m, 2, MidpointRounding.AwayFromZero)
                : amount;

            _subscriptionMonthlyEquivalentValue.Text =
                $"₱{monthlyEquivalent:N2} / month";
        }
        else
        {
            _subscriptionMonthlyEquivalentValue.Text = "-";
        }
    }

    private void SetSubscriptionSummary(
        string company,
        string plan,
        string cycle,
        string amount,
        string period,
        string status)
    {
        _summaryCompanyValue.Text = company;
        _summaryPlanValue.Text = plan;
        _summaryCycleValue.Text = cycle;
        _summaryAmountValue.Text = amount;
        _summaryDatesValue.Text = period;
        _summaryStatusValue.Text = status;

        _summaryStatusValue.ForeColor = status == "Active"
            ? Color.FromArgb(22, 101, 52)
            : status is "Inactive" or "Not configured"
                ? Color.FromArgb(107, 114, 128)
                : Color.FromArgb(153, 27, 27);
    }

    private static Panel CreateCard()
    {
        var panel = new Panel
        {
            BackColor = Color.White,
            Padding = new Padding(1)
        };

        panel.Paint += (_, e) =>
        {
            ControlPaint.DrawBorder(
                e.Graphics,
                panel.ClientRectangle,
                Color.FromArgb(229, 231, 235),
                ButtonBorderStyle.Solid);
        };

        return panel;
    }

    private static ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
    }

    private static void AddFormLabel(
        TableLayoutPanel form,
        int row,
        string text)
    {
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(75, 85, 99),
            TextAlign = ContentAlignment.MiddleLeft
        };

        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        form.Controls.Add(label, 0, row);
    }

    private static Label CreateFormValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 10F),
            ForeColor = Color.FromArgb(55, 65, 81),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static void AddSummaryRow(
        Panel parent,
        string title,
        ref int y,
        out Label value)
    {
        var label = new Label
        {
            Text = title,
            AutoSize = true,
            Location = new Point(22, y),
            Font = new Font("Segoe UI Semibold", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        value = new Label
        {
            Text = "-",
            AutoEllipsis = true,
            Size = new Size(260, 34),
            Location = new Point(22, y + 18),
            Font = new Font("Segoe UI Semibold", 10.5F),
            ForeColor = Color.FromArgb(31, 41, 55)
        };

        parent.Controls.Add(label);
        parent.Controls.Add(value);

        y += 58;
    }

    private static Button CreatePrimaryButton(
        string text,
        int x,
        int y,
        int width,
        int height)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9F),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private static Button CreateSecondaryButton(
        string text,
        int x,
        int y,
        int width,
        int height)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(75, 85, 99),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9F),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
        return button;
    }

    private static Panel CreateStatusMetricCard(
        string title,
        out Label valueLabel)
    {
        var card = CreateCard();

        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 10, 0);
        card.Padding = new Padding(18);

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

    private static Panel CreateMetricCard(
        string title,
        string subtitle,
        out Label valueLabel)
    {
        var card = CreateCard();

        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 10, 0);
        card.Padding = new Padding(16);

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
            Size = new Size(210, 30),
            Location = new Point(16, 68),
            Font = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(156, 163, 175)
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        card.Controls.Add(subtitleLabel);

        return card;
    }

    private sealed record CompanyChoice(int CompanyId, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    private sealed record NavItem(
        Control Target,
        string Title,
        string Subtitle);
}
