using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrozenBoyCore {
    public class GameBoy {

        public Memory memory;
        public CPU cpu;

        public GameBoy() {
            memory = new Memory {
                // data = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\boot_rom.gb")
                data = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\cpu_instrs.gb")
            };

            cpu = new CPU(memory);
        }
    }
}
