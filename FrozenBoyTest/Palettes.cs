using FrozenBoyCore.Graphics;

namespace FrozenBoyTest
{
    public class Palettes {
        public static GPU_Palette GetGreenPalette() {
            GPU_Color white = new(224, 248, 208, 255);
            GPU_Color lightGray = new(136, 192, 112, 255);
            GPU_Color darkGray = new(52, 104, 86, 255);
            GPU_Color black = new(8, 24, 32, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }

        public static GPU_Palette GetWhitePalette() {
            GPU_Color white = new(255, 255, 255, 255);
            GPU_Color lightGray = new(170, 170, 170, 255);
            GPU_Color darkGray = new(85, 85, 85, 255);
            GPU_Color black = new(0, 0, 0, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }
    }
}
