using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class RepairArmorsWorkFlow : IWorkflow
{
	public string WorkflowId => "RepairArmors";

	public bool IsActive => Settings.Instance.ScreenCapture.RepairArmors.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public RepairArmorsWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["RepairArmors"] = new WorkflowTransition("RepairArmors")
		{
			NextStepOnMatch = "RepairArmors",
			NextStepOnNotMatch = "RepairArmors"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
