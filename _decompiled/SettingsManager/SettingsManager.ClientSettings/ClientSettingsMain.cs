using FluxDB;

namespace SettingsManager.ClientSettings;

public class ClientSettingsMain(DbManager dbManager)
{
	public ClientSettings ClientSettings { get; private set; } = new ClientSettings(dbManager);
}
