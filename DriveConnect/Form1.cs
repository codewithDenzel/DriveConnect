using System;
using System.Windows.Forms;
using DriveConnect.winforms.Forms;
using DriveConnect.winforms.Modules.Admin.CustomerRelationship.Controls;
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

            var crmControl = new CustomerRecordsControl
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(crmControl);
            Text = $"DriveConnect CRM System - {UserSession.Role}";
        }
    }
}
