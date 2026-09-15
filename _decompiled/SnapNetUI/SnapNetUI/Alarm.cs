using System;
using System.IO;
using System.Media;

namespace SnapNetUI;

public class Alarm : IDisposable
{
	private SoundPlayer? _player;

	private bool _isAlarmActive;

	private bool _isSilentMode;

	private readonly object _lock = new object();

	private readonly string _alarmFilePath;

	private bool _disposed;

	public Alarm()
	{
		_alarmFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "alarm.wav");
		InitializeSoundPlayer();
	}

	private void InitializeSoundPlayer()
	{
		if (!File.Exists(_alarmFilePath))
		{
			throw new FileNotFoundException("Alarm file not found.", _alarmFilePath);
		}
		_player = new SoundPlayer(_alarmFilePath);
		_player.Load();
	}

	public void StartAlarm()
	{
		lock (_lock)
		{
			if (!_isAlarmActive && !_isSilentMode)
			{
				_player?.PlayLooping();
				_isAlarmActive = true;
			}
		}
	}

	public void StopAlarm()
	{
		lock (_lock)
		{
			if (_isAlarmActive)
			{
				_player?.Stop();
				_isAlarmActive = false;
			}
		}
	}

	public void SetSilentMode(bool isSilent)
	{
		lock (_lock)
		{
			_isSilentMode = isSilent;
			if (isSilent && _isAlarmActive)
			{
				_player?.Stop();
				_isAlarmActive = false;
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
			StopAlarm();
			_player?.Dispose();
			_disposed = true;
		}
	}
}
