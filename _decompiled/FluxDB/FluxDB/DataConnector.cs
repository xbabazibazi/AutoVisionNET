using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace FluxDB;

public abstract class DataConnector(string connectionString)
{
	private readonly string _connectionString = connectionString;

	private static DataConnector? _instance;

	private static readonly object _lock = new object();

	public static DataConnector Instance(string connectionString)
	{
		lock (_lock)
		{
			if (_instance == null)
			{
				_instance = new SqliteDataConnector(connectionString);
			}
			return _instance;
		}
	}

	protected SQLiteConnection GetConnection()
	{
		return new SQLiteConnection(_connectionString);
	}

	public void ExecuteNonQuery(string query, params SQLiteParameter[] parameters)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteCommand sQLiteCommand = new SQLiteCommand(query, sQLiteConnection);
		if (parameters != null)
		{
			sQLiteCommand.Parameters.AddRange(parameters);
		}
		sQLiteConnection.Open();
		sQLiteCommand.ExecuteNonQuery();
	}

	public void ExecuteNonQuery(string query)
	{
		ExecuteNonQuery(query, Array.Empty<SQLiteParameter>());
	}

	public T ExecuteScalar<T>(string query, params SQLiteParameter[] parameters)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteCommand sQLiteCommand = new SQLiteCommand(query, sQLiteConnection);
		sQLiteCommand.Parameters.AddRange(parameters);
		sQLiteConnection.Open();
		object value = sQLiteCommand.ExecuteScalar();
		try
		{
			return (T)Convert.ChangeType(value, typeof(T));
		}
		catch (InvalidCastException)
		{
			return default(T);
		}
	}

	public IEnumerable<T> ExecuteQuery<T>(string query, Func<IDataReader, T> map, params SQLiteParameter[] parameters)
	{
		List<T> list = new List<T>();
		using (SQLiteConnection sQLiteConnection = GetConnection())
		{
			using SQLiteCommand sQLiteCommand = new SQLiteCommand(query, sQLiteConnection);
			sQLiteCommand.Parameters.AddRange(parameters);
			sQLiteConnection.Open();
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				list.Add(map(sQLiteDataReader));
			}
		}
		return list;
	}

	public T ExecuteSingleRow<T>(string query, Func<IDataReader, T> map, params SQLiteParameter[] parameters) where T : new()
	{
		using (SQLiteConnection sQLiteConnection = GetConnection())
		{
			using SQLiteCommand sQLiteCommand = new SQLiteCommand(query, sQLiteConnection);
			sQLiteCommand.Parameters.AddRange(parameters);
			sQLiteConnection.Open();
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			if (sQLiteDataReader.Read())
			{
				return map(sQLiteDataReader);
			}
		}
		return new T();
	}
}
