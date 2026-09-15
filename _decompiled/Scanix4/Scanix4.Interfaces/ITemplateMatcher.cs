using System;
using OpenCvSharp;

namespace Scanix4.Interfaces;

public interface ITemplateMatcher : IDisposable
{
	MatchResult TryMatch(Mat screen);

	int CountMatches(Mat screen);
}
