using System;
using FluxDB;

namespace SettingsManager.Macro;

public class General : MacroSettingsBase
{
	protected override string[] Keys => new string[1] { "StartGenieOnTp" };

	public bool StartGenieOnTp
	{
		get
		{
			return GetSetting<bool>("StartGenieOnTp");
		}
		set
		{
			SetSetting("StartGenieOnTp", value);
		}
	}

	public General(DbManager dbManager)
		: base(dbManager, "General")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		if (key == "StartGenieOnTp")
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
