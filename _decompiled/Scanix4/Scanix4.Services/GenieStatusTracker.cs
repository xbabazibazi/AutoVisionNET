namespace Scanix4.Services;

/// <summary>
/// Whether Genie is currently believed to be active, from the same debounced reading
/// <see cref="Scanix4.Services.TaskCategories.GenieTasks"/> already uses to drive the
/// macro-toggle callbacks. Exists so <see cref="Scanix4.Services.ActionCategories.GenieActions.OnStartGenie"/>
/// can skip clicking the Genie button when it is already on - the button toggles rather than
/// only starting, so a redundant "start" (e.g. a job-wide remote command re-sent to fix one
/// character) was turning it back off on every other character that already had it running.
/// </summary>
public static class GenieStatusTracker
{
	public static bool IsActive { get; private set; }

	/// <summary>
	/// False until the Genie status scan has actually produced a reading. Anything gating its
	/// own behaviour on <see cref="IsActive"/> must check this first: on a setup where the Genie
	/// icon region was never drawn, the scan never runs and IsActive simply stays false forever,
	/// which would otherwise read as a confident "Genie is off" and silently disable that gate's
	/// feature.
	/// </summary>
	public static bool HasReading { get; private set; }

	public static void SetActive(bool isActive)
	{
		IsActive = isActive;
		HasReading = true;
	}
}
