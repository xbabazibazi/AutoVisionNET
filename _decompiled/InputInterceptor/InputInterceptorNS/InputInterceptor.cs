using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace InputInterceptorNS;

public static class InputInterceptor
{
	private static DllWrapper DllWrapper;

	public static bool Initialized;

	public static bool Disposed => DllWrapper?.Disposed ?? true;

	public static IntPtr CreateContext()
	{
		return DllWrapper.CreateContext();
	}

	public static void DestroyContext(IntPtr context)
	{
		DllWrapper.DestroyContext(context);
	}

	public static int GetPrecedence(IntPtr context, int device)
	{
		return DllWrapper.GetPrecedence(context, device);
	}

	public static void SetPrecedence(IntPtr context, int device, int precedence)
	{
		DllWrapper.SetPrecedence(context, device, precedence);
	}

	public static ushort GetFilter(IntPtr context, int device)
	{
		return DllWrapper.GetFilter(context, device);
	}

	public static void SetFilter(IntPtr context, Predicate interception_predicate, KeyboardFilter filter)
	{
		DllWrapper.SetFilter(context, interception_predicate, (ushort)filter);
	}

	public static void SetFilter(IntPtr context, Predicate interception_predicate, MouseFilter filter)
	{
		DllWrapper.SetFilter(context, interception_predicate, (ushort)filter);
	}

	public static void SetFilter(IntPtr context, Predicate interception_predicate, ushort filter)
	{
		DllWrapper.SetFilter(context, interception_predicate, filter);
	}

	public static int Wait(IntPtr context)
	{
		return DllWrapper.Wait(context);
	}

	public static int WaitWithTimeout(IntPtr context, ulong milliseconds)
	{
		return DllWrapper.WaitWithTimeout(context, milliseconds);
	}

	public static int Send(IntPtr context, int device, ref Stroke stroke, uint nstroke)
	{
		return DllWrapper.Send(context, device, ref stroke, nstroke);
	}

	public static int Receive(IntPtr context, int device, ref Stroke stroke, uint nstroke)
	{
		return DllWrapper.Receive(context, device, ref stroke, nstroke);
	}

	public static uint GetHardwareId(IntPtr context, int device, IntPtr hardware_id_buffer, uint buffer_size)
	{
		return DllWrapper.GetHardwareId(context, device, hardware_id_buffer, buffer_size);
	}

	public static bool IsInvalid(int device)
	{
		return DllWrapper.IsInvalid(device) != 0;
	}

	public static bool IsKeyboard(int device)
	{
		return DllWrapper.IsKeyboard(device) != 0;
	}

	public static bool IsMouse(int device)
	{
		return DllWrapper.IsMouse(device) != 0;
	}

	static InputInterceptor()
	{
		Initialized = DllWrapper != null;
		DllWrapper = null;
	}

	public static bool Initialize()
	{
		if (Initialized)
		{
			return true;
		}
		try
		{
			DllWrapper = new DllWrapper(Helpers.GetResource("interception_x" + ((IntPtr.Size == 8) ? "64" : "86") + ".dll"));
			return true;
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			return false;
		}
	}

	public static bool Dispose()
	{
		if (Disposed)
		{
			return true;
		}
		try
		{
			DllWrapper.Dispose();
			DllWrapper = null;
			return true;
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			return false;
		}
	}

	public static bool CheckDriverInstalled()
	{
		RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Services");
		RegistryKey registryKey2 = registryKey.OpenSubKey("keyboard");
		RegistryKey registryKey3 = registryKey.OpenSubKey("mouse");
		if (registryKey2 == null || registryKey3 == null)
		{
			return false;
		}
		if ((string)registryKey2.GetValue("DisplayName", string.Empty) != "Keyboard Upper Filter Driver")
		{
			return false;
		}
		if ((string)registryKey3.GetValue("DisplayName", string.Empty) != "Mouse Upper Filter Driver")
		{
			return false;
		}
		return true;
	}

	public static bool CheckAdministratorRights()
	{
		return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
	}

	private static bool ExecuteInstaller(string arguments)
	{
		bool result = false;
		if (CheckAdministratorRights())
		{
			string tempFileName = Path.GetTempFileName();
			try
			{
				File.WriteAllBytes(tempFileName, Helpers.GetResource("install-interception.exe"));
				Process process = new Process();
				process.StartInfo.FileName = tempFileName;
				process.StartInfo.Arguments = arguments;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.Start();
				process.WaitForExit();
				result = process.ExitCode == 0;
				File.Delete(tempFileName);
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		}
		return result;
	}

	public static bool InstallDriver()
	{
		if (!CheckDriverInstalled())
		{
			return ExecuteInstaller("/install");
		}
		return false;
	}

	public static bool UninstallDriver()
	{
		if (CheckDriverInstalled())
		{
			return ExecuteInstaller("/uninstall");
		}
		return false;
	}

	public static List<DeviceData> GetDeviceList(Predicate predicate = null)
	{
		IntPtr context = CreateContext();
		List<DeviceData> deviceList = GetDeviceList(context, predicate);
		DestroyContext(context);
		return deviceList;
	}

	public static List<DeviceData> GetDeviceList(IntPtr context, Predicate predicate = null)
	{
		List<DeviceData> list = new List<DeviceData>();
		char[] array = new char[1024];
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		IntPtr hardware_id_buffer = gCHandle.AddrOfPinnedObject();
		for (int i = 1; i <= 20; i++)
		{
			if (predicate?.Invoke(i) ?? (!IsInvalid(i)))
			{
				uint hardwareId = GetHardwareId(context, i, hardware_id_buffer, (uint)array.Length);
				if (hardwareId != 0)
				{
					string rawCompositeName = new string(array, 0, (int)hardwareId);
					list.Add(new DeviceData(i, rawCompositeName));
				}
			}
		}
		gCHandle.Free();
		return list;
	}
}
