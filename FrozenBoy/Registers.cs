using System;
using System.Collections.Generic;
using System.Text;

using u8 = System.Byte;
using u16 = System.UInt16;

#pragma warning disable IDE1006 // Naming Styles
namespace FrozenBoy {

    public class Registers {

        const int bitZeroPosition = 7;
        const int bitSubtractPosition = 6;
        const int bitHalfCarryPosition = 5;
        const int bitCarryPosition = 4;

        // 8 bit Real registers
        public u8 a { get; set; }
        public u8 b { get; set; }
        public u8 c { get; set; }
        public u8 d { get; set; }
        public u8 e { get; set; }
        public u8 f { get; set; }
        public u8 h { get; set; }
        public u8 l { get; set; }

        // 16 bit Virtual registers
        public u16 af {
            get {
                return (u16)((a << 8) | f);
            }

            set {
                a = (u8)((value & 0b_11111111_00000000) >> 8);
                f = (u8)(value & 0b_00000000_11111111);
            }
        }

        public u16 bc {
            get {
                return (u16)((b << 8) | c);
            }

            set {
                b = (u8)((value & 0b_11111111_00000000) >> 8);
                c = (u8)(value & 0b_00000000_11111111);
            }
        }

        public u16 de {
            get {
                return (u16)((d << 8) | e);
            }

            set {
                d = (u8)((value & 0b_11111111_00000000) >> 8);
                e = (u8)(value & 0b_00000000_11111111);
            }
        }

        public u16 hl {
            get {
                return (u16)((h << 8) | l);
            }

            set {
                h = (u8)((value & 0b_11111111_00000000) >> 8);
                l = (u8)(value & 0b_00000000_11111111);
            }
        }

        // F register flags
        public bool flagZero {
            get {
                // 11010000 & 10000000 = 10000000
                //                       10000000 != 0, so the bit is set
                return (f & (1 << bitZeroPosition)) != 0;
            }

            set {
                //        1 << bitZeroPosition = 10000000
                // newValue << bitZeroPosition = 00000000 (newValue = 0)
                //                          OR = 10000000
                //                         NOT = 01111111
                //                           f = 11110000
                //                         AND = 01110000
                var newBit = value ? 1 : 0;
                f = (u8)(f & ~(1 << bitZeroPosition) | (newBit << bitZeroPosition));
            }
        }

        public bool flagSubtract {
            get {
                return (f & (1 << bitSubtractPosition)) != 0;
            }

            set {
                var newBit = value ? 1 : 0;
                f = (u8)(f & ~(1 << bitSubtractPosition) | (newBit << bitSubtractPosition));
            }
        }

        public bool flagHalfCarry {
            get {
                return (f & (1 << bitHalfCarryPosition)) != 0;
            }

            set {
                var newBit = value ? 1 : 0;
                f = (u8)(f & ~(1 << bitHalfCarryPosition) | (newBit << bitHalfCarryPosition));
            }
        }

        public bool flagCarry {
            get {
                return (f & (1 << bitCarryPosition)) != 0;
            }

            set {
                var newBit = value ? 1 : 0;
                f = (u8)(f & ~(1 << bitCarryPosition) | (newBit << bitCarryPosition));
            }
        }

    }
}
#pragma warning restore IDE1006 // Naming Styles