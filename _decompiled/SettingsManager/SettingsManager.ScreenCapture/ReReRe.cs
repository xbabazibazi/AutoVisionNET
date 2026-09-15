using System;
using FluxDB;

namespace SettingsManager.ScreenCapture;

public class ReReRe : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[5] { "IsActive", "ReReReUndy", "ReReRe300Ac", "ReReReWolf", "ReReReSw" };

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

	public bool ReReReUndy
	{
		get
		{
			return GetSetting<bool>("ReReReUndy");
		}
		set
		{
			SetSetting("ReReReUndy", value);
		}
	}

	public bool ReReRe300Ac
	{
		get
		{
			return GetSetting<bool>("ReReRe300Ac");
		}
		set
		{
			SetSetting("ReReRe300Ac", value);
		}
	}

	public bool ReReReWolf
	{
		get
		{
			return GetSetting<bool>("ReReReWolf");
		}
		set
		{
			SetSetting("ReReReWolf", value);
		}
	}

	public bool ReReReSw
	{
		get
		{
			return GetSetting<bool>("ReReReSw");
		}
		set
		{
			SetSetting("ReReReSw", value);
		}
	}

	public ReReRe(DbManager dbManager)
		: base(dbManager, "ReReRe")
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
			"ReReReUndy" => false, 
			"ReReRe300Ac" => false, 
			"ReReReWolf" => false, 
			"ReReReSw" => false, 
			_ => throw new ArgumentException("Bilinmeyen ayar: " + key), 
		};
		if (1 == 0)
		{
		}
		return flag;
	}
}
