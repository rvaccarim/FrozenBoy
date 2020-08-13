using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {
    public class Memory {
        // 0xFFFF = 65536
        public u8[] data = new u8[0xFFFF];
    }
}
