using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SettingsManager;
using SettingsManager.Macro;
using SimpleLogger;

namespace UI2.Macro.UserControls;

public class Login : UserControl
{
	private readonly SettingsManager.Macro.Login _settings = Settings.Instance.Macro.Login;

	private readonly Logger _logger = Logger.Instance;

	private readonly Color _backgroundColor = Color.FromArgb(38, 38, 45);

	private readonly Color _surfaceColor = Color.FromArgb(48, 48, 55);

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private readonly Color _successColor = Color.FromArgb(0, 153, 102);

	private readonly Color _errorColor = Color.FromArgb(200, 60, 60);

	private IContainer components = null;

	private Label labelHeader;

	private Label labelShortcut;

	private GroupBox groupBoxLogin;

	private TextBox textBoxPass;

	private Label labelPass;

	private TextBox textBoxID;

	private Label labelID;

	private ToolTip toolTip;

	public Login()
	{
		InitializeComponent();
		LoadSettings();
		SetupEventHandlers();
	}

	private void SetupEventHandlers()
	{
		textBoxID.MouseEnter += delegate
		{
			textBoxID.BackColor = Color.FromArgb(70, 70, 75);
		};
		textBoxID.MouseLeave += delegate
		{
			textBoxID.BackColor = Color.FromArgb(60, 60, 65);
		};
		textBoxPass.MouseEnter += delegate
		{
			textBoxPass.BackColor = Color.FromArgb(70, 70, 75);
		};
		textBoxPass.MouseLeave += delegate
		{
			textBoxPass.BackColor = Color.FromArgb(60, 60, 65);
		};
	}

	public void SaveSettings()
	{
		try
		{
			_settings.UserID = textBoxID.Text;
			_settings.UserPassword = textBoxPass.Text;
			_logger.LogDebug("Login settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving Login settings: " + ex.Message);
		}
	}

	private void LoadSettings()
	{
		try
		{
			textBoxID.Text = _settings.UserID;
			textBoxPass.Text = _settings.UserPassword;
			_logger.LogDebug("Login settings loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading Login settings: " + ex.Message);
		}
	}

	private void textBoxID_TextChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.UserID = textBoxID.Text;
			_logger.LogDebug("UserID setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing UserID: " + ex.Message);
		}
	}

	private void textBoxPass_TextChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.UserPassword = textBoxPass.Text;
			_logger.LogDebug("UserPassword setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing UserPassword: " + ex.Message);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.labelHeader = new System.Windows.Forms.Label();
		this.labelShortcut = new System.Windows.Forms.Label();
		this.groupBoxLogin = new System.Windows.Forms.GroupBox();
		this.textBoxPass = new System.Windows.Forms.TextBox();
		this.labelPass = new System.Windows.Forms.Label();
		this.textBoxID = new System.Windows.Forms.TextBox();
		this.labelID = new System.Windows.Forms.Label();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxLogin.SuspendLayout();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = UI2.AppFonts.Header(13f);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(570, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Login Settings";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelShortcut.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelShortcut.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelShortcut.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic);
		this.labelShortcut.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelShortcut.Location = new System.Drawing.Point(0, 25);
		this.labelShortcut.Name = "labelShortcut";
		this.labelShortcut.Size = new System.Drawing.Size(570, 15);
		this.labelShortcut.TabIndex = 1;
		this.labelShortcut.Text = "Shortcut: NumLock";
		this.labelShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxLogin.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxLogin.Controls.Add(this.textBoxPass);
		this.groupBoxLogin.Controls.Add(this.labelPass);
		this.groupBoxLogin.Controls.Add(this.textBoxID);
		this.groupBoxLogin.Controls.Add(this.labelID);
		this.groupBoxLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxLogin.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxLogin.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxLogin.Location = new System.Drawing.Point(15, 50);
		this.groupBoxLogin.Name = "groupBoxLogin";
		this.groupBoxLogin.Size = new System.Drawing.Size(540, 130);
		this.groupBoxLogin.TabIndex = 2;
		this.groupBoxLogin.TabStop = false;
		this.groupBoxLogin.Text = "Login Information";
		this.textBoxPass.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.textBoxPass.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.textBoxPass.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.textBoxPass.Location = new System.Drawing.Point(20, 90);
		this.textBoxPass.Name = "textBoxPass";
#if !NET48
		this.textBoxPass.PlaceholderText = "Enter password";
#endif
		this.textBoxPass.Size = new System.Drawing.Size(500, 20);
		this.textBoxPass.TabIndex = 3;
		this.toolTip.SetToolTip(this.textBoxPass, "Enter your password");
		this.textBoxPass.TextChanged += new System.EventHandler(textBoxPass_TextChanged);
		this.labelPass.AutoSize = true;
		this.labelPass.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.labelPass.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelPass.Location = new System.Drawing.Point(20, 70);
		this.labelPass.Name = "labelPass";
		this.labelPass.Size = new System.Drawing.Size(60, 15);
		this.labelPass.TabIndex = 2;
		this.labelPass.Text = "Password:";
		this.textBoxID.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.textBoxID.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.textBoxID.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.textBoxID.Location = new System.Drawing.Point(20, 40);
		this.textBoxID.Name = "textBoxID";
#if !NET48
		this.textBoxID.PlaceholderText = "Enter user ID";
#endif
		this.textBoxID.Size = new System.Drawing.Size(500, 20);
		this.textBoxID.TabIndex = 1;
		this.toolTip.SetToolTip(this.textBoxID, "Enter your user ID");
		this.textBoxID.TextChanged += new System.EventHandler(textBoxID_TextChanged);
		this.labelID.AutoSize = true;
		this.labelID.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.labelID.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelID.Location = new System.Drawing.Point(20, 20);
		this.labelID.Name = "labelID";
		this.labelID.Size = new System.Drawing.Size(47, 15);
		this.labelID.TabIndex = 0;
		this.labelID.Text = "User ID:";
		this.toolTip.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.toolTip.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.Controls.Add(this.groupBoxLogin);
		base.Controls.Add(this.labelShortcut);
		base.Controls.Add(this.labelHeader);
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		this.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		base.Name = "Login";
		base.Size = new System.Drawing.Size(570, 300);
		this.groupBoxLogin.ResumeLayout(false);
		this.groupBoxLogin.PerformLayout();
		base.ResumeLayout(false);
	}
}
