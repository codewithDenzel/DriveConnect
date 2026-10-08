using System;
using System.Drawing;
using System.Windows.Forms;
using DriveConnect.winforms.Forms;
using DriveConnect.winforms.Modules.Admin.CustomerRelationship.Controls;
using DriveConnect.winforms.Modules.SystemAdministration.Controls;
using DriveConnect.winforms.Services;

namespace DriveConnect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.Sizable;
            Text = "DriveConnect CRM System";
            Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            ShowLogin();
        }

        private void ShowLogin()
        {
            using var loginForm = new LoginForm();

            if (loginForm.ShowDialog(this) != DialogResult.OK)
            {
                Close();
                return;
            }

            ShowMainSystem();
        }

        private void ShowMainSystem()
        {
            Controls.Clear();

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246)
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White,
                Padding = new Padding(10, 7, 12, 7)
            };

            var logoutButton = new Button
            {
                Text = "Log Out",
                Dock = DockStyle.Right,
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(75, 85, 99),
                Font = new Font("Segoe UI Semibold", 9F),
                Cursor = Cursors.Hand
            };

            logoutButton.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
            logoutButton.FlatAppearance.BorderSize = 1;
            logoutButton.Click += (_, _) =>
            {
                UserSession.Clear();
                Controls.Clear();
                Text = "DriveConnect CRM System";
                ShowLogin();
            };

            header.Controls.Add(logoutButton);

            if (string.Equals(
                    UserSession.Role,
                    "Super Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                var superAdminControl = new SuperAdminControl
                {
                    Dock = DockStyle.Fill
                };

                // Add the fill control first so the top logout header is never covered.
                mainPanel.Controls.Add(superAdminControl);
                mainPanel.Controls.Add(header);
                header.BringToFront();

                Text = "DriveConnect - Super Admin";
            }
            else
            {
                var crmControl = new CustomerRecordsControl
                {
                    Dock = DockStyle.Fill
                };

                // Keep the shared logout header above the CRM workspace.
                mainPanel.Controls.Add(crmControl);
                mainPanel.Controls.Add(header);
                header.BringToFront();

                Text = $"DriveConnect CRM System - {UserSession.Role}";
            }

            Controls.Add(mainPanel);
        }
    }
}