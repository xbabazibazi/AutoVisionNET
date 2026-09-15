using System;
using System.Drawing;
using System.Threading;
using InputInterceptorNS;
using InputManager;
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
		inputUtils.SimulateKeyPress(KeyCode.Seven, 50);
		Thread.Sleep(200);
	}

	public void OnMatchFoundRepairTomahawk(Point coordinates)
	{
		Logger.LogInformation("RepairWeapons OnMatchFoundRepairTomahawk");
		inputUtils.SimulateKeyPress(KeyCode.Seven, 50);
		Thread.Sleep(200);
	}
}
