using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UI2.ScreenCapture;

public class RectangleOverlayForm : Form
{
	private readonly Rectangle _targetRect;

	private readonly string _areaName;

	private readonly List<WeakReference> _hiddenForms = new List<WeakReference>();

	private IContainer components = null;

	public RectangleOverlayForm(Rectangle targetRect, string areaName)
	{
		_targetRect = targetRect;
		_areaName = areaName;
		InitializeForm();
		HideOtherForms();
	}

	private void InitializeForm()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		base.WindowState = FormWindowState.Maximized;
		base.TopMost = true;
		BackColor = Color.Magenta;
		base.TransparencyKey = Color.Magenta;
		DoubleBuffered = true;
		Cursor = Cursors.Default;
		base.KeyDown += delegate(object? s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				CloseForm();
			}
		};
		base.Click += delegate
		{
			CloseForm();
		};
	}

	private void HideOtherForms()
	{
		foreach (Form openForm in Application.OpenForms)
		{
			if (openForm.Visible && openForm != this)
			{
				_hiddenForms.Add(new WeakReference(openForm));
				openForm.Hide();
			}
		}
	}

	private void CloseForm()
	{
		RestoreForms();
		Close();
	}

	private void RestoreForms()
	{
		foreach (WeakReference hiddenForm in _hiddenForms)
		{
			if (hiddenForm.IsAlive && hiddenForm.Target is Form form)
			{
				try
				{
					form.Show();
					form.BringToFront();
				}
				catch
				{
				}
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		using (Pen pen = new Pen(Color.Red, 3f))
		{
			e.Graphics.DrawRectangle(pen, _targetRect);
		}
		using Font font = new Font("Arial", 12f, FontStyle.Bold);
		using SolidBrush brush = new SolidBrush(Color.White);
		using SolidBrush brush2 = new SolidBrush(Color.Black);
		string areaName = _areaName;
		SizeF sizeF = e.Graphics.MeasureString(areaName, font);
		Rectangle rect = new Rectangle(_targetRect.Left + 2, _targetRect.Top + 2, (int)sizeF.Width + 10, (int)sizeF.Height + 4);
		e.Graphics.FillRectangle(brush2, rect);
		e.Graphics.DrawString(areaName, font, brush, rect.Left + 5, rect.Top + 2);
	}

	protected override void Dispose(bool disposing)
	{
		RestoreForms();
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Name = "ShowRegionsForm";
		this.Text = "ShowRegionsForm";
	}
}
