﻿using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    public class Registers {

        const int bitZeroPosition = 7;
        const int bitSubtractPosition = 6;
        const int bitHalfCarryPosition = 5;
        const int bitCarryPosition = 4;

        public u16 PC;
        public u16 SP;

        // 8 bit Real registers
        public u8 A;
        public u8 B;
        public u8 C;
        public u8 D;
        public u8 E;
        public u8 F;
        public u8 H;
        public u8 L;

        // 16 bit Virtual registers
        public u16 AF {
            get {
                return (u16)((A << 8) | F);
            }

            set {
                A = (u8)((value & 0b_11111111_00000000) >> 8);
                F = (u8)(value & 0b_00000000_11111111);
            }
        }

        public u16 BC {
            get {
                return (u16)((B << 8) | C);
            }

            set {
                B = (u8)((value & 0b_11111111_00000000) >> 8);
                C = (u8)(value & 0b_00000000_11111111);
            }
        }

        public u16 DE {
            get {
                return (u16)((D << 8) | E);
            }

            set {
                D = (u8)((value & 0b_11111111_00000000) >> 8);
                E = (u8)(value & 0b_00000000_11111111);
            }
        }

        public u16 HL {
            get {
                return (u16)((H << 8) | L);
            }

            set {
                H = (u8)((value & 0b_11111111_00000000) >> 8);
                L = (u8)(value & 0b_00000000_11111111);
            }
        }

        // F register flags
        // Z = Zero
        // N = Subtract
        // H = Half carry
        // C = Carry
        public bool FlagZ {
            get {
                // 11010000 & 10000000 = 10000000
                //                       10000000 != 0, so the bit is set
                return (F & (1 << bitZeroPosition)) != 0;
            }

            set {
                //        1 << bitZeroPosition = 10000000
                // newValue << bitZeroPosition = 00000000 (newValue = 0)
                //                          OR = 10000000
                //                         NOT = 01111111
                //                           f = 11110000
                //                         AND = 01110000
                var newBit = value ? 1 : 0;
                F = (u8)(F & ~(1 << bitZeroPosition) | (newBit << bitZeroPosition));
            }
        }

        public bool FlagN {
            get {
                return (F & (1 << bitSubtractPosition)) != 0;
            }

            set {
                var newBit = value ? 1 : 0;
                F = (u8)(F & ~(1 << bitSubtractPosition) | (newBit << bitSubtractPosition));
            }
        }

        public bool FlagH {
            get {
                return (F & (1 << bitHalfCarryPosition)) != 0;
            }

            set {
                var newBit = value ? 1 : 0;
                F = (u8)(F & ~(1 << bitHalfCarryPosition) | (newBit << bitHalfCarryPosition));
            }
        }

        public bool FlagC {
            get {
                return (F & (1 << bitCarryPosition)) != 0;
            }

            set {
                var newBit = value ? 1 : 0;
                F = (u8)(F & ~(1 << bitCarryPosition) | (newBit << bitCarryPosition));
            }
        }

        public override string ToString() {
            string formattedStr = "";
            formattedStr += String.Format(@"a={0:x2} b={1:x2} c={2:x2} d={3:x2} ", A, B, C, D);
            formattedStr += String.Format(@"e={0:x2} f={1:x2} h={2:x2} l={3:x2} ", E, F, H, L);
            formattedStr += String.Format(@"   Z={0} N={1} H={2} C={3}",
                Convert.ToInt32(FlagZ), Convert.ToInt32(FlagN), Convert.ToInt32(FlagH), Convert.ToInt32(FlagC));
            return formattedStr;
        }

    }
}
