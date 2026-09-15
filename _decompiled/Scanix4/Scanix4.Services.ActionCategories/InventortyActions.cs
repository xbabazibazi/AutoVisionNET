using System;
using InputManager;
using Scanix4.Services.BaseClasses;
using SettingsManager;
using SettingsManager.ScreenCapture;
using SimpleLogger;
using SnapNetClient;

namespace Scanix4.Services.ActionCategories;

public class InventortyActions(Alarm alarm, Logger logger, InputUtils inputUtils) : BaseActions(inputUtils, logger)
{
	private readonly Alarm _alarm = alarm;

	private new readonly InputUtils _inputUtils = inputUtils;

	private readonly Logger _logger = logger ?? throw new ArgumentNullException("logger");

	private string? _nextTaskId;

	private readonly InventorySlotAlert _settings = Settings.Instance.ScreenCapture.InventorySlotAlert;

	public string? GetNextTaskId()
	{
		return _nextTaskId;
	}

	public void ResetNextTaskId()
	{
		_nextTaskId = null;
	}

	public void OnInventorySlotAlert(int EmptySlot)
	{
		if (EmptySlot <= _settings.LowSlotThreshold)
		{
			_logger.LogInformation($"Envanterde boş slot sayısı {EmptySlot} olarak algılandı. Uyarı gönderiliyor.");
			AppClient.SendCommandAsync("301");
		}
	}
}
