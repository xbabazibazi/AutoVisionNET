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
		bool freshInstall = false;
		if (!InitializeDriver())
		{
			InstallDriver();
			freshInstall = true;
			// InitializeDriver short-circuits before Initialize() when the driver is not registered
			// yet, so on a fresh machine DllWrapper is still null here. Retry now that the registry
			// entries exist: without this the hook constructors below dereference that null and take
			// the whole application down on the very first launch.
			if (!InitializeDriver())
			{
				// Every call site guards the hooks with ?., so leaving them null degrades input
				// simulation instead of crashing the process.
				_showMessage("Giriş sürücüsü başlatılamadı. EVOX'u yönetici olarak çalıştırıp tekrar deneyin; bu haliyle klavye ve fare simülasyonu çalışmaz.");
				return;
			}
		}
		KeyboardHook keyboardHook = new KeyboardHook(KeyboardCallback);
		MouseHook mouseHook = new MouseHook(MouseCallback);
		KeyboardHook = keyboardHook;
		MouseHook = mouseHook;
		// A filter driver only attaches to the keyboard/mouse device stacks at boot, so the context
		// stays empty until the machine restarts. Say so, instead of letting every macro fail silently.
		if (freshInstall && !(keyboardHook.CanSimulateInput && mouseHook.CanSimulateInput))
		{
			_showMessage("Giriş sürücüsü kuruldu ancak henüz etkin değil. Lütfen bilgisayarı yeniden başlatın; yeniden başlatmadan klavye ve fare simülasyonu çalışmaz.");
		}
	}

	/// <summary>Surfaces a message to the operator through whatever channel the host wired up.</summary>
	protected void ShowMessage(string message)
	{
		_showMessage(message);
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

	/// <summary>
	/// Probes for a usable driver. Deliberately silent: it is called twice during start-up and
	/// each failure is already followed by a more specific message from the constructor.
	/// </summary>
	private bool InitializeDriver()
	{
		return InputInterceptor.CheckDriverInstalled() && InputInterceptor.Initialize();
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
