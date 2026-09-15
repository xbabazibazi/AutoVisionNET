using System;
using System.Threading.Tasks;
using SnapNetClient;

namespace VisionMouseTest;

internal static class SnapNetFlowTest
{
    public static async Task<bool> RunAsync()
    {
        int port = 15987;
        bool commandReceived = false;

        Server server = new Server();
        server.LogMessage += msg => Console.WriteLine("[SUNUCU] " + msg);
        var clientRegisteredOnServer = new TaskCompletionSource<bool>();
        server.ClientCountChanged += count =>
        {
            if (count > 0) clientRegisteredOnServer.TrySetResult(true);
        };
        _ = server.StartAsync(port);
        await Task.Delay(300);

        AppClient.Initialize("127.0.0.1", port, "TestClient", Client.JobType.Warrior);
        AppClient.RegisterCommand("999", () =>
        {
            commandReceived = true;
            Console.WriteLine("ISTEMCI_KOMUTU_ALDI: 999 komutu calisti.");
        });
        AppClient.MessageReceived += msg => Console.WriteLine("[ISTEMCI] " + msg);

        await AppClient.ConnectAsync();

        // Gercek kullanimdaki gibi: operator, istemci sunucunun "Bagli Cihazlar" listesine
        // dustukten hemen sonra komut gonderir (once ConnectAsync doner, ClientForm'daki
        // RegisterGlobalCommands sirasindaki eski hata burada test ediliyor).
        await clientRegisteredOnServer.Task;
        await server.SendCommandToAllClientsAsync("999", withDelay: false);

        await Task.Delay(1000);

        AppClient.Disconnect();
        await server.StopAsync();
        server.Dispose();

        Console.WriteLine(commandReceived
            ? "SNAPNET_SONUC: PASS - Sunucudan gonderilen komut istemcide calisti."
            : "SNAPNET_SONUC: FAIL - Komut istemciye ulasti ama calistirilamadi (handler kayitli degildi).");

        return commandReceived;
    }
}
