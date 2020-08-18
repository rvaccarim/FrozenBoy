using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    // MMU = Memory Management Unit
    public class MMU {
        // 0xFFFF = 65536
        public u8[] data = new u8[0xFFFF];

        public u8 Read8(u16 address) {
            return data[address];
        }

        public u16 Read16(u16 address) {
            u8 a = data[address];
            u8 b = data[address + 1];
            return (u16)(b << 8 | a);
        }

        public void Write8(u16 address, u8 value) {
            data[address] = value;
        }

        public void Write16(u16 address, u16 value) {
            data[address + 1] = (u8)((value & 0b_11111111_00000000) >> 8);
            data[address] = (u8)(value & 0b_00000000_11111111);
        }

    }

}
