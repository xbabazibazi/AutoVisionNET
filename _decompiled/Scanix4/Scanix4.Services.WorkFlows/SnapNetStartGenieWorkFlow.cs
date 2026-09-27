using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class SnapNetStartGenieWorkFlow : IWorkflow
{
	public string WorkflowId => "SnapNetStartGenie";

	// NOT Macro.General.StartGenieOnTp - that's the unrelated "start genie after
	// teleport" macro's own gate (see StartGenieAfterTpWorkFlow). This workflow is
	// what the server's remote "Warrior/Priest Genie Aç" command actually starts,
	// so it must follow the same setting ClientForm's "401" handler already checks
	// and the SnapNetStartGenie settings panel actually exposes - otherwise the
	// command is received and silently dropped with no feedback on the operator's
	// side at all.
	public bool IsActive => Settings.Instance.ScreenCapture.StartGenie.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public SnapNetStartGenieWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["SnapNetStartGenie"] = new WorkflowTransition("SnapNetStartGenie")
		{
			NextStepOnMatch = "StartGenie",
			NextStepOnNotMatch = "StartGenie"
		};
		Steps["StartGenie"] = new WorkflowTransition("StartGenie")
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
