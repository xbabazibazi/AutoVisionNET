using System;
using OpenCvSharp;

namespace Scanix4.Interfaces;

public interface IScreenCapturer : IDisposable
{
	Mat Capture();
}
