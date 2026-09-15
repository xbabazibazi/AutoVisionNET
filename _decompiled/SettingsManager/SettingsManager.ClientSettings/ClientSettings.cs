using System;
using FluxDB;
using SettingsManager.Macro;

namespace SettingsManager.ClientSettings;

public class ClientSettings : MacroSettingsBase
{
	protected override string[] Keys => new string[4] { "ServerIP", "ServerPort", "CharacterNickname", "CharacterJob" };

	public string ServerIP
	{
		get
		{
			return GetSetting<string>("ServerIP");
		}
		set
		{
			SetSetting("ServerIP", value);
		}
	}

	public int ServerPort
	{
		get
		{
			return GetSetting<int>("ServerPort");
		}
		set
		{
			SetSetting("ServerPort", value);
		}
	}

	public string CharacterNickname
	{
		get
		{
			return GetSetting<string>("CharacterNickname");
		}
		set
		{
			SetSetting("CharacterNickname", value);
		}
	}

	public int CharacterJob
	{
		get
		{
			return GetSetting<int>("CharacterJob");
		}
		set
		{
			SetSetting("CharacterJob", value);
		}
	}

	public ClientSettings(DbManager dbManager)
		: base(dbManager, "ClientSettings")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		object result = key switch
		{
			"ServerIP" => "192.168.1.100", 
			"ServerPort" => 5000, 
			"CharacterNickname" => "TempNickName", 
			"CharacterJob" => 1, 
			_ => throw new ArgumentException("Unknown key: " + key), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
