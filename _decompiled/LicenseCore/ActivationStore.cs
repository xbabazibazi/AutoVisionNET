using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LicenseCore;

/// <summary>
/// Per-machine activation state: which licence was activated here, the furthest moment the clock
/// has reached, and the last revocation list we managed to download.
/// </summary>
/// <remarks>
/// Sealed with DPAPI at <see cref="DataProtectionScope.LocalMachine"/> scope. That is the whole
/// point of the file: Windows derives the key from this installation, so the bytes are garbage on
/// any other PC. Someone who activates the app and then zips up their whole EVOX folder to pass
/// around hands over something that cannot be opened at the other end - the recipient is back to
/// needing a key of their own. LocalMachine rather than CurrentUser so the state survives the
/// updater relaunching the app elevated or under a different account.
/// </remarks>
public sealed class ActivationStore
{
	private const string FileName = "activation.dat";

	private const string Header = "EVOX-ACT-v1";

	/// <summary>Extra entropy so the blob cannot be unsealed by just any process on the box.</summary>
	private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("EVOX-activation-entropy-v1");

	/// <summary>Licence ID that was activated on this machine, empty when never activated.</summary>
	public string LicenseId { get; set; } = string.Empty;

	/// <summary>Machine code recorded at activation time. Kept for display and diagnostics.</summary>
	public string MachineCode { get; set; } = string.Empty;

	public DateTime ClaimedUtc { get; set; } = DateTime.MinValue;

	/// <summary>Furthest point in time ever observed - the floor for <see cref="LicenseClock"/>.</summary>
	public DateTime LastSeenUtc { get; set; } = DateTime.MinValue;

	/// <summary>Raw text of the last revocation list that verified, cached for offline runs.</summary>
	public string RevocationDocument { get; set; } = string.Empty;

	public DateTime RevocationFetchedUtc { get; set; } = DateTime.MinValue;

	public bool HasClaim => !string.IsNullOrEmpty(LicenseId);

	public static IEnumerable<string> GetPaths()
	{
		foreach (string licensePath in LicenseGate.GetLicenseStorePaths())
		{
			string folder = Path.GetDirectoryName(licensePath);
			if (!string.IsNullOrEmpty(folder))
			{
				yield return Path.Combine(folder, FileName);
			}
		}
	}

	/// <summary>
	/// Reads the first store that unseals. A file that fails to unseal is not an error worth
	/// surfacing - it is exactly what a copied folder looks like, and it simply means "no claim".
	/// </summary>
	public static ActivationStore Load()
	{
		foreach (string path in GetPaths())
		{
			try
			{
				if (!File.Exists(path))
				{
					continue;
				}
				byte[] sealedBytes = File.ReadAllBytes(path);
				byte[] plainBytes = ProtectedData.Unprotect(sealedBytes, Entropy, DataProtectionScope.LocalMachine);
				ActivationStore store = Parse(Encoding.UTF8.GetString(plainBytes));
				if (store != null)
				{
					return store;
				}
			}
			catch
			{
			}
		}
		return new ActivationStore();
	}

	public bool Save()
	{
		byte[] sealedBytes;
		try
		{
			byte[] plainBytes = Encoding.UTF8.GetBytes(Serialize());
			sealedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.LocalMachine);
		}
		catch
		{
			return false;
		}
		bool written = false;
		foreach (string path in GetPaths())
		{
			try
			{
				string folder = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(folder))
				{
					Directory.CreateDirectory(folder);
				}
				File.WriteAllBytes(path, sealedBytes);
				written = true;
			}
			catch
			{
			}
		}
		return written;
	}

	// Hand-rolled key=value text rather than a JSON serializer: this assembly also builds for
	// net48, where System.Text.Json is not in the box, and the payload is four scalars.
	private string Serialize()
	{
		StringBuilder builder = new StringBuilder();
		builder.Append(Header).Append('\n');
		builder.Append("license=").Append(LicenseId).Append('\n');
		builder.Append("machine=").Append(MachineCode).Append('\n');
		builder.Append("claimed=").Append(ClaimedUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append('\n');
		builder.Append("lastseen=").Append(LastSeenUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append('\n');
		builder.Append("revfetched=").Append(RevocationFetchedUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append('\n');
		// Last, and base64'd, so newlines inside the signed document cannot break the parse.
		builder.Append("revdoc=").Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(RevocationDocument ?? string.Empty))).Append('\n');
		return builder.ToString();
	}

	private static ActivationStore Parse(string text)
	{
		string[] lines = text.Replace("\r\n", "\n").Split('\n');
		if (lines.Length == 0 || lines[0].Trim() != Header)
		{
			return null;
		}
		ActivationStore store = new ActivationStore();
		for (int i = 1; i < lines.Length; i++)
		{
			int separator = lines[i].IndexOf('=');
			if (separator <= 0)
			{
				continue;
			}
			string key = lines[i].Substring(0, separator);
			string value = lines[i].Substring(separator + 1);
			switch (key)
			{
				case "license":
					store.LicenseId = value;
					break;
				case "machine":
					store.MachineCode = value;
					break;
				case "claimed":
					store.ClaimedUtc = ParseUtc(value);
					break;
				case "lastseen":
					store.LastSeenUtc = ParseUtc(value);
					break;
				case "revfetched":
					store.RevocationFetchedUtc = ParseUtc(value);
					break;
				case "revdoc":
					try
					{
						store.RevocationDocument = Encoding.UTF8.GetString(Convert.FromBase64String(value));
					}
					catch
					{
						store.RevocationDocument = string.Empty;
					}
					break;
			}
		}
		return store;
	}

	private static DateTime ParseUtc(string ticksText)
	{
		if (long.TryParse(ticksText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks)
			&& ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks)
		{
			return new DateTime(ticks, DateTimeKind.Utc);
		}
		return DateTime.MinValue;
	}
}
