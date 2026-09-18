using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SimpleLogger;
using UI2.Interfaces;

namespace UI2;

public class Logs : Form, ILogsForm
{
	private const int WM_NCLBUTTONDOWN = 161;

	private const int HT_CAPTION = 2;

	private readonly Logger _logger = Logger.Instance;

	private readonly Color _backgroundColor = Color.FromArgb(28, 28, 33);

	private readonly Color _surfaceColor = Color.FromArgb(38, 38, 45);

	private readonly Color _inputBackgroundColor = Color.FromArgb(48, 48, 55);

	private readonly Color _textPrimaryColor = Color.FromArgb(235, 235, 240);

	private readonly Color _buttonSecondaryColor = Color.FromArgb(200, 60, 60);

	private readonly Color _connectedColor = Color.FromArgb(0, 153, 102);

	private const int MAX_LOG_ITEMS = 500;

	private IContainer components = null;

	private ListBox lstLogs;

	private Label lblStatus;

	private ComboBox cmbLogLevel;

	private Button btnClose;

	private Panel headerPanel;

	private Panel mainPanel;

	[DllImport("user32.dll")]
	private static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	private static extern int SendMessage(nint hWnd, int Msg, int wParam, int lParam);

	public Logs()
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.Manual;
		InitializeLogger();
		InitializeControls();
		SetupFormDragging();
		SetupCloseButton();
	}

	private void InitializeLogger()
	{
		_logger.SetLogMethod(delegate(string message)
		{
			if (lstLogs.InvokeRequired)
			{
				lstLogs.Invoke((MethodInvoker)delegate
				{
					AddLogItem(message);
				});
			}
			else
			{
				AddLogItem(message);
			}
			UpdateStatusLabel();
		});
	}

	private void AddLogItem(string message)
	{
		if (lstLogs.Items.Count >= 500)
		{
			lstLogs.Items.RemoveAt(0);
		}
		lstLogs.Items.Add(message);
		lstLogs.TopIndex = lstLogs.Items.Count - 1;
	}

	private void InitializeControls()
	{
		cmbLogLevel.DataSource = Enum.GetValues(typeof(LogLevel));
		cmbLogLevel.SelectedItem = _logger.GetCurrentLogLevel();
		cmbLogLevel.SelectedIndexChanged += delegate
		{
			_logger.SetLogLevel((LogLevel)cmbLogLevel.SelectedItem);
			UpdateStatusLabel();
		};
	}

	private void SetupFormDragging()
	{
		headerPanel.MouseDown += delegate(object? s, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(base.Handle, 161, 2, 0);
			}
		};
		lblStatus.MouseDown += delegate(object? s, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(base.Handle, 161, 2, 0);
			}
		};
	}

	private void SetupCloseButton()
	{
		btnClose.Click += delegate
		{
			Hide();
		};
		btnClose.MouseEnter += delegate
		{
			btnClose.BackColor = _buttonSecondaryColor;
			btnClose.ForeColor = Color.White;
		};
		btnClose.MouseLeave += delegate
		{
			btnClose.BackColor = Color.Transparent;
			btnClose.ForeColor = _textPrimaryColor;
		};
	}

	private void UpdateStatusLabel()
	{
		string statusText = $"● Aktif | Seviye: {_logger.GetCurrentLogLevel()} | Kayıt: {lstLogs.Items.Count}";
		if (lblStatus.InvokeRequired)
		{
			lblStatus.Invoke((MethodInvoker)delegate
			{
				lblStatus.Text = statusText;
			});
		}
		else
		{
			lblStatus.Text = statusText;
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		_logger.SetLogMethod(null);
		base.OnFormClosing(e);
	}

	public Form GetForm()
	{
		return this;
	}

	public void GetHide()
	{
		Hide();
	}

	public Logger GetLogInstance()
	{
		return _logger;
	}

	public bool GetIsDisposed()
	{
		return base.IsDisposed;
	}

	public void GetClose()
	{
		Close();
	}

	public void ClearLogs()
	{
		if (lstLogs.InvokeRequired)
		{
			lstLogs.Invoke((MethodInvoker)delegate
			{
				lstLogs.Items.Clear();
			});
		}
		else
		{
			lstLogs.Items.Clear();
		}
		UpdateStatusLabel();
	}

	public void TrimLogs(int maxItems)
	{
		if (lstLogs.InvokeRequired)
		{
			lstLogs.Invoke((MethodInvoker)delegate
			{
				TrimLogItems(maxItems);
			});
		}
		else
		{
			TrimLogItems(maxItems);
		}
		UpdateStatusLabel();
	}

	private void TrimLogItems(int maxItems)
	{
		while (lstLogs.Items.Count > maxItems)
		{
			lstLogs.Items.RemoveAt(0);
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
		this.lstLogs = new System.Windows.Forms.ListBox();
		this.lblStatus = new System.Windows.Forms.Label();
		this.cmbLogLevel = new System.Windows.Forms.ComboBox();
		this.btnClose = new System.Windows.Forms.Button();
		this.headerPanel = new System.Windows.Forms.Panel();
		this.mainPanel = new System.Windows.Forms.Panel();
		this.headerPanel.SuspendLayout();
		this.mainPanel.SuspendLayout();
		base.SuspendLayout();
		this.lstLogs.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.lstLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstLogs.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lstLogs.Font = new System.Drawing.Font("Consolas", 9f);
		this.lstLogs.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.lstLogs.FormattingEnabled = true;
		this.lstLogs.ItemHeight = 14;
		this.lstLogs.Location = new System.Drawing.Point(15, 15);
		this.lstLogs.Name = "lstLogs";
		this.lstLogs.Size = new System.Drawing.Size(570, 220);
		this.lstLogs.TabIndex = 0;
		this.lblStatus.AutoSize = true;
		this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(0, 153, 102);
		this.lblStatus.Location = new System.Drawing.Point(15, 8);
		this.lblStatus.Name = "lblStatus";
		this.lblStatus.Size = new System.Drawing.Size(102, 14);
		this.lblStatus.TabIndex = 1;
		this.lblStatus.Text = "● Hazır";
		this.cmbLogLevel.BackColor = System.Drawing.Color.FromArgb(48, 48, 55);
		this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cmbLogLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.cmbLogLevel.Font = new System.Drawing.Font("Segoe UI", 8f);
		this.cmbLogLevel.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.cmbLogLevel.FormattingEnabled = true;
		this.cmbLogLevel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.cmbLogLevel.Location = new System.Drawing.Point(440, 4);
		this.cmbLogLevel.Name = "cmbLogLevel";
		this.cmbLogLevel.Size = new System.Drawing.Size(120, 21);
		this.cmbLogLevel.TabIndex = 2;
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.btnClose.ForeColor = System.Drawing.Color.FromArgb(235, 235, 240);
		this.btnClose.Location = new System.Drawing.Point(575, 5);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(20, 20);
		this.btnClose.TabIndex = 3;
		this.btnClose.Text = "✕";
		this.btnClose.UseVisualStyleBackColor = false;
		this.headerPanel.BackColor = System.Drawing.Color.FromArgb(38, 38, 45);
		this.headerPanel.Controls.Add(this.lblStatus);
		this.headerPanel.Controls.Add(this.cmbLogLevel);
		this.headerPanel.Controls.Add(this.btnClose);
		this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.headerPanel.Location = new System.Drawing.Point(0, 0);
		this.headerPanel.Name = "headerPanel";
		this.headerPanel.Size = new System.Drawing.Size(600, 30);
		this.headerPanel.TabIndex = 4;
		this.mainPanel.BackColor = System.Drawing.Color.FromArgb(28, 28, 33);
		this.mainPanel.Controls.Add(this.lstLogs);
		this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.mainPanel.Location = new System.Drawing.Point(0, 30);
		this.mainPanel.Name = "mainPanel";
		this.mainPanel.Padding = new System.Windows.Forms.Padding(15);
		this.mainPanel.Size = new System.Drawing.Size(600, 250);
		this.mainPanel.TabIndex = 5;
		this.BackColor = System.Drawing.Color.FromArgb(28, 28, 33);
		base.ClientSize = new System.Drawing.Size(600, 325);
		base.Controls.Add(this.mainPanel);
		base.Controls.Add(this.headerPanel);
		this.Font = new System.Drawing.Font("Segoe UI", 8f);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "Logs";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Sistem Kayıtları";
		base.TopMost = true;
		this.headerPanel.ResumeLayout(false);
		this.headerPanel.PerformLayout();
		this.mainPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
