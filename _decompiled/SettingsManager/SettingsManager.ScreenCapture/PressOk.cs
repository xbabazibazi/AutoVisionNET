using System;
using EVOX.Data;

namespace SettingsManager.ScreenCapture;

public class PressOk : ScreenCaptureSettingsBase
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

	public PressOk(DbManager dbManager)
		: base(dbManager, "PressOk")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		return key switch
		{
			"IsActive" => false,
			"Alarm" => false,
			_ => throw new ArgumentException("Bilinmeyen ayar: " + key),
		};
	}
}
