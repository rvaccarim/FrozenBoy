using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Memory {

    public class Dma {
        private MMU mmu;

        private bool transferInProgress;
        private bool transferRestarted;
        private int from;
        private int ticks;
        private u8 _DMA_Register = 0xff;

        public void SetMMU(MMU mmu) {
            this.mmu = mmu;
        }

        public bool IsOamBlocked() => transferRestarted || transferInProgress && ticks >= 5;

        public u8 DMA_Register {
            get {
                return _DMA_Register;
            }
            set {
                from = value * 0x100;
                transferRestarted = IsOamBlocked();
                ticks = 0;
                transferInProgress = true;
                _DMA_Register = value;
            }
        }

        public void Tick() {
            if (!transferInProgress) return;
            if (++ticks < 648) return;

            transferInProgress = false;
            transferRestarted = false;
            ticks = 0;

            for (var i = 0; i < 0xA0; i++) {
                mmu.Write8((u16)(0xFE00 + i), mmu.Read8((u16)(from + i)));
            }
        }

    }
}
