using System;
using System.Threading;

namespace Scanix4.Services;

/// <summary>
/// The repair dialog opens on top of the inventory region, so an empty-slot scan taken while it
/// is up counts whatever the dialog is showing instead of the bag - which produced false "bag
/// full" alarms. Actions that pop such a window announce it here, and the inventory alert throws
/// away readings until the window is expected to be gone again.
///
/// A timed window is used rather than "is the repair workflow running", because the repair
/// workflow stays running for as long as the feature is enabled - gating on that would suppress
/// the inventory scan permanently.
/// </summary>
public static class InventoryScanSuppressor
{
	/// <summary>
	/// How long a single repair interaction is assumed to keep the inventory region covered.
	/// Each new repair action pushes the window forward, so a burst of repairs stays suppressed
	/// throughout.
	/// </summary>
	public static readonly TimeSpan DefaultSuppressionWindow = TimeSpan.FromSeconds(15.0);

	private static long _suppressedUntilTicks;

	// Covers manual interactions the bot has no visual hook for (walking to an NPC and repairing
	// by hand, trading, anything else that puts a window over the inventory) - a timed window
	// can't be sized correctly for these since their duration is whatever the operator takes, so
	// this is an explicit on/off the operator flips with a hotkey instead.
	private static bool _manuallySuppressed;

	public static bool IsSuppressed => _manuallySuppressed || DateTime.UtcNow.Ticks < Interlocked.Read(ref _suppressedUntilTicks);

	/// <summary>Flips manual suppression and returns the new state, for the caller to report to the operator.</summary>
	public static bool ToggleManual()
	{
		_manuallySuppressed = !_manuallySuppressed;
		return _manuallySuppressed;
	}

	public static void Suppress()
	{
		SuppressFor(DefaultSuppressionWindow);
	}

	/// <summary>
	/// Extends the suppression window. Never shortens an already longer one.
	/// </summary>
	public static void SuppressFor(TimeSpan duration)
	{
		long until = DateTime.UtcNow.Add(duration).Ticks;
		while (true)
		{
			long current = Interlocked.Read(ref _suppressedUntilTicks);
			if (until <= current)
			{
				return;
			}
			if (Interlocked.CompareExchange(ref _suppressedUntilTicks, until, current) == current)
			{
				return;
			}
		}
	}

	/// <summary>Clears the window. Intended for tests and for a clean restart.</summary>
	public static void Reset()
	{
		Interlocked.Exchange(ref _suppressedUntilTicks, 0L);
		_manuallySuppressed = false;
	}
}
