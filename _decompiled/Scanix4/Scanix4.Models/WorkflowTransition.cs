namespace Scanix4.Models;

public class WorkflowTransition
{
	public string TaskId { get; set; }

	public string? NextStepOnMatch { get; set; }

	public string? NextStepOnNotMatch { get; set; }

	public WorkflowTransition(string taskId)
	{
		TaskId = taskId;
	}
}
