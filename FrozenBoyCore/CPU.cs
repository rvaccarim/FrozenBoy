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
        public Registers regs;
        public Memory memory;
        public Instruction currentInstruction;

        public CPU(Memory memory) {
            regs = new Registers();
            this.memory = memory;
            regs.PC = 0;
            currentInstruction = new Instruction(regs.PC, memory);
        }

        public void Next() {
            // execute instruction
            var opcodeSize = currentInstruction.Execute(this);
            // dump CPU state
            // Debug.WriteLine(this.ToString());
            // go to the next instruction
            regs.PC = (u16)(regs.PC + opcodeSize);
            currentInstruction = new Instruction(regs.PC, memory);
        }

        public string GetCurrentInstruction() {
            return currentInstruction.ToString();
        }

        public string GetState() {
            return regs.ToString();
        }

        public void Ret() {

        }

        public override string ToString() {
            return currentInstruction.ToString() + " " + regs.ToString();
        }
    }
}
