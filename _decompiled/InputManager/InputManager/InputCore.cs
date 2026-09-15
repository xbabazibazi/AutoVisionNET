using System;
using InputInterceptorNS;

namespace InputManager;

public class InputCore : IDisposable
{
	private readonly Action<string> _showMessage;

	private readonly Action<KeyStroke>? _keyPressAction;

	private readonly Action<MouseStroke>? _mouseAction;

	private readonly Random _random = new Random();

	private bool _isDisposed;

	internal MouseHook? MouseHook { get; }

	internal KeyboardHook? KeyboardHook { get; }

	public InputCore(Action<string> showMessage, Action<KeyStroke>? keyPressAction = null, Action<MouseStroke>? mouseAction = null)
	{
		_showMessage = showMessage ?? throw new ArgumentNullException("showMessage");
		_keyPressAction = keyPressAction;
		_mouseAction = mouseAction;
		if (!InitializeDriver())
		{
			InstallDriver();
		}
		KeyboardHook = new KeyboardHook(KeyboardCallback);
		MouseHook = new MouseHook(MouseCallback);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				KeyboardHook?.Dispose();
				MouseHook?.Dispose();
			}
			_isDisposed = true;
		}
	}

	private bool InitializeDriver()
	{
		if (InputInterceptor.CheckDriverInstalled() && InputInterceptor.Initialize())
		{
			return true;
		}
		_showMessage("InputInterceptor başlatılamadı.");
		return false;
	}

	private void InstallDriver()
	{
		if (InputInterceptor.CheckAdministratorRights() && InputInterceptor.InstallDriver())
		{
			_showMessage("Sürücü başarıyla yüklendi.");
		}
		else
		{
			_showMessage("Sürücü yüklenemedi veya yönetici yetkileri gerekli.");
		}
	}

	public void SimulateKeyPress(KeyCode code, int delay = 20)
	{
		try
		{
			KeyboardHook?.SimulateKeyPress(code, delay);
		}
		catch (Exception ex) when (!(ex is OperationCanceledException))
		{
			_showMessage("Tuş basımı simülasyonu sırasında hata: " + ex.Message);
		}
	}

	public void SimulateLeftButtonClick(int delay = 50)
	{
		MouseHook?.SimulateLeftButtonClick(delay);
	}

	public void SimulateRightButtonClick(int delay = 50)
	{
		MouseHook?.SimulateRightButtonClick(delay);
	}

	public void SimulateMoveTo(int x, int y, int speed = 50)
	{
		MouseHook?.SimulateMoveTo(x, y, speed);
	}

	public void SimulateLeftButtonDown()
	{
		MouseHook?.SimulateLeftButtonDown();
	}

	public void SimulateLeftButtonUp()
	{
		MouseHook?.SimulateLeftButtonUp();
	}

	private void KeyboardCallback(ref KeyStroke keyStroke)
	{
		_keyPressAction?.Invoke(keyStroke);
	}

	private void MouseCallback(ref MouseStroke mouseStroke)
	{
		_mouseAction?.Invoke(mouseStroke);
	}
}
