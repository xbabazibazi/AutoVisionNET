using System;
using FluxDB;

namespace SettingsManager.Macro;

public class Login : MacroSettingsBase
{
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
			return GetSetting<string>("UserPassword");
		}
		set
		{
			SetSetting("UserPassword", value);
		}
	}

	public Login(DbManager dbManager)
		: base(dbManager, "Login")
	{
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
