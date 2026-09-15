using System;
using System.Collections.Generic;
using FluxDB;
using SettingsManager.ClientSettings;
using SettingsManager.GeneralSettingss;
using SettingsManager.Macro;
using SettingsManager.ScreenCapture;

namespace SettingsManager;

public sealed class Settings
{
	private static Settings? _instance;

	private static readonly object _lock = new object();

	public IReadOnlyDictionary<string, DbManager> DbManagers { get; }

	public MacroSettings Macro { get; }

	public ScreenCaptureSettings ScreenCapture { get; }

	public GeneralSettings GeneralSettings { get; }

	public ClientSettingsMain ClientSettings { get; }

	public static Settings Instance => _instance ?? throw new InvalidOperationException("Settings örneği başlatılmadı");

	private Settings(Dictionary<string, DbManager> dbManagers)
	{
		DbManagers = new Dictionary<string, DbManager>(dbManagers);
		Macro = new MacroSettings(dbManagers["Macro"]);
		ScreenCapture = new ScreenCaptureSettings(dbManagers["ScreenCapture"]);
		GeneralSettings = new GeneralSettings(dbManagers["General"]);
		ClientSettings = new ClientSettingsMain(dbManagers["Client"]);
	}

	public static Settings Initialize(Dictionary<string, DbManager> dbManagers)
	{
		lock (_lock)
		{
			return _instance ?? (_instance = new Settings(dbManagers));
		}
	}
}
