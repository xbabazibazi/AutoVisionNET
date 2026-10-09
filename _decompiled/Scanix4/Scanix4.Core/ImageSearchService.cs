using System;
using System.Drawing;
using System.IO;
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

	private bool _hasWarnedInvalidSearchArea;

	/// <summary>False when this step has no usable region or template and will never match, so a
	/// workflow can say which step it is stuck on instead of just quietly finding nothing.</summary>
	public bool IsUsable => _matcher != null && _capturer != null;

	/// <summary>The template this step is looking for - used in diagnostics.</summary>
	public string TemplatePath => _config.TemplatePath;

	/// <summary>The screen region this step searches - used in diagnostics.</summary>
	public Rectangle SearchArea => _config.SearchArea;

	/// <summary>The confidence a match has to reach - used in diagnostics.</summary>
	public double Threshold => _config.Threshold;

	/// <summary>
	/// Confidence of the most recent search, and the best ever reached by this step. Together
	/// they turn "it just does not work" into something readable: a best of ~0 means the region
	/// is showing something else entirely, while a best just under the threshold means the
	/// template is right and the bar is set too high.
	/// </summary>
	public double LastConfidence { get; private set; }

	public double BestConfidence { get; private set; }

	public ImageSearchService(SearchConfig config, Logger logger = null)
	{
		_config = config ?? throw new ArgumentNullException("config");
		_logger = logger ?? Logger.Instance;
		DisplayScalingCheck.WarnIfNonStandardScaling(_logger);
		if (!IsValidSearchArea(_config.SearchArea))
		{
			_logger.LogWarning($"Geçersiz arama alanı: {_config.SearchArea}. Servis pasif moda alındı.");
			return;
		}
		// A template file that is simply not there means "this slot has not been assigned yet",
		// which is a normal state for optional steps - not a failure worth alarming about.
		if (!TemplateFileExists(_config.TemplatePath))
		{
			_logger.LogInformation("Şablon atanmamış (" + _config.TemplatePath + "). Bu adım, görsel atanana kadar pasif.");
			return;
		}
		try
		{
			_matcher = new TemplateMatcher(_config.TemplatePath, _config.Threshold, _config.SearchArea, _config.UseColor, _logger);
			_capturer = new ScreenCapturer(_config.SearchArea, _config.UseColor, _logger);
			_screenBuffer = new Mat();
		}
		catch (Exception ex)
		{
			_logger.LogWarning($"Gorsel arama servisi baslatilamadi (sablon: {_config.TemplatePath}): {ex.Message}. Servis pasif moda alindi.");
			_matcher?.Dispose();
			_capturer?.Dispose();
			_matcher = null;
			_capturer = null;
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
				if (!_hasWarnedInvalidSearchArea)
				{
					_hasWarnedInvalidSearchArea = true;
					_logger.LogWarning("Geçersiz arama alanı tespit edildi. Arama iptal edildi.");
				}
				return GetSafeResult();
			}
			_hasWarnedInvalidSearchArea = false;
			if (_matcher == null || _capturer == null)
			{
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
			VisualVerificationTracker.Report(mode == MatchMode.SingleMatch ? (result != null) : ((int)result > 0));
			return result;
		}
		catch (Exception)
		{
			return (_config.Mode == MatchMode.SingleMatch) ? null : ((object)0);
		}
	}

	private static bool TemplateFileExists(string templatePath)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(templatePath))
			{
				return false;
			}
			if (File.Exists(templatePath))
			{
				return true;
			}
			return File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templatePath));
		}
		catch
		{
			return true;   // let the normal load path report anything unexpected
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
		LastConfidence = matchResult.Confidence;
		if (matchResult.Confidence > BestConfidence)
		{
			BestConfidence = matchResult.Confidence;
		}
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
			_screenBuffer?.Dispose();
			_matcher?.Dispose();
			_capturer?.Dispose();
			_disposed = true;
		}
	}
}
