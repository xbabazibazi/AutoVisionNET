using System;
using System.Drawing;
using InputManager;
using Scanix4.Services.BaseClasses;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;

namespace Scanix4.Services.ActionCategories;

public class BuffLineActions(Alarm alarm, Logger logger, InputUtils inputUtils) : BaseActions(inputUtils, logger)
{
	private readonly Alarm _alarm = alarm;

	private new readonly InputUtils _inputUtils = inputUtils;

	private readonly Logger _logger = logger ?? throw new ArgumentNullException("logger");

	private readonly ReReRe _settings = Settings.Instance.ScreenCapture.ReReRe;

	private string? _nextTaskId;

	public string? GetNextTaskId()
	{
		return _nextTaskId;
	}

	public void ResetNextTaskId()
	{
		_nextTaskId = null;
	}

	public void OnCheckReistance(Point coordinates)
	{
		_nextTaskId = "StartGenie";
	}

	public void On300Ac(Point coordinates)
	{
		if (_settings.ReReRe300Ac)
		{
			MoveAndDoubleLeftClick(coordinates);
		}
	}

	public void OnSw(Point coordinates)
	{
		if (_settings.ReReReSw)
		{
			MoveAndDoubleLeftClick(coordinates);
		}
	}

	public void OnUndy(Point coordinates)
	{
		if (_settings.ReReReUndy)
		{
			MoveAndDoubleLeftClick(coordinates);
		}
	}

	public void OnWolf(Point coordinates)
	{
		if (_settings.ReReReWolf)
		{
			MoveAndDoubleLeftClick(coordinates);
		}
	}
}
