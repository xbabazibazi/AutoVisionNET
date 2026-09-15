using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class RequestTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public RequestTasks(ActionCenter actionCenter)
	{
		DefaultSearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.AcceptParty.GetRectangle();
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "RequestParty",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("RequestParty", "Images/RequestParty.jpg"),
				SearchArea = DefaultSearchArea,
				Threshold = 0.8,
				IntervalMs = 1000,
				UseColor = false,
				OnMatchFound = _actionCenter._requestActions.OnPartyFound,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "katadora",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("katadora", "Images/katadora.jpg"),
				SearchArea = DefaultSearchArea,
				Threshold = 0.8,
				IntervalMs = 2000,
				UseColor = false,
				OnMatchFound = null,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CheckParty",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("CheckParty", "Images/RequestParty.jpg"),
				SearchArea = DefaultSearchArea,
				Threshold = 0.8,
				IntervalMs = 1000,
				UseColor = false,
				OnMatchFound = _actionCenter._requestActions.OnPartyFound,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "WhellOfFunButton",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("WhellOfFunButton", "Images/WhellOfFunButton.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._requestActions.MoveAndLeftClickWithDelay,
				OnMatchNotFound = null,
				Threshold = 0.9
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "WhellOfFunPushButton",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("WhellOfFunPushButton", "Images/WhellOfFunPushButton.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._requestActions.MoveAndLeftClickWithDelay,
				OnMatchNotFound = null,
				Threshold = 0.9
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "WhellOfFunYesButton",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("WhellOfFunYesButton", "Images/WhellOfFunYesButton.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._requestActions.WhellOfFunYesButton,
				OnMatchNotFound = _actionCenter._requestActions.WhellOfFunYesButton,
				Threshold = 0.94
			},
			Mode = SearchMode.Single
		});
	}
}
