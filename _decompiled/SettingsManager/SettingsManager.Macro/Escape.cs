using System;
using FluxDB;

namespace SettingsManager.Macro;

public class Escape : MacroSettingsBase
{
	protected override string[] Keys => new string[1] { "ESCTime" };

	public decimal ESCTime
	{
		get
		{
			return GetSetting<decimal>("ESCTime");
		}
		set
		{
			SetSetting("ESCTime", value);
		}
	}

	public Escape(DbManager dbManager)
		: base(dbManager, "Esc")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		if (key == "ESCTime")
		{
			decimal num = 1m;
			if (1 == 0)
			{
			}
			return num;
		}
		throw new ArgumentException("Bilinmeyen ayar: " + key);
	}
}
