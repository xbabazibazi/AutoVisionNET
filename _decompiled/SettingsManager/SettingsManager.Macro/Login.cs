using System;
using System.Security.Cryptography;
using System.Text;
using FluxDB;

namespace SettingsManager.Macro;

public class Login : MacroSettingsBase
{
	private const string EncryptedPrefix = "ENC1:";

	protected override string[] Keys => new string[2] { "UserID", "UserPassword" };

	public string UserID
	{
		get
		{
			return GetSetting<string>("UserID");
		}
		set
		{
			SetSetting("UserID", value);
		}
	}

	public string UserPassword
	{
		get
		{
			return Decrypt(GetSetting<string>("UserPassword"));
		}
		set
		{
			SetSetting("UserPassword", Encrypt(value));
		}
	}

	private static string Encrypt(string plainText)
	{
		if (string.IsNullOrEmpty(plainText))
		{
			return plainText ?? string.Empty;
		}
		byte[] protectedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), null, DataProtectionScope.CurrentUser);
		return EncryptedPrefix + Convert.ToBase64String(protectedBytes);
	}

	private static string Decrypt(string storedValue)
	{
		if (string.IsNullOrEmpty(storedValue))
		{
			return storedValue ?? string.Empty;
		}
		if (!storedValue.StartsWith(EncryptedPrefix, StringComparison.Ordinal))
		{
			return storedValue;
		}
		try
		{
			byte[] protectedBytes = Convert.FromBase64String(storedValue.Substring(EncryptedPrefix.Length));
			byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
			return Encoding.UTF8.GetString(plainBytes);
		}
		catch (CryptographicException)
		{
			return string.Empty;
		}
	}

	public Login(DbManager dbManager)
		: base(dbManager, "Login")
	{
		MigrateLegacyPlainTextPassword();
	}

	private void MigrateLegacyPlainTextPassword()
	{
		string stored = GetSetting<string>("UserPassword");
		if (!string.IsNullOrEmpty(stored) && !stored.StartsWith(EncryptedPrefix, StringComparison.Ordinal))
		{
			SetSetting("UserPassword", Encrypt(stored));
		}
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		string result;
		if (!(key == "UserID"))
		{
			if (!(key == "UserPassword"))
			{
				throw new ArgumentException("Bilinmeyen ayar: " + key);
			}
			result = "";
		}
		else
		{
			result = "";
		}
		if (1 == 0)
		{
		}
		return result;
	}
}
