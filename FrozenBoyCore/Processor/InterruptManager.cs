using FrozenBoyCore.Memory;
using System.Collections.Generic;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Processor {
    public class InterruptManager {
        // Interrupt Master Enable Register, it's a master switch for all interruptions
        public bool IME;
        public bool IME_EnableScheduled = false;
        public bool IME_DisableScheduled = false;

        public int pendingEnable = -1;
        public int pendingDisable = -1;


        // for the IF register
        private const int VBLANK_BITPOS = 0;
        private const int LCD_BITPOS = 1;
        private const int TIMER_BITPOS = 2;
        private const int LY_EQUALS_LYC_BITPOS = 6;

        private u8 _IF;

        // INTERRUPTION REGISTERS
        // IE = granular interrupt enabler. When bits are set, the corresponding interrupt can be triggered
        // IF = When bits are set, an interrupt has happened
        // They use the same bit positions
        // 
        // Bit 
        // 0   Vblank 
        // 1   LCD
        // 2   Timer 
        // 3   Serial Link 
        // 4   Joypad 
        public u8 IE { get; set; }
        public u8 IF { get => _IF; set => _IF = value |= 0xE0; }

        // ISR addresses
        public List<u16> ISR_Address = new List<u16> {
                { 0x0040 },    // Vblank
                { 0x0048 },    // LCD Status
                { 0x0050 },    // TimerOverflow
                { 0x0058 },    // SerialLink
                { 0x0060 } };  // JoypadPress,

        public void RequestTimer() {
            _IF |= (byte)(1 << TIMER_BITPOS);
        }

        public void RequestVBlank() {
            _IF |= (byte)(1 << VBLANK_BITPOS);
        }

        public void RequestLCD() {
            _IF |= (byte)(1 << LCD_BITPOS);
        }

        public void Request_LY_Equals_LYC() {
            _IF |= (byte)(1 << LY_EQUALS_LYC_BITPOS);
        }

        public bool IsInterruptRequested() {
            return (IE & IF) != 0;
        }

        public bool IsHaltBug() => (IE & IF & 0x1f) != 0 && !IME;

        public int GetEnabledInterrupt() {
            int interruptBit = -1;

            for (int bitPos = 0; bitPos < 5; bitPos++) {
                if ((((IE & IF) >> bitPos) & 0x1) == 1) {
                    interruptBit = bitPos;
                    break;
                }
            }
            return interruptBit;
        }


        public void EnableInterrupts() {
            IME = true;
            IME_EnableScheduled = false;
        }

        public void DisableInterrupts() {
            pendingEnable = -1;
            pendingDisable = -1;
            IME = false;
        }

        public void OnInstructionFinished() {
            if (pendingEnable != -1) {
                if (pendingEnable-- == 0) {
                    EnableInterrupts(false);
                }
            }

            if (pendingDisable != -1) {
                if (pendingDisable-- == 0) {
                    DisableInterrupts();
                }
            }
        }

        public void EnableInterrupts(bool withDelay) {
            pendingDisable = -1;

            if (withDelay) {
                if (pendingEnable == -1) {
                    pendingEnable = 1;
                }
            }
            else {
                pendingEnable = -1;
                IME = true;
            }
        }

    }
}
