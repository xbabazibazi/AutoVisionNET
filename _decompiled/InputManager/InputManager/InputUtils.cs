using System;
using System.Threading;
using System.Threading.Tasks;
using InputInterceptorNS;
using SettingsManager;
using SettingsManager.Macro;

namespace InputManager;

public class InputUtils(Action<string> showMessage, Action<KeyStroke>? keyPressAction = null, Action<MouseStroke>? mouseAction = null) : InputCore(showMessage, keyPressAction, mouseAction)
{
	private readonly Random _random = new Random();

	private readonly MacroSettings settings = Settings.Instance.Macro;

	private bool isUndyAcRunning;

	private CancellationTokenSource? cancellationTokenSourceUndyAc;

	public async Task StartLoginAsync()
	{
		await Login(settings.Login.UserID, settings.Login.UserPassword);
	}

	public async Task UndyAc()
	{
		if (!settings.UndyAc.UndyAcLoop)
		{
			return;
		}
		if (isUndyAcRunning)
		{
			cancellationTokenSourceUndyAc?.Cancel();
			cancellationTokenSourceUndyAc?.Dispose();
			cancellationTokenSourceUndyAc = null;
			isUndyAcRunning = false;
			return;
		}
		cancellationTokenSourceUndyAc = new CancellationTokenSource();
		isUndyAcRunning = true;
		try
		{
		}
		catch (TaskCanceledException)
		{
		}
		finally
		{
			isUndyAcRunning = false;
			cancellationTokenSourceUndyAc?.Dispose();
			cancellationTokenSourceUndyAc = null;
		}
	}

	private async Task Login(string username, string password)
	{
		base.KeyboardHook?.SimulateInput(username, 10, 10);
		await Task.Delay(200);
		SimulateKeyPress(KeyCode.Tab, 100);
		await Task.Delay(200);
		base.KeyboardHook?.SimulateInput(password, 10, 10);
		await Task.Delay(200);
		SimulateKeyPress(KeyCode.Enter);
	}

	public async Task IWill()
	{
		await Task.Delay(500);
		base.KeyboardHook?.SimulateInput("I WILL CONTINUE PLAYING KNIGHT ONLINE", 10, 10);
	}

	public async Task ILove()
	{
		await Task.Delay(500);
		base.KeyboardHook?.SimulateInput("I LOVE KNIGHT ONLINE", 10, 10);
	}

	public void WriteTowntoChat()
	{
		base.KeyboardHook?.SimulateKeyPress(KeyCode.Enter, 50);
		base.KeyboardHook?.SimulateKeyDown(KeyCode.Control);
		base.KeyboardHook?.SimulateKeyDown(KeyCode.Alt);
		base.KeyboardHook?.SimulateKeyDown(KeyCode.Three);
		base.KeyboardHook?.SimulateKeyUp(KeyCode.Three);
		base.KeyboardHook?.SimulateKeyUp(KeyCode.Alt);
		base.KeyboardHook?.SimulateKeyUp(KeyCode.Control);
		base.KeyboardHook?.SimulateInput("TOWN");
		base.KeyboardHook?.SimulateKeyPress(KeyCode.Enter, 50);
	}

	public async Task TPParty(int PartyMemberCount, int delay, bool isResist)
	{
		for (int i = 0; i < PartyMemberCount; i++)
		{
			SimulateKeyPress(KeyCode.F1, 50);
			await Task.Delay(100);
			SimulateKeyPress(KeyCode.Tab, 50);
			await Task.Delay(100);
			SimulateKeyPress(KeyCode.Three, delay);
			await Task.Delay(100);
			if (isResist)
			{
				SimulateKeyPress(KeyCode.Five, 50);
				await Task.Delay(1650);
			}
		}
	}

	public void ESC()
	{
		SimulateKeyPress(KeyCode.Eight, 50);
	}
}
