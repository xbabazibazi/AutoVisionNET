using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SettingsManager;
using SettingsManager.Macro;
using SimpleLogger;

namespace UI2.Macro.UserControls;

public class Attack : UserControl
{
	private readonly SettingsManager.Macro.Attack _settings = Settings.Instance.Macro.Attack;

	private readonly Logger _logger = Logger.Instance;

	private readonly Color _backgroundColor = Color.FromArgb(38, 38, 45);

	private readonly Color _surfaceColor = Color.FromArgb(48, 48, 55);

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private IContainer components = null;

	private Label labelHeader;

	private Label labelShortcut;

	private GroupBox groupBoxAttackKeys;

	private CheckBox checkBoxSkill;

	private CheckBox checkBoxR;

	private CheckBox checkBoxZ;

	private CheckBox checkBox8;

	private CheckBox checkBox9;

	private GroupBox groupBoxDelays;

	private NumericUpDown numericUpDownRDelay;

	private Label labelRDelay;

	private NumericUpDown numericUpDownSkillDelay;

	private Label labelSkillDelay;

	private GroupBox groupBoxFeatures;

	private CheckBox checkBoxRandomDelay;

	private CheckBox checkBoxGenie;

	private ToolTip toolTip;

	public Attack()
	{
		InitializeComponent();
		LoadSettings();
		SetupEventHandlers();
	}

	private void SetupEventHandlers()
	{
		checkBoxSkill.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxSkill, isHover: true);
		};
		checkBoxSkill.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxSkill, isHover: false);
		};
		checkBoxR.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxR, isHover: true);
		};
		checkBoxR.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxR, isHover: false);
		};
		checkBoxZ.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxZ, isHover: true);
		};
		checkBoxZ.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxZ, isHover: false);
		};
		checkBox8.MouseEnter += delegate
		{
			UpdateButtonHover(checkBox8, isHover: true);
		};
		checkBox8.MouseLeave += delegate
		{
			UpdateButtonHover(checkBox8, isHover: false);
		};
		checkBox9.MouseEnter += delegate
		{
			UpdateButtonHover(checkBox9, isHover: true);
		};
		checkBox9.MouseLeave += delegate
		{
			UpdateButtonHover(checkBox9, isHover: false);
		};
		checkBoxGenie.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxGenie, isHover: true);
		};
		checkBoxGenie.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxGenie, isHover: false);
		};
		checkBoxRandomDelay.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxRandomDelay, isHover: true);
		};
		checkBoxRandomDelay.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxRandomDelay, isHover: false);
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
			_settings.Delay = numericUpDownSkillDelay.Value;
			_settings.RDelay = numericUpDownRDelay.Value;
			_settings.HasSkill = checkBoxSkill.Checked;
			_settings.HasR = checkBoxR.Checked;
			_settings.HasZ = checkBoxZ.Checked;
			_settings.HasEight = checkBox8.Checked;
			_settings.HasNine = checkBox9.Checked;
			_settings.StartAttackOnGenieStart = checkBoxGenie.Checked;
			_settings.IsRandomDelay = checkBoxRandomDelay.Checked;
			_logger.LogDebug("Attack settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving Attack settings: " + ex.Message);
		}
	}

	private void LoadSettings()
	{
		try
		{
			numericUpDownSkillDelay.Value = _settings.Delay;
			numericUpDownRDelay.Value = _settings.RDelay;
			checkBoxSkill.Checked = _settings.HasSkill;
			checkBoxR.Checked = _settings.HasR;
			checkBoxZ.Checked = _settings.HasZ;
			checkBox8.Checked = _settings.HasEight;
			checkBox9.Checked = _settings.HasNine;
			checkBoxGenie.Checked = _settings.StartAttackOnGenieStart;
			checkBoxRandomDelay.Checked = _settings.IsRandomDelay;
			_logger.LogDebug("Attack settings loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading Attack settings: " + ex.Message);
		}
	}

	private void checkBoxSkill_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.HasSkill = checkBoxSkill.Checked;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing HasSkill: " + ex.Message);
		}
	}

	private void checkBoxR_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.HasR = checkBoxR.Checked;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing HasR: " + ex.Message);
		}
	}

	private void checkBoxZ_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.HasZ = checkBoxZ.Checked;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing HasZ: " + ex.Message);
		}
	}

	private void checkBox8_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.HasEight = checkBox8.Checked;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing HasEight: " + ex.Message);
		}
	}

	private void checkBox9_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.HasNine = checkBox9.Checked;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing HasNine: " + ex.Message);
		}
	}

	private void numericUpDownSkillDelay_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.Delay = numericUpDownSkillDelay.Value;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing Delay: " + ex.Message);
		}
	}

	private void numericUpDownRDelay_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.RDelay = numericUpDownRDelay.Value;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing RDelay: " + ex.Message);
		}
	}

	private void checkBoxGenie_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.StartAttackOnGenieStart = checkBoxGenie.Checked;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing StartAttackOnGenieStart: " + ex.Message);
		}
	}

	private void checkBoxRandomDelay_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.IsRandomDelay = checkBoxRandomDelay.Checked;
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing IsRandomDelay: " + ex.Message);
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
		this.groupBoxAttackKeys = new System.Windows.Forms.GroupBox();
		this.checkBox9 = new System.Windows.Forms.CheckBox();
		this.checkBox8 = new System.Windows.Forms.CheckBox();
		this.checkBoxZ = new System.Windows.Forms.CheckBox();
		this.checkBoxR = new System.Windows.Forms.CheckBox();
		this.checkBoxSkill = new System.Windows.Forms.CheckBox();
		this.groupBoxDelays = new System.Windows.Forms.GroupBox();
		this.numericUpDownRDelay = new System.Windows.Forms.NumericUpDown();
		this.labelRDelay = new System.Windows.Forms.Label();
		this.numericUpDownSkillDelay = new System.Windows.Forms.NumericUpDown();
		this.labelSkillDelay = new System.Windows.Forms.Label();
		this.groupBoxFeatures = new System.Windows.Forms.GroupBox();
		this.checkBoxRandomDelay = new System.Windows.Forms.CheckBox();
		this.checkBoxGenie = new System.Windows.Forms.CheckBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxAttackKeys.SuspendLayout();
		this.groupBoxDelays.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownRDelay).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownSkillDelay).BeginInit();
		this.groupBoxFeatures.SuspendLayout();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(2080, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Attack Settings";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelShortcut.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelShortcut.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelShortcut.Font = new System.Drawing.Font("Tahoma", 8f, System.Drawing.FontStyle.Italic);
		this.labelShortcut.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelShortcut.Location = new System.Drawing.Point(0, 25);
		this.labelShortcut.Name = "labelShortcut";
		this.labelShortcut.Size = new System.Drawing.Size(2080, 15);
		this.labelShortcut.TabIndex = 1;
		this.labelShortcut.Text = "Shortcut: Ctrl + A";
		this.labelShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxAttackKeys.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxAttackKeys.Controls.Add(this.checkBox9);
		this.groupBoxAttackKeys.Controls.Add(this.checkBox8);
		this.groupBoxAttackKeys.Controls.Add(this.checkBoxZ);
		this.groupBoxAttackKeys.Controls.Add(this.checkBoxR);
		this.groupBoxAttackKeys.Controls.Add(this.checkBoxSkill);
		this.groupBoxAttackKeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxAttackKeys.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxAttackKeys.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxAttackKeys.Location = new System.Drawing.Point(15, 50);
		this.groupBoxAttackKeys.Name = "groupBoxAttackKeys";
		this.groupBoxAttackKeys.Size = new System.Drawing.Size(540, 70);
		this.groupBoxAttackKeys.TabIndex = 2;
		this.groupBoxAttackKeys.TabStop = false;
		this.groupBoxAttackKeys.Text = "Attack Keys";
		this.checkBox9.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBox9.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBox9.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBox9.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBox9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBox9.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBox9.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBox9.Location = new System.Drawing.Point(436, 20);
		this.checkBox9.Name = "checkBox9";
		this.checkBox9.Size = new System.Drawing.Size(90, 30);
		this.checkBox9.TabIndex = 4;
		this.checkBox9.Text = "9";
		this.checkBox9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBox9, "Use 9 key for attack");
		this.checkBox9.UseVisualStyleBackColor = false;
		this.checkBox9.CheckedChanged += new System.EventHandler(checkBox9_CheckedChanged);
		this.checkBox8.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBox8.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBox8.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBox8.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBox8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBox8.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBox8.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBox8.Location = new System.Drawing.Point(336, 20);
		this.checkBox8.Name = "checkBox8";
		this.checkBox8.Size = new System.Drawing.Size(90, 30);
		this.checkBox8.TabIndex = 3;
		this.checkBox8.Text = "8";
		this.checkBox8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBox8, "Use 8 key for attack");
		this.checkBox8.UseVisualStyleBackColor = false;
		this.checkBox8.CheckedChanged += new System.EventHandler(checkBox8_CheckedChanged);
		this.checkBoxZ.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxZ.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxZ.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxZ.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBoxZ.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxZ.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBoxZ.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxZ.Location = new System.Drawing.Point(236, 20);
		this.checkBoxZ.Name = "checkBoxZ";
		this.checkBoxZ.Size = new System.Drawing.Size(90, 30);
		this.checkBoxZ.TabIndex = 2;
		this.checkBoxZ.Text = "Z";
		this.checkBoxZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxZ, "Use Z key for attack");
		this.checkBoxZ.UseVisualStyleBackColor = false;
		this.checkBoxZ.CheckedChanged += new System.EventHandler(checkBoxZ_CheckedChanged);
		this.checkBoxR.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxR.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxR.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxR.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBoxR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxR.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBoxR.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxR.Location = new System.Drawing.Point(136, 20);
		this.checkBoxR.Name = "checkBoxR";
		this.checkBoxR.Size = new System.Drawing.Size(90, 30);
		this.checkBoxR.TabIndex = 1;
		this.checkBoxR.Text = "R";
		this.checkBoxR.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxR, "Use R key for attack");
		this.checkBoxR.UseVisualStyleBackColor = false;
		this.checkBoxR.CheckedChanged += new System.EventHandler(checkBoxR_CheckedChanged);
		this.checkBoxSkill.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxSkill.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxSkill.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxSkill.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBoxSkill.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxSkill.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBoxSkill.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxSkill.Location = new System.Drawing.Point(36, 20);
		this.checkBoxSkill.Name = "checkBoxSkill";
		this.checkBoxSkill.Size = new System.Drawing.Size(90, 30);
		this.checkBoxSkill.TabIndex = 0;
		this.checkBoxSkill.Text = "SKILL";
		this.checkBoxSkill.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxSkill, "Enable skill use during attack");
		this.checkBoxSkill.UseVisualStyleBackColor = false;
		this.checkBoxSkill.CheckedChanged += new System.EventHandler(checkBoxSkill_CheckedChanged);
		this.groupBoxDelays.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxDelays.Controls.Add(this.numericUpDownRDelay);
		this.groupBoxDelays.Controls.Add(this.labelRDelay);
		this.groupBoxDelays.Controls.Add(this.numericUpDownSkillDelay);
		this.groupBoxDelays.Controls.Add(this.labelSkillDelay);
		this.groupBoxDelays.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxDelays.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxDelays.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxDelays.Location = new System.Drawing.Point(15, 130);
		this.groupBoxDelays.Name = "groupBoxDelays";
		this.groupBoxDelays.Size = new System.Drawing.Size(540, 70);
		this.groupBoxDelays.TabIndex = 3;
		this.groupBoxDelays.TabStop = false;
		this.groupBoxDelays.Text = "Delay Settings";
		this.numericUpDownRDelay.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.numericUpDownRDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.numericUpDownRDelay.Location = new System.Drawing.Point(350, 40);
		this.numericUpDownRDelay.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.numericUpDownRDelay.Name = "numericUpDownRDelay";
		this.numericUpDownRDelay.Size = new System.Drawing.Size(120, 22);
		this.numericUpDownRDelay.TabIndex = 3;
		this.toolTip.SetToolTip(this.numericUpDownRDelay, "R delay in milliseconds");
		this.numericUpDownRDelay.ValueChanged += new System.EventHandler(numericUpDownRDelay_ValueChanged);
		this.labelRDelay.AutoSize = true;
		this.labelRDelay.Font = new System.Drawing.Font("Tahoma", 8f);
		this.labelRDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelRDelay.Location = new System.Drawing.Point(250, 43);
		this.labelRDelay.Name = "labelRDelay";
		this.labelRDelay.Size = new System.Drawing.Size(72, 13);
		this.labelRDelay.TabIndex = 2;
		this.labelRDelay.Text = "R Delay (ms):";
		this.numericUpDownSkillDelay.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.numericUpDownSkillDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.numericUpDownSkillDelay.Location = new System.Drawing.Point(350, 15);
		this.numericUpDownSkillDelay.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.numericUpDownSkillDelay.Name = "numericUpDownSkillDelay";
		this.numericUpDownSkillDelay.Size = new System.Drawing.Size(120, 22);
		this.numericUpDownSkillDelay.TabIndex = 1;
		this.toolTip.SetToolTip(this.numericUpDownSkillDelay, "Skill delay in milliseconds");
		this.numericUpDownSkillDelay.ValueChanged += new System.EventHandler(numericUpDownSkillDelay_ValueChanged);
		this.labelSkillDelay.AutoSize = true;
		this.labelSkillDelay.Font = new System.Drawing.Font("Tahoma", 8f);
		this.labelSkillDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelSkillDelay.Location = new System.Drawing.Point(250, 18);
		this.labelSkillDelay.Name = "labelSkillDelay";
		this.labelSkillDelay.Size = new System.Drawing.Size(82, 13);
		this.labelSkillDelay.TabIndex = 0;
		this.labelSkillDelay.Text = "Skill Delay (ms):";
		this.groupBoxFeatures.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxFeatures.Controls.Add(this.checkBoxRandomDelay);
		this.groupBoxFeatures.Controls.Add(this.checkBoxGenie);
		this.groupBoxFeatures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxFeatures.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxFeatures.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxFeatures.Location = new System.Drawing.Point(15, 210);
		this.groupBoxFeatures.Name = "groupBoxFeatures";
		this.groupBoxFeatures.Size = new System.Drawing.Size(540, 70);
		this.groupBoxFeatures.TabIndex = 4;
		this.groupBoxFeatures.TabStop = false;
		this.groupBoxFeatures.Text = "Features";
		this.checkBoxRandomDelay.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxRandomDelay.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxRandomDelay.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxRandomDelay.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBoxRandomDelay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxRandomDelay.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBoxRandomDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxRandomDelay.Location = new System.Drawing.Point(290, 25);
		this.checkBoxRandomDelay.Name = "checkBoxRandomDelay";
		this.checkBoxRandomDelay.Size = new System.Drawing.Size(130, 30);
		this.checkBoxRandomDelay.TabIndex = 1;
		this.checkBoxRandomDelay.Text = "Random Delay";
		this.checkBoxRandomDelay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxRandomDelay, "Add random variation to delays");
		this.checkBoxRandomDelay.UseVisualStyleBackColor = false;
		this.checkBoxRandomDelay.CheckedChanged += new System.EventHandler(checkBoxRandomDelay_CheckedChanged);
		this.checkBoxGenie.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxGenie.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxGenie.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxGenie.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBoxGenie.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxGenie.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBoxGenie.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxGenie.Location = new System.Drawing.Point(140, 25);
		this.checkBoxGenie.Name = "checkBoxGenie";
		this.checkBoxGenie.Size = new System.Drawing.Size(130, 30);
		this.checkBoxGenie.TabIndex = 0;
		this.checkBoxGenie.Text = "Start with Genie";
		this.checkBoxGenie.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxGenie, "Start attack automatically when Genie is active");
		this.checkBoxGenie.UseVisualStyleBackColor = false;
		this.checkBoxGenie.CheckedChanged += new System.EventHandler(checkBoxGenie_CheckedChanged);
		this.toolTip.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.toolTip.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.Controls.Add(this.groupBoxFeatures);
		base.Controls.Add(this.groupBoxDelays);
		base.Controls.Add(this.groupBoxAttackKeys);
		base.Controls.Add(this.labelShortcut);
		base.Controls.Add(this.labelHeader);
		this.Font = new System.Drawing.Font("Tahoma", 9.75f);
		base.Name = "Attack";
		base.Size = new System.Drawing.Size(2080, 990);
		this.groupBoxAttackKeys.ResumeLayout(false);
		this.groupBoxDelays.ResumeLayout(false);
		this.groupBoxDelays.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownRDelay).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownSkillDelay).EndInit();
		this.groupBoxFeatures.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
