using FrozenBoyCore.Graphics;

namespace FrozenBoyCore {
    public class GameOptions(string romFilename, string romPath, GPU_Palette palette)
    {
        public string RomFilename { get; set; } = romFilename;
        public string RomPath { get; set; } = romPath;
        public GPU_Palette Palette { get; set; } = palette;
    }
}
