using FluxDB;

namespace SettingsManager.Macro;

public class Attack : MacroSettingsBase
{
	protected override string[] Keys => new string[9] { "Delay", "StartAttackOnGenieStart", "HasSkill", "HasZ", "HasR", "HasEight", "HasNine", "RDelay", "IsRandomDelay" };

	public decimal Delay
	{
		get
		{
			return GetSetting<decimal>("Delay");
		}
		set
		{
			SetSetting("Delay", value);
		}
	}

	public bool StartAttackOnGenieStart
	{
		get
		{
			return GetSetting<bool>("StartAttackOnGenieStart");
		}
		set
		{
			SetSetting("StartAttackOnGenieStart", value);
		}
	}

	public bool HasSkill
	{
		get
		{
			return GetSetting<bool>("HasSkill");
		}
		set
		{
			SetSetting("HasSkill", value);
		}
	}

	public bool HasZ
	{
		get
		{
			return GetSetting<bool>("HasZ");
		}
		set
		{
			SetSetting("HasZ", value);
		}
	}

	public bool HasR
	{
		get
		{
			return GetSetting<bool>("HasR");
		}
		set
		{
			SetSetting("HasR", value);
		}
	}

	public bool HasEight
	{
		get
		{
			return GetSetting<bool>("HasEight");
		}
		set
		{
			SetSetting("HasEight", value);
		}
	}

	public bool HasNine
	{
		get
		{
			return GetSetting<bool>("HasNine");
		}
		set
		{
			SetSetting("HasNine", value);
		}
	}

	public decimal RDelay
	{
		get
		{
			return GetSetting<decimal>("RDelay");
		}
		set
		{
			SetSetting("RDelay", value);
		}
	}

	public bool IsRandomDelay
	{
		get
		{
			return GetSetting<bool>("IsRandomDelay");
		}
		set
		{
			SetSetting("IsRandomDelay", value);
		}
	}

	public Attack(DbManager dbManager)
		: base(dbManager, "Attack")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		object result = ((key == "Delay") ? ((object)800m) : ((!(key == "RDelay")) ? ((object)false) : ((object)800m)));
		if (1 == 0)
		{
		}
		return result;
	}
}
