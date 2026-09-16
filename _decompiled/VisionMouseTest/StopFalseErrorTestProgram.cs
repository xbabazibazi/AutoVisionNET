using System;
using System.Threading.Tasks;
using SnapNetClient;

namespace VisionMouseTest;

internal static class StopFalseErrorTest
{
    public static async Task Run()
    {
        int port = 15989;
        bool sawFalseError = false;
        Server server = new Server();
        server.LogMessage += msg =>
        {
            Console.WriteLine("[SUNUCU] " + msg);
            if (msg.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                sawFalseError = true;
            }
        };
        _ = server.StartAsync(port);
        await Task.Delay(300);

        Console.WriteLine("Normal DURDUR cagriliyor...");
        await server.StopAsync();
        await Task.Delay(500);

        server.Dispose();
        Console.WriteLine(sawFalseError
            ? "SONUC: FAIL - normal durdurma sirasinda hala sahte 'ERROR' mesaji cikti."
            : "SONUC: PASS - normal durdurma sirasinda hicbir sahte hata mesaji cikmadi.");
    }
}
