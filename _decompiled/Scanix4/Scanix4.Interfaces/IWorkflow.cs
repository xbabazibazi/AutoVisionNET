using System.Collections.Generic;
using Scanix4.Models;

namespace Scanix4.Interfaces;

public interface IWorkflow
{
	string WorkflowId { get; }

	bool IsActive { get; }

	Dictionary<string, WorkflowTransition> GetWorkflowSteps();
}
