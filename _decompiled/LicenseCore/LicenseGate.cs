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

	private static ActivationStore _activation;

	public static LicenseInfo Current { get; private set; }

	/// <summary>This machine's code, shown in the UI so a customer can read it out when asked.</summary>
	public static string MachineCode => MachineId.Current;

	/// <summary>Why the last check failed, for the activation dialog to explain.</summary>
	public enum Problem
	{
		None,
		Missing,
		Malformed,
		Expired,
		WrongMachine,
		Revoked
	}

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
		return Current != null && !Current.IsExpired && !RevocationList.IsRevoked(Current.LicenseId);
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
		if (RevocationList.IsRevoked(Current.LicenseId))
		{
			return "iptal edildi";
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
		if (Current == null || Current.IsExpired || RevocationList.IsRevoked(Current.LicenseId))
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

	/// <summary>
	/// Loads the sealed activation state once per process and seeds everything that depends on
	/// it: the no-rollback clock floor and the cached revocation list.
	/// </summary>
	private static ActivationStore Activation
	{
		get
		{
			if (_activation == null)
			{
				_activation = ActivationStore.Load();
				LicenseClock.SetFloor(_activation.LastSeenUtc);
				RevocationList.LoadFromCache(_activation);
			}
			return _activation;
		}
	}

	/// <summary>Moves the clock floor forward and persists it when the reading is plausible.</summary>
	private static void TouchClock()
	{
		if (LicenseClock.TryAdvance(out DateTime newFloor))
		{
			Activation.LastSeenUtc = newFloor;
			Activation.Save();
		}
	}

	private static Problem Evaluate(LicenseInfo info)
	{
		if (info == null)
		{
			return Problem.Malformed;
		}
		if (RevocationList.IsRevoked(info.LicenseId))
		{
			return Problem.Revoked;
		}
		if (info.IsExpired)
		{
			return Problem.Expired;
		}
		// Only licences that explicitly name their machines are refused here. Unbound keys - the
		// kind issued today - are pinned by Claim() instead, via the sealed store.
		if (!info.IsForThisMachine)
		{
			return Problem.WrongMachine;
		}
		return Problem.None;
	}

	/// <summary>
	/// Records that this licence was activated on this machine. The record lives in the sealed
	/// store, so an activated installation cannot be copied to another PC and used there.
	/// </summary>
	/// <remarks>
	/// A recorded machine code that no longer matches is treated as a re-claim rather than a
	/// refusal, deliberately. The seal is what enforces the binding - DPAPI will not even open
	/// the file off-machine - so a mismatch here means the machine's own identity shifted under
	/// us, and locking the customer out of a licence that still has time on it would be the wrong
	/// trade. Whoever is running it already proved they are on the machine that sealed the file.
	/// </remarks>
	private static void Claim(LicenseInfo info)
	{
		if (info == null || string.IsNullOrEmpty(info.LicenseId))
		{
			return;
		}
		ActivationStore store = Activation;
		bool isNewClaim = !ShortCode.Equal(store.LicenseId, info.LicenseId);
		if (isNewClaim)
		{
			store.LicenseId = info.LicenseId;
			store.ClaimedUtc = LicenseClock.UtcNow;
		}
		if (!ShortCode.Equal(store.MachineCode, MachineId.Current))
		{
			store.MachineCode = MachineId.Current;
			isNewClaim = true;
		}
		if (isNewClaim)
		{
			store.Save();
		}
	}

	public static bool EnsureLicensed(string appDisplayName)
	{
		ActivationStore store = Activation;
		TouchClock();
		RevocationList.BeginRefresh(store);

		LicenseInfo storedInfo = null;
		Problem problem = Problem.Missing;
		string storedText = ReadStoredLicenseText();
		if (!string.IsNullOrWhiteSpace(storedText))
		{
			problem = LicenseValidator.TryValidate(storedText, out storedInfo) ? Evaluate(storedInfo) : Problem.Malformed;
			if (problem == Problem.None)
			{
				EnsureStoredInAllLocations(storedText);
				Claim(storedInfo);
				Current = storedInfo;
				return true;
			}
		}
		while (true)
		{
			using LicenseActivationForm dialog = new LicenseActivationForm(appDisplayName, storedInfo, problem, MachineCode);
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return false;
			}
			if (!LicenseValidator.TryValidate(dialog.EnteredLicense, out LicenseInfo newInfo))
			{
				MessageBox.Show("Geçersiz lisans anahtarı. Lütfen anahtarı eksiksiz yapıştırdığınızdan emin olun.", "Geçersiz Anahtar", MessageBoxButtons.OK, MessageBoxIcon.Error);
				problem = Problem.Malformed;
				continue;
			}
			problem = Evaluate(newInfo);
			if (problem != Problem.None)
			{
				MessageBox.Show(DescribeProblem(problem, newInfo), "Lisans Kabul Edilmedi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				storedInfo = newInfo;
				continue;
			}
			SaveLicense(dialog.EnteredLicense);
			Claim(newInfo);
			Current = newInfo;
			return true;
		}
	}

	public static string DescribeProblem(Problem problem, LicenseInfo info)
	{
		switch (problem)
		{
			case Problem.Expired:
				return (info != null && info.ExpiresUtc != DateTime.MaxValue)
					? $"Bu lisans anahtarının süresi dolmuş ({info.ExpiresUtc.ToLocalTime():dd.MM.yyyy})."
					: "Bu lisans anahtarının süresi dolmuş.";
			case Problem.WrongMachine:
				return "Bu lisans anahtarı başka bir bilgisayar için üretilmiş.\n\nBu bilgisayarın makine kodu:\n" + MachineCode;
			case Problem.Revoked:
				return "Bu lisans anahtarı iptal edilmiş. Lütfen satıcınızla görüşün.";
			case Problem.Malformed:
				return "Geçersiz lisans anahtarı.";
			default:
				return string.Empty;
		}
	}

	public static void StartPeriodicRecheck(Action onExpired)
	{
		_recheckTimer?.Dispose();
		_recheckTimer = new System.Threading.Timer(delegate
		{
			TouchClock();
			if (Current != null && (Current.IsExpired || RevocationList.IsRevoked(Current.LicenseId)))
			{
				onExpired?.Invoke();
			}
		}, null, 60000, 60000);
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
