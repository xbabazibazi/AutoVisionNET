using System;
using System.Windows.Forms;

namespace UI2.ScreenCapture;

public static class ControlExtensions
{
	public static void InvokeIfRequired<T>(this T control, Action<T> action) where T : Control
	{
		if (control.InvokeRequired)
		{
			control.Invoke(delegate
			{
				action(control);
			});
		}
		else
		{
			action(control);
		}
	}
}
