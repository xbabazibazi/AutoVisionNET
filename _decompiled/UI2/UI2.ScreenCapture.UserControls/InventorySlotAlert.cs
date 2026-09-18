using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Scanix4;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;

namespace UI2.ScreenCapture.UserControls;

public class InventorySlotAlert : UserControl, IServiceControl
{
	private readonly SettingsManager.ScreenCapture.InventorySlotAlert _settings;

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

	private Label lblThreshold;

	private NumericUpDown numThreshold;

	public bool IsActive => checkActive.Checked;

	public InventorySlotAlert(WorkflowEngine workflowEngine)
	{
		InitializeComponent();
		_workflowEngine = workflowEngine ?? throw new ArgumentNullException("workflowEngine");
		_logger = Logger.Instance;
		_settings = Settings.Instance.ScreenCapture.InventorySlotAlert;
		LoadSettings();
		SetupEventHandlers();
	}

	private void LoadSettings()
	{
		try
		{
			checkActive.Checked = _settings.IsActive;
			numThreshold.Value = _settings.LowSlotThreshold;
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
			_settings.LowSlotThreshold = (int)numThreshold.Value;
			_logger.LogDebug("InventorySlotAlert ayarları kaydedildi");
		}
		catch (Exception ex)
		{
			_logger.LogError("Ayarlar kaydedilirken hata: " + ex.Message);
		}
	}

	private void numThreshold_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			_settings.LowSlotThreshold = (int)numThreshold.Value;
			_logger.LogDebug($"Düşük yuvası eşiği: {_settings.LowSlotThreshold}");
		}
		catch (Exception ex)
		{
			_logger.LogError("Düşük yuvası eşiği değiştirilirken hata: " + ex.Message);
		}
	}

	private async void checkActive_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			UpdateStatus();
			if (checkActive.Checked)
			{
				await _workflowEngine.StartAsync("InventorySlotAlert");
				_logger.LogInformation("InventorySlotAlert servisi başlatıldı");
			}
			else
			{
				await _workflowEngine.StopAsync("InventorySlotAlert");
				_logger.LogInformation("InventorySlotAlert servisi durduruldu");
			}
			_settings.IsActive = checkActive.Checked;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.LogError("Servis durumu değiştirilirken hata: " + ex2.Message);
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
		this.numThreshold = new System.Windows.Forms.NumericUpDown();
		this.lblThreshold = new System.Windows.Forms.Label();
		this.checkActive = new System.Windows.Forms.CheckBox();
		this.toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.groupBoxSettings.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numThreshold).BeginInit();
		base.SuspendLayout();
		this.labelHeader.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
		this.labelHeader.Font = UI2.AppFonts.Header(13f);
		this.labelHeader.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.labelHeader.Location = new System.Drawing.Point(0, 0);
		this.labelHeader.Name = "labelHeader";
		this.labelHeader.Size = new System.Drawing.Size(380, 30);
		this.labelHeader.TabIndex = 0;
		this.labelHeader.Text = "ENVANTER YUVASI UYARISI";
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
		this.groupBoxSettings.Controls.Add(this.numThreshold);
		this.groupBoxSettings.Controls.Add(this.lblThreshold);
		this.groupBoxSettings.Controls.Add(this.checkActive);
		this.groupBoxSettings.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBoxSettings.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.groupBoxSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.groupBoxSettings.Location = new System.Drawing.Point(0, 50);
		this.groupBoxSettings.Name = "groupBoxSettings";
		this.groupBoxSettings.Padding = new System.Windows.Forms.Padding(10);
		this.groupBoxSettings.Size = new System.Drawing.Size(380, 150);
		this.groupBoxSettings.TabIndex = 2;
		this.numThreshold.BackColor = System.Drawing.Color.FromArgb(60, 60, 65);
		this.numThreshold.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.numThreshold.Location = new System.Drawing.Point(180, 60);
		this.numThreshold.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.numThreshold.Maximum = new decimal(new int[4] { 30, 0, 0, 0 });
		this.numThreshold.Name = "numThreshold";
		this.numThreshold.Size = new System.Drawing.Size(80, 23);
		this.numThreshold.TabIndex = 2;
		this.numThreshold.Value = new decimal(new int[4] { 5, 0, 0, 0 });
		this.toolTip.SetToolTip(this.numThreshold, "Uyarı için minimum envanter yuvası sayısını belirler");
		this.numThreshold.ValueChanged += new System.EventHandler(numThreshold_ValueChanged);
		this.lblThreshold.AutoSize = true;
		this.lblThreshold.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.lblThreshold.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lblThreshold.Location = new System.Drawing.Point(20, 63);
		this.lblThreshold.Name = "lblThreshold";
		this.lblThreshold.Size = new System.Drawing.Size(140, 15);
		this.lblThreshold.TabIndex = 1;
		this.lblThreshold.Text = "Düşük Yuva Eşiği:";
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
		this.checkActive.Text = "Etkinleştir";
		this.checkActive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip.SetToolTip(this.checkActive, "Envanter yuvası izleme özelliğini etkinleştirir veya devre dışı bırakır");
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
		base.Name = "InventorySlotAlert";
		base.Size = new System.Drawing.Size(380, 200);
		this.groupBoxSettings.ResumeLayout(false);
		this.groupBoxSettings.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numThreshold).EndInit();
		base.ResumeLayout(false);
	}
}
