using System;
using FluxDB;

namespace SettingsManager.Macro;

public class TpParty : MacroSettingsBase
{
	protected override string[] Keys => new string[3] { "PartyMemberCount", "TPDelay", "ResistOnTp" };

	public decimal PartyMemberCount
	{
		get
		{
			return GetSetting<decimal>("PartyMemberCount");
		}
		set
		{
			SetSetting("PartyMemberCount", value);
		}
	}

	public decimal TPDelay
	{
		get
		{
			return GetSetting<decimal>("TPDelay");
		}
		set
		{
			SetSetting("TPDelay", value);
		}
	}

	public bool ResistOnTp
	{
		get
		{
			return GetSetting<bool>("ResistOnTp");
		}
		set
		{
			SetSetting("ResistOnTp", value);
		}
	}

	public TpParty(DbManager dbManager)
		: base(dbManager, "TpParty")
	{
	}

	protected override object GetDefaultValue(string key)
	{
		if (1 == 0)
		{
		}
		object result = key switch
		{
			"PartyMemberCount" => 1m, 
			"TPDelay" => 1500m, 
			"ResistOnTp" => false, 
			_ => throw new ArgumentException("Bilinmeyen ayar: " + key), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
