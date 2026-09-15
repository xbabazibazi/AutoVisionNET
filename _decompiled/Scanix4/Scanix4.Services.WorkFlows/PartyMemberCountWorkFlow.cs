using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class PartyMemberCountWorkFlow : IWorkflow
{
	public string WorkflowId => "PartyMemberCount";

	public bool IsActive => Settings.Instance.ScreenCapture.PartyMemberCount.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public PartyMemberCountWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["PartyMemberCount"] = new WorkflowTransition("PartyMemberCount")
		{
			NextStepOnMatch = "PartyMemberCount",
			NextStepOnNotMatch = "PartyMemberCount"
		};
		Steps["PartyHeader"] = new WorkflowTransition("PartyHeader")
		{
			NextStepOnMatch = "BreakParty",
			NextStepOnNotMatch = "PartyMemberCount"
		};
		Steps["BreakParty"] = new WorkflowTransition("BreakParty")
		{
			NextStepOnMatch = "PartyMemberCount",
			NextStepOnNotMatch = "PartyMemberCount"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
