using System;
using FluxDB;

namespace SettingsManager.Macro;

public class UndyAc : MacroSettingsBase
{
	protected override string[] Keys => new string[3] { "UndyAcTabDelay", "UndyAcSkillDelay", "UndyAcLoop" };

	public decimal UndyAcTabDelay
	{
		get
		{
			return GetSetting<decimal>("UndyAcTabDelay");
		}
		set
		{
			SetSetting("UndyAcTabDelay", value);
		}
	}

	public decimal UndyAcSkillDelay
	{
		get
		{
			return GetSetting<decimal>("UndyAcSkillDelay");
		}
		set
		{
			SetSetting("UndyAcSkillDelay", value);
		}
	}

	public bool UndyAcLoop
	{
		get
		{
			return GetSetting<bool>("UndyAcLoop");
		}
		set
		{
			SetSetting("UndyAcLoop", value);
		}
	}

	public UndyAc(DbManager dbManager)
		: base(dbManager, "UndyAc")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		object result = key switch
		{
			"UndyAcTabDelay" => 10m, 
			"UndyAcSkillDelay" => 10m, 
			"UndyAcLoop" => false, 
			_ => throw new ArgumentException("Bilinmeyen ayar: " + key), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
