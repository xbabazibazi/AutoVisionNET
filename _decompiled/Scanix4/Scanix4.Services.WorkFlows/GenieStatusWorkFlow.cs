using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class GenieStatusWorkFlow : IWorkflow
{
	public string WorkflowId => "GenieStatus";

	// This workflow only WATCHES whether Genie is on; the things that act on that - stopping the
	// macros, auto-starting the attack, reporting GENIE:ON/OFF to the service - are each gated by
	// their own setting where they happen. Gating the observer itself on "Genie Durunca Makroları
	// Durdur" meant that with that one feature off, the panel reported every character's Genie as
	// closed and the attack macro never auto-started, neither of which that setting is about.
	public bool IsActive => true;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public GenieStatusWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["GenieStatus"] = new WorkflowTransition("GenieStatus")
		{
			NextStepOnMatch = "GenieStatus",
			NextStepOnNotMatch = "GenieStatus"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
