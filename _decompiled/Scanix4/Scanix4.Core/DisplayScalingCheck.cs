using System;
using System.Drawing;
using SimpleLogger;

namespace Scanix4.Core;

internal static class DisplayScalingCheck
{
	private const float ReferenceDpi = 96f;

	private const float ToleranceFraction = 0.01f;

	private static bool _hasWarned;

	private static readonly object _lock = new object();

	public static void WarnIfNonStandardScaling(Logger logger)
	{
		if (_hasWarned)
		{
			return;
		}
		lock (_lock)
		{
			if (_hasWarned)
			{
				return;
			}
			_hasWarned = true;
			float dpi;
			try
			{
				using Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
				dpi = graphics.DpiX;
			}
			catch (Exception)
			{
				return;
			}
			float scalePercent = dpi / ReferenceDpi * 100f;
			if (Math.Abs(dpi - ReferenceDpi) > ReferenceDpi * ToleranceFraction)
			{
				logger.LogWarning($"Windows görüntü ölçeklendirmesi %{scalePercent:F0} olarak tespit edildi (önerilen: %100). Şablon görselleri farklı bir ölçeklendirmede alındıysa görsel eşleşme (template matching) başarısız olabilir. Oyun ekranının bulunduğu monitörün ölçeklendirmesini %100 yapmanız veya şablonları bu makinede yeniden almanız önerilir.");
			}
		}
	}
}
