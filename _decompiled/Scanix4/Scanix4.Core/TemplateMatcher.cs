using System;
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
			try
			{
				Cv2.MatchTemplate(screen, _template, _resultBuffer, TemplateMatchModes.CCorrNormed);
				Cv2.MinMaxLoc(_resultBuffer, out var _, out var maxVal, out var _, out var maxLoc);
				if (maxVal < _threshold)
				{
					_logger.LogDebug($"Eşleşme bulunamadı. Güven: {maxVal:F4} (eşik: {_threshold}), Şablon boyutu: {_template.Width}x{_template.Height}, Arama alanı: {_searchArea.Width}x{_searchArea.Height}");
					return MatchResult.NoMatch;
				}
				System.Drawing.Point point = new System.Drawing.Point(_searchArea.X + maxLoc.X + _template.Width / 2, _searchArea.Y + maxLoc.Y + _template.Height / 2);
				_logger.LogDebug($"Eşleşme bulundu: {point} - Güven: {maxVal:F4}, Şablon boyutu: {_template.Width}x{_template.Height}");
				return new MatchResult(isMatch: true, point, maxVal);
			}
			catch (Exception)
			{
				throw;
			}
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
				_disposed = true;
			}
		}
	}
}
