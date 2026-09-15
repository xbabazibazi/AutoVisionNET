using FluxDB;

namespace SettingsManager;

public static class TemplateResolver
{
	private const string TableName = "Templates";

	private static DbManager Db => Settings.Instance.DbManagers["General"];

	public static string Resolve(string taskId, string defaultRelativePath)
	{
		string overridePath = Db.GetSetting<string>(TableName, taskId);
		return string.IsNullOrWhiteSpace(overridePath) ? defaultRelativePath : overridePath;
	}

	public static void SetOverride(string taskId, string filePath)
	{
		Db.SetSetting(TableName, taskId, filePath);
	}

	public static void ClearOverride(string taskId, string defaultRelativePath)
	{
		Db.SetSetting(TableName, taskId, defaultRelativePath);
	}
}
