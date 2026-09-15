using System;
using FluxDB;

namespace SettingsManager.ScreenCapture;

public class RepairWeapons : ScreenCaptureSettingsBase
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

	public RepairWeapons(DbManager dbManager)
		: base(dbManager, "RepairWeapons")
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
