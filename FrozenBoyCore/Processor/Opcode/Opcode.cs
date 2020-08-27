using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

public delegate void Step();

namespace FrozenBoyCore.Processor {
    public class Opcode {
        public u8 value;
        public string label;
        public int length;
        public int tcycles;
        public int mcycles;
        public Step[] steps;

        public Opcode(u8 value, string label, int length, int tcycles, Step[] steps) {
            this.value = value;
            this.label = label;
            this.length = length;
            this.tcycles = tcycles;
            this.mcycles = tcycles / 4;
            this.steps = steps;
        }

    }
}
