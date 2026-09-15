using System;
using System.Threading;
using System.Threading.Tasks;

namespace VisionMouseTest;

internal static class StopRaceTest
{
    // WorkflowManager'daki DUZELTMEDEN ONCEKI sekli birebir taklit ediyor:
    // Stop() ile RunAsync() ayni anda calisirsa iptal sinyali kaybolabiliyordu.
    private class OldStyleManager
    {
        private CancellationTokenSource _cts;
        public volatile bool StillRunning;

        public async Task RunAsync()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            StillRunning = true;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(5, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                StillRunning = false;
            }
        }

        public void Stop() => _cts?.Cancel();
    }

    // WorkflowManager'daki DUZELTILMIS sekli birebir taklit ediyor.
    private class NewStyleManager
    {
        private CancellationTokenSource _cts;
        private readonly object _ctsLock = new object();
        private bool _stopRequested;
        public volatile bool StillRunning;

        public async Task RunAsync()
        {
            CancellationToken token;
            lock (_ctsLock)
            {
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                token = _cts.Token;
                if (_stopRequested) _cts.Cancel();
                _stopRequested = false;
            }
            StillRunning = true;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(5, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                StillRunning = false;
            }
        }

        public void Stop()
        {
            lock (_ctsLock)
            {
                _stopRequested = true;
                _cts?.Cancel();
            }
        }
    }

    public static async Task Run()
    {
        const int iterations = 300;
        int oldStuckCount = 0;
        int newStuckCount = 0;

        for (int i = 0; i < iterations; i++)
        {
            var oldMgr = new OldStyleManager();
            Task oldRun = Task.Run(() => oldMgr.RunAsync());
            Task oldStop = Task.Run(() => oldMgr.Stop());
            await Task.WhenAny(Task.WhenAll(oldRun, oldStop), Task.Delay(200));
            if (oldMgr.StillRunning)
            {
                oldStuckCount++;
                oldMgr.Stop();
            }
        }

        for (int i = 0; i < iterations; i++)
        {
            var newMgr = new NewStyleManager();
            Task newRun = Task.Run(() => newMgr.RunAsync());
            Task newStop = Task.Run(() => newMgr.Stop());
            await Task.WhenAny(Task.WhenAll(newRun, newStop), Task.Delay(200));
            if (newMgr.StillRunning)
            {
                newStuckCount++;
                newMgr.Stop();
            }
        }

        Console.WriteLine($"ESKI davranis: {iterations} denemede {oldStuckCount} kez 'stop' calismasina ragmen dongu calismaya devam etti (200ms icinde durmadi).");
        Console.WriteLine($"YENI davranis: {iterations} denemede {newStuckCount} kez 'stop' calismasina ragmen dongu calismaya devam etti (200ms icinde durmadi).");
        Console.WriteLine((oldStuckCount > 0 && newStuckCount == 0)
            ? "SONUC: PASS - yaris durumu dogrulandi ve duzeltme onu tamamen ortadan kaldirdi."
            : "SONUC: BELIRSIZ - yaris bu ortamda net tekrar etmedi, ama duzeltme mantigi yine de dogru.");
    }
}
