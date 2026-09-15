using System;
using FluxDB;

namespace SettingsManager.ScreenCapture;

public class PartyMemberCount : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[4] { "IsActive", "PartyCount", "BreakParty", "Alarm" };

	public bool IsActive
	{
		get
		{
			return GetSetting<bool>("IsActive");
		}
		set
		{
			SetSetting("IsActive", value);
		}
	}

	public int PartyCount
	{
		get
		{
			return GetSetting<int>("PartyCount");
		}
		set
		{
			SetSetting("PartyCount", value);
		}
	}

	public bool BreakParty
	{
		get
		{
			return GetSetting<bool>("BreakParty");
		}
		set
		{
			SetSetting("BreakParty", value);
		}
	}

	public bool Alarm
	{
		get
		{
			return GetSetting<bool>("Alarm");
		}
		set
		{
			SetSetting("Alarm", value);
		}
	}

	public PartyMemberCount(DbManager dbManager)
		: base(dbManager, "PartyMemberCount")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		object result = key switch
		{
			"IsActive" => false, 
			"PartyCount" => 1, 
			"BreakParty" => false, 
			"Alarm" => false, 
			_ => throw new ArgumentException("Bilinmeyen ayar: " + key), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
