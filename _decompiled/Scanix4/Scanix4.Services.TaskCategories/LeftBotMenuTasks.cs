using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class LeftBotMenuTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public LeftBotMenuTasks(ActionCenter actionCenter)
	{
		DefaultSearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.LeftBotMenu.GetRectangle();
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "Event",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("Event", "Images/Event.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._magicBagActions.MoveAndLeftClick,
				IntervalMs = 1000,
				OnMatchNotFound = null,
				Threshold = 0.9
			},
			Mode = SearchMode.Single
		});
	}
}
