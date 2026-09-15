using FluxDB;

namespace SettingsManager.GeneralSettingss;

public class GeneralSettings(DbManager dbManager)
{
	public FormSettings FormSettings { get; private set; } = new FormSettings(dbManager);
}
