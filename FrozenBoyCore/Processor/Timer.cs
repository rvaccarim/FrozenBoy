using System.Collections.Generic;
using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;


namespace FrozenBoyCore.Processor {

    public class Timer {
        public Dictionary<int, int> TIMA_Freq = new Dictionary<int, int> {
                { 0x00, 4096 },
                { 0x01, 262144 },
                { 0x10, 65536 },
                { 0x11, 16384 } };

        // TODO
        public const int DivCycleFreq = 256;
        private const int TimerEnabledBitPosition = 2;
        private const int TimerInterruptBitPosition = 2;

        private readonly MMU mmu;
        public int timaCycles;
        public int divCycles;

        public Timer(MMU mmu) {
            this.mmu = mmu;
        }

        public void Update(int cycles) {
            // The divider register increments at a fixed frequency (1 per 256 clock cycles). From 0 to 255.
            divCycles += cycles;
            while (divCycles >= DivCycleFreq) {
                // divcycles = 0 is incorrect, because we might have exceeded DivCycleFreq
                divCycles -= DivCycleFreq;
                mmu.DIV++;
            }

            if (IsClockEnabled()) {
                timaCycles += cycles;
                int timaFreq = TIMA_Freq[mmu.TAC & 0b_0000_0011];

                while (timaCycles >= timaFreq) {
                    // timaCycles = 0 is incorrect, because we might have exceeded timaFreq
                    timaCycles -= timaFreq;
                    mmu.TIMA++;
                }

                if (mmu.TIMA == 255) {
                    mmu.TIMA = mmu.TMA;
                    mmu.RequestInterrupt(TimerInterruptBitPosition);
                }
            }
        }

        public bool IsClockEnabled() {
            // check if bit 2 is 1
            return ((mmu.TAC >> TimerEnabledBitPosition) & 0b_0000_0001) == 1;
        }

    }
}
