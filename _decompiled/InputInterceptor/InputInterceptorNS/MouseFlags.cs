using System;

namespace InputInterceptorNS;

[Flags]
public enum MouseFlags : ushort
{
	MoveRelative = 0,
	MoveAbsolute = 1,
	VirtualDesktop = 2,
	AttributesChanged = 4,
	MoveWithoutCoalescing = 8,
	TerminalServicesSourceShadow = 0x100
}
