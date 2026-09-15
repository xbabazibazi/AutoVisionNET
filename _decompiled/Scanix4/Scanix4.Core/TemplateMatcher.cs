using System;
using System.Collections.Generic;
using System.Drawing;
using OpenCvSharp;
using Scanix4.Interfaces;
using SimpleLogger;

namespace Scanix4.Core;

public class TemplateMatcher : ITemplateMatcher, IDisposable
{
	private readonly Mat _template;

	private readonly double _threshold;

	private readonly Rect _searchArea;

	private readonly Logger _logger = Logger.Instance;

	private readonly Mat _resultBuffer;

	private readonly Mat _thresholdedBuffer;

	private readonly Mat _nonZeroCoordinatesBuffer;

	private readonly object _lock = new object();

	private bool _disposed;

	// Sablonlar %100 olcekte alindigi icin, ekran %125/%150 gibi farkli
	// bir Windows olceklendirmesinde calisiyorsa eslesme bulunamaz. Bu
	// yaygin oranlarda buyutulmus/kucultulmus ek sablon kopyalari da
	// deneyerek eslesmeyi olceklendirmeden bagimsiz hale getiriyoruz.
	private static readonly double[] AdditionalScales = new double[4] { 1.25, 1.5, 0.8, 0.6667 };

	private readonly List<(double Scale, Mat Template)> _scaledTemplates = new List<(double, Mat)>();

	public TemplateMatcher(string templatePath, double threshold, Rectangle searchArea, bool useColor, Logger logger)
	{
		_threshold = threshold;
		_searchArea = new Rect(searchArea.X, searchArea.Y, searchArea.Width, searchArea.Height);
		_logger = logger ?? Logger.Instance;
		Mat mat = Cv2.ImRead(templatePath);
		if (mat.Empty())
		{
			throw new ArgumentException("Şablon yüklenemedi: " + templatePath);
		}
		_template = new Mat();
		Cv2.CvtColor(mat, _template, useColor ? ColorConversionCodes.BGR2RGB : ColorConversionCodes.BGR2GRAY);
		mat.Dispose();
		foreach (double scale in AdditionalScales)
		{
			int scaledWidth = (int)Math.Round(_template.Width * scale);
			int scaledHeight = (int)Math.Round(_template.Height * scale);
			if (scaledWidth < 8 || scaledHeight < 8 || scaledWidth > searchArea.Width || scaledHeight > searchArea.Height)
			{
				continue;
			}
			Mat scaledTemplate = new Mat();
			Cv2.Resize(_template, scaledTemplate, new OpenCvSharp.Size(scaledWidth, scaledHeight));
			_scaledTemplates.Add((scale, scaledTemplate));
		}
		_resultBuffer = new Mat();
		_thresholdedBuffer = new Mat();
		_nonZeroCoordinatesBuffer = new Mat();
	}

	public MatchResult TryMatch(Mat screen)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("TemplateMatcher");
		}
		lock (_lock)
		{
			double bestVal = -1.0;
			OpenCvSharp.Point bestLoc = default(OpenCvSharp.Point);
			int bestWidth = _template.Width;
			int bestHeight = _template.Height;
			double bestScale = 1.0;
			Cv2.MatchTemplate(screen, _template, _resultBuffer, TemplateMatchModes.CCorrNormed);
			Cv2.MinMaxLoc(_resultBuffer, out var _, out var maxVal, out var _, out var maxLoc);
			bestVal = maxVal;
			bestLoc = maxLoc;
			if (maxVal < _threshold)
			{
				foreach (var (scale, scaledTemplate) in _scaledTemplates)
				{
					Cv2.MatchTemplate(screen, scaledTemplate, _resultBuffer, TemplateMatchModes.CCorrNormed);
					Cv2.MinMaxLoc(_resultBuffer, out var _, out var scaledMaxVal, out var _, out var scaledMaxLoc);
					if (scaledMaxVal > bestVal)
					{
						bestVal = scaledMaxVal;
						bestLoc = scaledMaxLoc;
						bestWidth = scaledTemplate.Width;
						bestHeight = scaledTemplate.Height;
						bestScale = scale;
					}
					if (bestVal >= _threshold)
					{
						break;
					}
				}
			}
			if (bestVal < _threshold)
			{
				_logger.LogDebug($"Eşleşme bulunamadı. Güven: {bestVal:F4} (eşik: {_threshold}), Şablon boyutu: {_template.Width}x{_template.Height}, Arama alanı: {_searchArea.Width}x{_searchArea.Height}");
				return MatchResult.NoMatch;
			}
			System.Drawing.Point point = new System.Drawing.Point(_searchArea.X + bestLoc.X + bestWidth / 2, _searchArea.Y + bestLoc.Y + bestHeight / 2);
			if (bestScale != 1.0)
			{
				_logger.LogDebug($"Eşleşme bulundu (ölçek {bestScale:F2}x): {point} - Güven: {bestVal:F4}");
			}
			else
			{
				_logger.LogDebug($"Eşleşme bulundu: {point} - Güven: {bestVal:F4}, Şablon boyutu: {_template.Width}x{_template.Height}");
			}
			return new MatchResult(isMatch: true, point, bestVal);
		}
	}

	public int CountMatches(Mat screen)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("TemplateMatcher");
		}
		lock (_lock)
		{
			try
			{
				Cv2.MatchTemplate(screen, _template, _resultBuffer, TemplateMatchModes.CCorrNormed);
				Cv2.Threshold(_resultBuffer, _thresholdedBuffer, _threshold, 1.0, ThresholdTypes.Binary);
				Cv2.FindNonZero(_thresholdedBuffer, _nonZeroCoordinatesBuffer);
				return _nonZeroCoordinatesBuffer.Rows;
			}
			catch (Exception)
			{
				return 0;
			}
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			lock (_lock)
			{
				_template.Dispose();
				_resultBuffer.Dispose();
				_thresholdedBuffer.Dispose();
				_nonZeroCoordinatesBuffer.Dispose();
				foreach (var (_, scaledTemplate) in _scaledTemplates)
				{
					scaledTemplate.Dispose();
				}
				_disposed = true;
			}
		}
	}
}
