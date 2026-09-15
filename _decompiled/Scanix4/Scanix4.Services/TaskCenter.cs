using System;
using System.Collections.Generic;
using System.Drawing;
using InputManager;
using Scanix4.Models;
using Scanix4.Services.TaskCategories;
using SimpleLogger;

namespace Scanix4.Services;

public class TaskCenter
{
	public readonly Dictionary<string, SearchTask> _allTasks = new Dictionary<string, SearchTask>();

	public ActionCenter ActionCenter { get; set; }

	public TaskCenter(Alarm alarm, Logger logger, InputUtils inputUtils, Action<Point> onMatchFoundAction, Action onMatchNotFoundAction)
	{
		ActionCenter = new ActionCenter(alarm, logger, inputUtils);
		RegisterTasks(new RequestTasks(ActionCenter).Tasks);
		RegisterTasks(new PartyTasks(ActionCenter).Tasks);
		RegisterTasks(new GenieTasks(ActionCenter, onMatchFoundAction, onMatchNotFoundAction).Tasks);
		RegisterTasks(new BuffLineTasks(ActionCenter).Tasks);
		RegisterTasks(new SnapNetTasks(ActionCenter).Tasks);
		RegisterTasks(new WeaponsTasks(ActionCenter).Tasks);
		RegisterTasks(new MagicBagTasks(ActionCenter).Tasks);
		RegisterTasks(new InventoryTasks(ActionCenter).Tasks);
		RegisterTasks(new LeftBotMenuTasks(ActionCenter).Tasks);
	}

	private void RegisterTasks(List<SearchTask> tasks)
	{
		foreach (SearchTask task in tasks)
		{
			if (_allTasks.ContainsKey(task.TaskId))
			{
				throw new ArgumentException("TaskId çakışması: " + task.TaskId + " zaten kayıtlı.");
			}
			_allTasks[task.TaskId] = task;
		}
	}
}
