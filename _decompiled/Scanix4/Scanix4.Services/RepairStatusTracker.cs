namespace Scanix4.Services;

/// <summary>
/// Whether the most recent repair attempt (armor or weapon - this doesn't distinguish, the
/// operator just wants "is repair working") actually got the broken item fixed. A single click
/// doesn't tell you that by itself, since the broken icon can legitimately still be there for a
/// moment after clicking; this counts consecutive attempts at the SAME broken icon and only calls
/// it a failure once enough of them have happened in a row that the icon is clearly not going
/// away (e.g. out of repair hammers, or the item can't be repaired at all).
/// </summary>
public static class RepairStatusTracker
{
	private const int FailAfterConsecutiveAttempts = 5;

	private static int _consecutiveAttempts;

	public static bool? LastOutcomeOk { get; private set; }

	public static void ReportAttempt()
	{
		_consecutiveAttempts++;
		if (_consecutiveAttempts >= FailAfterConsecutiveAttempts)
		{
			LastOutcomeOk = false;
		}
	}

	/// <summary>Called on every reading where the broken icon is NOT seen - a cheap no-op unless
	/// a repair was actually in progress, in which case the icon going away means it worked.</summary>
	public static void ReportResolved()
	{
		if (_consecutiveAttempts > 0)
		{
			LastOutcomeOk = true;
		}
		_consecutiveAttempts = 0;
	}
}
