using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Diagnostics;

namespace FrozenBoyCore {
    public class Instruction {
        public u16 address;
        public Memory memory;
        public List<u8> operands;
        public Opcode opcode;

        private const string lineFormat = "${0,-6:x4} {1,-15}";

        public Instruction(u16 address, Memory memory) {
            this.address = address;
            this.memory = memory;

            u8 opcodeValue = memory.data[address];

            if (Opcodes.unprefixed.ContainsKey(opcodeValue)) {
                opcode = Opcodes.unprefixed[opcodeValue];
                u8 op1 = memory.data[address + 1];
                u8 op2 = memory.data[address + 2];
                operands = opcode.size == 1 ? new List<u8>() : opcode.size == 2 ? new List<u8> { op1 } : new List<u8> { op1, op2 };
            }
            else {
                Debug.WriteLine(String.Format("Unsupported opcode: {0:x2}", opcodeValue));
                System.Environment.Exit(0);
            }
        }

        public int Execute(CPU cpu) {
            opcode.function(cpu, this);
            return opcode.size;
        }

        public override string ToString() {
            switch (opcode.size) {
                case 2:
                    return String.Format(lineFormat, String.Format(opcode.assembler, address, operands[0])); ;
                case 3:
                    return String.Format(lineFormat, address, String.Format(opcode.assembler, operands[0], operands[1]));
                default:
                    return String.Format(lineFormat, address, opcode.assembler);
            }
        }
    }
}
