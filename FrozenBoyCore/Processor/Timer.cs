using FrozenBoyCore.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;


namespace FrozenBoyCore.Processor {
    public class Timer {

        private InterruptManager interruptManager;
        private static readonly int[] FreqToBit = { 9, 3, 5, 7 };

        // TODO: revoke public
        public int timerCounter = 0;
        private u8 tima;
        public u8 tac;
        public bool overflow;
        public int ticksSinceOverflow;
        public bool previousBit;

        public Timer(InterruptManager interruptManager) {
            this.interruptManager = interruptManager;
        }

        // The divider register increments at a fixed frequency
        // For some reason it's inside a 16 bit value (the 8 most significant bytes)
        public u8 DIV {
            get => (u8)(timerCounter >> 8);
            set => UpdateDiv(0);
        }

        //  The timer register increments at a configurable frequency and can provide an interrupt when it overflows.
        public u8 TIMA {
            get => tima;
            set {
                if (ticksSinceOverflow < 5) {
                    tima = value;
                    overflow = false;
                    ticksSinceOverflow = 0;
                };
            }
        }

        // When the TIMA overflows, after some cycles this data will be loaded.
        public byte TMA { get; set; }

        // Timer Control 
        // Bits 1-0 - Input Clock Select
        //            00: 4096   Hz 
        //            01: 262144 Hz
        //            10: 65536  Hz
        //            11: 16384  Hz
        // Bit  2   - Timer Enable
        //Note: The "Timer Enable" bit only affects the timer, the divider is ALWAYS counting.
        public u8 TAC { get => (u8)(tac | 0b11111000); set => tac = value; }

        public void Tick() {
            // 1111_1111_1111_1111 = 65535
            UpdateDiv((timerCounter + 1) & 65535);
            if (!overflow) {
                return;
            }

            ticksSinceOverflow++;
            if (ticksSinceOverflow == 4) {
                interruptManager.RequestInterruption(InterruptionType.Timer);
            }

            if (ticksSinceOverflow == 5) {
                tima = TMA;
            }

            if (ticksSinceOverflow == 6) {
                tima = TMA;
                overflow = false;
                ticksSinceOverflow = 0;
            }
        }

        private void UpdateDiv(int newTimerCounter) {
            timerCounter = newTimerCounter;

            int bitPos = FreqToBit[TAC & 0b11];
            //bitPos <<= _speedMode.GetSpeedMode() - 1;       
            bitPos <<= 0;

            bool bit = (timerCounter & (1 << bitPos)) != 0;
            bit &= (TAC & (1 << 2)) != 0;
            if (!bit && previousBit) {
                UpdateTima();
            }

            previousBit = bit;
        }

        private void UpdateTima() {
            tima++;
            int timaMod = tima % 256;

            if (timaMod == 0) {
                tima = 0;
                overflow = true;
                ticksSinceOverflow = 0;
            }
        }
    }
}
