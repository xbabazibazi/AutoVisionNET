using System;
using System.Drawing;
using System.Windows.Forms;
using SimpleLogger;

namespace UI2;

public class ErrorToastForm : Form
{
	private const int WS_EX_NOACTIVATE = 134217728;

	private readonly Label _titleLabel;

	private readonly Label _messageLabel;

	private readonly Timer _autoCloseTimer;

	protected override bool ShowWithoutActivation => true;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams cp = base.CreateParams;
			cp.ExStyle |= WS_EX_NOACTIVATE;
			return cp;
		}
	}

	public ErrorToastForm(LogLevel level, string message)
	{
		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		TopMost = true;
		ShowInTaskbar = false;
		Width = 380;
		Height = 90;
		BackColor = (level == LogLevel.Error) ? Color.FromArgb(180, 40, 40) : Color.FromArgb(190, 130, 20);

		_titleLabel = new Label
		{
			Text = (level == LogLevel.Error) ? "HATA" : "UYARI",
			ForeColor = Color.White,
			Font = new Font("Segoe UI", 10f, FontStyle.Bold),
			AutoSize = true,
			Location = new Point(10, 8)
		};
		_messageLabel = new Label
		{
			Text = message,
			ForeColor = Color.White,
			Font = new Font("Segoe UI", 9f),
			Location = new Point(10, 30),
			Size = new Size(340, 45),
			AutoEllipsis = true
		};
		Button closeButton = new Button
		{
			Text = "X",
			FlatStyle = FlatStyle.Flat,
			ForeColor = Color.White,
			BackColor = Color.Transparent,
			Location = new Point(345, 4),
			Size = new Size(28, 24)
		};
		closeButton.FlatAppearance.BorderSize = 0;
		closeButton.Click += (s, e) => Close();

		Controls.Add(_titleLabel);
		Controls.Add(_messageLabel);
		Controls.Add(closeButton);

		Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1024, 768);
		Location = new Point(workingArea.Right - Width - 16, workingArea.Bottom - Height - 16);

		_autoCloseTimer = new Timer { Interval = 8000 };
		_autoCloseTimer.Tick += (s, e) => Close();
		_autoCloseTimer.Start();
	}

	public bool IsUsable => !IsDisposed && !Disposing;

	public void UpdateMessage(LogLevel level, string message)
	{
		BackColor = (level == LogLevel.Error) ? Color.FromArgb(180, 40, 40) : Color.FromArgb(190, 130, 20);
		_titleLabel.Text = (level == LogLevel.Error) ? "HATA" : "UYARI";
		_messageLabel.Text = message;
		_autoCloseTimer.Stop();
		_autoCloseTimer.Start();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_autoCloseTimer?.Stop();
			_autoCloseTimer?.Dispose();
		}
		base.Dispose(disposing);
	}
}
