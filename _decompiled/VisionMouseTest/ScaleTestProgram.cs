using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Scanix4.Core;
using SimpleLogger;

namespace VisionMouseTest;

internal static class ScaleTest
{
    public static void Run()
    {
        Logger.Instance.SetLogMethod(msg => Console.WriteLine(msg));
        Logger.Instance.SetLogLevel(LogLevel.Debug);

        using Form form = new Form
        {
            Text = "ScaleTest",
            Size = new Size(400, 300),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(150, 150),
            TopMost = true
        };
        Panel target = new Panel
        {
            Size = new Size(100, 100),
            Location = new Point(150, 100)
        };
        target.Paint += (s, e) =>
        {
            const int cells = 10;
            int cw = target.Width / cells;
            int ch = target.Height / cells;
            for (int row = 0; row < cells; row++)
            {
                for (int col = 0; col < cells; col++)
                {
                    Brush brush = ((row + col) % 2 == 0) ? Brushes.Black : Brushes.White;
                    e.Graphics.FillRectangle(brush, col * cw, row * ch, cw, ch);
                }
            }
        };
        form.Controls.Add(target);
        form.Show();
        Application.DoEvents();
        System.Threading.Thread.Sleep(400);

        Rectangle targetScreenRect = new Rectangle(target.PointToScreen(Point.Empty), target.Size);
        Point expectedCenter = new Point(targetScreenRect.X + targetScreenRect.Width / 2, targetScreenRect.Y + targetScreenRect.Height / 2);
        Console.WriteLine($"HEDEF gercek boyutu (ekranda): {targetScreenRect.Width}x{targetScreenRect.Height} merkez=({expectedCenter.X},{expectedCenter.Y})");

        using Bitmap fullBmp = new Bitmap(targetScreenRect.Width, targetScreenRect.Height);
        using (Graphics g = Graphics.FromImage(fullBmp))
        {
            g.CopyFromScreen(targetScreenRect.Location, Point.Empty, targetScreenRect.Size);
        }

        // Sablonu kasitli olarak %65 kucult - taban olcekte (1.0x) artik eslesmeyecek,
        // sadece TemplateMatcher'in coklu-olcek denemesi (0.6667x civari) bulabilir.
        int shrunkWidth = (int)Math.Round(fullBmp.Width / 1.5);
        int shrunkHeight = (int)Math.Round(fullBmp.Height / 1.5);
        string templatePath = Path.Combine(Path.GetTempPath(), "scaletest_template.png");
        using (Bitmap shrunk = new Bitmap(shrunkWidth, shrunkHeight))
        {
            using (Graphics g = Graphics.FromImage(shrunk))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(fullBmp, 0, 0, shrunkWidth, shrunkHeight);
            }
            shrunk.Save(templatePath, ImageFormat.Png);
        }
        Console.WriteLine($"SABLON kasitli olarak kucultuldu: {shrunkWidth}x{shrunkHeight} (gercek boyutun %65'i) -> {templatePath}");

        Rectangle searchArea = Rectangle.Inflate(targetScreenRect, 60, 60);
        Logger logger = Logger.Instance;
        using ScreenCapturer capturer = new ScreenCapturer(searchArea, useColor: true, logger);
        using TemplateMatcher matcher = new TemplateMatcher(templatePath, 0.9, searchArea, useColor: true, logger);
        using OpenCvSharp.Mat screen = capturer.Capture();
        Scanix4.Interfaces.MatchResult result = matcher.TryMatch(screen);

        if (!result.IsMatch)
        {
            Console.WriteLine("SONUC: FAIL - farkli olcekteki sablon bulunamadi.");
        }
        else
        {
            int dx = result.MatchPoint.X - expectedCenter.X;
            int dy = result.MatchPoint.Y - expectedCenter.Y;
            Console.WriteLine($"SONUC: PASS - farkli olcekteki (%65 kuculmus) sablon yine de bulundu. Bulunan=({result.MatchPoint.X},{result.MatchPoint.Y}) Guven={result.Confidence:F4} Sapma=({dx},{dy})px");
        }
    }
}
