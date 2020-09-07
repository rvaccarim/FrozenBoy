using u8 = System.Byte;

namespace FrozenBoyCore.Graphics {
    public class GPU_Color {
        public GPU_Color(u8 red, u8 green, u8 blue, u8 alpha) {
            Red = red;
            Alpha = alpha;
            Green = green;
            Blue = blue;
        }

        public u8 Red { get; set; }
        public u8 Alpha { get; set; }
        public u8 Green { get; set; }
        public u8 Blue { get; set; }
    }
}
