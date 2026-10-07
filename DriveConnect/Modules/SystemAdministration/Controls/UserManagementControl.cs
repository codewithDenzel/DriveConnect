using System.Drawing;
using System.Net.Mail;
using System.Windows.Forms;
using DriveConnect.domain.DTO;
using DriveConnect.domain.Entities;
using DriveConnect.winforms.Services;

namespace DriveConnect.winforms.Modules.SystemAdministration.Controls;

public sealed class UserManagementControl : UserControl
{
    private readonly UserManagementApiService _api = new();
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtSearch = new();
    private readonly Button _btnNew = new();
    private readonly Label _lblStatus = new();

    private readonly List<UserListItem> _allUsers = new();
    private List<Branch> _allBranches = new();

    private const string SuperAdmin = "Super Admin";
    private const string Admin = "Admin";
    private const string Staff = "Staff";

    public UserManagementControl()
    {
        BuildUi();
        _ = LoadUsersAsync();
    }

    private void BuildUi()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(243, 244, 246);
        Font = new Font("Segoe UI", 10F);

        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 125,
            BackColor = Color.Transparent
        };

        var title = new Label
        {
            Text = string.Equals(
                UserSession.Role,
                SuperAdmin,
                StringComparison.OrdinalIgnoreCase)
                ? "Admin Accounts"
                : "Staff Management",
            AutoSize = true,
            Location = new Point(0, 3),
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };

        var subtitle = new Label
        {
            Text = string.Equals(
                UserSession.Role,
                SuperAdmin,
                StringComparison.OrdinalIgnoreCase)
                ? "Manage the dealership's Admin accounts only."
                : "Manage the dealership's Staff accounts only.",
            AutoSize = true,
            Location = new Point(2, 37),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        _btnNew.Text = "+ New User";
        _btnNew.Size = new Size(125, 38);
        _btnNew.Location = new Point(0, 75);
        _btnNew.BackColor = Color.FromArgb(79, 70, 229);
        _btnNew.ForeColor = Color.White;
        _btnNew.FlatStyle = FlatStyle.Flat;
        _btnNew.FlatAppearance.BorderSize = 0;
        _btnNew.Font = new Font("Segoe UI Semibold", 9.5F);
        _btnNew.Cursor = Cursors.Hand;
        _btnNew.Click += async (_, _) => await ShowUserModalAsync(null);

        _txtSearch.PlaceholderText = "Search name, username, email, role, or branch...";
        _txtSearch.Width = 340;
        _txtSearch.Height = 36;
        _txtSearch.Location = new Point(140, 76);
        _txtSearch.BorderStyle = BorderStyle.FixedSingle;
        _txtSearch.TextChanged += (_, _) => BindGrid();

        _lblStatus.AutoSize = true;
        _lblStatus.Location = new Point(500, 86);
        _lblStatus.Font = new Font("Segoe UI", 9F);
        _lblStatus.ForeColor = Color.FromArgb(107, 114, 128);

        topBar.Controls.Add(title);
        topBar.Controls.Add(subtitle);
        topBar.Controls.Add(_btnNew);
        topBar.Controls.Add(_txtSearch);
        topBar.Controls.Add(_lblStatus);

        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.GridColor = Color.FromArgb(243, 244, 246);
        _grid.AllowUserToAddRows = false;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowTemplate.Height = 45;
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            Padding = new Padding(12, 8, 12, 8),
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(55, 65, 81),
            SelectionBackColor = Color.FromArgb(238, 242, 255),
            SelectionForeColor = Color.FromArgb(79, 70, 229)
        };
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(249, 250, 251),
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = Color.FromArgb(107, 114, 128),
            Padding = new Padding(12)
        };
        _grid.EnableHeadersVisualStyles = false;
        _grid.CellDoubleClick += Grid_CellDoubleClick;

        var wrapper = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(1)
        };
        wrapper.Paint += (_, e) => ControlPaint.DrawBorder(
            e.Graphics,
            wrapper.ClientRectangle,
            Color.FromArgb(229, 231, 235),
            ButtonBorderStyle.Solid);
        wrapper.Controls.Add(_grid);

        Controls.Add(wrapper);
        Controls.Add(topBar);
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            if (!UserSession.IsLoggedIn ||
                !IsManagementRole(UserSession.Role))
            {
                ShowStatus("Access denied.", true);
                _btnNew.Enabled = false;
                return;
            }

            _allBranches = await _api.GetBranchesAsync(UserSession.CompanyId);
            var users = await _api.GetUsersAsync(UserSession.CompanyId);

            var managedRole = string.Equals(
                UserSession.Role,
                SuperAdmin,
                StringComparison.OrdinalIgnoreCase)
                ? Admin
                : Staff;

            _allUsers.Clear();
            _allUsers.AddRange(users.Where(x =>
                string.Equals(
                    x.Role,
                    managedRole,
                    StringComparison.OrdinalIgnoreCase)));

            BindGrid();
            ShowStatus($"{_allUsers.Count} {managedRole} user(s).", false);
        }
        catch (HttpRequestException)
        {
            ShowStatus("Unable to connect to the API.", true);
        }
        catch
        {
            ShowStatus("Unable to load users.", true);
        }
    }

    private void BindGrid()
    {
        var term = _txtSearch.Text.Trim();

        var managedRole = string.Equals(
            UserSession.Role,
            SuperAdmin,
            StringComparison.OrdinalIgnoreCase)
            ? Admin
            : Staff;

        var rows = _allUsers
            .Where(x => string.Equals(
                x.Role,
                managedRole,
                StringComparison.OrdinalIgnoreCase))
            .Where(x =>
                string.IsNullOrWhiteSpace(term) ||
                x.Username.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Role.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (x.BranchName ?? "").Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(x => new
            {
                ID = x.UserId,
                Name = string.IsNullOrWhiteSpace(x.FullName) ? x.Username : x.FullName,
                Username = x.Username,
                Email = x.Email,
                Role = x.Role,
                Branch = x.BranchName ?? "All / None",
                Status = x.IsActive ? "Active" : "Inactive",
                Created = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
            })
            .ToList();

        _grid.DataSource = rows;

        var id = _grid.Columns["ID"];
        if (id != null) id.Visible = false;
    }

    private async void Grid_CellDoubleClick(
        object? sender,
        DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var idCell = _grid.Rows[e.RowIndex].Cells["ID"].Value;
        if (idCell == null) return;

        var userId = Convert.ToInt32(idCell);
        var user = _allUsers.FirstOrDefault(x => x.UserId == userId);

        if (user != null)
            await ShowUserModalAsync(user);
    }

    private async Task ShowUserModalAsync(UserListItem? existing)
    {
        try
        {
            _allBranches = await _api.GetBranchesAsync(UserSession.CompanyId);
        }
        catch
        {
            MessageBox.Show(
                "Unable to load the latest branch list.",
                "Branch Load Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        using var form = new Form
        {
            Text = existing == null ? "New User" : "Edit User",
            ClientSize = new Size(470, 655),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = existing == null ? "Create User" : "Edit User",
            AutoSize = true,
            Location = new Point(30, 25),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };
        form.Controls.Add(title);

        int y = 75;

        var txtUsername = AddField(form, "Username", existing?.Username, ref y);
        var txtFullName = AddField(form, "Full Name", existing?.FullName, ref y);
        var txtEmail = AddField(form, "Email", existing?.Email, ref y);

        var availableRoles = GetAvailableRoles(existing);
        var cbRole = AddRoleCombo(
            form,
            "Role",
            availableRoles,
            existing?.Role ?? (UserSession.Role == Admin ? Staff : Admin),
            ref y);

        var isEditingSuperAdmin =
            existing != null &&
            string.Equals(
                existing.Role,
                SuperAdmin,
                StringComparison.OrdinalIgnoreCase);

        cbRole.Enabled = !isEditingSuperAdmin;

        var branches = _allBranches
            .Where(x => x.IsActive)
            .OrderBy(x => x.BranchName)
            .ToList();

        var cbBranch = AddBranchCombo(
            form,
            "Branch",
            branches,
            existing?.BranchId,
            ref y);

        var txtPassword = AddPasswordField(
            form,
            existing == null ? "Password" : "New Password (Optional)",
            ref y);

        var chkActive = new CheckBox
        {
            Text = "Active",
            Checked = existing?.IsActive ?? true,
            Location = new Point(30, y + 5),
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = Color.FromArgb(75, 85, 99),
            Enabled = !isEditingSuperAdmin
        };
        form.Controls.Add(chkActive);
        y += 45;

        var lblHint = new Label
        {
            AutoSize = false,
            Size = new Size(400, 44),
            Location = new Point(30, y),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };
        form.Controls.Add(lblHint);
        y += 55;

        var btnSave = new Button
        {
            Text = existing == null ? "Create User" : "Save Changes",
            Size = new Size(400, 44),
            Location = new Point(30, y),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 10F),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        form.Controls.Add(btnSave);

        void ApplyRoleRules()
        {
            var role = cbRole.SelectedItem?.ToString() ?? Staff;
            var isSuperAdmin = role == SuperAdmin;

            cbBranch.Enabled = !isSuperAdmin;

            if (isSuperAdmin)
                cbBranch.SelectedIndex = 0;

            lblHint.Text = string.Equals(
                UserSession.Role,
                SuperAdmin,
                StringComparison.OrdinalIgnoreCase)
                ? "Super Admin accounts are system-owned. This screen manages Admin accounts only."
                : "Staff users must be assigned to an active branch.";
        }

        cbRole.SelectedIndexChanged += (_, _) => ApplyRoleRules();
        ApplyRoleRules();

        btnSave.Click += async (_, _) =>
        {
            var username = txtUsername.Text.Trim();
            var fullName = txtFullName.Text.Trim();
            var email = txtEmail.Text.Trim();
            var role = cbRole.SelectedItem?.ToString() ?? Staff;
            var password = txtPassword.Text;
            var selectedBranch = cbBranch.SelectedItem as BranchChoice;
            int? branchId = selectedBranch?.Id;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Full Name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                MessageBox.Show("A valid email address is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (existing == null && password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(password) && password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (role == Staff && !branchId.HasValue)
            {
                MessageBox.Show("Staff users must be assigned to a branch.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (role == SuperAdmin)
                branchId = null;

            btnSave.Enabled = false;
            Cursor = Cursors.WaitCursor;

            HttpResponseMessage response = existing == null
                ? await _api.CreateUserAsync(
                    UserSession.CompanyId,
                    new CreateUserRequest(
                        username,
                        fullName,
                        email,
                        password,
                        role,
                        branchId,
                        chkActive.Checked))
                : await _api.UpdateUserAsync(
                    UserSession.CompanyId,
                    existing.UserId,
                    new UpdateUserRequest(
                        username,
                        fullName,
                        email,
                        string.IsNullOrWhiteSpace(password) ? null : password,
                        role,
                        branchId,
                        chkActive.Checked));

            Cursor = Cursors.Default;
            btnSave.Enabled = true;

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                MessageBox.Show(
                    string.IsNullOrWhiteSpace(message)
                        ? "The user could not be saved."
                        : message.Trim('"'),
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        if (form.ShowDialog(FindForm()) == DialogResult.OK)
            await LoadUsersAsync();
    }

    private static string[] GetAvailableRoles(UserListItem? existing)
    {
        if (string.Equals(
                UserSession.Role,
                SuperAdmin,
                StringComparison.OrdinalIgnoreCase))
        {
            return new[] { Admin };
        }

        return new[] { Staff };
    }

    private static bool IsManagementRole(string role)
    {
        return string.Equals(role, SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase);
    }

    private static int? ParseBranchId(string? item)
    {
        if (string.IsNullOrWhiteSpace(item) ||
            string.Equals(item, "None", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var separator = item.IndexOf('|');

        if (separator < 0)
            return null;

        return int.TryParse(item[..separator], out var branchId)
            ? branchId
            : null;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return string.Equals(
                address.Address,
                email,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static TextBox AddField(
        Form parent,
        string labelText,
        string? value,
        ref int y)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Location = new Point(30, y),
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(75, 85, 99)
        };

        var text = new TextBox
        {
            Text = value ?? string.Empty,
            Location = new Point(30, y + 22),
            Width = 400,
            Font = new Font("Segoe UI", 10F),
            BorderStyle = BorderStyle.FixedSingle
        };

        parent.Controls.Add(label);
        parent.Controls.Add(text);

        y += 62;

        return text;
    }

    private static TextBox AddPasswordField(
        Form parent,
        string labelText,
        ref int y)
    {
        var text = AddField(parent, labelText, null, ref y);
        text.UseSystemPasswordChar = true;
        return text;
    }

    private static ComboBox AddRoleCombo(
        Form parent,
        string labelText,
        string[] items,
        string value,
        ref int y)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Location = new Point(30, y),
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(75, 85, 99)
        };

        var combo = new ComboBox
        {
            Location = new Point(30, y + 22),
            Width = 400,
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        combo.Items.AddRange(items);
        combo.SelectedItem = combo.Items.Contains(value)
            ? value
            : items[0];

        parent.Controls.Add(label);
        parent.Controls.Add(combo);

        y += 62;
        return combo;
    }

    private static ComboBox AddBranchCombo(
        Form parent,
        string labelText,
        IReadOnlyList<Branch> branches,
        int? branchId,
        ref int y)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Location = new Point(30, y),
            Font = new Font("Segoe UI Semibold", 9F),
            ForeColor = Color.FromArgb(75, 85, 99)
        };

        var combo = new ComboBox
        {
            Location = new Point(30, y + 22),
            Width = 400,
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        combo.Items.Add(new BranchChoice(null, "None"));

        foreach (var branch in branches)
            combo.Items.Add(new BranchChoice(branch.BranchId, branch.BranchName));

        var selected = combo.Items
            .OfType<BranchChoice>()
            .FirstOrDefault(x => x.Id == branchId);

        combo.SelectedItem = selected ?? combo.Items[0];

        parent.Controls.Add(label);
        parent.Controls.Add(combo);

        y += 62;
        return combo;
    }

    private sealed record BranchChoice(int? Id, string Name)
    {
        public override string ToString() => Name;
    }

    private void ShowStatus(string message, bool error)
    {
        _lblStatus.Text = message;
        _lblStatus.ForeColor = error
            ? Color.FromArgb(220, 38, 38)
            : Color.FromArgb(107, 114, 128);
    }
}
