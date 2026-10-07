using System;
using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using Scanix4.Services;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class GenieTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	private readonly Rectangle _genieStatusArea;

	private readonly Action<Point> _onMatchFoundAction;

	private readonly Action _onMatchNotFoundAction;

	// Starting still waits for 2 consistent matches (6s) to avoid a one-frame flicker instantly
	// flipping the macro on. Stopping is intentionally instant (1 miss): log evidence showed
	// "Attack stopped" firing in the same millisecond as "Genie durduruldu" - the debounce here
	// was entirely in how long it took to CONFIRM the real-world stop, and a delayed stop means
	// the attack macro keeps swinging for real seconds after Genie is actually off, which is worse
	// than the rare false stop a brief icon occlusion might cause.
	private const int RequiredMatchReadings = 2;

	// Deliberately 1, not 2 - see the reasoning above RequiredMatchReadings. Raising it was tried
	// as an anti-oscillation measure and reverted: the oscillation came from OnStartGenie treating
	// "no reading at all" as "Genie is off", not from this constant, and slowing the stop down
	// means the attack macro keeps swinging for seconds after Genie has really stopped.
	private const int RequiredMissReadings = 1;

	private int _consecutiveMatches;

	private int _consecutiveMisses;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public GenieTasks(ActionCenter actionCenter, Action<Point> onMatchFoundAction, Action onMatchNotFoundAction)
	{
		_onMatchFoundAction = onMatchFoundAction;
		_onMatchNotFoundAction = onMatchNotFoundAction;
		var rectangles = Settings.Instance.ScreenCapture.RectanglesSettings;
		DefaultSearchArea = rectangles.Genie.GetRectangle();
		// The "Genie is active" icon and the start button don't share a screen location, so the
		// status check needs its own area. This deliberately does NOT fall back to the button's
		// area when unset: the button is visible exactly when Genie is OFF, so searching for the
		// status icon there instead risked stray matches against the button and flipped the macro
		// on/off backwards. Left unset, the search area is simply invalid and the status check
		// stays inactive (never calls OnMatchFound) until the operator configures its own area.
		_genieStatusArea = rectangles.GenieStatus.GetRectangle();
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "GenieStatus",
			Config = new SearchConfig
			{
				// Must match the DefaultPath used for this TaskId in TemplateManagerForm's Slots
				// list, or "reset to default" there and this fallback would disagree.
				TemplatePath = TemplateResolver.Resolve("GenieStatus", "Images/GenieStatusActive.jpg"),
				SearchArea = _genieStatusArea,
				// 0.9997 demands a near pixel-perfect match - normal JPEG/rendering noise never
				// clears it, so this never actually fired OnMatchFound and the Genie-start macro
				// hookup (OnIsGenieStart) never ran no matter how correctly the template was set up.
				Threshold = 0.95,
				IntervalMs = 3000,
				// The active-icon template is a small, mostly solid-colour block, exactly the kind
				// of image that grayscale matching confuses with other similarly-toned UI elements.
				// Its colour is the strongest distinguishing feature it has, so use it.
				UseColor = true,
				OnMatchFound = OnGenieStatusMatch,
				OnMatchNotFound = OnGenieStatusNoMatch
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "StartGenie",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("StartGenie", "Images/GenieStart.jpg"),
				SearchArea = DefaultSearchArea,
				IntervalMs = 1000,
				UseColor = false,
				OnMatchFound = _actionCenter._genieActions.OnStartGenie,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
	}

	private void OnGenieStatusMatch(Point coordinates)
	{
		_consecutiveMisses = 0;
		_consecutiveMatches++;
		if (_consecutiveMatches >= RequiredMatchReadings)
		{
			GenieStatusTracker.SetActive(isActive: true);
			_onMatchFoundAction?.Invoke(coordinates);
		}
	}

	private void OnGenieStatusNoMatch()
	{
		_consecutiveMatches = 0;
		_consecutiveMisses++;
		if (_consecutiveMisses >= RequiredMissReadings)
		{
			GenieStatusTracker.SetActive(isActive: false);
			_onMatchNotFoundAction?.Invoke();
		}
	}
}
