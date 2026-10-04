using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LicenseCore;

/// <summary>
/// The kill switch for a key that got passed around. A short, RSA-signed list of licence IDs is
/// published in the GitHub repo; every client picks it up in the background and refuses any key
/// named on it.
/// </summary>
/// <remarks>
/// Signed with the same private key as the licences, so the list cannot be forged or emptied by
/// anyone sitting between the app and GitHub. The header line keeps the two document types apart:
/// a licence can never be served as a revocation list, nor the other way round.
/// Fail-open by design. No internet, GitHub down, a corporate proxy in the way - the app keeps
/// running on the last list it managed to verify. A blocklist that locked people out whenever the
/// network hiccuped would cause far more damage than the copying it prevents.
/// </remarks>
public static class RevocationList
{
	public const string Header = "EVOX-REVOKE-v1";

	private const string RepoOwner = "xbabazibazi";

	private const string RepoName = "AutoVisionNET";

	private const string FileName = "revoked.txt";

	/// <summary>How long a cached list is used before another download is attempted.</summary>
	private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(12.0);

	private static readonly object _lock = new object();

	private static HashSet<string> _revoked = new HashSet<string>(StringComparer.Ordinal);

	private static long _documentIssuedTicks;

	private static bool _refreshing;

	/// <summary>Raised after a refresh that actually changed the list, so callers can re-check.</summary>
	public static event Action Updated;

	public static bool IsRevoked(string licenseId)
	{
		if (string.IsNullOrEmpty(licenseId))
		{
			return false;
		}
		string normalized = ShortCode.Normalize(licenseId);
		lock (_lock)
		{
			return _revoked.Contains(normalized);
		}
	}

	public static int Count
	{
		get
		{
			lock (_lock)
			{
				return _revoked.Count;
			}
		}
	}

	/// <summary>Loads the cached list from the sealed store. Instant, never touches the network.</summary>
	public static void LoadFromCache(ActivationStore store)
	{
		if (store != null && !string.IsNullOrWhiteSpace(store.RevocationDocument))
		{
			TryApply(store.RevocationDocument);
		}
	}

	/// <summary>
	/// Downloads a fresh list in the background. Never awaited by startup: the gate runs on the
	/// cached list so a machine with no internet is not held up at launch.
	/// </summary>
	public static void BeginRefresh(ActivationStore store)
	{
		lock (_lock)
		{
			if (_refreshing)
			{
				return;
			}
			if (store != null && store.RevocationFetchedUtc != DateTime.MinValue
				&& LicenseClock.UtcNow - store.RevocationFetchedUtc < RefreshInterval)
			{
				return;
			}
			_refreshing = true;
		}
		Task.Run(async delegate
		{
			try
			{
				string document = await DownloadAsync().ConfigureAwait(false);
				if (document != null && TryApply(document) && store != null)
				{
					store.RevocationDocument = document;
					store.RevocationFetchedUtc = LicenseClock.UtcNow;
					store.Save();
					Updated?.Invoke();
				}
			}
			catch
			{
			}
			finally
			{
				lock (_lock)
				{
					_refreshing = false;
				}
			}
		});
	}

	private static async Task<string> DownloadAsync()
	{
#if NET48
		// Windows 7 does not negotiate TLS 1.2 unless the process opts in, and GitHub rejects
		// anything older - same opt-in the update checker needs.
		System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
#endif
		using HttpClient client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(15.0);
		client.DefaultRequestHeaders.Add("User-Agent", "EVOX-LicenseCore");
		// The default branch is whichever of these exists; try both rather than hard-coding one.
		foreach (string branch in new[] { "main", "master" })
		{
			try
			{
				string url = $"https://raw.githubusercontent.com/{RepoOwner}/{RepoName}/{branch}/{FileName}";
				HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				}
			}
			catch
			{
			}
		}
		return null;
	}

	/// <summary>
	/// Verifies a document and adopts it when it is newer than the one in force. Returns false
	/// for anything that does not verify, so a truncated download or a captive-portal login page
	/// can never clear the list.
	/// </summary>
	public static bool TryApply(string document)
	{
		if (!TryParse(document, out HashSet<string> ids, out long issuedTicks))
		{
			return false;
		}
		lock (_lock)
		{
			if (issuedTicks < _documentIssuedTicks)
			{
				// An older list is a replay of a document from before a key was revoked.
				return false;
			}
			_revoked = ids;
			_documentIssuedTicks = issuedTicks;
		}
		return true;
	}

	public static bool TryParse(string document, out HashSet<string> ids, out long issuedTicks)
	{
		ids = new HashSet<string>(StringComparer.Ordinal);
		issuedTicks = 0L;
		if (string.IsNullOrWhiteSpace(document))
		{
			return false;
		}
		try
		{
			string[] parts = document.Trim().Split('.');
			if (parts.Length != 2)
			{
				return false;
			}
			byte[] payloadBytes = Convert.FromBase64String(parts[0]);
			byte[] signatureBytes = Convert.FromBase64String(parts[1]);
			if (!LicenseValidator.VerifySignature(payloadBytes, signatureBytes))
			{
				return false;
			}
			string[] lines = Encoding.UTF8.GetString(payloadBytes).Replace("\r\n", "\n").Split('\n');
			if (lines.Length == 0 || lines[0].Trim() != Header)
			{
				return false;
			}
			for (int i = 1; i < lines.Length; i++)
			{
				string line = lines[i].Trim();
				if (line.Length == 0)
				{
					continue;
				}
				if (line.StartsWith("issued=", StringComparison.Ordinal))
				{
					long.TryParse(line.Substring(7), NumberStyles.Integer, CultureInfo.InvariantCulture, out issuedTicks);
					continue;
				}
				string normalized = ShortCode.Normalize(line);
				if (normalized.Length != 0)
				{
					ids.Add(normalized);
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}
}
