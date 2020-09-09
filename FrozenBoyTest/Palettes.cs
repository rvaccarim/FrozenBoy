using FrozenBoyCore.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyTest {
    public class Palettes {
        public GPU_Palette GetGreenPalette() {
            GPU_Color white = new GPU_Color(224, 248, 208, 255);
            GPU_Color lightGray = new GPU_Color(136, 192, 112, 255);
            GPU_Color darkGray = new GPU_Color(52, 104, 86, 255);
            GPU_Color black = new GPU_Color(8, 24, 32, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }

        public GPU_Palette GetWhitePalette() {
            var white = new GPU_Color(255, 255, 255, 255);
            var lightGray = new GPU_Color(170, 170, 170, 255);
            var darkGray = new GPU_Color(85, 85, 85, 255);
            var black = new GPU_Color(0, 0, 0, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }
    }
}
