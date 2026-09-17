using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SettingsManager;
using SettingsManager.Macro;
using SimpleLogger;

namespace UI2.Macro.UserControls;

public class UndyAc : UserControl
{
	private readonly SettingsManager.Macro.UndyAc _settings = Settings.Instance.Macro.UndyAc;

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

	private CheckBox checkBoxUndyAcLoop;

	private NumericUpDown numericUpDownUndyAcSkillDelay;

	private Label labelSkillDelay;

	private NumericUpDown numericUpDownUndyAcTabDelay;

	private Label labelTabDelay;

	private ToolTip toolTip;

	public UndyAc()
	{
		InitializeComponent();
		LoadSettings();
		SetupEventHandlers();
	}

	private void SetupEventHandlers()
	{
		checkBoxUndyAcLoop.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxUndyAcLoop, isHover: true);
		};
		checkBoxUndyAcLoop.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxUndyAcLoop, isHover: false);
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
			_settings.UndyAcTabDelay = numericUpDownUndyAcTabDelay.Value;
			_settings.UndyAcSkillDelay = numericUpDownUndyAcSkillDelay.Value;
			_settings.UndyAcLoop = checkBoxUndyAcLoop.Checked;
			_logger.LogDebug("UndyAc settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving UndyAc settings: " + ex.Message);
		}
	}

	private void LoadSettings()
	{
		try
		{
			numericUpDownUndyAcSkillDelay.Value = _settings.UndyAcSkillDelay;
			numericUpDownUndyAcTabDelay.Value = _settings.UndyAcTabDelay;
			checkBoxUndyAcLoop.Checked = _settings.UndyAcLoop;
			_logger.LogDebug("UndyAc settings loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading UndyAc settings: " + ex.Message);
		}
	}

	private void numericUpDownUndyAcTabDelay_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.UndyAcTabDelay = numericUpDownUndyAcTabDelay.Value;
			_logger.LogDebug("UndyAcTabDelay setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing UndyAcTabDelay: " + ex.Message);
		}
	}

	private void numericUpDownUndyAcSkillDelay_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.UndyAcSkillDelay = numericUpDownUndyAcSkillDelay.Value;
			_logger.LogDebug("UndyAcSkillDelay setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing UndyAcSkillDelay: " + ex.Message);
		}
	}

	private void checkBoxUndyAcLoop_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.UndyAcLoop = checkBoxUndyAcLoop.Checked;
			_logger.LogDebug("UndyAcLoop setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing UndyAcLoop: " + ex.Message);
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
		this.checkBoxUndyAcLoop = new System.Windows.Forms.CheckBox();
		this.numericUpDownUndyAcSkillDelay = new System.Windows.Forms.NumericUpDown();
		this.labelSkillDelay = new System.Windows.Forms.Label();
		this.numericUpDownUndyAcTabDelay = new System.Windows.Forms.NumericUpDown();
		this.labelTabDelay = new System.Windows.Forms.Label();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxSettings.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownUndyAcSkillDelay).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownUndyAcTabDelay).BeginInit();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(570, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Undy/Ac Settings";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelShortcut.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelShortcut.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelShortcut.Font = new System.Drawing.Font("Tahoma", 8f, System.Drawing.FontStyle.Italic);
		this.labelShortcut.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelShortcut.Location = new System.Drawing.Point(0, 25);
		this.labelShortcut.Name = "labelShortcut";
		this.labelShortcut.Size = new System.Drawing.Size(570, 15);
		this.labelShortcut.TabIndex = 1;
		this.labelShortcut.Text = "Shortcut: Ctrl + Q";
		this.labelShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxSettings.Controls.Add(this.checkBoxUndyAcLoop);
		this.groupBoxSettings.Controls.Add(this.numericUpDownUndyAcSkillDelay);
		this.groupBoxSettings.Controls.Add(this.labelSkillDelay);
		this.groupBoxSettings.Controls.Add(this.numericUpDownUndyAcTabDelay);
		this.groupBoxSettings.Controls.Add(this.labelTabDelay);
		this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxSettings.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(15, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Size = new System.Drawing.Size(540, 150);
		this.groupBoxSettings.TabIndex = 2;
		this.groupBoxSettings.TabStop = false;
		this.groupBoxSettings.Text = "Undy/Ac Settings";
		this.checkBoxUndyAcLoop.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxUndyAcLoop.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxUndyAcLoop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxUndyAcLoop.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(55, 78, 92);
		this.checkBoxUndyAcLoop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxUndyAcLoop.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBoxUndyAcLoop.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxUndyAcLoop.Location = new System.Drawing.Point(20, 100);
		this.checkBoxUndyAcLoop.Name = "checkBoxUndyAcLoop";
		this.checkBoxUndyAcLoop.Size = new System.Drawing.Size(200, 30);
		this.checkBoxUndyAcLoop.TabIndex = 4;
		this.checkBoxUndyAcLoop.Text = "Apply to Whole Party";
		this.checkBoxUndyAcLoop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxUndyAcLoop, "Apply Undy/Ac to all party members");
		this.checkBoxUndyAcLoop.UseVisualStyleBackColor = false;
		this.checkBoxUndyAcLoop.CheckedChanged += new System.EventHandler(checkBoxUndyAcLoop_CheckedChanged);
		this.numericUpDownUndyAcSkillDelay.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.numericUpDownUndyAcSkillDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.numericUpDownUndyAcSkillDelay.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDownUndyAcSkillDelay.Location = new System.Drawing.Point(120, 60);
		this.numericUpDownUndyAcSkillDelay.Maximum = new decimal(new int[4] { 5000, 0, 0, 0 });
		this.numericUpDownUndyAcSkillDelay.Minimum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.numericUpDownUndyAcSkillDelay.Name = "numericUpDownUndyAcSkillDelay";
		this.numericUpDownUndyAcSkillDelay.Size = new System.Drawing.Size(120, 22);
		this.numericUpDownUndyAcSkillDelay.TabIndex = 3;
		this.numericUpDownUndyAcSkillDelay.Value = new decimal(new int[4] { 10, 0, 0, 0 });
		this.toolTip.SetToolTip(this.numericUpDownUndyAcSkillDelay, "Set skill delay in milliseconds");
		this.numericUpDownUndyAcSkillDelay.ValueChanged += new System.EventHandler(numericUpDownUndyAcSkillDelay_ValueChanged);
		this.labelSkillDelay.AutoSize = true;
		this.labelSkillDelay.Font = new System.Drawing.Font("Tahoma", 8f);
		this.labelSkillDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelSkillDelay.Location = new System.Drawing.Point(20, 62);
		this.labelSkillDelay.Name = "labelSkillDelay";
		this.labelSkillDelay.Size = new System.Drawing.Size(90, 15);
		this.labelSkillDelay.TabIndex = 2;
		this.labelSkillDelay.Text = "Skill Delay (ms):";
		this.numericUpDownUndyAcTabDelay.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.numericUpDownUndyAcTabDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.numericUpDownUndyAcTabDelay.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.numericUpDownUndyAcTabDelay.Location = new System.Drawing.Point(120, 20);
		this.numericUpDownUndyAcTabDelay.Maximum = new decimal(new int[4] { 5000, 0, 0, 0 });
		this.numericUpDownUndyAcTabDelay.Minimum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.numericUpDownUndyAcTabDelay.Name = "numericUpDownUndyAcTabDelay";
		this.numericUpDownUndyAcTabDelay.Size = new System.Drawing.Size(120, 22);
		this.numericUpDownUndyAcTabDelay.TabIndex = 1;
		this.numericUpDownUndyAcTabDelay.Value = new decimal(new int[4] { 10, 0, 0, 0 });
		this.toolTip.SetToolTip(this.numericUpDownUndyAcTabDelay, "Set tab delay in milliseconds");
		this.numericUpDownUndyAcTabDelay.ValueChanged += new System.EventHandler(numericUpDownUndyAcTabDelay_ValueChanged);
		this.labelTabDelay.AutoSize = true;
		this.labelTabDelay.Font = new System.Drawing.Font("Tahoma", 8f);
		this.labelTabDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelTabDelay.Location = new System.Drawing.Point(20, 22);
		this.labelTabDelay.Name = "labelTabDelay";
		this.labelTabDelay.Size = new System.Drawing.Size(87, 15);
		this.labelTabDelay.TabIndex = 0;
		this.labelTabDelay.Text = "Tab Delay (ms):";
		this.toolTip.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.toolTip.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.Controls.Add(this.groupBoxSettings);
		base.Controls.Add(this.labelShortcut);
		base.Controls.Add(this.labelHeader);
		this.Dock = System.Windows.Forms.DockStyle.Fill;
		this.Font = new System.Drawing.Font("Tahoma", 9.75f);
		base.Name = "UndyAc";
		base.Size = new System.Drawing.Size(570, 300);
		this.groupBoxSettings.ResumeLayout(false);
		this.groupBoxSettings.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownUndyAcSkillDelay).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownUndyAcTabDelay).EndInit();
		base.ResumeLayout(false);
	}
}
