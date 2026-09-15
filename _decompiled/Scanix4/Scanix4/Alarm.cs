using System;
using System.IO;
using System.Media;
using SimpleLogger;

namespace Scanix4;

public class Alarm : IDisposable
{
	private SoundPlayer? _player;

	private bool _isAlarmActive;

	private readonly object _lock = new object();

	private readonly Logger _logger;

	private readonly string _alarmFilePath;

	private bool _disposed;

	public Alarm(Logger logger)
	{
		_alarmFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "alarm.wav");
		_logger = logger ?? throw new ArgumentNullException("logger");
		InitializeSoundPlayer();
	}

	private void InitializeSoundPlayer()
	{
		try
		{
			if (!File.Exists(_alarmFilePath))
			{
				_logger.LogError("Alarm file not found: " + _alarmFilePath);
				throw new FileNotFoundException("Alarm file not found.", _alarmFilePath);
			}
			_player = new SoundPlayer(_alarmFilePath);
			_player.Load();
		}
		catch (Exception ex)
		{
			_logger.LogError("Alarm initialization failed: " + ex.Message);
			throw;
		}
	}

	public void StartAlarm()
	{
		lock (_lock)
		{
			if (_isAlarmActive)
			{
				return;
			}
			try
			{
				_player?.PlayLooping();
				_isAlarmActive = true;
				_logger.LogInformation("Alarm started");
			}
			catch (Exception ex)
			{
				_logger.LogError("Alarm play error: " + ex.Message);
				_isAlarmActive = false;
			}
		}
	}

	public void StopAlarm()
	{
		lock (_lock)
		{
			if (!_isAlarmActive)
			{
				return;
			}
			try
			{
				_player?.Stop();
				_isAlarmActive = false;
				_logger.LogInformation("Alarm stopped");
			}
			catch (Exception ex)
			{
				_logger.LogError("Alarm stop error: " + ex.Message);
			}
		}
	}

	public bool IsAlarmActive()
	{
		lock (_lock)
		{
			return _isAlarmActive;
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			try
			{
				StopAlarm();
				_player?.Dispose();
				_logger.LogInformation("Alarm disposed");
			}
			catch (Exception ex)
			{
				_logger.LogError("Alarm dispose error: " + ex.Message);
			}
			_disposed = true;
		}
	}
}
