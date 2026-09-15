using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class CureDBWorkFlow : IWorkflow
{
	public string WorkflowId => "CureDB";

	public bool IsActive => Settings.Instance.ScreenCapture.CureDB.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public CureDBWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["CureDB"] = new WorkflowTransition("CureDB")
		{
			NextStepOnMatch = "CureDB",
			NextStepOnNotMatch = "CureDB"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
