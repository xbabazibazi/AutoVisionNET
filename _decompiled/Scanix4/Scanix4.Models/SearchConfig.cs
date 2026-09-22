using System;
using System.Drawing;

namespace Scanix4.Models;

public class SearchConfig
{
	private string _templatePath;

	public string TemplatePath
	{
		get
		{
			return _templatePath;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("TemplatePath boş olamaz.");
			}
			_templatePath = value;
		}
	}

	public Rectangle SearchArea { get; set; } = new Rectangle(0, 0, 100, 100);

	public double Threshold { get; set; } = 0.8;

	public int IntervalMs { get; set; } = 500;

	public bool UseColor { get; set; } = false;

	public Action<Point>? OnMatchFound { get; set; } = null;

	public Action? OnMatchNotFound { get; set; } = null;

	public Action<int>? OnFoundCount { get; set; } = null;

	/// <summary>
	/// By default a count of 0 is treated as "not found" and <see cref="OnFoundCount"/> is skipped.
	/// For a task where zero is itself meaningful - e.g. the inventory alert, where 0 empty slots
	/// means the bag is completely full - set this so the zero reading is reported as well.
	/// </summary>
	public bool ReportZeroCount { get; set; } = false;

	public MatchMode Mode { get; set; } = MatchMode.SingleMatch;
}
