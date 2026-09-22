using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using LicenseCore;

namespace SnapNetUI;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		// Updates are checked/applied BEFORE the license gate on purpose: a user
		// whose license is about to expire (or has just expired) should still be
		// able to receive app updates - the license only gates actual feature
		// usage below, not whether the app can update itself.
		if (CheckForUpdatesBeforeLicenseGate())
		{
			return;
		}
		if (!LicenseGate.EnsureLicensed("EVOX.Service"))
		{
			return;
		}
		Application.Run(new ServerForm());
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
				Arguments = "\"" + zipPath + "\" \"EVOX.Service.exe\"",
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
}
