using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class InputBlocker : IDisposable
{
	private delegate nint LowLevelProc(int nCode, nint wParam, nint lParam);

	private static readonly LowLevelProc _keyboardProc = KeyboardHookCallback;

	private static readonly LowLevelProc _mouseProc = MouseHookCallback;

	private static nint _keyboardHookID = IntPtr.Zero;

	private static nint _mouseHookID = IntPtr.Zero;

	private const int WH_KEYBOARD_LL = 13;

	private const int WH_MOUSE_LL = 14;

	private const int WM_KEYDOWN = 256;

	private const int WM_MOUSEFIRST = 512;

	private const int WM_MOUSELAST = 525;

	private bool _disposed = false;

	private readonly object _lockObj = new object();

	public InputBlocker()
	{
		_keyboardHookID = SetHook(_keyboardProc, 13);
		_mouseHookID = SetHook(_mouseProc, 14);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		lock (_lockObj)
		{
			if (disposing)
			{
			}
			if (_keyboardHookID != IntPtr.Zero)
			{
				if (!UnhookWindowsHookEx(_keyboardHookID))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				_keyboardHookID = IntPtr.Zero;
			}
			if (_mouseHookID != IntPtr.Zero)
			{
				if (!UnhookWindowsHookEx(_mouseHookID))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				_mouseHookID = IntPtr.Zero;
			}
			_disposed = true;
		}
	}

	~InputBlocker()
	{
		Dispose(disposing: false);
	}

	private static nint SetHook(LowLevelProc proc, int hookType)
	{
		using Process process = Process.GetCurrentProcess();
		ProcessModule mainModule = process.MainModule;
		try
		{
			if (mainModule == null)
			{
				throw new InvalidOperationException("Current process module is null.");
			}
			nint num = SetWindowsHookEx(hookType, proc, GetModuleHandle(mainModule.ModuleName), 0u);
			if (num == IntPtr.Zero)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			return num;
		}
		finally
		{
			((IDisposable)mainModule)?.Dispose();
		}
	}

	private static nint KeyboardHookCallback(int nCode, nint wParam, nint lParam)
	{
		if (nCode >= 0 && wParam == 256)
		{
			return 1;
		}
		return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
	}

	private static nint MouseHookCallback(int nCode, nint wParam, nint lParam)
	{
		if (nCode >= 0 && (int)wParam >= 512 && (int)wParam <= 525)
		{
			return 1;
		}
		return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern nint SetWindowsHookEx(int idHook, LowLevelProc lpfn, nint hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(nint hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern nint GetModuleHandle(string lpModuleName);
}
