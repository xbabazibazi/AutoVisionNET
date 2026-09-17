using System;
using System.Drawing;
using System.Windows.Forms;

namespace LicenseCore;

internal sealed class LicenseActivationForm : Form
{
	private readonly TextBox _textBox;

	public string EnteredLicense => _textBox.Text.Trim();

	public LicenseActivationForm(string appDisplayName, LicenseInfo expiredInfo)
	{
		Text = "Lisans Etkinleştir";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterScreen;
		MinimizeBox = false;
		MaximizeBox = false;
		ClientSize = new Size(460, 260);
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

		string statusText = (expiredInfo != null)
			? $"Lisansınızın süresi doldu ({expiredInfo.CustomerName}, bitiş: {expiredInfo.ExpiresUtc.ToLocalTime():dd.MM.yyyy}).\nDevam etmek için yeni bir lisans anahtarı girin."
			: "Bu uygulamayı kullanmak için bir lisans anahtarı girmeniz gerekiyor.";
		Label status = new Label
		{
			Text = statusText,
			ForeColor = Color.FromArgb(200, 120, 120),
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

		Button btnActivate = new Button
		{
			Text = "Aktifleştir",
			DialogResult = DialogResult.OK,
			BackColor = Color.FromArgb(55, 78, 92),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat,
			Location = new Point(240, 204),
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
			Location = new Point(346, 204),
			Size = new Size(94, 34)
		};
		btnExit.FlatAppearance.BorderSize = 0;

		Controls.Add(title);
		Controls.Add(status);
		Controls.Add(hint);
		Controls.Add(_textBox);
		Controls.Add(btnActivate);
		Controls.Add(btnExit);
		AcceptButton = btnActivate;
		CancelButton = btnExit;
	}
}
