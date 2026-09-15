using FluxDB;
using FluxDB.Models;

namespace SettingsManager.ScreenCapture;

public class RectanglesSettings
{
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
}
