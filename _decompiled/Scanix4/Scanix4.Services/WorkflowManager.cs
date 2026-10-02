using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Scanix4.Core;
using Scanix4.Interfaces;
using Scanix4.Models;
using SimpleLogger;

namespace Scanix4.Services;

public class WorkflowManager
{
	private const string LOGGED_WORKFLOW_ID = "GenieStatussdfsdfsdf";

	private readonly TaskCenter _taskCenter;

	private readonly ActionCenter _actionCenter;

	private readonly Dictionary<string, WorkflowTransition> _transitions;

	private CancellationTokenSource _cts;

	private readonly object _ctsLock = new object();

	private bool _stopRequested;

	private readonly string _startTaskId;

	private readonly string _workflowId;

	private readonly Dictionary<string, ImageSearchService> _searchServices;

	// Steps already reported as unconfigured, so the warning appears once instead of on every
	// pass of a loop that runs several times a second.
	private readonly HashSet<string> _warnedUnusableSteps = new HashSet<string>();

	public IWorkflow Workflow { get; }

	public string WorkflowId => _workflowId;

	public WorkflowManager(IWorkflow workflow, TaskCenter taskCenter, WorkflowEngine workflowEngine)
	{
		_taskCenter = taskCenter;
		_actionCenter = taskCenter.ActionCenter;
		_transitions = workflow.GetWorkflowSteps() ?? throw new ArgumentNullException("workflow");
		_startTaskId = _transitions.Keys.First();
		_workflowId = workflow.WorkflowId;
		Workflow = workflow ?? throw new ArgumentNullException("workflow");
		_searchServices = new Dictionary<string, ImageSearchService>();
		foreach (string key in _transitions.Keys)
		{
			if (_taskCenter._allTasks.TryGetValue(key, out SearchTask value))
			{
				_searchServices[key] = new ImageSearchService(value.Config);
			}
		}
	}

	public Task RunAsync()
	{
		return RunAsync(_startTaskId);
	}

	public async Task RunAsync(string startTaskId)
	{
		CancellationToken token;
		lock (_ctsLock)
		{
			_cts?.Dispose();
			_cts = new CancellationTokenSource();
			token = _cts.Token;
			if (_stopRequested)
			{
				_cts.Cancel();
			}
			_stopRequested = false;
		}
		Dictionary<string, SearchTask> tasks = _taskCenter._allTasks;
		string current = startTaskId;
		try
		{
			if (_workflowId == "GenieStatussdfsdfsdf")
			{
				Logger.Instance.LogInformation("İş akışı başlatıldı. Başlangıç görevi: " + startTaskId);
			}
			Point match = default(Point);
			while (current != null && !token.IsCancellationRequested)
			{
				token.ThrowIfCancellationRequested();
				if (!tasks.TryGetValue(current, out SearchTask task))
				{
					// A step whose id has no matching task ends the workflow for good. Logging this
					// used to be gated behind a debug id that is never true, so a simple typo in a
					// workflow definition silently disabled the whole feature with no trace.
					Logger.Instance.LogError($"'{_workflowId}' iş akışı durdu: '{current}' adımı için tanımlı görev yok.");
					break;
				}
				if (!_transitions.TryGetValue(current, out WorkflowTransition transition))
				{
					Logger.Instance.LogError($"'{_workflowId}' iş akışı durdu: '{current}' adımı tanımlı değil.");
					break;
				}
				if (_workflowId == "GenieStatussdfsdfsdf")
				{
					Logger.Instance.LogInformation($"Mevcut görev: {current} | Mod: {task.Mode} | Eşleşme Modu: {task.Config.Mode}");
				}
				ImageSearchService service = _searchServices[current];
				// A step with no region drawn or template assigned can never match. Say so once -
				// otherwise the whole workflow just quietly does nothing and looks like the
				// command was ignored - but carry on down the not-found branch rather than
				// stopping: most workflows chain through their steps on both outcomes, so one
				// unconfigured step in the middle must not kill the ones after it.
				if (!service.IsUsable && task.Mode != SearchMode.SnapNet && _warnedUnusableSteps.Add(current))
				{
					Logger.Instance.LogWarning($"'{_workflowId}' iş akışının '{current}' adımı atlanıyor: tarama bölgesi çizilmemiş veya şablonu atanmamış (şablon: {service.TemplatePath}, bölge: {service.SearchArea}).");
				}
				int delayMs;
				if (task.Mode == SearchMode.SnapNet)
				{
					current = _actionCenter._partyActions.GetNextTaskId() ?? _actionCenter._genieActions.GetNextTaskId() ?? _actionCenter._buffLineActions.GetNextTaskId() ?? transition.NextStepOnMatch;
					if (_workflowId == "GenieStatussdfsdfsdf")
					{
						Logger.Instance.LogInformation("SnapNet sonrası yeni görev: " + current);
					}
					delayMs = 100;
				}
				else
				{
					object result = service.Search();
					token.ThrowIfCancellationRequested();
					if (task.Config.Mode == MatchMode.SingleMatch)
					{
						int num;
						if (result is Point)
						{
							match = (Point)result;
							num = 1;
						}
						else
						{
							num = 0;
						}
						if (num != 0)
						{
							task.Config.OnMatchFound?.Invoke(match);
							current = _actionCenter._partyActions.GetNextTaskId() ?? _actionCenter._genieActions.GetNextTaskId() ?? _actionCenter._buffLineActions.GetNextTaskId() ?? transition.NextStepOnMatch;
							delayMs = 300;
							if (_workflowId == "GenieStatussdfsdfsdf")
							{
								Logger.Instance.LogInformation($"Eşleşme bulundu: Confidence = X={match.X}, Y={match.Y} | Sonraki görev: {current}");
							}
						}
						else
						{
							task.Config.OnMatchNotFound?.Invoke();
							current = transition.NextStepOnNotMatch;
							delayMs = task.Config.IntervalMs;
							if (_workflowId == "GenieStatussdfsdfsdf")
							{
								Logger.Instance.LogInformation($"Eşleşme bulunamadı. Sonraki adım: {current} | Bekleme süresi: {task.Config.IntervalMs}ms");
							}
						}
					}
					else if (task.Config.Mode == MatchMode.CountMatches)
					{
						int matchCount = (int)result;
						if (matchCount > 0)
						{
							task.Config.OnFoundCount?.Invoke(matchCount);
							current = _actionCenter._partyActions.GetNextTaskId() ?? transition.NextStepOnMatch;
							delayMs = task.Config.IntervalMs;
							if (_workflowId == "GenieStatussdfsdfsdf")
							{
								Logger.Instance.LogInformation($"Eşleşme sayısı: {matchCount} | Sonraki görev: {current}");
							}
						}
						else
						{
							// A zero count normally means "nothing found", but for some tasks zero is
							// the state we actually care about (inventory completely full), so those
							// opt in to having it reported.
							if (task.Config.ReportZeroCount)
							{
								task.Config.OnFoundCount?.Invoke(0);
							}
							task.Config.OnMatchNotFound?.Invoke();
							current = transition.NextStepOnNotMatch;
							delayMs = task.Config.IntervalMs;
							if (_workflowId == "GenieStatussdfsdfsdf")
							{
								Logger.Instance.LogInformation($"Eşleşme bulunamadı. Sonraki adım: {current} | Bekleme süresi: {task.Config.IntervalMs}ms");
							}
						}
					}
					else
					{
						delayMs = 300;
						if (_workflowId == "GenieStatussdfsdfsdf")
						{
							Logger.Instance.LogInformation($"Bilinmeyen eşleşme modu: {task.Config.Mode}");
						}
					}
				}
				await Task.Delay(delayMs, token).ConfigureAwait(continueOnCapturedContext: false);
				task = null;
				transition = null;
			}
			if (_workflowId == "GenieStatussdfsdfsdf")
			{
				Logger.Instance.LogInformation((current == null) ? "Tüm görevler tamamlandı. İş akışı başarıyla sonlandırıldı." : ("İş akışı erken sonlandırıldı. Son görev: " + current));
			}
		}
		catch (OperationCanceledException)
		{
			if (_workflowId == "GenieStatussdfsdfsdf")
			{
				Logger.Instance.LogInformation("İş akışı iptal edildi.");
			}
		}
		catch (Exception ex2)
		{
			// This catch sits OUTSIDE the task loop, so anything thrown by a task's action
			// handler kills the whole workflow for good. Logging it used to be gated behind a
			// leftover debug workflow id that is never true, which made such a death completely
			// invisible - the service simply stopped working with no trace at all.
			Logger.Instance.LogError($"'{_workflowId}' iş akışı hata nedeniyle durdu: {ex2.GetType().Name} - {ex2.Message}");
		}
	}

	public void Stop()
	{
		lock (_ctsLock)
		{
			_stopRequested = true;
			_cts?.Cancel();
		}
	}

	public void Dispose()
	{
		foreach (ImageSearchService value in _searchServices.Values)
		{
			value.Dispose();
		}
		_cts?.Dispose();
	}
}
