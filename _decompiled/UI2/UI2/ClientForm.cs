using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SettingsManager;
using SettingsManager.ClientSettings;
using SnapNetClient;

namespace UI2;

public class ClientForm : Form
{
	private const int MAX_LOG_ITEMS = 1000;

	private readonly StringBuilder _logBuffer = new StringBuilder();

	private DateTime _lastLogUpdate = DateTime.MinValue;

	private ClientSettings _clientSettings;

	private System.Threading.Timer _uiUpdateTimer;

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private readonly Color _buttonPrimaryColor = Color.FromArgb(0, 122, 204);

	private readonly Color _buttonSecondaryColor = Color.FromArgb(200, 60, 60);

	private readonly Color _buttonSuccessColor = Color.FromArgb(0, 153, 102);

	private readonly Color _primaryHoverColor = Color.FromArgb(0, 145, 245);

	private readonly Color _secondaryHoverColor = Color.FromArgb(220, 80, 80);

	private readonly Color _connectingColor = Color.FromArgb(0, 122, 204);

	private readonly Color _connectedColor = Color.FromArgb(0, 153, 102);

	private readonly Color _disconnectedColor = Color.FromArgb(200, 60, 60);

	private readonly Color _errorColor = Color.FromArgb(200, 60, 60);

	private IContainer components = null;

	private Panel mainContainer;

	private Panel headerPanel;

	private Panel connectionPanel;

	private Panel logPanel;

	private Panel commandPanel;

	private Panel statusPanel;

	private Button btnClose;

	private Label lblTitle;

	private Label lblNickname;

	private TextBox txtNickname;

	private Label lblJob;

	private ComboBox cmbJob;

	private Label lblIp;

	private TextBox txtIpAddress;

	private Label lblPort;

	private TextBox txtPort;

	private Button btnConnect;

	private Button btnDisconnect;

	private ListBox lstLogs;

	private Button btnClearLogs;

	private TextBox txtCommand;

	private Button btnSend;

	private Label lblStatus;

	public bool IsConnected => AppClient.IsConnected;

	public event Action<bool> ConnectionStatusChanged;

	public ClientForm()
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.Manual;
		btnClearLogs.BringToFront();
		InitializeAdvancedComponents();
		SetupClientEvents();
		LoadSettings();
		AttachEventHandlers();
		btnConnect_Click(null, EventArgs.Empty);
	}

	private void InitializeAdvancedComponents()
	{
		typeof(ListBox).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(lstLogs, true);
		_uiUpdateTimer = new System.Threading.Timer(delegate
		{
			UpdateUIState();
		}, null, 0, 250);
	}

	private void AttachEventHandlers()
	{
		txtNickname.TextChanged += SettingsChanged;
		txtIpAddress.TextChanged += SettingsChanged;
		txtPort.TextChanged += SettingsChanged;
		cmbJob.SelectedIndexChanged += cmbJob_SelectedIndexChanged;
		btnConnect.MouseEnter += btnConnect_MouseEnter;
		btnConnect.MouseLeave += btnConnect_MouseLeave;
		btnSend.MouseEnter += btnSend_MouseEnter;
		btnSend.MouseLeave += btnSend_MouseLeave;
		btnDisconnect.MouseEnter += btnDisconnect_MouseEnter;
		btnDisconnect.MouseLeave += btnDisconnect_MouseLeave;
		btnClearLogs.MouseEnter += btnClearLogs_MouseEnter;
		btnClearLogs.MouseLeave += btnClearLogs_MouseLeave;
	}

	private void LoadSettings()
	{
		_clientSettings = Settings.Instance.ClientSettings.ClientSettings;
		txtIpAddress.Text = _clientSettings.ServerIP;
		txtPort.Text = _clientSettings.ServerPort.ToString();
		txtNickname.Text = _clientSettings.CharacterNickname;
		cmbJob.SelectedIndex = Math.Max(0, Math.Min(cmbJob.Items.Count - 1, _clientSettings.CharacterJob - 1));
	}

	private void SaveSettings()
	{
		_clientSettings.ServerIP = txtIpAddress.Text;
		if (int.TryParse(txtPort.Text, out var result))
		{
			_clientSettings.ServerPort = result;
		}
		else
		{
			_clientSettings.ServerPort = 5000;
		}
		_clientSettings.CharacterNickname = txtNickname.Text;
	}

	private void SetupClientEvents()
	{
		AppClient.MessageReceived += delegate(string msg)
		{
			if (base.InvokeRequired)
			{
				BeginInvoke(delegate
				{
					AddLog(msg);
				});
			}
			else
			{
				AddLog(msg);
			}
		};
		AppClient.Connected += delegate
		{
			SafeInvoke(delegate
			{
				UpdateStatus("● BAĞLANTI BAŞARILI", _connectedColor);
				ConnectionStatusChanged?.Invoke(obj: true);
				Form1.Instance?.InvokeIfRequired(delegate
				{
					Form1.Instance.ToolStripButtonClient.BackColor = _buttonSuccessColor;
					Form1.Instance.ToolStripButtonClient.ToolTipText = "Sunucuya bağlı";
				});
			});
		};
		AppClient.Disconnected += delegate
		{
			SafeInvoke(delegate
			{
				UpdateStatus("● BAĞLANTI KOPTU", _disconnectedColor);
				AddLog("Sunucu bağlantısı kesildi!");
				ConnectionStatusChanged?.Invoke(obj: false);
				Form1.Instance?.InvokeIfRequired(delegate
				{
					Form1.Instance.ToolStripButtonClient.BackColor = _errorColor;
					Form1.Instance.ToolStripButtonClient.ToolTipText = "Bağlantı kesik";
				});
			});
		};
		AppClient.ConnectionFailed += delegate(Exception ex)
		{
			SafeInvoke(delegate
			{
				AddLog("Bağlantı hatası: " + ex.Message);
				UpdateStatus("● BAĞLANTI BAŞARISIZ", _errorColor);
				ConnectionStatusChanged?.Invoke(obj: false);
				Form1.Instance?.InvokeIfRequired(delegate
				{
					Form1.Instance.ToolStripButtonClient.BackColor = _errorColor;
					Form1.Instance.ToolStripButtonClient.ToolTipText = "Bağlantı kesik";
				});
			});
		};
	}

	private void RegisterGlobalCommands()
	{
		AppClient.RegisterCommand("101", delegate
		{
			Form1.Instance?._screenCaptureMainForm?.Alarm?.StopAlarm();
		});
		AppClient.RegisterCommand("201", async delegate
		{
			Settings settings = Settings.Instance;
			if (settings != null && settings.ScreenCapture?.ReReRe?.IsActive == true)
			{
				for (int i = 0; i < 4; i++)
				{
					await (Form1.Instance?._screenCaptureMainForm?.WorkflowEngine?.StartAsync("SnapNetReReRe"));
				}
			}
		});
		AppClient.RegisterCommand("301", delegate
		{
			Form1.Instance?.CheckForUpdates();
		});
		AppClient.RegisterCommand("401", async delegate
		{
			Settings settings = Settings.Instance;
			if (settings != null && settings.ScreenCapture?.StartGenie?.IsActive == true)
			{
				await (Form1.Instance?._screenCaptureMainForm?.WorkflowEngine?.StartAsync("SnapNetStartGenie"));
			}
		});
		AppClient.RegisterCommand("501", async delegate
		{
			Settings settings = Settings.Instance;
			if (settings != null && settings.ScreenCapture?.WhellOfFun?.IsActive == true)
			{
				await (Form1.Instance?._screenCaptureMainForm?.WorkflowEngine?.StartAsync("SnapNetWhellOfFun"));
			}
		});
	}

	private async void btnConnect_Click(object sender, EventArgs e)
	{
		SafeInvoke(delegate
		{
			UpdateStatus("● BAĞLANIYOR...", _connectingColor);
		});
		try
		{
			ClientSettings settings = Settings.Instance.ClientSettings.ClientSettings;
			AppClient.Initialize(settings.ServerIP ?? "192.168.1.100", (settings.ServerPort > 0) ? settings.ServerPort : 5000, settings.CharacterNickname ?? "TempNickName", (Client.JobType)(cmbJob.SelectedIndex + 1));
			RegisterGlobalCommands();
			await AppClient.ConnectAsync();
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			AddLog("Bağlantı hatası: " + ex2.Message);
			UpdateStatus("● BAĞLANTI HATASI", _errorColor);
		}
	}

	private bool ValidateCommandInput()
	{
		if (!int.TryParse(txtCommand.Text, out var _))
		{
			MessageBox.Show("Geçersiz komut formatı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}
		return true;
	}

	private void btnDisconnect_Click(object sender, EventArgs e)
	{
		AppClient.Disconnect();
		SafeInvoke(delegate
		{
			UpdateStatus("● BAĞLANTI KESİLDİ", _disconnectedColor);
			AddLog("Bağlantı manuel olarak kesildi!");
		});
		ConnectionStatusChanged?.Invoke(obj: false);
	}

	public void Disconnect()
	{
		btnDisconnect_Click(null, EventArgs.Empty);
	}

	private void AddLog(string message)
	{
		lock (_logBuffer)
		{
			StringBuilder logBuffer = _logBuffer;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, logBuffer);
			handler.AppendLiteral("[");
			handler.AppendFormatted(DateTime.Now, "HH:mm:ss.fff");
			handler.AppendLiteral("] ");
			handler.AppendFormatted(message);
			logBuffer.AppendLine(ref handler);
		}
		if ((DateTime.Now - _lastLogUpdate).TotalMilliseconds > 500.0 || _logBuffer.Length > 2048)
		{
			FlushLogs();
		}
	}

	private void FlushLogs()
	{
		string[] logs;
		lock (_logBuffer)
		{
			if (_logBuffer.Length == 0)
			{
				return;
			}
			logs = _logBuffer.ToString().Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			_logBuffer.Clear();
		}
		SafeInvoke(delegate
		{
			lstLogs.BeginUpdate();
			try
			{
				int num = lstLogs.Items.Count + logs.Length - 1000;
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						lstLogs.Items.RemoveAt(0);
					}
				}
				ListBox.ObjectCollection items = lstLogs.Items;
				object[] items2 = logs;
				items.AddRange(items2);
				lstLogs.TopIndex = Math.Max(lstLogs.Items.Count - 1, 0);
			}
			finally
			{
				lstLogs.EndUpdate();
			}
			_lastLogUpdate = DateTime.Now;
		});
	}

	private void UpdateUIState()
	{
		bool isConnected = AppClient.IsConnected;
		SafeInvoke(delegate
		{
			btnConnect.Enabled = !isConnected;
			btnDisconnect.Enabled = isConnected;
			txtIpAddress.Enabled = !isConnected;
			txtPort.Enabled = !isConnected;
			txtNickname.Enabled = !isConnected;
			cmbJob.Enabled = !isConnected;
			txtCommand.Enabled = isConnected;
			btnSend.Enabled = isConnected;
		});
	}

	private void UpdateStatus(string text, Color color)
	{
		SafeInvoke(delegate
		{
			lblStatus.Text = text;
			lblStatus.ForeColor = color;
		});
	}

	private void SafeInvoke(Action action)
	{
		if (!base.IsDisposed && base.IsHandleCreated)
		{
			if (base.InvokeRequired)
			{
				BeginInvoke(action);
			}
			else
			{
				action();
			}
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		SaveSettings();
		_uiUpdateTimer?.Dispose();
		AppClient.Disconnect();
		base.OnFormClosing(e);
	}

	private void btnClearLogs_Click(object sender, EventArgs e)
	{
		lstLogs.Items.Clear();
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Hide();
	}

	private void btnClose_MouseEnter(object sender, EventArgs e)
	{
		btnClose.BackColor = _buttonSecondaryColor;
		btnClose.ForeColor = Color.White;
	}

	private void btnClose_MouseLeave(object sender, EventArgs e)
	{
		btnClose.BackColor = Color.Transparent;
		btnClose.ForeColor = _textPrimaryColor;
	}

	private async void btnSend_Click(object sender, EventArgs e)
	{
		if (!ValidateCommandInput())
		{
			return;
		}
		try
		{
			string command = txtCommand.Text.Trim();
			await AppClient.SendCommandAsync(command);
			AddLog("Komut " + command + " gönderildi");
			txtCommand.Clear();
		}
		catch (Exception ex)
		{
			AddLog("Gönderme hatası: " + ex.Message);
		}
	}

	private void SettingsChanged(object sender, EventArgs e)
	{
		SaveSettings();
	}

	private void cmbJob_SelectedIndexChanged(object sender, EventArgs e)
	{
		_clientSettings.CharacterJob = ((cmbJob.SelectedIndex < 0) ? 1 : (cmbJob.SelectedIndex + 1));
	}

	private void btnConnect_MouseEnter(object sender, EventArgs e)
	{
		if (btnConnect.Enabled)
		{
			btnConnect.BackColor = _primaryHoverColor;
		}
	}

	private void btnConnect_MouseLeave(object sender, EventArgs e)
	{
		if (btnConnect.Enabled)
		{
			btnConnect.BackColor = _buttonPrimaryColor;
		}
	}

	private void btnSend_MouseEnter(object sender, EventArgs e)
	{
		if (btnSend.Enabled)
		{
			btnSend.BackColor = _primaryHoverColor;
		}
	}

	private void btnSend_MouseLeave(object sender, EventArgs e)
	{
		if (btnSend.Enabled)
		{
			btnSend.BackColor = _buttonPrimaryColor;
		}
	}

	private void btnDisconnect_MouseEnter(object sender, EventArgs e)
	{
		if (btnDisconnect.Enabled)
		{
			btnDisconnect.BackColor = _secondaryHoverColor;
		}
	}

	private void btnDisconnect_MouseLeave(object sender, EventArgs e)
	{
		if (btnDisconnect.Enabled)
		{
			btnDisconnect.BackColor = _buttonSecondaryColor;
		}
	}

	private void btnClearLogs_MouseEnter(object sender, EventArgs e)
	{
		btnClearLogs.BackColor = _secondaryHoverColor;
	}

	private void btnClearLogs_MouseLeave(object sender, EventArgs e)
	{
		btnClearLogs.BackColor = _buttonSecondaryColor;
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
		this.mainContainer = new System.Windows.Forms.Panel();
		this.headerPanel = new System.Windows.Forms.Panel();
		this.lblTitle = new System.Windows.Forms.Label();
		this.btnClose = new System.Windows.Forms.Button();
		this.connectionPanel = new System.Windows.Forms.Panel();
		this.lblNickname = new System.Windows.Forms.Label();
		this.txtNickname = new System.Windows.Forms.TextBox();
		this.lblJob = new System.Windows.Forms.Label();
		this.cmbJob = new System.Windows.Forms.ComboBox();
		this.lblIp = new System.Windows.Forms.Label();
		this.txtIpAddress = new System.Windows.Forms.TextBox();
		this.lblPort = new System.Windows.Forms.Label();
		this.txtPort = new System.Windows.Forms.TextBox();
		this.btnConnect = new System.Windows.Forms.Button();
		this.btnDisconnect = new System.Windows.Forms.Button();
		this.logPanel = new System.Windows.Forms.Panel();
		this.lstLogs = new System.Windows.Forms.ListBox();
		this.btnClearLogs = new System.Windows.Forms.Button();
		this.commandPanel = new System.Windows.Forms.Panel();
		this.txtCommand = new System.Windows.Forms.TextBox();
		this.btnSend = new System.Windows.Forms.Button();
		this.statusPanel = new System.Windows.Forms.Panel();
		this.lblStatus = new System.Windows.Forms.Label();
		this.headerPanel.SuspendLayout();
		this.connectionPanel.SuspendLayout();
		this.logPanel.SuspendLayout();
		this.commandPanel.SuspendLayout();
		this.statusPanel.SuspendLayout();
		base.SuspendLayout();
		this.mainContainer.BackColor = System.Drawing.Color.FromArgb(28, 28, 33);
		this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.mainContainer.Location = new System.Drawing.Point(0, 0);
		this.mainContainer.Name = "mainContainer";
		this.mainContainer.Size = new System.Drawing.Size(500, 450);
		this.mainContainer.TabIndex = 0;
		this.headerPanel.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.headerPanel.Controls.Add(this.lblTitle);
		this.headerPanel.Controls.Add(this.btnClose);
		this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.headerPanel.Location = new System.Drawing.Point(0, 120);
		this.headerPanel.Name = "headerPanel";
		this.headerPanel.Size = new System.Drawing.Size(500, 30);
		this.headerPanel.TabIndex = 0;
		this.lblTitle.AutoSize = true;
		this.lblTitle.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lblTitle.Location = new System.Drawing.Point(10, 8);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(98, 14);
		this.lblTitle.TabIndex = 1;
		this.lblTitle.Text = "SnapNet Client";
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.btnClose.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.btnClose.Location = new System.Drawing.Point(475, 5);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(20, 20);
		this.btnClose.TabIndex = 0;
		this.btnClose.Text = "✕";
		this.btnClose.UseVisualStyleBackColor = false;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		this.btnClose.MouseEnter += new System.EventHandler(btnClose_MouseEnter);
		this.btnClose.MouseLeave += new System.EventHandler(btnClose_MouseLeave);
		this.connectionPanel.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.connectionPanel.Controls.Add(this.lblNickname);
		this.connectionPanel.Controls.Add(this.txtNickname);
		this.connectionPanel.Controls.Add(this.lblJob);
		this.connectionPanel.Controls.Add(this.cmbJob);
		this.connectionPanel.Controls.Add(this.lblIp);
		this.connectionPanel.Controls.Add(this.txtIpAddress);
		this.connectionPanel.Controls.Add(this.lblPort);
		this.connectionPanel.Controls.Add(this.txtPort);
		this.connectionPanel.Controls.Add(this.btnConnect);
		this.connectionPanel.Controls.Add(this.btnDisconnect);
		this.connectionPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.connectionPanel.Location = new System.Drawing.Point(0, 0);
		this.connectionPanel.Name = "connectionPanel";
		this.connectionPanel.Padding = new System.Windows.Forms.Padding(15);
		this.connectionPanel.Size = new System.Drawing.Size(500, 120);
		this.connectionPanel.TabIndex = 1;
		this.lblNickname.AutoSize = true;
		this.lblNickname.Font = new System.Drawing.Font("Tahoma", 8f);
		this.lblNickname.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lblNickname.Location = new System.Drawing.Point(15, 15);
		this.lblNickname.Name = "lblNickname";
		this.lblNickname.Size = new System.Drawing.Size(56, 13);
		this.lblNickname.TabIndex = 0;
		this.lblNickname.Text = "Nickname:";
		this.txtNickname.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.txtNickname.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtNickname.Font = new System.Drawing.Font("Tahoma", 8f);
		this.txtNickname.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.txtNickname.Location = new System.Drawing.Point(15, 31);
		this.txtNickname.Name = "txtNickname";
		this.txtNickname.Size = new System.Drawing.Size(150, 20);
		this.txtNickname.TabIndex = 1;
		this.lblJob.AutoSize = true;
		this.lblJob.Font = new System.Drawing.Font("Tahoma", 8f);
		this.lblJob.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lblJob.Location = new System.Drawing.Point(15, 60);
		this.lblJob.Name = "lblJob";
		this.lblJob.Size = new System.Drawing.Size(28, 13);
		this.lblJob.TabIndex = 2;
		this.lblJob.Text = "Job:";
		this.cmbJob.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.cmbJob.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cmbJob.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.cmbJob.Font = new System.Drawing.Font("Tahoma", 8f);
		this.cmbJob.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.cmbJob.FormattingEnabled = true;
		this.cmbJob.Items.AddRange(new object[5] { "Mage", "Rogue", "Warrior", "Kurian", "Priest" });
		this.cmbJob.Location = new System.Drawing.Point(15, 76);
		this.cmbJob.Name = "cmbJob";
		this.cmbJob.Size = new System.Drawing.Size(150, 21);
		this.cmbJob.TabIndex = 3;
		this.lblIp.AutoSize = true;
		this.lblIp.Font = new System.Drawing.Font("Tahoma", 8f);
		this.lblIp.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lblIp.Location = new System.Drawing.Point(180, 15);
		this.lblIp.Name = "lblIp";
		this.lblIp.Size = new System.Drawing.Size(63, 13);
		this.lblIp.TabIndex = 4;
		this.lblIp.Text = "IP Address:";
		this.txtIpAddress.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.txtIpAddress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtIpAddress.Font = new System.Drawing.Font("Tahoma", 8f);
		this.txtIpAddress.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.txtIpAddress.Location = new System.Drawing.Point(180, 31);
		this.txtIpAddress.Name = "txtIpAddress";
		this.txtIpAddress.Size = new System.Drawing.Size(150, 20);
		this.txtIpAddress.TabIndex = 5;
		this.lblPort.AutoSize = true;
		this.lblPort.Font = new System.Drawing.Font("Tahoma", 8f);
		this.lblPort.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lblPort.Location = new System.Drawing.Point(180, 60);
		this.lblPort.Name = "lblPort";
		this.lblPort.Size = new System.Drawing.Size(31, 13);
		this.lblPort.TabIndex = 6;
		this.lblPort.Text = "Port:";
		this.txtPort.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.txtPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPort.Font = new System.Drawing.Font("Tahoma", 8f);
		this.txtPort.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.txtPort.Location = new System.Drawing.Point(180, 76);
		this.txtPort.Name = "txtPort";
		this.txtPort.Size = new System.Drawing.Size(150, 20);
		this.txtPort.TabIndex = 7;
		this.btnConnect.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.btnConnect.FlatAppearance.BorderSize = 0;
		this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnConnect.Font = new System.Drawing.Font("Tahoma", 8f, System.Drawing.FontStyle.Bold);
		this.btnConnect.ForeColor = System.Drawing.Color.White;
		this.btnConnect.Location = new System.Drawing.Point(345, 15);
		this.btnConnect.Name = "btnConnect";
		this.btnConnect.Size = new System.Drawing.Size(140, 35);
		this.btnConnect.TabIndex = 8;
		this.btnConnect.Text = "Bağlan";
		this.btnConnect.UseVisualStyleBackColor = false;
		this.btnConnect.Click += new System.EventHandler(btnConnect_Click);
		this.btnConnect.MouseEnter += new System.EventHandler(btnConnect_MouseEnter);
		this.btnConnect.MouseLeave += new System.EventHandler(btnConnect_MouseLeave);
		this.btnDisconnect.BackColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.btnDisconnect.Enabled = false;
		this.btnDisconnect.FlatAppearance.BorderSize = 0;
		this.btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnDisconnect.Font = new System.Drawing.Font("Tahoma", 8f, System.Drawing.FontStyle.Bold);
		this.btnDisconnect.ForeColor = System.Drawing.Color.White;
		this.btnDisconnect.Location = new System.Drawing.Point(345, 60);
		this.btnDisconnect.Name = "btnDisconnect";
		this.btnDisconnect.Size = new System.Drawing.Size(140, 35);
		this.btnDisconnect.TabIndex = 9;
		this.btnDisconnect.Text = "Bağlantıyı Kes";
		this.btnDisconnect.UseVisualStyleBackColor = false;
		this.btnDisconnect.Click += new System.EventHandler(btnDisconnect_Click);
		this.btnDisconnect.MouseEnter += new System.EventHandler(btnDisconnect_MouseEnter);
		this.btnDisconnect.MouseLeave += new System.EventHandler(btnDisconnect_MouseLeave);
		this.logPanel.BackColor = System.Drawing.Color.FromArgb(28, 28, 33);
		this.logPanel.Controls.Add(this.lstLogs);
		this.logPanel.Controls.Add(this.btnClearLogs);
		this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.logPanel.Location = new System.Drawing.Point(0, 150);
		this.logPanel.Name = "logPanel";
		this.logPanel.Padding = new System.Windows.Forms.Padding(15);
		this.logPanel.Size = new System.Drawing.Size(500, 200);
		this.logPanel.TabIndex = 2;
		this.lstLogs.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.lstLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstLogs.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lstLogs.Font = new System.Drawing.Font("Consolas", 8f);
		this.lstLogs.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lstLogs.FormattingEnabled = true;
		this.lstLogs.ItemHeight = 13;
		this.lstLogs.Location = new System.Drawing.Point(15, 15);
		this.lstLogs.Name = "lstLogs";
		this.lstLogs.Size = new System.Drawing.Size(470, 170);
		this.lstLogs.TabIndex = 0;
		this.btnClearLogs.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnClearLogs.BackColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.btnClearLogs.FlatAppearance.BorderSize = 0;
		this.btnClearLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClearLogs.Font = new System.Drawing.Font("Tahoma", 8f);
		this.btnClearLogs.ForeColor = System.Drawing.Color.White;
		this.btnClearLogs.Location = new System.Drawing.Point(350, 190);
		this.btnClearLogs.Name = "btnClearLogs";
		this.btnClearLogs.Size = new System.Drawing.Size(135, 20);
		this.btnClearLogs.TabIndex = 1;
		this.btnClearLogs.Text = "Logları Temizle";
		this.btnClearLogs.UseVisualStyleBackColor = false;
		this.btnClearLogs.Click += new System.EventHandler(btnClearLogs_Click);
		this.btnClearLogs.MouseEnter += new System.EventHandler(btnClearLogs_MouseEnter);
		this.btnClearLogs.MouseLeave += new System.EventHandler(btnClearLogs_MouseLeave);
		this.commandPanel.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.commandPanel.Controls.Add(this.txtCommand);
		this.commandPanel.Controls.Add(this.btnSend);
		this.commandPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.commandPanel.Location = new System.Drawing.Point(0, 350);
		this.commandPanel.Name = "commandPanel";
		this.commandPanel.Padding = new System.Windows.Forms.Padding(15);
		this.commandPanel.Size = new System.Drawing.Size(500, 50);
		this.commandPanel.TabIndex = 3;
		this.txtCommand.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.txtCommand.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtCommand.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtCommand.Font = new System.Drawing.Font("Tahoma", 9f);
		this.txtCommand.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.txtCommand.Location = new System.Drawing.Point(15, 15);
		this.txtCommand.Name = "txtCommand";
		this.txtCommand.Size = new System.Drawing.Size(350, 22);
		this.txtCommand.TabIndex = 0;
		this.btnSend.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
		this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
		this.btnSend.FlatAppearance.BorderSize = 0;
		this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSend.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.btnSend.ForeColor = System.Drawing.Color.White;
		this.btnSend.Location = new System.Drawing.Point(365, 15);
		this.btnSend.Name = "btnSend";
		this.btnSend.Size = new System.Drawing.Size(120, 20);
		this.btnSend.TabIndex = 1;
		this.btnSend.Text = "Gönder";
		this.btnSend.UseVisualStyleBackColor = false;
		this.btnSend.Click += new System.EventHandler(btnSend_Click);
		this.btnSend.MouseEnter += new System.EventHandler(btnSend_MouseEnter);
		this.btnSend.MouseLeave += new System.EventHandler(btnSend_MouseLeave);
		this.statusPanel.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.statusPanel.Controls.Add(this.lblStatus);
		this.statusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.statusPanel.Location = new System.Drawing.Point(0, 400);
		this.statusPanel.Name = "statusPanel";
		this.statusPanel.Size = new System.Drawing.Size(500, 50);
		this.statusPanel.TabIndex = 4;
		this.lblStatus.AutoSize = true;
		this.lblStatus.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(0, 153, 102);
		this.lblStatus.Location = new System.Drawing.Point(15, 18);
		this.lblStatus.Name = "lblStatus";
		this.lblStatus.Size = new System.Drawing.Size(102, 14);
		this.lblStatus.TabIndex = 0;
		this.lblStatus.Text = "● Bağlantı Hazır";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(500, 450);
		base.Controls.Add(this.logPanel);
		base.Controls.Add(this.headerPanel);
		base.Controls.Add(this.connectionPanel);
		base.Controls.Add(this.commandPanel);
		base.Controls.Add(this.statusPanel);
		this.Font = new System.Drawing.Font("Tahoma", 8f);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "ClientForm";
		this.Text = "SnapNet Client";
		this.headerPanel.ResumeLayout(false);
		this.headerPanel.PerformLayout();
		this.connectionPanel.ResumeLayout(false);
		this.connectionPanel.PerformLayout();
		this.logPanel.ResumeLayout(false);
		this.commandPanel.ResumeLayout(false);
		this.commandPanel.PerformLayout();
		this.statusPanel.ResumeLayout(false);
		this.statusPanel.PerformLayout();
		base.ResumeLayout(false);
	}
}
