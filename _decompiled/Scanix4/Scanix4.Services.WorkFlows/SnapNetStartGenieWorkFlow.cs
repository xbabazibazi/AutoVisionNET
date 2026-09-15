using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class SnapNetStartGenieWorkFlow : IWorkflow
{
	public string WorkflowId => "SnapNetStartGenie";

	public bool IsActive => Settings.Instance.Macro.General.StartGenieOnTp;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public SnapNetStartGenieWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["SnapNetStartGenie"] = new WorkflowTransition("SnapNetStartGenie")
		{
			NextStepOnMatch = "StartGenie",
			NextStepOnNotMatch = "StartGenie"
		};
		Steps["StartGenie"] = new WorkflowTransition("StartGenie")
		{
			NextStepOnMatch = null,
			NextStepOnNotMatch = null
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
