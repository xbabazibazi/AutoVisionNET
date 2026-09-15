using System;
using System.Collections.Generic;
using System.Threading;

namespace InputInterceptorNS;

public abstract class Hook<TCallbackStroke> : IDisposable
{
	public delegate void CallbackAction(ref TCallbackStroke stroke);

	public IntPtr Context { get; private set; }

	public int Device { get; private set; }

	public int RandomDevice { get; private set; }

	public ushort FilterMode { get; private set; }

	public Predicate Predicate { get; private set; }

	public CallbackAction Callback { get; private set; }

	public Exception Exception { get; private set; }

	public bool Active { get; private set; }

	public Thread Thread { get; private set; }

	public bool IsInitialized
	{
		get
		{
			if (Context != IntPtr.Zero)
			{
				return Device != -1;
			}
			return false;
		}
	}

	public bool CanSimulateInput
	{
		get
		{
			if (Context != IntPtr.Zero)
			{
				if (Device == -1)
				{
					return RandomDevice != -1;
				}
				return true;
			}
			return false;
		}
	}

	public bool HasException => Exception != null;

	protected int AnyDevice
	{
		get
		{
			if (Device == -1)
			{
				return RandomDevice;
			}
			return Device;
		}
	}

	protected abstract void CallbackWrapper(ref Stroke stroke);

	public Hook(ushort filterMode, Predicate predicate, CallbackAction callback)
	{
		IntPtr context = InputInterceptor.CreateContext();
		List<DeviceData> deviceList = InputInterceptor.GetDeviceList(context, predicate);
		Context = context;
		Device = -1;
		RandomDevice = ((deviceList.Count > 0) ? deviceList[0].Device : (-1));
		FilterMode = filterMode;
		Predicate = predicate;
		Callback = callback;
		Exception = null;
		if (Context != IntPtr.Zero)
		{
			Active = filterMode != 0 || callback != null;
			Thread = new Thread(InterceptionMain);
			Thread.Priority = ((Callback != null) ? ThreadPriority.Highest : ThreadPriority.Normal);
			Thread.IsBackground = true;
			Thread.Start();
		}
		else
		{
			Active = false;
			Thread = null;
		}
	}

	private void InterceptionMain()
	{
		InputInterceptor.SetFilter(Context, Predicate, FilterMode);
		Stroke stroke = default(Stroke);
		while (Active)
		{
			int device = InputInterceptor.WaitWithTimeout(Context, 100uL);
			if (InputInterceptor.Receive(Context, device, ref stroke, 1u) <= 0)
			{
				continue;
			}
			Device = device;
			if (Active && Callback != null)
			{
				try
				{
					CallbackWrapper(ref stroke);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					Exception = ex;
					Active = false;
				}
			}
			InputInterceptor.Send(Context, device, ref stroke, 1u);
		}
	}

	public void Dispose()
	{
		if (Context != IntPtr.Zero)
		{
			if (Active)
			{
				Active = false;
				Thread.Join();
			}
			InputInterceptor.DestroyContext(Context);
			Context = IntPtr.Zero;
		}
	}
}
