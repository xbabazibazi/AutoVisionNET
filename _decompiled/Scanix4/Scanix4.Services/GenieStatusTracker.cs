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

	public static void SetActive(bool isActive)
	{
		IsActive = isActive;
	}
}
