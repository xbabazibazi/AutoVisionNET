using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class BuffLineTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public BuffLineTasks(ActionCenter actionCenter)
	{
		DefaultSearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.BuffLine.GetRectangle();
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "StartGenieAfterTp",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("StartGenieAfterTp", "Images/IceResistance.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = null,
				IntervalMs = 1000,
				OnMatchNotFound = null,
				Threshold = 0.903
			},
			Mode = SearchMode.Continuous
		});
		// Closes the IceResist -> start-Genie chain: it answers "is the Genie-active icon on
		// screen right now" so the workflow can finish instead of looping back and clicking the
		// Genie button again.
		// A separate task id rather than reusing "GenieStatus" on purpose: that task's callbacks
		// drive the debounce counters behind GenieStatusTracker, and letting a second workflow
		// drive them too would corrupt that state machine. Hence no callbacks here - this step is
		// a pure question, it changes nothing.
		// The template is resolved through the SAME override key as the status task, so assigning
		// the status image once in Templates covers both.
		Tasks.Add(new SearchTask
		{
			TaskId = "GenieActiveCheck",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("GenieStatus", "Images/GenieStatusActive.jpg"),
				SearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.GenieStatus.GetRectangle(),
				Threshold = 0.95,
				UseColor = true,
				IntervalMs = 1000,
				OnMatchFound = null,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "DeleteResistance",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("DeleteResistance", "Images/IceResistance.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._buffLineActions.MoveAndDoubleLeftClick,
				IntervalMs = 1000,
				OnMatchNotFound = null,
				Threshold = 0.9
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "Undy",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("Undy", "Images/Undy.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._buffLineActions.OnUndy,
				OnMatchNotFound = null,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "300Ac",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("300Ac", "Images/300Ac.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._buffLineActions.On300Ac,
				OnMatchNotFound = null,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "Sw",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("Sw", "Images/Sw.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._buffLineActions.OnSw,
				OnMatchNotFound = null,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "Wolf",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("Wolf", "Images/Wolf.jpg"),
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._buffLineActions.OnWolf,
				OnMatchNotFound = null,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
	}
}
