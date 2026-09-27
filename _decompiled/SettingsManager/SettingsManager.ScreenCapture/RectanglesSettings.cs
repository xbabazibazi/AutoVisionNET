using System.Collections.Generic;
using EVOX.Data;
using EVOX.Data.Models;

namespace SettingsManager.ScreenCapture;

public class RectanglesSettings
{
	private static readonly string[] AreaNames = new string[13]
	{
		"AcceptParty", "Genie", "ChatWindow", "BuffLine", "Weapons", "Inventory",
		"MagicBag", "Town", "Party", "Info", "LeftBotMenu", "WeaponInventory", "GenieStatus"
	};

	private readonly DbManager _dbManager;

	/// <summary>
	/// Where the weapon swap looks for a replacement in the bag. Separate from <see cref="Inventory"/>
	/// so the search can be narrowed to the rows the spare weapons live in without also narrowing
	/// the empty-slot count, which needs to see the whole bag. Left unset it falls back to
	/// <see cref="Inventory"/>.
	/// </summary>
	public RectangleSettings WeaponInventory
	{
		get
		{
			return Get("WeaponInventory");
		}
		set
		{
			Set(value, "WeaponInventory");
		}
	}

	public RectangleSettings AcceptParty
	{
		get
		{
			return Get("AcceptParty");
		}
		set
		{
			Set(value, "AcceptParty");
		}
	}

	public RectangleSettings Genie
	{
		get
		{
			return Get("Genie");
		}
		set
		{
			Set(value, "Genie");
		}
	}

	/// <summary>
	/// Where the "Genie is active" status icon appears - separate from <see cref="Genie"/> (the
	/// start button's area), since the icon and the button don't sit at the same screen location.
	/// </summary>
	public RectangleSettings GenieStatus
	{
		get
		{
			return Get("GenieStatus");
		}
		set
		{
			Set(value, "GenieStatus");
		}
	}

	public RectangleSettings ChatWindow
	{
		get
		{
			return Get("ChatWindow");
		}
		set
		{
			Set(value, "ChatWindow");
		}
	}

	public RectangleSettings BuffLine
	{
		get
		{
			return Get("BuffLine");
		}
		set
		{
			Set(value, "BuffLine");
		}
	}

	public RectangleSettings Weapons
	{
		get
		{
			return Get("Weapons");
		}
		set
		{
			Set(value, "Weapons");
		}
	}

	public RectangleSettings Inventory
	{
		get
		{
			return Get("Inventory");
		}
		set
		{
			Set(value, "Inventory");
		}
	}

	public RectangleSettings MagicBag
	{
		get
		{
			return Get("MagicBag");
		}
		set
		{
			Set(value, "MagicBag");
		}
	}

	public RectangleSettings Town
	{
		get
		{
			return Get("Town");
		}
		set
		{
			Set(value, "Town");
		}
	}

	public RectangleSettings Party
	{
		get
		{
			return Get("Party");
		}
		set
		{
			Set(value, "Party");
		}
	}

	public RectangleSettings Info
	{
		get
		{
			return Get("Info");
		}
		set
		{
			Set(value, "Info");
		}
	}

	public RectangleSettings LeftBotMenu
	{
		get
		{
			return Get("LeftBotMenu");
		}
		set
		{
			Set(value, "LeftBotMenu");
		}
	}

	public RectanglesSettings(DbManager dbManager)
	{
		_dbManager = dbManager;
	}

	private RectangleSettings Get(string name)
	{
		return _dbManager.GetRectangleSettings(name);
	}

	private void Set(RectangleSettings settings, string expectedName)
	{
		if (settings.Name != expectedName)
		{
			settings.Name = expectedName;
		}
		_dbManager.SetRectangleSettings(settings);
	}

	public string ActiveProfileName
	{
		get
		{
			return _dbManager.GetSetting<string>("ResolutionProfileMeta", "ActiveProfile");
		}
		set
		{
			_dbManager.SetSetting("ResolutionProfileMeta", "ActiveProfile", value);
		}
	}

	public List<string> ListProfiles()
	{
		return _dbManager.ListResolutionProfiles();
	}

	public void SaveAsProfile(string profileName)
	{
		foreach (string areaName in AreaNames)
		{
			_dbManager.SaveAreaToProfile(profileName, Get(areaName));
		}
		ActiveProfileName = profileName;
	}

	public bool LoadProfile(string profileName)
	{
		bool anyApplied = false;
		foreach (string areaName in AreaNames)
		{
			RectangleSettings saved = _dbManager.GetAreaFromProfile(profileName, areaName);
			if (saved != null)
			{
				Set(saved, areaName);
				anyApplied = true;
			}
		}
		if (anyApplied)
		{
			ActiveProfileName = profileName;
		}
		return anyApplied;
	}

	public void DeleteProfile(string profileName)
	{
		_dbManager.DeleteResolutionProfile(profileName);
	}
}
