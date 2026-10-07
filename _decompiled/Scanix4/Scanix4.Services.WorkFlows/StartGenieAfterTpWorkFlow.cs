using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class StartGenieAfterTpWorkFlow : IWorkflow
{
	public string WorkflowId => "StartGenieAfterTp";

	public bool IsActive => Settings.Instance.Macro.General.StartGenieOnTp;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public StartGenieAfterTpWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["StartGenieAfterTp"] = new WorkflowTransition("StartGenieAfterTp")
		{
			NextStepOnMatch = "StartGenie",
			NextStepOnNotMatch = "StartGenieAfterTp"
		};
		Steps["StartGenie"] = new WorkflowTransition("StartGenie")
		{
			NextStepOnMatch = "DeleteResistance",
			NextStepOnNotMatch = "StartGenie"
		};
		// Resist deleted (or already gone) - now confirm Genie actually came on before doing
		// anything else. Without this the chain went straight back to watching for the resist
		// icon, so a buff that had not faded yet re-triggered the whole thing and clicked the
		// Genie button a second time, switching it off again.
		Steps["DeleteResistance"] = new WorkflowTransition("DeleteResistance")
		{
			NextStepOnMatch = "GenieActiveCheck",
			NextStepOnNotMatch = "GenieActiveCheck"
		};
		Steps["GenieActiveCheck"] = new WorkflowTransition("GenieActiveCheck")
		{
			// Genie is confirmed on: this run is done. Back to the first step to wait for the
			// next cast - NOT null, because a workflow that returns null is removed from the
			// engine's running set and never restarted, which would make the whole feature work
			// exactly once per launch.
			NextStepOnMatch = "StartGenieAfterTp",
			// Not on yet: the click did not take, so try the start once more. Bounded by
			// OnStartGenie itself - it skips while Genie reads as active, and rate-limits itself
			// when the status cannot be read at all.
			NextStepOnNotMatch = "StartGenie"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
