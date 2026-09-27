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

	// A count of 0 is ambiguous: it means "bag completely full" but it is also what the scan
	// returns whenever the inventory window simply isn't open right now - which, in normal bot
	// operation, is most of the time. A zero is only trusted as "full" if the bag was confirmed
	// open (a non-zero reading) recently; once that confirmation goes stale, the bag is assumed
	// closed rather than full, however long ago it was last actually seen full or empty. Without
	// this window, a single non-zero reading anywhere in the session would make the alert fire
	// every single time the bag was later closed for more than a couple of polls.
	private static readonly TimeSpan MaxGapSinceConfirmedOpen = TimeSpan.FromSeconds(20.0);

	private readonly Alarm _alarm = alarm;

	private new readonly InputUtils _inputUtils = inputUtils;

	private readonly Logger _logger = logger ?? throw new ArgumentNullException("logger");

	private string? _nextTaskId;

	private readonly InventorySlotAlert _settings = Settings.Instance.ScreenCapture.InventorySlotAlert;

	private DateTime? _lastConfirmedOpenUtc;

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
	public static bool ShouldAlert(int emptySlot, int lowSlotThreshold, TimeSpan? timeSinceConfirmedOpen, int consecutiveZeroReadings)
	{
		if (emptySlot > 0)
		{
			return emptySlot <= lowSlotThreshold;
		}
		// emptySlot == 0: only trust this as "genuinely full" while we recently confirmed the bag
		// was actually open (otherwise 0 just means "the window is closed right now").
		return timeSinceConfirmedOpen.HasValue
			&& timeSinceConfirmedOpen.Value <= MaxGapSinceConfirmedOpen
			&& consecutiveZeroReadings >= RequiredZeroReadings;
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

		DateTime now = DateTime.UtcNow;
		if (EmptySlot > 0)
		{
			_lastConfirmedOpenUtc = now;
			_consecutiveZeroReadings = 0;
		}
		else
		{
			_consecutiveZeroReadings++;
		}

		TimeSpan? timeSinceConfirmedOpen = _lastConfirmedOpenUtc.HasValue ? now - _lastConfirmedOpenUtc.Value : null;
		if (!ShouldAlert(EmptySlot, _settings.LowSlotThreshold, timeSinceConfirmedOpen, _consecutiveZeroReadings))
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
