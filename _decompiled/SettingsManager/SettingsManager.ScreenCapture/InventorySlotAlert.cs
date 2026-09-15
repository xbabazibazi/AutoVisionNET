using System;
using FluxDB;

namespace SettingsManager.ScreenCapture;

public class InventorySlotAlert : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[2] { "IsActive", "LowSlotThreshold" };

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

	public int LowSlotThreshold
	{
		get
		{
			return GetSetting<int>("LowSlotThreshold");
		}
		set
		{
			SetSetting("LowSlotThreshold", value);
		}
	}

	public InventorySlotAlert(DbManager dbManager)
		: base(dbManager, "InventorySlotAlert")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		object result;
		if (!(key == "IsActive"))
		{
			if (!(key == "LowSlotThreshold"))
			{
				throw new ArgumentException("Bilinmeyen ayar: " + key);
			}
			result = 5;
		}
		else
		{
			result = false;
		}
		if (1 == 0)
		{
		}
		return result;
	}
}
