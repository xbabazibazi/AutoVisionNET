using System;
using System.Collections.Generic;
using System.Globalization;
using FluxDB;

namespace SettingsManager.Macro;

public abstract class MacroSettingsBase
{
	protected readonly DbManager _dbManager;

	protected readonly Dictionary<string, object> _settingsCache = new Dictionary<string, object>();

	protected readonly string TableName;

	protected abstract string[] Keys { get; }

	protected MacroSettingsBase(DbManager dbManager, string tableName)
	{
		_dbManager = dbManager ?? throw new ArgumentNullException("dbManager");
		TableName = tableName ?? throw new ArgumentNullException("tableName");
		LoadInitialSettings();
	}

	protected abstract object GetDefaultValue(string key);

	protected void LoadInitialSettings()
	{
		string[] keys = Keys;
		foreach (string key in keys)
		{
			_settingsCache[key] = _dbManager.GetSetting<object>(TableName, key) ?? GetDefaultValue(key);
			_dbManager.SetSetting(TableName, key, _settingsCache[key]?.ToString() ?? string.Empty);
		}
		Console.WriteLine("İlk ayarlar yüklendi ve senkronize edildi.");
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
