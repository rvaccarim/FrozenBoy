using System.Collections.Generic;
using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;


namespace FrozenBoyCore.Processor {

    public class Timer {
        public const double ClockFrequency = 4194304;
        public const int DivFrequency = 16384;

        public Dictionary<int, int> TIMA_ClockCycles = new Dictionary<int, int> {
                { 0b00, (int) ClockFrequency / 4096 },
                { 0b01, (int) ClockFrequency / 262144 },
                { 0b10, (int) ClockFrequency / 65536},
                { 0b11, (int) ClockFrequency / 16384 } };

        // TODO
        public const int DivCycleFreq = (int)(ClockFrequency / DivFrequency);
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
            if (divCycles >= DivCycleFreq) {
                // divcycles = 0 is incorrect, because we might have exceeded DivCycleFreq
                divCycles -= DivCycleFreq;
                mmu.DIV++;
            }

            if (IsClockEnabled()) {
                timaCycles += cycles;
                int timaFreq = TIMA_ClockCycles[mmu.TAC & 0b_0000_0011];

                if (timaCycles >= timaFreq) {
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
