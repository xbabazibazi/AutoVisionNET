using System;
using System.Drawing;
using InputManager;
using Scanix4.Services;
using Scanix4.Services.BaseClasses;
using SimpleLogger;

namespace Scanix4.Services.ActionCategories;

public class GenieActions(Alarm alarm, Logger logger, InputUtils inputUtils) : BaseActions(inputUtils, logger)
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

	public void OnStartGenie(Point coordinates)
	{
		// The in-game button toggles rather than only starting Genie, so clicking it while it is
		// already on turns it back off. A redundant "start" command (e.g. a job-wide remote
		// command re-sent to fix one character) used to switch off every other character that
		// already had Genie running.
		if (GenieStatusTracker.IsActive)
		{
			_logger.LogDebug("OnStartGenie: Genie zaten aktif, tıklama atlanıyor.");
			return;
		}
		MoveAndLeftClick(coordinates);
	}
}
