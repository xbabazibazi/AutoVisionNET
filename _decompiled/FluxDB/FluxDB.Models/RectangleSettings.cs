using System.Drawing;

namespace FluxDB.Models;

public class RectangleSettings
{
	public int ID { get; set; }

	public string Name { get; set; } = string.Empty;

	public int CoordinateX { get; set; }

	public int CoordinateY { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public Rectangle GetRectangle()
	{
		return new Rectangle(CoordinateX, CoordinateY, Width, Height);
	}
}
