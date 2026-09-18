using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using SimpleLogger;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace Scanix4.Core;

[SupportedOSPlatform("windows10.0.10240.0")]
public class OcrTextScanner
{
	private readonly Logger _logger;

	public OcrTextScanner(Logger logger = null)
	{
		_logger = logger ?? Logger.Instance;
	}

	public async Task<Point?> FindTextCenterAsync(Rectangle screenArea, string targetText)
	{
		List<(string Text, Rectangle Bounds)> words = await ReadTextAsync(screenArea);
		foreach (var (text, bounds) in words)
		{
			if (string.Equals(text.Trim(), targetText.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return new Point(screenArea.X + bounds.X + bounds.Width / 2, screenArea.Y + bounds.Y + bounds.Height / 2);
			}
		}
		return null;
	}

	public async Task<List<(string Text, Rectangle Bounds)>> ReadTextAsync(Rectangle screenArea)
	{
		using Bitmap bitmap = CaptureScreen(screenArea);
		return await ReadTextFromBitmapAsync(bitmap);
	}

	private static Bitmap CaptureScreen(Rectangle area)
	{
		Bitmap bitmap = new Bitmap(area.Width, area.Height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.CopyFromScreen(area.X, area.Y, 0, 0, area.Size);
		return bitmap;
	}

	private async Task<List<(string, Rectangle)>> ReadTextFromBitmapAsync(Bitmap bitmap)
	{
		List<(string, Rectangle)> results = new List<(string, Rectangle)>();
		try
		{
			using MemoryStream memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, ImageFormat.Bmp);
			memoryStream.Position = 0L;
			using IRandomAccessStream randomAccessStream = memoryStream.AsRandomAccessStream();
			BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
			SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
			OcrEngine engine = OcrEngine.TryCreateFromUserProfileLanguages() ?? OcrEngine.TryCreateFromLanguage(new Language("en"));
			if (engine == null)
			{
				_logger.LogWarning("OCR motoru başlatılamadı (Windows dil paketi eksik olabilir).");
				return results;
			}
			OcrResult ocrResult = await engine.RecognizeAsync(softwareBitmap);
			foreach (OcrLine line in ocrResult.Lines)
			{
				foreach (OcrWord word in line.Words)
				{
					Rectangle bounds = new Rectangle((int)word.BoundingRect.X, (int)word.BoundingRect.Y, (int)word.BoundingRect.Width, (int)word.BoundingRect.Height);
					results.Add((word.Text, bounds));
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning("OCR okuma hatası: " + ex.Message);
		}
		return results;
	}
}
