using u8 = System.Byte;

namespace FrozenBoyCore.Graphics {
    public class GPU_Color(u8 red, u8 green, u8 blue, u8 alpha)
    {
        public u8 Red { get; set; } = red;
        public u8 Alpha { get; set; } = alpha;
        public u8 Green { get; set; } = green;
        public u8 Blue { get; set; } = blue;
    }
}
