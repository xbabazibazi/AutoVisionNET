using System.Runtime.InteropServices;

namespace InputInterceptorNS;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool Predicate(int device);
