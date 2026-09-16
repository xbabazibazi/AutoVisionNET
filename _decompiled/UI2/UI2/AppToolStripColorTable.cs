using System.Drawing;
using System.Windows.Forms;

namespace UI2;

// Uygulamanin karanlik temasina ve ikonundaki camgobegi vurguya uyan
// ozel bir ToolStrip renk paleti. Varsayilan "System" render modu
// Windows'un acik mavi/gri hover vurgusunu kullaniyordu; bu, koyu
// temayla celisiyordu.
internal sealed class AppToolStripColorTable : ProfessionalColorTable
{
	private static readonly Color Base = Color.FromArgb(40, 40, 45);
	private static readonly Color Hover = Color.FromArgb(55, 78, 92);
	private static readonly Color HoverBorder = Color.FromArgb(90, 190, 210);
	private static readonly Color Pressed = Color.FromArgb(40, 95, 110);
	private static readonly Color MenuBg = Color.FromArgb(28, 32, 43);
	private static readonly Color MenuBorderColor = Color.FromArgb(70, 80, 100);

	public override Color ToolStripGradientBegin => Base;
	public override Color ToolStripGradientMiddle => Base;
	public override Color ToolStripGradientEnd => Base;

	public override Color ButtonSelectedHighlight => Hover;
	public override Color ButtonSelectedHighlightBorder => HoverBorder;
	public override Color ButtonSelectedGradientBegin => Hover;
	public override Color ButtonSelectedGradientMiddle => Hover;
	public override Color ButtonSelectedGradientEnd => Hover;

	public override Color ButtonPressedHighlight => Pressed;
	public override Color ButtonPressedHighlightBorder => HoverBorder;
	public override Color ButtonPressedGradientBegin => Pressed;
	public override Color ButtonPressedGradientMiddle => Pressed;
	public override Color ButtonPressedGradientEnd => Pressed;

	public override Color MenuItemSelected => Hover;
	public override Color MenuItemSelectedGradientBegin => Hover;
	public override Color MenuItemSelectedGradientEnd => Hover;
	public override Color MenuItemBorder => HoverBorder;
	public override Color MenuItemPressedGradientBegin => Pressed;
	public override Color MenuItemPressedGradientEnd => Pressed;

	public override Color MenuStripGradientBegin => MenuBg;
	public override Color MenuStripGradientEnd => MenuBg;
	public override Color ImageMarginGradientBegin => MenuBg;
	public override Color ImageMarginGradientMiddle => MenuBg;
	public override Color ImageMarginGradientEnd => MenuBg;
	public override Color ToolStripDropDownBackground => MenuBg;
	public override Color MenuBorder => MenuBorderColor;
	public override Color SeparatorDark => MenuBorderColor;
	public override Color SeparatorLight => Base;
	public override Color ToolStripBorder => MenuBorderColor;
}
