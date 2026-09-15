using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class GenieStatusWorkFlow : IWorkflow
{
	public string WorkflowId => "GenieStatus";

	public bool IsActive => Settings.Instance.ScreenCapture.StopMacrosOnGenieStop.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public GenieStatusWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["GenieStatus"] = new WorkflowTransition("GenieStatus")
		{
			NextStepOnMatch = "GenieStatus",
			NextStepOnNotMatch = "GenieStatus"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
