using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

public delegate void Function(CPU cpu, Instruction instruction);
public delegate int FunctionAlt(CPU cpu, Instruction instruction);

namespace FrozenBoyCore {
    public class Opcode {
        public u8 value;
        public string assembler;
        public int size;
        public Function function;

        public Opcode(u8 value, string assembler, int size, Function function) {
            this.value = value;
            this.assembler = assembler;
            this.size = size;
            this.function = function;
        }
    }
}
