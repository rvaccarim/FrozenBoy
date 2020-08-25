using FrozenBoyCore.Memory;
using System.Collections.Generic;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Processor {
    public class InterruptManager {
        // Interrupt Master Enable Register, it's a master switch for all interruptions
        public bool IME;
        public bool IME_Scheduled = false;

        // for the IF register
        private const int VBLANK_BITPOS = 0;
        private const int LCD_BITPOS = 1;
        private const int TIMER_BITPOS = 2;

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

    }
}
