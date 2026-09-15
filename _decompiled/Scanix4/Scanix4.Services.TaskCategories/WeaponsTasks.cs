using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

public class WeaponsTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public WeaponsTasks(ActionCenter actionCenter)
	{
		DefaultSearchArea = Settings.Instance.ScreenCapture.RectanglesSettings.Weapons.GetRectangle();
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "RepairArmors",
			Config = new SearchConfig
			{
				TemplatePath = "Images/BrokenFullPlateArmorPauldron.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._weaponsActions.OnMatchFoundBrokenFullPlateArmorPauldron,
				Threshold = 0.983,
				IntervalMs = 2000
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "RepairWeapons",
			Config = new SearchConfig
			{
				TemplatePath = "Images/BrokenTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._weaponsActions.OnMatchFoundRepairTomahawk,
				Threshold = 0.99,
				IntervalMs = 1000,
				UseColor = true
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "SwapTomahawk",
			Config = new SearchConfig
			{
				TemplatePath = "Images/BrokenTomahawk.jpg",
				SearchArea = DefaultSearchArea,
				OnMatchFound = _actionCenter._weaponsActions.MoveAndRightClick,
				Threshold = 0.99,
				IntervalMs = 5000,
				UseColor = true
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CheckRightHandIsEmpty",
			Config = new SearchConfig
			{
				TemplatePath = "Images/EmptyRightHand.jpg",
				SearchArea = DefaultSearchArea,
				Threshold = 0.9
			},
			Mode = SearchMode.Single
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "CheckLeftHandIsEmpty",
			Config = new SearchConfig
			{
				TemplatePath = "Images/EmptyLeftHand.jpg",
				SearchArea = DefaultSearchArea,
				Threshold = 0.9
			},
			Mode = SearchMode.Single
		});
	}
}
