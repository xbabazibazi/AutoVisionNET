using System;

namespace LicenseCore;

/// <summary>
/// The clock licence expiry is measured against. It is the system clock, except that it never
/// goes backwards: the furthest point in time the app has ever seen is remembered in the sealed
/// activation store and acts as a floor.
/// </summary>
/// <remarks>
/// Winding the PC clock back used to turn a 5-day trial into an unlimited one. Rather than refuse
/// to run when the clock looks wrong - which would punish anyone whose CMOS battery died or who
/// fixed a genuinely wrong date - the floor simply means a rollback buys no extra time. The app
/// keeps working either way, it just stops being a way to extend a licence.
/// </remarks>
public static class LicenseClock
{
	/// <summary>
	/// A reading this far ahead of the recorded floor is treated as a wild clock (dead CMOS
	/// battery, a date typed in by hand) and is not persisted, so one bad boot cannot permanently
	/// kill a licence that still has time left on it.
	/// </summary>
	private static readonly TimeSpan MaxPlausibleJump = TimeSpan.FromDays(400.0);

	/// <summary>Small slack so routine NTP corrections and DST edges are not read as tampering.</summary>
	private static readonly TimeSpan RollbackTolerance = TimeSpan.FromHours(6.0);

	private static DateTime _floorUtc = DateTime.MinValue;

	/// <summary>UTC now, but never earlier than the furthest moment already observed.</summary>
	public static DateTime UtcNow
	{
		get
		{
			DateTime systemNow = DateTime.UtcNow;
			return (systemNow < _floorUtc) ? _floorUtc : systemNow;
		}
	}

	/// <summary>True when the system clock sits meaningfully behind what we have already seen.</summary>
	public static bool RollbackDetected => DateTime.UtcNow + RollbackTolerance < _floorUtc;

	public static DateTime FloorUtc => _floorUtc;

	/// <summary>Seeds the floor from the sealed store at startup.</summary>
	public static void SetFloor(DateTime utc)
	{
		if (utc > _floorUtc)
		{
			_floorUtc = utc;
		}
	}

	/// <summary>
	/// Whether a clock reading is a believable step forward from the recorded floor. Pure, so the
	/// guard can be exercised directly instead of only through the real system clock.
	/// </summary>
	public static bool IsPlausibleAdvance(DateTime floorUtc, DateTime nowUtc)
	{
		if (nowUtc <= floorUtc)
		{
			return false;
		}
		// A first run has no floor yet, so any reading is as good as we are going to get.
		return floorUtc == DateTime.MinValue || nowUtc - floorUtc <= MaxPlausibleJump;
	}

	/// <summary>
	/// Advances the floor to the current reading when that reading is plausible, and reports
	/// whether the caller should persist it.
	/// </summary>
	public static bool TryAdvance(out DateTime newFloorUtc)
	{
		newFloorUtc = _floorUtc;
		DateTime systemNow = DateTime.UtcNow;
		if (!IsPlausibleAdvance(_floorUtc, systemNow))
		{
			return false;
		}
		_floorUtc = systemNow;
		newFloorUtc = systemNow;
		return true;
	}
}
