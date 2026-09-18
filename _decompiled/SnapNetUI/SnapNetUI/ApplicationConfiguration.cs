using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace SnapNetUI;

[CompilerGenerated]
internal static class ApplicationConfiguration
{
	public static void Initialize()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
#if !NET48
		Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
	}
}
