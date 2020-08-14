using System;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Diagnostics;


namespace FrozenBoyCore {

    public class CPU {
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

                if (opcode.value != 0xCB) {
                    opcode.logic.Invoke(memory, registers);
                }
                else {
                    u8 cbOpcodeValue = memory.data[registers.PC + 1];

                    if (Opcodes.prefixed.ContainsKey(cbOpcodeValue)) {
                        Opcode cb_opcode = Opcodes.prefixed[cbOpcodeValue];
                        cb_opcode.logic.Invoke(memory, registers);
                    }
                    else {
                        Debug.WriteLine(String.Format("Unsupported cb_opcode: {0:x2}", cbOpcodeValue));
                        System.Environment.Exit(0);
                    }
                }

                // move to the next one
                registers.PC = (u16)(registers.PC + opcode.length);
            }
            else {
                Debug.WriteLine(String.Format("Unsupported opcode: {0:x2}", opcodeValue));
                System.Environment.Exit(0);
            }
        }
    }
}
