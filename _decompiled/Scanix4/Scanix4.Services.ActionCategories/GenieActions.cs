using System;
using System.Drawing;
using InputManager;
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
		MoveAndLeftClick(coordinates);
	}
}
