namespace UI2.ScreenCapture;

public class ServiceInfo
{
	public string DisplayName { get; set; }

	public string ServiceName { get; set; }

	public IServiceControl Control { get; set; }

	public bool IsActive => Control?.IsActive ?? false;
}
