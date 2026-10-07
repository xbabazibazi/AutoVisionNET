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
		// stays false forever. Clicking on that basis once a second thrashed Genie on and off
		// endlessly, because the button toggles.
		// Refusing to click outright is not the answer either: it silently kills the workflows
		// that depend on this step - the IceResist-after-TP chain starts the macro through here,
		// so a blind refusal means the macro simply never starts.
		// So: click while blind, but no more than once per interval. One stray toggle every
		// fifteen seconds is something a player recovers from; sixty a minute is not. The warning
		// points at the real fix, which is to assign a status template that matches this client.
		if (!GenieStatusTracker.HasReading)
		{
			if (_blindClickTimer.IsRunning && _blindClickTimer.Elapsed < BlindClickInterval)
			{
				return;
			}
			_blindClickTimer.Restart();
			WarnStatusUnreadableOnce();
			MoveAndLeftClick(coordinates);
			return;
		}
		MoveAndLeftClick(coordinates);
	}

	/// <summary>
	/// How long to wait between clicks taken without a status reading. Deliberately long: every
	/// such click is a coin flip that may switch Genie off, so it has to be rare enough to be
	/// recoverable while still letting the start-Genie workflows make progress.
	/// </summary>
	private static readonly TimeSpan BlindClickInterval = TimeSpan.FromSeconds(15.0);

	/// <summary>Measures the gap between blind clicks. A Stopwatch, so changing the PC clock cannot affect it.</summary>
	private readonly System.Diagnostics.Stopwatch _blindClickTimer = new System.Diagnostics.Stopwatch();

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
