using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class StartGenieAfterTpWorkFlow : IWorkflow
{
	public string WorkflowId => "StartGenieAfterTp";

	public bool IsActive => Settings.Instance.Macro.General.StartGenieOnTp;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public StartGenieAfterTpWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["StartGenieAfterTp"] = new WorkflowTransition("StartGenieAfterTp")
		{
			NextStepOnMatch = "StartGenie",
			NextStepOnNotMatch = "StartGenieAfterTp"
		};
		Steps["StartGenie"] = new WorkflowTransition("StartGenie")
		{
			NextStepOnMatch = "DeleteResistance",
			NextStepOnNotMatch = "StartGenie"
		};
		Steps["DeleteResistance"] = new WorkflowTransition("DeleteResistance")
		{
			NextStepOnMatch = "StartGenieAfterTp",
			NextStepOnNotMatch = "StartGenieAfterTp"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
