using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

	private DateTime? _lastErrorTime;

	private string _lastErrorMessage;

	private static readonly TimeSpan LastErrorAutoClearAfter = TimeSpan.FromMinutes(5.0);

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

	private Panel clientsHeader;

	private Panel commandPanel;



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

	private Label lblVersion;

	private Panel cardStatus;

	private Panel cardClients;

	private Panel cardUptime;

	private Label lblClientsEmpty;

	private Label lblLicenseStatus;

	private System.Threading.Timer _licenseStatusTimer;

	private Button btnPartyForm;

	private Button btnCheckForUpdate;

	private bool _isCheckingForUpdate;

	private PartyForm _partyForm;

	public ServerForm()
	{
		InitializeComponent();
		InitializeServerEvents();
		InitializeCommands();
		SetupModernUI();
		AddTitleBarIcon();
		InitializeExtendedStatus();
		StartLogTimer();
		UpdateSilentModeButton();
		UpdateUI();
		if (!_alarm.IsSoundLoaded)
		{
			_logQueue.Enqueue("UYARI: alarm.wav bulunamadı veya yüklenemedi - envanter/üst dolu alarmı sesli çalmayacak.");
		}
		LicenseCore.LicenseGate.StartPeriodicRecheck(delegate
		{
			SafeInvoke(async delegate
			{
				if (_isRunning)
				{
					await _server.StopAsync();
					_isRunning = false;
					_uptime.Stop();
					UpdateUI();
				}
				MessageBox.Show("Lisansınızın süresi doldu. Devam etmek için yeni bir lisans anahtarı girin.", "Lisans Süresi Doldu", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				if (!LicenseCore.LicenseGate.EnsureLicensed("EVOX.Service"))
				{
					Close();
				}
				UpdateLicenseStatusLabel();
			});
		});
		_licenseStatusTimer = new System.Threading.Timer(delegate
		{
			SafeInvoke(UpdateLicenseStatusLabel);
		}, null, 30000, 30000);
		try
		{
			Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		}
		catch
		{
		}
	}

	private void AddTitleBarIcon()
	{
		try
		{
			using Icon appIcon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
			PictureBox iconBox = new PictureBox
			{
				Image = appIcon.ToBitmap(),
				SizeMode = PictureBoxSizeMode.Zoom,
				Size = new Size(34, 34),
				Location = new Point(16, 8),
				BackColor = Color.Transparent
			};
			titlePanel.Controls.Add(iconBox);
			iconBox.BringToFront();
			lblTitle.Location = new Point(60, lblTitle.Location.Y);
			// Two px further in than the wordmark so the letter-spaced subtitle reads as aligned
			// with it rather than sitting a hair to its left.
			lblSubtitle.Location = new Point(62, lblSubtitle.Location.Y);
		}
		catch
		{
		}
	}

	private void InitializeExtendedStatus()
	{
		statusPanel.Height = 78;

		lblUptime = new Label
		{
			Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold),
			ForeColor = Color.FromArgb(235, 235, 240),
			Location = new Point(12, 18),
			Size = new Size(166, 24),
			TextAlign = ContentAlignment.MiddleLeft,
			Text = "00:00:00"
		};
		cardUptime.Controls.Add(lblUptime);

		lblJobBreakdown = new Label
		{
			Font = new Font("Segoe UI", 9f),
			ForeColor = Color.FromArgb(180, 185, 195),
			Location = new Point(5, 5),
			Size = new Size(965, 22),
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			Text = "(bağlı cihaz yok)"
		};
		lblLastError = new Label
		{
			Font = new Font("Segoe UI", 8.5f),
			ForeColor = Color.FromArgb(220, 120, 120),
			Location = new Point(5, 27),
			Size = new Size(830, 22),
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			Text = ""
		};
		lblVersion = new Label
		{
			Font = new Font("Segoe UI", 7.5f),
			ForeColor = Color.FromArgb(120, 125, 140),
			Location = new Point(840, 32),
			Size = new Size(135, 16),
			TextAlign = ContentAlignment.MiddleRight,
			Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
		};
		lblLicenseStatus = new Label
		{
			Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
			ForeColor = LicenseCore.LicenseGate.GetStatusColor(),
			Location = new Point(5, 50),
			Size = new Size(820, 20),
			TextAlign = ContentAlignment.MiddleLeft,
			Text = LicenseCore.LicenseGate.GetStatusText()
		};
		btnCheckForUpdate = new Button
		{
			Font = new Font("Segoe UI", 8f, FontStyle.Bold),
			BackColor = Color.FromArgb(55, 78, 92),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(830, 47),
			Size = new Size(140, 26),
			Text = "Güncellemeyi Kontrol Et"
		};
		btnCheckForUpdate.FlatAppearance.BorderSize = 0;
		btnCheckForUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 98, 112);
		btnCheckForUpdate.Click += BtnCheckForUpdate_Click;
		statusPanel.Controls.Add(lblJobBreakdown);
		statusPanel.Controls.Add(lblLastError);
		statusPanel.Controls.Add(lblLicenseStatus);
		statusPanel.Controls.Add(btnCheckForUpdate);
		statusPanel.Controls.Add(lblVersion);
	}

	private void UpdateLicenseStatusLabel()
	{
		lblLicenseStatus.Text = LicenseCore.LicenseGate.GetStatusText();
		lblLicenseStatus.ForeColor = LicenseCore.LicenseGate.GetStatusColor();
	}

	private void StartLogTimer()
	{
		_logTimer = new System.Threading.Timer(delegate
		{
			UpdateLogs();
			UpdateUptimeLabel();
			UpdateLastErrorLabel();
		}, null, 100, 100);
	}

	private void UpdateLastErrorLabel()
	{
		if (_lastErrorTime == null)
		{
			return;
		}
		TimeSpan elapsed = DateTime.Now - _lastErrorTime.Value;
		if (elapsed >= LastErrorAutoClearAfter)
		{
			SafeInvoke(delegate
			{
				lblLastError.Text = "Son hata: yok";
			});
			_lastErrorTime = null;
			_lastErrorMessage = null;
			return;
		}
		string ago = (elapsed.TotalSeconds < 60.0) ? $"{(int)elapsed.TotalSeconds} sn önce" : $"{(int)elapsed.TotalMinutes} dk önce";
		SafeInvoke(delegate
		{
			lblLastError.Text = $"Son hata ({ago}): {_lastErrorMessage}";
		});
	}

	private void UpdateUptimeLabel()
	{
		TimeSpan elapsed = _uptime.Elapsed;
		SafeInvoke(delegate
		{
			lblUptime.Text = elapsed.ToString("hh\\:mm\\:ss");
		});
	}

	private void SetupModernUI()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		base.Padding = new Padding(1);
		BackColor = Color.FromArgb(28, 28, 33);
		btnStart.FlatStyle = FlatStyle.Flat;
		btnStop.FlatStyle = FlatStyle.Flat;
		btnStopAlarm.FlatStyle = FlatStyle.Flat;
		btnClearLogs.FlatStyle = FlatStyle.Flat;
		btnCopyLogs.FlatStyle = FlatStyle.Flat;
		btnSilentMode.FlatStyle = FlatStyle.Flat;
		// Both sit inside the logs header band, which is a Dock=Top label added BEFORE them -
		// in WinForms that puts the label in front and hides them entirely.
		btnCopyLogs.BringToFront();
		btnClearLogs.BringToFront();
		mainContainer.Paint += delegate(object? s, PaintEventArgs e)
		{
			ControlPaint.DrawBorder(e.Graphics, mainContainer.ClientRectangle, Color.FromArgb(70, 80, 100), ButtonBorderStyle.Solid);
		};
	}

	// Each command is its own button now: the old list + "Seçili Komutları Gönder" meant two
	// interactions (select, then send) for something that is always a single action, and the
	// list hid everything past the first few rows behind a scrollbar.
	private static readonly (string Label, string Code)[] CommandButtons = new (string, string)[7]
	{
		("⚔   Warrior Genie Aç", "WARRIOR_GENIE"),
		("✚   Priest Genie Aç", "PRIEST_GENIE"),
		("🔁   ReReRe", "201"),
		("🎡   Çark Çevir", "501"),
		("🔕   Tüm Alarmları Durdur", "101"),
		("⬇   Konsolları Güncelle", "301"),
		("🧪   Özel Komut", "999")
	};

	private void InitializeCommands()
	{
		const int columns = 4;
		const int gapX = 8;
		const int gapY = 8;
		const int buttonHeight = 38;
		int buttonWidth = (commandPanel.Width - (columns - 1) * gapX) / columns;
		for (int i = 0; i < CommandButtons.Length; i++)
		{
			var (label, code) = CommandButtons[i];
			Button button = new Button
			{
				Text = label,
				Tag = code,
				Location = new Point(i % columns * (buttonWidth + gapX), i / columns * (buttonHeight + gapY)),
				Size = new Size(buttonWidth, buttonHeight),
				BackColor = Color.FromArgb(56, 56, 63),
				ForeColor = Color.FromArgb(235, 235, 240),
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f, FontStyle.Regular),
				TextAlign = ContentAlignment.MiddleLeft,
				Padding = new Padding(10, 0, 0, 0),
				UseVisualStyleBackColor = false,
				AutoEllipsis = true,
				Cursor = Cursors.Hand
			};
			button.FlatAppearance.BorderSize = 1;
			button.FlatAppearance.BorderColor = Color.FromArgb(68, 68, 76);
			button.FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 94, 108);
			button.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 60, 72);
			button.Click += CommandButton_Click;
			commandPanel.Controls.Add(button);
		}
	}

	private async void CommandButton_Click(object sender, EventArgs e)
	{
		if (sender is not Button { Tag: string code })
		{
			return;
		}
		try
		{
			switch (code)
			{
				case "WARRIOR_GENIE":
					await _server.SendCommandToSpecificJobAsync("401", Server.JobType.Warrior);
					_logQueue.Enqueue("401 komutu sadece Warrior'lara gönderildi");
					break;
				case "PRIEST_GENIE":
					await _server.SendCommandToSpecificJobAsync("401", Server.JobType.Priest);
					_logQueue.Enqueue("401 komutu sadece Priest'lere gönderildi");
					break;
				default:
					await _server.SendCommandToAllClientsAsync(code, withDelay: false);
					_logQueue.Enqueue("KOMUT_GONDERILDI:" + code);
					break;
			}
		}
		catch (Exception ex)
		{
			HandleError("Komut gönderim hatası: " + ex.Message);
		}
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
		_server.ClientStatusUpdated += delegate
		{
			SafeInvoke(UpdateClientList);
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
		if (!LicenseCore.LicenseGate.IsCurrentlyValid())
		{
			MessageBox.Show("Lisansınızın süresi doldu. Sunucu başlatılamıyor.", "Lisans Süresi Doldu", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
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
			_partyForm?.ResetSelections();
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			HandleError("Durdurma hatası: " + ex2.Message);
		}
	}

	private void BtnPartyForm_Click(object sender, EventArgs e)
	{
		if (_partyForm == null || _partyForm.IsDisposed)
		{
			_partyForm = new PartyForm(_server);
		}
		if (_partyForm.Visible)
		{
			_partyForm.Hide();
			return;
		}
		_partyForm.Show(this);
		_partyForm.BringToFront();
	}

	private async void BtnCheckForUpdate_Click(object sender, EventArgs e)
	{
		if (_isCheckingForUpdate)
		{
			return;
		}
		_isCheckingForUpdate = true;
		string originalText = btnCheckForUpdate.Text;
		btnCheckForUpdate.Enabled = false;
		btnCheckForUpdate.Text = "Kontrol ediliyor...";
		try
		{
			UpdateInfo update = await UpdateChecker.CheckForUpdateAsync();
			if (update == null)
			{
				MessageBox.Show(this, "Güncelleme bulunamadı. En güncel sürümü kullanıyorsunuz.", "Güncelleme Yok", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			string message = $"Yeni sürüm bulundu: v{update.Version} (mevcut: v{UpdateChecker.CurrentVersion})\n\nŞimdi güncellensin mi?";
			if (MessageBox.Show(this, message, "Güncelleme Mevcut", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
			{
				return;
			}
			if (string.IsNullOrEmpty(update.DownloadUrl))
			{
				MessageBox.Show(this, "Güncelleme dosyası bulunamadı. Lütfen manuel indirin: " + update.HtmlUrl, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			btnCheckForUpdate.Text = "İndiriliyor...";
			string zipPath = await UpdateChecker.DownloadUpdateAsync(update.DownloadUrl);
			try
			{
				string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EVOX.Updater.exe");
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = fileName,
					Arguments = "\"" + zipPath + "\" \"EVOX.Service.exe\"",
					UseShellExecute = true,
					Verb = "runas"
				};
				Process.Start(startInfo);
				Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Updater başlatılamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		catch (Exception ex2)
		{
			MessageBox.Show(this, "Güncelleme kontrol hatası: " + ex2.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			_isCheckingForUpdate = false;
			btnCheckForUpdate.Enabled = true;
			btnCheckForUpdate.Text = originalText;
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
		var clientList = _server.GetClientList()
			.OrderBy((c) => c.job, StringComparer.OrdinalIgnoreCase)
			.ThenBy((c) => c.nickname, StringComparer.OrdinalIgnoreCase)
			.ToList();
		ClientListItem[] clients = (from c in clientList
			select new ClientListItem
			{
				Text = $"{c.nickname} | {c.job} | {c.endpoint}",
				Nickname = c.nickname,
				Job = c.job,
				VerificationOk = c.lastVerificationOk,
				VerificationTime = c.lastVerificationTime,
				EmptySlots = c.emptySlots,
				EmptySlotsTime = c.emptySlotsTime,
				InventoryClosed = c.inventoryClosed,
				GenieActive = c.genieActive,
				MacroActive = c.macroActive,
				RepairOk = c.repairOk,
				PingRttMs = c.pingRttMs
			}).ToArray();
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
			lblClientsEmpty.Visible = !clients.Any();
			lblJobBreakdown.Text = string.IsNullOrEmpty(breakdown) ? "(bağlı cihaz yok)" : breakdown;
		});
	}

	// Column layout for the connected-devices table, shared by the header strip and every row so
	// the two can never drift apart. X is the left edge inside the list; Width is what the text
	// or pill is allowed to occupy.
	private static readonly (string Title, int X, int Width)[] ClientColumns = new (string, int, int)[8]
	{
		("", 12, 12),
		("NICKNAME", 32, 180),
		("JOB", 218, 80),
		("GENIE", 304, 90),
		("ENVANTER", 400, 95),
		("MAKRO", 501, 100),
		("TAMIR", 607, 90),
		("PING", 703, 70)
	};

	private class ClientListItem
	{
		public string Text { get; set; }

		public string Nickname { get; set; }

		public string Job { get; set; }

		public bool? VerificationOk { get; set; }

		public DateTime? VerificationTime { get; set; }

		public int? EmptySlots { get; set; }

		public DateTime? EmptySlotsTime { get; set; }

		public bool InventoryClosed { get; set; }

		public bool? GenieActive { get; set; }

		public bool? MacroActive { get; set; }

		public bool? RepairOk { get; set; }

		public int? PingRttMs { get; set; }
	}

	private void LstClients_DrawItem(object sender, DrawItemEventArgs e)
	{
		e.DrawBackground();
		if (e.Index < 0 || e.Index >= lstClients.Items.Count)
		{
			return;
		}
		if (lstClients.Items[e.Index] is not ClientListItem item)
		{
			return;
		}
		e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		// Hairline between rows, so the columns read as a table rather than a run-together list.
		using (Pen separator = new Pen(Color.FromArgb(52, 52, 60)))
		{
			e.Graphics.DrawLine(separator, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
		}
		int centreY = e.Bounds.Top + e.Bounds.Height / 2;
		// Connection dot: green when the last visual verification passed, amber when it failed,
		// grey while the client has not reported one yet.
		Color dotColor = !item.VerificationOk.HasValue
			? Color.FromArgb(108, 108, 118)
			: (item.VerificationOk.Value ? Color.FromArgb(46, 153, 102) : Color.FromArgb(212, 163, 57));
		using (SolidBrush dotBrush = new SolidBrush(dotColor))
		{
			e.Graphics.FillEllipse(dotBrush, ColumnLeft(e, 0), centreY - 4, 8, 8);
		}
		using (Font rowFont = new Font(e.Font.FontFamily, 8.5f))
		using (Font nameFont = new Font(e.Font.FontFamily, 8.5f, FontStyle.Bold))
		{
			DrawColumnText(e, 1, item.Nickname ?? item.Text, nameFont, Color.FromArgb(145, 198, 220), centreY);
			DrawColumnText(e, 2, item.Job ?? string.Empty, rowFont, Color.FromArgb(214, 214, 222), centreY);

			(string Text, PillTone Tone) genie = !item.GenieActive.HasValue
				? ("—", PillTone.Idle)
				: (item.GenieActive.Value ? ("Açık", PillTone.Good) : ("Kapalı", PillTone.Warn));
			DrawPill(e, 3, genie.Text, genie.Tone, rowFont, centreY);

			(string Text, PillTone Tone) inventory = DescribeInventory(item);
			DrawPill(e, 4, inventory.Text, inventory.Tone, rowFont, centreY);

			(string Text, PillTone Tone) macro = !item.MacroActive.HasValue
				? ("—", PillTone.Idle)
				: (item.MacroActive.Value ? ("Çalışıyor", PillTone.Good) : ("Durdu", PillTone.Warn));
			DrawPill(e, 5, macro.Text, macro.Tone, rowFont, centreY);

			(string Text, PillTone Tone) repair = !item.RepairOk.HasValue
				? ("—", PillTone.Idle)
				: (item.RepairOk.Value ? ("OK", PillTone.Good) : ("HATA", PillTone.Bad));
			DrawPill(e, 6, repair.Text, repair.Tone, rowFont, centreY);

			DrawColumnText(e, 7, item.PingRttMs.HasValue ? item.PingRttMs.Value + "ms" : "—", rowFont, Color.FromArgb(150, 155, 168), centreY);
		}
		e.DrawFocusRectangle();
	}

	private static (string Text, PillTone Tone) DescribeInventory(ClientListItem item)
	{
		if (item.InventoryClosed)
		{
			return ("Kapalı", PillTone.Idle);
		}
		if (!item.EmptySlots.HasValue)
		{
			return ("—", PillTone.Idle);
		}
		int slots = item.EmptySlots.Value;
		PillTone tone = (slots == 0) ? PillTone.Bad : ((slots <= 5) ? PillTone.Warn : PillTone.Good);
		return (slots + " boş", tone);
	}

	private enum PillTone
	{
		Idle,
		Good,
		Warn,
		Bad
	}

	private static int ColumnLeft(DrawItemEventArgs e, int column)
	{
		return e.Bounds.Left + ClientColumns[column].X;
	}

	private static void DrawColumnText(DrawItemEventArgs e, int column, string text, Font font, Color color, int centreY)
	{
		using SolidBrush brush = new SolidBrush(color);
		using StringFormat format = new StringFormat
		{
			Trimming = StringTrimming.EllipsisCharacter,
			FormatFlags = StringFormatFlags.NoWrap,
			LineAlignment = StringAlignment.Center
		};
		Rectangle cell = new Rectangle(ColumnLeft(e, column), centreY - 9, ClientColumns[column].Width, 18);
		e.Graphics.DrawString(text, font, brush, cell, format);
	}

	/// <summary>Draws a status value as a tinted rounded chip, so a glance down the column shows
	/// which characters need attention without having to read any of the text.</summary>
	private static void DrawPill(DrawItemEventArgs e, int column, string text, PillTone tone, Font font, int centreY)
	{
		(Color Background, Color Foreground) colors = tone switch
		{
			PillTone.Good => (Color.FromArgb(28, 62, 48), Color.FromArgb(127, 217, 168)),
			PillTone.Warn => (Color.FromArgb(68, 55, 28), Color.FromArgb(232, 193, 100)),
			PillTone.Bad => (Color.FromArgb(74, 34, 34), Color.FromArgb(232, 130, 130)),
			_ => (Color.FromArgb(50, 50, 58), Color.FromArgb(130, 130, 142)),
		};
		Rectangle pill = new Rectangle(ColumnLeft(e, column), centreY - 9, ClientColumns[column].Width - 6, 18);
		using (SolidBrush background = new SolidBrush(colors.Background))
		using (System.Drawing.Drawing2D.GraphicsPath path = RoundedRectangle(pill, 9))
		{
			e.Graphics.FillPath(background, path);
		}
		using SolidBrush foreground = new SolidBrush(colors.Foreground);
		using StringFormat format = new StringFormat
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center,
			Trimming = StringTrimming.EllipsisCharacter,
			FormatFlags = StringFormatFlags.NoWrap
		};
		e.Graphics.DrawString(text, font, foreground, pill, format);
	}

	private static System.Drawing.Drawing2D.GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
	{
		int diameter = radius * 2;
		System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
		path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180f, 90f);
		path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270f, 90f);
		path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
		path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
		path.CloseFigure();
		return path;
	}

	/// <summary>Paints the column titles above the list from the same offsets the rows use, so a
	/// header can never end up sitting over the wrong column.</summary>
	private void ClientsHeader_Paint(object sender, PaintEventArgs e)
	{
		Panel panel = (Panel)sender;
		using (Pen underline = new Pen(Color.FromArgb(58, 58, 66)))
		{
			e.Graphics.DrawLine(underline, 0, panel.Height - 1, panel.Width, panel.Height - 1);
		}
		using Font font = new Font("Segoe UI", 7f, FontStyle.Bold);
		using SolidBrush brush = new SolidBrush(Color.FromArgb(118, 118, 130));
		using StringFormat format = new StringFormat
		{
			LineAlignment = StringAlignment.Center,
			FormatFlags = StringFormatFlags.NoWrap
		};
		foreach (var (title, x, width) in ClientColumns)
		{
			if (title.Length == 0)
			{
				continue;
			}
			e.Graphics.DrawString(title, font, brush, new Rectangle(x, 0, width, panel.Height), format);
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
			btnSilentMode.Text = "🔇  Sessiz Mod: AÇIK";
			btnSilentMode.BackColor = Color.FromArgb(46, 120, 84);
			btnSilentMode.FlatAppearance.BorderColor = Color.FromArgb(60, 150, 105);
		}
		else
		{
			btnSilentMode.Text = "🔇  Sessiz Mod";
			btnSilentMode.BackColor = Color.FromArgb(56, 56, 63);
			btnSilentMode.FlatAppearance.BorderColor = Color.FromArgb(68, 68, 76);
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

	private static void DrawCardBorder(object sender, PaintEventArgs e)
	{
		Control control = (Control)sender;
		using System.Drawing.Drawing2D.GraphicsPath path = RoundedCardPath(control.Width, control.Height, 6);
		control.Region = new Region(path);
		e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		using Pen pen = new Pen(Color.FromArgb(60, 64, 76));
		e.Graphics.DrawPath(pen, path);
	}

	private static System.Drawing.Drawing2D.GraphicsPath RoundedCardPath(int width, int height, int radius)
	{
		System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
		int d = radius * 2;
		Rectangle rect = new Rectangle(0, 0, width - 1, height - 1);
		path.AddArc(rect.X, rect.Y, d, d, 180, 90);
		path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
		path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
		path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
		path.CloseFigure();
		return path;
	}

	private static readonly Color DisabledButtonBack = Color.FromArgb(60, 60, 65);

	private static readonly Color DisabledButtonFore = Color.FromArgb(140, 140, 145);

	private void UpdateUI()
	{
		SafeInvoke(delegate
		{
			btnStart.Enabled = !_isRunning;
			btnStart.BackColor = _isRunning ? DisabledButtonBack : Color.FromArgb(80, 180, 80);
			btnStart.ForeColor = _isRunning ? DisabledButtonFore : Color.White;
			btnStop.Enabled = _isRunning;
			btnStop.BackColor = _isRunning ? Color.FromArgb(200, 80, 80) : DisabledButtonBack;
			btnStop.ForeColor = _isRunning ? Color.White : DisabledButtonFore;
			txtPort.Enabled = !_isRunning;
			txtPort.BackColor = _isRunning ? Color.FromArgb(26, 26, 31) : Color.FromArgb(33, 33, 40);
			txtPort.ForeColor = _isRunning ? DisabledButtonFore : Color.White;
			lblServerStatus.Text = (_isRunning ? "● ÇALIŞIYOR" : "● DURDURULDU");
			lblServerStatus.ForeColor = (_isRunning ? Color.FromArgb(110, 200, 110) : Color.FromArgb(210, 100, 100));
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
		_lastErrorTime = DateTime.Now;
		_lastErrorMessage = message;
		SafeInvoke(delegate
		{
			lblLastError.Text = $"Son hata (az önce): {message}";
			if (_currentToast != null && _currentToast.IsUsable)
			{
				_currentToast.UpdateMessage(message);
				return;
			}
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
		_licenseStatusTimer?.Dispose();
		_alarm?.Dispose();
		_partyForm?.Close();
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
		this.cardStatus = new System.Windows.Forms.Panel();
		this.cardClients = new System.Windows.Forms.Panel();
		this.cardUptime = new System.Windows.Forms.Panel();
		this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
		this.lstClients = new System.Windows.Forms.ListBox();
		this.lblClients = new System.Windows.Forms.Label();
		this.lstLogs = new System.Windows.Forms.ListBox();
		this.lblLogs = new System.Windows.Forms.Label();
		this.btnCopyLogs = new System.Windows.Forms.Button();
		this.commandPanel = new System.Windows.Forms.Panel();
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
		this.btnPartyForm = new System.Windows.Forms.Button();
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
		this.mainContainer.BackColor = System.Drawing.Color.FromArgb(28, 28, 33);
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
		this.statusPanel.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.statusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.statusPanel.Location = new System.Drawing.Point(10, 650);
		this.statusPanel.Name = "statusPanel";
		this.statusPanel.Padding = new System.Windows.Forms.Padding(5);
		this.statusPanel.Size = new System.Drawing.Size(980, 40);
		this.statusPanel.TabIndex = 9;
		this.lblClientCount.Font = new System.Drawing.Font("Segoe UI Semibold", 13f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblClientCount.ForeColor = System.Drawing.Color.FromArgb(120, 210, 230);
		this.lblClientCount.Location = new System.Drawing.Point(12, 18);
		this.lblClientCount.Name = "lblClientCount";
		this.lblClientCount.Size = new System.Drawing.Size(126, 24);
		this.lblClientCount.TabIndex = 7;
		this.lblClientCount.Text = "0 Bağlı Cihaz";
		this.lblClientCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lblServerStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblServerStatus.ForeColor = System.Drawing.Color.FromArgb(200, 80, 80);
		this.lblServerStatus.Location = new System.Drawing.Point(12, 18);
		this.lblServerStatus.Name = "lblServerStatus";
		this.lblServerStatus.Size = new System.Drawing.Size(100, 24);
		this.lblServerStatus.TabIndex = 6;
		this.lblServerStatus.Text = "DURDURULDU";
		this.lblServerStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cardStatus.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.cardStatus.Location = new System.Drawing.Point(150, 0);
		this.cardStatus.Name = "cardStatus";
		this.cardStatus.Size = new System.Drawing.Size(120, 45);
		this.cardStatus.TabIndex = 20;
		this.cardStatus.Controls.Add(this.lblServerStatus);
		this.cardStatus.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "DURUM",
			Font = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(130, 135, 150),
			Location = new System.Drawing.Point(12, 4),
			AutoSize = true
		});
		this.cardStatus.Paint += DrawCardBorder;
		this.cardClients.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.cardClients.Location = new System.Drawing.Point(280, 0);
		this.cardClients.Name = "cardClients";
		this.cardClients.Size = new System.Drawing.Size(150, 45);
		this.cardClients.TabIndex = 21;
		this.cardClients.Controls.Add(this.lblClientCount);
		this.cardClients.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "BAĞLI CİHAZ",
			Font = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(130, 135, 150),
			Location = new System.Drawing.Point(12, 4),
			AutoSize = true
		});
		this.cardClients.Paint += DrawCardBorder;
		this.cardUptime.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.cardUptime.Location = new System.Drawing.Point(440, 0);
		this.cardUptime.Name = "cardUptime";
		this.cardUptime.Size = new System.Drawing.Size(190, 45);
		this.cardUptime.TabIndex = 22;
		this.cardUptime.Controls.Add(new System.Windows.Forms.Label
		{
			Text = "ÇALIŞMA SÜRESİ",
			Font = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(130, 135, 150),
			Location = new System.Drawing.Point(12, 4),
			AutoSize = true
		});
		this.cardUptime.Paint += DrawCardBorder;
		this.mainSplitContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.mainSplitContainer.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.mainSplitContainer.Location = new System.Drawing.Point(10, 280);
		this.mainSplitContainer.Name = "mainSplitContainer";
		this.mainSplitContainer.Panel1.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.lblClientsEmpty = new System.Windows.Forms.Label
		{
			Text = "Henüz bağlı cihaz yok",
			Dock = System.Windows.Forms.DockStyle.Fill,
			TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
			ForeColor = System.Drawing.Color.FromArgb(90, 95, 110),
			Font = new System.Drawing.Font("Segoe UI", 10f),
			BackColor = System.Drawing.Color.FromArgb(33, 33, 40)
		};
		this.clientsHeader = new System.Windows.Forms.Panel
		{
			Dock = System.Windows.Forms.DockStyle.Top,
			Height = 22,
			BackColor = System.Drawing.Color.FromArgb(44, 44, 52)
		};
		this.clientsHeader.Paint += new System.Windows.Forms.PaintEventHandler(ClientsHeader_Paint);
		// Add order sets z-order, and docking runs from the highest index down: lblClients takes
		// the top strip, then this header sits directly under it, then the list fills the rest.
		this.mainSplitContainer.Panel1.Controls.Add(this.lblClientsEmpty);
		this.mainSplitContainer.Panel1.Controls.Add(this.lstClients);
		this.mainSplitContainer.Panel1.Controls.Add(this.clientsHeader);
		this.mainSplitContainer.Panel1.Controls.Add(this.lblClients);
		this.lblClientsEmpty.BringToFront();
		this.mainSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
		this.mainSplitContainer.Panel2.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.mainSplitContainer.Panel2.Controls.Add(this.lstLogs);
		this.mainSplitContainer.Panel2.Controls.Add(this.lblLogs);
		this.mainSplitContainer.Panel2.Controls.Add(this.btnCopyLogs);
		this.mainSplitContainer.Panel2.Controls.Add(this.btnClearLogs);
		this.mainSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
		this.mainSplitContainer.Size = new System.Drawing.Size(980, 322);
		// Stacked rather than side by side: the device table is the thing being watched and now
		// gets the full width for its columns, while the log - a "what just happened" summary,
		// with the full history living in its own window - is a short strip underneath.
		this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.mainSplitContainer.SplitterDistance = 208;
		this.mainSplitContainer.SplitterWidth = 8;
		this.mainSplitContainer.TabIndex = 8;
		this.lstClients.BackColor = System.Drawing.Color.FromArgb(33, 33, 40);
		this.lstClients.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstClients.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lstClients.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.lstClients.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.lstClients.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lstClients.FormattingEnabled = true;
		this.lstClients.ItemHeight = 30;
		this.lstClients.Location = new System.Drawing.Point(0, 25);
		this.lstClients.Name = "lstClients";
		this.lstClients.Size = new System.Drawing.Size(345, 335);
		this.lstClients.TabIndex = 3;
		this.lstClients.DrawItem += new System.Windows.Forms.DrawItemEventHandler(LstClients_DrawItem);
		this.lblClients.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
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
		this.lstLogs.BackColor = System.Drawing.Color.FromArgb(33, 33, 40);
		this.lstLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstLogs.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lstLogs.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.lstLogs.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lstLogs.FormattingEnabled = true;
		this.lstLogs.ItemHeight = 15;
		this.lstLogs.Location = new System.Drawing.Point(5, 25);
		this.lstLogs.Name = "lstLogs";
		this.lstLogs.Size = new System.Drawing.Size(615, 335);
		this.lstLogs.TabIndex = 5;
		this.lblLogs.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
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
		this.btnCopyLogs.BackColor = System.Drawing.Color.FromArgb(56, 56, 63);
		this.btnCopyLogs.FlatAppearance.BorderSize = 1;
		this.btnCopyLogs.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(68, 68, 76);
		this.btnCopyLogs.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(58, 94, 108);
		this.btnCopyLogs.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(40, 60, 72);
		this.btnCopyLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCopyLogs.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.btnCopyLogs.ForeColor = System.Drawing.Color.FromArgb(200, 200, 208);
		this.btnCopyLogs.Cursor = System.Windows.Forms.Cursors.Hand;
		this.btnCopyLogs.Location = new System.Drawing.Point(485, 0);
		this.btnCopyLogs.Name = "btnCopyLogs";
		this.btnCopyLogs.Size = new System.Drawing.Size(130, 25);
		this.btnCopyLogs.TabIndex = 6;
		this.btnCopyLogs.Text = "📋  Logları Kopyala";
		this.btnCopyLogs.UseVisualStyleBackColor = false;
		this.btnCopyLogs.Click += new System.EventHandler(BtnCopyLogs_Click);
		this.commandPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.commandPanel.BackColor = System.Drawing.Color.Transparent;
		this.commandPanel.Location = new System.Drawing.Point(10, 180);
		this.commandPanel.Name = "commandPanel";
		this.commandPanel.Size = new System.Drawing.Size(980, 90);
		this.commandPanel.TabIndex = 7;
		this.controlPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.controlPanel.BackColor = System.Drawing.Color.Transparent;
		this.controlPanel.Controls.Add(this.btnSilentMode);
		this.controlPanel.Controls.Add(this.btnStopAlarm);
		this.controlPanel.Controls.Add(this.txtPort);
		this.controlPanel.Controls.Add(this.lblPort);
		this.controlPanel.Controls.Add(this.btnStop);
		this.controlPanel.Controls.Add(this.btnStart);
		this.controlPanel.Controls.Add(this.cardStatus);
		this.controlPanel.Controls.Add(this.cardClients);
		this.controlPanel.Controls.Add(this.cardUptime);
		this.controlPanel.Location = new System.Drawing.Point(10, 70);
		this.controlPanel.Name = "controlPanel";
		this.controlPanel.Size = new System.Drawing.Size(980, 100);
		this.controlPanel.TabIndex = 6;
		this.btnSilentMode.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnSilentMode.BackColor = System.Drawing.Color.FromArgb(56, 56, 63);
		this.btnSilentMode.FlatAppearance.BorderSize = 1;
		this.btnSilentMode.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(68, 68, 76);
		this.btnSilentMode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(58, 94, 108);
		this.btnSilentMode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(40, 60, 72);
		this.btnSilentMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSilentMode.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.btnSilentMode.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.btnSilentMode.Cursor = System.Windows.Forms.Cursors.Hand;
		this.btnSilentMode.Location = new System.Drawing.Point(720, 58);
		this.btnSilentMode.Name = "btnSilentMode";
		this.btnSilentMode.Size = new System.Drawing.Size(130, 26);
		this.btnSilentMode.TabIndex = 8;
		this.btnSilentMode.Text = "🔇  Sessiz Mod";
		this.btnSilentMode.UseVisualStyleBackColor = false;
		this.btnSilentMode.Click += new System.EventHandler(BtnSilentMode_Click);
		this.btnStopAlarm.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnStopAlarm.BackColor = System.Drawing.Color.FromArgb(143, 48, 48);
		this.btnStopAlarm.FlatAppearance.BorderSize = 1;
		this.btnStopAlarm.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(178, 62, 62);
		this.btnStopAlarm.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(200, 70, 70);
		this.btnStopAlarm.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(120, 40, 40);
		this.btnStopAlarm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnStopAlarm.Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.btnStopAlarm.ForeColor = System.Drawing.Color.FromArgb(245, 230, 230);
		this.btnStopAlarm.Cursor = System.Windows.Forms.Cursors.Hand;
		this.btnStopAlarm.Location = new System.Drawing.Point(858, 58);
		this.btnStopAlarm.Name = "btnStopAlarm";
		this.btnStopAlarm.Size = new System.Drawing.Size(122, 26);
		this.btnStopAlarm.TabIndex = 7;
		this.btnStopAlarm.Text = "🔔  Alarmı Durdur";
		this.btnStopAlarm.UseVisualStyleBackColor = false;
		this.btnStopAlarm.Click += new System.EventHandler(BtnStopAlarm_Click);
		this.btnClearLogs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClearLogs.BackColor = System.Drawing.Color.FromArgb(56, 56, 63);
		this.btnClearLogs.FlatAppearance.BorderSize = 1;
		this.btnClearLogs.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(68, 68, 76);
		this.btnClearLogs.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(120, 60, 60);
		this.btnClearLogs.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(90, 45, 45);
		this.btnClearLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClearLogs.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.btnClearLogs.ForeColor = System.Drawing.Color.FromArgb(200, 200, 208);
		this.btnClearLogs.Cursor = System.Windows.Forms.Cursors.Hand;
		this.btnClearLogs.Location = new System.Drawing.Point(450, 0);
		this.btnClearLogs.Name = "btnClearLogs";
		this.btnClearLogs.Size = new System.Drawing.Size(30, 25);
		this.btnClearLogs.TabIndex = 4;
		this.btnClearLogs.Text = "🗑";
		this.btnClearLogs.UseVisualStyleBackColor = false;
		this.btnClearLogs.Click += new System.EventHandler(BtnClearLogs_Click);
		this.txtPort.BackColor = System.Drawing.Color.FromArgb(33, 33, 40);
		this.txtPort.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtPort.Font = new System.Drawing.Font("Segoe UI", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.txtPort.ForeColor = System.Drawing.Color.White;
		this.txtPort.Location = new System.Drawing.Point(20, 20);
		this.txtPort.Name = "txtPort";
		this.txtPort.Size = new System.Drawing.Size(100, 20);
		this.txtPort.TabIndex = 3;
		this.txtPort.Text = "5000";
		this.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.lblPort.AutoSize = true;
		this.lblPort.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.lblPort.ForeColor = System.Drawing.Color.FromArgb(200, 200, 205);
		this.lblPort.Location = new System.Drawing.Point(20, 4);
		this.lblPort.Name = "lblPort";
		this.lblPort.Size = new System.Drawing.Size(34, 15);
		this.lblPort.TabIndex = 2;
		this.lblPort.Text = "Port:";
		this.btnStop.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnStop.BackColor = System.Drawing.Color.FromArgb(200, 80, 80);
		this.btnStop.Enabled = false;
		this.btnStop.FlatAppearance.BorderSize = 0;
		this.btnStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 100, 100);
		this.btnStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(170, 60, 60);
		this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnStop.ForeColor = System.Drawing.Color.White;
		this.btnStop.Location = new System.Drawing.Point(680, 0);
		this.btnStop.Name = "btnStop";
		this.btnStop.Size = new System.Drawing.Size(140, 40);
		this.btnStop.TabIndex = 1;
		this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
		this.btnStop.Text = "⏹  DURDUR";
		this.btnStop.UseVisualStyleBackColor = false;
		this.btnStop.Click += new System.EventHandler(BtnStop_Click);
		this.btnStart.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnStart.BackColor = System.Drawing.Color.FromArgb(80, 180, 80);
		this.btnStart.FlatAppearance.BorderSize = 0;
		this.btnStart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(100, 200, 100);
		this.btnStart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(60, 150, 60);
		this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnStart.ForeColor = System.Drawing.Color.White;
		this.btnStart.Location = new System.Drawing.Point(840, 0);
		this.btnStart.Name = "btnStart";
		this.btnStart.Size = new System.Drawing.Size(140, 40);
		this.btnStart.TabIndex = 0;
		this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
		this.btnStart.Text = "▶  BAŞLAT";
		this.btnStart.UseVisualStyleBackColor = false;
		this.btnStart.Click += new System.EventHandler(BtnStart_Click);
		this.titlePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.titlePanel.BackColor = System.Drawing.Color.FromArgb(33, 33, 40);
		this.titlePanel.Controls.Add(this.btnClose);
		this.titlePanel.Controls.Add(this.btnMinimize);
		this.titlePanel.Controls.Add(this.btnPartyForm);
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
		this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(70, 80, 100);
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
		this.btnPartyForm.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
		this.btnPartyForm.BackColor = System.Drawing.Color.FromArgb(55, 78, 92);
		this.btnPartyForm.FlatAppearance.BorderSize = 0;
		this.btnPartyForm.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(75, 98, 112);
		this.btnPartyForm.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(40, 60, 72);
		this.btnPartyForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnPartyForm.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.btnPartyForm.ForeColor = System.Drawing.Color.White;
		this.btnPartyForm.Location = new System.Drawing.Point(650, 8);
		this.btnPartyForm.Name = "btnPartyForm";
		this.btnPartyForm.Size = new System.Drawing.Size(180, 34);
		this.btnPartyForm.TabIndex = 9;
		this.btnPartyForm.Text = "PARTİ KUR (8 KİŞİ)";
		this.btnPartyForm.UseVisualStyleBackColor = false;
		this.btnPartyForm.Click += new System.EventHandler(BtnPartyForm_Click);
		// Hidden for now - not an active feature yet. Logic (this button, BtnPartyForm_Click,
		// PartyForm.cs) is left in place untouched so it can come back with a one-line change.
		this.btnPartyForm.Visible = false;
		// Wordmark treatment: "EVOX" alone, with "SERVİS" set small and letter-spaced beneath it.
		// The spacing is written into the string because WinForms labels have no tracking of
		// their own, and a plain tight "SERVİS" under a 22pt wordmark reads as cramped.
		this.lblSubtitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(132, 140, 156);
		this.lblSubtitle.Location = new System.Drawing.Point(22, 29);
		this.lblSubtitle.Name = "lblSubtitle";
		this.lblSubtitle.Size = new System.Drawing.Size(160, 16);
		this.lblSubtitle.TabIndex = 1;
		this.lblSubtitle.Text = "S E R V İ S";
		this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
		this.lblTitle.Font = AppFonts.Header(22f, System.Drawing.FontStyle.Regular);
		this.lblTitle.ForeColor = System.Drawing.Color.White;
		this.lblTitle.Location = new System.Drawing.Point(20, 2);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(160, 30);
		this.lblTitle.TabIndex = 0;
		this.lblTitle.Text = "EVOX";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(28, 28, 33);
		base.ClientSize = new System.Drawing.Size(1000, 700);
		base.Controls.Add(this.mainContainer);
		this.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.ForeColor = System.Drawing.Color.White;
		this.MinimumSize = new System.Drawing.Size(1000, 700);
		base.Name = "ServerForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "EVOX.Service v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
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
