using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Scanix4.Interfaces;
using Scanix4.Models;

namespace Scanix4.Services.TaskCategories;

public class SnapNetTasks : ITaskCategory
{
	private readonly ActionCenter _actionCenter;

	private readonly Rectangle DefaultSearchArea;

	private const int SM_CXSCREEN = 0;

	private const int SM_CYSCREEN = 1;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	[DllImport("user32.dll")]
	private static extern int GetSystemMetrics(int nIndex);

	public SnapNetTasks(ActionCenter actionCenter)
	{
		int systemMetrics = GetSystemMetrics(0);
		int systemMetrics2 = GetSystemMetrics(1);
		DefaultSearchArea = new Rectangle(0, 0, systemMetrics, systemMetrics2);
		_actionCenter = actionCenter;
		CreateTasks();
	}

	private void CreateTasks()
	{
		Tasks.Add(new SearchTask
		{
			TaskId = "SnapNetReReRe",
			Config = new SearchConfig
			{
				TemplatePath = "Images/GenieStart.jpg",
				SearchArea = DefaultSearchArea
			},
			Mode = SearchMode.SnapNet
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "SnapNetStartGenie",
			Config = new SearchConfig
			{
				TemplatePath = "Images/GenieStart.jpg",
				SearchArea = DefaultSearchArea
			},
			Mode = SearchMode.SnapNet
		});
		Tasks.Add(new SearchTask
		{
			TaskId = "SnapNetWhellOfFun",
			Config = new SearchConfig
			{
				TemplatePath = "Images/GenieStart.jpg",
				SearchArea = DefaultSearchArea
			},
			Mode = SearchMode.SnapNet
		});
	}
}
