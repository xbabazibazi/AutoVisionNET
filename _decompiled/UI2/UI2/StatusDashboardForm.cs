using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using InputInterceptorNS;
using SettingsManager;
using UI2.ScreenCapture;

namespace UI2;

public class StatusDashboardForm : Form
{
	private readonly TableLayoutPanel _grid;

	private readonly Timer _refreshTimer;

	private readonly ScreenCaptureMainForm _screenCaptureMainForm;

	public StatusDashboardForm(ScreenCaptureMainForm screenCaptureMainForm)
	{
		_screenCaptureMainForm = screenCaptureMainForm;
		Text = "Durum";
		Size = new Size(420, 460);
		StartPosition = FormStartPosition.Manual;
		ShowInTaskbar = false;
		FormBorderStyle = FormBorderStyle.None;
		BackColor = Color.FromArgb(28, 28, 33);
		ForeColor = Color.FromArgb(235, 235, 240);
		Font = new Font("Tahoma", 8f);

		Panel headerPanel = new Panel
		{
			Dock = DockStyle.Top,
			Height = 30,
			BackColor = Color.FromArgb(38, 38, 45)
		};
		Label lblTitle = new Label
		{
			Text = "Durum",
			ForeColor = Color.FromArgb(235, 235, 240),
			Font = new Font("Segoe UI", 9f, FontStyle.Bold),
			AutoSize = true,
			Location = new Point(10, 7)
		};
		Button btnClose = new Button
		{
			Text = "✕",
			FlatStyle = FlatStyle.Flat,
			ForeColor = Color.FromArgb(235, 235, 240),
			BackColor = Color.Transparent,
			Font = new Font("Segoe UI", 9f, FontStyle.Bold),
			Size = new Size(20, 20),
			Anchor = AnchorStyles.Top | AnchorStyles.Right,
			Location = new Point(headerPanel.Width - 30, 5)
		};
		btnClose.FlatAppearance.BorderSize = 0;
		btnClose.Click += delegate { Hide(); };
		btnClose.MouseEnter += delegate { btnClose.BackColor = Color.FromArgb(90, 50, 50); };
		btnClose.MouseLeave += delegate { btnClose.BackColor = Color.Transparent; };
		headerPanel.Controls.Add(lblTitle);
		headerPanel.Controls.Add(btnClose);

		Panel bodyPanel = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(28, 28, 33),
			AutoScroll = true
		};

		_grid = new TableLayoutPanel
		{
			Dock = DockStyle.Top,
			ColumnCount = 2,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = Color.FromArgb(28, 28, 33),
			Padding = new Padding(10)
		};
		_grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
		_grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
		bodyPanel.Controls.Add(_grid);

		Button btnRefresh = new Button
		{
			Text = "Yenile",
			Dock = DockStyle.Bottom,
			Height = 32,
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(55, 78, 92),
			ForeColor = Color.White,
			UseVisualStyleBackColor = false
		};
		btnRefresh.FlatAppearance.BorderSize = 0;
		btnRefresh.Click += (s, e) => RefreshStatus();
		bodyPanel.Controls.Add(btnRefresh);

		Controls.Add(bodyPanel);
		Controls.Add(headerPanel);

		_refreshTimer = new Timer { Interval = 3000 };
		_refreshTimer.Tick += (s, e) => RefreshStatus();
		_refreshTimer.Start();

		RefreshStatus();
	}

	private void AddRow(string label, string value, Color? valueColor = null)
	{
		Label lbl = new Label { Text = label, ForeColor = Color.FromArgb(180, 180, 200), AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 0, 6) };
		Label val = new Label { Text = value, ForeColor = valueColor ?? Color.White, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(0, 6, 0, 6) };
		_grid.RowCount++;
		_grid.Controls.Add(lbl, 0, _grid.RowCount - 1);
		_grid.Controls.Add(val, 1, _grid.RowCount - 1);
	}

	private void RefreshStatus()
	{
		_grid.Controls.Clear();
		_grid.RowCount = 0;

		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		AddRow("Uygulama Sürümü:", "v" + version);

		bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		AddRow("Yönetici Modu:", isAdmin ? "Evet" : "Hayır", isAdmin ? Color.LightGreen : Color.Orange);

		bool driverInstalled = SafeCheckDriverInstalled();
		AddRow("Girdi Sürücüsü:", driverInstalled ? "Kurulu" : "Kurulu Değil", driverInstalled ? Color.LightGreen : Color.OrangeRed);

		bool isConnected = SnapNetClient.AppClient.IsConnected;
		string serverInfo = Settings.Instance.ClientSettings.ClientSettings.ServerIP + ":" + Settings.Instance.ClientSettings.ClientSettings.ServerPort;
		AddRow("SnapNet Bağlantısı:", isConnected ? ("Bağlı (" + serverInfo + ")") : "Bağlı Değil", isConnected ? Color.LightGreen : Color.Gray);

		string activeProfile = Settings.Instance.ScreenCapture.RectanglesSettings.ActiveProfileName;
		AddRow("Aktif Çözünürlük Profili:", string.IsNullOrEmpty(activeProfile) ? "(kaydedilmemiş)" : activeProfile);

		int runningWorkflowCount = 0;
		try
		{
			runningWorkflowCount = System.Linq.Enumerable.Count(_screenCaptureMainForm?.WorkflowEngine?.GetRunningWorkflows() ?? Array.Empty<string>());
		}
		catch
		{
		}
		AddRow("Çalışan İş Akışı Sayısı:", runningWorkflowCount.ToString());

		AddRow("", "");
		AddRow("Veritabanı Durumu:", "");
		string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DB");
		foreach (string dbFile in new[] { "MacroSettings.db", "ScreenCaptureSettings.db", "GeneralSettings.db", "ClientSettings.db" })
		{
			string fullPath = Path.Combine(dbFolder, dbFile);
			bool exists = File.Exists(fullPath);
			string sizeText = exists ? $"{new FileInfo(fullPath).Length / 1024} KB" : "bulunamadı";
			AddRow("  " + dbFile, sizeText, exists ? Color.LightGreen : Color.OrangeRed);
		}
	}

	private static bool SafeCheckDriverInstalled()
	{
		try
		{
			return InputInterceptor.CheckDriverInstalled();
		}
		catch
		{
			return false;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_refreshTimer?.Stop();
			_refreshTimer?.Dispose();
		}
		base.Dispose(disposing);
	}
}
