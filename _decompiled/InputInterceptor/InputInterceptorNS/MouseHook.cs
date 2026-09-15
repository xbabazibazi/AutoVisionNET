using System;
using System.Threading;

namespace InputInterceptorNS;

public class MouseHook : Hook<MouseStroke>
{
	private const int SM_CXSCREEN = 0;

	private const int SM_CYSCREEN = 1;

	private static readonly int PrimaryScreenWidth;

	private static readonly int PrimaryScreenHeight;

	static MouseHook()
	{
		PrimaryScreenWidth = NativeMethods.GetSystemMetrics(0);
		PrimaryScreenHeight = NativeMethods.GetSystemMetrics(1);
	}

	public MouseHook(MouseFilter filter = MouseFilter.None, CallbackAction callback = null)
		: base((ushort)filter, (Predicate)InputInterceptor.IsMouse, callback)
	{
	}

	public MouseHook(CallbackAction callback)
		: base(ushort.MaxValue, (Predicate)InputInterceptor.IsMouse, callback)
	{
	}

	protected override void CallbackWrapper(ref Stroke stroke)
	{
		base.Callback(ref stroke.Mouse);
	}

	public bool SetMouseState(MouseState state, short rolling = 0)
	{
		if (base.CanSimulateInput)
		{
			Stroke stroke = new Stroke
			{
				Mouse = 
				{
					State = state,
					Rolling = rolling
				}
			};
			return InputInterceptor.Send(base.Context, base.AnyDevice, ref stroke, 1u) == 1;
		}
		return false;
	}

	public bool SimulateLeftButtonDown()
	{
		return SetMouseState(MouseState.LeftButtonDown, 0);
	}

	public bool SimulateLeftButtonUp()
	{
		return SetMouseState(MouseState.LeftButtonUp, 0);
	}

	public bool SimulateLeftButtonClick(int releaseDelay = 50)
	{
		if (SimulateLeftButtonDown())
		{
			Thread.Sleep(releaseDelay);
			return SimulateLeftButtonUp();
		}
		return false;
	}

	public bool SimulateMiddleButtonDown()
	{
		return SetMouseState(MouseState.MiddleButtonDown, 0);
	}

	public bool SimulateMiddleButtonUp()
	{
		return SetMouseState(MouseState.MiddleButtonUp, 0);
	}

	public bool SimulateMiddleButtonClick(int releaseDelay = 50)
	{
		if (SimulateMiddleButtonDown())
		{
			Thread.Sleep(releaseDelay);
			return SimulateMiddleButtonUp();
		}
		return false;
	}

	public bool SimulateRightButtonDown()
	{
		return SetMouseState(MouseState.RightButtonDown, 0);
	}

	public bool SimulateRightButtonUp()
	{
		return SetMouseState(MouseState.RightButtonUp, 0);
	}

	public bool SimulateRightButtonClick(int releaseDelay = 50)
	{
		if (SimulateRightButtonDown())
		{
			Thread.Sleep(releaseDelay);
			return SimulateRightButtonUp();
		}
		return false;
	}

	public bool SimulateScrollDown(short rolling = 120)
	{
		return SetMouseState(MouseState.ScrollVertical, (short)(-rolling));
	}

	public bool SimulateScrollUp(short rolling = 120)
	{
		return SetMouseState(MouseState.ScrollVertical, rolling);
	}

	public Win32Point GetCursorPosition()
	{
		NativeMethods.GetCursorPos(out var lpPoint);
		return lpPoint;
	}

	public bool SetCursorPosition(Win32Point point, bool useWinAPI = false)
	{
		return SetCursorPosition(point.X, point.Y, useWinAPI);
	}

	public bool SetCursorPosition(int x, int y, bool useWinAPI = false)
	{
		if (useWinAPI)
		{
			return NativeMethods.SetCursorPos(x, y);
		}
		if (base.CanSimulateInput)
		{
			Stroke stroke = new Stroke
			{
				Mouse = 
				{
					X = 65535 * x / (PrimaryScreenWidth - 1),
					Y = 65535 * y / (PrimaryScreenHeight - 1),
					Flags = MouseFlags.MoveAbsolute
				}
			};
			return InputInterceptor.Send(base.Context, base.AnyDevice, ref stroke, 1u) == 1;
		}
		return false;
	}

	public bool MoveCursorBy(int dX, int dY, bool useWinAPI = false)
	{
		if (useWinAPI)
		{
			Win32Point cursorPosition = GetCursorPosition();
			return NativeMethods.SetCursorPos(cursorPosition.X + dX, cursorPosition.Y + dY);
		}
		if (base.CanSimulateInput)
		{
			Stroke stroke = new Stroke
			{
				Mouse = 
				{
					X = dX,
					Y = dY,
					Flags = MouseFlags.MoveRelative
				}
			};
			return InputInterceptor.Send(base.Context, base.AnyDevice, ref stroke, 1u) == 1;
		}
		return false;
	}

	private bool SmoothMoveCursorBy(Win32Point startPosition, int dX, int dY, int speed = 15, bool useWinAPI = false)
	{
		if (!base.CanSimulateInput)
		{
			return false;
		}
		if (dX == 0 && dY == 0)
		{
			return true;
		}
		if (Math.Abs(dX) >= Math.Abs(dY))
		{
			double num = (double)dY / (double)dX;
			int i = 0;
			for (int num2 = Math.Abs(dX / speed); i < num2; i++)
			{
				int x = startPosition.X + i * dX / num2;
				int y = (int)((double)startPosition.Y + (double)(i * dX / num2) * num);
				if (!SetCursorPosition(x, y, useWinAPI))
				{
					return false;
				}
				Thread.Sleep(10);
			}
		}
		else
		{
			double num3 = (double)dX / (double)dY;
			int j = 0;
			for (int num4 = Math.Abs(dY / speed); j < num4; j++)
			{
				int x2 = (int)((double)startPosition.X + (double)(j * dY / num4) * num3);
				int y2 = startPosition.Y + j * dY / num4;
				if (!SetCursorPosition(x2, y2, useWinAPI))
				{
					return false;
				}
				Thread.Sleep(10);
			}
		}
		if (!SetCursorPosition(startPosition.X + dX, startPosition.Y + dY, useWinAPI))
		{
			return false;
		}
		return true;
	}

	public bool SimulateMoveTo(Win32Point point, int speed = 15, bool useWinAPI = false)
	{
		return SimulateMoveTo(point.X, point.Y, speed, useWinAPI);
	}

	public bool SimulateMoveTo(int x, int y, int speed = 15, bool useWinAPI = false)
	{
		if (!base.CanSimulateInput)
		{
			return false;
		}
		Win32Point cursorPosition = GetCursorPosition();
		int dX = x - cursorPosition.X;
		int dY = y - cursorPosition.Y;
		return SmoothMoveCursorBy(cursorPosition, dX, dY, speed, useWinAPI);
	}

	public bool SimulateMoveBy(int dX, int dY, int speed = 15, bool useWinAPI = false)
	{
		if (!base.CanSimulateInput)
		{
			return false;
		}
		Win32Point cursorPosition = GetCursorPosition();
		return SmoothMoveCursorBy(cursorPosition, dX, dY, speed, useWinAPI);
	}
}
