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
		// After a successful click, drop back to "katadora" instead of immediately
		// re-scanning "RequestParty" - the accept dialog can take longer than the
		// workflow's 300ms post-match delay to actually close (client animation /
		// server round-trip), and looping back onto itself was re-clicking the same
		// still-visible dialog over and over instead of registering the invite as handled.
		Steps["RequestParty"] = new WorkflowTransition("RequestParty")
		{
			NextStepOnMatch = "katadora",
			NextStepOnNotMatch = "katadora"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
