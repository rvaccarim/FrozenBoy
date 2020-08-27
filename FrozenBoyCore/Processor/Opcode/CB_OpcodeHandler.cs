using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Memory;

namespace FrozenBoyCore.Processor {
    public class CB_OpcodeHandler {
        private readonly Registers regs;
        private readonly MMU mmu;
        private u8 operand;

        public Dictionary<u8, Opcode> cbOpcodes;

        public CB_OpcodeHandler(Registers regs, MMU mmu) {
            this.regs = regs;
            this.mmu = mmu;

            cbOpcodes = InitializeCB();
        }

        private Dictionary<u8, Opcode> InitializeCB() {
            return new Dictionary<u8, Opcode> {              
                // rotate left (one position)
                { 0x00, new Opcode(0x00, "RLC B",                2,  8, new Step[] { () => { regs.B = RLC(regs.B); } })},
                { 0x01, new Opcode(0x01, "RLC C",                2,  8, new Step[] { () => { regs.C = RLC(regs.C); } })},
                { 0x02, new Opcode(0x02, "RLC D",                2,  8, new Step[] { () => { regs.D = RLC(regs.D); } })},
                { 0x03, new Opcode(0x03, "RLC E",                2,  8, new Step[] { () => { regs.E = RLC(regs.E); } })},
                { 0x04, new Opcode(0x04, "RLC H",                2,  8, new Step[] { () => { regs.H = RLC(regs.H); } })},
                { 0x05, new Opcode(0x05, "RLC L",                2,  8, new Step[] { () => { regs.L = RLC(regs.L); } })},
                { 0x07, new Opcode(0x07, "RLC A",                2,  8, new Step[] { () => { regs.A = RLC(regs.A); } })},
                { 0x06, new Opcode(0x06, "RLC (HL)",             2, 16, new Step[] {
                                                                             () => { operand = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, RLC(operand)); },
                                                                         })},
                // rotate right (one position)
                { 0x08, new Opcode(0x08, "RRC B",                2,  8, new Step[] { () => { regs.B = RRC(regs.B); } })},
                { 0x09, new Opcode(0x09, "RRC C",                2,  8, new Step[] { () => { regs.C = RRC(regs.C); } })},
                { 0x0A, new Opcode(0x0A, "RRC D",                2,  8, new Step[] { () => { regs.D = RRC(regs.D); } })},
                { 0x0B, new Opcode(0x0B, "RRC E",                2,  8, new Step[] { () => { regs.E = RRC(regs.E); } })},
                { 0x0C, new Opcode(0x0C, "RRC H",                2,  8, new Step[] { () => { regs.H = RRC(regs.H); } })},
                { 0x0D, new Opcode(0x0D, "RRC L",                2,  8, new Step[] { () => { regs.L = RRC(regs.L); } })},
                { 0x0F, new Opcode(0x0F, "RRC A",                2,  8, new Step[] { () => { regs.A = RRC(regs.A); } })},
                { 0x0E, new Opcode(0x0E, "RRC (HL)",             2, 16, new Step[] {
                                                                             () => { operand = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, RRC(operand)); },
                                                                         })},

                // Rotate n left through Carry flag
                { 0x10, new Opcode(0x10, "RL B",                 2,  8, new Step[] { () => { regs.B = RL(regs.B); } })},
                { 0x11, new Opcode(0x11, "RL C",                 2,  8, new Step[] { () => { regs.C = RL(regs.C); } })},
                { 0x12, new Opcode(0x12, "RL D",                 2,  8, new Step[] { () => { regs.D = RL(regs.D); } })},
                { 0x13, new Opcode(0x13, "RL E",                 2,  8, new Step[] { () => { regs.E = RL(regs.E); } })},
                { 0x14, new Opcode(0x14, "RL H",                 2,  8, new Step[] { () => { regs.H = RL(regs.H); } })},
                { 0x15, new Opcode(0x15, "RL L",                 2,  8, new Step[] { () => { regs.L = RL(regs.L); } })},
                { 0x17, new Opcode(0x17, "RL A",                 2,  8, new Step[] { () => { regs.A = RL(regs.A); } })},
                { 0x16, new Opcode(0x16, "RL (HL)",              2, 16, new Step[] {
                                                                             () => { operand = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, RL(operand)); },
                                                                         })},

                // Rotate n right through Carry flag.
                { 0x18, new Opcode(0x18, "RR B",                 2,  8, new Step[] { () => { regs.B = RR(regs.B); } })},
                { 0x19, new Opcode(0x19, "RR C",                 2,  8, new Step[] { () => { regs.C = RR(regs.C); } })},
                { 0x1A, new Opcode(0x1A, "RR D",                 2,  8, new Step[] { () => { regs.D = RR(regs.D); } })},
                { 0x1B, new Opcode(0x1B, "RR E",                 2,  8, new Step[] { () => { regs.E = RR(regs.E); } })},
                { 0x1C, new Opcode(0x1C, "RR H",                 2,  8, new Step[] { () => { regs.H = RR(regs.H); } })},
                { 0x1D, new Opcode(0x1D, "RR L",                 2,  8, new Step[] { () => { regs.L = RR(regs.L); } })},
                { 0x1F, new Opcode(0x1F, "RR A",                 2,  8, new Step[] { () => { regs.A = RR(regs.A); } })},
                { 0x1E, new Opcode(0x1E, "RR (HL)",              2, 16, new Step[] {
                                                                             () => { operand = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, RR(operand)); },
                                                                         })}, 

                // Shift n left into Carry. LSB of n set to 0
                { 0x20, new Opcode(0x20, "SLA B",                2,  8, new Step[] { () => { regs.B = SLA(regs.B); } })},
                { 0x21, new Opcode(0x21, "SLA C",                2,  8, new Step[] { () => { regs.C = SLA(regs.C); } })},
                { 0x22, new Opcode(0x22, "SLA D",                2,  8, new Step[] { () => { regs.D = SLA(regs.D); } })},
                { 0x23, new Opcode(0x23, "SLA E",                2,  8, new Step[] { () => { regs.E = SLA(regs.E); } })},
                { 0x24, new Opcode(0x24, "SLA H",                2,  8, new Step[] { () => { regs.H = SLA(regs.H); } })},
                { 0x25, new Opcode(0x25, "SLA L",                2,  8, new Step[] { () => { regs.L = SLA(regs.L); } })},
                { 0x27, new Opcode(0x27, "SLA A",                2,  8, new Step[] { () => { regs.A = SLA(regs.A); } })},
                { 0x26, new Opcode(0x26, "SLA (HL)",             2, 16, new Step[]  {
                                                                             () => { operand = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, SLA(operand)); },
                                                                         })},

                // Shift n right into Carry
                { 0x28, new Opcode(0x28, "SRA B",                2,  8, new Step[] { () => { regs.B = SRA(regs.B); } })},
                { 0x29, new Opcode(0x29, "SRA C",                2,  8, new Step[] { () => { regs.C = SRA(regs.C); } })},
                { 0x2A, new Opcode(0x2A, "SRA D",                2,  8, new Step[] { () => { regs.D = SRA(regs.D); } })},
                { 0x2B, new Opcode(0x2B, "SRA E",                2,  8, new Step[] { () => { regs.E = SRA(regs.E); } })},
                { 0x2C, new Opcode(0x2C, "SRA H",                2,  8, new Step[] { () => { regs.H = SRA(regs.H); } })},
                { 0x2D, new Opcode(0x2D, "SRA L",                2,  8, new Step[] { () => { regs.L = SRA(regs.L); } })},
                { 0x2F, new Opcode(0x2F, "SRA A",                2,  8, new Step[] { () => { regs.A = SRA(regs.A); } })},
                { 0x2E, new Opcode(0x2E, "SRA (HL)",             2, 16, new Step[]  {
                                                                             () => { operand = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, SRA(operand)); },
                                                                         })},

                // Swap upper & lower nibles of n
                { 0x30, new Opcode(0x30, "SWAP B",               2,  8, new Step[] { () => { regs.B = SWAP(regs.B); } })},
                { 0x31, new Opcode(0x31, "SWAP C",               2,  8, new Step[] { () => { regs.C = SWAP(regs.C); } })},
                { 0x32, new Opcode(0x32, "SWAP D",               2,  8, new Step[] { () => { regs.D = SWAP(regs.D); } })},
                { 0x33, new Opcode(0x33, "SWAP E",               2,  8, new Step[] { () => { regs.E = SWAP(regs.E); } })},
                { 0x34, new Opcode(0x34, "SWAP H",               2,  8, new Step[] { () => { regs.H = SWAP(regs.H); } })},
                { 0x35, new Opcode(0x35, "SWAP L",               2,  8, new Step[] { () => { regs.L = SWAP(regs.L); } })},
                { 0x37, new Opcode(0x37, "SWAP A",               2,  8, new Step[] { () => { regs.A = SWAP(regs.A); } })},
                { 0x36, new Opcode(0x36, "SWAP (HL)",            2, 16, new Step[]  {
                                                                             () => { operand = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, SWAP(operand)); },
                                                                         })},

                // Shift n right into Carry. MSB set to 0
                { 0x38, new Opcode(0x38, "SRL B",                2,  8, new Step[] { () => { regs.B = SRL(regs.B); } })},
                { 0x39, new Opcode(0x39, "SRL C",                2,  8, new Step[] { () => { regs.C = SRL(regs.C); } })},
                { 0x3A, new Opcode(0x3A, "SRL D",                2,  8, new Step[] { () => { regs.D = SRL(regs.D); } })},
                { 0x3B, new Opcode(0x3B, "SRL E",                2,  8, new Step[] { () => { regs.E = SRL(regs.E); } })},
                { 0x3C, new Opcode(0x3C, "SRL H",                2,  8, new Step[] { () => { regs.H = SRL(regs.H); } })},
                { 0x3D, new Opcode(0x3D, "SRL L",                2,  8, new Step[] { () => { regs.L = SRL(regs.L); } })},
                { 0x3F, new Opcode(0x3F, "SRL A",                2,  8, new Step[] { () => { regs.A = SRL(regs.A); } })},
                { 0x3E, new Opcode(0x3E, "SRL (HL)",             2, 16, new Step[] {
                                                                             () => { operand = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, SRL(operand)); },
                                                                         })},
                                                                 
                // Test bit b in register r                    
                { 0x40, new Opcode(0x40, "BIT 0, B",             2,  8, new Step[] { () => { BIT(regs.B, 0); } })},
                { 0x41, new Opcode(0x41, "BIT 0, C",             2,  8, new Step[] { () => { BIT(regs.C, 0); } })},
                { 0x42, new Opcode(0x42, "BIT 0, D",             2,  8, new Step[] { () => { BIT(regs.D, 0); } })},
                { 0x43, new Opcode(0x43, "BIT 0, E",             2,  8, new Step[] { () => { BIT(regs.E, 0); } })},
                { 0x44, new Opcode(0x44, "BIT 0, H",             2,  8, new Step[] { () => { BIT(regs.H, 0); } })},
                { 0x45, new Opcode(0x45, "BIT 0, L",             2,  8, new Step[] { () => { BIT(regs.L, 0); } })},
                { 0x47, new Opcode(0x47, "BIT 0, A",             2,  8, new Step[] { () => { BIT(regs.A, 0); } })},
                { 0x46, new Opcode(0x46, "BIT 0, (HL)",          2, 12, new Step[] { () => { BIT(mmu.Read8(regs.HL), 0); } })},

                { 0x48, new Opcode(0x48, "BIT 1, B",             2,  8, new Step[] { () => { BIT(regs.B, 1); } })},
                { 0x49, new Opcode(0x49, "BIT 1, C",             2,  8, new Step[] { () => { BIT(regs.C, 1); } })},
                { 0x4A, new Opcode(0x4A, "BIT 1, D",             2,  8, new Step[] { () => { BIT(regs.D, 1); } })},
                { 0x4B, new Opcode(0x4B, "BIT 1, E",             2,  8, new Step[] { () => { BIT(regs.E, 1); } })},
                { 0x4C, new Opcode(0x4C, "BIT 1, H",             2,  8, new Step[] { () => { BIT(regs.H, 1); } })},
                { 0x4D, new Opcode(0x4D, "BIT 1, L",             2,  8, new Step[] { () => { BIT(regs.L, 1); } })},
                { 0x4F, new Opcode(0x4F, "BIT 1, A",             2,  8, new Step[] { () => { BIT(regs.A, 1); } })},
                { 0x4E, new Opcode(0x4E, "BIT 1, (HL)",          2, 12, new Step[] { () => { BIT(mmu.Read8(regs.HL), 1); } })},

                { 0x50, new Opcode(0x50, "BIT 2, B",             2,  8, new Step[] { () => { BIT(regs.B, 2); } })},
                { 0x51, new Opcode(0x51, "BIT 2, C",             2,  8, new Step[] { () => { BIT(regs.C, 2); } })},
                { 0x52, new Opcode(0x52, "BIT 2, D",             2,  8, new Step[] { () => { BIT(regs.D, 2); } })},
                { 0x53, new Opcode(0x53, "BIT 2, E",             2,  8, new Step[] { () => { BIT(regs.E, 2); } })},
                { 0x54, new Opcode(0x54, "BIT 2, H",             2,  8, new Step[] { () => { BIT(regs.H, 2); } })},
                { 0x55, new Opcode(0x55, "BIT 2, L",             2,  8, new Step[] { () => { BIT(regs.L, 2); } })},
                { 0x57, new Opcode(0x57, "BIT 2, A",             2,  8, new Step[] { () => { BIT(regs.A, 2); } })},
                { 0x56, new Opcode(0x56, "BIT 2, (HL)",          2, 12, new Step[] { () => { BIT(mmu.Read8(regs.HL), 2); } })},

                { 0x58, new Opcode(0x58, "BIT 3, B",             2,  8, new Step[] { () => { BIT(regs.B, 3); } })},
                { 0x59, new Opcode(0x59, "BIT 3, C",             2,  8, new Step[] { () => { BIT(regs.C, 3); } })},
                { 0x5A, new Opcode(0x5A, "BIT 3, D",             2,  8, new Step[] { () => { BIT(regs.D, 3); } })},
                { 0x5B, new Opcode(0x5B, "BIT 3, E",             2,  8, new Step[] { () => { BIT(regs.E, 3); } })},
                { 0x5C, new Opcode(0x5C, "BIT 3, H",             2,  8, new Step[] { () => { BIT(regs.H, 3); } })},
                { 0x5D, new Opcode(0x5D, "BIT 3, L",             2,  8, new Step[] { () => { BIT(regs.L, 3); } })},
                { 0x5F, new Opcode(0x5F, "BIT 3, A",             2,  8, new Step[] { () => { BIT(regs.A, 3); } })},
                { 0x5E, new Opcode(0x5E, "BIT 3, (HL)",          2, 12, new Step[] { () => { BIT(mmu.Read8(regs.HL), 3); } })},

                { 0x60, new Opcode(0x60, "BIT 4, B",             2,  8, new Step[] { () => { BIT(regs.B, 4); } })},
                { 0x61, new Opcode(0x61, "BIT 4, C",             2,  8, new Step[] { () => { BIT(regs.C, 4); } })},
                { 0x62, new Opcode(0x62, "BIT 4, D",             2,  8, new Step[] { () => { BIT(regs.D, 4); } })},
                { 0x63, new Opcode(0x63, "BIT 4, E",             2,  8, new Step[] { () => { BIT(regs.E, 4); } })},
                { 0x64, new Opcode(0x64, "BIT 4, H",             2,  8, new Step[] { () => { BIT(regs.H, 4); } })},
                { 0x65, new Opcode(0x65, "BIT 4, L",             2,  8, new Step[] { () => { BIT(regs.L, 4); } })},
                { 0x67, new Opcode(0x67, "BIT 4, A",             2,  8, new Step[] { () => { BIT(regs.A, 4); } })},
                { 0x66, new Opcode(0x66, "BIT 4, (HL)",          2, 12, new Step[] { () => { BIT(mmu.Read8(regs.HL), 4); } })},

                { 0x68, new Opcode(0x68, "BIT 5, B",             2,  8, new Step[] { () => { BIT(regs.B, 5); } })},
                { 0x69, new Opcode(0x69, "BIT 5, C",             2,  8, new Step[] { () => { BIT(regs.C, 5); } })},
                { 0x6A, new Opcode(0x6A, "BIT 5, D",             2,  8, new Step[] { () => { BIT(regs.D, 5); } })},
                { 0x6B, new Opcode(0x6B, "BIT 5, E",             2,  8, new Step[] { () => { BIT(regs.E, 5); } })},
                { 0x6C, new Opcode(0x6C, "BIT 5, H",             2,  8, new Step[] { () => { BIT(regs.H, 5); } })},
                { 0x6D, new Opcode(0x6D, "BIT 5, L",             2,  8, new Step[] { () => { BIT(regs.L, 5); } })},
                { 0x6F, new Opcode(0x6F, "BIT 5, A",             2,  8, new Step[] { () => { BIT(regs.A, 5); } })},
                { 0x6E, new Opcode(0x6E, "BIT 5, (HL)",          2, 12, new Step[] { () => { BIT(mmu.Read8(regs.HL), 5); } })},

                { 0x70, new Opcode(0x70, "BIT 6, B",             2,  8, new Step[] { () => { BIT(regs.B, 6); } })},
                { 0x71, new Opcode(0x71, "BIT 6, C",             2,  8, new Step[] { () => { BIT(regs.C, 6); } })},
                { 0x72, new Opcode(0x72, "BIT 6, D",             2,  8, new Step[] { () => { BIT(regs.D, 6); } })},
                { 0x73, new Opcode(0x73, "BIT 6, E",             2,  8, new Step[] { () => { BIT(regs.E, 6); } })},
                { 0x74, new Opcode(0x74, "BIT 6, H",             2,  8, new Step[] { () => { BIT(regs.H, 6); } })},
                { 0x75, new Opcode(0x75, "BIT 6, L",             2,  8, new Step[] { () => { BIT(regs.L, 6); } })},
                { 0x77, new Opcode(0x77, "BIT 6, A",             2,  8, new Step[] { () => { BIT(regs.A, 6); } })},
                { 0x76, new Opcode(0x76, "BIT 6, (HL)",          2, 12, new Step[] { () => { BIT(mmu.Read8(regs.HL), 6); } })},

                { 0x78, new Opcode(0x78, "BIT 7, B",             2,  8, new Step[] { () => { BIT(regs.B, 7); } })},
                { 0x79, new Opcode(0x79, "BIT 7, C",             2,  8, new Step[] { () => { BIT(regs.C, 7); } })},
                { 0x7A, new Opcode(0x7A, "BIT 7, D",             2,  8, new Step[] { () => { BIT(regs.D, 7); } })},
                { 0x7B, new Opcode(0x7B, "BIT 7, E",             2,  8, new Step[] { () => { BIT(regs.E, 7); } })},
                { 0x7C, new Opcode(0x7C, "BIT 7, H",             2,  8, new Step[] { () => { BIT(regs.H, 7); } })},
                { 0x7D, new Opcode(0x7D, "BIT 7, L",             2,  8, new Step[] { () => { BIT(regs.L, 7); } })},
                { 0x7F, new Opcode(0x7F, "BIT 7, A",             2,  8, new Step[] { () => { BIT(regs.A, 7); } })},
                { 0x7E, new Opcode(0x7E, "BIT 7, (HL)",          2, 12, new Step[] { () => { BIT(mmu.Read8(regs.HL), 7); } })},

                // Reset bit in value
                { 0x80, new Opcode(0x80, "RES 0, B",             2,  8, new Step[] { () => { regs.B = RES(regs.B, 0); } })},
                { 0x81, new Opcode(0x81, "RES 0, C",             2,  8, new Step[] { () => { regs.C = RES(regs.C, 0); } })},
                { 0x82, new Opcode(0x82, "RES 0, D",             2,  8, new Step[] { () => { regs.D = RES(regs.D, 0); } })},
                { 0x83, new Opcode(0x83, "RES 0, E",             2,  8, new Step[] { () => { regs.E = RES(regs.E, 0); } })},
                { 0x84, new Opcode(0x84, "RES 0, H",             2,  8, new Step[] { () => { regs.H = RES(regs.H, 0); } })},
                { 0x85, new Opcode(0x85, "RES 0, L",             2,  8, new Step[] { () => { regs.L = RES(regs.L, 0); } })},
                { 0x87, new Opcode(0x87, "RES 0, A",             2,  8, new Step[] { () => { regs.A = RES(regs.A, 0); } })},
                { 0x86, new Opcode(0x86, "RES 0, (HL)",          2, 16, CB_RES_Steps(0) )},

                { 0x88, new Opcode(0x88, "RES 1, B",             2,  8, new Step[] { () => { regs.B = RES(regs.B, 1); } })},
                { 0x89, new Opcode(0x89, "RES 1, C",             2,  8, new Step[] { () => { regs.C = RES(regs.C, 1); } })},
                { 0x8A, new Opcode(0x8A, "RES 1, D",             2,  8, new Step[] { () => { regs.D = RES(regs.D, 1); } })},
                { 0x8B, new Opcode(0x8B, "RES 1, E",             2,  8, new Step[] { () => { regs.E = RES(regs.E, 1); } })},
                { 0x8C, new Opcode(0x8C, "RES 1, H",             2,  8, new Step[] { () => { regs.H = RES(regs.H, 1); } })},
                { 0x8D, new Opcode(0x8D, "RES 1, L",             2,  8, new Step[] { () => { regs.L = RES(regs.L, 1); } })},
                { 0x8F, new Opcode(0x8F, "RES 1, A",             2,  8, new Step[] { () => { regs.A = RES(regs.A, 1); } })},
                { 0x8E, new Opcode(0x8E, "RES 1, (HL)",          2, 16, CB_RES_Steps(1) )},

                { 0x90, new Opcode(0x90, "RES 2, B",             2,  8, new Step[] { () => { regs.B = RES(regs.B, 2); } })},
                { 0x91, new Opcode(0x91, "RES 2, C",             2,  8, new Step[] { () => { regs.C = RES(regs.C, 2); } })},
                { 0x92, new Opcode(0x92, "RES 2, D",             2,  8, new Step[] { () => { regs.D = RES(regs.D, 2); } })},
                { 0x93, new Opcode(0x93, "RES 2, E",             2,  8, new Step[] { () => { regs.E = RES(regs.E, 2); } })},
                { 0x94, new Opcode(0x94, "RES 2, H",             2,  8, new Step[] { () => { regs.H = RES(regs.H, 2); } })},
                { 0x95, new Opcode(0x95, "RES 2, L",             2,  8, new Step[] { () => { regs.L = RES(regs.L, 2); } })},
                { 0x97, new Opcode(0x97, "RES 2, A",             2,  8, new Step[] { () => { regs.A = RES(regs.A, 2); } })},
                { 0x96, new Opcode(0x96, "RES 2, (HL)",          2, 16, CB_RES_Steps(2) )},

                { 0x98, new Opcode(0x98, "RES 3, B",             2,  8, new Step[] { () => { regs.B = RES(regs.B, 3); } })},
                { 0x99, new Opcode(0x99, "RES 3, C",             2,  8, new Step[] { () => { regs.C = RES(regs.C, 3); } })},
                { 0x9A, new Opcode(0x9A, "RES 3, D",             2,  8, new Step[] { () => { regs.D = RES(regs.D, 3); } })},
                { 0x9B, new Opcode(0x9B, "RES 3, E",             2,  8, new Step[] { () => { regs.E = RES(regs.E, 3); } })},
                { 0x9C, new Opcode(0x9C, "RES 3, H",             2,  8, new Step[] { () => { regs.H = RES(regs.H, 3); } })},
                { 0x9D, new Opcode(0x9D, "RES 3, L",             2,  8, new Step[] { () => { regs.L = RES(regs.L, 3); } })},
                { 0x9F, new Opcode(0x9F, "RES 3, A",             2,  8, new Step[] { () => { regs.A = RES(regs.A, 3); } })},
                { 0x9E, new Opcode(0x9E, "RES 3, (HL)",          2, 16, CB_RES_Steps(3) )},

                { 0xA0, new Opcode(0xA0, "RES 4, B",             2,  8, new Step[] { () => { regs.B = RES(regs.B, 4); } })},
                { 0xA1, new Opcode(0xA1, "RES 4, C",             2,  8, new Step[] { () => { regs.C = RES(regs.C, 4); } })},
                { 0xA2, new Opcode(0xA2, "RES 4, D",             2,  8, new Step[] { () => { regs.D = RES(regs.D, 4); } })},
                { 0xA3, new Opcode(0xA3, "RES 4, E",             2,  8, new Step[] { () => { regs.E = RES(regs.E, 4); } })},
                { 0xA4, new Opcode(0xA4, "RES 4, H",             2,  8, new Step[] { () => { regs.H = RES(regs.H, 4); } })},
                { 0xA5, new Opcode(0xA5, "RES 4, L",             2,  8, new Step[] { () => { regs.L = RES(regs.L, 4); } })},
                { 0xA7, new Opcode(0xA7, "RES 4, A",             2,  8, new Step[] { () => { regs.A = RES(regs.A, 4); } })},
                { 0xA6, new Opcode(0xA6, "RES 4, (HL)",          2, 16, CB_RES_Steps(4) )},

                { 0xA8, new Opcode(0xA8, "RES 5, B",             2,  8, new Step[] { () => { regs.B = RES(regs.B, 5); } })},
                { 0xA9, new Opcode(0xA9, "RES 5, C",             2,  8, new Step[] { () => { regs.C = RES(regs.C, 5); } })},
                { 0xAA, new Opcode(0xAA, "RES 5, D",             2,  8, new Step[] { () => { regs.D = RES(regs.D, 5); } })},
                { 0xAB, new Opcode(0xAB, "RES 5, E",             2,  8, new Step[] { () => { regs.E = RES(regs.E, 5); } })},
                { 0xAC, new Opcode(0xAC, "RES 5, H",             2,  8, new Step[] { () => { regs.H = RES(regs.H, 5); } })},
                { 0xAD, new Opcode(0xAD, "RES 5, L",             2,  8, new Step[] { () => { regs.L = RES(regs.L, 5); } })},
                { 0xAF, new Opcode(0xAF, "RES 5, A",             2,  8, new Step[] { () => { regs.A = RES(regs.A, 5); } })},
                { 0xAE, new Opcode(0xAE, "RES 5, (HL)",          2, 16, CB_RES_Steps(5) )},

                { 0xB0, new Opcode(0xB0, "RES 6, B",             2,  8, new Step[] { () => { regs.B = RES(regs.B, 6); } })},
                { 0xB1, new Opcode(0xB1, "RES 6, C",             2,  8, new Step[] { () => { regs.C = RES(regs.C, 6); } })},
                { 0xB2, new Opcode(0xB2, "RES 6, D",             2,  8, new Step[] { () => { regs.D = RES(regs.D, 6); } })},
                { 0xB3, new Opcode(0xB3, "RES 6, E",             2,  8, new Step[] { () => { regs.E = RES(regs.E, 6); } })},
                { 0xB4, new Opcode(0xB4, "RES 6, H",             2,  8, new Step[] { () => { regs.H = RES(regs.H, 6); } })},
                { 0xB5, new Opcode(0xB5, "RES 6, L",             2,  8, new Step[] { () => { regs.L = RES(regs.L, 6); } })},
                { 0xB7, new Opcode(0xB7, "RES 6, A",             2,  8, new Step[] { () => { regs.A = RES(regs.A, 6); } })},
                { 0xB6, new Opcode(0xB6, "RES 6, (HL)",          2, 16, CB_RES_Steps(6) )},

                { 0xB8, new Opcode(0xB8, "RES 7, B",             2,  8, new Step[] { () => { regs.B = RES(regs.B, 7); } })},
                { 0xB9, new Opcode(0xB9, "RES 7, C",             2,  8, new Step[] { () => { regs.C = RES(regs.C, 7); } })},
                { 0xBA, new Opcode(0xBA, "RES 7, D",             2,  8, new Step[] { () => { regs.D = RES(regs.D, 7); } })},
                { 0xBB, new Opcode(0xBB, "RES 7, E",             2,  8, new Step[] { () => { regs.E = RES(regs.E, 7); } })},
                { 0xBC, new Opcode(0xBC, "RES 7, H",             2,  8, new Step[] { () => { regs.H = RES(regs.H, 7); } })},
                { 0xBD, new Opcode(0xBD, "RES 7, L",             2,  8, new Step[] { () => { regs.L = RES(regs.L, 7); } })},
                { 0xBF, new Opcode(0xBF, "RES 7, A",             2,  8, new Step[] { () => { regs.A = RES(regs.A, 7); } })},
                { 0xBE, new Opcode(0xBE, "RES 7, (HL)",          2, 16, CB_RES_Steps(7) )},

                // Set bit in value
                { 0xC0, new Opcode(0xC0, "SET 0, B",             2,  8, new Step[] { () => { regs.B = SET(regs.B, 0); } })},
                { 0xC1, new Opcode(0xC1, "SET 0, C",             2,  8, new Step[] { () => { regs.C = SET(regs.C, 0); } })},
                { 0xC2, new Opcode(0xC2, "SET 0, D",             2,  8, new Step[] { () => { regs.D = SET(regs.D, 0); } })},
                { 0xC3, new Opcode(0xC3, "SET 0, E",             2,  8, new Step[] { () => { regs.E = SET(regs.E, 0); } })},
                { 0xC4, new Opcode(0xC4, "SET 0, H",             2,  8, new Step[] { () => { regs.H = SET(regs.H, 0); } })},
                { 0xC5, new Opcode(0xC5, "SET 0, L",             2,  8, new Step[] { () => { regs.L = SET(regs.L, 0); } })},
                { 0xC7, new Opcode(0xC7, "SET 0, A",             2,  8, new Step[] { () => { regs.A = SET(regs.A, 0); } })},
                { 0xC6, new Opcode(0xC6, "SET 0, (HL)",          2, 16, CB_SET_Steps(0) )},

                { 0xC8, new Opcode(0xC8, "SET 1, B",             2,  8, new Step[] { () => { regs.B = SET(regs.B, 1); } })},
                { 0xC9, new Opcode(0xC9, "SET 1, C",             2,  8, new Step[] { () => { regs.C = SET(regs.C, 1); } })},
                { 0xCA, new Opcode(0xCA, "SET 1, D",             2,  8, new Step[] { () => { regs.D = SET(regs.D, 1); } })},
                { 0xCB, new Opcode(0xCB, "SET 1, E",             2,  8, new Step[] { () => { regs.E = SET(regs.E, 1); } })},
                { 0xCC, new Opcode(0xCC, "SET 1, H",             2,  8, new Step[] { () => { regs.H = SET(regs.H, 1); } })},
                { 0xCD, new Opcode(0xCD, "SET 1, L",             2,  8, new Step[] { () => { regs.L = SET(regs.L, 1); } })},
                { 0xCF, new Opcode(0xCF, "SET 1, A",             2,  8, new Step[] { () => { regs.A = SET(regs.A, 1); } })},
                { 0xCE, new Opcode(0xCE, "SET 1, (HL)",          2, 16, CB_SET_Steps(1) )},

                { 0xD0, new Opcode(0xD0, "SET 2, B",             2,  8, new Step[] { () => { regs.B = SET(regs.B, 2); } })},
                { 0xD1, new Opcode(0xD1, "SET 2, C",             2,  8, new Step[] { () => { regs.C = SET(regs.C, 2); } })},
                { 0xD2, new Opcode(0xD2, "SET 2, D",             2,  8, new Step[] { () => { regs.D = SET(regs.D, 2); } })},
                { 0xD3, new Opcode(0xD3, "SET 2, E",             2,  8, new Step[] { () => { regs.E = SET(regs.E, 2); } })},
                { 0xD4, new Opcode(0xD4, "SET 2, H",             2,  8, new Step[] { () => { regs.H = SET(regs.H, 2); } })},
                { 0xD5, new Opcode(0xD5, "SET 2, L",             2,  8, new Step[] { () => { regs.L = SET(regs.L, 2); } })},
                { 0xD7, new Opcode(0xD7, "SET 2, A",             2,  8, new Step[] { () => { regs.A = SET(regs.A, 2); } })},
                { 0xD6, new Opcode(0xD6, "SET 2, (HL)",          2, 16, CB_SET_Steps(2) )},

                { 0xD8, new Opcode(0xD8, "SET 3, B",             2,  8, new Step[] { () => { regs.B = SET(regs.B, 3); } })},
                { 0xD9, new Opcode(0xD9, "SET 3, C",             2,  8, new Step[] { () => { regs.C = SET(regs.C, 3); } })},
                { 0xDA, new Opcode(0xDA, "SET 3, D",             2,  8, new Step[] { () => { regs.D = SET(regs.D, 3); } })},
                { 0xDB, new Opcode(0xDB, "SET 3, E",             2,  8, new Step[] { () => { regs.E = SET(regs.E, 3); } })},
                { 0xDC, new Opcode(0xDC, "SET 3, H",             2,  8, new Step[] { () => { regs.H = SET(regs.H, 3); } })},
                { 0xDD, new Opcode(0xDD, "SET 3, L",             2,  8, new Step[] { () => { regs.L = SET(regs.L, 3); } })},
                { 0xDF, new Opcode(0xDF, "SET 3, A",             2,  8, new Step[] { () => { regs.A = SET(regs.A, 3); } })},
                { 0xDE, new Opcode(0xDE, "SET 3, (HL)",          2, 16, CB_SET_Steps(3) )},

                { 0xE0, new Opcode(0xE0, "SET 4, B",             2,  8, new Step[] { () => { regs.B = SET(regs.B, 4); } })},
                { 0xE1, new Opcode(0xE1, "SET 4, C",             2,  8, new Step[] { () => { regs.C = SET(regs.C, 4); } })},
                { 0xE2, new Opcode(0xE2, "SET 4, D",             2,  8, new Step[] { () => { regs.D = SET(regs.D, 4); } })},
                { 0xE3, new Opcode(0xE3, "SET 4, E",             2,  8, new Step[] { () => { regs.E = SET(regs.E, 4); } })},
                { 0xE4, new Opcode(0xE4, "SET 4, H",             2,  8, new Step[] { () => { regs.H = SET(regs.H, 4); } })},
                { 0xE5, new Opcode(0xE5, "SET 4, L",             2,  8, new Step[] { () => { regs.L = SET(regs.L, 4); } })},
                { 0xE7, new Opcode(0xE7, "SET 4, A",             2,  8, new Step[] { () => { regs.A = SET(regs.A, 4); } })},
                { 0xE6, new Opcode(0xE6, "SET 4, (HL)",          2, 16, CB_SET_Steps(4) )},

                { 0xE8, new Opcode(0xE8, "SET 5, B",             2,  8, new Step[] { () => { regs.B = SET(regs.B, 5); } })},
                { 0xE9, new Opcode(0xE9, "SET 5, C",             2,  8, new Step[] { () => { regs.C = SET(regs.C, 5); } })},
                { 0xEA, new Opcode(0xEA, "SET 5, D",             2,  8, new Step[] { () => { regs.D = SET(regs.D, 5); } })},
                { 0xEB, new Opcode(0xEB, "SET 5, E",             2,  8, new Step[] { () => { regs.E = SET(regs.E, 5); } })},
                { 0xEC, new Opcode(0xEC, "SET 5, H",             2,  8, new Step[] { () => { regs.H = SET(regs.H, 5); } })},
                { 0xED, new Opcode(0xED, "SET 5, L",             2,  8, new Step[] { () => { regs.L = SET(regs.L, 5); } })},
                { 0xEF, new Opcode(0xEF, "SET 5, A",             2,  8, new Step[] { () => { regs.A = SET(regs.A, 5); } })},
                { 0xEE, new Opcode(0xEE, "SET 5, (HL)",          2, 16, CB_SET_Steps(5) )},

                { 0xF0, new Opcode(0xF0, "SET 6, B",             2,  8, new Step[] { () => { regs.B = SET(regs.B, 6); } })},
                { 0xF1, new Opcode(0xF1, "SET 6, C",             2,  8, new Step[] { () => { regs.C = SET(regs.C, 6); } })},
                { 0xF2, new Opcode(0xF2, "SET 6, D",             2,  8, new Step[] { () => { regs.D = SET(regs.D, 6); } })},
                { 0xF3, new Opcode(0xF3, "SET 6, E",             2,  8, new Step[] { () => { regs.E = SET(regs.E, 6); } })},
                { 0xF4, new Opcode(0xF4, "SET 6, H",             2,  8, new Step[] { () => { regs.H = SET(regs.H, 6); } })},
                { 0xF5, new Opcode(0xF5, "SET 6, L",             2,  8, new Step[] { () => { regs.L = SET(regs.L, 6); } })},
                { 0xF7, new Opcode(0xF7, "SET 6, A",             2,  8, new Step[] { () => { regs.A = SET(regs.A, 6); } })},
                { 0xF6, new Opcode(0xF6, "SET 6, (HL)",          2, 16, CB_SET_Steps(6) )},

                { 0xF8, new Opcode(0xF8, "SET 7, B",             2,  8, new Step[] { () => { regs.B = SET(regs.B, 7); } })},
                { 0xF9, new Opcode(0xF9, "SET 7, C",             2,  8, new Step[] { () => { regs.C = SET(regs.C, 7); } })},
                { 0xFA, new Opcode(0xFA, "SET 7, D",             2,  8, new Step[] { () => { regs.D = SET(regs.D, 7); } })},
                { 0xFB, new Opcode(0xFB, "SET 7, E",             2,  8, new Step[] { () => { regs.E = SET(regs.E, 7); } })},
                { 0xFC, new Opcode(0xFC, "SET 7, H",             2,  8, new Step[] { () => { regs.H = SET(regs.H, 7); } })},
                { 0xFD, new Opcode(0xFD, "SET 7, L",             2,  8, new Step[] { () => { regs.L = SET(regs.L, 7); } })},
                { 0xFF, new Opcode(0xFF, "SET 7, A",             2,  8, new Step[] { () => { regs.A = SET(regs.A, 7); } })},
                { 0xFE, new Opcode(0xFE, "SET 7, (HL)",          2, 16, CB_SET_Steps(7) )},
            };
        }

        private Step[] CB_RES_Steps(int bitPos) {
            return new Step[] {
                () => { operand = mmu.Read8(regs.HL); },
                () => { mmu.Write8(regs.HL, RES(operand, bitPos)); },
            };
        }

        private Step[] CB_SET_Steps(int bitPos) {
            return new Step[] {
                () => { operand = mmu.Read8(regs.HL); },
                () => { mmu.Write8(regs.HL, SET(operand, bitPos)); },
            };
        }

        private u8 RLC(u8 value) {
            byte result = (byte)((value << 1) | (value >> 7));
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = (value & (0b_0000_0001 << 7)) == (0b_0000_0001 << 7);
            return result;
        }

        // Set bit in value
        private byte SET(u8 value, int bitPosition) {
            return (u8)(value | (0b_0000_0001 << bitPosition));
        }

        // Reset bit in value
        private byte RES(u8 value, int bitPosition) {
            return (byte)(value & ~(0b_0000_0001 << bitPosition));
        }

        private u8 RRC(u8 value) {
            u8 result = (u8)((value >> 1) | (value << 7));
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = (value & 0b_0000_0001) == 1;
            return result;
        }

        private u8 RL(u8 value) {
            u8 result = (u8)((value << 1) | (regs.FlagC ? 1 : 0));
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = (value & (0b_0000_0001 << 7)) == (0b_0000_0001 << 7);
            return result;
        }

        private u8 RR(u8 value) {
            u8 result = (u8)((value >> 1) | (regs.FlagC ? 1 << 7 : 0));
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = (value & 1) == 1;
            return result;
        }

        private u8 SRA(byte value) {
            u8 result = (u8)((value >> 1) | (value & 0b_1000_0000));
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = (value & 1) == 1;
            return result;
        }

        private u8 SLA(byte value) {
            u8 result = (u8)(value << 1);
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = (value & (1 << 7)) == (1 << 7);
            return result;
        }

        private u8 SWAP(byte value) {
            u8 result = (u8)((value & 0b_1111_0000) >> 4 | (value & 0b_0000_1111) << 4);
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = false;
            return result;
        }

        private u8 SRL(u8 value) {
            byte result = (byte)(value >> 1);
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = (value & 0b_0000_0001) != 0;
            return result;
        }

        // Test bit in value
        // Z - Set if bit b of register r is 0.
        private void BIT(u8 value, int bitPosition) {
            regs.FlagZ = ((value >> bitPosition) & 0b_0000_0001) == 0;
            regs.FlagN = false;
            regs.FlagH = true;
            // r.FlagC -> unmodified
        }

    }
}
