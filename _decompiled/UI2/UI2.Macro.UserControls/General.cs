using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SettingsManager;
using SettingsManager.Macro;
using SimpleLogger;

namespace UI2.Macro.UserControls;

public class General : UserControl
{
	private readonly SettingsManager.Macro.General _settings = Settings.Instance.Macro.General;

	private readonly Logger _logger = Logger.Instance;

	private readonly Color _backgroundColor = Color.FromArgb(38, 38, 45);

	private readonly Color _surfaceColor = Color.FromArgb(48, 48, 55);

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private readonly Color _successColor = Color.FromArgb(0, 153, 102);

	private readonly Color _errorColor = Color.FromArgb(200, 60, 60);

	private IContainer components = null;

	private Label labelHeader;

	private Label labelShortcut;

	private GroupBox groupBoxSettings;

	private CheckBox checkBoxStartGenieAfterTp;

	private ToolTip toolTip;

	public General()
	{
		InitializeComponent();
		LoadSettings();
		SetupEventHandlers();
	}

	private void SetupEventHandlers()
	{
		checkBoxStartGenieAfterTp.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxStartGenieAfterTp, isHover: true);
		};
		checkBoxStartGenieAfterTp.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxStartGenieAfterTp, isHover: false);
		};
	}

	private void UpdateButtonHover(CheckBox button, bool isHover)
	{
		if (!button.Checked)
		{
			button.BackColor = (isHover ? Color.FromArgb(70, 70, 75) : Color.FromArgb(60, 60, 65));
		}
	}

	public void SaveSettings()
	{
		try
		{
			_settings.StartGenieOnTp = checkBoxStartGenieAfterTp.Checked;
			_logger.LogDebug("General settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving General settings: " + ex.Message);
		}
	}

	private void LoadSettings()
	{
		try
		{
			checkBoxStartGenieAfterTp.Checked = _settings.StartGenieOnTp;
			_logger.LogDebug("General settings loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading General settings: " + ex.Message);
		}
	}

	private void checkBoxStartGenieAfterTp_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.StartGenieOnTp = checkBoxStartGenieAfterTp.Checked;
			_logger.LogDebug("StartGenieAfterTp setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing StartGenieAfterTp: " + ex.Message);
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
		this.groupBoxSettings = new System.Windows.Forms.GroupBox();
		this.checkBoxStartGenieAfterTp = new System.Windows.Forms.CheckBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxSettings.SuspendLayout();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = UI2.AppFonts.Header(13f);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(570, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "General Settings";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelShortcut.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelShortcut.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelShortcut.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic);
		this.labelShortcut.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelShortcut.Location = new System.Drawing.Point(0, 25);
		this.labelShortcut.Name = "labelShortcut";
		this.labelShortcut.Size = new System.Drawing.Size(570, 15);
		this.labelShortcut.TabIndex = 1;
		this.labelShortcut.Text = "Shortcuts: Ctrl+X (I Love), Ctrl+Z (I Will)";
		this.labelShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxSettings.Controls.Add(this.checkBoxStartGenieAfterTp);
		this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxSettings.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(15, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Size = new System.Drawing.Size(540, 70);
		this.groupBoxSettings.TabIndex = 2;
		this.groupBoxSettings.TabStop = false;
		this.groupBoxSettings.Text = "Settings";
		this.checkBoxStartGenieAfterTp.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxStartGenieAfterTp.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxStartGenieAfterTp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxStartGenieAfterTp.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(55, 78, 92);
		this.checkBoxStartGenieAfterTp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxStartGenieAfterTp.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkBoxStartGenieAfterTp.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxStartGenieAfterTp.Location = new System.Drawing.Point(20, 25);
		this.checkBoxStartGenieAfterTp.Name = "checkBoxStartGenieAfterTp";
		this.checkBoxStartGenieAfterTp.Size = new System.Drawing.Size(200, 30);
		this.checkBoxStartGenieAfterTp.TabIndex = 0;
		this.checkBoxStartGenieAfterTp.Text = "Start Genie After Teleport";
		this.checkBoxStartGenieAfterTp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxStartGenieAfterTp, "Automatically starts Genie after teleport");
		this.checkBoxStartGenieAfterTp.UseVisualStyleBackColor = false;
		this.checkBoxStartGenieAfterTp.CheckedChanged += new System.EventHandler(checkBoxStartGenieAfterTp_CheckedChanged);
		this.toolTip.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.toolTip.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.Controls.Add(this.groupBoxSettings);
		base.Controls.Add(this.labelShortcut);
		base.Controls.Add(this.labelHeader);
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		this.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		base.Name = "General";
		base.Size = new System.Drawing.Size(570, 300);
		this.groupBoxSettings.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
