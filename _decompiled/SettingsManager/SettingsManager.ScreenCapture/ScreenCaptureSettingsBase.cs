using System;
using System.Collections.Generic;
using System.Globalization;
using EVOX.Data;
using SimpleLogger;

namespace SettingsManager.ScreenCapture;

public abstract class ScreenCaptureSettingsBase
{
	protected readonly DbManager _dbManager;

	protected readonly Dictionary<string, object> _settingsCache = new Dictionary<string, object>();

	protected readonly string TableName;

	private readonly Logger _logger;

	protected abstract string[] Keys { get; }

	protected ScreenCaptureSettingsBase(DbManager dbManager, string tableName)
	{
		_dbManager = dbManager ?? throw new ArgumentNullException("dbManager");
		TableName = tableName ?? throw new ArgumentNullException("tableName");
		LoadInitialSettings();
		_logger = Logger.Instance;
	}

	protected abstract object GetDefaultValue(string key);

	protected void LoadInitialSettings()
	{
		string[] keys = Keys;
		int seeded = 0;
		foreach (string key in keys)
		{
			object stored = _dbManager.GetSetting<object>(TableName, key);
			_settingsCache[key] = stored ?? GetDefaultValue(key);
			// Only keys that are genuinely absent get written. The write used to be
			// unconditional, so every launch rewrote every key with the value it already held -
			// around fifty pointless writes across the settings groups before the window even
			// appeared. It matters more than the milliseconds suggest when several VMs on one
			// host all start up against the same disk.
			if (stored == null)
			{
				_dbManager.SetSetting(TableName, key, _settingsCache[key]?.ToString() ?? string.Empty);
				seeded++;
			}
		}
		Console.WriteLine($"Ayarlar yüklendi ({keys.Length} anahtar, {seeded} tanesi ilk kez yazıldı).");
	}

	protected T GetSetting<T>(string key)
	{
		if (_settingsCache.TryGetValue(key, out object value))
		{
			try
			{
				return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Hata: {key} için {typeof(T)} dönüşümü başarısız. Varsayılan değer döndürülüyor. Hata: {ex.Message}");
				return (T)GetDefaultValue(key);
			}
		}
		throw new KeyNotFoundException("Ayar bulunamadı: " + key);
	}

	protected void SetSetting<T>(string key, T value)
	{
		_settingsCache[key] = value;
		DbManager dbManager = _dbManager;
		string tableName = TableName;
		object obj = value?.ToString();
		if (obj == null)
		{
			obj = string.Empty;
		}
		dbManager.SetSetting(tableName, key, (string)obj);
		Console.WriteLine($"Ayar güncellendi ve kaydedildi: {key} = {value}");
	}
}
