using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace LicenseCore;

/// <summary>
/// Derives a short, stable, human-dictatable code that identifies this Windows installation.
/// </summary>
/// <remarks>
/// Stability is the overriding requirement: a code that drifts locks a paying customer out of
/// software that is working fine, which is worse than the copying it would prevent. So the code
/// comes from MachineGuid alone whenever that is readable. Windows Setup writes MachineGuid once
/// and nothing afterwards changes it - not app updates, not driver or hardware changes, not a new
/// GPU or disk, not Windows Update. Only a Windows reinstall does.
/// Deliberately NOT used: MAC addresses (Tailscale/VPN/USB adapters would invalidate a licence
/// mid-session), the system volume serial as the primary ingredient (a disk format or re-image
/// would break an otherwise fine licence), and WMI (seconds of startup cost on Win7 guests, and
/// board/BIOS serials read back as "To be filled by O.E.M." inside VMs).
/// Worth knowing: two VMs cloned from one image share a MachineGuid and so share a code.
/// </remarks>
public static class MachineId
{
	/// <summary>
	/// Salt for the hash. This must NEVER change: every machine-bound licence ever issued is tied
	/// to the code this salt produces, so bumping it would invalidate all of them at once.
	/// </summary>
	private const string AlgorithmSalt = "EVOX-MID-v1";

	private const int CodeLength = 15;

	private const int GroupSize = 5;

	private const string CacheFileName = "machine.id";

	private static readonly Lazy<string> _current = new Lazy<string>(Compute);

	/// <summary>This machine's code, formatted as XXXXX-XXXXX-XXXXX. Computed once per process.</summary>
	public static string Current => _current.Value;

	/// <summary>True when <paramref name="candidate"/> names this machine, ignoring formatting.</summary>
	public static bool Matches(string candidate)
	{
		return ShortCode.Equal(candidate, Current);
	}

	private static string Compute()
	{
		string machineGuid = ReadMachineGuid();
		if (machineGuid.Length != 0)
		{
			string code = Derive("G:" + machineGuid.ToUpperInvariant());
			WriteCachedCode(code);
			return code;
		}
		// MachineGuid unreadable. Before falling back to a weaker identity, reuse the code this
		// machine was already known by - otherwise one locked-down boot would silently change the
		// machine's identity. The cache is only ever consulted on this path, so copying machine.id
		// to another PC buys nothing: that PC reads its own MachineGuid and never opens the file.
		string cached = ReadCachedCode();
		if (cached.Length != 0)
		{
			return cached;
		}
		string fallback = Derive("F:" + ReadSystemVolumeSerial() + "|" + Environment.MachineName.ToUpperInvariant());
		WriteCachedCode(fallback);
		return fallback;
	}

	private static string Derive(string seed)
	{
		return ShortCode.Format(ShortCode.Hash(AlgorithmSalt + "|" + seed, CodeLength), GroupSize);
	}

	private static string ReadMachineGuid()
	{
		// Try every view: on 64-bit Windows the value lives in the 64-bit hive while this process
		// may be 32-bit, and on 32-bit Windows the Registry64 view yields nothing at all.
		foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32, RegistryView.Default })
		{
			try
			{
				using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
				using RegistryKey key = baseKey?.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography");
				if (key?.GetValue("MachineGuid") is string value && !string.IsNullOrWhiteSpace(value))
				{
					return value.Trim();
				}
			}
			catch
			{
			}
		}
		return string.Empty;
	}

	private static string ReadSystemVolumeSerial()
	{
		try
		{
			string root = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
			if (string.IsNullOrEmpty(root))
			{
				return string.Empty;
			}
			StringBuilder volumeName = new StringBuilder(261);
			StringBuilder fileSystemName = new StringBuilder(261);
			if (GetVolumeInformation(root, volumeName, volumeName.Capacity, out uint serial, out uint _, out uint _, fileSystemName, fileSystemName.Capacity))
			{
				return serial.ToString("X8", CultureInfo.InvariantCulture);
			}
		}
		catch
		{
		}
		return string.Empty;
	}

	/// <summary>Cache locations mirror the licence store so one unwritable folder is not fatal.</summary>
	private static IEnumerable<string> GetCachePaths()
	{
		foreach (string licensePath in LicenseGate.GetLicenseStorePaths())
		{
			string folder = Path.GetDirectoryName(licensePath);
			if (!string.IsNullOrEmpty(folder))
			{
				yield return Path.Combine(folder, CacheFileName);
			}
		}
	}

	private static string ReadCachedCode()
	{
		foreach (string path in GetCachePaths())
		{
			try
			{
				if (File.Exists(path))
				{
					string normalized = ShortCode.Normalize(File.ReadAllText(path));
					if (normalized.Length == CodeLength)
					{
						return ShortCode.Format(normalized, GroupSize);
					}
				}
			}
			catch
			{
			}
		}
		return string.Empty;
	}

	private static void WriteCachedCode(string code)
	{
		foreach (string path in GetCachePaths())
		{
			try
			{
				string folder = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(folder))
				{
					Directory.CreateDirectory(folder);
				}
				if (!File.Exists(path) || !ShortCode.Equal(File.ReadAllText(path), code))
				{
					File.WriteAllText(path, code);
				}
			}
			catch
			{
			}
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetVolumeInformation(
		string rootPathName,
		StringBuilder volumeNameBuffer,
		int volumeNameSize,
		out uint volumeSerialNumber,
		out uint maximumComponentLength,
		out uint fileSystemFlags,
		StringBuilder fileSystemNameBuffer,
		int fileSystemNameSize);
}
