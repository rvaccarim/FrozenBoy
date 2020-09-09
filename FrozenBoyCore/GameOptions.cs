using FrozenBoyCore.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public class GameOptions {

        public string RomFilename { get; set; }
        public string RomPath { get; set; }
        public GPU_Palette Palette { get; set; }

        public GameOptions(string romFilename, string romPath, GPU_Palette palette) {
            this.RomFilename = romFilename;
            this.RomPath = romPath;
            this.Palette = palette;
        }
    }
}
