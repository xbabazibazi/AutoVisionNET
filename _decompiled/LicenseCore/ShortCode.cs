using System.Security.Cryptography;
using System.Text;

namespace LicenseCore;

/// <summary>
/// Short identifiers that a person can read off a screen and type back without ambiguity:
/// machine codes and licence IDs. Crockford Base32 - the alphabet omits I, L, O and U, and
/// decoding folds the look-alikes back, so "0" cannot be mistaken for "O" nor "1" for "I".
/// </summary>
public static class ShortCode
{
	public const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

	/// <summary>Strips formatting and folds look-alike characters to their canonical form.</summary>
	public static string Normalize(string code)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			return string.Empty;
		}
		StringBuilder builder = new StringBuilder(code.Length);
		foreach (char raw in code)
		{
			char c = char.ToUpperInvariant(raw);
			switch (c)
			{
				case 'I':
				case 'L':
					c = '1';
					break;
				case 'O':
					c = '0';
					break;
				case 'U':
					c = 'V';
					break;
			}
			if (Alphabet.IndexOf(c) >= 0)
			{
				builder.Append(c);
			}
		}
		return builder.ToString();
	}

	/// <summary>Groups a normalized code with dashes, e.g. XXXXX-XXXXX-XXXXX.</summary>
	public static string Format(string code, int groupSize)
	{
		string normalized = Normalize(code);
		if (normalized.Length == 0 || groupSize <= 0)
		{
			return normalized;
		}
		StringBuilder builder = new StringBuilder(normalized.Length + normalized.Length / groupSize);
		for (int i = 0; i < normalized.Length; i++)
		{
			if (i > 0 && i % groupSize == 0)
			{
				builder.Append('-');
			}
			builder.Append(normalized[i]);
		}
		return builder.ToString();
	}

	public static bool Equal(string left, string right)
	{
		string a = Normalize(left);
		return a.Length != 0 && a == Normalize(right);
	}

	/// <summary>SHA-256 of <paramref name="seed"/>, rendered as the first N Base32 characters.</summary>
	public static string Hash(string seed, int characters)
	{
		using SHA256 sha = SHA256.Create();
		return Encode(sha.ComputeHash(Encoding.UTF8.GetBytes(seed)), characters);
	}

	public static string Encode(byte[] bytes, int characters)
	{
		StringBuilder builder = new StringBuilder(characters);
		int bitBuffer = 0;
		int bitCount = 0;
		foreach (byte b in bytes)
		{
			bitBuffer = (bitBuffer << 8) | b;
			bitCount += 8;
			while (bitCount >= 5 && builder.Length < characters)
			{
				bitCount -= 5;
				builder.Append(Alphabet[(bitBuffer >> bitCount) & 0x1F]);
			}
			if (builder.Length >= characters)
			{
				break;
			}
		}
		return builder.ToString();
	}
}
