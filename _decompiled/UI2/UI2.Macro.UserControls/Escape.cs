using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using InputManager;
using SettingsManager;
using SettingsManager.Macro;
using SimpleLogger;

namespace UI2.Macro.UserControls;

public class Escape : UserControl
{
	private readonly SettingsManager.Macro.Escape _settings = Settings.Instance.Macro.Escape;

	private readonly InputUtils _inputUtils;

	private readonly Logger _logger = Logger.Instance;

	private int _eSCTimerRemainingSeconds;

	private bool _isESCTimerRunning;

	private readonly Color _backgroundColor = Color.FromArgb(38, 38, 45);

	private readonly Color _surfaceColor = Color.FromArgb(48, 48, 55);

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private readonly Color _successColor = Color.FromArgb(0, 153, 102);

	private readonly Color _errorColor = Color.FromArgb(200, 60, 60);

	private IContainer components = null;

	private Label labelHeader;

	private Label labelShortcut;

	private GroupBox groupBoxTimerSettings;

	private NumericUpDown numericUpDownESCTimer;

	private Label labelTimerUnit;

	private Button buttonESCTimer;

	private GroupBox groupBoxTimerStatus;

	private Label labelESCTimer;

	private Timer timerESCTimer;

	private ToolTip toolTip;

	public Escape(InputUtils inputUtils)
	{
		InitializeComponent();
		_inputUtils = inputUtils ?? throw new ArgumentNullException("inputUtils");
		LoadSettings();
		SetupEventHandlers();
	}

	private void SetupEventHandlers()
	{
		buttonESCTimer.MouseEnter += delegate
		{
			buttonESCTimer.BackColor = Color.FromArgb(70, 70, 75);
		};
		buttonESCTimer.MouseLeave += delegate
		{
			buttonESCTimer.BackColor = Color.FromArgb(60, 60, 65);
		};
	}

	public void SaveSettings()
	{
		try
		{
			_settings.ESCTime = numericUpDownESCTimer.Value;
			_logger.LogDebug("Escape settings saved");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error saving Escape settings: " + ex.Message);
		}
	}

	private void LoadSettings()
	{
		try
		{
			numericUpDownESCTimer.Value = _settings.ESCTime;
			UpdateCountdownLabel();
			_logger.LogDebug("Escape settings loaded");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error loading Escape settings: " + ex.Message);
		}
	}

	private void numericUpDownESCTimer_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.ESCTime = numericUpDownESCTimer.Value;
			if (!_isESCTimerRunning)
			{
				_eSCTimerRemainingSeconds = (int)numericUpDownESCTimer.Value * 60;
				UpdateCountdownLabel();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("Error changing ESCTime: " + ex.Message);
		}
	}

	private void buttonESCTimer_Click(object sender, EventArgs e)
	{
		try
		{
			if (_isESCTimerRunning)
			{
				timerESCTimer.Stop();
				_isESCTimerRunning = false;
				buttonESCTimer.Text = "Start";
				_logger.LogDebug("ESC timer stopped");
				return;
			}
			if (_eSCTimerRemainingSeconds == 0)
			{
				_eSCTimerRemainingSeconds = (int)numericUpDownESCTimer.Value * 60;
			}
			timerESCTimer.Start();
			_isESCTimerRunning = true;
			buttonESCTimer.Text = "Stop";
			UpdateCountdownLabel();
			_logger.LogDebug("ESC timer started");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error in ESC timer click: " + ex.Message);
		}
	}

	private void UpdateCountdownLabel()
	{
		if (base.InvokeRequired)
		{
			Invoke(UpdateCountdownLabel);
			return;
		}
		try
		{
			int value = _eSCTimerRemainingSeconds / 60;
			int value2 = _eSCTimerRemainingSeconds % 60;
			labelESCTimer.Text = $"Remaining: {value:D2}:{value2:D2}";
		}
		catch (Exception ex)
		{
			_logger.LogError("Error updating countdown label: " + ex.Message);
		}
	}

	private void timerESCTimer_Tick(object sender, EventArgs e)
	{
		try
		{
			if (_eSCTimerRemainingSeconds > 0)
			{
				_eSCTimerRemainingSeconds--;
				UpdateCountdownLabel();
				return;
			}
			timerESCTimer.Stop();
			_isESCTimerRunning = false;
			buttonESCTimer.Text = "Start";
			_inputUtils.ESC();
			_logger.LogDebug("ESC timer completed, ESC key triggered");
		}
		catch (Exception ex)
		{
			_logger.LogError("Error in timer tick: " + ex.Message);
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
		this.groupBoxTimerSettings = new System.Windows.Forms.GroupBox();
		this.numericUpDownESCTimer = new System.Windows.Forms.NumericUpDown();
		this.labelTimerUnit = new System.Windows.Forms.Label();
		this.buttonESCTimer = new System.Windows.Forms.Button();
		this.groupBoxTimerStatus = new System.Windows.Forms.GroupBox();
		this.labelESCTimer = new System.Windows.Forms.Label();
		this.timerESCTimer = new System.Windows.Forms.Timer(this.components);
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxTimerSettings.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownESCTimer).BeginInit();
		this.groupBoxTimerStatus.SuspendLayout();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(2080, 25);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "Escape Settings";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelShortcut.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelShortcut.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelShortcut.Font = new System.Drawing.Font("Tahoma", 8f, System.Drawing.FontStyle.Italic);
		this.labelShortcut.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelShortcut.Location = new System.Drawing.Point(0, 25);
		this.labelShortcut.Name = "labelShortcut";
		this.labelShortcut.Size = new System.Drawing.Size(2080, 15);
		this.labelShortcut.TabIndex = 1;
		this.labelShortcut.Text = "Shortcut: Ctrl + E";
		this.labelShortcut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxTimerSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxTimerSettings.Controls.Add(this.numericUpDownESCTimer);
		this.groupBoxTimerSettings.Controls.Add(this.labelTimerUnit);
		this.groupBoxTimerSettings.Controls.Add(this.buttonESCTimer);
		this.groupBoxTimerSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxTimerSettings.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxTimerSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxTimerSettings.Location = new System.Drawing.Point(15, 50);
		this.groupBoxTimerSettings.Name = "groupBoxTimerSettings";
		this.groupBoxTimerSettings.Size = new System.Drawing.Size(540, 80);
		this.groupBoxTimerSettings.TabIndex = 2;
		this.groupBoxTimerSettings.TabStop = false;
		this.groupBoxTimerSettings.Text = "Timer Settings";
		this.numericUpDownESCTimer.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.numericUpDownESCTimer.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.numericUpDownESCTimer.Location = new System.Drawing.Point(20, 35);
		this.numericUpDownESCTimer.Maximum = new decimal(new int[4] { 600, 0, 0, 0 });
		this.numericUpDownESCTimer.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownESCTimer.Name = "numericUpDownESCTimer";
		this.numericUpDownESCTimer.Size = new System.Drawing.Size(100, 22);
		this.numericUpDownESCTimer.TabIndex = 0;
		this.toolTip.SetToolTip(this.numericUpDownESCTimer, "Set timer duration in minutes");
		this.numericUpDownESCTimer.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numericUpDownESCTimer.ValueChanged += new System.EventHandler(numericUpDownESCTimer_ValueChanged);
		this.labelTimerUnit.AutoSize = true;
		this.labelTimerUnit.Font = new System.Drawing.Font("Tahoma", 8f);
		this.labelTimerUnit.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelTimerUnit.Location = new System.Drawing.Point(130, 38);
		this.labelTimerUnit.Name = "labelTimerUnit";
		this.labelTimerUnit.Size = new System.Drawing.Size(44, 13);
		this.labelTimerUnit.TabIndex = 1;
		this.labelTimerUnit.Text = "Minutes";
		this.buttonESCTimer.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.buttonESCTimer.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.buttonESCTimer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(80, 80, 90);
		this.buttonESCTimer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(70, 70, 75);
		this.buttonESCTimer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.buttonESCTimer.Font = new System.Drawing.Font("Tahoma", 8f);
		this.buttonESCTimer.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.buttonESCTimer.Location = new System.Drawing.Point(200, 30);
		this.buttonESCTimer.Name = "buttonESCTimer";
		this.buttonESCTimer.Size = new System.Drawing.Size(100, 30);
		this.buttonESCTimer.TabIndex = 2;
		this.buttonESCTimer.Text = "Start";
		this.toolTip.SetToolTip(this.buttonESCTimer, "Start or stop the ESC timer");
		this.buttonESCTimer.UseVisualStyleBackColor = false;
		this.buttonESCTimer.Click += new System.EventHandler(buttonESCTimer_Click);
		this.groupBoxTimerStatus.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxTimerStatus.Controls.Add(this.labelESCTimer);
		this.groupBoxTimerStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.groupBoxTimerStatus.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxTimerStatus.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxTimerStatus.Location = new System.Drawing.Point(15, 140);
		this.groupBoxTimerStatus.Name = "groupBoxTimerStatus";
		this.groupBoxTimerStatus.Size = new System.Drawing.Size(540, 70);
		this.groupBoxTimerStatus.TabIndex = 3;
		this.groupBoxTimerStatus.TabStop = false;
		this.groupBoxTimerStatus.Text = "Timer Status";
		this.labelESCTimer.Font = new System.Drawing.Font("Tahoma", 9f);
		this.labelESCTimer.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelESCTimer.Location = new System.Drawing.Point(20, 25);
		this.labelESCTimer.Name = "labelESCTimer";
		this.labelESCTimer.Size = new System.Drawing.Size(500, 25);
		this.labelESCTimer.TabIndex = 0;
		this.labelESCTimer.Text = "Remaining: 00:00";
		this.labelESCTimer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.timerESCTimer.Interval = 1000;
		this.timerESCTimer.Tick += new System.EventHandler(timerESCTimer_Tick);
		this.toolTip.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.toolTip.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.Controls.Add(this.groupBoxTimerStatus);
		base.Controls.Add(this.groupBoxTimerSettings);
		base.Controls.Add(this.labelShortcut);
		base.Controls.Add(this.labelHeader);
		this.Font = new System.Drawing.Font("Tahoma", 9.75f);
		base.Name = "Escape";
		base.Size = new System.Drawing.Size(2080, 990);
		this.groupBoxTimerSettings.ResumeLayout(false);
		this.groupBoxTimerSettings.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDownESCTimer).EndInit();
		this.groupBoxTimerStatus.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
