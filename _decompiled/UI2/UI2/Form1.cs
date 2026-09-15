#define DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using InputInterceptorNS;
using InputManager;
using SettingsManager;
using SimpleLogger;
using UI2.Macro;
using UI2.ScreenCapture;
using UI2.Services;

namespace UI2;

public class Form1 : Form
{
	private bool _isFormDragging;

	private Point _dragStartPoint;

	private Color _toolStripOriginalColor;

	private bool _isClosing;

	private bool _isControlKeyPressed;

	private readonly CancellationTokenSource _cancellationTokenSourceTpParty;

	private readonly Settings _settings = Settings.Instance;

	public readonly ScreenCaptureMainForm _screenCaptureMainForm;

	private readonly Macro2 _macroForm;

	private readonly Logs _logsForm;

	private readonly AttackService _attackService;

	private readonly SettingsForm _settingsForm;

	private readonly InputUtils _inputUtils;

	private readonly Logger _logger = Logger.Instance;

	private readonly ClientForm _clientForm;

	private readonly KeyCommandManager _keyManager;

	private readonly FormManager _formManager;

	private IContainer components = null;

	private ToolStrip toolStrip1;

	private ToolStripButton toolStripButtonDrag;

	private ToolStripButton toolStripButtonClose;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripDropDownButton toolStripDropDownButtonForms;

	private ToolStripMenuItem toolStripButtonMacro;

	private ToolStripMenuItem toolStripSplitButton1;

	private ToolStripMenuItem seceneklerToolStripMenuItem;

	private ToolStripMenuItem ayarlarToolStripMenuItem;

	private ToolStripMenuItem toolStripButtonClient;

	private ToolStripMenuItem toolStripButtonLogs;

	private ToolStripDropDownButton toolStripDropDownButtonTools;

	private ToolStripMenuItem toolStripMenuItemCheckForUpdates;

	public static Form1 Instance { get; private set; }

	public ToolStripItem ToolStripButtonClient => toolStripButtonClient;

	public Form1()
	{
		Instance = this;
		InitializeComponent();
		LoadFormPosition();
		_formManager = new FormManager(this);
		_logsForm = new Logs();
		_clientForm = new ClientForm();
		_inputUtils = new InputUtils(ShowMessage, OnKeyPress, OnMouseAction);
		_attackService = new AttackService(_inputUtils, _logsForm.GetLogInstance());
		_macroForm = new Macro2(_inputUtils);
		_settingsForm = new SettingsForm();
		_screenCaptureMainForm = new ScreenCaptureMainForm(_inputUtils, _logsForm, _attackService, _macroForm, this);
		_formManager.RegisterForm("Macro", _macroForm.GetForm());
		_formManager.RegisterForm("ScreenCapture", _screenCaptureMainForm);
		_formManager.RegisterForm("Settings", _settingsForm.GetForm());
		_formManager.RegisterForm("Logs", _logsForm.GetForm());
		_formManager.RegisterForm("Client", _clientForm);
		_keyManager = new KeyCommandManager(_attackService, _inputUtils, _settings, _screenCaptureMainForm, this);
		_cancellationTokenSourceTpParty = new CancellationTokenSource();
		UpdateDB("UpdateDB.sql");
		base.TopMost = true;
	}

	private void UpdateDB(string sqlFileName)
	{
		try
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sqlFileName);
			if (!File.Exists(path))
			{
				return;
			}
			string text = File.ReadAllText(path);
			string[] array = text.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string value = text2.Trim();
				if (!string.IsNullOrWhiteSpace(value))
				{
				}
			}
			File.Delete(path);
			ShowMessage("Veritabanı başarıyla güncellendi.");
		}
		catch (Exception ex)
		{
			ShowError("Veritabanı güncellenirken hata oluştu: " + ex.Message);
		}
	}

	private async void OnKeyPress(KeyStroke keyStroke)
	{
		if (keyStroke.State == KeyState.Down)
		{
			if (keyStroke.Code == KeyCode.Control)
			{
				_isControlKeyPressed = true;
			}
			else
			{
				_keyManager.Execute(keyStroke.Code, _isControlKeyPressed);
			}
		}
		else if (keyStroke.State == KeyState.Up && keyStroke.Code == KeyCode.Control)
		{
			_isControlKeyPressed = false;
		}
	}

	private void toolStripButtonClose_Click(object sender, EventArgs e)
	{
		try
		{
			Environment.Exit(0);
		}
		catch (Exception ex)
		{
			ShowError("Uygulama kapatılırken hata oluştu: " + ex.Message);
		}
	}

	private void toolStripButtonDrag_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_isFormDragging = true;
			_dragStartPoint = new Point(e.X, e.Y);
			Cursor = Cursors.SizeAll;
			_toolStripOriginalColor = toolStrip1.BackColor;
			toolStrip1.BackColor = Color.LightGray;
		}
	}

	private void toolStripButtonDrag_MouseMove(object sender, MouseEventArgs e)
	{
		if (_isFormDragging)
		{
			Point point = PointToScreen(e.Location);
			base.Location = new Point(point.X - _dragStartPoint.X, point.Y - _dragStartPoint.Y);
		}
	}

	private void toolStripButtonDrag_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_isFormDragging = false;
			Cursor = Cursors.Default;
			toolStrip1.BackColor = _toolStripOriginalColor;
			SaveFormPosition();
		}
	}

	public void CheckForUpdates()
	{
		toolStripButtonCheckForUpdates_Click(null, EventArgs.Empty);
	}

	private async Task<bool> CheckForUpdatesAsync()
	{
		using HttpClient client = new HttpClient();
		try
		{
			return (await client.GetAsync("https://github.com/katadora/my-app-updates/raw/main/update.zip")).IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			ShowError("Güncelleme kontrolü sırasında hata: " + ex2.Message);
			return false;
		}
	}

	private async void toolStripButtonCheckForUpdates_Click(object sender, EventArgs e)
	{
		try
		{
			if (await CheckForUpdatesAsync())
			{
				StartUpdaterAsAdmin();
				Environment.Exit(0);
			}
			else
			{
				ShowMessage("Güncelleme bulunamadı.");
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			ShowError("Güncelleme kontrol hatası: " + ex2.Message);
		}
	}

	private void StartUpdaterAsAdmin()
	{
		try
		{
			string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FluxioUpdater.exe");
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = fileName,
				UseShellExecute = true,
				Verb = "runas"
			};
			Process.Start(startInfo);
		}
		catch (Exception ex)
		{
			ShowError("Updater başlatılamadı: " + ex.Message);
		}
	}

	private void toolStripButtonMacro_Click(object sender, EventArgs e)
	{
		_formManager.ToggleForm("Macro");
	}

	private void seceneklerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_formManager.ToggleForm("ScreenCapture");
	}

	private void toolStripButtonLogs_Click(object sender, EventArgs e)
	{
		_formManager.ToggleForm("Logs");
	}

	private void ayarlarToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_formManager.ToggleForm("Settings");
	}

	private void toolStripButtonClient_Click(object sender, EventArgs e)
	{
		_formManager.ToggleForm("Client");
		toolStripButtonClient.BackColor = (_clientForm.IsConnected ? Color.LimeGreen : Color.Red);
	}

	private void Form1_Shown(object sender, EventArgs e)
	{
		base.TopMost = false;
		base.TopMost = true;
	}

	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (_isClosing)
		{
			return;
		}
		_isClosing = true;
		try
		{
			SaveFormPosition();
			_cancellationTokenSourceTpParty?.Cancel();
			_formManager.CloseAllForms();
			Task.Run(delegate
			{
				try
				{
					_inputUtils?.Dispose();
				}
				catch
				{
				}
			});
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Kapanma hatası: " + ex.Message);
		}
	}

	public void ShowMessage(string message)
	{
		this.InvokeIfRequired(delegate
		{
			MessageBox.Show(message, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		});
	}

	public void ShowError(string message)
	{
		this.InvokeIfRequired(delegate
		{
			MessageBox.Show(message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		});
	}

	public void OnMouseAction(MouseStroke mouseStroke)
	{
	}

	private void CloseAllForms()
	{
		CloseFormIfNotDisposed(_macroForm);
		CloseFormIfNotDisposed(_logsForm);
		CloseFormIfNotDisposed(_screenCaptureMainForm);
		CloseFormIfNotDisposed(_settingsForm);
		CloseFormIfNotDisposed(_clientForm);
		List<Form> list = new List<Form>(Application.OpenForms.Cast<Form>());
		foreach (Form item in list)
		{
			if (item is RectangleOverlayForm { IsDisposed: false } rectangleOverlayForm)
			{
				rectangleOverlayForm.Close();
			}
		}
	}

	private void CloseFormIfNotDisposed(Form form)
	{
		if (form != null && !form.IsDisposed)
		{
			form.Close();
		}
	}

	private void SaveFormPosition()
	{
		_settings.GeneralSettings.FormSettings.FormLocationX = base.Location.X;
		_settings.GeneralSettings.FormSettings.FormLocationY = base.Location.Y;
	}

	private void LoadFormPosition()
	{
		base.StartPosition = FormStartPosition.Manual;
		base.Location = new Point(_settings.GeneralSettings.FormSettings.FormLocationX, _settings.GeneralSettings.FormSettings.FormLocationY);
	}

	private void ToggleFormVisibility(Form formToOpen, params Form[] otherForms)
	{
		foreach (Form form in otherForms)
		{
			if (form.Visible)
			{
				form.Hide();
			}
		}
		if (formToOpen.Visible)
		{
			formToOpen.Hide();
			return;
		}
		formToOpen.Location = CalculateAdjacentFormPosition(formToOpen);
		formToOpen.Show();
		formToOpen.BringToFront();
	}

	private Point CalculateAdjacentFormPosition(Form formToOpen)
	{
		Point point = PointToScreen(Point.Empty);
		int num = point.X;
		int num2 = point.Y + base.Height;
		Rectangle? rectangle = Screen.PrimaryScreen?.WorkingArea;
		if (rectangle.HasValue)
		{
			Rectangle value = rectangle.Value;
			if (num2 + formToOpen.Height > value.Bottom)
			{
				num2 = point.Y - formToOpen.Height;
			}
			num2 = Math.Max(value.Top, Math.Min(num2, value.Bottom - formToOpen.Height));
			num = Math.Max(value.Left, Math.Min(num, value.Right - formToOpen.Width));
		}
		return new Point(num, num2);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UI2.Form1));
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.toolStripButtonDrag = new System.Windows.Forms.ToolStripButton();
		this.toolStripButtonClose = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripDropDownButtonForms = new System.Windows.Forms.ToolStripDropDownButton();
		this.toolStripButtonMacro = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripMenuItem();
		this.seceneklerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ayarlarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripButtonClient = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripButtonLogs = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripDropDownButtonTools = new System.Windows.Forms.ToolStripDropDownButton();
		this.toolStripMenuItemCheckForUpdates = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStrip1.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip1.AutoSize = false;
		this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(40, 40, 45);
		this.toolStrip1.Font = new System.Drawing.Font("Tahoma", 8f);
		this.toolStrip1.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
		this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripButtonDrag, this.toolStripButtonClose, this.toolStripSeparator1, this.toolStripDropDownButtonForms, this.toolStripSeparator2, this.toolStripDropDownButtonTools });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
		this.toolStrip1.Size = new System.Drawing.Size(240, 35);
		this.toolStrip1.TabIndex = 0;
		this.toolStripButtonDrag.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.toolStripButtonDrag.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
		this.toolStripButtonDrag.Name = "toolStripButtonDrag";
		this.toolStripButtonDrag.Size = new System.Drawing.Size(30, 32);
		this.toolStripButtonDrag.Text = "Taşı";
		this.toolStripButtonDrag.ToolTipText = "Formu Taşı";
		this.toolStripButtonDrag.MouseDown += new System.Windows.Forms.MouseEventHandler(toolStripButtonDrag_MouseDown);
		this.toolStripButtonDrag.MouseMove += new System.Windows.Forms.MouseEventHandler(toolStripButtonDrag_MouseMove);
		this.toolStripButtonDrag.MouseUp += new System.Windows.Forms.MouseEventHandler(toolStripButtonDrag_MouseUp);
		this.toolStripButtonClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.toolStripButtonClose.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
		this.toolStripButtonClose.Name = "toolStripButtonClose";
		this.toolStripButtonClose.Size = new System.Drawing.Size(39, 32);
		this.toolStripButtonClose.Text = "Kapat";
		this.toolStripButtonClose.ToolTipText = "Programı Kapat";
		this.toolStripButtonClose.Click += new System.EventHandler(toolStripButtonClose_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 35);
		this.toolStripDropDownButtonForms.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.toolStripDropDownButtonForms.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.toolStripButtonMacro, this.toolStripSplitButton1, this.toolStripButtonClient, this.toolStripButtonLogs });
		this.toolStripDropDownButtonForms.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
		this.toolStripDropDownButtonForms.Name = "toolStripDropDownButtonForms";
		this.toolStripDropDownButtonForms.Size = new System.Drawing.Size(60, 32);
		this.toolStripDropDownButtonForms.Text = "Menü ▼";
		this.toolStripButtonMacro.Name = "toolStripButtonMacro";
		this.toolStripButtonMacro.Size = new System.Drawing.Size(180, 22);
		this.toolStripButtonMacro.Text = "Makro";
		this.toolStripButtonMacro.Click += new System.EventHandler(toolStripButtonMacro_Click);
		this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.seceneklerToolStripMenuItem, this.ayarlarToolStripMenuItem });
		this.toolStripSplitButton1.Name = "toolStripSplitButton1";
		this.toolStripSplitButton1.Size = new System.Drawing.Size(180, 22);
		this.toolStripSplitButton1.Text = "Ekran Yakalama";
		this.seceneklerToolStripMenuItem.Name = "seceneklerToolStripMenuItem";
		this.seceneklerToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
		this.seceneklerToolStripMenuItem.Text = "Seçenekler";
		this.seceneklerToolStripMenuItem.Click += new System.EventHandler(seceneklerToolStripMenuItem_Click);
		this.ayarlarToolStripMenuItem.Name = "ayarlarToolStripMenuItem";
		this.ayarlarToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
		this.ayarlarToolStripMenuItem.Text = "Ayarlar";
		this.ayarlarToolStripMenuItem.Click += new System.EventHandler(ayarlarToolStripMenuItem_Click);
		this.toolStripButtonClient.Name = "toolStripButtonClient";
		this.toolStripButtonClient.Size = new System.Drawing.Size(180, 22);
		this.toolStripButtonClient.Text = "Client";
		this.toolStripButtonClient.Click += new System.EventHandler(toolStripButtonClient_Click);
		this.toolStripButtonLogs.Name = "toolStripButtonLogs";
		this.toolStripButtonLogs.Size = new System.Drawing.Size(180, 22);
		this.toolStripButtonLogs.Text = "Loglar";
		this.toolStripButtonLogs.Click += new System.EventHandler(toolStripButtonLogs_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 35);
		this.toolStripDropDownButtonTools.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.toolStripDropDownButtonTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.toolStripMenuItemCheckForUpdates });
		this.toolStripDropDownButtonTools.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
		this.toolStripDropDownButtonTools.Name = "toolStripDropDownButtonTools";
		this.toolStripDropDownButtonTools.Size = new System.Drawing.Size(68, 32);
		this.toolStripDropDownButtonTools.Text = "Araçlar ▼";
		this.toolStripMenuItemCheckForUpdates.Name = "toolStripMenuItemCheckForUpdates";
		this.toolStripMenuItemCheckForUpdates.Size = new System.Drawing.Size(192, 22);
		this.toolStripMenuItemCheckForUpdates.Text = "Güncellemeleri Kontrol Et";
		this.toolStripMenuItemCheckForUpdates.Click += new System.EventHandler(toolStripButtonCheckForUpdates_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(30, 30, 35);
		base.ClientSize = new System.Drawing.Size(240, 35);
		base.ControlBox = false;
		base.Controls.Add(this.toolStrip1);
		this.Font = new System.Drawing.Font("Tahoma", 8f);
		this.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "Form1";
		base.ShowIcon = false;
		this.Text = "SnapNet";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form1_FormClosing);
		base.Shown += new System.EventHandler(Form1_Shown);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		base.ResumeLayout(false);
	}
}
