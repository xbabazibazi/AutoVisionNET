using System;
using System.Linq;
using System.Threading.Tasks;
using SnapNetClient;

namespace VisionMouseTest;

internal static class JobBreakdownTest
{
    public static async Task Run()
    {
        int port = 15988;
        Server server = new Server();
        var clientCountTcs = new TaskCompletionSource<bool>();
        int connectedCount = 0;
        server.ClientCountChanged += count =>
        {
            connectedCount = count;
            if (count >= 3) clientCountTcs.TrySetResult(true);
        };
        _ = server.StartAsync(port);
        await Task.Delay(300);

        AppClient.Initialize("127.0.0.1", port, "Savasci1", Client.JobType.Warrior);
        await AppClient.ConnectAsync();
        var c1 = new Client();
        await c1.ConnectAsync("127.0.0.1", port, "Savasci2", Client.JobType.Warrior);
        var c2 = new Client();
        await c2.ConnectAsync("127.0.0.1", port, "Rahip1", Client.JobType.Priest);

        await Task.WhenAny(clientCountTcs.Task, Task.Delay(2000));

        var clientList = server.GetClientList().ToList();
        Console.WriteLine("Bagli istemciler: " + string.Join(", ", clientList.Select(c => $"{c.nickname}({c.job})")));

        string breakdown = string.Join("   ", clientList
            .GroupBy(c => c.job)
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()}"));
        Console.WriteLine("Hesaplanan dagilim: " + breakdown);

        bool ok = connectedCount == 3 && breakdown.Contains("Warrior: 2") && breakdown.Contains("Priest: 1");
        Console.WriteLine(ok ? "SONUC: PASS - meslek dagilimi dogru hesaplandi." : "SONUC: FAIL - beklenen dagilim gelmedi.");

        AppClient.Disconnect();
        c1.Disconnect();
        c2.Disconnect();
        await server.StopAsync();
        server.Dispose();
    }
}
