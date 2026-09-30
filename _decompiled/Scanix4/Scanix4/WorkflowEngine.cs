using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using InputManager;
using Scanix4.Interfaces;
using Scanix4.Services;
using Scanix4.Services.WorkFlows;
using SimpleLogger;

namespace Scanix4;

public class WorkflowEngine
{
	private TaskCenter _taskCenter;

	private readonly Alarm _alarm;

	private readonly InputUtils _inputUtils;

	private readonly Action<Point> _onMatchFoundAction;

	private readonly Action _onMatchNotFoundAction;

	private readonly Dictionary<string, WorkflowManager> _managers = new Dictionary<string, WorkflowManager>();

	private readonly Dictionary<string, Task> _running = new Dictionary<string, Task>();

	private readonly object _lock = new object();

	private readonly Logger _logger = Logger.Instance;

	private static readonly Dictionary<string, Type> _workflowTypes = new Dictionary<string, Type>
	{
		{
			"RequestParty",
			typeof(RequestPartyWorkflow)
		},
		{
			"HandlePartyMemberDeath",
			typeof(HandlePartyMemberDeathWorkFlow)
		},
		{
			"PartyMemberCount",
			typeof(PartyMemberCountWorkFlow)
		},
		{
			"GenieStatus",
			typeof(GenieStatusWorkFlow)
		},
		{
			"StartGenieAfterTp",
			typeof(StartGenieAfterTpWorkFlow)
		},
		{
			"RepairArmors",
			typeof(RepairArmorsWorkFlow)
		},
		{
			"CureDB",
			typeof(CureDBWorkFlow)
		},
		{
			"SwapTomahawk",
			typeof(SwapTomahawkWorkFlow)
		},
		{
			"SnapNetReReRe",
			typeof(SnapNetReReReWorkFlow)
		},
		{
			"SnapNetStartGenie",
			typeof(SnapNetStartGenieWorkFlow)
		},
		{
			"RepairWeapons",
			typeof(RepairWeaponsWorkFlow)
		},
		{
			"InventorySlotAlert",
			typeof(InventorySlotAlertWorkFlow)
		},
		{
			"SnapNetWhellOfFun",
			typeof(SnapNetWhellOfFunWorkFlow)
		}
	};

	public string WorkflowId => _managers.Values.FirstOrDefault()?.WorkflowId;

	public WorkflowEngine(Alarm alarm, InputUtils inputUtils, Action<Point> onMatchFoundAction, Action onMatchNotFoundAction)
	{
		_alarm = alarm;
		_inputUtils = inputUtils;
		_onMatchFoundAction = onMatchFoundAction;
		_onMatchNotFoundAction = onMatchNotFoundAction;
		_taskCenter = new TaskCenter(alarm, _logger, inputUtils, onMatchFoundAction, onMatchNotFoundAction);
	}

	/// <summary>
	/// Rebuilds the task graph from current Settings without restarting the whole app. Every task
	/// category reads its search area/template path once, in its constructor, so a region redrawn
	/// or a template re-uploaded in Settings never reached an already-running task before this -
	/// only relaunching EVOX.Console did, by re-constructing everything from scratch. This stops
	/// whatever workflows were running, throws away the stale task graph and managers, builds a
	/// fresh one, then restarts the same workflows so the operator doesn't have to re-enable them.
	/// </summary>
	public async Task ReloadTasksAsync()
	{
		List<string> activeWorkflows = GetRunningWorkflows().ToList();
		await StopAllAsync();
		lock (_lock)
		{
			_managers.Clear();
			_taskCenter = new TaskCenter(_alarm, _logger, _inputUtils, _onMatchFoundAction, _onMatchNotFoundAction);
		}
		foreach (string workflowId in activeWorkflows)
		{
			await StartAsync(workflowId);
		}
	}

	private void RegisterWorkflow(IWorkflow workflow)
	{
		lock (_lock)
		{
			if (!_managers.ContainsKey(workflow.WorkflowId))
			{
				_managers[workflow.WorkflowId] = new WorkflowManager(workflow, _taskCenter, this);
			}
		}
	}

	public async Task StartAsync(string workflowId)
	{
		WorkflowManager manager;
		lock (_lock)
		{
			if (!_managers.TryGetValue(workflowId, out manager))
			{
				if (!_workflowTypes.TryGetValue(workflowId, out Type workflowType))
				{
					return;
				}
				IWorkflow workflow = (IWorkflow)Activator.CreateInstance(workflowType);
				if (!workflow.IsActive)
				{
					Logger.Instance.LogWarning("Workflow " + workflowId + " aktif değil ve başlatılmayacak.");
					return;
				}
				RegisterWorkflow(workflow);
				_managers.TryGetValue(workflowId, out manager);
			}
			else if (!manager.Workflow.IsActive)
			{
				Logger.Instance.LogWarning("Workflow " + workflowId + " aktif değil ve başlatılmayacak.");
				return;
			}
			if (_running.ContainsKey(workflowId))
			{
				return;
			}
			_running[workflowId] = null;
		}
		try
		{
			if (manager != null)
			{
				Task task = manager.RunAsync();
				lock (_lock)
				{
					_running[workflowId] = task;
				}
				await task;
			}
		}
		catch (OperationCanceledException)
		{
			manager.Stop();
		}
		finally
		{
			lock (_lock)
			{
				_running.Remove(workflowId);
			}
		}
	}

	public async Task StopAsync(string workflowId)
	{
		Task task;
		lock (_lock)
		{
			if (!_running.TryGetValue(workflowId, out task))
			{
				return;
			}
			_managers[workflowId].Stop();
		}
		try
		{
			if (task != null)
			{
				await task;
			}
		}
		finally
		{
			lock (_lock)
			{
				_running.Remove(workflowId);
			}
		}
	}

	public async Task StopAllAsync()
	{
		List<Task> tasks;
		lock (_lock)
		{
			tasks = _running.Values.Where((Task t) => t != null).ToList();
			foreach (WorkflowManager manager in _managers.Values)
			{
				manager.Stop();
			}
		}
		try
		{
			await Task.WhenAll(tasks);
		}
		finally
		{
			lock (_lock)
			{
				_running.Clear();
			}
		}
	}

	public IEnumerable<string> GetRunningWorkflows()
	{
		lock (_lock)
		{
			return _running.Keys.ToList();
		}
	}
}
