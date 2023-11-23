using System.Collections.Generic;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Processor
{

    // for the IF register
    public enum InterruptionType : int {
        VBlank = 0,
        LCD = 1,
        Timer = 2,
        SerialLink = 3,
        Joypad = 4
    };

    public class InterruptManager {

        // Interrupt Master Enable Register, it's a master switch for all interruptions
        public bool IME;
        public bool IME_EnableScheduled = false;
        public bool IME_DisableScheduled = false;

        public int pendingEnable = -1;
        public int pendingDisable = -1;

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

        // 0xFFFF               
        public u8 IE { get; set; }
        // 0xFF0F
        private u8 _IF;
        public u8 IF { get => _IF; set => _IF = value |= 0xE0; }

        // ISR addresses
        public List<u16> ISR_Address = new()
        {
                { 0x0040 },    // Vblank
                { 0x0048 },    // LCD Status
                { 0x0050 },    // TimerOverflow
                { 0x0058 },    // SerialLink
                { 0x0060 } };  // JoypadPress,

        public bool IsInterruptRequested() {
            return (IE & IF) != 0;
        }

        public void RequestInterruption(InterruptionType interruption) {
            _IF |= (u8)(1 << (int)interruption);
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

        //public void EnableInterrupts() {
        //    IME = true;
        //    IME_EnableScheduled = false;
        //}

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
