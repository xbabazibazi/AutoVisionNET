using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace LicenseCore;

public static class LicenseGate
{
	private static string LicenseFilePath => Path.Combine(AppContext.BaseDirectory, "license.key");

	private static System.Threading.Timer _recheckTimer;

	public static LicenseInfo Current { get; private set; }

	public static bool IsCurrentlyValid()
	{
		return Current != null && !Current.IsExpired;
	}

	public static string GetStatusText()
	{
		return "Lisans: " + GetRemainingText();
	}

	public static string GetRemainingText()
	{
		if (Current == null || Current.IsExpired)
		{
			return "geçersiz";
		}
		if (Current.ExpiresUtc == DateTime.MaxValue)
		{
			return "SINIRSIZ (VIP)";
		}
		TimeSpan remaining = Current.TimeRemaining;
		if (remaining.TotalDays >= 1.0)
		{
			return $"{(int)remaining.TotalDays} gün kaldı";
		}
		if (remaining.TotalHours >= 1.0)
		{
			return $"{(int)remaining.TotalHours} saat kaldı";
		}
		return "az sonra dolacak";
	}

	public static Color GetStatusColor()
	{
		if (Current == null || Current.IsExpired)
		{
			return Color.FromArgb(220, 120, 120);
		}
		if (Current.ExpiresUtc == DateTime.MaxValue)
		{
			return Color.FromArgb(230, 190, 90);
		}
		if (Current.TimeRemaining.TotalDays <= 3.0)
		{
			return Color.FromArgb(230, 160, 90);
		}
		return Color.FromArgb(130, 200, 130);
	}

	public static bool EnsureLicensed(string appDisplayName)
	{
		TryLoadStoredLicense(out LicenseInfo storedInfo);
		if (storedInfo != null && !storedInfo.IsExpired)
		{
			Current = storedInfo;
			return true;
		}
		while (true)
		{
			using LicenseActivationForm dialog = new LicenseActivationForm(appDisplayName, storedInfo);
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return false;
			}
			if (LicenseValidator.TryValidate(dialog.EnteredLicense, out LicenseInfo newInfo))
			{
				if (newInfo.IsExpired)
				{
					MessageBox.Show($"Bu lisans anahtarının süresi dolmuş ({newInfo.ExpiresUtc.ToLocalTime():dd.MM.yyyy}).", "Lisans Süresi Dolmuş", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					storedInfo = newInfo;
					continue;
				}
				SaveLicense(dialog.EnteredLicense);
				Current = newInfo;
				return true;
			}
			MessageBox.Show("Geçersiz lisans anahtarı. Lütfen anahtarı eksiksiz yapıştırdığınızdan emin olun.", "Geçersiz Anahtar", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	public static void StartPeriodicRecheck(Action onExpired)
	{
		_recheckTimer?.Dispose();
		_recheckTimer = new System.Threading.Timer(delegate
		{
			if (Current != null && Current.IsExpired)
			{
				onExpired?.Invoke();
			}
		}, null, 60000, 60000);
	}

	private static bool TryLoadStoredLicense(out LicenseInfo info)
	{
		info = null;
		try
		{
			if (!File.Exists(LicenseFilePath))
			{
				return false;
			}
			string text = File.ReadAllText(LicenseFilePath);
			return LicenseValidator.TryValidate(text, out info);
		}
		catch
		{
			return false;
		}
	}

	private static void SaveLicense(string licenseText)
	{
		try
		{
			File.WriteAllText(LicenseFilePath, licenseText);
		}
		catch
		{
		}
	}
}
