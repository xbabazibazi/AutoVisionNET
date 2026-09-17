using System;
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
