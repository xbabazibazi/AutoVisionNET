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

	private readonly string _startTaskId;

	private readonly string _workflowId;

	private readonly Dictionary<string, ImageSearchService> _searchServices;

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
		_cts?.Dispose();
		_cts = new CancellationTokenSource();
		CancellationToken token = _cts.Token;
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
					if (_workflowId == "GenieStatussdfsdfsdf")
					{
						Logger.Instance.LogInformation("Görev bulunamadı: " + current + ". İş akışı sonlandırılıyor.");
					}
					break;
				}
				if (!_transitions.TryGetValue(current, out WorkflowTransition transition))
				{
					if (_workflowId == "GenieStatussdfsdfsdf")
					{
						Logger.Instance.LogInformation("Geçiş tanımı bulunamadı: " + current + ". İş akışı sonlandırılıyor.");
					}
					break;
				}
				if (_workflowId == "GenieStatussdfsdfsdf")
				{
					Logger.Instance.LogInformation($"Mevcut görev: {current} | Mod: {task.Mode} | Eşleşme Modu: {task.Config.Mode}");
				}
				ImageSearchService service = _searchServices[current];
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
			if (_workflowId == "GenieStatussdfsdfsdf")
			{
				Logger.Instance.LogInformation("HATA: " + ex2.Message);
			}
		}
	}

	public void Stop()
	{
		_cts?.Cancel();
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
