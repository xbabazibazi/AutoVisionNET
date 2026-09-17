using System.Drawing;
using System.Windows.Forms;

namespace UI2.ScreenCapture;

public class SimpleTextPromptForm : Form
{
	private readonly TextBox _textBox;

	public string ResultText => _textBox.Text;

	public SimpleTextPromptForm(string title, string label, string defaultValue)
	{
		Text = title;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		MaximizeBox = false;
		TopMost = true;
		ClientSize = new Size(320, 110);

		Label lbl = new Label
		{
			Text = label,
			AutoSize = true,
			Location = new Point(10, 12)
		};
		_textBox = new TextBox
		{
			Location = new Point(10, 35),
			Width = 300,
			Text = defaultValue ?? string.Empty
		};
		Button btnOk = new Button
		{
			Text = "Tamam",
			DialogResult = DialogResult.OK,
			Location = new Point(150, 70),
			Width = 75
		};
		Button btnCancel = new Button
		{
			Text = "İptal",
			DialogResult = DialogResult.Cancel,
			Location = new Point(235, 70),
			Width = 75
		};
		Controls.Add(lbl);
		Controls.Add(_textBox);
		Controls.Add(btnOk);
		Controls.Add(btnCancel);
		AcceptButton = btnOk;
		CancelButton = btnCancel;
	}
}
