using System.IO;
using System.Reflection;

namespace InputInterceptorNS;

internal static class Helpers
{
	public static byte[] GetResource(string name)
	{
		TypeInfo typeInfo = typeof(Helpers).GetTypeInfo();
		Assembly assembly = typeInfo.Assembly;
		string name2 = typeInfo.Namespace + ".Resources." + name;
		using Stream stream = assembly.GetManifestResourceStream(name2);
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, (int)stream.Length);
		return array;
	}
}
