using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class RequestPartyWorkflow : IWorkflow
{
	public string WorkflowId => "RequestParty";

	public bool IsActive => Settings.Instance.ScreenCapture.AcceptParty.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public RequestPartyWorkflow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["katadora"] = new WorkflowTransition("katadora")
		{
			NextStepOnMatch = "RequestParty",
			NextStepOnNotMatch = "katadora"
		};
		Steps["RequestParty"] = new WorkflowTransition("RequestParty")
		{
			NextStepOnMatch = "RequestParty",
			NextStepOnNotMatch = "katadora"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
