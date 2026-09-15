using System;
using System.Windows.Forms;

namespace UI2;

internal static class FluxioUtiles
{
	public static decimal ToDecimalOrZero(this string value, decimal defaultValue = 0m)
	{
		decimal result;
		return decimal.TryParse(value, out result) ? result : defaultValue;
	}

	public static void InvokeIfRequired(this Control control, Action action)
	{
		if (control.InvokeRequired)
		{
			control.Invoke(action);
		}
		else
		{
			action();
		}
	}
}
