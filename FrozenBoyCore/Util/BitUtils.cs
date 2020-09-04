using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Runtime.CompilerServices;

namespace FrozenBoyCore.Util {
    public static class BitUtils {

        // Most significant byte - the ones to the right
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static u8 Msb(u16 value) {
            return (u8)((value & 0b_1111_1111_0000_0000) >> 8);
        }

        // Least significant byte - the ones to the left
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static u8 Lsb(u16 value) {
            return (u8)(value & 0b_0000_0000_1111_1111);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static u16 ToUnsigned16(u8 msb, u8 lsb) {
            return (u16)((msb << 8) | lsb);
        }

        // tests if bit is set
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(u8 value, int bitPosition) {
            return ((value >> bitPosition) & 0b_0000_0001) == 1;
        }

        // Reset bit in value
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static u8 BitReset(u8 value, int bitPosition) {
            return (u8)(value & ~(0b_0000_0001 << bitPosition));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static u8 BitSet(u8 value, int bitPosition) {
            return (u8)(value | (0b_0000_0001 << bitPosition));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static u8 ChangeBits(u8 val, u8 mask, u8 newvalue) {
            val &= (u8)~mask;
            val |= (u8)(newvalue & mask);
            return val;
        }

    }
}
