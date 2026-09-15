using System;
using System.Runtime.InteropServices;

namespace InputInterceptorNS;

internal class NativeMethods
{
	private const string KERNEL32 = "kernel32.dll";

	private const string USER32 = "user32.dll";

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr LoadLibrary(string lpLibFileName);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FreeLibrary(IntPtr hLibModule);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr GetCurrentProcess();

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool GetCursorPos(out Win32Point lpPoint);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool SetCursorPos(int x, int y);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetSystemMetrics(int nIndex);
}
