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
		// No reading at all is NOT the same as "Genie is off". The status scan drops into passive
		// mode when its region was never drawn or its template file is missing, and IsActive then
		// stays false forever - which read as a confident "off" and clicked the button once a
		// second. Because the button toggles, that thrashed Genie on and off indefinitely. A
		// customer hit exactly this: a template reset pointed the status slot at a stock image
		// that does not match their client, the scan went quiet, and the bot clicked endlessly.
		// Refusing to click blind leaves Genie to be started by hand, which is strictly better
		// than fighting the user over it - and it is what GenieStatusTracker.HasReading exists for.
		if (!GenieStatusTracker.HasReading)
		{
			WarnStatusUnreadableOnce();
			return;
		}
		MoveAndLeftClick(coordinates);
	}

	private bool _warnedStatusUnreadable;

	/// <summary>Said once a session: at one click per second this would otherwise flood the log.</summary>
	private void WarnStatusUnreadableOnce()
	{
		if (_warnedStatusUnreadable)
		{
			return;
		}
		_warnedStatusUnreadable = true;
		_logger.LogWarning("Genie durumu okunamadığı için Genie başlatma tıklaması yapılmıyor. Şablonlar bölümünden \"Genie Durumu\" görselini bu bilgisayarda yeniden atayın ve arama bölgesinin çizili olduğundan emin olun.");
	}
}
