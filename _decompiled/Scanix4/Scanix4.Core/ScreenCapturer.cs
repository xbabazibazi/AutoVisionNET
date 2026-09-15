using System;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Scanix4.Interfaces;
using SimpleLogger;

namespace Scanix4.Core;

public class ScreenCapturer : IScreenCapturer, IDisposable
{
	private readonly Rectangle _area;

	private readonly ColorConversionCodes _conversion;

	private readonly Logger _logger;

	private readonly Bitmap _bitmapBuffer;

	private readonly object _lock = new object();

	private bool _disposed;

	public ScreenCapturer(Rectangle area, bool useColor, Logger logger)
	{
		_area = area;
		_conversion = (useColor ? ColorConversionCodes.BGR2RGB : ColorConversionCodes.BGR2GRAY);
		_logger = logger ?? Logger.Instance;
		_bitmapBuffer = new Bitmap(_area.Width, _area.Height);
	}

	public Mat Capture()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("ScreenCapturer");
		}
		lock (_lock)
		{
			try
			{
				using (Graphics graphics = Graphics.FromImage(_bitmapBuffer))
				{
					graphics.CopyFromScreen(_area.X, _area.Y, 0, 0, _area.Size);
				}
				using Mat mat = _bitmapBuffer.ToMat();
				Mat mat2 = new Mat();
				Cv2.CvtColor(mat, mat2, _conversion);
				return mat2;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			lock (_lock)
			{
				_bitmapBuffer.Dispose();
				_disposed = true;
			}
		}
	}
}
