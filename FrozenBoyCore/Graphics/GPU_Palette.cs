using System.Collections.Generic;

namespace FrozenBoyCore.Graphics
{
    public class GPU_Palette(GPU_Color white, GPU_Color lightGray, GPU_Color darkGray, GPU_Color black)
    {
        public List<GPU_Color> colors = [white, lightGray, darkGray, black ];
    }
}
