using System;

namespace LicenseCore;

public sealed class LicenseInfo
{
	public string CustomerName { get; init; }

	public DateTime IssuedUtc { get; init; }

	public DateTime ExpiresUtc { get; init; }

	public bool IsExpired => DateTime.UtcNow > ExpiresUtc;

	public TimeSpan TimeRemaining => ExpiresUtc - DateTime.UtcNow;
}
