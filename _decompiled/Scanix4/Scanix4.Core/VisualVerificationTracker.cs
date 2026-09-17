using System;

namespace Scanix4.Core;

public static class VisualVerificationTracker
{
	public static bool? LastSuccess { get; private set; }

	public static DateTime LastUpdateUtc { get; private set; }

	public static void Report(bool success)
	{
		LastSuccess = success;
		LastUpdateUtc = DateTime.UtcNow;
	}
}
