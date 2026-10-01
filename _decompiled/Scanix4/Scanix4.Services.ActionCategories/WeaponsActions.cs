using System;
using System.Drawing;
using System.Threading;
using InputInterceptorNS;
using InputManager;
using Scanix4.Services;
using Scanix4.Services.BaseClasses;
using SimpleLogger;

namespace Scanix4.Services.ActionCategories;

public class WeaponsActions(Alarm alarm, Logger logger, InputUtils inputUtils) : BaseActions(inputUtils, logger)
{
	private readonly Alarm _alarm = alarm;

	private new readonly InputUtils _inputUtils = inputUtils;

	private readonly Logger _logger = logger ?? throw new ArgumentNullException("logger");

	private string? _nextTaskId;

	public string? GetNextTaskId()
	{
		return _nextTaskId;
	}

	public void ResetNextTaskId()
	{
		_nextTaskId = null;
	}

	public void OnMatchFoundBrokenFullPlateArmorPauldron(Point coordinates)
	{
		Logger.LogInformation("RepairArmors OnMatchFoundBrokenFullPlateArmorPauldron");
		// The repair dialog is about to cover the inventory region; tell the inventory alert to
		// ignore its readings until it is gone, otherwise it counts the dialog and false-alarms.
		InventoryScanSuppressor.Suppress();
		RepairStatusTracker.ReportAttempt();
		inputUtils.SimulateKeyPress(KeyCode.Seven, 50);
		Thread.Sleep(200);
	}

	public void OnMatchFoundRepairTomahawk(Point coordinates)
	{
		Logger.LogInformation("RepairWeapons OnMatchFoundRepairTomahawk");
		InventoryScanSuppressor.Suppress();
		RepairStatusTracker.ReportAttempt();
		inputUtils.SimulateKeyPress(KeyCode.Seven, 50);
		Thread.Sleep(200);
	}

	/// <summary>Wired as OnMatchNotFound for the repair tasks - runs on every reading where the
	/// broken icon isn't there, which is most of the time; RepairStatusTracker itself is a cheap
	/// no-op unless a repair attempt is actually in flight.</summary>
	public void OnRepairResolved()
	{
		RepairStatusTracker.ReportResolved();
	}
}
