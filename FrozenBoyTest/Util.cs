using FrozenBoyCore.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyTest {
    public class Util {
        public GPU_Palette GetTestPalette() {
            GPU_Color white = new GPU_Color(255, 255, 255, 255);
            GPU_Color lightGray = new GPU_Color(170, 170, 170, 255);
            GPU_Color darkGray = new GPU_Color(85, 85, 85, 255);
            GPU_Color black = new GPU_Color(0, 0, 0, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }
    }
}
