using EVOX.Data;

namespace SettingsManager.ClientSettings;

public class ClientSettingsMain(DbManager dbManager)
{
	public ClientSettings ClientSettings { get; private set; } = new ClientSettings(dbManager);
}
