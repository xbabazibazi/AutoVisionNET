using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
		Console.WriteLine("UI2 surum: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
		bool createdNew;
		using (new Mutex(initiallyOwned: true, "FluxioPlatform", out createdNew))
		{
			if (!createdNew)
			{
				MessageBox.Show("FluxioPlatform zaten çalışıyor!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			ApplicationConfiguration.Initialize();
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += (s, e) => HandleFatalError(e.Exception);
			AppDomain.CurrentDomain.UnhandledException += (s, e) => HandleFatalError(e.ExceptionObject as Exception ?? new Exception("Bilinmeyen hata: " + e.ExceptionObject));
			RunApplication();
		}
	}

	private static void RunApplication()
	{
		Dictionary<string, DbManager> dictionary = null;
		try
		{
			WarmUpSqliteNativeLibrary();
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

	private static void WarmUpSqliteNativeLibrary()
	{
		// System.Data.SQLite'in native kutuphanesi ilk kullanimda tek seferlik bir
		// yukleme yapiyor; bu ilk cagri bazen "Value cannot be null (Parameter 'path1')"
		// gibi zararsiz ama korkutucu bir hatayla basarisiz olabiliyor (bilinen bir
		// .NET Core / self-contained tek-dosya yayinlarda gorulen uyumluluk sorunu).
		// :memory: baglanti bu yolu tetiklemeye yetmiyor; gercek dosya tabanli
		// baglantilarin izledigi native kod yolunu taklit etmek icin gecici bir
		// disk dosyasi kullanip birkac kez deniyoruz.
		string tempDbPath = Path.Combine(Path.GetTempPath(), "fluxio_sqlite_warmup_" + Guid.NewGuid() + ".db");
		for (int attempt = 0; attempt < 3; attempt++)
		{
			try
			{
				using SQLiteConnection sQLiteConnection = new SQLiteConnection("Data Source=" + tempDbPath + ";Version=3;");
				sQLiteConnection.Open();
				using (SQLiteCommand sQLiteCommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS WarmUp (X INTEGER); INSERT INTO WarmUp VALUES (1); SELECT * FROM WarmUp;", sQLiteConnection))
				{
					sQLiteCommand.ExecuteNonQuery();
				}
				break;
			}
			catch
			{
			}
		}
		try
		{
			if (File.Exists(tempDbPath))
			{
				File.Delete(tempDbPath);
			}
		}
		catch
		{
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
