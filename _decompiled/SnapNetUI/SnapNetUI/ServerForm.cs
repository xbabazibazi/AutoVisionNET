using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SnapNetUI;

public class ServerForm : Form
{
	private Server _server = new Server();

	private volatile bool _isRunning;

	private ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();

	private System.Threading.Timer _logTimer;

	private Stopwatch _uptime = new Stopwatch();

	private Alarm _alarm = new Alarm();

	private bool _isSilentMode = false;

	private IContainer components = null;

	private Panel mainContainer;

	private Panel titlePanel;

	private Label lblTitle;

	private Label lblSubtitle;

	private Button btnMinimize;

	private Button btnClose;

	private Panel controlPanel;

	private TextBox txtPort;

	private Label lblPort;

	private Button btnStart;

	private Button btnStop;

	private Button btnClearLogs;

	private Panel commandPanel;

	private ListBox lstCommands;

	private Button btnSendCommand;

	private SplitContainer mainSplitContainer;

	private ListBox lstClients;

	private ListBox lstLogs;

	private Label lblClients;

	private Label lblLogs;

	private Label lblServerStatus;

	private Label lblClientCount;

	private Button btnCopyLogs;

	private Button btnStopAlarm;

	private Panel statusPanel;

	private Button btnSilentMode;

	private Label lblUptime;

	private Label lblJobBreakdown;

	private Label lblLastError;

	public ServerForm()
	{
		InitializeComponent();
		InitializeServerEvents();
		InitializeCommands();
		SetupModernUI();
		InitializeExtendedStatus();
		StartLogTimer();
		UpdateSilentModeButton();
	}

	private void InitializeExtendedStatus()
	{
		statusPanel.Height = 85;

		lblUptime = new Label
		{
			Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
			ForeColor = Color.FromArgb(200, 200, 220),
			Location = new Point(5, 30),
			Size = new Size(220, 25),
			TextAlign = ContentAlignment.MiddleLeft,
			Text = "Çalışma süresi: 00:00:00"
		};
		lblJobBreakdown = new Label
		{
			Font = new Font("Segoe UI", 9f),
			ForeColor = Color.FromArgb(180, 200, 220),
			Location = new Point(230, 30),
			Size = new Size(745, 25),
			TextAlign = ContentAlignment.MiddleRight,
			AutoEllipsis = true,
			Text = "(bağlı cihaz yok)"
		};
		lblLastError = new Label
		{
			Font = new Font("Segoe UI", 8.5f),
			ForeColor = Color.FromArgb(220, 120, 120),
			Location = new Point(5, 55),
			Size = new Size(970, 25),
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			Text = ""
		};
		statusPanel.Controls.Add(lblUptime);
		statusPanel.Controls.Add(lblJobBreakdown);
		statusPanel.Controls.Add(lblLastError);
	}

	private void StartLogTimer()
	{
		_logTimer = new System.Threading.Timer(delegate
		{
			UpdateLogs();
			UpdateUptimeLabel();
		}, null, 100, 100);
	}

	private void UpdateUptimeLabel()
	{
		TimeSpan elapsed = _uptime.Elapsed;
		SafeInvoke(delegate
		{
			lblUptime.Text = "Çalışma süresi: " + elapsed.ToString("hh\\:mm\\:ss");
		});
	}

	private void SetupModernUI()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		base.Padding = new Padding(1);
		BackColor = Color.FromArgb(45, 45, 55);
		btnStart.FlatStyle = FlatStyle.Flat;
		btnStop.FlatStyle = FlatStyle.Flat;
		btnSendCommand.FlatStyle = FlatStyle.Flat;
		btnStopAlarm.FlatStyle = FlatStyle.Flat;
		btnClearLogs.FlatStyle = FlatStyle.Flat;
		btnCopyLogs.FlatStyle = FlatStyle.Flat;
		btnSilentMode.FlatStyle = FlatStyle.Flat;
		mainContainer.Paint += delegate(object? s, PaintEventArgs e)
		{
			ControlPaint.DrawBorder(e.Graphics, mainContainer.ClientRectangle, Color.FromArgb(80, 80, 100), ButtonBorderStyle.Solid);
		};
	}

	private void InitializeCommands()
	{
		lstCommands.Items.Add("501 - Çark Çevir");
		lstCommands.Items.Add("201 - ReReRe");
		lstCommands.Items.Add("Warrior Genie Aç");
		lstCommands.Items.Add("Priest Genie Aç");
		lstCommands.Items.Add("101 - Tüm alarmları durdur");
		lstCommands.Items.Add("301 - Güncellemeleri yap");
		lstCommands.Items.Add("999 - Özel komut (log'a yazar)");
	}

	private void InitializeServerEvents()
	{
		_server.ClientCountChanged += delegate(int count)
		{
			SafeInvoke(delegate
			{
				lblClientCount.Text = $"{count} Bağlı Cihaz";
				UpdateClientList();
			});
		};
		_server.LogMessage += delegate(string msg)
		{
			if (!msg.Contains("PING") && !msg.Contains("PONG") && !msg.Contains("UNPROCESSED_MSG") && !msg.Contains("COMMAND_RECEIVED"))
			{
				_logQueue.Enqueue(msg);
				if (msg.Contains("ERROR", StringComparison.OrdinalIgnoreCase) || msg.Contains("hata", StringComparison.OrdinalIgnoreCase))
				{
					ShowErrorNotification(msg);
				}
			}
		};
		_server.RegisterCommand("101", delegate(string nickname)
		{
			_logQueue.Enqueue(nickname + " tüm alarmları durdurdu");
		});
		_server.RegisterCommand("201", delegate(string nickname)
		{
			_logQueue.Enqueue(nickname + " ReReRe komutunu çalıştırdı");
		});
		_server.RegisterCommand("301", delegate(string nickname)
		{
			_logQueue.Enqueue(nickname + " üstü doldu.");
			if (!_isSilentMode)
			{
				SafeInvoke(delegate
				{
					_alarm.StartAlarm();
				});
			}
		});
		_server.RegisterCommand("999", delegate(string nickname)
		{
			_logQueue.Enqueue("OZEL_KOMUT:" + nickname + " tarafından çalıştırıldı");
		});
	}

	private void BtnStopAlarm_Click(object sender, EventArgs e)
	{
		_alarm.StopAlarm();
		_logQueue.Enqueue("Alarm manuel olarak durduruldu");
	}

	private async void BtnStart_Click(object sender, EventArgs e)
	{
		if (!int.TryParse(txtPort.Text, out var port) || port < 1 || port > 65535)
		{
			MessageBox.Show("Geçersiz port numarası!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		try
		{
			_isRunning = true;
			_uptime.Start();
			UpdateUI();
			await _server.StartAsync(port);
		}
		catch (Exception ex)
		{
			HandleError("Başlatma hatası: " + ex.Message);
		}
	}

	private async void BtnStop_Click(object sender, EventArgs e)
	{
		try
		{
			await _server.StopAsync();
			_isRunning = false;
			_uptime.Stop();
			UpdateUI();
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			HandleError("Durdurma hatası: " + ex2.Message);
		}
	}

	private void BtnCopyLogs_Click(object sender, EventArgs e)
	{
		try
		{
			string text = string.Join(Environment.NewLine, lstLogs.Items.Cast<string>());
			if (!string.IsNullOrEmpty(text))
			{
				Clipboard.SetText(text);
				_logQueue.Enqueue("Loglar panoya kopyalandı!");
			}
		}
		catch (Exception ex)
		{
			_logQueue.Enqueue("Log kopyalama hatası: " + ex.Message);
		}
	}

	private void UpdateLogs()
	{
		if (_logQueue.IsEmpty)
		{
			return;
		}
		List<string> logsToAdd = new List<string>();
		string result;
		while (_logQueue.TryDequeue(out result) && logsToAdd.Count < 50)
		{
			logsToAdd.Add($"[{DateTime.Now:HH:mm:ss.fff}] {result}");
		}
		if (logsToAdd.Count <= 0)
		{
			return;
		}
		SafeInvoke(delegate
		{
			lstLogs.BeginUpdate();
			try
			{
				ListBox.ObjectCollection items = lstLogs.Items;
				object[] items2 = logsToAdd.ToArray();
				items.AddRange(items2);
				while (lstLogs.Items.Count > 2000)
				{
					lstLogs.Items.RemoveAt(0);
				}
				lstLogs.TopIndex = lstLogs.Items.Count - 1;
			}
			finally
			{
				lstLogs.EndUpdate();
			}
		});
	}

	private void UpdateClientList()
	{
		var clientList = _server.GetClientList().ToList();
		string[] clients = (from c in clientList
			select $"{c.nickname} | {c.job} | {c.endpoint}").ToArray();
		string breakdown = string.Join("   ", clientList
			.GroupBy((c) => c.job)
			.OrderBy((g) => g.Key)
			.Select((g) => $"{g.Key}: {g.Count()}"));
		SafeInvoke(delegate
		{
			lstClients.BeginUpdate();
			lstClients.Items.Clear();
			if (clients.Any())
			{
				ListBox.ObjectCollection items = lstClients.Items;
				object[] items2 = clients;
				items.AddRange(items2);
			}
			lstClients.EndUpdate();
			lblJobBreakdown.Text = string.IsNullOrEmpty(breakdown) ? "(bağlı cihaz yok)" : breakdown;
		});
	}

	private async void BtnSendCommand_Click(object sender, EventArgs e)
	{
		try
		{
			string[] selectedCommands = (from string c in lstCommands.SelectedItems
				select c.Split('-')[0].Trim()).ToArray();
			if (!selectedCommands.Any())
			{
				return;
			}
			string[] array = selectedCommands;
			foreach (string command in array)
			{
				if (command == "Warrior Genie Aç")
				{
					await _server.SendCommandToSpecificJobAsync("401", Server.JobType.Warrior);
					_logQueue.Enqueue("401 komutu sadece Warrior'lara gönderildi");
				}
				else if (command == "Priest Genie Aç")
				{
					await _server.SendCommandToSpecificJobAsync("401", Server.JobType.Priest);
					_logQueue.Enqueue("401 komutu sadece Priest'lere gönderildi");
				}
				else
				{
					string cmdCode = command.Split(' ')[0];
					await _server.SendCommandToAllClientsAsync(cmdCode, withDelay: false);
					_logQueue.Enqueue("KOMUT_GONDERILDI:" + cmdCode);
				}
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			HandleError("Komut gönderim hatası: " + ex2.Message);
		}
	}

	private void BtnSilentMode_Click(object sender, EventArgs e)
	{
		_isSilentMode = !_isSilentMode;
		_alarm.SetSilentMode(_isSilentMode);
		if (_isSilentMode)
		{
			_logQueue.Enqueue("Sessiz mod aktif edildi");
		}
		else
		{
			_logQueue.Enqueue("Sessiz mod kapatıldı");
		}
		UpdateSilentModeButton();
	}

	private void UpdateSilentModeButton()
	{
		if (_isSilentMode)
		{
			btnSilentMode.Text = "SESSİZ MOD: AÇIK";
			btnSilentMode.BackColor = Color.FromArgb(80, 180, 80);
		}
		else
		{
			btnSilentMode.Text = "SESSİZ MOD: KAPALI";
			btnSilentMode.BackColor = Color.FromArgb(120, 120, 120);
		}
	}

	private void BtnClearLogs_Click(object sender, EventArgs e)
	{
		lstLogs.Items.Clear();
		_logQueue = new ConcurrentQueue<string>();
	}

	private void SafeInvoke(Action action)
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

	private void UpdateUI()
	{
		SafeInvoke(delegate
		{
			btnStart.Enabled = !_isRunning;
			btnStop.Enabled = _isRunning;
			txtPort.Enabled = !_isRunning;
			lblServerStatus.Text = (_isRunning ? "● ÇALIŞIYOR" : "● DURDURULDU");
			lblServerStatus.BackColor = (_isRunning ? Color.FromArgb(80, 180, 80) : Color.FromArgb(180, 80, 80));
		});
	}

	private ErrorToastForm _currentToast;

	private void HandleError(string message)
	{
		_logQueue.Enqueue("HATA:" + message);
		UpdateUI();
		ShowErrorNotification(message);
	}

	private void ShowErrorNotification(string message)
	{
		SafeInvoke(delegate
		{
			lblLastError.Text = $"Son hata ({DateTime.Now:HH:mm:ss}): {message}";
			_currentToast?.Close();
			_currentToast = new ErrorToastForm(message);
			_currentToast.Show();
		});
	}

	protected override async void OnFormClosing(FormClosingEventArgs e)
	{
		if (_isRunning)
		{
			e.Cancel = true;
			btnStop.Enabled = false;
			await _server.StopAsync();
			_isRunning = false;
			Close();
		}
		_logTimer?.Dispose();
		_alarm?.Dispose();
		base.OnFormClosing(e);
	}

	private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			NativeMethods.ReleaseCapture();
			NativeMethods.SendMessage(base.Handle, 161, 2, 0);
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
		this.mainContainer = new System.Windows.Forms.Panel();
		this.statusPanel = new System.Windows.Forms.Panel();
		this.lblClientCount = new System.Windows.Forms.Label();
		this.lblServerStatus = new System.Windows.Forms.Label();
		this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
		this.lstClients = new System.Windows.Forms.ListBox();
		this.lblClients = new System.Windows.Forms.Label();
		this.lstLogs = new System.Windows.Forms.ListBox();
		this.lblLogs = new System.Windows.Forms.Label();
		this.btnCopyLogs = new System.Windows.Forms.Button();
		this.commandPanel = new System.Windows.Forms.Panel();
		this.btnSendCommand = new System.Windows.Forms.Button();
		this.lstCommands = new System.Windows.Forms.ListBox();
		this.controlPanel = new System.Windows.Forms.Panel();
		this.btnSilentMode = new System.Windows.Forms.Button();
		this.btnStopAlarm = new System.Windows.Forms.Button();
		this.btnClearLogs = new System.Windows.Forms.Button();
		this.txtPort = new System.Windows.Forms.TextBox();
		this.lblPort = new System.Windows.Forms.Label();
		this.btnStop = new System.Windows.Forms.Button();
		this.btnStart = new System.Windows.Forms.Button();
		this.titlePanel = new System.Windows.Forms.Panel();
		this.btnClose = new System.Windows.Forms.Button();
		this.btnMinimize = new System.Windows.Forms.Button();
		this.lblSubtitle = new System.Windows.Forms.Label();
		this.lblTitle = new System.Windows.Forms.Label();
		this.mainContainer.SuspendLayout();
		this.statusPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.mainSplitContainer).BeginInit();
		this.mainSplitContainer.Panel1.SuspendLayout();
		this.mainSplitContainer.Panel2.SuspendLayout();
		this.mainSplitContainer.SuspendLayout();
		this.commandPanel.SuspendLayout();
		this.controlPanel.SuspendLayout();
		this.titlePanel.SuspendLayout();
		base.SuspendLayout();
		this.mainContainer.BackColor = System.Drawing.Color.FromArgb(30, 30, 40);
		this.mainContainer.Controls.Add(this.statusPanel);
		this.mainContainer.Controls.Add(this.mainSplitContainer);
		this.mainContainer.Controls.Add(this.commandPanel);
		this.mainContainer.Controls.Add(this.controlPanel);
		this.mainContainer.Controls.Add(this.titlePanel);
		this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.mainContainer.Location = new System.Drawing.Point(0, 0);
		this.mainContainer.Name = "mainContainer";
		this.mainContainer.Padding = new System.Windows.Forms.Padding(10);
		this.mainContainer.Size = new System.Drawing.Size(1000, 700);
		this.mainContainer.TabIndex = 0;
		this.statusPanel.BackColor = System.Drawing.Color.FromArgb(40, 40, 50);
		this.statusPanel.Controls.Add(this.lblClientCount);
		this.statusPanel.Controls.Add(this.lblServerStatus);
		this.statusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.statusPanel.Location = new System.Drawing.Point(10, 650);
		this.statusPanel.Name = "statusPanel";
		this.statusPanel.Padding = new System.Windows.Forms.Padding(5);
		this.statusPanel.Size = new System.Drawing.Size(980, 40);
		this.statusPanel.TabIndex = 9;
		this.lblClientCount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblClientCount.Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblClientCount.ForeColor = System.Drawing.Color.FromArgb(200, 200, 220);
		this.lblClientCount.Location = new System.Drawing.Point(780, 5);
		this.lblClientCount.Name = "lblClientCount";
		this.lblClientCount.Size = new System.Drawing.Size(195, 30);
		this.lblClientCount.TabIndex = 7;
		this.lblClientCount.Text = "0 Bağlı Cihaz";
		this.lblClientCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblServerStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblServerStatus.ForeColor = System.Drawing.Color.FromArgb(200, 200, 220);
		this.lblServerStatus.Location = new System.Drawing.Point(5, 5);
		this.lblServerStatus.Name = "lblServerStatus";
		this.lblServerStatus.Size = new System.Drawing.Size(195, 30);
		this.lblServerStatus.TabIndex = 6;
		this.lblServerStatus.Text = "● DURDURULDU";
		this.lblServerStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.mainSplitContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.mainSplitContainer.BackColor = System.Drawing.Color.FromArgb(40, 40, 50);
		this.mainSplitContainer.Location = new System.Drawing.Point(10, 280);
		this.mainSplitContainer.Name = "mainSplitContainer";
		this.mainSplitContainer.Panel1.BackColor = System.Drawing.Color.FromArgb(40, 40, 50);
		this.mainSplitContainer.Panel1.Controls.Add(this.lstClients);
		this.mainSplitContainer.Panel1.Controls.Add(this.lblClients);
		this.mainSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
		this.mainSplitContainer.Panel2.BackColor = System.Drawing.Color.FromArgb(40, 40, 50);
		this.mainSplitContainer.Panel2.Controls.Add(this.lstLogs);
		this.mainSplitContainer.Panel2.Controls.Add(this.lblLogs);
		this.mainSplitContainer.Panel2.Controls.Add(this.btnCopyLogs);
		this.mainSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
		this.mainSplitContainer.Size = new System.Drawing.Size(980, 360);
		this.mainSplitContainer.SplitterDistance = 350;
		this.mainSplitContainer.SplitterWidth = 10;
		this.mainSplitContainer.TabIndex = 8;
		this.lstClients.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
		this.lstClients.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstClients.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lstClients.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.lstClients.ForeColor = System.Drawing.Color.FromArgb(220, 220, 240);
		this.lstClients.FormattingEnabled = true;
		this.lstClients.ItemHeight = 15;
		this.lstClients.Location = new System.Drawing.Point(0, 25);
		this.lstClients.Name = "lstClients";
		this.lstClients.Size = new System.Drawing.Size(345, 335);
		this.lstClients.TabIndex = 3;
		this.lblClients.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
		this.lblClients.Dock = System.Windows.Forms.DockStyle.Top;
		this.lblClients.Font = new System.Drawing.Font("Segoe UI", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblClients.ForeColor = System.Drawing.Color.White;
		this.lblClients.Location = new System.Drawing.Point(0, 0);
		this.lblClients.Name = "lblClients";
		this.lblClients.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
		this.lblClients.Size = new System.Drawing.Size(345, 25);
		this.lblClients.TabIndex = 0;
		this.lblClients.Text = "BAĞLI CİHAZLAR";
		this.lblClients.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lstLogs.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
		this.lstLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstLogs.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lstLogs.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.lstLogs.ForeColor = System.Drawing.Color.FromArgb(220, 220, 240);
		this.lstLogs.FormattingEnabled = true;
		this.lstLogs.ItemHeight = 15;
		this.lstLogs.Location = new System.Drawing.Point(5, 25);
		this.lstLogs.Name = "lstLogs";
		this.lstLogs.Size = new System.Drawing.Size(615, 335);
		this.lstLogs.TabIndex = 5;
		this.lblLogs.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
		this.lblLogs.Dock = System.Windows.Forms.DockStyle.Top;
		this.lblLogs.Font = new System.Drawing.Font("Segoe UI", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblLogs.ForeColor = System.Drawing.Color.White;
		this.lblLogs.Location = new System.Drawing.Point(5, 0);
		this.lblLogs.Name = "lblLogs";
		this.lblLogs.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
		this.lblLogs.Size = new System.Drawing.Size(615, 25);
		this.lblLogs.TabIndex = 4;
		this.lblLogs.Text = "SİSTEM LOGLARI";
		this.lblLogs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.btnCopyLogs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnCopyLogs.BackColor = System.Drawing.Color.FromArgb(80, 120, 200);
		this.btnCopyLogs.FlatAppearance.BorderSize = 0;
		this.btnCopyLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCopyLogs.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnCopyLogs.ForeColor = System.Drawing.Color.White;
		this.btnCopyLogs.Location = new System.Drawing.Point(490, 0);
		this.btnCopyLogs.Name = "btnCopyLogs";
		this.btnCopyLogs.Size = new System.Drawing.Size(130, 25);
		this.btnCopyLogs.TabIndex = 6;
		this.btnCopyLogs.Text = "LOGLARI KOPYALA";
		this.btnCopyLogs.UseVisualStyleBackColor = false;
		this.btnCopyLogs.Click += new System.EventHandler(BtnCopyLogs_Click);
		this.commandPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.commandPanel.BackColor = System.Drawing.Color.Transparent;
		this.commandPanel.Controls.Add(this.btnSendCommand);
		this.commandPanel.Controls.Add(this.lstCommands);
		this.commandPanel.Location = new System.Drawing.Point(10, 180);
		this.commandPanel.Name = "commandPanel";
		this.commandPanel.Size = new System.Drawing.Size(980, 90);
		this.commandPanel.TabIndex = 7;
		this.btnSendCommand.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnSendCommand.BackColor = System.Drawing.Color.FromArgb(80, 120, 200);
		this.btnSendCommand.FlatAppearance.BorderSize = 0;
		this.btnSendCommand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSendCommand.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnSendCommand.ForeColor = System.Drawing.Color.White;
		this.btnSendCommand.Location = new System.Drawing.Point(800, 20);
		this.btnSendCommand.Name = "btnSendCommand";
		this.btnSendCommand.Size = new System.Drawing.Size(170, 50);
		this.btnSendCommand.TabIndex = 2;
		this.btnSendCommand.Text = "SEÇİLİ KOMUTLARI GÖNDER";
		this.btnSendCommand.UseVisualStyleBackColor = false;
		this.btnSendCommand.Click += new System.EventHandler(BtnSendCommand_Click);
		this.lstCommands.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lstCommands.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
		this.lstCommands.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstCommands.Font = new System.Drawing.Font("Segoe UI", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.lstCommands.ForeColor = System.Drawing.Color.FromArgb(220, 220, 240);
		this.lstCommands.FormattingEnabled = true;
		this.lstCommands.ItemHeight = 17;
		this.lstCommands.Location = new System.Drawing.Point(0, 0);
		this.lstCommands.Name = "lstCommands";
		this.lstCommands.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
		this.lstCommands.Size = new System.Drawing.Size(780, 85);
		this.lstCommands.TabIndex = 1;
		this.controlPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.controlPanel.BackColor = System.Drawing.Color.Transparent;
		this.controlPanel.Controls.Add(this.btnSilentMode);
		this.controlPanel.Controls.Add(this.btnStopAlarm);
		this.controlPanel.Controls.Add(this.btnClearLogs);
		this.controlPanel.Controls.Add(this.txtPort);
		this.controlPanel.Controls.Add(this.lblPort);
		this.controlPanel.Controls.Add(this.btnStop);
		this.controlPanel.Controls.Add(this.btnStart);
		this.controlPanel.Location = new System.Drawing.Point(10, 70);
		this.controlPanel.Name = "controlPanel";
		this.controlPanel.Size = new System.Drawing.Size(980, 100);
		this.controlPanel.TabIndex = 6;
		this.btnSilentMode.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnSilentMode.BackColor = System.Drawing.Color.FromArgb(120, 120, 120);
		this.btnSilentMode.FlatAppearance.BorderSize = 0;
		this.btnSilentMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSilentMode.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnSilentMode.ForeColor = System.Drawing.Color.White;
		this.btnSilentMode.Location = new System.Drawing.Point(520, 50);
		this.btnSilentMode.Name = "btnSilentMode";
		this.btnSilentMode.Size = new System.Drawing.Size(140, 40);
		this.btnSilentMode.TabIndex = 8;
		this.btnSilentMode.Text = "SESSİZ MOD: KAPALI";
		this.btnSilentMode.UseVisualStyleBackColor = false;
		this.btnSilentMode.Click += new System.EventHandler(BtnSilentMode_Click);
		this.btnStopAlarm.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnStopAlarm.BackColor = System.Drawing.Color.FromArgb(200, 80, 80);
		this.btnStopAlarm.FlatAppearance.BorderSize = 0;
		this.btnStopAlarm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnStopAlarm.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnStopAlarm.ForeColor = System.Drawing.Color.White;
		this.btnStopAlarm.Location = new System.Drawing.Point(680, 50);
		this.btnStopAlarm.Name = "btnStopAlarm";
		this.btnStopAlarm.Size = new System.Drawing.Size(140, 40);
		this.btnStopAlarm.TabIndex = 7;
		this.btnStopAlarm.Text = "ALARM DURDUR";
		this.btnStopAlarm.UseVisualStyleBackColor = false;
		this.btnStopAlarm.Click += new System.EventHandler(BtnStopAlarm_Click);
		this.btnClearLogs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClearLogs.BackColor = System.Drawing.Color.FromArgb(80, 120, 200);
		this.btnClearLogs.FlatAppearance.BorderSize = 0;
		this.btnClearLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClearLogs.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnClearLogs.ForeColor = System.Drawing.Color.White;
		this.btnClearLogs.Location = new System.Drawing.Point(840, 50);
		this.btnClearLogs.Name = "btnClearLogs";
		this.btnClearLogs.Size = new System.Drawing.Size(140, 40);
		this.btnClearLogs.TabIndex = 4;
		this.btnClearLogs.Text = "LOGLARI TEMİZLE";
		this.btnClearLogs.UseVisualStyleBackColor = false;
		this.btnClearLogs.Click += new System.EventHandler(BtnClearLogs_Click);
		this.txtPort.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
		this.txtPort.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtPort.Font = new System.Drawing.Font("Segoe UI", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.txtPort.ForeColor = System.Drawing.Color.White;
		this.txtPort.Location = new System.Drawing.Point(20, 50);
		this.txtPort.Name = "txtPort";
		this.txtPort.Size = new System.Drawing.Size(100, 20);
		this.txtPort.TabIndex = 3;
		this.txtPort.Text = "5000";
		this.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.lblPort.AutoSize = true;
		this.lblPort.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblPort.ForeColor = System.Drawing.Color.FromArgb(180, 180, 220);
		this.lblPort.Location = new System.Drawing.Point(20, 30);
		this.lblPort.Name = "lblPort";
		this.lblPort.Size = new System.Drawing.Size(34, 15);
		this.lblPort.TabIndex = 2;
		this.lblPort.Text = "Port:";
		this.btnStop.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnStop.BackColor = System.Drawing.Color.FromArgb(200, 80, 80);
		this.btnStop.Enabled = false;
		this.btnStop.FlatAppearance.BorderSize = 0;
		this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnStop.ForeColor = System.Drawing.Color.White;
		this.btnStop.Location = new System.Drawing.Point(680, 0);
		this.btnStop.Name = "btnStop";
		this.btnStop.Size = new System.Drawing.Size(140, 40);
		this.btnStop.TabIndex = 1;
		this.btnStop.Text = "DURDUR";
		this.btnStop.UseVisualStyleBackColor = false;
		this.btnStop.Click += new System.EventHandler(BtnStop_Click);
		this.btnStart.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnStart.BackColor = System.Drawing.Color.FromArgb(80, 180, 80);
		this.btnStart.FlatAppearance.BorderSize = 0;
		this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnStart.ForeColor = System.Drawing.Color.White;
		this.btnStart.Location = new System.Drawing.Point(840, 0);
		this.btnStart.Name = "btnStart";
		this.btnStart.Size = new System.Drawing.Size(140, 40);
		this.btnStart.TabIndex = 0;
		this.btnStart.Text = "BAŞLAT";
		this.btnStart.UseVisualStyleBackColor = false;
		this.btnStart.Click += new System.EventHandler(BtnStart_Click);
		this.titlePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.titlePanel.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
		this.titlePanel.Controls.Add(this.btnClose);
		this.titlePanel.Controls.Add(this.btnMinimize);
		this.titlePanel.Controls.Add(this.lblSubtitle);
		this.titlePanel.Controls.Add(this.lblTitle);
		this.titlePanel.Location = new System.Drawing.Point(10, 10);
		this.titlePanel.Name = "titlePanel";
		this.titlePanel.Size = new System.Drawing.Size(980, 50);
		this.titlePanel.TabIndex = 5;
		this.titlePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(TitlePanel_MouseDown);
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(200, 80, 80);
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnClose.ForeColor = System.Drawing.Color.White;
		this.btnClose.Location = new System.Drawing.Point(940, 0);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(40, 40);
		this.btnClose.TabIndex = 3;
		this.btnClose.Text = "X";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(BtnClose_Click);
		this.btnMinimize.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnMinimize.FlatAppearance.BorderSize = 0;
		this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, 80, 100);
		this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnMinimize.ForeColor = System.Drawing.Color.White;
		this.btnMinimize.Location = new System.Drawing.Point(900, 0);
		this.btnMinimize.Name = "btnMinimize";
		this.btnMinimize.Size = new System.Drawing.Size(40, 40);
		this.btnMinimize.TabIndex = 2;
		this.btnMinimize.Text = "_";
		this.btnMinimize.UseVisualStyleBackColor = true;
		this.btnMinimize.Click += new System.EventHandler(BtnMinimize_Click);
		this.lblSubtitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
		this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(180, 180, 220);
		this.lblSubtitle.Location = new System.Drawing.Point(20, 25);
		this.lblSubtitle.Name = "lblSubtitle";
		this.lblSubtitle.Size = new System.Drawing.Size(200, 20);
		this.lblSubtitle.TabIndex = 1;
		this.lblSubtitle.Text = "Sunucu Kontrol Paneli";
		this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblTitle.ForeColor = System.Drawing.Color.White;
		this.lblTitle.Location = new System.Drawing.Point(20, 0);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(300, 30);
		this.lblTitle.TabIndex = 0;
		this.lblTitle.Text = "SNAP NET SERVER";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(45, 45, 55);
		base.ClientSize = new System.Drawing.Size(1000, 700);
		base.Controls.Add(this.mainContainer);
		this.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.ForeColor = System.Drawing.Color.White;
		this.MinimumSize = new System.Drawing.Size(1000, 700);
		base.Name = "ServerForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "SnapNet Server Pro v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
		this.mainContainer.ResumeLayout(false);
		this.statusPanel.ResumeLayout(false);
		this.mainSplitContainer.Panel1.ResumeLayout(false);
		this.mainSplitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.mainSplitContainer).EndInit();
		this.mainSplitContainer.ResumeLayout(false);
		this.commandPanel.ResumeLayout(false);
		this.controlPanel.ResumeLayout(false);
		this.controlPanel.PerformLayout();
		this.titlePanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	private void BtnMinimize_Click(object sender, EventArgs e)
	{
		base.WindowState = FormWindowState.Minimized;
	}

	private void BtnClose_Click(object sender, EventArgs e)
	{
		Close();
	}
}
