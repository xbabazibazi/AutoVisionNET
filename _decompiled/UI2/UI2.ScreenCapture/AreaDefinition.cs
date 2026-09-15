using System.Windows.Forms;

namespace UI2.ScreenCapture;

public class AreaDefinition
{
	public string SettingName { get; }

	public Button Button { get; }

	public Label Label { get; }

	public string DisplayName { get; }

	public AreaDefinition(string settingName, Button button, Label label, string displayName)
	{
		SettingName = settingName;
		Button = button;
		Label = label;
		DisplayName = displayName;
	}
}
