using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UI2;

public class FormManager
{
	private readonly Dictionary<string, Form> _forms;

	private readonly Form _mainForm;

	public FormManager(Form mainForm)
	{
		_mainForm = mainForm;
		_forms = new Dictionary<string, Form>();
	}

	public void RegisterForm(string key, Form form)
	{
		_forms[key] = form;
	}

	public void ShowForm(string key)
	{
		if (_forms.TryGetValue(key, out Form value))
		{
			HideAllForms();
			if (!value.Visible)
			{
				value.Location = CalculateAdjacentFormPosition(value);
				value.Show();
				value.BringToFront();
			}
		}
	}

	public void ToggleForm(string key)
	{
		if (_forms.TryGetValue(key, out Form value))
		{
			if (value.Visible)
			{
				value.Hide();
			}
			else
			{
				ShowForm(key);
			}
		}
	}

	public void HideAllForms()
	{
		foreach (Form value in _forms.Values)
		{
			if (value.Visible)
			{
				value.Hide();
			}
		}
	}

	public void CloseAllForms()
	{
		foreach (Form value in _forms.Values)
		{
			if (!value.IsDisposed)
			{
				value.Close();
			}
		}
	}

	private Point CalculateAdjacentFormPosition(Form formToOpen)
	{
		Point point = _mainForm.PointToScreen(Point.Empty);
		int num = point.X;
		int num2 = point.Y + _mainForm.Height;
		Rectangle? rectangle = Screen.PrimaryScreen?.WorkingArea;
		if (rectangle.HasValue)
		{
			Rectangle value = rectangle.Value;
			if (num2 + formToOpen.Height > value.Bottom)
			{
				num2 = point.Y - formToOpen.Height;
			}
			num2 = Math.Max(value.Top, Math.Min(num2, value.Bottom - formToOpen.Height));
			num = Math.Max(value.Left, Math.Min(num, value.Right - formToOpen.Width));
		}
		return new Point(num, num2);
	}
}
