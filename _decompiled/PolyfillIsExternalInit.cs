// Polyfill for the 'init' accessor keyword when targeting net48, which doesn't ship this type.
// Referenced (via <Compile Include> Link) by any net48-multi-targeted project that uses 'init' properties.
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit
{
}
