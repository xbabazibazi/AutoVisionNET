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

	public void OnPartyFound(Point coordinates)
	{
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
		_inputUtils.SimulateKeyPress(KeyCode.I, 100);
	}

	public void WhellOfFunYesButton()
	{
		_inputUtils.SimulateKeyPress(KeyCode.Escape, 100);
		_logger.LogDebug("Escape tuşu simüle edildi.");
		_inputUtils.SimulateKeyPress(KeyCode.Escape, 100);
		_logger.LogDebug("Escape tuşu simüle edildi.");
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
