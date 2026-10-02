using EVOX.Data;

namespace SettingsManager.Macro;

public class Attack : MacroSettingsBase
{
	protected override string[] Keys => new string[10] { "Delay", "StartAttackOnGenieStart", "HasSkill", "HasZ", "HasR", "HasEight", "HasNine", "RDelay", "IsRandomDelay", "RRepeatCount" };

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

	/// <summary>
	/// How many times R is pressed back to back each cycle, before waiting out
	/// <see cref="RDelay"/> again - so a value of 3 produces "RRR ... RRR ..." rather than a
	/// single R per interval. 1 keeps the original one-press-per-cycle behaviour.
	/// </summary>
	public int RRepeatCount
	{
		get
		{
			int count = GetSetting<int>("RRepeatCount");
			return (count < 1) ? 1 : count;
		}
		set
		{
			SetSetting("RRepeatCount", value);
		}
	}

	public Attack(DbManager dbManager)
		: base(dbManager, "Attack")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		return key switch
		{
			"Delay" => 800m,
			"RDelay" => 800m,
			"RRepeatCount" => 1,
			_ => false,
		};
	}
}
