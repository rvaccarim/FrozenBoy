using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {
    public class Memory {
        // 0xFFFF = 65536
        public u8[] data = new u8[0xFFFF];

        public u8 Read8(u16 address) {
            return data[address];
        }

        public u8 ReadParm8(u16 address) {
            return data[address + 1];
        }

        public u16 Read16(u16 address) {
            u8 a = data[address];
            u8 b = data[address + 1];
            return (u16)(b << 8 | a);
        }

        public u16 ReadParm16(u16 address) {
            u8 a = data[address + 1];
            u8 b = data[address + 2];
            return (u16)(b << 8 | a);
        }

        public void Write8(u16 address, u8 value) {
            data[address] = value;
        }

    }

}
