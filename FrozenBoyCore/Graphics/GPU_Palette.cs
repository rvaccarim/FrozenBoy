using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore.Graphics {
    public class GPU_Palette {
        public List<GPU_Color> colors;

        public GPU_Palette(GPU_Color white, GPU_Color lightGray, GPU_Color darkGray, GPU_Color black) {
            colors = new List<GPU_Color> {
                white,
                lightGray,
                darkGray,
                black
            };
        }
    }
}
