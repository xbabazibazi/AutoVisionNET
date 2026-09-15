using System;
using FluxDB;

namespace SettingsManager.ScreenCapture;

public class StopMacrosOnGenieStop : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[1] { "IsActive" };

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

	public StopMacrosOnGenieStop(DbManager dbManager)
		: base(dbManager, "StopMacrosOnGenieStop")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		if (key == "IsActive")
		{
			bool flag = false;
			if (1 == 0)
			{
			}
			return flag;
		}
		throw new ArgumentException("Bilinmeyen ayar: " + key);
	}
}
