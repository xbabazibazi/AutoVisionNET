using System;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using FluxDB.Models;

namespace FluxDB;

public class DbManager : SqliteDataConnector
{
	public DbManager(string connectionString)
		: base(connectionString)
	{
	}

	private void EnsureTableExists(string tableName)
	{
		ExecuteNonQuery("CREATE TABLE IF NOT EXISTS \"" + tableName + "\" (SettingKey TEXT PRIMARY KEY, SettingValue TEXT)");
	}

	private static bool IsMissingTableError(Exception ex)
	{
		return ex is SQLiteException && ex.Message.IndexOf("no such table", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public void SetSetting(string tableName, string key, string value)
	{
		if (string.IsNullOrWhiteSpace(tableName))
		{
			throw new ArgumentException("Tablo adı boş olamaz.", "tableName");
		}
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Anahtar boş olamaz.", "key");
		}
		try
		{
			UpsertSetting(tableName, key, value);
		}
		catch (Exception ex) when (IsMissingTableError(ex))
		{
			EnsureTableExists(tableName);
			UpsertSetting(tableName, key, value);
			Console.WriteLine($"Ayar kaydedildi (tablo oluşturuldu): {tableName}.{key} = {value}");
		}
		catch (Exception ex2)
		{
			Console.WriteLine("Ayar kaydı hatası: " + ex2.Message);
			throw;
		}
	}

	private void UpsertSetting(string tableName, string key, string value)
	{
		string safeValue = value ?? string.Empty;
		string updateQuery = "UPDATE " + tableName + " SET SettingValue = @value WHERE SettingKey = @key";
		int affected = ExecuteNonQuery(updateQuery, new SQLiteParameter("@key", key), new SQLiteParameter("@value", safeValue));
		if (affected == 0)
		{
			string insertQuery = "INSERT INTO " + tableName + " (SettingKey, SettingValue) VALUES (@key, @value)";
			ExecuteNonQuery(insertQuery, new SQLiteParameter("@key", key), new SQLiteParameter("@value", safeValue));
		}
		Console.WriteLine($"Ayar kaydedildi: {tableName}.{key} = {value}");
	}

	public T GetSetting<T>(string tableName, string key)
	{
		if (string.IsNullOrWhiteSpace(tableName))
		{
			throw new ArgumentException("Tablo adı boş olamaz.", "tableName");
		}
		if (string.IsNullOrWhiteSpace(key))
		{
			throw new ArgumentException("Anahtar boş olamaz.", "key");
		}
		string query = "SELECT SettingValue FROM " + tableName + " WHERE SettingKey = @key LIMIT 1";
		SQLiteParameter[] parameters = new SQLiteParameter[1]
		{
			new SQLiteParameter("@key", key)
		};
		try
		{
			string text;
			try
			{
				text = ExecuteScalar<string>(query, parameters);
			}
			catch (Exception ex3) when (IsMissingTableError(ex3))
			{
				EnsureTableExists(tableName);
				text = null;
			}
			if (text == null)
			{
				Console.WriteLine($"Ayar bulunamadı: {tableName}.{key}, varsayılan değer döndürüldü.");
				return default(T);
			}
			if (typeof(T).IsEnum)
			{
				return (T)Enum.Parse(typeof(T), text, ignoreCase: true);
			}
			if (typeof(T) == typeof(bool))
			{
				text = text.Trim().ToLower();
				if (text == "1" || text == "true")
				{
					return (T)(object)true;
				}
				if (text == "0" || text == "false")
				{
					return (T)(object)false;
				}
				throw new FormatException("Geçersiz boolean değeri: " + text);
			}
			return (T)Convert.ChangeType(text, typeof(T), CultureInfo.InvariantCulture);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ayar okuma hatası: {tableName}.{key}, hata: {ex.Message}");
			return default(T);
		}
	}

	public string GetScreenCaptureSetting(string name)
	{
		string query = "SELECT isActive FROM ScreenCaptureSettings WHERE name = @name LIMIT 1";
		SQLiteParameter[] parameters = new SQLiteParameter[1]
		{
			new SQLiteParameter("@name", name)
		};
		return ExecuteScalar<string>(query, parameters);
	}

	public void SetRectangleSettings(RectangleSettings settings)
	{
		string query = "\r\n                UPDATE RectanglesSettings \r\n                SET \r\n                    coordinateX = @coordinateX,\r\n                    coordinateY = @coordinateY,\r\n                    width = @width,\r\n                    height = @height\r\n                WHERE name = @name";
		SQLiteParameter[] parameters = new SQLiteParameter[5]
		{
			new SQLiteParameter("@coordinateX", settings.CoordinateX),
			new SQLiteParameter("@coordinateY", settings.CoordinateY),
			new SQLiteParameter("@width", settings.Width),
			new SQLiteParameter("@height", settings.Height),
			new SQLiteParameter("@name", settings.Name)
		};
		ExecuteNonQuery(query, parameters);
	}

	public RectangleSettings GetRectangleSettings(string name)
	{
		string query = "SELECT * FROM RectanglesSettings WHERE name = @name";
		SQLiteParameter[] parameters = new SQLiteParameter[1]
		{
			new SQLiteParameter("@name", name)
		};
		return ExecuteSingleRow(query, (IDataReader reader) => new RectangleSettings
		{
			ID = Convert.ToInt32(reader["id"]),
			Name = reader["name"].ToString(),
			CoordinateX = Convert.ToInt32(reader["coordinateX"]),
			CoordinateY = Convert.ToInt32(reader["coordinateY"]),
			Width = Convert.ToInt32(reader["width"]),
			Height = Convert.ToInt32(reader["height"])
		}, parameters);
	}
}
