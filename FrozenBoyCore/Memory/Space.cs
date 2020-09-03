using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Memory {
    public class Space {
        private readonly u8[] data;
        private readonly u16 from;
        private readonly u16 toInclusive;

        public Space(u16 from, u16 toInclusive) {
            this.from = from;
            this.toInclusive = toInclusive;

            data = new u8[toInclusive - from + 1];
        }

        public byte this[u16 address] {
            get {
                return data[address - from];
            }
            set {
                data[address - from] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Manages(u16 address) {
            if (address >= from && address <= toInclusive) {
                return true;
            }
            return false;
        }

    }
}
