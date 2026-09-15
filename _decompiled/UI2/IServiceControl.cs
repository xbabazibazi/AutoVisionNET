public interface IServiceControl
{
	bool IsActive { get; }

	void SaveSettings();
}
