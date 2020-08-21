using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public class PPU {
        private int scanlineCounter;
        private MMU mmu;

        public PPU(MMU mmu) {
            this.mmu = mmu;
        }

        public void Update(int cycles) {
            scanlineCounter += cycles;
        }
    }
}
