using System;

namespace Scanix4.Services;

/// <summary>
/// Publishes the most recent empty-slot reading so the UI can show what the scanner is actually
/// seeing right now. Without this the threshold has to be set blind: there is no way to tell
/// whether the scan sees 0, 3 or 30 slots, or whether the search region / template is wrong.
/// </summary>
public static class InventorySlotMonitor
{
	private static readonly object _lock = new object();

	private static int _lastEmptySlots;

	private static DateTime _lastReadingUtc = DateTime.MinValue;

	private static bool _lastWasSuppressed;

	private static bool _lastLooksClosed;

	/// <summary>A reading older than this means the scan is not running at all.</summary>
	public static readonly TimeSpan StaleAfter = TimeSpan.FromSeconds(15.0);

	public static void Report(int emptySlots, bool suppressed, bool looksClosed)
	{
		lock (_lock)
		{
			_lastEmptySlots = emptySlots;
			_lastWasSuppressed = suppressed;
			_lastLooksClosed = looksClosed;
			_lastReadingUtc = DateTime.UtcNow;
		}
	}

	/// <summary>
	/// Returns false when no reading has arrived yet or the last one is stale, which means the
	/// inventory scan is not currently running. <paramref name="looksClosed"/> is true when the
	/// last 0 reading was judged to mean "the inventory window isn't open" rather than "the bag is
	/// genuinely full" - see InventortyActions for how that call is made.
	/// </summary>
	public static bool TryGetLastReading(out int emptySlots, out bool suppressed, out bool looksClosed, out TimeSpan age)
	{
		lock (_lock)
		{
			emptySlots = _lastEmptySlots;
			suppressed = _lastWasSuppressed;
			looksClosed = _lastLooksClosed;
			if (_lastReadingUtc == DateTime.MinValue)
			{
				age = TimeSpan.MaxValue;
				return false;
			}
			age = DateTime.UtcNow - _lastReadingUtc;
			return age <= StaleAfter;
		}
	}

	public static void Reset()
	{
		lock (_lock)
		{
			_lastReadingUtc = DateTime.MinValue;
			_lastEmptySlots = 0;
			_lastWasSuppressed = false;
			_lastLooksClosed = false;
		}
	}
}
