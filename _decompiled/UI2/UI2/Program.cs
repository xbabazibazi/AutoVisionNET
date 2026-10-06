using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using EVOX.Data;
using InputInterceptorNS;
using LicenseCore;
using SettingsManager;
using UI2.Database;

namespace UI2;

internal static class Program
{
	private const string AppName = "EVOX.Console";

	[STAThread]
	private static void Main()
	{
		Console.WriteLine("UI2 surum: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
		if (!IsRunningAsAdministrator() && !IsInputDriverAlreadyInstalled() && RelaunchAsAdministrator())
		{
			return;
		}
		bool createdNew;
		using (new Mutex(initiallyOwned: true, "EVOX.Console", out createdNew))
		{
			if (!createdNew)
			{
				MessageBox.Show("EVOX.Console zaten çalışıyor!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			ApplicationConfiguration.Initialize();
			// These go up here, before anything that can put a window on screen.
			// SetUnhandledExceptionMode throws the moment a Control exists on this thread, and
			// both the update prompt and the licence dialog below create one. Leaving the call
			// until after the gate meant it threw with no handler installed yet - which is what
			// reached the user as Windows' "stopped working" box immediately after their key was
			// accepted and written to disk. Installing them first also means a failure inside
			// the update check or the licence gate now shows a readable error instead of
			// killing the process without a word.
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += (s, e) => HandleFatalError(e.Exception);
			AppDomain.CurrentDomain.UnhandledException += (s, e) => HandleFatalError(e.ExceptionObject as Exception ?? new Exception("Bilinmeyen hata: " + e.ExceptionObject));
			ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new AppToolStripColorTable());
			// Updates are checked/applied BEFORE the license gate on purpose: a user
			// whose license is about to expire (or has just expired) should still be
			// able to receive app updates - the license only gates actual feature
			// usage below, not whether the app can update itself.
			if (CheckForUpdatesBeforeLicenseGate())
			{
				return;
			}
			if (!LicenseGate.EnsureLicensed("EVOX.Console"))
			{
				return;
			}
			RunApplication();
		}
	}

	// Returns true if the app already handed off to the updater and Main() should exit immediately.
	private static bool CheckForUpdatesBeforeLicenseGate()
	{
		try
		{
			UpdateInfo update = UpdateChecker.CheckForUpdateAsync().GetAwaiter().GetResult();
			if (update == null)
			{
				return false;
			}
			string message = $"Yeni sürüm bulundu: v{update.Version} (mevcut: v{UpdateChecker.CurrentVersion})\n\nŞimdi güncellensin mi?";
			if (MessageBox.Show(message, "Güncelleme Mevcut", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
			{
				return false;
			}
			if (string.IsNullOrEmpty(update.DownloadUrl))
			{
				MessageBox.Show("Güncelleme dosyası bulunamadı. Lütfen manuel indirin: " + update.HtmlUrl, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}
			string zipPath = UpdateChecker.DownloadUpdateAsync(update.DownloadUrl).GetAwaiter().GetResult();
			StartUpdaterAsAdmin(zipPath);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void StartUpdaterAsAdmin(string zipPath)
	{
		try
		{
			string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EVOX.Updater.exe");
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = "\"" + zipPath + "\" \"EVOX.Console.exe\"",
				UseShellExecute = true,
				Verb = "runas"
			};
			Process.Start(startInfo);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Updater başlatılamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private static bool IsRunningAsAdministrator()
	{
		using WindowsIdentity identity = WindowsIdentity.GetCurrent();
		WindowsPrincipal principal = new WindowsPrincipal(identity);
		return principal.IsInRole(WindowsBuiltInRole.Administrator);
	}

	private static bool IsInputDriverAlreadyInstalled()
	{
		try
		{
			return InputInterceptor.CheckDriverInstalled();
		}
		catch
		{
			return false;
		}
	}

	private static bool RelaunchAsAdministrator()
	{
		try
		{
			string exePath = Process.GetCurrentProcess().MainModule?.FileName;
			if (string.IsNullOrEmpty(exePath))
			{
				return false;
			}
			ProcessStartInfo startInfo = new ProcessStartInfo(exePath)
			{
				UseShellExecute = true,
				Verb = "runas"
			};
			Process.Start(startInfo);
			return true;
		}
		catch (Win32Exception)
		{
			return false;
		}
		catch (Exception)
		{
			return false;
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
		string tempDbPath = Path.Combine(Path.GetTempPath(), "evox_sqlite_warmup_" + Guid.NewGuid() + ".db");
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
