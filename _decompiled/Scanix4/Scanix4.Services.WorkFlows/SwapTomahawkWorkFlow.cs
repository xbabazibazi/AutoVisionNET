using System.Collections.Generic;
using Scanix4.Interfaces;
using Scanix4.Models;
using Scanix4.Services.TaskCategories;
using SettingsManager;

namespace Scanix4.Services.WorkFlows;

/// <summary>
/// Weapon swap. While a broken weapon is equipped, the inventory is scanned for a repaired one and
/// a single right-click on it puts it on - the game swaps the broken weapon out by itself.
///
/// The broken weapon is never taken off first: if no replacement happened to be in the inventory,
/// the character would otherwise be left holding nothing.
///
/// The magic bags are no longer part of this flow; only the inventory is searched.
///
/// Steps are generated from <see cref="WeaponSwapPlan"/>, the same helper the task list uses, so a
/// step can never point at a task id that does not exist. The earlier hand-written version had two
/// such dead ends and each one silently disabled the whole feature.
/// </summary>
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
		int weapons = WeaponSwapPlan.WeaponCount;

		// --- trigger: is any of our weapons equipped and broken? -----------------------------
		// Inserted first on purpose: the engine starts a workflow at the first key added.
		for (int w = 1; w <= weapons; w++)
		{
			Steps[WeaponSwapPlan.BrokenEquipped(w)] = new WorkflowTransition(WeaponSwapPlan.BrokenEquipped(w))
			{
				// Broken weapon in hand -> start looking for a replacement in the inventory.
				NextStepOnMatch = WeaponSwapPlan.EquipFromInventory(1),
				// Otherwise check the next weapon, and after the last one sweep again.
				NextStepOnNotMatch = (w < weapons)
					? WeaponSwapPlan.BrokenEquipped(w + 1)
					: WeaponSwapPlan.LeftBrokenEquipped(1)
			};
		}

		// --- the swap: first repaired weapon found in the inventory wins ---------------------
		// The weapons are alternatives, not ranked.
		for (int w = 1; w <= weapons; w++)
		{
			Steps[WeaponSwapPlan.EquipFromInventory(w)] = new WorkflowTransition(WeaponSwapPlan.EquipFromInventory(w))
			{
				// Equipped - back to watching the weapon in hand.
				NextStepOnMatch = WeaponSwapPlan.BrokenEquipped(1),
				// Not this one -> try the next; if none of them is in the inventory there is
				// nothing to swap to, so go back to watching and try again later.
				NextStepOnNotMatch = (w < weapons)
					? WeaponSwapPlan.EquipFromInventory(w + 1)
					: WeaponSwapPlan.LeftBrokenEquipped(1)
			};
		}

		DefineLeftHandSteps(weapons);
	}

	/// <summary>
	/// Left hand. Right-clicking in the inventory only fills the right hand, so the replacement is
	/// dragged into the slot: the repaired item is picked up in the inventory and dropped onto the
	/// broken one sitting in the slot, and the two swap. The broken item is never taken off first,
	/// so the hand is never left empty.
	/// </summary>
	private void DefineLeftHandSteps(int items)
	{
		for (int L = 1; L <= items; L++)
		{
			// --- trigger ---------------------------------------------------------------------
			Steps[WeaponSwapPlan.LeftBrokenEquipped(L)] = new WorkflowTransition(WeaponSwapPlan.LeftBrokenEquipped(L))
			{
				NextStepOnMatch = WeaponSwapPlan.LeftGrabDirect(1, L),
				NextStepOnNotMatch = (L < items)
					? WeaponSwapPlan.LeftBrokenEquipped(L + 1)
					: WeaponSwapPlan.BrokenEquipped(1)
			};

			// --- drag the replacement straight onto the broken item --------------------------
			for (int R = 1; R <= items; R++)
			{
				Steps[WeaponSwapPlan.LeftGrabDirect(R, L)] = new WorkflowTransition(WeaponSwapPlan.LeftGrabDirect(R, L))
				{
					NextStepOnMatch = WeaponSwapPlan.LeftDropOnBroken(L),
					// No replacement of this kind in the inventory - try the next kind, and if none
					// of them is there give up this round (nothing was picked up, so nothing to drop).
					NextStepOnNotMatch = (R < items)
						? WeaponSwapPlan.LeftGrabDirect(R + 1, L)
						: WeaponSwapPlan.BrokenEquipped(1)
				};
			}

			Steps[WeaponSwapPlan.LeftDropOnBroken(L)] = new WorkflowTransition(WeaponSwapPlan.LeftDropOnBroken(L))
			{
				// Dropped - back to watching.
				NextStepOnMatch = WeaponSwapPlan.BrokenEquipped(1),
				// Target vanished; the task released the button anyway.
				NextStepOnNotMatch = WeaponSwapPlan.BrokenEquipped(1)
			};
		}
	}

	public Dictionary<string, WorkflowTransition> GetWorkflowSteps()
	{
		return Steps;
	}
}
