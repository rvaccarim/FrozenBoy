using FrozenBoyCore.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public class GameOptions {

        public string RomPath { get; set; }
        public GPU_Palette Palette { get; set; }

        public GameOptions(string romPath, GPU_Palette palette) {
            this.RomPath = romPath;
            this.Palette = palette;
        }
    }
}
