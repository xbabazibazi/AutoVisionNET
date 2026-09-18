using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Scanix4;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;

namespace UI2.ScreenCapture.UserControls;

public class SnapNetReReRe : UserControl, IServiceControl
{
	private readonly ReReRe _settings;

	private readonly WorkflowEngine _workflowEngine;

	private readonly Logger _logger;

	private readonly Color _backgroundColor = Color.FromArgb(38, 38, 45);

	private readonly Color _surfaceColor = Color.FromArgb(48, 48, 55);

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private readonly Color _successColor = Color.FromArgb(0, 153, 102);

	private readonly Color _errorColor = Color.FromArgb(200, 60, 60);

	private IContainer components = null;

	private Label labelHeader;

	private Label labelStatus;

	private Panel groupBoxSettings;

	private CheckBox checkActive;

	private CheckBox checkUndy;

	private CheckBox check300Ac;

	private CheckBox checkWolf;

	private CheckBox checkSw;

	private ToolTip toolTip;

	public bool IsActive => checkActive.Checked;

	public SnapNetReReRe(WorkflowEngine workflowEngine)
	{
		InitializeComponent();
		_workflowEngine = workflowEngine ?? throw new ArgumentNullException("workflowEngine");
		_logger = Logger.Instance;
		_settings = Settings.Instance.ScreenCapture.ReReRe;
		LoadSettings();
		SetupEventHandlers();
	}

	private void LoadSettings()
	{
		try
		{
			checkActive.Checked = _settings.IsActive;
			checkUndy.Checked = _settings.ReReReUndy;
			check300Ac.Checked = _settings.ReReRe300Ac;
			checkWolf.Checked = _settings.ReReReWolf;
			checkSw.Checked = _settings.ReReReSw;
			UpdateStatus();
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading settings: " + ex.Message);
		}
	}

	private void SetupEventHandlers()
	{
		checkActive.MouseEnter += delegate
		{
			UpdateButtonHover(checkActive, isHover: true);
		};
		checkActive.MouseLeave += delegate
		{
			UpdateButtonHover(checkActive, isHover: false);
		};
		checkUndy.MouseEnter += delegate
		{
			UpdateButtonHover(checkUndy, isHover: true);
		};
		checkUndy.MouseLeave += delegate
		{
			UpdateButtonHover(checkUndy, isHover: false);
		};
		check300Ac.MouseEnter += delegate
		{
			UpdateButtonHover(check300Ac, isHover: true);
		};
		check300Ac.MouseLeave += delegate
		{
			UpdateButtonHover(check300Ac, isHover: false);
		};
		checkWolf.MouseEnter += delegate
		{
			UpdateButtonHover(checkWolf, isHover: true);
		};
		checkWolf.MouseLeave += delegate
		{
			UpdateButtonHover(checkWolf, isHover: false);
		};
		checkSw.MouseEnter += delegate
		{
			UpdateButtonHover(checkSw, isHover: true);
		};
		checkSw.MouseLeave += delegate
		{
			UpdateButtonHover(checkSw, isHover: false);
		};
	}

	private void UpdateButtonHover(CheckBox button, bool isHover)
	{
		if (!button.Checked)
		{
			button.BackColor = (isHover ? Color.FromArgb(70, 70, 75) : Color.FromArgb(60, 60, 65));
		}
	}

	private void UpdateStatus()
	{
		if (base.InvokeRequired)
		{
			Invoke(UpdateStatus);
			return;
		}
		try
		{
			labelStatus.Text = (checkActive.Checked ? "● Active" : "● Inactive");
			labelStatus.ForeColor = (checkActive.Checked ? _successColor : _errorColor);
		}
		catch (Exception ex)
		{
			_logger.LogError("Error updating status: " + ex.Message);
		}
	}

	public void SaveSettings()
	{
		try
		{
			_settings.IsActive = checkActive.Checked;
			_settings.ReReReUndy = checkUndy.Checked;
			_settings.ReReRe300Ac = check300Ac.Checked;
			_settings.ReReReWolf = checkWolf.Checked;
			_settings.ReReReSw = checkSw.Checked;
			_logger.LogDebug("SnapNetReReRe settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogInformation("Error saving settings: " + ex.Message);
		}
	}

	private async void checkActive_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			UpdateStatus();
			if (checkActive.Checked)
			{
				_logger.LogInformation("SnapNetReReRe service started");
			}
			else
			{
				_logger.LogInformation("SnapNetReReRe service stopped");
			}
			_settings.IsActive = checkActive.Checked;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.LogError("Error changing service state: " + ex2.Message);
			if (base.InvokeRequired)
			{
				Invoke((MethodInvoker)delegate
				{
					checkActive.Checked = !checkActive.Checked;
				});
			}
			else
			{
				checkActive.Checked = !checkActive.Checked;
			}
		}
	}

	private void checkUndy_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.ReReReUndy = checkUndy.Checked;
			_logger.LogDebug($"ReReReUndy setting: {checkUndy.Checked}");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing ReReReUndy setting: " + ex.Message);
		}
	}

	private void check300Ac_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.ReReRe300Ac = check300Ac.Checked;
			_logger.LogDebug($"ReReRe300Ac setting: {check300Ac.Checked}");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing ReReRe300Ac setting: " + ex.Message);
		}
	}

	private void checkWolf_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.ReReReWolf = checkWolf.Checked;
			_logger.LogDebug($"ReReReWolf setting: {checkWolf.Checked}");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing ReReReWolf setting: " + ex.Message);
		}
	}

	private void checkSw_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.ReReReSw = checkSw.Checked;
			_logger.LogDebug($"ReReReSw setting: {checkSw.Checked}");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing ReReReSw setting: " + ex.Message);
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
		this.labelStatus = new System.Windows.Forms.Label();
		this.groupBoxSettings = new System.Windows.Forms.Panel();
		this.checkSw = new System.Windows.Forms.CheckBox();
		this.checkWolf = new System.Windows.Forms.CheckBox();
		this.check300Ac = new System.Windows.Forms.CheckBox();
		this.checkUndy = new System.Windows.Forms.CheckBox();
		this.checkActive = new System.Windows.Forms.CheckBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxSettings.SuspendLayout();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = UI2.AppFonts.Header(13f);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(380, 30);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "SNAP NET RERERE";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelStatus.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelStatus.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.labelStatus.Location = new System.Drawing.Point(0, 30);
		this.labelStatus.Name = "labelStatus";
		this.labelStatus.Size = new System.Drawing.Size(380, 20);
		this.labelStatus.TabIndex = 1;
		this.labelStatus.Text = "● Inactive";
		this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxSettings.Controls.Add(this.checkSw);
		this.groupBoxSettings.Controls.Add(this.checkWolf);
		this.groupBoxSettings.Controls.Add(this.check300Ac);
		this.groupBoxSettings.Controls.Add(this.checkUndy);
		this.groupBoxSettings.Controls.Add(this.checkActive);
		this.groupBoxSettings.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBoxSettings.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(0, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Padding = new System.Windows.Forms.Padding(10);
		this.groupBoxSettings.Size = new System.Drawing.Size(380, 150);
		this.groupBoxSettings.TabIndex = 2;
		this.checkSw.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkSw.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkSw.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkSw.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkSw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkSw.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkSw.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkSw.Location = new System.Drawing.Point(20, 145);
		this.checkSw.Name = "checkSw";
		this.checkSw.Size = new System.Drawing.Size(200, 30);
		this.checkSw.TabIndex = 4;
		this.checkSw.Text = "Sw";
		this.checkSw.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkSw, "Enable Sw option for ReReRe");
		this.checkSw.UseVisualStyleBackColor = false;
		this.checkSw.CheckedChanged += new System.EventHandler(checkSw_CheckedChanged);
		this.checkWolf.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkWolf.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkWolf.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkWolf.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkWolf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkWolf.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkWolf.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkWolf.Location = new System.Drawing.Point(20, 110);
		this.checkWolf.Name = "checkWolf";
		this.checkWolf.Size = new System.Drawing.Size(200, 30);
		this.checkWolf.TabIndex = 3;
		this.checkWolf.Text = "Wolf";
		this.checkWolf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkWolf, "Enable Wolf option for ReReRe");
		this.checkWolf.UseVisualStyleBackColor = false;
		this.checkWolf.CheckedChanged += new System.EventHandler(checkWolf_CheckedChanged);
		this.check300Ac.Appearance = System.Windows.Forms.Appearance.Button;
		this.check300Ac.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.check300Ac.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.check300Ac.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.check300Ac.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.check300Ac.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.check300Ac.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.check300Ac.Location = new System.Drawing.Point(20, 75);
		this.check300Ac.Name = "check300Ac";
		this.check300Ac.Size = new System.Drawing.Size(200, 30);
		this.check300Ac.TabIndex = 2;
		this.check300Ac.Text = "300Ac";
		this.check300Ac.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.check300Ac, "Enable 300Ac option for ReReRe");
		this.check300Ac.UseVisualStyleBackColor = false;
		this.check300Ac.CheckedChanged += new System.EventHandler(check300Ac_CheckedChanged);
		this.checkUndy.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkUndy.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkUndy.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkUndy.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkUndy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkUndy.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkUndy.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkUndy.Location = new System.Drawing.Point(20, 40);
		this.checkUndy.Name = "checkUndy";
		this.checkUndy.Size = new System.Drawing.Size(200, 30);
		this.checkUndy.TabIndex = 1;
		this.checkUndy.Text = "Undy";
		this.checkUndy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkUndy, "Enable Undy option for ReReRe");
		this.checkUndy.UseVisualStyleBackColor = false;
		this.checkUndy.CheckedChanged += new System.EventHandler(checkUndy_CheckedChanged);
		this.checkActive.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkActive.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkActive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 153, 102);
		this.checkActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkActive.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkActive.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkActive.Location = new System.Drawing.Point(20, 5);
		this.checkActive.Name = "checkActive";
		this.checkActive.Size = new System.Drawing.Size(200, 30);
		this.checkActive.TabIndex = 0;
		this.checkActive.Text = "Enable";
		this.checkActive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkActive, "Enable SnapNet ReReRe monitoring");
		this.checkActive.UseVisualStyleBackColor = false;
		this.checkActive.CheckedChanged += new System.EventHandler(checkActive_CheckedChanged);
		this.toolTip.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.toolTip.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.Controls.Add(this.groupBoxSettings);
		base.Controls.Add(this.labelStatus);
		base.Controls.Add(this.labelHeader);
		this.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		base.Name = "SnapNetReReRe";
		base.Size = new System.Drawing.Size(380, 200);
		this.groupBoxSettings.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
