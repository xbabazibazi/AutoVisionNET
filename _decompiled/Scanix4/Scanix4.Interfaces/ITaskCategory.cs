using System.Collections.Generic;
using Scanix4.Models;

namespace Scanix4.Interfaces;

public interface ITaskCategory
{
	List<SearchTask> Tasks { get; }
}
