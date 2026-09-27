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

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public GenieTasks(ActionCenter actionCenter, Action<Point> onMatchFoundAction, Action onMatchNotFoundAction)
	{
		_onMatchFoundAction = onMatchFoundAction;
		_onMatchNotFoundAction = onMatchNotFoundAction;
		var rectangles = Settings.Instance.ScreenCapture.RectanglesSettings;
		DefaultSearchArea = rectangles.Genie.GetRectangle();
		// The "Genie is active" icon and the start button don't share a screen location, so the
		// status check needs its own area. Unset (never configured) falls back to the button's
		// area, matching the old behaviour rather than silently never matching anything.
		Rectangle genieStatus = rectangles.GenieStatus.GetRectangle();
		_genieStatusArea = (genieStatus.Width > 0 && genieStatus.Height > 0) ? genieStatus : DefaultSearchArea;
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
				OnMatchFound = _onMatchFoundAction,
				OnMatchNotFound = _onMatchNotFoundAction
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
}
