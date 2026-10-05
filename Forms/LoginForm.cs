using System.Drawing;
using System.Windows.Forms;
using DriveConnect.winforms.Services;

namespace DriveConnect.winforms.Forms;

public sealed class LoginForm : Form
{
    private readonly TextBox _txtUsername = new();
    private readonly TextBox _txtPassword = new();
    private readonly Button _btnLogin = new();
    private readonly Label _lblStatus = new();
    private readonly AuthApiService _authApi = new();

    public LoginForm()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        Text = "DriveConnect - Login";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(430, 500);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(45)
        };

        var lblLogo = new Label
        {
            Text = "DriveConnect",
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            ForeColor = Color.FromArgb(79, 70, 229),
            AutoSize = true,
            Location = new Point(45, 45)
        };

        var lblTitle = new Label
        {
            Text = "Sign in to your account",
            Font = new Font("Segoe UI Semibold", 12F),
            ForeColor = Color.FromArgb(55, 65, 81),
            AutoSize = true,
            Location = new Point(48, 95)
        };

        var lblUsername = CreateLabel("Username", 48, 150);
        _txtUsername.Location = new Point(48, 174);
        _txtUsername.Width = 330;
        _txtUsername.Height = 34;
        _txtUsername.BorderStyle = BorderStyle.FixedSingle;

        var lblPassword = CreateLabel("Password", 48, 225);
        _txtPassword.Location = new Point(48, 249);
        _txtPassword.Width = 330;
        _txtPassword.Height = 34;
        _txtPassword.BorderStyle = BorderStyle.FixedSingle;
        _txtPassword.UseSystemPasswordChar = true;

        _btnLogin.Text = "Sign In";
        _btnLogin.Location = new Point(48, 310);
        _btnLogin.Width = 330;
        _btnLogin.Height = 45;
        _btnLogin.BackColor = Color.FromArgb(79, 70, 229);
        _btnLogin.ForeColor = Color.White;
        _btnLogin.FlatStyle = FlatStyle.Flat;
        _btnLogin.FlatAppearance.BorderSize = 0;
        _btnLogin.Font = new Font("Segoe UI Semibold", 10F);
        _btnLogin.Cursor = Cursors.Hand;
        _btnLogin.Click += BtnLogin_Click;

        _lblStatus.AutoSize = false;
        _lblStatus.Size = new Size(330, 60);
        _lblStatus.Location = new Point(48, 370);
        _lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
        _lblStatus.TextAlign = ContentAlignment.TopCenter;

        AcceptButton = _btnLogin;

        panel.Controls.Add(lblLogo);
        panel.Controls.Add(lblTitle);
        panel.Controls.Add(lblUsername);
        panel.Controls.Add(_txtUsername);
        panel.Controls.Add(lblPassword);
        panel.Controls.Add(_txtPassword);
        panel.Controls.Add(_btnLogin);
        panel.Controls.Add(_lblStatus);

        Controls.Add(panel);

        Shown += (_, _) => _txtUsername.Focus();
    }

    private static Label CreateLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Location = new Point(x, y),
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = Color.FromArgb(75, 85, 99)
        };
    }

    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        var username = _txtUsername.Text.Trim();
        var password = _txtPassword.Text;

        _lblStatus.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            _lblStatus.Text = "Username is required.";
            _txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            _lblStatus.Text = "Password is required.";
            _txtPassword.Focus();
            return;
        }

        _btnLogin.Enabled = false;
        _btnLogin.Text = "Signing in...";
        Cursor = Cursors.WaitCursor;

        var (response, error) = await _authApi.LoginAsync(username, password);

        Cursor = Cursors.Default;
        _btnLogin.Enabled = true;
        _btnLogin.Text = "Sign In";

        if (response == null)
        {
            _lblStatus.Text = error;
            return;
        }

        UserSession.Set(response);
        DialogResult = DialogResult.OK;
        Close();
    }
}
