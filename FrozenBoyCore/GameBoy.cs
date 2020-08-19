using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrozenBoyCore {
    public class GameBoy {
        public MMU mmu;
        public CPU cpu;

        public GameBoy(string romName) {
            mmu = new MMU();

            byte[] romData = File.ReadAllBytes(romName);

            int i = 0;
            foreach (byte b in romData) {
                mmu.data[i] = b;
                i++;
            }

            cpu = new CPU(mmu);
        }

    }
}
