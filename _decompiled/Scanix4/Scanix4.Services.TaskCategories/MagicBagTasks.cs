using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class MagicBagTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public MagicBagTasks(ActionCenter actionCenter)
	{
		DefaultSearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.MagicBag.GetRectangle();
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "OpenMagicBag",
			Config = new SearchConfig
			{
				TemplatePath = "Images/OpenMagicBag.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._magicBagActions.MoveAndLeftClick,
				OnMatchNotFound = null,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "OpenMagicBag2",
			Config = new SearchConfig
			{
				TemplatePath = "Images/OpenMagicBag.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._magicBagActions.MoveAndLeftClick,
				OnMatchNotFound = null,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CheckRepairedTomahawkOnFirstMagicBag",
			Config = new SearchConfig
			{
				TemplatePath = "Images/RepairedTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = null,
				OnMatchNotFound = null,
				UseColor = true,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CheckRepairedTomahawkOnFirstMagicBag2",
			Config = new SearchConfig
			{
				TemplatePath = "Images/RepairedTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = delegate(Point point)
				{
					_actionCenter._magicBagActions.SimulateRightClick(point);
				},
				OnMatchNotFound = null,
				UseColor = true,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CheckRepairedTomahawkOnSecondMagicBag2",
			Config = new SearchConfig
			{
				TemplatePath = "Images/RepairedTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = delegate(Point point)
				{
					_actionCenter._magicBagActions.SimulateRightClick(point);
				},
				OnMatchNotFound = null,
				UseColor = true,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CheckRepairedTomahawkOnSecondMagicBag",
			Config = new SearchConfig
			{
				TemplatePath = "Images/RepairedTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = null,
				OnMatchNotFound = null,
				UseColor = true,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "FindRepairedTomahawkOnMagicBag",
			Config = new SearchConfig
			{
				TemplatePath = "Images/RepairedTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._magicBagActions.MoveAndLeftButtonUp,
				OnMatchNotFound = _actionCenter._magicBagActions.LeftButtonUp,
				UseColor = true,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CloseMagicBag",
			Config = new SearchConfig
			{
				TemplatePath = "Images/CloseMagicBag.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._magicBagActions.MoveAndLeftClick,
				OnMatchNotFound = null,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "OpenSecondMagicBag",
			Config = new SearchConfig
			{
				TemplatePath = "Images/SecondMagicBag.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._magicBagActions.MoveAndLeftClick,
				OnMatchNotFound = null
			},
			Mode = SearchMode.Single
		});
	}
}
