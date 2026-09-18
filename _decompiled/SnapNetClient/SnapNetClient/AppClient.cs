using System;
using System.Threading.Tasks;

namespace SnapNetClient;

public static class AppClient
{
	private static Client _instance;

	private static readonly object _lock = new object();

	private static bool _isInitialized = false;

	private static string _serverIp;

	private static int _serverPort;

	private static string _nickname;

	private static Client.JobType _job;

	public static string Nickname => _instance?.Nickname;

	public static Client.JobType Job => _instance?.Job ?? Client.JobType.None;

	public static bool IsConnected => _instance?.IsConnected ?? false;

	public static event Action<string> MessageReceived;

	public static event Action Connected;

	public static event Action Disconnected;

	public static event Action<Exception> ConnectionFailed;

	public static event Action<string[]> PartyFormRequested;

	private static void WireClientEvents(Client client)
	{
		client.MessageReceived += delegate(string msg)
		{
			MessageReceived?.Invoke(msg);
		};
		client.Connected += delegate
		{
			Connected?.Invoke();
		};
		client.Disconnected += delegate
		{
			Disconnected?.Invoke();
		};
		client.ConnectionFailed += delegate(Exception ex)
		{
			ConnectionFailed?.Invoke(ex);
		};
		client.PartyFormRequested += delegate(string[] members)
		{
			PartyFormRequested?.Invoke(members);
		};
	}

	public static void Initialize(string serverIp, int serverPort, string nickname, Client.JobType job)
	{
		lock (_lock)
		{
			if (_isInitialized)
			{
				Reset();
			}
			_serverIp = serverIp;
			_serverPort = serverPort;
			_nickname = nickname;
			_job = job;
			_instance = new Client();
			WireClientEvents(_instance);
			_isInitialized = true;
		}
	}

	public static async Task ConnectAsync()
	{
		if (!_isInitialized)
		{
			throw new InvalidOperationException("AppClient başlatılmamış. Önce Initialize() çağrılmalı.");
		}
		await _instance.ConnectAsync(_serverIp, _serverPort, _nickname, _job);
	}

	public static void Disconnect()
	{
		_instance?.Disconnect();
	}

	public static void Reset()
	{
		lock (_lock)
		{
			if (_instance != null)
			{
				_instance.Disconnect();
				_instance.Dispose();
			}
			_instance = null;
			_isInitialized = false;
		}
	}

	public static Task SendCommandAsync(string command)
	{
		if (_instance == null || !_instance.IsConnected)
		{
			throw new InvalidOperationException("AppClient başlatılmamış veya bağlı değil");
		}
		return _instance.SendCommandAsync(command);
	}

	public static void RegisterCommand(string command, Action handler)
	{
		if (_instance == null)
		{
			throw new InvalidOperationException("AppClient başlatılmamış");
		}
		_instance.RegisterCommand(command, handler);
	}
}
