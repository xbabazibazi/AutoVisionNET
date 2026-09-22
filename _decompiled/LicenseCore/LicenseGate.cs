using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace LicenseCore;

public static class LicenseGate
{
	private const string LicenseFileName = "license.key";

	private const string StoreFolderName = "EVOX";

	private static System.Threading.Timer _recheckTimer;

	public static LicenseInfo Current { get; private set; }

	// The licence used to be stored next to the exe. That location is fragile:
	//  * the install folder can be read-only (Program Files), and SaveLicense swallowed
	//    the resulting exception, so the licence silently never persisted;
	//  * the updater relaunches the app elevated, so post-update runs do not necessarily
	//    resolve/permission that path the same way a normal run does;
	//  * re-extracting the package somewhere else leaves the licence behind.
	// It is now kept in stable per-machine / per-user folders. The old path is still read
	// so an existing activation migrates across automatically and nobody has to re-enter it.
	/// <summary>
	/// A licence dropped next to the executable is adopted and copied into the real store on the
	/// next run. This is read-only and doubles as the way to provision a machine by hand: put
	/// license.key beside the exe once and it moves itself into place.
	/// </summary>
	private static IReadOnlyList<string> GetLegacyLicensePaths()
	{
		List<string> paths = new List<string>();
		try
		{
			paths.Add(Path.Combine(AppContext.BaseDirectory, LicenseFileName));
		}
		catch
		{
		}
		return paths;
	}

	/// <summary>
	/// Where the licence is persisted, in read priority order. ProgramData comes first
	/// because it is shared across users *and* across elevated/non-elevated runs of the app.
	/// </summary>
	public static IReadOnlyList<string> GetLicenseStorePaths()
	{
		List<string> paths = new List<string>();
		AddStorePath(paths, Environment.SpecialFolder.CommonApplicationData, StoreFolderName);
		AddStorePath(paths, Environment.SpecialFolder.ApplicationData, StoreFolderName);
		return paths;
	}

	private static void AddStorePath(List<string> paths, Environment.SpecialFolder folder, string folderName)
	{
		try
		{
			string root = Environment.GetFolderPath(folder);
			if (!string.IsNullOrEmpty(root))
			{
				paths.Add(Path.Combine(root, folderName, LicenseFileName));
			}
		}
		catch
		{
		}
	}

	public static string ReadStoredLicenseText()
	{
		foreach (string path in GetLicenseStorePaths())
		{
			string text = TryReadFile(path);
			if (text != null)
			{
				return text;
			}
		}
		foreach (string legacyPath in GetLegacyLicensePaths())
		{
			string legacyText = TryReadFile(legacyPath);
			if (legacyText != null)
			{
				return legacyText;
			}
		}
		return null;
	}

	private static string TryReadFile(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				string text = File.ReadAllText(path);
				if (!string.IsNullOrWhiteSpace(text))
				{
					return text;
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private static bool TryWriteFile(string path, string licenseText)
	{
		try
		{
			string folder = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(folder))
			{
				Directory.CreateDirectory(folder);
			}
			File.WriteAllText(path, licenseText);
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Writes the licence to every store location that accepts it. Returns false only when
	/// none of them could be written, which is the case the caller must surface to the user.
	/// </summary>
	public static bool WriteLicenseText(string licenseText)
	{
		bool written = false;
		foreach (string path in GetLicenseStorePaths())
		{
			if (TryWriteFile(path, licenseText))
			{
				written = true;
			}
		}
		return written;
	}

	private static void EnsureStoredInAllLocations(string licenseText)
	{
		foreach (string path in GetLicenseStorePaths())
		{
			if (TryReadFile(path) == null)
			{
				TryWriteFile(path, licenseText);
			}
		}
	}

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
		string licenseText = ReadStoredLicenseText();
		if (string.IsNullOrWhiteSpace(licenseText))
		{
			return false;
		}
		if (!LicenseValidator.TryValidate(licenseText, out info))
		{
			return false;
		}
		// Migrate a licence that still lives in the old install-folder location (or that is
		// only present in one of the stores) so later runs find it wherever they look.
		EnsureStoredInAllLocations(licenseText);
		return true;
	}

	private static void SaveLicense(string licenseText)
	{
		if (!WriteLicenseText(licenseText))
		{
			// Never fail silently here: a licence that cannot be written is exactly why the
			// activation prompt would keep coming back on every launch.
			MessageBox.Show(
				"Lisans diske kaydedilemedi, bu yüzden uygulama tekrar açıldığında lisans isteyebilir.\n\nDenenen konumlar:\n" + string.Join("\n", GetLicenseStorePaths()),
				"Lisans Kaydedilemedi",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}
	}
}
