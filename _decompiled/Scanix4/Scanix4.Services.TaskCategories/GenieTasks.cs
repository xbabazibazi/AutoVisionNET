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

	private readonly Action<Point> _onMatchFoundAction;

	private readonly Action _onMatchNotFoundAction;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public GenieTasks(ActionCenter actionCenter, Action<Point> onMatchFoundAction, Action onMatchNotFoundAction)
	{
		_onMatchFoundAction = onMatchFoundAction;
		_onMatchNotFoundAction = onMatchNotFoundAction;
		DefaultSearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.Genie.GetRectangle();
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
				SearchArea = DefaultSearchArea,
				Threshold = 0.9997,
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
