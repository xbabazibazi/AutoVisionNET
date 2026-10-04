using System;
using System.Collections.Generic;
using System.Linq;

namespace LicenseCore;

public sealed class LicenseInfo
{
	public string CustomerName { get; init; }

	public DateTime IssuedUtc { get; init; }

	public DateTime ExpiresUtc { get; init; }

	/// <summary>
	/// Short identifier for this exact key, derived from its text. It is what goes on the
	/// revocation list, so it has to be stable and short enough to read out loud.
	/// </summary>
	public string LicenseId { get; init; }

	/// <summary>
	/// Machine codes this licence may run on. Empty means unbound - the shape every licence had
	/// before machine binding existed, and the shape still issued today, so keys already in
	/// customers' hands keep working.
	/// </summary>
	public IReadOnlyList<string> MachineIds { get; init; } = Array.Empty<string>();

	public bool IsMachineBound => MachineIds != null && MachineIds.Count > 0;

	/// <summary>An unbound licence runs anywhere; a bound one only on a machine it names.</summary>
	public bool IsForThisMachine => !IsMachineBound || MachineIds.Any(MachineId.Matches);

	// Expiry is measured against LicenseClock, not DateTime.UtcNow, so winding the PC clock back
	// does not hand out extra days.
	public bool IsExpired => LicenseClock.UtcNow > ExpiresUtc;

	public TimeSpan TimeRemaining => ExpiresUtc - LicenseClock.UtcNow;
}
