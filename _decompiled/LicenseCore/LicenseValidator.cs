using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LicenseCore;

public static class LicenseValidator
{
	internal const string PublicKeyXml = "<RSAKeyValue><Modulus>wJOgXXeFG/b0s3IYoirFwT2UkKFiQszbY09FC9Pg5FnYepEaCd8SZTJ8vUoirLk7qekUhLMi1bFEAIK//JnVXoxFkdgM3t6Aap1dE4E2mnEBJS1yKl/1SOFhsty8dqIdJr/DsaciL+2kbiS2LEWuAm8fqbz8bbzRfNwN3+HQM7rvZ5PiKbbOEJSe6bStDGR3ogF3k906NiaEI+Wgwiou55YkQATp+xJ0OmbGeu7zetrgXajbnug37aGPcKV1Ji1fIGbUJwuYCWrhnyb2hW19QJXtaCf04FTvQk2O4t5h7r/W0CESlTEdWUsLIver/+fzNbItx+e7TLMaiAnaU9xJOQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

	/// <summary>Length of the short licence ID used by the revocation list.</summary>
	public const int LicenseIdLength = 10;

	/// <summary>Separates several machine codes inside one licence payload.</summary>
	public const char MachineIdSeparator = ';';

	/// <summary>
	/// Checks the signature and parses the payload. Does NOT consider expiry, machine binding or
	/// revocation - those are policy and belong to <see cref="LicenseGate"/>.
	/// </summary>
	public static bool TryValidate(string licenseText, out LicenseInfo info)
	{
		info = null;
		if (string.IsNullOrWhiteSpace(licenseText))
		{
			return false;
		}
		try
		{
			string trimmed = licenseText.Trim();
			string[] parts = trimmed.Split('.');
			if (parts.Length != 2)
			{
				return false;
			}
			byte[] payloadBytes = Convert.FromBase64String(parts[0]);
			byte[] signatureBytes = Convert.FromBase64String(parts[1]);
			if (!VerifySignature(payloadBytes, signatureBytes))
			{
				return false;
			}
			string payload = Encoding.UTF8.GetString(payloadBytes);
			string[] fields = payload.Split('|');
			// 3 fields: the original, unbound format. 4 fields: with machine binding. Both stay
			// valid so keys already issued keep working for the rest of their term.
			if (fields.Length != 3 && fields.Length != 4)
			{
				return false;
			}
			List<string> machineIds = new List<string>();
			if (fields.Length == 4)
			{
				foreach (string raw in fields[3].Split(MachineIdSeparator))
				{
					string normalized = ShortCode.Normalize(raw);
					if (normalized.Length != 0)
					{
						machineIds.Add(normalized);
					}
				}
			}
			info = new LicenseInfo
			{
				CustomerName = fields[0],
				IssuedUtc = new DateTime(long.Parse(fields[1]), DateTimeKind.Utc),
				ExpiresUtc = new DateTime(long.Parse(fields[2]), DateTimeKind.Utc),
				MachineIds = machineIds,
				LicenseId = ComputeLicenseId(trimmed)
			};
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// The identifier a key is known by on the revocation list. Derived from the key text itself
	/// so the generator and every client arrive at the same value with nothing to look up.
	/// </summary>
	public static string ComputeLicenseId(string licenseText)
	{
		if (string.IsNullOrWhiteSpace(licenseText))
		{
			return string.Empty;
		}
		return ShortCode.Hash("EVOX-LID-v1|" + licenseText.Trim(), LicenseIdLength);
	}

	internal static bool VerifySignature(byte[] payloadBytes, byte[] signatureBytes)
	{
		try
		{
			using RSA rsa = RSA.Create();
			rsa.FromXmlString(PublicKeyXml);
			return rsa.VerifyData(payloadBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		}
		catch
		{
			return false;
		}
	}
}
