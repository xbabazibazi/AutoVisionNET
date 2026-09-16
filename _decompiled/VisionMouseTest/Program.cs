using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Scanix4.Core;
using SimpleLogger;

namespace VisionMouseTest;

internal static class Program
{
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;

    private static bool _clicked;
    private static Panel _target;
    private static Form _form;

    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "snapnet")
        {
            SnapNetFlowTest.RunAsync().GetAwaiter().GetResult();
            return;
        }
        if (args.Length > 0 && args[0] == "scale")
        {
            ScaleTest.Run();
            return;
        }
        if (args.Length > 0 && args[0] == "profile")
        {
            ProfileTest.Run();
            return;
        }
        if (args.Length > 0 && args[0] == "driverguard")
        {
            DriverGuardTest.Run();
            return;
        }
        if (args.Length > 0 && args[0] == "stoprace")
        {
            StopRaceTest.Run().GetAwaiter().GetResult();
            return;
        }
        if (args.Length > 0 && args[0] == "formmanager")
        {
            FormManagerTest.Run();
            return;
        }
        if (args.Length > 0 && args[0] == "jobbreakdown")
        {
            JobBreakdownTest.Run().GetAwaiter().GetResult();
            return;
        }
        if (args.Length > 0 && args[0] == "stopfalseerror")
        {
            StopFalseErrorTest.Run().GetAwaiter().GetResult();
            return;
        }

        Logger.Instance.SetLogMethod(msg => Console.WriteLine(msg));
        Logger.Instance.SetLogLevel(LogLevel.Debug);

        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        _form = new Form
        {
            Text = "VisionMouseTest hedef penceresi",
            Size = new Size(400, 300),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(150, 150),
            TopMost = true
        };

        _target = new Panel
        {
            BackColor = Color.Magenta,
            Size = new Size(80, 80),
            Location = new Point(180, 120)
        };
        _target.Click += (s, e) =>
        {
            _clicked = true;
            _target.BackColor = Color.Lime;
            Console.WriteLine("HEDEFE_TIKLANDI: Click olayi tetiklendi.");
        };
        _form.Controls.Add(_target);

        _form.Shown += async (s, e) =>
        {
            await System.Threading.Tasks.Task.Delay(700);
            RunTest();
            await System.Threading.Tasks.Task.Delay(1500);
            Console.WriteLine(_clicked ? "SONUC: PASS - gorsel bulundu ve tiklama hedefe ulasti." : "SONUC: FAIL - tiklama hedefe ulasmadi.");
            Application.Exit();
        };

        Application.Run(_form);
    }

    private static void RunTest()
    {
        Rectangle targetScreenRect = new Rectangle(_target.PointToScreen(Point.Empty), _target.Size);
        Point expectedCenter = new Point(targetScreenRect.X + targetScreenRect.Width / 2, targetScreenRect.Y + targetScreenRect.Height / 2);
        Console.WriteLine($"HEDEF_GERCEK_KONUM: X={expectedCenter.X}, Y={expectedCenter.Y} (kontrol dogrudan sorularak alindi)");

        string templatePath = Path.Combine(Path.GetTempPath(), "vmt_template.png");
        using (Bitmap bmp = new Bitmap(targetScreenRect.Width, targetScreenRect.Height))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(targetScreenRect.Location, Point.Empty, targetScreenRect.Size);
            }
            bmp.Save(templatePath, ImageFormat.Png);
        }
        Console.WriteLine("SABLON_KAYDEDILDI: " + templatePath);

        Rectangle fullScreen = Screen.PrimaryScreen.Bounds;
        Logger logger = Logger.Instance;

        using ScreenCapturer capturer = new ScreenCapturer(fullScreen, useColor: true, logger);
        using TemplateMatcher matcher = new TemplateMatcher(templatePath, 0.95, fullScreen, useColor: true, logger);

        using OpenCvSharp.Mat screen = capturer.Capture();
        Scanix4.Interfaces.MatchResult result = matcher.TryMatch(screen);

        if (!result.IsMatch)
        {
            Console.WriteLine("GORSEL_ARAMA_SONUCU: Eslesme bulunamadi.");
            return;
        }

        int dx = result.MatchPoint.X - expectedCenter.X;
        int dy = result.MatchPoint.Y - expectedCenter.Y;
        Console.WriteLine($"GORSEL_ARAMA_SONUCU: Bulunan=({result.MatchPoint.X},{result.MatchPoint.Y}) Guven={result.Confidence:F4} Sapma=({dx},{dy})px");

        Cursor.Position = result.MatchPoint;
        System.Threading.Thread.Sleep(150);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        System.Threading.Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        Console.WriteLine("FARE_TIKLAMASI_GONDERILDI: Bulunan koordinata sol tik gonderildi.");
    }
}
