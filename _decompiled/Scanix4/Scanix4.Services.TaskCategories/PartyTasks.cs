using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class PartyTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public PartyTasks(ActionCenter actionCenter)
	{
		DefaultSearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.Party.GetRectangle();
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "HandlePartyMemberDeath",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("HandlePartyMemberDeath", "Images/Dead.jpg"),
				SearchArea = DefaultSearchArea,
				Threshold = 0.87,
				IntervalMs = 5000,
				UseColor = false,
				OnMatchFound = _actionCenter._partyActions.OnHandlePartyMemberDeath,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "PartyHeader",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("PartyHeader", "Images/PartyHeader.jpg"),
				SearchArea = DefaultSearchArea,
				Threshold = 0.8,
				IntervalMs = 500,
				UseColor = false,
				OnMatchFound = _actionCenter._partyActions.OnFoundPartyHeader,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "BreakParty",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("BreakParty", "Images/BreakParty.jpg"),
				SearchArea = DefaultSearchArea,
				Threshold = 0.8,
				IntervalMs = 500,
				UseColor = false,
				OnMatchFound = _actionCenter._partyActions.OnBreakParty,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CureDB",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("CureDB", "Images/DB.jpg"),
				SearchArea = DefaultSearchArea,
				Threshold = 0.991,
				UseColor = true,
				IntervalMs = 2000,
				OnMatchFound = _actionCenter._partyActions.OnCureDb,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Continuous
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "Town",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("Town", "Images/Town.jpg"),
				SearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.Town.GetRectangle(),
				OnMatchFound = _actionCenter._partyActions.OnTown,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "PartyMemberCount",
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve("PartyMemberCount", "Images/PartyCount.jpg"),
				SearchArea = DefaultSearchArea,
				Threshold = 0.9,
				IntervalMs = 5000,
				UseColor = true,
				OnMatchFound = null,
				OnMatchNotFound = null,
				OnFoundCount = _actionCenter._partyActions.OnPartyCount,
				Mode = MatchMode.CountMatches
			},
			Mode = SearchMode.Continuous
		});
	}
}
