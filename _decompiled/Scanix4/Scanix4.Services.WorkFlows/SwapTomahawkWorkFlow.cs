using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

public class SwapTomahawkWorkFlow : IWorkflow
{
	public string WorkflowId => "SwapTomahawk";

	public bool IsActive => Settings.Instance.ScreenCapture.SwapTomahawk.IsActive;

	public Dictionary<string, WorkflowTransition> Steps { get; } = new Dictionary<string, WorkflowTransition>();

	public SwapTomahawkWorkFlow()
	{
		DefineSteps();
	}

	private void DefineSteps()
	{
		Steps["SwapTomahawk"] = new WorkflowTransition("SwapTomahawk")
		{
			NextStepOnMatch = "OpenMagicBag",
			NextStepOnNotMatch = "SwapTomahawk"
		};
		Steps["CheckRepairedTomahawkOnInventory"] = new WorkflowTransition("CheckRepairedTomahawkOnInventory")
		{
			NextStepOnMatch = "EquipTomahawk",
			NextStepOnNotMatch = "OpenMagicBag"
		};
		Steps["OpenMagicBag"] = new WorkflowTransition("OpenMagicBag")
		{
			NextStepOnMatch = "CheckRepairedTomahawkOnFirstMagicBag",
			NextStepOnNotMatch = "CheckRepairedTomahawkOnFirstMagicBag"
		};
		Steps["CheckRepairedTomahawkOnFirstMagicBag"] = new WorkflowTransition("CheckRepairedTomahawkOnFirstMagicBag")
		{
			NextStepOnMatch = "FindBrokenTomahawkOnInventory",
			NextStepOnNotMatch = "OpenSecondMagicBag"
		};
		Steps["FindBrokenTomahawkOnInventory"] = new WorkflowTransition("FindBrokenTomahawkOnInventory")
		{
			NextStepOnMatch = "FindRepairedTomahawkOnMagicBag",
			NextStepOnNotMatch = "SwapTomahawk"
		};
		Steps["FindRepairedTomahawkOnMagicBag"] = new WorkflowTransition("FindRepairedTomahawkOnMagicBag")
		{
			NextStepOnMatch = "EquipTomahawk",
			NextStepOnNotMatch = "CheckRightHandIsEmpty"
		};
		Steps["EquipTomahawk"] = new WorkflowTransition("EquipTomahawk")
		{
			NextStepOnMatch = "CloseMagicBag",
			NextStepOnNotMatch = "CheckRightHandIsEmpty"
		};
		Steps["CloseMagicBag"] = new WorkflowTransition("CloseMagicBag")
		{
			NextStepOnMatch = "CheckRightHandIsEmpty",
			NextStepOnNotMatch = "CheckRightHandIsEmpty"
		};
		Steps["OpenSecondMagicBag"] = new WorkflowTransition("OpenSecondMagicBag")
		{
			NextStepOnMatch = "CheckRepairedTomahawkOnSecondMagicBag",
			NextStepOnNotMatch = "CloseMagicBag"
		};
		Steps["CheckRepairedTomahawkOnSecondMagicBag"] = new WorkflowTransition("CheckRepairedTomahawkOnSecondMagicBag")
		{
			NextStepOnMatch = "FindBrokenTomahawkOnInventory",
			NextStepOnNotMatch = "TOWN"
		};
		Steps["Town"] = new WorkflowTransition("Town")
		{
			NextStepOnMatch = null,
			NextStepOnNotMatch = null
		};
		Steps["CheckRightHandIsEmpty"] = new WorkflowTransition("CheckRightHandIsEmpty")
		{
			NextStepOnMatch = "OpenMagicBag2",
			NextStepOnNotMatch = "CheckLeftHandIsEmpty"
		};
		Steps["CheckLeftHandIsEmpty"] = new WorkflowTransition("CheckLeftHandIsEmpty")
		{
			NextStepOnMatch = "OpenMagicBag2",
			NextStepOnNotMatch = "SwapTomahawk"
		};
		Steps["OpenMagicBag2"] = new WorkflowTransition("OpenMagicBag")
		{
			NextStepOnMatch = "CheckRepairedTomahawkOnFirstMagicBag2",
			NextStepOnNotMatch = "CheckRepairedTomahawkOnFirstMagicBag2"
		};
		Steps["CheckRepairedTomahawkOnFirstMagicBag2"] = new WorkflowTransition("CheckRepairedTomahawkOnFirstMagicBag2")
		{
			NextStepOnMatch = "EquipTomahawk",
			NextStepOnNotMatch = "OpenSecondMagicBag2"
		};
		Steps["OpenSecondMagicBag2"] = new WorkflowTransition("OpenSecondMagicBag")
		{
			NextStepOnMatch = "CheckRepairedTomahawkOnSecondMagicBag2",
			NextStepOnNotMatch = "CloseMagicBag"
		};
		Steps["CheckRepairedTomahawkOnSecondMagicBag2"] = new WorkflowTransition("CheckRepairedTomahawkOnSecondMagicBag2")
		{
			NextStepOnMatch = "EquipTomahawk",
			NextStepOnNotMatch = "SwapTomahawk"
		};
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
