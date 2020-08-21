using FrozenBoyCore.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore.Graphics {

    public class GPU {
        private int scanlineCounter;
        private MMU mmu;

        public GPU(MMU mmu) {
            this.mmu = mmu;
        }

        public void Update(int cycles) {
            scanlineCounter += cycles;
        }
    }
}
