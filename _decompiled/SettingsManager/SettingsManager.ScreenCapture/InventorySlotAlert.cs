using System;
using EVOX.Data;

namespace SettingsManager.ScreenCapture;

public class InventorySlotAlert : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[3] { "IsActive", "LowSlotThreshold", "OnlyWhenGenieActive" };

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

	/// <summary>
	/// Only raise the inventory alarm while Genie is running. Repairing by hand moves the
	/// inventory window, so the slot count read during a repair describes whatever is now in
	/// that region rather than the bag - which set the alarm off every time. Genie running is a
	/// good stand-in for "actually farming, so this reading means something".
	/// </summary>
	public bool OnlyWhenGenieActive
	{
		get
		{
			return GetSetting<bool>("OnlyWhenGenieActive");
		}
		set
		{
			SetSetting("OnlyWhenGenieActive", value);
		}
	}

	public InventorySlotAlert(DbManager dbManager)
		: base(dbManager, "InventorySlotAlert")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		return key switch
		{
			"IsActive" => false,
			"LowSlotThreshold" => 5,
			"OnlyWhenGenieActive" => true,
			_ => throw new ArgumentException("Bilinmeyen ayar: " + key),
		};
	}
}
