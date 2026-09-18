using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Scanix4;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;

namespace UI2.ScreenCapture.UserControls;

public class HandlePartyMemberDeath : UserControl, IServiceControl
{
	private readonly SettingsManager.ScreenCapture.HandlePartyMemberDeath _settings;

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

	private CheckBox checkBoxBreakParty;

	private CheckBox checkBoxAlarm;

	private CheckBox checkBoxActive;

	private ToolTip toolTip;

	public bool IsActive => checkBoxActive.Checked;

	public HandlePartyMemberDeath(WorkflowEngine workflowEngine)
	{
		InitializeComponent();
		_workflowEngine = workflowEngine ?? throw new ArgumentNullException("workflowEngine");
		_logger = Logger.Instance;
		_settings = Settings.Instance.ScreenCapture.HandlePartyMemberDeath;
		LoadSettings();
		SetupEventHandlers();
	}

	private void LoadSettings()
	{
		try
		{
			checkBoxActive.Checked = _settings.IsActive;
			checkBoxAlarm.Checked = _settings.Alarm;
			checkBoxBreakParty.Checked = _settings.BreakParty;
			UpdateStatus();
		}
		catch (Exception ex)
		{
			_logger.LogError("Ayarlar yüklenirken hata: " + ex.Message);
		}
	}

	private void SetupEventHandlers()
	{
		checkBoxActive.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxActive, isHover: true);
		};
		checkBoxActive.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxActive, isHover: false);
		};
		checkBoxAlarm.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxAlarm, isHover: true);
		};
		checkBoxAlarm.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxAlarm, isHover: false);
		};
		checkBoxBreakParty.MouseEnter += delegate
		{
			UpdateButtonHover(checkBoxBreakParty, isHover: true);
		};
		checkBoxBreakParty.MouseLeave += delegate
		{
			UpdateButtonHover(checkBoxBreakParty, isHover: false);
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
			labelStatus.Text = (checkBoxActive.Checked ? "● Aktif" : "● Devre Dışı");
			labelStatus.ForeColor = (checkBoxActive.Checked ? _successColor : _errorColor);
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
			_settings.IsActive = checkBoxActive.Checked;
			_settings.Alarm = checkBoxAlarm.Checked;
			_settings.BreakParty = checkBoxBreakParty.Checked;
			_logger.LogDebug("HandlePartyMemberDeath ayarları kaydedildi");
		}
		catch (Exception ex)
		{
			_logger.LogError("Ayarlar kaydedilirken hata: " + ex.Message);
		}
	}

	private async void checkBoxActive_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			UpdateStatus();
			if (checkBoxActive.Checked)
			{
				await _workflowEngine.StartAsync("HandlePartyMemberDeath");
				_logger.LogInformation("HandlePartyMemberDeath servisi başlatıldı");
			}
			else
			{
				await _workflowEngine.StopAsync("HandlePartyMemberDeath");
				_logger.LogInformation("HandlePartyMemberDeath servisi durduruldu");
			}
			_settings.IsActive = checkBoxActive.Checked;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.LogError("Servis durumu değiştirilirken hata: " + ex2.Message);
			if (base.InvokeRequired)
			{
				Invoke((MethodInvoker)delegate
				{
					checkBoxActive.Checked = !checkBoxActive.Checked;
				});
			}
			else
			{
				checkBoxActive.Checked = !checkBoxActive.Checked;
			}
		}
	}

	private void checkBoxAlarm_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.Alarm = checkBoxAlarm.Checked;
			_logger.LogDebug($"Alarm durumu: {checkBoxAlarm.Checked}");
		}
		catch (Exception ex)
		{
			_logger.LogError("Alarm ayarı değiştirilirken hata: " + ex.Message);
		}
	}

	private void checkBoxBreakParty_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.BreakParty = checkBoxBreakParty.Checked;
			_logger.LogDebug($"BreakParty durumu: {checkBoxBreakParty.Checked}");
		}
		catch (Exception ex)
		{
			_logger.LogError("BreakParty ayarı değiştirilirken hata: " + ex.Message);
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
		this.checkBoxBreakParty = new System.Windows.Forms.CheckBox();
		this.checkBoxAlarm = new System.Windows.Forms.CheckBox();
		this.checkBoxActive = new System.Windows.Forms.CheckBox();
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
		this.labelHeader.Text = "PARTİ ÜYESİ ÖLÜMÜ";
		this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelStatus.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelStatus.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.labelStatus.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.labelStatus.Location = new System.Drawing.Point(0, 30);
		this.labelStatus.Name = "labelStatus";
		this.labelStatus.Size = new System.Drawing.Size(380, 20);
		this.labelStatus.TabIndex = 1;
		this.labelStatus.Text = "● Devre Dışı";
		this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.groupBoxSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.groupBoxSettings.Controls.Add(this.checkBoxBreakParty);
		this.groupBoxSettings.Controls.Add(this.checkBoxAlarm);
		this.groupBoxSettings.Controls.Add(this.checkBoxActive);
		this.groupBoxSettings.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBoxSettings.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(0, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Size = new System.Drawing.Size(380, 150);
		this.groupBoxSettings.TabIndex = 2;
		this.checkBoxBreakParty.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxBreakParty.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxBreakParty.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxBreakParty.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBoxBreakParty.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxBreakParty.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkBoxBreakParty.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxBreakParty.Location = new System.Drawing.Point(20, 110);
		this.checkBoxBreakParty.Name = "checkBoxBreakParty";
		this.checkBoxBreakParty.Size = new System.Drawing.Size(200, 30);
		this.checkBoxBreakParty.TabIndex = 2;
		this.checkBoxBreakParty.Text = "Partiyi Otomatik Dağıt";
		this.checkBoxBreakParty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxBreakParty, "Parti üyesinin ölümü durumunda partiyi otomatik olarak dağıtır");
		this.checkBoxBreakParty.UseVisualStyleBackColor = false;
		this.checkBoxBreakParty.CheckedChanged += new System.EventHandler(checkBoxBreakParty_CheckedChanged);
		this.checkBoxAlarm.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxAlarm.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxAlarm.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxAlarm.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.checkBoxAlarm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxAlarm.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkBoxAlarm.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxAlarm.Location = new System.Drawing.Point(20, 70);
		this.checkBoxAlarm.Name = "checkBoxAlarm";
		this.checkBoxAlarm.Size = new System.Drawing.Size(200, 30);
		this.checkBoxAlarm.TabIndex = 1;
		this.checkBoxAlarm.Text = "Alarmı Etkinleştir";
		this.checkBoxAlarm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxAlarm, "Parti üyesinin ölümü durumunda alarm sesini açar/kapatır");
		this.checkBoxAlarm.UseVisualStyleBackColor = false;
		this.checkBoxAlarm.CheckedChanged += new System.EventHandler(checkBoxAlarm_CheckedChanged);
		this.checkBoxActive.Appearance = System.Windows.Forms.Appearance.Button;
		this.checkBoxActive.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.checkBoxActive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(80, 80, 85);
		this.checkBoxActive.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(0, 153, 102);
		this.checkBoxActive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.checkBoxActive.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.checkBoxActive.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.checkBoxActive.Location = new System.Drawing.Point(20, 30);
		this.checkBoxActive.Name = "checkBoxActive";
		this.checkBoxActive.Size = new System.Drawing.Size(200, 30);
		this.checkBoxActive.TabIndex = 0;
		this.checkBoxActive.Text = "Etkinleştir";
		this.checkBoxActive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkBoxActive, "Parti üyesinin ölümü kontrol özelliğini etkinleştirir veya devre dışı bırakır");
		this.checkBoxActive.UseVisualStyleBackColor = false;
		this.checkBoxActive.CheckedChanged += new System.EventHandler(checkBoxActive_CheckedChanged);
		this.toolTip.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.toolTip.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		base.Controls.Add(this.groupBoxSettings);
		base.Controls.Add(this.labelStatus);
		base.Controls.Add(this.labelHeader);
		this.Font = new System.Drawing.Font("Segoe UI", 9.75f);
		base.Name = "HandlePartyMemberDeath";
		base.Size = new System.Drawing.Size(380, 200);
		this.groupBoxSettings.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
