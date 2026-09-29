using EVOX.Data;

namespace SettingsManager.GeneralSettingss;

public class GeneralSettings(DbManager dbManager)
{
	public FormSettings FormSettings { get; private set; } = new FormSettings(dbManager);

	public Telegram Telegram { get; private set; } = new Telegram(dbManager);
}
