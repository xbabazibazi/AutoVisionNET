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

public class PartyActions(Alarm alarm, Logger logger, InputUtils inputUtils) : BaseActions(inputUtils, logger)
{
	private readonly Alarm _alarm = alarm;

	private new readonly InputUtils _inputUtils = inputUtils;

	private readonly Logger _logger = logger ?? throw new ArgumentNullException("logger");

	private readonly ScreenCaptureSettings _settings = Settings.Instance.ScreenCapture;

	private string? _nextTaskId;

	public void OnHandlePartyMemberDeath(Point coordinates)
	{
		if (_settings.HandlePartyMemberDeath.Alarm)
		{
			_logger.LogInformation("HandlePartyMemberDeath Alarm");
			_alarm.StartAlarm();
		}
		if (_settings.HandlePartyMemberDeath.BreakParty)
		{
			_nextTaskId = "PartyHeader";
		}
		else
		{
			_nextTaskId = null;
		}
	}

	public void OnBreakParty(Point coordinates)
	{
		Logger.LogInformation("OnPartyHeader");
		MoveAndLeftClick(coordinates);
	}

	public void OnFoundPartyHeader(Point coordinates)
	{
		Logger.LogInformation("OnFoundPartyHeader");
		MoveAndLeftClick(new Point(coordinates.X, coordinates.Y + 25));
		Thread.Sleep(200);
	}

	public void OnPartyCount(int PartyMemberCount)
	{
		if (PartyMemberCount < _settings.PartyMemberCount.PartyCount && _settings.PartyMemberCount.Alarm)
		{
			_logger.LogInformation("OnPartyCount Alarm");
			_alarm.StartAlarm();
		}
		if (_settings.PartyMemberCount.BreakParty)
		{
			_nextTaskId = "PartyHeader";
		}
		else
		{
			_nextTaskId = null;
		}
	}

	public void OnCureDb(Point coordinates)
	{
		_inputUtils?.SimulateKeyPress(KeyCode.Zero, 50);
		Thread.Sleep(50);
		if (_settings.CureDB.Alarm)
		{
			Logger.LogInformation("CureDB Alarm");
			_alarm.StartAlarm();
		}
	}

	public void OnTown(Point coordinates)
	{
		MoveAndLeftClick(coordinates);
		_alarm.StartAlarm();
	}

	public string? GetNextTaskId()
	{
		return _nextTaskId;
	}

	public void ResetNextTaskId()
	{
		_nextTaskId = null;
	}
}
