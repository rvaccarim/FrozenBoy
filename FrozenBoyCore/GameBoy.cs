using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrozenBoyCore {

    public class GameBoy {
        public MMU mmu;
        public CPU cpu;

        // constructor
        public GameBoy(string romName) {
            mmu = new MMU();
            byte[] romData = File.ReadAllBytes(romName);
            Buffer.BlockCopy(romData, 0, mmu.data, 0, romData.Length);

            cpu = new CPU(mmu);
        }

        public void Run() {
            int cpuCycles = 0;

            while (true) {
                cpuCycles = cpu.ExecuteNext();
            }
        }

    }
}
