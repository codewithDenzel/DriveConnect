using System;
using System.Windows.Forms;
using DriveConnect.winforms.Modules.Admin.CustomerRelationship.Controls;

namespace DriveConnect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Lock as Fullscreen
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable; // Keeps standard window controls if needed, or set to None for borderless kiosk
            this.Text = "DriveConnect CRM System";
            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            this.Controls.Clear();
            var crmControl = new CustomerRecordsControl
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(crmControl);
        }
    }
}