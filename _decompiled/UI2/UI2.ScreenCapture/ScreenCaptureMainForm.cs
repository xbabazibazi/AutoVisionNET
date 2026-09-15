using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using InputManager;
using Scanix4;
using SettingsManager;
using SimpleLogger;
using UI2.Interfaces;
using UI2.ScreenCapture.UserControls;

namespace UI2.ScreenCapture;

public class ScreenCaptureMainForm : Form
{
	private readonly Logger _logger;

	private readonly Settings _settings = Settings.Instance;

	private readonly IMacroForm _macroForm;

	private readonly Form1 _form1;

	public readonly Alarm Alarm;

	public readonly WorkflowEngine WorkflowEngine;

	private readonly List<ServiceInfo> _services;

	private readonly Dictionary<string, UserControl> _settingControls;

	private bool _isGenieWorking = false;

	private readonly Color _backgroundColor = Color.FromArgb(28, 28, 33);

	private readonly Color _surfaceColor = Color.FromArgb(38, 38, 45);

	private readonly Color _inputBackgroundColor = Color.FromArgb(48, 48, 55);

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private readonly Color _buttonSecondaryColor = Color.FromArgb(200, 60, 60);

	private IContainer components = null;

	private Label lblTitle;

	private Panel pnlContainer;

	private ListBox listBoxScreenCaptureSettings;

	private Panel panelScreenCaptureOptions;

	private Button btnClose;

	private Button btnHide;

	public IAttack Attack { get; }

	public ScreenCaptureMainForm(InputUtils inputUtils, Logs logsForm, IAttack attack, IMacroForm macroForm, Form1 form1)
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.Manual;
		_logger = logsForm.GetLogInstance();
		Attack = attack;
		_macroForm = macroForm;
		_form1 = form1;
		Alarm = new Alarm(_logger);
		WorkflowEngine = new WorkflowEngine(Alarm, inputUtils, OnIsGenieStart, OnIsGenieStop);
		_services = InitializeServices();
		_settingControls = InitializeSettingControls();
		ScreenCaptureMainForm_Load(null, null);
		_logger.LogInformation("ScreenCaptureMainForm başlatıldı.");
	}

	private List<ServiceInfo> InitializeServices()
	{
		List<ServiceInfo> list = new List<ServiceInfo>();
		var array = new[]
		{
			new
			{
				Type = typeof(AcceptParty),
				DisplayName = "Partiyi Kabul Et",
				ServiceName = "RequestParty"
			},
			new
			{
				Type = typeof(HandlePartyMemberDeath),
				DisplayName = "Partide Ölü Varsa",
				ServiceName = "HandlePartyMemberDeath"
			},
			new
			{
				Type = typeof(PartyMemberCount),
				DisplayName = "Parti Kişi Sayısı Kontrolü",
				ServiceName = "PartyMemberCount"
			},
			new
			{
				Type = typeof(StopMacrosWhenGenieStopped),
				DisplayName = "Genie Durunca Makroları Durdur",
				ServiceName = "StopMacrosWhenGenieStopped"
			},
			new
			{
				Type = typeof(RepairArmor),
				DisplayName = "Repair Armor",
				ServiceName = "RepairArmors"
			},
			new
			{
				Type = typeof(CureDb),
				DisplayName = "Cure DB",
				ServiceName = "CureDB"
			},
			new
			{
				Type = typeof(SwapTomahawk),
				DisplayName = "Tomahawk Değiştir",
				ServiceName = "SwapTomahawk"
			},
			new
			{
				Type = typeof(SnapNetReReRe),
				DisplayName = "SnapNet ReReRe",
				ServiceName = "SnapNetReReRe"
			},
			new
			{
				Type = typeof(RepairWeapons),
				DisplayName = "Repair Weapons",
				ServiceName = "RepairWeapons"
			},
			new
			{
				Type = typeof(InventorySlotAlert),
				DisplayName = "Inventory Slot Alert",
				ServiceName = "InventorySlotAlert"
			},
			new
			{
				Type = typeof(SnapNetWhellOfFun),
				DisplayName = "SnapNet Whell Of Fun",
				ServiceName = "SnapNetWhellOfFun"
			},
			new
			{
				Type = typeof(SnapNetStartGenie),
				DisplayName = "SnapNet Start Genie",
				ServiceName = "SnapNetStartGenie"
			}
		};
		var array2 = array;
		foreach (var anon in array2)
		{
			try
			{
				ConstructorInfo constructor = anon.Type.GetConstructor(new Type[1] { typeof(WorkflowEngine) });
				if (constructor != null)
				{
					UserControl userControl = (UserControl)constructor.Invoke(new object[1] { WorkflowEngine });
					list.Add(new ServiceInfo
					{
						DisplayName = anon.DisplayName,
						ServiceName = anon.ServiceName,
						Control = (IServiceControl)userControl
					});
				}
				else
				{
					_logger.LogWarning(anon.Type.Name + " için WorkflowEngine constructor'ı bulunamadı!");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(anon.DisplayName + " servisi oluşturulurken hata: " + ex.Message);
			}
		}
		return list;
	}

	private Dictionary<string, UserControl> InitializeSettingControls()
	{
		Dictionary<string, UserControl> dictionary = new Dictionary<string, UserControl>();
		foreach (ServiceInfo service in _services)
		{
			try
			{
				dictionary[service.DisplayName] = (UserControl)service.Control;
			}
			catch (Exception ex)
			{
				_logger.LogError(service.DisplayName + " kontrolü eklenirken hata: " + ex.Message);
			}
		}
		return dictionary;
	}

	private async void StartServiceFirstTime()
	{
		_logger.LogInformation("Servisler İlk kez Başlatılıyor.");
		List<(bool IsActive, string ServiceName)> servicesToStart = new List<(bool, string)>();
		foreach (ServiceInfo service in _services)
		{
			servicesToStart.Add((service.IsActive, service.ServiceName));
		}
		servicesToStart.Add((_settings.Macro.General.StartGenieOnTp, "StartGenieAfterTp"));
		servicesToStart.Add((true, "GenieStatus"));
		foreach (var (isActive, serviceName) in servicesToStart)
		{
			if (serviceName.Contains("SnapNet"))
			{
				_logger.LogInformation("SnapNet içeren servis atlandı: " + serviceName);
			}
			else if (isActive)
			{
				try
				{
					await WorkflowEngine.StartAsync(serviceName);
					_logger.LogInformation("Servis başlatıldı: " + serviceName);
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					_logger.LogInformation("Servis başlatılamadı (" + serviceName + "): " + ex2.Message);
				}
			}
		}
	}

	public async void OnIsGenieStop()
	{
		try
		{
			if (_isGenieWorking)
			{
				_isGenieWorking = false;
				UpdateGenieStatus(isWorking: false, "Kapalı");
				_logger.LogInformation("Genie durduruldu");
				if (_settings.ScreenCapture.StopMacrosOnGenieStop.IsActive)
				{
					Attack.ToggleGenieStarted(status: false);
					_macroForm.ToggleControls(enabled: true);
					await WorkflowEngine.StopAsync("SwapTomahawk");
				}
			}
			else
			{
				_logger.LogDebug("OnIsGenieStop tekrar çağrıldı, atlanıyor.");
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.LogError("OnIsGenieStop hatası: " + ex2.Message, ex2);
		}
	}

	public async void OnIsGenieStart(Point coordinates)
	{
		if (_isGenieWorking)
		{
			return;
		}
		_isGenieWorking = true;
		UpdateGenieStatus(isWorking: true, "Açık");
		_logger.LogInformation("Genie başlatıldı");
		if (_settings.Macro.Attack.StartAttackOnGenieStart)
		{
			Attack.ToggleGenieStarted(status: true);
			_macroForm.ToggleControls(enabled: false);
			if (_settings.ScreenCapture.SwapTomahawk.IsActive)
			{
				await WorkflowEngine.StartAsync("SwapTomahawk");
			}
		}
	}

	private void ScreenCaptureMainForm_Load(object sender, EventArgs e)
	{
		try
		{
			string[] array = _services.Select((ServiceInfo s) => s.DisplayName).ToArray();
			ListBox.ObjectCollection items = listBoxScreenCaptureSettings.Items;
			object[] items2 = array;
			items.AddRange(items2);
			if (listBoxScreenCaptureSettings.Items.Count > 0)
			{
				listBoxScreenCaptureSettings.SelectedIndex = 0;
			}
			StartServiceFirstTime();
			_logger.LogInformation("ScreenCaptureMainForm yüklendi");
		}
		catch (Exception ex)
		{
			_logger.LogError("Form yüklenirken hata: " + ex.Message, ex);
		}
	}

	private void listBoxScreenCaptureSettings_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			if (listBoxScreenCaptureSettings.SelectedItem is string key && _settingControls.TryGetValue(key, out UserControl value))
			{
				panelScreenCaptureOptions.Controls.Clear();
				value.Dock = DockStyle.Fill;
				panelScreenCaptureOptions.Controls.Add(value);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("Ayar seçimi değiştirilirken hata: " + ex.Message);
		}
	}

	private void ScreenCaptureMainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		SaveAllSettings();
	}

	private void ScreenCaptureMainForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		SaveAllSettings();
	}

	private void BtnHide_Click(object sender, EventArgs e)
	{
		Hide();
	}

	private void SaveAllSettings()
	{
		try
		{
			foreach (ServiceInfo service in _services)
			{
				service.Control.SaveSettings();
			}
			_logger.LogInformation("Tüm ayarlar kaydedildi");
		}
		catch (Exception ex)
		{
			_logger.LogError("Ayarlar kaydedilirken hata: " + ex.Message);
		}
	}

	private void UpdateGenieStatus(bool isWorking, string statusText)
	{
		FluxioUtiles.InvokeIfRequired(this, delegate
		{
			_isGenieWorking = isWorking;
		});
	}

	private void btnHide_MouseEnter(object sender, EventArgs e)
	{
		btnHide.BackColor = _buttonSecondaryColor;
		btnHide.ForeColor = Color.White;
	}

	private void btnHide_MouseLeave(object sender, EventArgs e)
	{
		btnHide.BackColor = Color.Transparent;
		btnHide.ForeColor = _textPrimaryColor;
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
		this.lblTitle = new System.Windows.Forms.Label();
		this.pnlContainer = new System.Windows.Forms.Panel();
		this.listBoxScreenCaptureSettings = new System.Windows.Forms.ListBox();
		this.panelScreenCaptureOptions = new System.Windows.Forms.Panel();
		this.btnHide = new System.Windows.Forms.Button();
		this.btnClose = new System.Windows.Forms.Button();
		this.pnlContainer.SuspendLayout();
		base.SuspendLayout();
		this.lblTitle.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
		this.lblTitle.Font = new System.Drawing.Font("Tahoma", 12f, System.Drawing.FontStyle.Bold);
		this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lblTitle.Location = new System.Drawing.Point(0, 0);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(650, 40);
		this.lblTitle.TabIndex = 0;
		this.lblTitle.Text = "EKRAN YAKALAMA AYARLARI";
		this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.pnlContainer.BackColor = System.Drawing.Color.FromArgb(28, 28, 33);
		this.pnlContainer.Controls.Add(this.listBoxScreenCaptureSettings);
		this.pnlContainer.Controls.Add(this.panelScreenCaptureOptions);
		this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pnlContainer.Location = new System.Drawing.Point(0, 40);
		this.pnlContainer.Name = "pnlContainer";
		this.pnlContainer.Padding = new System.Windows.Forms.Padding(10);
		this.pnlContainer.Size = new System.Drawing.Size(650, 360);
		this.pnlContainer.TabIndex = 1;
		this.listBoxScreenCaptureSettings.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.listBoxScreenCaptureSettings.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.listBoxScreenCaptureSettings.Font = new System.Drawing.Font("Tahoma", 9f);
		this.listBoxScreenCaptureSettings.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.listBoxScreenCaptureSettings.FormattingEnabled = true;
		this.listBoxScreenCaptureSettings.ItemHeight = 14;
		this.listBoxScreenCaptureSettings.Location = new System.Drawing.Point(10, 10);
		this.listBoxScreenCaptureSettings.Name = "listBoxScreenCaptureSettings";
		this.listBoxScreenCaptureSettings.Size = new System.Drawing.Size(220, 336);
		this.listBoxScreenCaptureSettings.TabIndex = 0;
		this.listBoxScreenCaptureSettings.SelectedIndexChanged += new System.EventHandler(listBoxScreenCaptureSettings_SelectedIndexChanged);
		this.panelScreenCaptureOptions.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.panelScreenCaptureOptions.Location = new System.Drawing.Point(240, 10);
		this.panelScreenCaptureOptions.Name = "panelScreenCaptureOptions";
		this.panelScreenCaptureOptions.Padding = new System.Windows.Forms.Padding(10);
		this.panelScreenCaptureOptions.Size = new System.Drawing.Size(400, 340);
		this.panelScreenCaptureOptions.TabIndex = 1;
		this.btnHide.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnHide.FlatAppearance.BorderSize = 0;
		this.btnHide.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnHide.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.btnHide.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.btnHide.Location = new System.Drawing.Point(625, 5);
		this.btnHide.Name = "btnHide";
		this.btnHide.Size = new System.Drawing.Size(20, 20);
		this.btnHide.TabIndex = 2;
		this.btnHide.Text = "✕";
		this.btnHide.UseVisualStyleBackColor = false;
		this.btnHide.Click += new System.EventHandler(BtnHide_Click);
		this.btnHide.MouseEnter += new System.EventHandler(btnHide_MouseEnter);
		this.btnHide.MouseLeave += new System.EventHandler(btnHide_MouseLeave);
		this.btnClose.Location = new System.Drawing.Point(60, 12);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(75, 23);
		this.btnClose.TabIndex = 2;
		this.btnClose.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(28, 28, 33);
		base.ClientSize = new System.Drawing.Size(650, 400);
		base.Controls.Add(this.btnHide);
		base.Controls.Add(this.pnlContainer);
		base.Controls.Add(this.lblTitle);
		base.Controls.Add(this.btnClose);
		this.Font = new System.Drawing.Font("Tahoma", 9.75f);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "ScreenCaptureMainForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "ScreenCaptureMainForm";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(ScreenCaptureMainForm_FormClosing);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(ScreenCaptureMainForm_FormClosed);
		this.pnlContainer.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
