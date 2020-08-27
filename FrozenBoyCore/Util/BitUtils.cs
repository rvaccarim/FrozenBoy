using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Util {
    public static class BitUtils {

        // Most significant byte - the ones to the right
        public static u8 Msb(u16 value) {
            return (u8)((value & 0b_11111111_00000000) >> 8);
        }

        // Least significant byte - the ones to the left
        public static u8 Lsb(u16 value) {
            return (u8)(value & 0b_00000000_11111111);
        }

        public static u16 ToUnsigned16(u8 msb, u8 lsb) {
            return (u16)((msb << 8) | lsb);
        }
    }
}
