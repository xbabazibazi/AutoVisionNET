using System;

namespace SimpleLogger;

public class Logger : IDisposable
{
	private static Logger? _instance;

	private Action<string>? _logMethod;

	private LogLevel _currentLogLevel = LogLevel.Info;

	public static Logger Instance => _instance ?? (_instance = new Logger());

	public void SetLogMethod(Action<string> logMethod)
	{
		_logMethod = logMethod;
	}

	public void SetLogLevel(LogLevel level)
	{
		_currentLogLevel = level;
	}

	public LogLevel GetCurrentLogLevel()
	{
		return _currentLogLevel;
	}

	public void LogDebug(string message)
	{
		LogIf(LogLevel.Debug, message, "DEBUG");
	}

	public void LogInformation(string message)
	{
		LogIf(LogLevel.Info, message, "INFO");
	}

	public void LogWarning(string message)
	{
		LogIf(LogLevel.Warning, message, "WARN");
	}

	public void LogError(string message)
	{
		LogIf(LogLevel.Error, message, "ERROR");
	}

	public void LogError(string message, Exception ex)
	{
		LogError(message + " | Hata: " + ex?.Message);
	}

	private void LogIf(LogLevel level, string message, string logLevel)
	{
		if (_currentLogLevel <= level)
		{
			_logMethod?.Invoke($"[{DateTime.Now:HH:mm:ss.fff}] [{logLevel}] {message}");
		}
	}

	public void Dispose()
	{
		_logMethod = null;
	}
}
