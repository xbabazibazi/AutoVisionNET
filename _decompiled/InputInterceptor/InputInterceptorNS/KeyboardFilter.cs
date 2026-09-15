using System;

namespace InputInterceptorNS;

[Flags]
public enum KeyboardFilter : ushort
{
	None = 0,
	All = 0xFF,
	KeyDown = 1,
	KeyUp = 2,
	KeyE0 = 4,
	KeyE1 = 8,
	KeyTermsrvSetLED = 0x10,
	KeyTermsrvShadow = 0x20,
	KeyTermsrvVKPacket = 0x40
}
