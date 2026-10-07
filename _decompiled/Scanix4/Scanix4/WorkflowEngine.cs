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
		// Everything currently running, plus every workflow its own settings say should be - a
		// workflow whose region had not been drawn yet ends on its first pass, so restarting only
		// what happens to be running would never bring those back. The ones named SnapNet* are
		// left out on purpose: they are fired by remote commands, not run continuously.
		HashSet<string> toRestart = new HashSet<string>(GetRunningWorkflows());
		foreach (string workflowId in _workflowTypes.Keys)
		{
			if (!workflowId.Contains("SnapNet"))
			{
				toRestart.Add(workflowId);
			}
		}
		await StopAllAsync();
		lock (_lock)
		{
			foreach (WorkflowManager manager in _managers.Values)
			{
				// Each manager owns an ImageSearchService per step, holding native OpenCV and GDI
				// capture resources; dropping them without disposing leaks on every reload.
				manager.Dispose();
			}
			_managers.Clear();
			_taskCenter = new TaskCenter(_alarm, _logger, _inputUtils, _onMatchFoundAction, _onMatchNotFoundAction);
		}
		foreach (string workflowId in toRestart)
		{
			// StartAsync already skips anything its IsActive says is switched off.
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

	// Deliberately not async: manager.RunAsync() returns a Task that only completes once the
	// workflow is stopped (its loop runs until cancelled), so a caller awaiting this method used
	// to hang until the workflow stopped - which meant a caller starting several workflows in a
	// row (ReloadTasksAsync, StartServiceFirstTime) only ever got past the first one, silently
	// never starting the rest. This now only awaits the work of REGISTERING and LAUNCHING the
	// workflow, and lets it keep running in the background; cleanup of _running happens via the
	// continuation once the workflow eventually does stop.
	public Task StartAsync(string workflowId)
	{
		lock (_lock)
		{
			if (!_managers.TryGetValue(workflowId, out WorkflowManager manager))
			{
				if (!_workflowTypes.TryGetValue(workflowId, out Type workflowType))
				{
					return Task.CompletedTask;
				}
				IWorkflow workflow = (IWorkflow)Activator.CreateInstance(workflowType);
				if (!workflow.IsActive)
				{
					Logger.Instance.LogWarning("Workflow " + workflowId + " aktif değil ve başlatılmayacak.");
					return Task.CompletedTask;
				}
				RegisterWorkflow(workflow);
				_managers.TryGetValue(workflowId, out manager);
			}
			else if (!manager.Workflow.IsActive)
			{
				Logger.Instance.LogWarning("Workflow " + workflowId + " aktif değil ve başlatılmayacak.");
				return Task.CompletedTask;
			}
			if (_running.ContainsKey(workflowId) || manager == null)
			{
				return Task.CompletedTask;
			}
			// Task.Run, not a bare RunAsync(): the loop inside runs a blocking image search before
			// it reaches its first await, so calling it directly would stall whoever started the
			// workflow - the keyboard hook thread for a hotkey, or the TCP receive loop for a
			// remote command from the service, which would hold up every command behind it.
			WorkflowManager started = manager;
			Task task = Task.Run(() => started.RunAsync());
			_running[workflowId] = task;
			task.ContinueWith(delegate
			{
				lock (_lock)
				{
					// Only clear the slot if it still holds THIS task. A reload stops the old run
					// and starts a new one for the same id, and this continuation can land after
					// that - removing the entry the new run just put there. The engine then
					// believed nothing was running while a workflow actually was, so the next
					// reload neither awaited nor replaced it and a second instance could end up
					// scanning and acting alongside the first.
					if (_running.TryGetValue(workflowId, out Task tracked) && tracked == task)
					{
						_running.Remove(workflowId);
					}
				}
			}, TaskScheduler.Default);
		}
		return Task.CompletedTask;
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
