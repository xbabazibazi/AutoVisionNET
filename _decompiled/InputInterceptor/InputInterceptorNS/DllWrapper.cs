using System;
using System.IO;
using System.Runtime.InteropServices;

namespace InputInterceptorNS;

internal class DllWrapper : IDisposable
{
	private readonly string DllTempName;

	private readonly IntPtr DllPointer;

	public readonly InterceptionMethods.CreateContext CreateContext;

	public readonly InterceptionMethods.DestroyContext DestroyContext;

	public readonly InterceptionMethods.GetPrecedence GetPrecedence;

	public readonly InterceptionMethods.SetPrecedence SetPrecedence;

	public readonly InterceptionMethods.GetFilter GetFilter;

	public readonly InterceptionMethods.SetFilter SetFilter;

	public readonly InterceptionMethods.Wait Wait;

	public readonly InterceptionMethods.WaitWithTimeout WaitWithTimeout;

	public readonly InterceptionMethods.Send Send;

	public readonly InterceptionMethods.Receive Receive;

	public readonly InterceptionMethods.GetHardwareId GetHardwareId;

	public readonly InterceptionMethods.IsInvalid IsInvalid;

	public readonly InterceptionMethods.IsKeyboard IsKeyboard;

	public readonly InterceptionMethods.IsMouse IsMouse;

	public bool Disposed;

	public DllWrapper(byte[] DllBytes)
	{
		DllTempName = Path.GetTempFileName();
		File.WriteAllBytes(DllTempName, DllBytes);
		DllPointer = NativeMethods.LoadLibrary(DllTempName);
		CreateContext = GetFunction<InterceptionMethods.CreateContext>("interception_create_context");
		DestroyContext = GetFunction<InterceptionMethods.DestroyContext>("interception_destroy_context");
		GetPrecedence = GetFunction<InterceptionMethods.GetPrecedence>("interception_get_precedence");
		SetPrecedence = GetFunction<InterceptionMethods.SetPrecedence>("interception_set_precedence");
		GetFilter = GetFunction<InterceptionMethods.GetFilter>("interception_get_filter");
		SetFilter = GetFunction<InterceptionMethods.SetFilter>("interception_set_filter");
		Wait = GetFunction<InterceptionMethods.Wait>("interception_wait");
		WaitWithTimeout = GetFunction<InterceptionMethods.WaitWithTimeout>("interception_wait_with_timeout");
		Send = GetFunction<InterceptionMethods.Send>("interception_send");
		Receive = GetFunction<InterceptionMethods.Receive>("interception_receive");
		GetHardwareId = GetFunction<InterceptionMethods.GetHardwareId>("interception_get_hardware_id");
		IsInvalid = GetFunction<InterceptionMethods.IsInvalid>("interception_is_invalid");
		IsKeyboard = GetFunction<InterceptionMethods.IsKeyboard>("interception_is_keyboard");
		IsMouse = GetFunction<InterceptionMethods.IsMouse>("interception_is_mouse");
		Disposed = false;
	}

	~DllWrapper()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			NativeMethods.FreeLibrary(DllPointer);
			File.Delete(DllTempName);
			Disposed = true;
		}
	}

	private TDelegate GetFunction<TDelegate>(string procedureName)
	{
		return Marshal.GetDelegateForFunctionPointer<TDelegate>(NativeMethods.GetProcAddress(DllPointer, procedureName));
	}
}
