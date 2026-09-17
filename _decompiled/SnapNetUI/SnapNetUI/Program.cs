using System;
using System.Windows.Forms;
using LicenseCore;

namespace SnapNetUI;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		if (!LicenseGate.EnsureLicensed("SnapNet Server"))
		{
			return;
		}
		Application.Run(new ServerForm());
	}
}
