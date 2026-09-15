using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluxDB;

namespace UI2.Database;

public sealed class DatabaseUpdater
{
	private class SqlCommandParser
	{
		public List<string> CommonCommands { get; } = new List<string>();

		public Dictionary<string, List<string>> DatabaseSpecificCommands { get; } = new Dictionary<string, List<string>>();

		public void Parse(string sqlScript)
		{
			CommonCommands.Clear();
			DatabaseSpecificCommands.Clear();
			string[] array = sqlScript.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
			string currentDb = null;
			StringBuilder stringBuilder = new StringBuilder();
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Trim();
				if (string.IsNullOrWhiteSpace(text2))
				{
					continue;
				}
				if (text2.StartsWith("[") && text2.EndsWith("]"))
				{
					if (stringBuilder.Length > 0)
					{
						SaveCommand(stringBuilder.ToString(), currentDb);
						stringBuilder.Clear();
					}
					currentDb = text2.Substring(1, text2.Length - 2).Trim();
				}
				else
				{
					stringBuilder.AppendLine(text2);
					if (text2.EndsWith(";"))
					{
						SaveCommand(stringBuilder.ToString(), currentDb);
						stringBuilder.Clear();
					}
				}
			}
			if (stringBuilder.Length > 0)
			{
				SaveCommand(stringBuilder.ToString(), currentDb);
			}
		}

		private void SaveCommand(string command, string currentDb)
		{
			command = command.Trim();
			if (string.IsNullOrWhiteSpace(command))
			{
				return;
			}
			if (currentDb == null)
			{
				CommonCommands.Add(command);
				return;
			}
			if (!DatabaseSpecificCommands.ContainsKey(currentDb))
			{
				DatabaseSpecificCommands[currentDb] = new List<string>();
			}
			DatabaseSpecificCommands[currentDb].Add(command);
		}
	}

	private readonly Dictionary<string, DbManager> _dbManagers;

	public DatabaseUpdater(Dictionary<string, DbManager> dbManagers)
	{
		_dbManagers = dbManagers;
	}

	public void ExecuteUpdates()
	{
		string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdateDB.sql");
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			string sqlScript = File.ReadAllText(path);
			ProcessScript(sqlScript);
			File.Delete(path);
			Console.WriteLine("Veritabanı güncellemeleri başarıyla uygulandı.");
		}
		catch (Exception ex)
		{
			Console.WriteLine("Database update error: " + ex.Message);
		}
	}

	private void ProcessScript(string sqlScript)
	{
		SqlCommandParser sqlCommandParser = new SqlCommandParser();
		sqlCommandParser.Parse(sqlScript);
		foreach (DbManager value2 in _dbManagers.Values)
		{
			ExecuteCommands(value2, sqlCommandParser.CommonCommands);
		}
		foreach (KeyValuePair<string, List<string>> databaseSpecificCommand in sqlCommandParser.DatabaseSpecificCommands)
		{
			var (dbName, commands) = databaseSpecificCommand;
			string text2 = _dbManagers.Keys.FirstOrDefault((string k) => k.Equals(dbName, StringComparison.OrdinalIgnoreCase));
			if (text2 != null && _dbManagers.TryGetValue(text2, out DbManager value))
			{
				ExecuteCommands(value, commands);
			}
		}
	}

	private void ExecuteCommands(DbManager dbManager, IEnumerable<string> commands)
	{
		if (!commands.Any())
		{
			return;
		}
		try
		{
			foreach (string command in commands)
			{
				if (!string.IsNullOrWhiteSpace(command) && !command.TrimStart().StartsWith("--"))
				{
					string text = MakeCommandSafe(command);
					Console.WriteLine($"Executing on {dbManager.GetType().Name}: {text.Substring(0, Math.Min(text.Length, 100))}...");
					dbManager.ExecuteNonQuery(text, (SQLiteParameter[])null);
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Database error: " + ex.Message);
		}
	}

	private string MakeCommandSafe(string command)
	{
		if (Regex.IsMatch(command, "^\\s*CREATE\\s+TABLE", RegexOptions.IgnoreCase) && !Regex.IsMatch(command, "IF\\s+NOT\\s+EXISTS", RegexOptions.IgnoreCase))
		{
			return Regex.Replace(command, "CREATE\\s+TABLE", "CREATE TABLE IF NOT EXISTS", RegexOptions.IgnoreCase);
		}
		if (Regex.IsMatch(command, "^\\s*INSERT", RegexOptions.IgnoreCase) && !Regex.IsMatch(command, "OR\\s+IGNORE", RegexOptions.IgnoreCase))
		{
			return Regex.Replace(command, "INSERT", "INSERT OR IGNORE", RegexOptions.IgnoreCase);
		}
		return command;
	}
}
