using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class SnapNetWhellOfFunWorkFlow : IWorkflow
{
	public string WorkflowId => "SnapNetWhellOfFun";

	public bool IsActive => Settings.Instance.ScreenCapture.WhellOfFun.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public SnapNetWhellOfFunWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["SnapNetWhellOfFun"] = new WorkflowTransition("SnapNetWhellOfFun")
		{
			NextStepOnMatch = "Event",
			NextStepOnNotMatch = "Event"
		};
		Steps["Event"] = new WorkflowTransition("Event")
		{
			NextStepOnMatch = "RequestParty",
			NextStepOnNotMatch = "RequestParty"
		};
		Steps["RequestParty"] = new WorkflowTransition("RequestParty")
		{
			NextStepOnMatch = "WhellOfFunButton",
			NextStepOnNotMatch = "WhellOfFunButton"
		};
		Steps["WhellOfFunButton"] = new WorkflowTransition("WhellOfFunButton")
		{
			NextStepOnMatch = "WhellOfFunPushButton",
			NextStepOnNotMatch = "WhellOfFunPushButton"
		};
		Steps["WhellOfFunPushButton"] = new WorkflowTransition("WhellOfFunPushButton")
		{
			NextStepOnMatch = "WhellOfFunYesButton",
			NextStepOnNotMatch = "WhellOfFunYesButton"
		};
		Steps["WhellOfFunYesButton"] = new WorkflowTransition("WhellOfFunYesButton")
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
