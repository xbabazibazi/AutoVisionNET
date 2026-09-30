using System;
using System.Drawing;
using System.Threading;
using InputInterceptorNS;
using InputManager;
using Scanix4.Services.BaseClasses;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;

namespace Scanix4.Services.ActionCategories;

public class RequestActions(Alarm alarm, Logger logger, InputUtils inputUtils) : BaseActions(inputUtils, logger)
{
	private readonly Alarm _alarm = alarm;

	private new readonly InputUtils _inputUtils = inputUtils;

	private readonly Logger _logger = logger ?? throw new ArgumentNullException("logger");

	private readonly ScreenCaptureSettings _settings = Settings.Instance.ScreenCapture;

	// Belt-and-braces on top of RequestPartyWorkflow's own state machine: that workflow already
	// steps away from "RequestParty" after a match, but if the precursor cue ("EVOX") is still
	// visible right after the click (dialog not fully closed yet, or a lingering UI element), the
	// workflow bounces straight back into "RequestParty" and re-clicks the same still-open dialog.
	// A live log showed this firing ~13 times in 15s at the exact same coordinates. This cooldown
	// makes that impossible regardless of what the state machine does.
	private static readonly TimeSpan ClickCooldown = TimeSpan.FromSeconds(5.0);

	private DateTime _lastClickedUtc = DateTime.MinValue;

	public void OnPartyFound(Point coordinates)
	{
		if (DateTime.UtcNow - _lastClickedUtc < ClickCooldown)
		{
			return;
		}
		_lastClickedUtc = DateTime.UtcNow;
		_logger.LogInformation($"OnPartyFound tetiklendi. Koordinatlar: ({coordinates.X}, {coordinates.Y})");
		MoveAndLeftClick(coordinates);
		_logger.LogDebug("Sol tıklama simüle edildi.");
		if (_settings.AcceptParty.Alarm)
		{
			_logger.LogInformation("RequestParty Alarm tetiklendi.");
			_alarm.StartAlarm();
		}
	}

	public void WhellOfFunYesButton(Point coordinates)
	{
		_logger.LogInformation($"WhellOfFunYesButton tetiklendi. Koordinatlar: ({coordinates.X}, {coordinates.Y})");
		MoveAndLeftClick(coordinates);
		_logger.LogDebug("Sol tıklama simüle edildi.");
		Thread.Sleep(10000);
		_inputUtils.SimulateKeyPress(KeyCode.Escape, 100);
		_logger.LogDebug("Escape tuşu simüle edildi.");
		_inputUtils.SimulateKeyPress(KeyCode.Escape, 100);
		_logger.LogDebug("Escape tuşu simüle edildi.");
		// The I keypress toggles the inventory panel, so it can just as easily close an
		// already-open bag as open a closed one - either way the next scan or two would
		// otherwise read a false "0 empty slots" off the mid-toggle/closed window.
		InventoryScanSuppressor.Suppress();
		_inputUtils.SimulateKeyPress(KeyCode.I, 100);
	}

	public void WhellOfFunYesButton()
	{
		_inputUtils.SimulateKeyPress(KeyCode.Escape, 100);
		_logger.LogDebug("Escape tuşu simüle edildi.");
		_inputUtils.SimulateKeyPress(KeyCode.Escape, 100);
		_logger.LogDebug("Escape tuşu simüle edildi.");
		InventoryScanSuppressor.Suppress();
		_inputUtils.SimulateKeyPress(KeyCode.I, 100);
	}

	public void MoveAndLeftClickWithDelay(Point coordinates)
	{
		_logger.LogInformation($"MoveAndLeftClickWithDelay tetiklendi. Koordinatlar: ({coordinates.X}, {coordinates.Y})");
		MoveAndLeftClick(coordinates);
		_logger.LogDebug("Sol tıklama simüle edildi.");
		Thread.Sleep(1000);
	}
}
