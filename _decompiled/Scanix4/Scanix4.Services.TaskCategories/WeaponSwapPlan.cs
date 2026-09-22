namespace Scanix4.Services.TaskCategories;

/// <summary>
/// Single source of truth for the weapon-swap step names and template slots.
///
/// The workflow engine resolves a step's SearchTask by the step KEY, so a step name that does not
/// exactly match a task id silently kills the whole workflow. The old hand-written swap workflow
/// had exactly that failure twice ("TOWN" vs "Town", and an "OpenSecondMagicBag2" step with no
/// task). Both the task list and the workflow steps are generated from the helpers below, so the
/// two cannot drift apart.
///
/// The magic bags are deliberately not part of this any more: the replacement weapon is taken
/// straight out of the inventory.
/// </summary>
public static class WeaponSwapPlan
{
	public const int WeaponCount = 3;

	// ---- step / task ids -------------------------------------------------------------------

	/// <summary>Broken weapon W still equipped - the trigger. Look only, nothing is clicked.</summary>
	public static string BrokenEquipped(int weapon) => $"WeaponBrokenEquipped{weapon}";

	/// <summary>Repaired weapon W lying in the inventory - right-clicking it equips it.</summary>
	public static string EquipFromInventory(int weapon) => $"WeaponEquip{weapon}";

	// ---- left hand ---------------------------------------------------------------------------
	// Right-clicking an item in the inventory only ever fills the RIGHT hand, so the left hand is
	// served by dragging: pick the repaired item up in the inventory and drop it straight onto the
	// broken one sitting in the slot - the two swap places. The broken item is never taken off
	// first, so the hand is never briefly empty.

	/// <summary>Broken left-hand item L still equipped - the trigger. Look only.</summary>
	public static string LeftBrokenEquipped(int item) => $"LeftBrokenEquipped{item}";

	/// <summary>Pick up repaired left-hand item R in the inventory, heading for the slot of L.</summary>
	public static string LeftGrabDirect(int repaired, int broken) => $"LeftGrab{repaired}For{broken}";

	/// <summary>Release it onto the broken item L sitting in the slot - the two swap.</summary>
	public static string LeftDropOnBroken(int broken) => $"LeftDropOnBroken{broken}";

	// ---- template slots --------------------------------------------------------------------

	public static string BrokenTemplateKey(int weapon) => $"Weapon{weapon}Broken";

	public static string RepairedTemplateKey(int weapon) => $"Weapon{weapon}Repaired";

	// Every weapon uses the same naming, so each one shows up in the Template Manager as its own
	// clearly labelled pair of slots ("Silah N - KIRIK" / "Silah N - SAGLAM") and can be assigned
	// independently. Weapon 1 ships with the existing tomahawk pictures already copied into those
	// files, so it works out of the box; weapons 2-3 point at files that do not exist yet and an
	// unassigned template simply never matches, leaving those branches inert until captured.
	public static string BrokenTemplateDefault(int weapon) => $"Images/Weapon{weapon}Broken.jpg";

	public static string RepairedTemplateDefault(int weapon) => $"Images/Weapon{weapon}Repaired.jpg";

	public static string LeftBrokenTemplateKey(int item) => $"LeftHand{item}Broken";

	public static string LeftRepairedTemplateKey(int item) => $"LeftHand{item}Repaired";

	public static string LeftBrokenTemplateDefault(int item) => $"Images/LeftHand{item}Broken.jpg";

	public static string LeftRepairedTemplateDefault(int item) => $"Images/LeftHand{item}Repaired.jpg";
}
