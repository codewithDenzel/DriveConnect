using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DriveConnect.domain.Entities;

namespace DriveConnect.winforms.Modules.Admin.CustomerRelationship.Controls;

public partial class CustomerRecordsControl
{
    private async Task LoadBranchesAsync()
    {
        try
        {
            _allBranches = await _apiService.GetBranchesAsync(CurrentCompanyId) ?? new List<Branch>();
        }
        catch
        {
            _allBranches = new List<Branch>();
        }
    }

    private void BindBranchGrid()
    {
        string query = txtSearch.Text.Trim();

        var branches = _allBranches.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            branches = branches.Where(x =>
                x.BranchCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.BranchName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (x.Address ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (x.PhoneNumber ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        gridView.DataSource = branches
            .OrderBy(x => x.BranchName)
            .Select(x => new
            {
                ID = x.BranchId,
                Code = x.BranchCode,
                Branch = x.BranchName,
                Address = string.IsNullOrWhiteSpace(x.Address) ? "-" : x.Address,
                Phone = string.IsNullOrWhiteSpace(x.PhoneNumber) ? "-" : x.PhoneNumber,
                Status = x.IsActive ? "Active" : "Inactive",
                DateAdded = x.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
            })
            .ToList();

        var dateAddedColumn = gridView.Columns["DateAdded"];
        if (dateAddedColumn != null)
            dateAddedColumn.HeaderText = "Date Added";
    }

    private async Task ShowBranchModalAsync(Branch? existing)
    {
        using Form form = CreateBaseModal(
            existing == null ? "New Branch" : "Edit Branch",
            500);

        int y = 70;

        TextBox txtCode = AddFormField(
            form,
            "Branch Code",
            existing?.BranchCode,
            ref y);

        TextBox txtName = AddFormField(
            form,
            "Branch Name",
            existing?.BranchName,
            ref y);

        TextBox txtAddress = AddFormField(
            form,
            "Address (Optional)",
            existing?.Address,
            ref y);

        TextBox txtPhone = AddFormField(
            form,
            "Phone Number (Optional)",
            existing?.PhoneNumber,
            ref y);

        ComboBox cbStatus = AddFormCombo(
            form,
            "Status",
            new[] { "Active", "Inactive" },
            existing?.IsActive == false ? "Inactive" : "Active",
            ref y);

        Button btnSave = AddFormSubmitButton(
            form,
            existing == null ? "Save Branch" : "Update Branch",
            y);

        btnSave.Click += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show(
                    "Branch Code is required.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!Regex.IsMatch(txtCode.Text.Trim(), @"^[A-Za-z0-9-]+$"))
            {
                MessageBox.Show(
                    "Branch Code can contain letters, numbers, and hyphens only.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(
                    "Branch Name is required.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtPhone.Text) &&
                !Regex.IsMatch(txtPhone.Text.Trim(), @"^[0-9]{7,15}$"))
            {
                MessageBox.Show(
                    "Phone Number must contain 7 to 15 digits.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            bool isActive = cbStatus.SelectedItem?.ToString() != "Inactive";

            if (!ConfirmAction(
                    existing == null
                        ? "Confirm adding this branch?"
                        : "Confirm updating this branch?"))
            {
                return;
            }

            Branch branch = existing ?? new Branch();

            branch.BranchCode = txtCode.Text.Trim();
            branch.BranchName = txtName.Text.Trim();
            branch.Address = string.IsNullOrWhiteSpace(txtAddress.Text)
                ? null
                : txtAddress.Text.Trim();
            branch.PhoneNumber = string.IsNullOrWhiteSpace(txtPhone.Text)
                ? null
                : txtPhone.Text.Trim();
            branch.IsActive = isActive;

            HttpResponseMessage response = existing == null
                ? await _apiService.CreateBranchAsync(CurrentCompanyId, branch)
                : await _apiService.UpdateBranchAsync(
                    CurrentCompanyId,
                    branch.BranchId,
                    branch);

            if (!response.IsSuccessStatusCode)
            {
                string message = response.StatusCode == System.Net.HttpStatusCode.Conflict
                    ? "A branch with the same code or name already exists."
                    : "The branch could not be saved.";

                MessageBox.Show(
                    message,
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            form.DialogResult = DialogResult.OK;
        };

        if (form.ShowDialog() == DialogResult.OK)
            await LoadDataFromApiAsync();
    }
}
