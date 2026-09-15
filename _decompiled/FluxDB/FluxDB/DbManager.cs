using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
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
		try
		{
			return ExecuteScalar<string>(query, parameters);
		}
		catch (Exception ex) when (IsMissingTableError(ex))
		{
			return null;
		}
	}

	private void EnsureRectanglesSettingsTableExists()
	{
		ExecuteNonQuery("CREATE TABLE IF NOT EXISTS RectanglesSettings (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT UNIQUE, coordinateX INTEGER, coordinateY INTEGER, width INTEGER, height INTEGER)");
	}

	public void SetRectangleSettings(RectangleSettings settings)
	{
		string updateQuery = "UPDATE RectanglesSettings SET coordinateX = @coordinateX, coordinateY = @coordinateY, width = @width, height = @height WHERE name = @name";
		string insertQuery = "INSERT INTO RectanglesSettings (name, coordinateX, coordinateY, width, height) VALUES (@name, @coordinateX, @coordinateY, @width, @height)";
		SQLiteParameter[] MakeParams()
		{
			return new SQLiteParameter[5]
			{
				new SQLiteParameter("@coordinateX", settings.CoordinateX),
				new SQLiteParameter("@coordinateY", settings.CoordinateY),
				new SQLiteParameter("@width", settings.Width),
				new SQLiteParameter("@height", settings.Height),
				new SQLiteParameter("@name", settings.Name)
			};
		}
		try
		{
			if (ExecuteNonQuery(updateQuery, MakeParams()) == 0)
			{
				ExecuteNonQuery(insertQuery, MakeParams());
			}
		}
		catch (Exception ex) when (IsMissingTableError(ex))
		{
			EnsureRectanglesSettingsTableExists();
			if (ExecuteNonQuery(updateQuery, MakeParams()) == 0)
			{
				ExecuteNonQuery(insertQuery, MakeParams());
			}
		}
	}

	public RectangleSettings GetRectangleSettings(string name)
	{
		string query = "SELECT * FROM RectanglesSettings WHERE name = @name";
		SQLiteParameter[] MakeParams()
		{
			return new SQLiteParameter[1] { new SQLiteParameter("@name", name) };
		}
		Func<IDataReader, RectangleSettings> map = (IDataReader reader) => new RectangleSettings
		{
			ID = Convert.ToInt32(reader["id"]),
			Name = reader["name"].ToString(),
			CoordinateX = Convert.ToInt32(reader["coordinateX"]),
			CoordinateY = Convert.ToInt32(reader["coordinateY"]),
			Width = Convert.ToInt32(reader["width"]),
			Height = Convert.ToInt32(reader["height"])
		};
		try
		{
			return ExecuteSingleRow(query, map, MakeParams());
		}
		catch (Exception ex) when (IsMissingTableError(ex))
		{
			EnsureRectanglesSettingsTableExists();
			return new RectangleSettings { Name = name };
		}
	}

	private void EnsureResolutionProfilesTableExists()
	{
		ExecuteNonQuery("CREATE TABLE IF NOT EXISTS ResolutionProfiles (profileName TEXT, areaName TEXT, coordinateX INTEGER, coordinateY INTEGER, width INTEGER, height INTEGER, PRIMARY KEY (profileName, areaName))");
	}

	public void SaveAreaToProfile(string profileName, RectangleSettings settings)
	{
		string updateQuery = "UPDATE ResolutionProfiles SET coordinateX = @x, coordinateY = @y, width = @w, height = @h WHERE profileName = @profile AND areaName = @area";
		string insertQuery = "INSERT INTO ResolutionProfiles (profileName, areaName, coordinateX, coordinateY, width, height) VALUES (@profile, @area, @x, @y, @w, @h)";
		SQLiteParameter[] MakeParams()
		{
			return new SQLiteParameter[6]
			{
				new SQLiteParameter("@profile", profileName),
				new SQLiteParameter("@area", settings.Name),
				new SQLiteParameter("@x", settings.CoordinateX),
				new SQLiteParameter("@y", settings.CoordinateY),
				new SQLiteParameter("@w", settings.Width),
				new SQLiteParameter("@h", settings.Height)
			};
		}
		try
		{
			if (ExecuteNonQuery(updateQuery, MakeParams()) == 0)
			{
				ExecuteNonQuery(insertQuery, MakeParams());
			}
		}
		catch (Exception ex) when (IsMissingTableError(ex))
		{
			EnsureResolutionProfilesTableExists();
			if (ExecuteNonQuery(updateQuery, MakeParams()) == 0)
			{
				ExecuteNonQuery(insertQuery, MakeParams());
			}
		}
	}

	public RectangleSettings GetAreaFromProfile(string profileName, string areaName)
	{
		string query = "SELECT * FROM ResolutionProfiles WHERE profileName = @profile AND areaName = @area";
		SQLiteParameter[] MakeParams()
		{
			return new SQLiteParameter[2]
			{
				new SQLiteParameter("@profile", profileName),
				new SQLiteParameter("@area", areaName)
			};
		}
		Func<IDataReader, RectangleSettings> map = (IDataReader reader) => new RectangleSettings
		{
			Name = areaName,
			CoordinateX = Convert.ToInt32(reader["coordinateX"]),
			CoordinateY = Convert.ToInt32(reader["coordinateY"]),
			Width = Convert.ToInt32(reader["width"]),
			Height = Convert.ToInt32(reader["height"])
		};
		try
		{
			RectangleSettings result = ExecuteSingleRow(query, map, MakeParams());
			return (result.Name == areaName && (result.Width != 0 || result.Height != 0 || result.CoordinateX != 0 || result.CoordinateY != 0)) ? result : null;
		}
		catch (Exception ex) when (IsMissingTableError(ex))
		{
			EnsureResolutionProfilesTableExists();
			return null;
		}
	}

	public List<string> ListResolutionProfiles()
	{
		try
		{
			return ExecuteQuery("SELECT DISTINCT profileName FROM ResolutionProfiles ORDER BY profileName", (IDataReader reader) => reader["profileName"].ToString()).ToList();
		}
		catch (Exception ex) when (IsMissingTableError(ex))
		{
			EnsureResolutionProfilesTableExists();
			return new List<string>();
		}
	}

	public void DeleteResolutionProfile(string profileName)
	{
		try
		{
			ExecuteNonQuery("DELETE FROM ResolutionProfiles WHERE profileName = @profile", new SQLiteParameter("@profile", profileName));
		}
		catch (Exception ex) when (IsMissingTableError(ex))
		{
		}
	}
}
