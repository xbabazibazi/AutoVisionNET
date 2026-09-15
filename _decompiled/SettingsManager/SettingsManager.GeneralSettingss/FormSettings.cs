using System;
using FluxDB;
using SettingsManager.ScreenCapture;

namespace SettingsManager.GeneralSettingss;

public class FormSettings : ScreenCaptureSettingsBase
{
	protected override string[] Keys => new string[2] { "FormLocationX", "FormLocationY" };

	public int FormLocationX
	{
		get
		{
			return GetSetting<int>("FormLocationX");
		}
		set
		{
			SetSetting("FormLocationX", value);
		}
	}

	public int FormLocationY
	{
		get
		{
			return GetSetting<int>("FormLocationY");
		}
		set
		{
			SetSetting("FormLocationY", value);
		}
	}

	public FormSettings(DbManager dbManager)
		: base(dbManager, "FormSettings")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		int num;
		if (!(key == "FormLocationX"))
		{
			if (!(key == "FormLocationY"))
			{
				throw new ArgumentException("Bilinmeyen ayar: " + key);
			}
			num = 0;
		}
		else
		{
			num = 0;
		}
		if (1 == 0)
		{
		}
		return num;
	}
}
