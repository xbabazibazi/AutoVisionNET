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

	/// <summary>
	/// The moment this licence is measured against: the no-rollback clock, but never earlier than
	/// the licence's own issue date, because a licence cannot be in use before it was issued.
	/// </summary>
	/// <remarks>
	/// This is what makes a term hold on a freshly installed machine, where no stored clock floor
	/// exists yet and the first reading is therefore accepted whatever it says.
	/// Kept per-licence rather than pushed into LicenseClock's shared floor deliberately: that
	/// floor only ever moves forward, so one key carrying a wrong issue date would raise it for
	/// the whole process and could push an otherwise fine key into expiry - including the very
	/// next key the customer pastes into the activation dialog.
	/// Nothing here is persisted; it is re-derived from the signed payload on every launch, so
	/// wiping the activation store does not shake it off.
	/// </remarks>
	private DateTime EffectiveNow
	{
		get
		{
			DateTime now = LicenseClock.UtcNow;
			return (now < IssuedUtc) ? IssuedUtc : now;
		}
	}

	// Expiry is measured against LicenseClock, not DateTime.UtcNow, so winding the PC clock back
	// does not hand out extra days.
	public bool IsExpired => EffectiveNow > ExpiresUtc;

	public TimeSpan TimeRemaining => ExpiresUtc - EffectiveNow;
}
