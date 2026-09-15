using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using FluxDB;
using SettingsManager;
using UI2.Database;

namespace UI2;

internal static class Program
{
	private const string AppName = "FluxioPlatform";

	[STAThread]
	private static void Main()
	{
		bool createdNew;
		using (new Mutex(initiallyOwned: true, "FluxioPlatform", out createdNew))
		{
			if (!createdNew)
			{
				MessageBox.Show("FluxioPlatform zaten çalışıyor!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			ApplicationConfiguration.Initialize();
			RunApplication();
		}
	}

	private static void RunApplication()
	{
		Dictionary<string, DbManager> dictionary = null;
		try
		{
			dictionary = CreateDbManagers();
			DatabaseUpdater databaseUpdater = new DatabaseUpdater(dictionary);
			databaseUpdater.ExecuteUpdates();
			Settings.Initialize(dictionary);
			Application.Run(new Form1());
		}
		catch (Exception ex)
		{
			HandleFatalError(ex);
		}
		finally
		{
			if (dictionary != null)
			{
				foreach (DbManager value in dictionary.Values)
				{
					(value as IDisposable)?.Dispose();
				}
			}
		}
	}

	private static Dictionary<string, DbManager> CreateDbManagers()
	{
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DB");
		Directory.CreateDirectory(text);
		return new Dictionary<string, DbManager>
		{
			["Macro"] = CreateDbManager(text, "MacroSettings.db"),
			["ScreenCapture"] = CreateDbManager(text, "ScreenCaptureSettings.db"),
			["General"] = CreateDbManager(text, "GeneralSettings.db"),
			["Client"] = CreateDbManager(text, "ClientSettings.db")
		};
	}

	private static DbManager CreateDbManager(string folder, string dbName)
	{
		string text = Path.Combine(folder, dbName);
		return new DbManager("Data Source=" + text + ";Version=3;");
	}

	private static void HandleFatalError(Exception ex)
	{
		string text = "Kritik Hata: " + ex.Message + "\n\nDetaylar:\n" + ex.StackTrace;
		MessageBox.Show(text, "Uygulama Çöktü", MessageBoxButtons.OK, MessageBoxIcon.Hand);
	}
}
