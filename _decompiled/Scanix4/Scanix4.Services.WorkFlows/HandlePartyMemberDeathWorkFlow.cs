using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class HandlePartyMemberDeathWorkFlow : IWorkflow
{
	public string WorkflowId => "HandlePartyMemberDeath";

	public bool IsActive => Settings.Instance.ScreenCapture.HandlePartyMemberDeath.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public HandlePartyMemberDeathWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["HandlePartyMemberDeath"] = new WorkflowTransition("HandlePartyMemberDeath")
		{
			NextStepOnMatch = "HandlePartyMemberDeath",
			NextStepOnNotMatch = "HandlePartyMemberDeath"
		};
		Steps["PartyHeader"] = new WorkflowTransition("PartyHeader")
		{
			NextStepOnMatch = "BreakParty",
			NextStepOnNotMatch = "HandlePartyMemberDeath"
		};
		Steps["BreakParty"] = new WorkflowTransition("BreakParty")
		{
			NextStepOnMatch = "HandlePartyMemberDeath",
			NextStepOnNotMatch = "HandlePartyMemberDeath"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
