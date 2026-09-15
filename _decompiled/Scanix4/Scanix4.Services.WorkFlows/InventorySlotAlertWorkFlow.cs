using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class InventorySlotAlertWorkFlow : IWorkflow
{
	public string WorkflowId => "InventorySlotAlert";

	public bool IsActive => Settings.Instance.ScreenCapture.InventorySlotAlert.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public InventorySlotAlertWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["InventorySlotAlert"] = new WorkflowTransition("InventorySlotAlert")
		{
			NextStepOnMatch = "InventorySlotAlert",
			NextStepOnNotMatch = "InventorySlotAlert"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
