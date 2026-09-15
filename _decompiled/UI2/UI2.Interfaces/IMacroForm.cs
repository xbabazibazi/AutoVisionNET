using System.Windows.Forms;

namespace UI2.Interfaces;

public interface IMacroForm
{
	bool GetIsDisposed();

	void GetClose();

	Form GetForm();

	void ToggleControls(bool enabled);

	void SetEnabled(bool enabled);

	void GetHide();
}
