namespace Scanix4.Models;

public class SearchTask
{
	public string TaskId { get; set; }

	public SearchConfig Config { get; set; }

	public SearchMode Mode { get; set; }
}
