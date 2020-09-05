using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Util;
using FrozenBoyCore.Processor;

namespace FrozenBoyCore.Controls {
    public class Joypad {

        private readonly InterruptManager intManager;

        public bool[] JoypadKeys = new bool[8];
        private const int Right = 0;
        private const int Left = 1;
        private const int Up = 2;
        private const int Down = 3;
        private const int A = 4;
        private const int B = 5;
        private const int Select = 6;
        private const int Start = 7;

        private u8 _JOYP;

        // 0xFF00 
        // 0 = Pressed, 1=Unpressed; how creative 
        // Bit 7 - Not used
        // Bit 6 - Not used
        // Bit 5 - Select Button            R/W
        // Bit 4 - Select Direction         R/W
        // Bit 3 - Input Down or Start      (Read Only)
        // Bit 2 - Input Up or Select       (Read Only)
        // Bit 1 - Input Left or Button B   (Read Only)
        // Bit 0 - Input Right or Button A  (Read Only)
        public u8 JOYP {
            get {
                BuildState();
                return _JOYP;
            }
            set => _JOYP = value;
        }

        public Joypad(InterruptManager intManager) {
            this.intManager = intManager;
        }

        public void BuildState() {
            // these are set by the game to indicate which 4 bits they are interested in, see JoypadWiring.png
            // It's one or the other, not both at once
            bool directionQuery = BitUtils.IsBitSet(_JOYP, 4);
            bool buttonQuery = BitUtils.IsBitSet(_JOYP, 5);

            // we translate our array of pressed keys to the format
            // the game understands
            u8 newState = 0;
            u8 prevState = _JOYP;

            if (directionQuery || buttonQuery) {
                // very creative, 0 means pressed
                if (!directionQuery) {
                    newState = 0b_0000_1111;
                    if (JoypadKeys[Right]) newState = BitUtils.BitReset(newState, 0);
                    if (JoypadKeys[Left]) newState = BitUtils.BitReset(newState, 1);
                    if (JoypadKeys[Up]) newState = BitUtils.BitReset(newState, 2);
                    if (JoypadKeys[Down]) newState = BitUtils.BitReset(newState, 3);
                }
                else {
                    if (!buttonQuery) {
                        newState = 0b_0000_1111;
                        if (JoypadKeys[A]) newState = BitUtils.BitReset(newState, 0);
                        if (JoypadKeys[B]) newState = BitUtils.BitReset(newState, 1);
                        if (JoypadKeys[Select]) newState = BitUtils.BitReset(newState, 2);
                        if (JoypadKeys[Start]) newState = BitUtils.BitReset(newState, 3);
                    }
                }

                // if something went from unpressed (1) to pressed (0) we need to request
                // an interruption
                u8 lsb = (u8)(BitUtils.Lsb(newState) & ~BitUtils.Lsb(prevState));

                if (lsb != 0) {
                    intManager.RequestInterruption(InterruptionType.Joypad);
                }

                u8 tmp = (u8)(newState | (prevState & 0b_0011_0000));

                // Prevents the emulator from being detected as a Super Gameboy
                // https://www.reddit.com/r/EmuDev/comments/5bgcw1/gb_lcd_disableenable_behavior/
                if ((tmp & 0x30) == 0x10 || (tmp & 0x30) == 0x20) {
                    _JOYP = tmp;
                }
                else {
                    _JOYP = 0xff;
                }
            }
        }
    }
}
