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
	/// <summary>
	/// How many consecutive zero readings are required before a zero is trusted as "bag full".
	/// A single zero can be a capture glitch or a frame where the inventory was redrawing.
	/// </summary>
	private const int RequiredZeroReadings = 2;

	private readonly Alarm _alarm = alarm;

	private new readonly InputUtils _inputUtils = inputUtils;

	private readonly Logger _logger = logger ?? throw new ArgumentNullException("logger");

	private string? _nextTaskId;

	private readonly InventorySlotAlert _settings = Settings.Instance.ScreenCapture.InventorySlotAlert;

	// A count of 0 is ambiguous: it means "bag completely full" but it is also what the scan
	// returns when the inventory window simply is not open. We only trust a zero once we have
	// actually seen empty slots at least once, which proves the window is open and the template
	// matching works for this resolution/layout.
	private bool _hasSeenEmptySlots;

	private int _consecutiveZeroReadings;

	public string? GetNextTaskId()
	{
		return _nextTaskId;
	}

	public void ResetNextTaskId()
	{
		_nextTaskId = null;
	}

	/// <summary>
	/// Pure decision logic for the inventory alert, split out so it can be exercised directly.
	/// </summary>
	public static bool ShouldAlert(int emptySlot, int lowSlotThreshold, bool hasSeenEmptySlots, int consecutiveZeroReadings)
	{
		if (emptySlot > 0)
		{
			return emptySlot <= lowSlotThreshold;
		}
		// emptySlot == 0: only a genuinely full bag, never a closed/undetected inventory window.
		return hasSeenEmptySlots && consecutiveZeroReadings >= RequiredZeroReadings;
	}

	public void OnInventorySlotAlert(int EmptySlot)
	{
		bool suppressed = InventoryScanSuppressor.IsSuppressed;
		// Publish every reading - including suppressed ones - so the UI can show what the scanner
		// currently sees and the threshold can be set against real numbers instead of guesswork.
		InventorySlotMonitor.Report(EmptySlot, suppressed);

		// A repair dialog (or anything else that covers the inventory region) makes this reading
		// meaningless - it counts the dialog, not the bag. Drop it entirely without touching the
		// streak state, so a covered screen can neither raise an alarm nor corrupt the zero run.
		if (suppressed)
		{
			return;
		}

		if (EmptySlot > 0)
		{
			_hasSeenEmptySlots = true;
			_consecutiveZeroReadings = 0;
		}
		else
		{
			_consecutiveZeroReadings++;
		}

		if (!ShouldAlert(EmptySlot, _settings.LowSlotThreshold, _hasSeenEmptySlots, _consecutiveZeroReadings))
		{
			return;
		}

		_logger.LogInformation((EmptySlot == 0)
			? "Envanter tamamen dolu (0 boş slot) algılandı. Uyarı gönderiliyor."
			: $"Envanterde boş slot sayısı {EmptySlot} olarak algılandı. Uyarı gönderiliyor.");

		try
		{
			// AppClient.SendCommandAsync throws synchronously when the client is not connected.
			// Uncaught, that exception escapes the workflow loop and permanently kills the
			// InventorySlotAlert workflow, so the alert would never fire again.
			AppClient.SendCommandAsync("301");
		}
		catch (Exception ex)
		{
			_logger.LogWarning("Envanter uyarısı sunucuya gönderilemedi (alarm sesi panelde çalar, bu yüzden panele bağlı olmanız gerekir): " + ex.Message);
		}
	}
}
