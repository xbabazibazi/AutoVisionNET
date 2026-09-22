using System.Collections.Generic;
using System.Drawing;
using Scanix4.Interfaces;
using Scanix4.Models;
using SettingsManager;

namespace Scanix4.Services.TaskCategories;

/// <summary>
/// The two searches the weapon swap needs, generated from <see cref="WeaponSwapPlan"/> so the task
/// ids always match the workflow step names.
///
/// No magic bag is involved: the replacement is taken from the inventory. Right-clicking a
/// repaired weapon there equips it and the broken one comes off by itself, so the broken weapon is
/// never removed first - that way the character can never end up holding nothing when no
/// replacement turns out to be available.
/// </summary>
public class WeaponSwapTasks : ITaskCategory
{
	private const double MatchThreshold = 0.99;

	private readonly ActionCenter _actionCenter;

	private readonly Rectangle _weaponsArea;

	private readonly Rectangle _inventoryArea;

	public List<SearchTask> Tasks { get; } = new List<SearchTask>();

	public WeaponSwapTasks(ActionCenter actionCenter)
	{
		_actionCenter = actionCenter;
		var rectangles = Settings.Instance.ScreenCapture.RectanglesSettings;
		_weaponsArea = rectangles.Weapons.GetRectangle();
		// The swap may be pointed at just the rows the spare weapons live in, so that narrowing it
		// does not also narrow the empty-slot count, which needs the whole bag. Unset means "use
		// the whole inventory", which is what this did before the separate area existed.
		Rectangle weaponInventory = rectangles.WeaponInventory.GetRectangle();
		_inventoryArea = (weaponInventory.Width > 0 && weaponInventory.Height > 0)
			? weaponInventory
			: rectangles.Inventory.GetRectangle();
		CreateTasks();
	}

	private void CreateTasks()
	{
		for (int weapon = 1; weapon <= WeaponSwapPlan.WeaponCount; weapon++)
		{
			// Trigger only - deliberately no action, because right-clicking here would take the
			// weapon off before we know a replacement exists.
			Add(WeaponSwapPlan.BrokenEquipped(weapon),
				WeaponSwapPlan.BrokenTemplateKey(weapon),
				WeaponSwapPlan.BrokenTemplateDefault(weapon),
				_weaponsArea,
				onMatchFound: null,
				intervalMs: 5000);

			// The swap itself: one right-click on the repaired weapon in the inventory.
			Add(WeaponSwapPlan.EquipFromInventory(weapon),
				WeaponSwapPlan.RepairedTemplateKey(weapon),
				WeaponSwapPlan.RepairedTemplateDefault(weapon),
				_inventoryArea,
				onMatchFound: _actionCenter._inventortyActions.MoveAndRightClick,
				intervalMs: 1000);
		}

		CreateLeftHandTasks();
	}

	/// <summary>
	/// The left hand cannot be filled by right-clicking, so the item is dragged there instead.
	/// </summary>
	private void CreateLeftHandTasks()
	{
		for (int item = 1; item <= WeaponSwapPlan.WeaponCount; item++)
		{
			string brokenKey = WeaponSwapPlan.LeftBrokenTemplateKey(item);
			string brokenDefault = WeaponSwapPlan.LeftBrokenTemplateDefault(item);
			string repairedKey = WeaponSwapPlan.LeftRepairedTemplateKey(item);
			string repairedDefault = WeaponSwapPlan.LeftRepairedTemplateDefault(item);

			// Trigger - look only.
			Add(WeaponSwapPlan.LeftBrokenEquipped(item), brokenKey, brokenDefault, _weaponsArea,
				onMatchFound: null, intervalMs: 5000);

			// Grab the repaired item in the inventory. One variant per broken item we are heading
			// for, because the drop step differs per slot occupant.
			for (int broken = 1; broken <= WeaponSwapPlan.WeaponCount; broken++)
			{
				Add(WeaponSwapPlan.LeftGrabDirect(item, broken), repairedKey, repairedDefault, _inventoryArea,
					onMatchFound: _actionCenter._inventortyActions.MoveAndLeftButtonDown);
			}

			// Release onto the broken item sitting in the slot - the two swap. If it vanished
			// meanwhile, still release so the mouse button is never left held down.
			AddWithRelease(WeaponSwapPlan.LeftDropOnBroken(item), brokenKey, brokenDefault, _weaponsArea);
		}
	}

	private void AddWithRelease(string taskId, string templateKey, string templateDefault, Rectangle area)
	{
		Tasks.Add(new SearchTask
		{
			TaskId = taskId,
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve(taskId, TemplateResolver.Resolve(templateKey, templateDefault)),
				SearchArea = area,
				OnMatchFound = _actionCenter._weaponsActions.MoveAndLeftButtonUp,
				OnMatchNotFound = _actionCenter._weaponsActions.LeftButtonUp,
				UseColor = true,
				Threshold = MatchThreshold,
				IntervalMs = 500
			},
			Mode = SearchMode.Single
		});
	}

	private void Add(string taskId, string templateKey, string templateDefault, Rectangle area,
		System.Action<Point> onMatchFound, int intervalMs = 500)
	{
		Tasks.Add(new SearchTask
		{
			TaskId = taskId,
			Config = new SearchConfig
			{
				TemplatePath = TemplateResolver.Resolve(taskId, TemplateResolver.Resolve(templateKey, templateDefault)),
				SearchArea = area,
				OnMatchFound = onMatchFound,
				OnMatchNotFound = null,
				UseColor = true,
				Threshold = MatchThreshold,
				IntervalMs = intervalMs
			},
			Mode = SearchMode.Single
		});
	}
}
