using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class InventoryTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public InventoryTasks(ActionCenter actionCenter)
	{
		DefaultSearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.Inventory.GetRectangle();
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "FindBrokenTomahawkOnInventory",
			Config = new SearchConfig
			{
				TemplatePath = "Images/BrokenTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._inventortyActions.MoveAndLeftButtonDown,
				OnMatchNotFound = null,
				UseColor = true,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CheckRepairedTomahawkOnInventory",
			Config = new SearchConfig
			{
				TemplatePath = "Images/RepairedTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._inventortyActions.MoveAndRightClick,
				OnMatchNotFound = null,
				UseColor = true,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "EquipTomahawk",
			Config = new SearchConfig
			{
				TemplatePath = "Images/RepairedTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._inventortyActions.MoveAndRightClick,
				OnMatchNotFound = null,
				UseColor = true,
				Threshold = 0.99
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "InventorySlotAlert",
			Config = new SearchConfig
			{
				TemplatePath = "Images/EmptyInventorySlot.jpg",
				SearchArea = DefaultSearchArea,
				Threshold = 0.99,
				IntervalMs = 5000,
				UseColor = true,
				OnMatchFound = null,
				OnMatchNotFound = null,
				OnFoundCount = _actionCenter._inventortyActions.OnInventorySlotAlert,
				Mode = MatchMode.CountMatches
			},
			Mode = SearchMode.Continuous
		});
	}
}
