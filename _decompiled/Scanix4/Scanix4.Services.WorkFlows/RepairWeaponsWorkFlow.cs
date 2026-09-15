using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class RepairWeaponsWorkFlow : IWorkflow
{
	public string WorkflowId => "RepairWeapons";

	public bool IsActive => Settings.Instance.ScreenCapture.RepairWeapons.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public RepairWeaponsWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["RepairWeapons"] = new WorkflowTransition("RepairWeapons")
		{
			NextStepOnMatch = "RepairWeapons",
			NextStepOnNotMatch = "RepairWeapons"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
