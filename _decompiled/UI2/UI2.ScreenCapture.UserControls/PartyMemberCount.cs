using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Scanix4;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;

namespace UI2.ScreenCapture.UserControls;

public class PartyMemberCount : UserControl, IServiceControl
{
	private readonly SettingsManager.ScreenCapture.PartyMemberCount _settings;

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

	private Panel panelThreshold;

	private NumericUpDown numCount;

	private Label labelThreshold;

	private CheckBox checkBreakParty;

	private CheckBox checkAlarm;

	private ToolTip toolTip;

	public bool IsActive => checkActive.Checked;

	public PartyMemberCount(WorkflowEngine workflowEngine)
	{
		InitializeComponent();
		_workflowEngine = workflowEngine ?? throw new ArgumentNullException("workflowEngine");
		_logger = Logger.Instance;
		_settings = Settings.Instance.ScreenCapture.PartyMemberCount;
		LoadSettings();
		SetupEventHandlers();
	}

	private void LoadSettings()
	{
		try
		{
			checkActive.Checked = _settings.IsActive;
			numCount.Value = _settings.PartyCount;
			checkBreakParty.Checked = _settings.BreakParty;
			checkAlarm.Checked = _settings.Alarm;
			UpdateStatus();
		}
		catch (Exception ex)
		{
			_logger.LogError("Ayarlar yüklenirken hata: " + ex.Message);
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
		checkBreakParty.MouseEnter += delegate
		{
			UpdateButtonHover(checkBreakParty, isHover: true);
		};
		checkBreakParty.MouseLeave += delegate
		{
			UpdateButtonHover(checkBreakParty, isHover: false);
		};
		checkAlarm.MouseEnter += delegate
		{
			UpdateButtonHover(checkAlarm, isHover: true);
		};
		checkAlarm.MouseLeave += delegate
		{
			UpdateButtonHover(checkAlarm, isHover: false);
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
			labelStatus.Text = (checkActive.Checked ? "● Aktif" : "● Devre Dışı");
			labelStatus.ForeColor = (checkActive.Checked ? _successColor : _errorColor);
		}
		catch (Exception ex)
		{
			_logger.LogError("Durum güncellenirken hata: " + ex.Message);
		}
	}

	public void SaveSettings()
	{
		try
		{
			_settings.IsActive = checkActive.Checked;
			_settings.PartyCount = (int)numCount.Value;
			_settings.BreakParty = checkBreakParty.Checked;
			_settings.Alarm = checkAlarm.Checked;
			_logger.LogDebug("PartyMemberCount ayarları kaydedildi");
		}
		catch (Exception ex)
		{
			_logger.LogError("Ayarlar kaydedilirken hata: " + ex.Message);
		}
	}

	private async void checkActive_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			UpdateStatus();
			if (checkActive.Checked)
			{
				await _workflowEngine.StartAsync("PartyMemberCount");
				_logger.LogInformation("PartyMemberCount servisi başlatıldı");
			}
			else
			{
				await _workflowEngine.StopAsync("PartyMemberCount");
				_logger.LogInformation("PartyMemberCount servisi durduruldu");
			}
			_settings.IsActive = checkActive.Checked;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.LogError("Servis durumu değiştirilirken hata: " + ex2.Message);
			if (base.InvokeRequired)
			{
				Invoke(delegate
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

	private void numCount_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.PartyCount = (int)numCount.Value;
			_logger.LogDebug($"Parti üye sayısı eşiği: {_settings.PartyCount}");
		}
		catch (Exception ex)
		{
			_logger.LogError("Parti üye sayısı eşiği değiştirilirken hata: " + ex.Message);
		}
	}

	private void checkBreakParty_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.BreakParty = checkBreakParty.Checked;
			_logger.LogDebug($"BreakParty durumu: {checkBreakParty.Checked}");
		}
		catch (Exception ex)
		{
			_logger.LogError("BreakParty ayarı değiştirilirken hata: " + ex.Message);
		}
	}

	private void checkAlarm_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.Alarm = checkAlarm.Checked;
			_logger.LogDebug($"Alarm durumu: {checkAlarm.Checked}");
		}
		catch (Exception ex)
		{
			_logger.LogError("Alarm ayarı değiştirilirken hata: " + ex.Message);
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
		this.checkAlarm = new System.Windows.Forms.CheckBox();
		this.checkBreakParty = new System.Windows.Forms.CheckBox();
		this.panelThreshold = new System.Windows.Forms.Panel();
		this.numCount = new System.Windows.Forms.NumericUpDown();
		this.labelThreshold = new System.Windows.Forms.Label();
		this.checkActive = new System.Windows.Forms.CheckBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxSettings.SuspendLayout();
		this.panelThreshold.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numCount).BeginInit();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(380, 30);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "PARTİ ÜYE SAYISI KONTROLÜ";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelStatus.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelStatus.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelStatus.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.labelStatus.Location = new System.Drawing.Point(0, 30);
		this.labelStatus.Name = "labelStatus";
		this.labelStatus.Size = new System.Drawing.Size(380, 20);
		this.labelStatus.TabIndex = 1;
		this.labelStatus.Text = "● Devre Dışı";
		this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxSettings.Controls.Add(this.checkAlarm);
		this.groupBoxSettings.Controls.Add(this.checkBreakParty);
		this.groupBoxSettings.Controls.Add(this.panelThreshold);
		this.groupBoxSettings.Controls.Add(this.checkActive);
		this.groupBoxSettings.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBoxSettings.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(0, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Padding = new System.Windows.Forms.Padding(10);
		this.groupBoxSettings.Size = new System.Drawing.Size(380, 150);
		this.groupBoxSettings.TabIndex = 2;
		this.checkAlarm.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkAlarm.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkAlarm.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkAlarm.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkAlarm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkAlarm.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkAlarm.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkAlarm.Location = new System.Drawing.Point(20, 110);
		this.checkAlarm.Name = "checkAlarm";
		this.checkAlarm.Size = new System.Drawing.Size(200, 30);
		this.checkAlarm.TabIndex = 3;
		this.checkAlarm.Text = "Alarmı Etkinleştir";
		this.checkAlarm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkAlarm, "Üye sayısı eşik altına düştüğünde alarm çalar");
		this.checkAlarm.UseVisualStyleBackColor = false;
		this.checkAlarm.CheckedChanged += new System.EventHandler(checkAlarm_CheckedChanged);
		this.checkBreakParty.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBreakParty.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBreakParty.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBreakParty.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBreakParty.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBreakParty.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkBreakParty.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBreakParty.Location = new System.Drawing.Point(20, 70);
		this.checkBreakParty.Name = "checkBreakParty";
		this.checkBreakParty.Size = new System.Drawing.Size(200, 30);
		this.checkBreakParty.TabIndex = 2;
		this.checkBreakParty.Text = "Partiyi Otomatik Dağıt";
		this.checkBreakParty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBreakParty, "Üye sayısı eşik altına düştüğünde partiyi otomatik olarak dağıtır");
		this.checkBreakParty.UseVisualStyleBackColor = false;
		this.checkBreakParty.CheckedChanged += new System.EventHandler(checkBreakParty_CheckedChanged);
		this.panelThreshold.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.panelThreshold.Controls.Add(this.numCount);
		this.panelThreshold.Controls.Add(this.labelThreshold);
		this.panelThreshold.Location = new System.Drawing.Point(20, 30);
		this.panelThreshold.Name = "panelThreshold";
		this.panelThreshold.Size = new System.Drawing.Size(330, 30);
		this.panelThreshold.TabIndex = 1;
		this.numCount.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.numCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.numCount.Font = new System.Drawing.Font("Tahoma", 8f);
		this.numCount.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.numCount.Location = new System.Drawing.Point(180, 3);
		this.numCount.Maximum = new decimal(new int[4] { 8, 0, 0, 0 });
		this.numCount.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numCount.Name = "numCount";
		this.numCount.Size = new System.Drawing.Size(50, 23);
		this.numCount.TabIndex = 0;
		this.numCount.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.toolTip.SetToolTip(this.numCount, "Parti üye sayısı bu değerin altına düştüğünde uyarı verir");
		this.numCount.ValueChanged += new System.EventHandler(numCount_ValueChanged);
		this.labelThreshold.AutoSize = true;
		this.labelThreshold.Font = new System.Drawing.Font("Tahoma", 8f);
		this.labelThreshold.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelThreshold.Location = new System.Drawing.Point(20, 6);
		this.labelThreshold.Name = "labelThreshold";
		this.labelThreshold.Size = new System.Drawing.Size(113, 15);
		this.labelThreshold.TabIndex = 1;
		this.labelThreshold.Text = "Minimum Eşik:";
		this.checkActive.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkActive.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkActive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 153, 102);
		this.checkActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkActive.Font = new System.Drawing.Font("Tahoma", 8f);
		this.checkActive.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkActive.Location = new System.Drawing.Point(20, 110);
		this.checkActive.Name = "checkActive";
		this.checkActive.Size = new System.Drawing.Size(200, 30);
		this.checkActive.TabIndex = 0;
		this.checkActive.Text = "Etkinleştir";
		this.checkActive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkActive, "Parti üye sayısı izleme özelliğini etkinleştirir veya devre dışı bırakır");
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
		this.Font = new System.Drawing.Font("Tahoma", 9.75f);
		base.Name = "PartyMemberCount";
		base.Size = new System.Drawing.Size(380, 200);
		this.groupBoxSettings.ResumeLayout(false);
		this.panelThreshold.ResumeLayout(false);
		this.panelThreshold.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numCount).EndInit();
		base.ResumeLayout(false);
	}
}
