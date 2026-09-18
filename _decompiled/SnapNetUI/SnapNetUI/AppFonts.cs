using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SnapNetUI;

internal static class AppFonts
{
	private static readonly PrivateFontCollection _collection = Load();

	public static FontFamily HeaderFamily { get; } = _collection.Families.Length > 0 ? _collection.Families[0] : FontFamily.GenericSansSerif;

	public static Font Header(float size, FontStyle style = FontStyle.Bold)
	{
		return new Font(HeaderFamily, size, style);
	}

	private static PrivateFontCollection Load()
	{
		PrivateFontCollection collection = new PrivateFontCollection();
		try
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string resourceName = Array.Find(assembly.GetManifestResourceNames(), n => n.EndsWith("Rajdhani-SemiBold.ttf", StringComparison.OrdinalIgnoreCase));
			if (resourceName == null)
			{
				return collection;
			}
			using Stream stream = assembly.GetManifestResourceStream(resourceName);
			byte[] fontData = new byte[stream.Length];
			int totalRead = 0;
			while (totalRead < fontData.Length)
			{
				int read = stream.Read(fontData, totalRead, fontData.Length - totalRead);
				if (read == 0)
				{
					break;
				}
				totalRead += read;
			}
			IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
			Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
			collection.AddMemoryFont(fontPtr, fontData.Length);
			// fontPtr is intentionally never freed: PrivateFontCollection requires the
			// memory to remain valid for as long as the collection (and thus the app) is alive.
		}
		catch
		{
		}
		return collection;
	}
}
