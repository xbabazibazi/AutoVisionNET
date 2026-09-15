using System.Windows.Forms;
using SimpleLogger;

namespace UI2.Interfaces;

public interface ILogsForm
{
	Logger GetLogInstance();

	bool GetIsDisposed();

	void GetClose();

	Form GetForm();

	void GetHide();
}
