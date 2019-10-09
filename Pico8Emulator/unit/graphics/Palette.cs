using Microsoft.Xna.Framework;

namespace Pico8Emulator.unit.graphics {
	public static class Palette {
		public const int Size = 16;

		public static Color[] StandardPalette = {
			new Color(0, 0, 0),
			new Color(29, 43, 83),
			new Color(126, 37, 83),
			new Color(0, 135, 81),
			new Color(171, 82, 54),
			new Color(95, 87, 79),
			new Color(194, 195, 199),
			new Color(255, 241, 232),
			new Color(255, 0, 77),
			new Color(255, 163, 0),
			new Color(255, 236, 39),
			new Color(0, 228, 54),
			new Color(41, 173, 255),
			new Color(131, 118, 156),
			new Color(255, 119, 168),
			new Color(255, 204, 170)
		};

		public static Color[] AlternativePalette = {
			new Color(42, 24, 22),
			new Color(17, 29, 53),
			new Color(66, 33, 54),
			new Color(15, 84, 91),
			new Color(116, 47, 40),
			new Color(72, 50, 63),
			new Color(162, 136, 121),
			new Color(242, 239, 124),
			new Color(190, 17, 80),
			new Color(255, 109, 36),
			new Color(169, 231, 46),
			new Color(0, 181, 68),
			new Color(6, 89, 181),
			new Color(117, 70, 102),
			new Color(255, 110, 89),
			new Color(255, 157, 128)
		};
	}
}