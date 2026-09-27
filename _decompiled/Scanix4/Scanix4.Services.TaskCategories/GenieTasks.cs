using System;
using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class GenieTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	private readonly Rectangle _genieStatusArea;

	private readonly Action<Point> _onMatchFoundAction;

	private readonly Action _onMatchNotFoundAction;

	// The status icon can flicker/animate from one 3s poll to the next, so a single match or miss
	// isn't trusted on its own - that was flipping the Genie-started macro on and off every few
	// seconds ("çalışıyor kafasına göre"). Requiring a couple of consecutive, consistent readings
	// before actually calling the real callback smooths that out.
	private const int RequiredConsecutiveReadings = 2;

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
				TemplatePath = TemplateResolver.Resolve("GenieStatus", "Images/GenieStart.jpg"),
				SearchArea = _genieStatusArea,
				// 0.9997 demands a near pixel-perfect match - normal JPEG/rendering noise never
				// clears it, so this never actually fired OnMatchFound and the Genie-start macro
				// hookup (OnIsGenieStart) never ran no matter how correctly the template was set up.
				Threshold = 0.95,
				IntervalMs = 3000,
				UseColor = false,
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
		if (_consecutiveMatches >= RequiredConsecutiveReadings)
		{
			_onMatchFoundAction?.Invoke(coordinates);
		}
	}

	private void OnGenieStatusNoMatch()
	{
		_consecutiveMatches = 0;
		_consecutiveMisses++;
		if (_consecutiveMisses >= RequiredConsecutiveReadings)
		{
			_onMatchNotFoundAction?.Invoke();
		}
	}
}
