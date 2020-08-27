using System;
using u8 = System.Byte;
using u16 = System.UInt16;


namespace FrozenBoyCore.Processor {
    public class Disassembler {

        private const string opcodeFormat = "{0,-15} ;${1,-6:x4} O=0x{2:x2}";

        public static string OpcodeToStr(CPU cpu, Opcode o, int address) {
            return o.length switch
            {
                2 => String.Format(opcodeFormat,
                                   String.Format(o.label, cpu.mmu.Read8((u16)(address + 1))),
                                   address,
                                   o.value),

                3 => String.Format(opcodeFormat,
                                   String.Format(o.label, cpu.mmu.Read16((u16)(address + 1))),
                                   address,
                                   o.value),

                _ => String.Format(opcodeFormat,
                                   String.Format(o.value != 0xCB ? o.label
                                                                 : cpu.cbHandler.cbOpcodes[cpu.mmu.Read8((u16)(address + 1))].label),
                                   address,
                                   o.value),
            };
        }


    }
}
