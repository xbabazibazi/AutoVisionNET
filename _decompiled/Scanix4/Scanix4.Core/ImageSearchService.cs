using System;
using System.Drawing;
using OpenCvSharp;
using Scanix4.Interfaces;
using Scanix4.Models;
using SimpleLogger;

namespace Scanix4.Core;

public class ImageSearchService : IDisposable
{
	private readonly ITemplateMatcher _matcher;

	private readonly IScreenCapturer _capturer;

	private readonly SearchConfig _config;

	private readonly Logger _logger = Logger.Instance;

	private readonly Mat _screenBuffer;

	private bool _disposed;

	public ImageSearchService(SearchConfig config, Logger logger = null)
	{
		_config = config ?? throw new ArgumentNullException("config");
		_logger = logger ?? Logger.Instance;
		if (!IsValidSearchArea(_config.SearchArea))
		{
			_logger.LogWarning($"Geçersiz arama alanı: {_config.SearchArea}. Servis pasif moda alındı.");
			return;
		}
		try
		{
			_matcher = new TemplateMatcher(_config.TemplatePath, _config.Threshold, _config.SearchArea, _config.UseColor, _logger);
			_capturer = new ScreenCapturer(_config.SearchArea, _config.UseColor, _logger);
			_screenBuffer = new Mat();
		}
		catch (Exception)
		{
		}
	}

	public object Search()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ImageSearchService");
		}
		try
		{
			if (!IsValidSearchArea(_config.SearchArea))
			{
				_logger.LogWarning("Geçersiz arama alanı tespit edildi. Arama iptal edildi.");
				return GetSafeResult();
			}
			using (Mat mat = _capturer.Capture())
			{
				mat.CopyTo(_screenBuffer);
			}
			MatchMode mode = _config.Mode;
			if (1 == 0)
			{
			}
			object result = mode switch
			{
				MatchMode.SingleMatch => SearchSingleMatch(), 
				MatchMode.CountMatches => SearchCountMatches(), 
				_ => throw new InvalidOperationException($"Bilinmeyen MatchMode: {_config.Mode}"), 
			};
			if (1 == 0)
			{
			}
			return result;
		}
		catch (Exception)
		{
			return (_config.Mode == MatchMode.SingleMatch) ? null : ((object)0);
		}
	}

	private bool IsValidSearchArea(Rectangle area)
	{
		return area.Width > 0 && area.Height > 0 && area.X >= 0 && area.Y >= 0 && area.Width < 5000 && area.Height < 5000;
	}

	private object GetSafeResult()
	{
		return (_config.Mode == MatchMode.SingleMatch) ? null : ((object)0);
	}

	private System.Drawing.Point? SearchSingleMatch()
	{
		MatchResult matchResult = _matcher.TryMatch(_screenBuffer);
		if (matchResult.IsMatch)
		{
			return matchResult.MatchPoint;
		}
		return null;
	}

	private int SearchCountMatches()
	{
		return (_matcher as TemplateMatcher)?.CountMatches(_screenBuffer) ?? 0;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_screenBuffer.Dispose();
			_matcher.Dispose();
			_capturer.Dispose();
			_disposed = true;
		}
	}
}
