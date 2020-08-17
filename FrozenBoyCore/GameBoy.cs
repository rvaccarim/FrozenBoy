using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrozenBoyCore {
    public class GameBoy {
        private const string romPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\";

        public Memory memory;
        public CPU cpu;

        public GameBoy() {
            memory = new Memory();
            byte[] romData = File.ReadAllBytes(romPath + @"blargg\cpu_instrs\individual\11-op a,(hl).gb");

            int i = 0;
            foreach (byte b in romData) {
                memory.data[i] = b;
                i++;
            }

            cpu = new CPU(memory);
        }

    }
}
