﻿using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

public delegate void Logic(Memory memory, Registers registers);

namespace FrozenBoyCore {
    public class Opcode {
        public u8 value;
        public string asmInstruction;
        public int length;
        //public int mcycles;
        public Logic logic;

        //public Opcode(u8 value, string asm, int length, int mcycles, Logic logic) {
        public Opcode(u8 value, string asm, int length, Logic logic) {
            this.value = value;
            this.asmInstruction = asm;
            this.length = length;
            //this.mcycles = mcycles;
            this.logic = logic;
        }

    }
}
