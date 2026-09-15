using System;
using System.IO;
using FluxDB;
using FluxDB.Models;
using SettingsManager.ScreenCapture;

namespace VisionMouseTest;

internal static class ProfileTest
{
    public static void Run()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), "profiletest_" + Guid.NewGuid() + ".db");
        DbManager db = new DbManager("Data Source=" + dbPath + ";Version=3;");
        RectanglesSettings settings = new RectanglesSettings(db);

        settings.Party = new RectangleSettings { CoordinateX = 100, CoordinateY = 200, Width = 300, Height = 400 };
        settings.Genie = new RectangleSettings { CoordinateX = 10, CoordinateY = 20, Width = 30, Height = 40 };
        Console.WriteLine($"1) 1920x1080 icin ayarlandi: Party=({settings.Party.CoordinateX},{settings.Party.CoordinateY},{settings.Party.Width}x{settings.Party.Height})");

        settings.SaveAsProfile("1920x1080");
        Console.WriteLine("2) '1920x1080' profili olarak kaydedildi.");

        settings.Party = new RectangleSettings { CoordinateX = 5, CoordinateY = 5, Width = 50, Height = 50 };
        settings.Genie = new RectangleSettings { CoordinateX = 1, CoordinateY = 1, Width = 5, Height = 5 };
        Console.WriteLine($"3) Farkli degerlere degistirildi (1366x768 simulasyonu): Party=({settings.Party.CoordinateX},{settings.Party.CoordinateY},{settings.Party.Width}x{settings.Party.Height})");
        settings.SaveAsProfile("1366x768");
        Console.WriteLine("4) '1366x768' profili olarak kaydedildi.");

        bool loaded = settings.LoadProfile("1920x1080");
        RectangleSettings restoredParty = settings.Party;
        Console.WriteLine($"5) '1920x1080' profili geri yuklendi (basarili={loaded}): Party=({restoredParty.CoordinateX},{restoredParty.CoordinateY},{restoredParty.Width}x{restoredParty.Height})");

        bool ok = restoredParty.CoordinateX == 100 && restoredParty.CoordinateY == 200 && restoredParty.Width == 300 && restoredParty.Height == 400;
        Console.WriteLine("Mevcut profiller: " + string.Join(", ", settings.ListProfiles()));
        Console.WriteLine("Aktif profil: " + settings.ActiveProfileName);
        Console.WriteLine(ok ? "SONUC: PASS - profil kaydet/yukle dogru calisiyor." : "SONUC: FAIL - geri yuklenen degerler beklenenle eslesmiyor.");

        try { File.Delete(dbPath); } catch { }
    }
}
