using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Scanix4;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;

namespace UI2.ScreenCapture.UserControls;

public class RepairWeapons : UserControl, IServiceControl
{
	private readonly SettingsManager.ScreenCapture.RepairWeapons _settings;

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

	private ToolTip toolTip;

	public bool IsActive => checkActive.Checked;

	public RepairWeapons(WorkflowEngine workflowEngine)
	{
		InitializeComponent();
		_workflowEngine = workflowEngine ?? throw new ArgumentNullException("workflowEngine");
		_logger = Logger.Instance;
		_settings = Settings.Instance.ScreenCapture.RepairWeapons;
		LoadSettings();
		SetupEventHandlers();
	}

	private void LoadSettings()
	{
		try
		{
			checkActive.Checked = _settings.IsActive;
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
			_logger.LogDebug("RepairWeapons settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving settings: " + ex.Message);
		}
	}

	private async void checkActive_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			UpdateStatus();
			if (checkActive.Checked)
			{
				await _workflowEngine.StartAsync("RepairWeapons");
				_logger.LogInformation("RepairWeapons service started");
			}
			else
			{
				await _workflowEngine.StopAsync("RepairWeapons");
				_logger.LogInformation("RepairWeapons service stopped");
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
		this.labelHeader.Text = "REPAIR WEAPONS";
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
		this.groupBoxSettings.Controls.Add(this.checkActive);
		this.groupBoxSettings.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBoxSettings.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(0, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Padding = new System.Windows.Forms.Padding(10);
		this.groupBoxSettings.Size = new System.Drawing.Size(380, 150);
		this.groupBoxSettings.TabIndex = 2;
		this.checkActive.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkActive.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkActive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 153, 102);
		this.checkActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkActive.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkActive.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkActive.Location = new System.Drawing.Point(20, 25);
		this.checkActive.Name = "checkActive";
		this.checkActive.Size = new System.Drawing.Size(200, 30);
		this.checkActive.TabIndex = 0;
		this.checkActive.Text = "Enable";
		this.checkActive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkActive, "Enable weapon repair monitoring");
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
		base.Name = "RepairWeapons";
		base.Size = new System.Drawing.Size(380, 200);
		this.groupBoxSettings.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
