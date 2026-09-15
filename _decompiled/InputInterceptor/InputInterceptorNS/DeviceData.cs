using System.Collections.Generic;

namespace InputInterceptorNS;

public class DeviceData
{
	public int Device;

	public string CompositeName;

	public List<string> Names;

	public DeviceData(int device, string rawCompositeName)
	{
		Device = device;
		CompositeName = string.Empty;
		Names = new List<string>();
		string[] array = rawCompositeName.Split(new char[1]);
		foreach (string text in array)
		{
			if (text.Length > 0)
			{
				CompositeName += text;
				Names.Add(text);
			}
		}
	}
}
