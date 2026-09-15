using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class SnapNetReReReWorkFlow : IWorkflow
{
	public string WorkflowId => "SnapNetReReRe";

	public bool IsActive => Settings.Instance.ScreenCapture.ReReRe.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public SnapNetReReReWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["SnapNetReReRe"] = new WorkflowTransition("SnapNetReReRe")
		{
			NextStepOnMatch = "Undy",
			NextStepOnNotMatch = "Undy"
		};
		Steps["Undy"] = new WorkflowTransition("Undy")
		{
			NextStepOnMatch = "300Ac",
			NextStepOnNotMatch = "300Ac"
		};
		Steps["300Ac"] = new WorkflowTransition("300Ac")
		{
			NextStepOnMatch = "Sw",
			NextStepOnNotMatch = "Sw"
		};
		Steps["Sw"] = new WorkflowTransition("Sw")
		{
			NextStepOnMatch = "Wolf",
			NextStepOnNotMatch = "Wolf"
		};
		Steps["Wolf"] = new WorkflowTransition("Wolf")
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
