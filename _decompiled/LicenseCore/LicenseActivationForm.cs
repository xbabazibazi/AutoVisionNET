using System;
using System.Drawing;
using System.Windows.Forms;

namespace LicenseCore;

internal sealed class LicenseActivationForm : Form
{
	private readonly TextBox _textBox;

	public string EnteredLicense => _textBox.Text.Trim();

	public LicenseActivationForm(string appDisplayName, LicenseInfo problemInfo, LicenseGate.Problem problem, string machineCode)
	{
		Text = "Lisans Etkinleştir";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterScreen;
		MinimizeBox = false;
		MaximizeBox = false;
		ClientSize = new Size(460, 322);
		BackColor = Color.FromArgb(28, 28, 33);
		ForeColor = Color.FromArgb(235, 235, 240);
		Font = new Font("Segoe UI", 9f);

		Label title = new Label
		{
			Text = appDisplayName,
			Font = new Font("Segoe UI", 13f, FontStyle.Bold),
			ForeColor = Color.White,
			AutoSize = true,
			Location = new Point(20, 16)
		};

		Label status = new Label
		{
			Text = DescribeState(problemInfo, problem),
			ForeColor = (problem == LicenseGate.Problem.Missing)
				? Color.FromArgb(180, 185, 195)
				: Color.FromArgb(200, 120, 120),
			Location = new Point(20, 50),
			Size = new Size(420, 40),
			Font = new Font("Segoe UI", 8.5f)
		};

		Label hint = new Label
		{
			Text = "Lisans anahtarı:",
			ForeColor = Color.FromArgb(180, 185, 195),
			Location = new Point(20, 96),
			AutoSize = true
		};

		_textBox = new TextBox
		{
			Multiline = true,
			ScrollBars = ScrollBars.Vertical,
			Location = new Point(20, 118),
			Size = new Size(420, 70),
			BackColor = Color.FromArgb(48, 48, 55),
			ForeColor = Color.White,
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Consolas", 8.5f)
		};

		// The machine code is shown on every activation, not only when a key is rejected for the
		// wrong machine: it is the one piece of information support will ask for, and a customer
		// who cannot start the app has no other screen left to read it from.
		Label machineLabel = new Label
		{
			Text = "Bu bilgisayarın makine kodu (destek isterse bunu iletin):",
			ForeColor = Color.FromArgb(150, 155, 165),
			Location = new Point(20, 198),
			AutoSize = true,
			Font = new Font("Segoe UI", 8f)
		};

		TextBox machineBox = new TextBox
		{
			Text = machineCode,
			ReadOnly = true,
			Location = new Point(20, 220),
			Size = new Size(310, 24),
			BackColor = Color.FromArgb(40, 40, 46),
			ForeColor = Color.FromArgb(200, 205, 215),
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Consolas", 9.5f),
			TextAlign = HorizontalAlignment.Center
		};

		Button btnCopyMachine = new Button
		{
			Text = "Kopyala",
			BackColor = Color.FromArgb(52, 52, 60),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(338, 219),
			Size = new Size(102, 26)
		};
		btnCopyMachine.FlatAppearance.BorderSize = 0;
		btnCopyMachine.Click += delegate
		{
			try
			{
				Clipboard.SetText(machineCode);
				btnCopyMachine.Text = "Kopyalandı";
			}
			catch
			{
				// Another process can hold the clipboard open; the code is selectable either way.
				btnCopyMachine.Text = "Kopyalanamadı";
			}
		};

		Button btnActivate = new Button
		{
			Text = "Aktifleştir",
			DialogResult = DialogResult.OK,
			BackColor = Color.FromArgb(55, 78, 92),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(240, 266),
			Size = new Size(100, 34)
		};
		btnActivate.FlatAppearance.BorderSize = 0;

		Button btnExit = new Button
		{
			Text = "Çıkış",
			DialogResult = DialogResult.Cancel,
			BackColor = Color.FromArgb(60, 60, 65),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(346, 266),
			Size = new Size(94, 34)
		};
		btnExit.FlatAppearance.BorderSize = 0;

		Controls.Add(title);
		Controls.Add(status);
		Controls.Add(hint);
		Controls.Add(_textBox);
		Controls.Add(machineLabel);
		Controls.Add(machineBox);
		Controls.Add(btnCopyMachine);
		Controls.Add(btnActivate);
		Controls.Add(btnExit);
		AcceptButton = btnActivate;
		CancelButton = btnExit;
	}

	private static string DescribeState(LicenseInfo info, LicenseGate.Problem problem)
	{
		switch (problem)
		{
			case LicenseGate.Problem.Expired:
				return (info != null)
					? $"Lisansınızın süresi doldu ({info.CustomerName}, bitiş: {info.ExpiresUtc.ToLocalTime():dd.MM.yyyy}).\nDevam etmek için yeni bir lisans anahtarı girin."
					: "Lisansınızın süresi doldu. Devam etmek için yeni bir lisans anahtarı girin.";
			case LicenseGate.Problem.Revoked:
				return "Bu lisans anahtarı iptal edilmiş. Lütfen satıcınızla görüşün.";
			case LicenseGate.Problem.WrongMachine:
				return "Bu lisans anahtarı başka bir bilgisayar için üretilmiş.\nAşağıdaki makine kodu ile yeni bir anahtar isteyin.";
			case LicenseGate.Problem.Malformed:
				return "Kayıtlı lisans anahtarı okunamadı. Lütfen anahtarı yeniden girin.";
			default:
				return "Bu uygulamayı kullanmak için bir lisans anahtarı girmeniz gerekiyor.";
		}
	}
}
