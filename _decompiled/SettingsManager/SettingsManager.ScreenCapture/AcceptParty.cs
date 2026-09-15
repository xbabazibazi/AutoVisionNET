using System;
using FluxDB;

namespace SettingsManager.ScreenCapture;

public class AcceptParty : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[2] { "IsActive", "Alarm" };

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

	public AcceptParty(DbManager dbManager)
		: base(dbManager, "AcceptParty")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		bool flag;
		if (!(key == "IsActive"))
		{
			if (!(key == "Alarm"))
			{
				throw new ArgumentException("Bilinmeyen ayar: " + key);
			}
			flag = false;
		}
		else
		{
			flag = false;
		}
		if (1 == 0)
		{
		}
		return flag;
	}
}
