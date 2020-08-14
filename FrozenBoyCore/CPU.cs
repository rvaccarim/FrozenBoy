using System;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Reflection.Emit;

namespace FrozenBoyCore {

    public class CPU {
        private const string dbgFormat = "{0,-15} ;${1,-6:x4} {2}";

        public Registers registers;
        public Memory memory;
        public Opcode opcode; // current Opcode

        public CPU(Memory memory) {
            registers = new Registers();
            this.memory = memory;
            registers.PC = 0;
        }

        public void Step() {
            u8 opcodeValue = memory.data[registers.PC];

            if (Opcodes.unprefixed.ContainsKey(opcodeValue)) {
                opcode = Opcodes.unprefixed[opcodeValue];
                opcode.logic.Invoke(memory, registers);

                Debug.WriteLine(DumpState());

                // move to the next one
                registers.PC = (u16)(registers.PC + opcode.length);
            }
            else {
                Debug.WriteLine(String.Format("Unsupported opcode: {0:x2}", opcodeValue));
                System.Environment.Exit(0);
            }
        }


        private string DumpState() {

            return opcode.length switch
            {
                2 => String.Format(dbgFormat,
                                   String.Format(opcode.asmInstruction,
                                                 registers.PC,
                                                 memory.data[registers.PC + 1]),
                                   registers.PC,
                                   registers.ToString()),

                3 => String.Format(dbgFormat,
                                   String.Format(opcode.asmInstruction,
                                                 registers.PC,
                                                 memory.data[registers.PC + 1],
                                                 memory.data[registers.PC + 2]),
                                   registers.PC,
                                   registers.ToString()),

                _ => String.Format(dbgFormat, opcode.asmInstruction, registers.PC, registers.ToString()),
            };
        }
    }
}
