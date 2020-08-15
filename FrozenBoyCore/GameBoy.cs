using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrozenBoyCore {
    public class GameBoy {

        public Memory memory;
        public CPU cpu;

        public GameBoy() {
            memory = new Memory();

            byte[] romData = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\boot_rom.gb");

            int i = 0;
            foreach (byte b in romData) {
                memory.data[i] = b;
                i++;
            }

            cpu = new CPU(memory);
        }

    }
}
