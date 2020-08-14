using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {
    public class Memory {
        // 0xFFFF = 65536
        public u8[] data = new u8[0xFFFF];

        //public static u16 LitteEndian(u8 a, u8 b) {
        //    return (u16)(b << 8 | a);
        //}

        public u8 ReadNext8(int address) {
            return data[address + 1];
        }

        public u16 ReadNext16(int address) {
            u8 a = data[address + 1];
            u8 b = data[address + 2];

            return (u16)(b << 8 | a);
        }


    }

}
