using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnapNetUI;

public class CommandCard : Panel
{
	private Label lblCode;

	private Label lblDescription;

	private bool isSelected = false;

	public string CommandCode { get; }

	public bool IsSelected
	{
		get
		{
			return isSelected;
		}
		set
		{
			isSelected = value;
			UpdateSelectionState();
			SelectionChanged?.Invoke(isSelected);
		}
	}

	public event Action<bool> SelectionChanged;

	public CommandCard(string code, string description, Color color)
	{
		CommandCode = code;
		base.Height = 60;
		BackColor = Color.FromArgb(55, 55, 65);
		base.BorderStyle = BorderStyle.None;
		Cursor = Cursors.Hand;
		lblCode = new Label
		{
			Text = code,
			Font = new Font("Segoe UI", 12f, FontStyle.Bold),
			ForeColor = Color.White,
			BackColor = color,
			Dock = DockStyle.Left,
			Width = 70,
			TextAlign = ContentAlignment.MiddleCenter,
			Margin = new Padding(0)
		};
		lblDescription = new Label
		{
			Text = description,
			Font = new Font("Segoe UI", 10f),
			ForeColor = Color.FromArgb(220, 220, 240),
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Padding = new Padding(10, 0, 0, 0),
			Margin = new Padding(0)
		};
		Panel panel = new Panel
		{
			Dock = DockStyle.Right,
			Width = 5,
			BackColor = Color.Transparent,
			Margin = new Padding(0)
		};
		base.MouseEnter += delegate
		{
			OnMouseEnterCard();
		};
		base.MouseLeave += delegate
		{
			OnMouseLeaveCard();
		};
		lblCode.MouseEnter += delegate
		{
			OnMouseEnterCard();
		};
		lblDescription.MouseEnter += delegate
		{
			OnMouseEnterCard();
		};
		panel.MouseEnter += delegate
		{
			OnMouseEnterCard();
		};
		lblCode.MouseLeave += delegate
		{
			OnMouseLeaveCard();
		};
		lblDescription.MouseLeave += delegate
		{
			OnMouseLeaveCard();
		};
		panel.MouseLeave += delegate
		{
			OnMouseLeaveCard();
		};
		base.Controls.Add(panel);
		base.Controls.Add(lblDescription);
		base.Controls.Add(lblCode);
		base.Click += delegate
		{
			IsSelected = !IsSelected;
		};
		lblCode.Click += delegate
		{
			IsSelected = !IsSelected;
		};
		lblDescription.Click += delegate
		{
			IsSelected = !IsSelected;
		};
		panel.Click += delegate
		{
			IsSelected = !IsSelected;
		};
	}

	private void UpdateSelectionState()
	{
		base.Controls[0].BackColor = (isSelected ? Color.FromArgb(100, 180, 100) : Color.Transparent);
		BackColor = (isSelected ? Color.FromArgb(65, 65, 75) : Color.FromArgb(55, 55, 65));
	}

	private void OnMouseEnterCard()
	{
		if (!isSelected)
		{
			BackColor = Color.FromArgb(60, 60, 70);
		}
	}

	private void OnMouseLeaveCard()
	{
		if (!isSelected)
		{
			BackColor = Color.FromArgb(55, 55, 65);
		}
	}
}
