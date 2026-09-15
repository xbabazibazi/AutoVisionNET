using System;

namespace InputInterceptorNS;

[Flags]
public enum MouseState : ushort
{
	LeftButtonDown = 1,
	LeftButtonUp = 2,
	RightButtonDown = 4,
	RightButtonUp = 8,
	MiddleButtonDown = 0x10,
	MiddleButtonUp = 0x20,
	ExtraButton1Down = 0x40,
	ExtraButton1Up = 0x80,
	ExtraButton2Down = 0x100,
	ExtraButton2Up = 0x200,
	ScrollVertical = 0x400,
	ScrollHorizontal = 0x800
}
