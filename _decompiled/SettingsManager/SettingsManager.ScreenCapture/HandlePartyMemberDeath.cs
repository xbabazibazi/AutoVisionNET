using System;
using FluxDB;

namespace SettingsManager.ScreenCapture;

public class HandlePartyMemberDeath : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[3] { "IsActive", "Alarm", "BreakParty" };

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

	public HandlePartyMemberDeath(DbManager dbManager)
		: base(dbManager, "HandlePartyMemberDeath")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		bool flag = key switch
		{
			"IsActive" => false, 
			"Alarm" => false, 
			"BreakParty" => false, 
			_ => throw new ArgumentException("Bilinmeyen ayar: " + key), 
		};
		if (1 == 0)
		{
		}
		return flag;
	}
}
