using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class Server : IDisposable
{
	public enum JobType
	{
		None,
		Mage,
		Rogue,
		Warrior,
		Kurian,
		Priest
	}

	private class ClientInfo : IDisposable
	{
		public TcpClient Client { get; }

		public JobType Job { get; }

		public DateTime LastActivity { get; set; }

		public bool? LastVerificationOk { get; set; }

		public DateTime? LastVerificationTime { get; set; }

		public NetworkStream Stream { get; }

		public ClientInfo(TcpClient client, JobType job)
		{
			Client = client;
			Job = job;
			Stream = client.GetStream();
			LastActivity = DateTime.UtcNow;
		}

		public void Dispose()
		{
			try
			{
				Stream?.Dispose();
				Client.Dispose();
			}
			catch
			{
			}
		}
	}

	private TcpListener _listener;

	private readonly ConcurrentDictionary<string, ClientInfo> _clients = new ConcurrentDictionary<string, ClientInfo>();

	private readonly Timer _connectionMonitor;

	private readonly Timer _pingTimer;

	private volatile bool _isRunning;

	private const int PingInterval = 5000;

	private const int TimeoutSeconds = 30;

	private const int MaxMessageSize = 67108864;

	private readonly ConcurrentDictionary<string, Action<string>> _commandHandlers = new ConcurrentDictionary<string, Action<string>>();

	public event Action<int> ClientCountChanged;

	public event Action<string> LogMessage;

	public event Action ClientStatusUpdated;

	public Server()
	{
		_connectionMonitor = new Timer(delegate
		{
			CheckConnections();
		}, null, 5000, 5000);
		_pingTimer = new Timer(delegate
		{
			SendPingToAll();
		}, null, 5000, 5000);
	}

	private async void SendPingToAll()
	{
		if (!_isRunning)
		{
			return;
		}
		List<Task> tasks = new List<Task>();
		foreach (ClientInfo client in _clients.Values.ToList())
		{
			try
			{
				if (client.Client.Connected)
				{
					tasks.Add(SendMessageAsync(client.Stream, "PING"));
				}
			}
			catch
			{
				string key = _clients.FirstOrDefault<KeyValuePair<string, ClientInfo>>((KeyValuePair<string, ClientInfo> x) => x.Value == client).Key;
				if (key != null)
				{
					_clients.TryRemove(key, out ClientInfo _);
				}
			}
		}
		await Task.WhenAll(tasks);
	}

	public async Task StartAsync(int port)
	{
		_listener = new TcpListener(IPAddress.Any, port);
		_listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
		_listener.Start();
		_isRunning = true;
		LogMessage?.Invoke($"SERVER_START:{port}");
		try
		{
			while (_isRunning)
			{
				TcpClient client = await _listener.AcceptTcpClientAsync();
				client.NoDelay = true;
				HandleClientAsync(client);
			}
		}
		catch (ObjectDisposedException)
		{
		}
		catch (Exception ex2)
		{
			if (_isRunning)
			{
				LogMessage?.Invoke("ERROR_ACCEPT:" + ex2.Message);
			}
		}
	}

	public void RegisterCommand(string command, Action<string> handler)
	{
		_commandHandlers[command] = handler;
	}

	private async Task HandleClientAsync(TcpClient client)
	{
		string nickname = null;
		try
		{
			using (client)
			{
				NetworkStream stream = client.GetStream();
				string initMessage = await ReadMessageAsync(stream);
				if (initMessage == null)
				{
					return;
				}
				string[] parts = initMessage.Split('|');
				if (parts.Length != 2)
				{
					await SendMessageAsync(stream, "ERROR:Invalid initial message");
					return;
				}
				nickname = parts[0];
				if (!Enum.TryParse<JobType>(parts[1], out var job) || !Enum.IsDefined(typeof(JobType), job))
				{
					await SendMessageAsync(stream, "ERROR:Invalid job type");
					return;
				}
				if (_clients.ContainsKey(nickname))
				{
					await SendMessageAsync(stream, "ERROR:Nickname already taken");
					return;
				}
				ClientInfo clientInfo = new ClientInfo(client, job);
				_clients[nickname] = clientInfo;
				ClientCountChanged?.Invoke(_clients.Count);
				LogMessage?.Invoke($"CLIENT_CONNECT:{nickname} ({job})");
				await SendMessageAsync(stream, "OK");
				while (_isRunning && client.Connected)
				{
					string message = await ReadMessageAsync(stream);
					if (message == null)
					{
						break;
					}
					clientInfo.LastActivity = DateTime.UtcNow;
					if (message == "PING")
					{
						await SendMessageAsync(stream, "PONG");
					}
					else
					{
						if (message == "PONG")
						{
							continue;
						}
						if (message.Contains('|'))
						{
							string[] msgParts = message.Split('|');
							if (msgParts.Length == 3)
							{
								string msgNickname = msgParts[0];
								string msgJob = msgParts[1];
								string commandStr = msgParts[2].Trim();
								if (!Enum.TryParse<JobType>(msgJob, out var msgJobEnum))
								{
									LogMessage?.Invoke("INVALID_JOB:" + msgJob);
									continue;
								}
								if (string.Equals(msgNickname, nickname, StringComparison.OrdinalIgnoreCase) && msgJobEnum == clientInfo.Job)
								{
									if (commandStr == "VOK" || commandStr == "VFAIL")
									{
										clientInfo.LastVerificationOk = commandStr == "VOK";
										clientInfo.LastVerificationTime = DateTime.UtcNow;
										ClientStatusUpdated?.Invoke();
										continue;
									}
									if (_commandHandlers.TryGetValue(commandStr, out Action<string> handler))
									{
										handler(nickname);
									}
									else
									{
										LogMessage?.Invoke("UNKNOWN_COMMAND:" + nickname + ":" + commandStr);
									}
									handler = null;
									continue;
								}
								LogMessage?.Invoke($"AUTH_MISMATCH: {msgNickname} vs {nickname}, {msgJob} vs {clientInfo.Job}");
							}
							else
							{
								LogMessage?.Invoke("INVALID_CMD_FORMAT: " + message);
							}
						}
						else
						{
							LogMessage?.Invoke("UNPROCESSED_MSG:" + message);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			LogMessage?.Invoke(string.Concat(str3: ex.Message, str0: "ERROR:", str1: nickname ?? "unknown", str2: ":"));
		}
		finally
		{
			if (nickname != null && _clients.TryRemove(nickname, out ClientInfo clientInfo2))
			{
				clientInfo2.Dispose();
				ClientCountChanged?.Invoke(_clients.Count);
				LogMessage?.Invoke("CLIENT_DISCONNECT:" + nickname);
			}
		}
	}

	public async Task SendCommandToSpecificJobAsync(string command, JobType job, bool withDelay = true)
	{
		List<KeyValuePair<string, ClientInfo>> clientsToSend = _clients.Where<KeyValuePair<string, ClientInfo>>((KeyValuePair<string, ClientInfo> c) => c.Value.Client.Connected && c.Value.Job == job).ToList();
		for (int i = 0; i < clientsToSend.Count; i++)
		{
			KeyValuePair<string, ClientInfo> client = clientsToSend[i];
			try
			{
				await SendMessageAsync(message: $"{client.Key}|{(int)client.Value.Job}|{command}", stream: client.Value.Stream);
			}
			catch (Exception ex)
			{
				LogMessage?.Invoke("Gönderme hatası (" + client.Key + "): " + ex.Message);
			}
			if (withDelay && i < clientsToSend.Count - 1)
			{
				await Task.Delay(2000);
			}
		}
	}

	public async Task SendCommandToAllClientsAsync(string command, bool withDelay = true)
	{
		List<KeyValuePair<string, ClientInfo>> clientsToSend = _clients.Where<KeyValuePair<string, ClientInfo>>((KeyValuePair<string, ClientInfo> c) => c.Value.Client.Connected).ToList();
		for (int i = 0; i < clientsToSend.Count; i++)
		{
			KeyValuePair<string, ClientInfo> client = clientsToSend[i];
			try
			{
				await SendMessageAsync(message: $"{client.Key}|{(int)client.Value.Job}|{command}", stream: client.Value.Stream);
			}
			catch (Exception ex)
			{
				LogMessage?.Invoke("Gönderme hatası (" + client.Key + "): " + ex.Message);
			}
			if (withDelay && i < clientsToSend.Count - 1)
			{
				await Task.Delay(2000);
			}
		}
	}

	private async Task<string> ReadMessageAsync(NetworkStream stream)
	{
		byte[] lengthBuffer = new byte[4];
		if (await stream.ReadAsync(lengthBuffer, 0, 4) != 4)
		{
			return null;
		}
		int length = BitConverter.ToInt32(lengthBuffer, 0);
		if (length <= 0 || length > 67108864)
		{
			LogMessage?.Invoke($"Geçersiz mesaj boyutu: {length}");
			return null;
		}
		byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
		try
		{
			int bytesRead;
			for (int totalRead = 0; totalRead < length; totalRead += bytesRead)
			{
				bytesRead = await stream.ReadAsync(buffer, totalRead, Math.Min(8192, length - totalRead));
				if (bytesRead == 0)
				{
					return null;
				}
			}
			return Encoding.UTF8.GetString(buffer, 0, length);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	private async Task SendMessageAsync(NetworkStream stream, string message)
	{
		byte[] data = Encoding.UTF8.GetBytes(message);
		if (data.Length > 67108864)
		{
			LogMessage?.Invoke($"Mesaj çok büyük: {data.Length} bayt");
		}
		else
		{
			byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
			await stream.WriteAsync(lengthPrefix, 0, 4);
			await stream.WriteAsync(data, 0, data.Length);
		}
	}

	private void CheckConnections()
	{
		if (!_isRunning)
		{
			return;
		}
		DateTime now = DateTime.UtcNow;
		List<string> list = (from c in _clients
			where now - c.Value.LastActivity > TimeSpan.FromSeconds(30.0)
			select c.Key).ToList();
		foreach (string item in list)
		{
			if (!_clients.TryRemove(item, out ClientInfo value))
			{
				continue;
			}
			try
			{
				value.Stream.Close();
				value.Client.Close();
			}
			catch
			{
			}
			finally
			{
				value.Dispose();
				LogMessage?.Invoke("CLIENT_TIMEOUT:" + item);
			}
		}
		if (list.Count > 0)
		{
			ClientCountChanged?.Invoke(_clients.Count);
		}
	}

	public void Dispose()
	{
		_pingTimer?.Dispose();
		_connectionMonitor?.Dispose();
		StopAsync().Wait();
	}

	public IEnumerable<(string nickname, string job, string endpoint, bool? lastVerificationOk, DateTime? lastVerificationTime)> GetClientList()
	{
		return _clients.Select<KeyValuePair<string, ClientInfo>, (string, string, string, bool?, DateTime?)>((KeyValuePair<string, ClientInfo> c) => (Key: c.Key, c.Value.Job.ToString(), c.Value.Client.Client.RemoteEndPoint?.ToString() ?? "disconnected", c.Value.LastVerificationOk, c.Value.LastVerificationTime));
	}

	public async Task StopAsync()
	{
		if (!_isRunning)
		{
			return;
		}
		_isRunning = false;
		_listener?.Stop();
		foreach (ClientInfo clientInfo in _clients.Values)
		{
			clientInfo.Dispose();
		}
		_clients.Clear();
		ClientCountChanged?.Invoke(0);
		LogMessage?.Invoke("SERVER_STOP");
	}
}
