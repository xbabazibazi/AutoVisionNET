using System;

namespace InputInterceptorNS;

[Flags]
public enum KeyState : ushort
{
	Down = 0,
	Up = 1,
	E0 = 2,
	E1 = 4,
	TermsrvSetLED = 8,
	TermsrvShadow = 0x10,
	TermsrvVKPacket = 0x20
}
