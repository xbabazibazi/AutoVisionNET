using System;
using System.Security.Cryptography;
using System.Text;

namespace LicenseCore;

public static class LicenseValidator
{
	private const string PublicKeyXml = "<RSAKeyValue><Modulus>wJOgXXeFG/b0s3IYoirFwT2UkKFiQszbY09FC9Pg5FnYepEaCd8SZTJ8vUoirLk7qekUhLMi1bFEAIK//JnVXoxFkdgM3t6Aap1dE4E2mnEBJS1yKl/1SOFhsty8dqIdJr/DsaciL+2kbiS2LEWuAm8fqbz8bbzRfNwN3+HQM7rvZ5PiKbbOEJSe6bStDGR3ogF3k906NiaEI+Wgwiou55YkQATp+xJ0OmbGeu7zetrgXajbnug37aGPcKV1Ji1fIGbUJwuYCWrhnyb2hW19QJXtaCf04FTvQk2O4t5h7r/W0CESlTEdWUsLIver/+fzNbItx+e7TLMaiAnaU9xJOQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

	public static bool TryValidate(string licenseText, out LicenseInfo info)
	{
		info = null;
		if (string.IsNullOrWhiteSpace(licenseText))
		{
			return false;
		}
		try
		{
			string[] parts = licenseText.Trim().Split('.');
			if (parts.Length != 2)
			{
				return false;
			}
			byte[] payloadBytes = Convert.FromBase64String(parts[0]);
			byte[] signatureBytes = Convert.FromBase64String(parts[1]);
			using RSA rsa = RSA.Create();
			rsa.FromXmlString(PublicKeyXml);
			if (!rsa.VerifyData(payloadBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
			{
				return false;
			}
			string payload = Encoding.UTF8.GetString(payloadBytes);
			string[] fields = payload.Split('|');
			if (fields.Length != 3)
			{
				return false;
			}
			string customerName = fields[0];
			long issuedTicks = long.Parse(fields[1]);
			long expiresTicks = long.Parse(fields[2]);
			info = new LicenseInfo
			{
				CustomerName = customerName,
				IssuedUtc = new DateTime(issuedTicks, DateTimeKind.Utc),
				ExpiresUtc = new DateTime(expiresTicks, DateTimeKind.Utc)
			};
			return true;
		}
		catch
		{
			return false;
		}
	}
}
