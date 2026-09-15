using System.Drawing;

namespace Scanix4.Interfaces;

public struct MatchResult
{
	public bool IsMatch { get; }

	public Point MatchPoint { get; }

	public double Confidence { get; }

	public static MatchResult NoMatch => new MatchResult(isMatch: false, Point.Empty, 0.0);

	public MatchResult(bool isMatch, Point matchPoint, double confidence)
	{
		IsMatch = isMatch;
		MatchPoint = matchPoint;
		Confidence = confidence;
	}
}
