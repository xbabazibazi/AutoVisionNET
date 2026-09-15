using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UI2;

public class FullScreenSnipForm : Form
{
	private Point startPoint;

	private Rectangle selectionRect;

	private bool isSelecting;

	private IContainer components = null;

	public Rectangle SelectedArea { get; private set; }

	public FullScreenSnipForm()
	{
		InitializeComponent();
		InitializeForm();
	}

	private void InitializeForm()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		base.WindowState = FormWindowState.Maximized;
		base.TopMost = true;
		DoubleBuffered = true;
		BackColor = Color.Black;
		base.Opacity = 0.3;
		Cursor = Cursors.Cross;
		base.MouseDown += OnMouseDown;
		base.MouseMove += OnMouseMove;
		base.MouseUp += OnMouseUp;
		base.KeyDown += OnKeyDown;
	}

	private void OnMouseDown(object sender, MouseEventArgs e)
	{
		startPoint = e.Location;
		selectionRect = new Rectangle(e.Location, Size.Empty);
		isSelecting = true;
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (isSelecting)
		{
			selectionRect = new Rectangle(Math.Min(startPoint.X, e.X), Math.Min(startPoint.Y, e.Y), Math.Abs(startPoint.X - e.X), Math.Abs(startPoint.Y - e.Y));
			Invalidate();
		}
	}

	private void OnMouseUp(object sender, MouseEventArgs e)
	{
		isSelecting = false;
		if (selectionRect.Width > 10 && selectionRect.Height > 10)
		{
			SelectedArea = selectionRect;
			base.DialogResult = DialogResult.OK;
		}
		else
		{
			base.DialogResult = DialogResult.Cancel;
		}
		Close();
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (!isSelecting)
		{
			return;
		}
		using SolidBrush brush = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
		using Pen pen = new Pen(Color.Red, 2f);
		e.Graphics.FillRectangle(brush, selectionRect);
		e.Graphics.DrawRectangle(pen, selectionRect);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(800, 450);
		this.Cursor = System.Windows.Forms.Cursors.Cross;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "FullScreenSnipForm";
		base.Opacity = 0.3;
		this.Text = "FullScreenSnipForm";
		base.TopMost = true;
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.ResumeLayout(false);
	}
}
