using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

internal class Updater
{
	private const string AppBaseName = "UI2";

	private static readonly string[] ProcessVariants = new string[5] { "UI2", "UI2.vshost", "UI2.exe", "UI2Client", "UI2Game" };

	private const string UpdateZipUrl = "https://github.com/katadora/my-app-updates/raw/main/update.zip";

	private static async Task<int> Main(string[] args)
	{
		string tempZip = Path.Combine(Path.GetTempPath(), $"update_{Guid.NewGuid()}.zip");
		string extractDir = Path.Combine(Path.GetTempPath(), $"update_extract_{Guid.NewGuid()}");
		string targetDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
		string targetExe = Path.Combine(targetDir, "UI2.exe");
		Console.WriteLine("[UPD] Başlıyor. Hedef klasör: " + targetDir);
		try
		{
			Console.WriteLine("[UPD] 'UI2' kapatılıyor...");
			await CloseAppProcessesAsync();
			Console.WriteLine("[UPD] Uygulama kapalı. Güncelleme sürecine geçiliyor.");
			Console.WriteLine("[UPD] Zip indiriliyor...");
			await DownloadFileAsync("https://github.com/katadora/my-app-updates/raw/main/update.zip", tempZip);
			Console.WriteLine("[UPD] Zip indirildi: " + tempZip);
			Directory.CreateDirectory(extractDir);
			Console.WriteLine("[UPD] Zip çıkarılıyor...");
			ZipFile.ExtractToDirectory(tempZip, extractDir, overwriteFiles: true);
			Console.WriteLine("[UPD] Zip çıkarıldı: " + extractDir);
			Console.WriteLine("[UPD] Dosyalar kopyalanıyor...");
			await CopyDirectoryWithRetriesAsync(extractDir, targetDir);
			if (File.Exists(targetExe))
			{
				Console.WriteLine("[UPD] Uygulama başlatılıyor: " + targetExe);
				StartProcess(targetExe);
			}
			else
			{
				Console.WriteLine("[UPD][UYARI] Başlatılacak exe bulunamadı: " + targetExe);
			}
			Console.WriteLine("[UPD] Güncelleme tamamlandı.");
			return 0;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			Console.WriteLine($"[UPD][HATA] {ex2}");
			return 2;
		}
		finally
		{
			SafeDelete(tempZip);
			SafeDeleteDir(extractDir);
			Console.WriteLine("[UPD] Updater sonlandı.");
		}
	}

	private static async Task CloseAppProcessesAsync()
	{
		for (int attempt = 1; attempt <= 3; attempt++)
		{
			Console.WriteLine($"[UPD] Kapatma denemesi {attempt}/{3}...");
			List<Process> processes = (from p in SafeGetProcesses()
				where ProcessVariants.Any((string v) => (p.ProcessName ?? "").IndexOf(v.Replace(".exe", ""), StringComparison.OrdinalIgnoreCase) >= 0)
				select p).ToList();
			if (!processes.Any())
			{
				Console.WriteLine("[UPD] Hiç process bulunamadı - zaten kapalı");
				return;
			}
			Console.WriteLine($"[UPD] {processes.Count} process bulundu, kapatılıyor...");
			foreach (Process process in processes)
			{
				try
				{
					if (!process.HasExited && process.MainWindowHandle != IntPtr.Zero)
					{
						process.CloseMainWindow();
						Console.WriteLine($"[UPD] Nazik kapatma: {process.ProcessName} (PID: {process.Id})");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("[UPD] Nazik kapatma hatası " + process.ProcessName + ": " + ex.Message);
				}
			}
			await Task.Delay(1500);
			List<Process> stillRunning = processes.Where((Process p) => !p.HasExited).ToList();
			if (!stillRunning.Any())
			{
				Console.WriteLine("[UPD] Tüm process'ler nazikçe kapatıldı");
				return;
			}
			foreach (Process process2 in stillRunning)
			{
				try
				{
					if (!process2.HasExited)
					{
						process2.Kill();
						Console.WriteLine($"[UPD] Zorla kapatıldı: {process2.ProcessName} (PID: {process2.Id})");
					}
				}
				catch (Exception ex2)
				{
					Exception ex3 = ex2;
					Console.WriteLine("[UPD] Zorla kapatma hatası " + process2.ProcessName + ": " + ex3.Message);
				}
				finally
				{
					process2.Dispose();
				}
			}
			await Task.Delay(2000);
			List<Process> finalCheck = (from p in SafeGetProcesses()
				where ProcessVariants.Any((string v) => (p.ProcessName ?? "").IndexOf(v.Replace(".exe", ""), StringComparison.OrdinalIgnoreCase) >= 0)
				select p).ToList();
			if (!finalCheck.Any())
			{
				Console.WriteLine("[UPD] Tüm process'ler kapatıldı");
				return;
			}
			Console.WriteLine($"[UPD] {finalCheck.Count} process hala çalışıyor, {((attempt < 3) ? "tekrar denenecek" : "devam edilecek")}");
			if (attempt < 3)
			{
				await Task.Delay(2000);
			}
		}
		Console.WriteLine("[UPD] UYARI: Bazı process'ler kapatılamadı, ancak güncellemeye devam ediliyor");
	}

	private static Process[] SafeGetProcesses()
	{
		try
		{
			return Process.GetProcesses();
		}
		catch
		{
			return Array.Empty<Process>();
		}
	}

	private static async Task DownloadFileAsync(string url, string outPath)
	{
		using HttpClient http = new HttpClient
		{
			Timeout = TimeSpan.FromMinutes(5.0)
		};
		using HttpResponseMessage resp = await http.GetAsync(url);
		resp.EnsureSuccessStatusCode();
		Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? Path.GetTempPath());
		using FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None);
		await resp.Content.CopyToAsync(fs);
	}

	private static async Task CopyDirectoryWithRetriesAsync(string sourceDir, string destDir)
	{
		string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
		string[] array = files;
		foreach (string src in array)
		{
			string relative = Path.GetRelativePath(sourceDir, src);
			string dst = Path.Combine(destDir, relative);
			string srcFileName = Path.GetFileName(src);
			string thisUpdaterName = AppDomain.CurrentDomain.FriendlyName;
			if (string.Equals(srcFileName, thisUpdaterName, StringComparison.OrdinalIgnoreCase) || string.Equals(srcFileName, "FluxioUpdater.exe", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("[UPD] Atlandı (updater): " + relative);
				continue;
			}
			Directory.CreateDirectory(Path.GetDirectoryName(dst) ?? destDir);
			int attempt = 0;
			while (true)
			{
				attempt++;
				try
				{
					if (File.Exists(dst) && !IsFileUnlocked(dst))
					{
						throw new IOException("Hedef dosya kilitli: " + dst);
					}
					File.Copy(src, dst, overwrite: true);
					Console.WriteLine("[UPD] Kopyalandı: " + relative);
				}
				catch (IOException ex)
				{
					if (attempt >= 18)
					{
						Console.WriteLine("[UPD][HATA] " + relative + " kopyalanamadı: " + ex.Message);
						throw;
					}
					Console.WriteLine($"[UPD] Kopya hatası ({attempt}/{18}) {relative}: {ex.Message}. Bekleniyor...");
					await Task.Delay(1000 * attempt);
					continue;
				}
				break;
			}
		}
	}

	private static bool IsFileUnlocked(string path)
	{
		if (!File.Exists(path))
		{
			return true;
		}
		try
		{
			using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			{
				return true;
			}
		}
		catch (IOException)
		{
			return false;
		}
		catch
		{
			return true;
		}
	}

	private static void StartProcess(string exePath)
	{
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(exePath)
			{
				UseShellExecute = true,
				WorkingDirectory = Path.GetDirectoryName(exePath)
			};
			Process.Start(startInfo);
			Console.WriteLine("[UPD] Uygulama başlatıldı.");
		}
		catch (Exception ex)
		{
			Console.WriteLine("[UPD][HATA] Başlatılamadı: " + ex.Message);
		}
	}

	private static void SafeDelete(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch
		{
		}
	}

	private static void SafeDeleteDir(string path)
	{
		try
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
		catch
		{
		}
	}
}
