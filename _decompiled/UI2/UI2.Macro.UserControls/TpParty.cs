using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SettingsManager;
using SettingsManager.Macro;
using SimpleLogger;

namespace UI2.Macro.UserControls;

public class TpParty : UserControl
{
	private readonly SettingsManager.Macro.TpParty _settings = Settings.Instance.Macro.TpParty;

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

	private CheckBox checkBoxResistAfterTp;

	private NumericUpDown numericUpDownTPPartyDelay;

	private Label labelDelay;

	private NumericUpDown PartyMemberCount;

	private Label labelMemberCount;

	private ToolTip toolTip;

	public TpParty()
	{
		InitializeComponent();
		LoadSettings();
		SetupEventHandlers();
	}

	private void SetupEventHandlers()
	{
		checkBoxResistAfterTp.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxResistAfterTp, isHover: true);
		};
		checkBoxResistAfterTp.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxResistAfterTp, isHover: false);
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
			_settings.PartyMemberCount = PartyMemberCount.Value;
			_settings.TPDelay = numericUpDownTPPartyDelay.Value;
			_settings.ResistOnTp = checkBoxResistAfterTp.Checked;
			_logger.LogDebug("TpParty settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving TpParty settings: " + ex.Message);
		}
	}

	private void LoadSettings()
	{
		try
		{
			PartyMemberCount.Value = _settings.PartyMemberCount;
			numericUpDownTPPartyDelay.Value = _settings.TPDelay;
			checkBoxResistAfterTp.Checked = _settings.ResistOnTp;
			_logger.LogDebug("TpParty settings loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading TpParty settings: " + ex.Message);
		}
	}

	private void PartyMemberCount_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.PartyMemberCount = PartyMemberCount.Value;
			_logger.LogDebug("PartyMemberCount setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing PartyMemberCount: " + ex.Message);
		}
	}

	private void numericUpDownTPPartyDelay_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.TPDelay = numericUpDownTPPartyDelay.Value;
			_logger.LogDebug("TPDelay setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing TPDelay: " + ex.Message);
		}
	}

	private void checkBoxResistAfterTp_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.ResistOnTp = checkBoxResistAfterTp.Checked;
			_logger.LogDebug("ResistOnTp setting changed");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing ResistOnTp: " + ex.Message);
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
		this.checkBoxResistAfterTp = new System.Windows.Forms.CheckBox();
		this.numericUpDownTPPartyDelay = new System.Windows.Forms.NumericUpDown();
		this.labelDelay = new System.Windows.Forms.Label();
		this.PartyMemberCount = new System.Windows.Forms.NumericUpDown();
		this.labelMemberCount = new System.Windows.Forms.Label();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxSettings.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownTPPartyDelay).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.PartyMemberCount).BeginInit();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(570, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "TP Party Settings";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelShortcut.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelShortcut.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelShortcut.Font = new System.Drawing.Font("Tahoma", 8f, System.Drawing.FontStyle.Italic);
		this.labelShortcut.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelShortcut.Location = new System.Drawing.Point(0, 25);
		this.labelShortcut.Name = "labelShortcut";
		this.labelShortcut.Size = new System.Drawing.Size(570, 15);
		this.labelShortcut.TabIndex = 1;
		this.labelShortcut.Text = "Shortcut: Ctrl + D";
		this.labelShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxSettings.Controls.Add(this.checkBoxResistAfterTp);
		this.groupBoxSettings.Controls.Add(this.numericUpDownTPPartyDelay);
		this.groupBoxSettings.Controls.Add(this.labelDelay);
		this.groupBoxSettings.Controls.Add(this.PartyMemberCount);
		this.groupBoxSettings.Controls.Add(this.labelMemberCount);
		this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxSettings.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(15, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Size = new System.Drawing.Size(540, 150);
		this.groupBoxSettings.TabIndex = 2;
		this.groupBoxSettings.TabStop = false;
		this.groupBoxSettings.Text = "TP Party Settings";
		this.checkBoxResistAfterTp.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxResistAfterTp.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxResistAfterTp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxResistAfterTp.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(55, 78, 92);
		this.checkBoxResistAfterTp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxResistAfterTp.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBoxResistAfterTp.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxResistAfterTp.Location = new System.Drawing.Point(20, 105);
		this.checkBoxResistAfterTp.Name = "checkBoxResistAfterTp";
		this.checkBoxResistAfterTp.Size = new System.Drawing.Size(200, 30);
		this.checkBoxResistAfterTp.TabIndex = 4;
		this.checkBoxResistAfterTp.Text = "Resist After TP";
		this.checkBoxResistAfterTp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxResistAfterTp, "Enable resistance after teleport");
		this.checkBoxResistAfterTp.UseVisualStyleBackColor = false;
		this.checkBoxResistAfterTp.CheckedChanged += new System.EventHandler(checkBoxResistAfterTp_CheckedChanged);
		this.numericUpDownTPPartyDelay.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.numericUpDownTPPartyDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.numericUpDownTPPartyDelay.Location = new System.Drawing.Point(20, 76);
		this.numericUpDownTPPartyDelay.Maximum = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.numericUpDownTPPartyDelay.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownTPPartyDelay.Name = "numericUpDownTPPartyDelay";
		this.numericUpDownTPPartyDelay.Size = new System.Drawing.Size(120, 22);
		this.numericUpDownTPPartyDelay.TabIndex = 3;
		this.numericUpDownTPPartyDelay.Value = new decimal(new int[4] { 1500, 0, 0, 0 });
		this.toolTip.SetToolTip(this.numericUpDownTPPartyDelay, "Set teleport delay in milliseconds");
		this.numericUpDownTPPartyDelay.ValueChanged += new System.EventHandler(numericUpDownTPPartyDelay_ValueChanged);
		this.labelDelay.AutoSize = true;
		this.labelDelay.Font = new System.Drawing.Font("Tahoma", 8f);
		this.labelDelay.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelDelay.Location = new System.Drawing.Point(20, 56);
		this.labelDelay.Name = "labelDelay";
		this.labelDelay.Size = new System.Drawing.Size(66, 15);
		this.labelDelay.TabIndex = 2;
		this.labelDelay.Text = "Delay (ms):";
		this.PartyMemberCount.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.PartyMemberCount.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.PartyMemberCount.Location = new System.Drawing.Point(20, 34);
		this.PartyMemberCount.Maximum = new decimal(new int[4] { 1111, 0, 0, 0 });
		this.PartyMemberCount.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.PartyMemberCount.Name = "PartyMemberCount";
		this.PartyMemberCount.Size = new System.Drawing.Size(120, 22);
		this.PartyMemberCount.TabIndex = 1;
		this.PartyMemberCount.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.toolTip.SetToolTip(this.PartyMemberCount, "Set number of party members");
		this.PartyMemberCount.ValueChanged += new System.EventHandler(PartyMemberCount_ValueChanged);
		this.labelMemberCount.AutoSize = true;
		this.labelMemberCount.Font = new System.Drawing.Font("Tahoma", 8f);
		this.labelMemberCount.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelMemberCount.Location = new System.Drawing.Point(20, 14);
		this.labelMemberCount.Name = "labelMemberCount";
		this.labelMemberCount.Size = new System.Drawing.Size(91, 15);
		this.labelMemberCount.TabIndex = 0;
		this.labelMemberCount.Text = "Member Count:";
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
		base.Name = "TpParty";
		base.Size = new System.Drawing.Size(570, 300);
		this.groupBoxSettings.ResumeLayout(false);
		this.groupBoxSettings.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownTPPartyDelay).EndInit();
		((System.ComponentModel.ISupportInitialize)this.PartyMemberCount).EndInit();
		base.ResumeLayout(false);
	}
}
