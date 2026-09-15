using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnapNetClient;

public class Client : IDisposable
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

	private TcpClient _tcpClient;

	private NetworkStream _stream;

	private readonly byte[] _chunkBuffer = new byte[8192];

	private CancellationTokenSource _receiveCts;

	private bool _isRunning;

	private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

	private Timer _pingTimer;

	private readonly Dictionary<string, Action> _commandHandlers = new Dictionary<string, Action>();

	private bool _autoReconnect = true;

	private int _reconnectAttempts = 0;

	private const int MAX_RECONNECT_ATTEMPTS = 5;

	private const int MAX_MESSAGE_SIZE = 67108864;

	private const int CONNECT_TIMEOUT = 10000;

	private const int PING_INTERVAL = 15000;

	private const int RECEIVE_TIMEOUT = 30000;

	public string ServerIp { get; private set; }

	public int ServerPort { get; private set; }

	public string Nickname { get; private set; }

	public JobType Job { get; private set; }

	public bool IsConnected { get; private set; }

	public event Action<string> MessageReceived;

	public event Action Connected;

	public event Action Disconnected;

	public event Action<Exception> ConnectionFailed;

	public async Task ConnectAsync(string ipAddress, int port, string nickname, JobType job, CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			Disconnect();
			ServerIp = ipAddress;
			ServerPort = port;
			Nickname = nickname;
			Job = job;
			_tcpClient = new TcpClient
			{
				NoDelay = true
			};
			_receiveCts = new CancellationTokenSource();
			using CancellationTokenSource timeoutCts = new CancellationTokenSource(10000);
			using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
			await _tcpClient.ConnectAsync(ipAddress, port, linkedCts.Token);
			_stream = _tcpClient.GetStream();
			_isRunning = true;
			IsConnected = true;
			await SendMessageAsync($"{nickname}|{(int)job}");
			_pingTimer = new Timer(async delegate
			{
				await SendPingAsync();
			}, null, 15000, 15000);
			Connected?.Invoke();
			MessageReceived?.Invoke("Bağlantı başarılı: " + nickname);
			ReceiveMessagesAsync(_receiveCts.Token);
		}
		catch (Exception ex)
		{
			MessageReceived?.Invoke("Bağlantı hatası: " + ex.Message);
			ConnectionFailed?.Invoke(ex);
			if (!_autoReconnect || _reconnectAttempts >= 5)
			{
				Disconnect();
				throw;
			}
			await ReconnectWithBackoffAsync();
		}
	}

	private async Task SendPingAsync()
	{
		if (!IsConnected)
		{
			return;
		}
		try
		{
			await SendMessageAsync("PING");
		}
		catch (Exception ex)
		{
			MessageReceived?.Invoke("PING gönderme hatası: " + ex.Message);
			Disconnect();
		}
	}

	private async Task ReconnectWithBackoffAsync()
	{
		while (_reconnectAttempts < 5 && _autoReconnect)
		{
			_reconnectAttempts++;
			int delay = Math.Min(1000 * (int)Math.Pow(2.0, _reconnectAttempts), 30000);
			await Task.Delay(delay);
			try
			{
				await ConnectAsync(ServerIp, ServerPort, Nickname, Job);
				_reconnectAttempts = 0;
				return;
			}
			catch
			{
				MessageReceived?.Invoke($"Yeniden bağlanma hatası ({_reconnectAttempts}/{5})");
			}
		}
		if (_autoReconnect)
		{
			MessageReceived?.Invoke("Maksimum yeniden bağlanma denemesi aşıldı.");
			Disconnect();
		}
	}

	public void Disconnect()
	{
		if (!_isRunning)
		{
			return;
		}
		_isRunning = false;
		IsConnected = false;
		_receiveCts?.Cancel();
		_pingTimer?.Dispose();
		_pingTimer = null;
		try
		{
			_stream?.Close();
			_tcpClient?.Close();
		}
		catch (Exception ex)
		{
			MessageReceived?.Invoke("Bağlantı kapatılırken hata: " + ex.Message);
		}
		finally
		{
			_stream = null;
			_tcpClient = null;
			Disconnected?.Invoke();
		}
	}

	public async Task SendMessageAsync(string message)
	{
		if (!IsConnected)
		{
			throw new InvalidOperationException("Bağlantı yok");
		}
		byte[] data = Encoding.UTF8.GetBytes(message);
		byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
		await _sendLock.WaitAsync();
		try
		{
			await _stream.WriteAsync(lengthPrefix, 0, 4);
			await _stream.WriteAsync(data, 0, data.Length);
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			MessageReceived?.Invoke("Gönderme hatası: " + ex2.Message);
			Disconnect();
			throw;
		}
		finally
		{
			_sendLock.Release();
		}
	}

	public async Task SendCommandAsync(string command)
	{
		string message = $"{Nickname}|{(int)Job}|{command}";
		await SendMessageAsync(message);
	}

	public void RegisterCommand(string command, Action handler)
	{
		lock (_commandHandlers)
		{
			_commandHandlers[command] = handler;
		}
	}

	private async Task<string> ReadMessageAsync(CancellationToken token)
	{
		try
		{
			byte[] lengthBuffer = new byte[4];
			if (await _stream.ReadAsync(lengthBuffer, 0, 4, token) != 4)
			{
				return null;
			}
			int length = BitConverter.ToInt32(lengthBuffer, 0);
			if (length <= 0 || length > 67108864)
			{
				throw new InvalidDataException($"Geçersiz mesaj boyutu: {length}");
			}
			byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
			try
			{
				int read;
				for (int totalRead = 0; totalRead < length; totalRead += read)
				{
					int toRead = Math.Min(_chunkBuffer.Length, length - totalRead);
					read = await _stream.ReadAsync(_chunkBuffer, 0, toRead, token);
					if (read == 0)
					{
						return null;
					}
					Buffer.BlockCopy(_chunkBuffer, 0, buffer, totalRead, read);
				}
				return Encoding.UTF8.GetString(buffer, 0, length);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}
		catch (IOException)
		{
			return null;
		}
		catch (ObjectDisposedException)
		{
			return null;
		}
	}

	private async Task ReceiveMessagesAsync(CancellationToken token)
	{
		DateTime lastReceiveTime = DateTime.UtcNow;
		try
		{
			while (_isRunning && !token.IsCancellationRequested)
			{
				if ((DateTime.UtcNow - lastReceiveTime).TotalMilliseconds > 30000.0)
				{
					MessageReceived?.Invoke("Sunucu yanıt vermiyor, bağlantı kesiliyor");
					Disconnect();
					break;
				}
				string message = await ReadMessageAsync(token);
				if (message == null)
				{
					MessageReceived?.Invoke("Sunucu bağlantıyı kapattı");
					Disconnect();
					break;
				}
				lastReceiveTime = DateTime.UtcNow;
				if (message == "PING")
				{
					await SendMessageAsync("PONG");
				}
				else
				{
					ProcessMessage(message);
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			Exception ex3 = ex2;
			if (!token.IsCancellationRequested)
			{
				MessageReceived?.Invoke("Alım hatası: " + ex3.GetType().Name + " - " + ex3.Message);
				Disconnect();
			}
		}
		finally
		{
			MessageReceived?.Invoke("Alıcı döngüsü sonlandı.");
		}
	}

	private void ProcessMessage(string message)
	{
		try
		{
			MessageReceived?.Invoke("Ham mesaj: " + message);
			string[] array = message.Split('|');
			if (array.Length < 3)
			{
				MessageReceived?.Invoke("Geçersiz mesaj formatı");
				return;
			}
			string value = array[0].Trim();
			string value2 = array[1].Trim();
			string text = array[2].Trim();
			MessageReceived?.Invoke($"Parçalanmış: Nick={value}, Job={value2}, Komut={text}");
			lock (_commandHandlers)
			{
				if (_commandHandlers.TryGetValue(text, out Action value3))
				{
					MessageReceived?.Invoke("Komut '" + text + "' için handler bulundu");
					value3();
				}
				else
				{
					MessageReceived?.Invoke("Komut '" + text + "' için handler bulunamadı");
				}
			}
		}
		catch (Exception ex)
		{
			MessageReceived?.Invoke("İşleme hatası: " + ex.Message);
		}
	}

	public void Dispose()
	{
		_sendLock?.Dispose();
		_receiveCts?.Dispose();
		_pingTimer?.Dispose();
		Disconnect();
	}
}
