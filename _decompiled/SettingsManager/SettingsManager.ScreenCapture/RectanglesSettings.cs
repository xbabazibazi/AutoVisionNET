using System.Collections.Generic;
using FluxDB;
using FluxDB.Models;

namespace SettingsManager.ScreenCapture;

public class RectanglesSettings
{
	private static readonly string[] AreaNames = new string[11]
	{
		"AcceptParty", "Genie", "ChatWindow", "BuffLine", "Weapons", "Inventory",
		"MagicBag", "Town", "Party", "Info", "LeftBotMenu"
	};

	private readonly DbManager _dbManager;

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
