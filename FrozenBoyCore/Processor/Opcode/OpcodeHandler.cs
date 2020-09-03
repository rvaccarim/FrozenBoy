using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Memory;
using FrozenBoyCore.Util;
using System.Runtime.CompilerServices;
using FrozenBoyCore.Graphics;
using System.IO;

namespace FrozenBoyCore.Processor {
    public class OpcodeHandler {
        private readonly Registers regs;
        private readonly MMU mmu;
        private readonly GPU gpu;
        private readonly InterruptManager intManager;

        private u8 lsb;
        private u8 msb;
        private u8 operand8;
        private u16 address16;
        private u16 result16;
        public int deltaCycles;
        public bool stop;

        public Dictionary<u8, Opcode> opcodes;

        public OpcodeHandler(Registers regs, MMU mmu, InterruptManager intManager, GPU gpu) {
            this.regs = regs;
            this.mmu = mmu;
            this.gpu = gpu;
            this.intManager = intManager;

            opcodes = InitializeOpcodes();
        }

        private Dictionary<u8, Opcode> InitializeOpcodes() {
            return new Dictionary<u8, Opcode> {
                // ==================================================================================================================
                // ADDITION FAMILY
                // ==================================================================================================================
                // Add value + Carry flag to A
                { 0x88, new Opcode(0x88, "ADC A, B",             1,  4, new Step[] { () => { ADC(regs.B); } })},
                { 0x89, new Opcode(0x89, "ADC A, C",             1,  4, new Step[] { () => { ADC(regs.C); } })},
                { 0x8A, new Opcode(0x8A, "ADC A, D",             1,  4, new Step[] { () => { ADC(regs.D); } })},
                { 0x8B, new Opcode(0x8B, "ADC A, E",             1,  4, new Step[] { () => { ADC(regs.E); } })},
                { 0x8C, new Opcode(0x8C, "ADC A, H",             1,  4, new Step[] { () => { ADC(regs.H); } })},
                { 0x8D, new Opcode(0x8D, "ADC A, L",             1,  4, new Step[] { () => { ADC(regs.L); } })},
                { 0x8F, new Opcode(0x8F, "ADC A, A",             1,  4, new Step[] { () => { ADC(regs.A); } })},
                { 0x8E, new Opcode(0x8E, "ADC A, (HL)",          1,  8, new Step[] { () => { ADC(mmu.Read8(regs.HL)); } })},
                { 0xCE, new Opcode(0xCE, "ADC A, ${0:x2}",       2,  8, new Step[] { () => { ADC(mmu.Read8(regs.PC++)); } })},  // no need to split it in two steps,
                                                                                                                          // because the memory access and the
                                                                                                                          // arithmetic operations and memory access
                                                                                                                          // can be done in the same cycle
                // Add 16 bit
                { 0x09, new Opcode(0x09, "ADD HL, BC",           1,  8, new Step[] { () => { ADD_HL(regs.BC); } })},
                { 0x19, new Opcode(0x19, "ADD HL, DE",           1,  8, new Step[] { () => { ADD_HL(regs.DE); } })},
                { 0x29, new Opcode(0x29, "ADD HL, HL",           1,  8, new Step[] { () => { ADD_HL(regs.HL); } })},
                { 0x39, new Opcode(0x39, "ADD HL, SP",           1,  8, new Step[] { () => { ADD_HL(regs.SP); } })},
                // Add 8 bit
                { 0x80, new Opcode(0x80, "ADD A, B",             1,  4, new Step[] { () => { ADD(regs.B); } })},
                { 0x81, new Opcode(0x81, "ADD A, C",             1,  4, new Step[] { () => { ADD(regs.C); } })},
                { 0x82, new Opcode(0x82, "ADD A, D",             1,  4, new Step[] { () => { ADD(regs.D); } })},
                { 0x83, new Opcode(0x83, "ADD A, E",             1,  4, new Step[] { () => { ADD(regs.E); } })},
                { 0x84, new Opcode(0x84, "ADD A, H",             1,  4, new Step[] { () => { ADD(regs.H); } })},
                { 0x85, new Opcode(0x85, "ADD A, L",             1,  4, new Step[] { () => { ADD(regs.L); } })},
                { 0x87, new Opcode(0x87, "ADD A, A",             1,  4, new Step[] { () => { ADD(regs.A); } })},
                { 0x86, new Opcode(0x86, "ADD A, (HL)",          1,  8, new Step[] { () => { ADD(mmu.Read8(regs.HL)); } })},
                { 0xC6, new Opcode(0xC6, "ADD A, ${0:x2}",       2,  8, new Step[] { () => { ADD(mmu.Read8(regs.PC++)); } })},
                { 0xE8, new Opcode(0xE8, "ADD SP, ${0:x2}",      2, 16, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); },
                                                                             () => { result16 = ADD_Signed8(regs.SP, operand8); },
                                                                             () => { regs.SP = result16; }
                                                                         })},
                // INC - 8 bit                                  
                { 0x04, new Opcode(0x04, "INC B",                1,  4, new Step[] { () => { regs.B = INC(regs.B); } })},
                { 0x0C, new Opcode(0x0C, "INC C",                1,  4, new Step[] { () => { regs.C = INC(regs.C); } })},
                { 0x14, new Opcode(0x14, "INC D",                1,  4, new Step[] { () => { regs.D = INC(regs.D); } })},
                { 0x1C, new Opcode(0x1C, "INC E",                1,  4, new Step[] { () => { regs.E = INC(regs.E); } })},
                { 0x24, new Opcode(0x24, "INC H",                1,  4, new Step[] { () => { regs.H = INC(regs.H); } })},
                { 0x2C, new Opcode(0x2C, "INC L",                1,  4, new Step[] { () => { regs.L = INC(regs.L); } })},
                { 0x3C, new Opcode(0x3C, "INC A",                1,  4, new Step[] { () => { regs.A = INC(regs.A); } })},
                { 0x34, new Opcode(0x34, "INC (HL)",             1, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, INC(operand8)); },
                                                                         })},
                // INC - 16 bit
                { 0x03, new Opcode(0x03, "INC BC",               1,  8, new Step[] { () => { if (OAM_Bug(regs.BC)) CorruptOAM(CorruptionType.INC_DEC);
                                                                                             regs.BC++; } })},
                { 0x13, new Opcode(0x13, "INC DE",               1,  8, new Step[] { () => { if (OAM_Bug(regs.DE)) CorruptOAM(CorruptionType.INC_DEC);
                                                                                             regs.DE++; } })},
                { 0x23, new Opcode(0x23, "INC HL",               1,  8, new Step[] { () => { if (OAM_Bug(regs.HL)) CorruptOAM(CorruptionType.INC_DEC);
                                                                                             regs.HL++; } })},
                { 0x33, new Opcode(0x33, "INC SP",               1,  8, new Step[] { () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.INC_DEC);
                                                                                             regs.SP++; } })}, 

                // ==================================================================================================================
                // AND, OR, XOR
                // ==================================================================================================================
                { 0xA0, new Opcode(0xA0, "AND B",                1,  4, new Step[] { () => { AND(regs.B); } })},
                { 0xA1, new Opcode(0xA1, "AND C",                1,  4, new Step[] { () => { AND(regs.C); } })},
                { 0xA2, new Opcode(0xA2, "AND D",                1,  4, new Step[] { () => { AND(regs.D); } })},
                { 0xA3, new Opcode(0xA3, "AND E",                1,  4, new Step[] { () => { AND(regs.E); } })},
                { 0xA4, new Opcode(0xA4, "AND H",                1,  4, new Step[] { () => { AND(regs.H); } })},
                { 0xA5, new Opcode(0xA5, "AND L",                1,  4, new Step[] { () => { AND(regs.L); } })},
                { 0xA7, new Opcode(0xA7, "AND A",                1,  4, new Step[] { () => { AND(regs.A); } })},
                { 0xA6, new Opcode(0xA6, "AND (HL)",             1,  8, new Step[] { () => { AND(mmu.Read8(regs.HL)); }, })},
                { 0xE6, new Opcode(0xE6, "AND ${0:x2}",          2,  8, new Step[] { () => { AND(mmu.Read8(regs.PC++)); } })},
                // OR
                { 0xB0, new Opcode(0xB0, "OR B",                 1,  4, new Step[] { () => { OR(regs.B); } })},
                { 0xB1, new Opcode(0xB1, "OR C",                 1,  4, new Step[] { () => { OR(regs.C); } })},
                { 0xB2, new Opcode(0xB2, "OR D",                 1,  4, new Step[] { () => { OR(regs.D); } })},
                { 0xB3, new Opcode(0xB3, "OR E",                 1,  4, new Step[] { () => { OR(regs.E); } })},
                { 0xB4, new Opcode(0xB4, "OR H",                 1,  4, new Step[] { () => { OR(regs.H); } })},
                { 0xB5, new Opcode(0xB5, "OR L",                 1,  4, new Step[] { () => { OR(regs.L); } })},
                { 0xB7, new Opcode(0xB7, "OR A",                 1,  4, new Step[] { () => { OR(regs.A); } })},
                { 0xB6, new Opcode(0xB6, "OR (HL)",              1,  8, new Step[] { () => { OR(mmu.Read8(regs.HL)); } })},
                { 0xF6, new Opcode(0xF6, "OR ${0:x2}",           2,  8, new Step[] { () => { OR(mmu.Read8(regs.PC++)); } })},
                // XOR
                { 0xA8, new Opcode(0xA8, "XOR B",                1,  4, new Step[] { () => { XOR(regs.B); } })},
                { 0xA9, new Opcode(0xA9, "XOR C",                1,  4, new Step[] { () => { XOR(regs.C); } })},
                { 0xAA, new Opcode(0xAA, "XOR D",                1,  4, new Step[] { () => { XOR(regs.D); } })},
                { 0xAB, new Opcode(0xAB, "XOR E",                1,  4, new Step[] { () => { XOR(regs.E); } })},
                { 0xAC, new Opcode(0xAC, "XOR H",                1,  4, new Step[] { () => { XOR(regs.H); } })},
                { 0xAD, new Opcode(0xAD, "XOR L",                1,  4, new Step[] { () => { XOR(regs.L); } })},
                { 0xAF, new Opcode(0xAF, "XOR A",                1,  4, new Step[] { () => { XOR(regs.A); } })},
                { 0xAE, new Opcode(0xAE, "XOR (HL)",             1,  8, new Step[] { () => { XOR(mmu.Read8(regs.HL)); ; } })},
                { 0xEE, new Opcode(0xEE, "XOR ${0:x2}",          2,  8, new Step[] { () => { XOR(mmu.Read8(regs.PC++)); } })},

                // ==================================================================================================================
                // CALL AND RETURN
                // ==================================================================================================================
                // Push address of next instruction onto stack and then jump to address nn.
                { 0xCD, new Opcode(0xCD, "CALL ${0:x4}",         3, 24, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.PC)); },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.PC)); },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); },
                                                                             })},
                // In C# you can't pass a value by reference to an anonymous function
                { 0xC4, new Opcode(0xC4, "CALL NZ, ${0:x4}",     3, 24, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); }, //  8t
                                                                             () => { msb = mmu.Read8(regs.PC++); if ( regs.FlagZ) { stop = true; } }, // 12t
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.PC)); }, // 16t
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.PC)); }, // 20t                        
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, // 24t
                                                                             })},
                { 0xCC, new Opcode(0xCC, "CALL Z, ${0:x4}",      3, 24, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); }, //  8t
                                                                             () => { msb = mmu.Read8(regs.PC++); if (!regs.FlagZ) { stop = true; } }, // 12t
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.PC)); }, // 16t
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.PC)); }, // 20t                        
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, // 24t
                                                                             })},
                { 0xD4, new Opcode(0xD4, "CALL NC, ${0:x4}",     3, 24, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); }, //  8t
                                                                             () => { msb = mmu.Read8(regs.PC++); if (regs.FlagC) { stop = true; } }, // 12t
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.PC)); }, // 16t
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.PC)); }, // 20t                        
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, // 24t
                                                                             })},
                { 0xDC, new Opcode(0xDC, "CALL C, ${0:x4}",      3, 24, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); }, //  8t
                                                                             () => { msb = mmu.Read8(regs.PC++); if (!regs.FlagC) { stop = true; } }, // 12t
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.PC)); }, // 16t
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                     regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.PC)); }, // 20t                        
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, // 24t 
                                                                             })},

                // Return - Pop two bytes from stack & jump to that address.
                { 0xC0, new Opcode(0xC0, "RET NZ",               1, 20, new Step[] {
                                                                             // You can't pass a value by ref to an anonymous function
                                                                             () => { if (regs.FlagZ) {stop = true; } },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0xC8, new Opcode(0xC8, "RET Z",                1, 20, new Step[] {
                                                                             () => { if (! regs.FlagZ) {stop = true; } },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0xD0, new Opcode(0xD0, "RET NC",               1, 20, new Step[] {
                                                                             () => { if (regs.FlagC) {stop = true; } },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0xD8, new Opcode(0xD8, "RET C",                1, 20, new Step[] {
                                                                             () => { if (! regs.FlagC) {stop = true; } },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0xC9, new Opcode(0xC9, "RET",                  1, 16, new Step[] {
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0xD9, new Opcode(0xD9, "RETI",                 1, 16, new Step[] {
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); intManager.IME = true; }, })},

                // ==================================================================================================================
                // COMPARE
                // ==================================================================================================================
                // Compare A with n. This is basically an A - n subtraction instruction but the results are thrown away
                { 0xB8, new Opcode(0xB8, "CP B",                 1,  4, new Step[] { () => { CP(regs.B); } })},
                { 0xB9, new Opcode(0xB9, "CP C",                 1,  4, new Step[] { () => { CP(regs.C); } })},
                { 0xBA, new Opcode(0xBA, "CP D",                 1,  4, new Step[] { () => { CP(regs.D); } })},
                { 0xBB, new Opcode(0xBB, "CP E",                 1,  4, new Step[] { () => { CP(regs.E); } })},
                { 0xBC, new Opcode(0xBC, "CP H",                 1,  4, new Step[] { () => { CP(regs.H); } })},
                { 0xBD, new Opcode(0xBD, "CP L",                 1,  4, new Step[] { () => { CP(regs.L); } })},
                { 0xBE, new Opcode(0xBE, "CP (HL)",              1,  8, new Step[] { () => { CP(mmu.Read8(regs.HL)); } })},
                { 0xBF, new Opcode(0xBF, "CP A",                 1,  4, new Step[] { () => { CP(regs.A); } })},
                { 0xFE, new Opcode(0xFE, "CP ${0:x2}",           2,  8, new Step[] { () => { CP(mmu.Read8(regs.PC++)); } })},

                // ==================================================================================================================
                // INTERRUPTS
                // ==================================================================================================================
                // Disables interrupt handling by setting IME=0 and cancelling any scheduled effects of the EI instruction if any
                { 0xF3, new Opcode(0xF3, "DI",                   1,  4, new Step[] { () => { intManager.DisableInterrupts(); } })},
                // Schedules interrupt handling to be enabled after the next machine cycle
                { 0xFB, new Opcode(0xFB, "EI",                   1,  4, new Step[] { () => { intManager.EnableInterrupts(true); } })},
                                        
                // ==================================================================================================================
                // JUMP FAMILY
                // ==================================================================================================================
                // JP - Jump to location
                { 0xE9, new Opcode(0xE9, "JP (HL)",              1,  4, new Step[] { () => { regs.PC = regs.HL; } })},
                { 0xC3, new Opcode(0xC3, "JP ${0:x4}",           3, 16, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); } })},
                { 0xC2, new Opcode(0xC2, "JP NZ, ${0:x4}",       3, 16, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); if (regs.FlagZ) { stop=true; } },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); } })},
                { 0xCA, new Opcode(0xCA, "JP Z, ${0:x4}",        3, 16, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); if (! regs.FlagZ) { stop=true; } },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); } })},
                { 0xD2, new Opcode(0xD2, "JP NC, ${0:x4}",       3, 16, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); if (regs.FlagC) { stop=true; } },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); } })},
                { 0xDA, new Opcode(0xDA, "JP C, ${0:x4}",        3, 16, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); if (! regs.FlagC) { stop=true; } },
                                                                             () => { regs.PC = BitUtils.ToUnsigned16(msb, lsb); } })},
                // Jump to location relative to the current location
                // 12t if branched, 8 if not branched 
                { 0x18, new Opcode(0x18, "JR ${0:x2}",           2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); },
                                                                             () => { regs.PC = (u16)(regs.PC + ToSigned(operand8)); } })},
                { 0x20, new Opcode(0x20, "JR NZ, ${0:x2}",       2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); if ( regs.FlagZ) { stop = true; } },
                                                                             () => { regs.PC = (u16)(regs.PC + ToSigned(operand8)); } })},
                { 0x28, new Opcode(0x28, "JR Z, ${0:x2}",        2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); if (!regs.FlagZ) { stop = true; } },
                                                                             () => { regs.PC = (u16)(regs.PC + ToSigned(operand8));  } })},
                { 0x30, new Opcode(0x30, "JR NC, ${0:x2}",       2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); if ( regs.FlagC) { stop = true; } },
                                                                             () => { regs.PC = (u16)(regs.PC + ToSigned(operand8));  } })},
                { 0x38, new Opcode(0x38, "JR C, ${0:x2}",        2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); if (!regs.FlagC) { stop = true; } },
                                                                             () => { regs.PC = (u16)(regs.PC + ToSigned(operand8)); } })},
                                                                       
                // ==================================================================================================================
                // LOAD FANILY
                // ==================================================================================================================
                // load direct value into register - 8 bit
                { 0x06, new Opcode(0x06, "LD B, ${0:x2}",        2,  8, new Step[] { () => { regs.B = mmu.Read8(regs.PC++); } })},
                { 0x0E, new Opcode(0x0E, "LD C, ${0:x2}",        2,  8, new Step[] { () => { regs.C = mmu.Read8(regs.PC++); } })},
                { 0x16, new Opcode(0x16, "LD D, ${0:x2}",        2,  8, new Step[] { () => { regs.D = mmu.Read8(regs.PC++); } })},
                { 0x1E, new Opcode(0x1E, "LD E, ${0:x2}",        2,  8, new Step[] { () => { regs.E = mmu.Read8(regs.PC++); } })},
                { 0x26, new Opcode(0x26, "LD H, ${0:x2}",        2,  8, new Step[] { () => { regs.H = mmu.Read8(regs.PC++); } })},
                { 0x2E, new Opcode(0x2E, "LD L, ${0:x2}",        2,  8, new Step[] { () => { regs.L = mmu.Read8(regs.PC++); } })},
                { 0x3E, new Opcode(0x3E, "LD A, ${0:x2}",        2,  8, new Step[] { () => { regs.A = mmu.Read8(regs.PC++); } })},
                { 0x36, new Opcode(0x36, "LD (HL), ${0:x2}",     2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); },
                                                                             () => { mmu.Write8(regs.HL, operand8); }, })},
                // load direct value into register - 16 bit
                { 0x01, new Opcode(0x01, "LD BC, ${0:x4}",       3, 12, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); regs.BC = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0x11, new Opcode(0x11, "LD DE, ${0:x4}",       3, 12, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); regs.DE = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0x21, new Opcode(0x21, "LD HL, ${0:x4}",       3, 12, new Step[] {
                                                                             () => { if (OAM_Bug(regs.PC)) CorruptOAM(CorruptionType.LD_HL);
                                                                                     lsb = mmu.Read8(regs.PC++); },
                                                                             () => { if (OAM_Bug(regs.PC)) CorruptOAM(CorruptionType.LD_HL);
                                                                                     msb = mmu.Read8(regs.PC++);
                                                                                     regs.HL = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0x31, new Opcode(0x31, "LD SP, ${0:x4}",       3, 12, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); regs.SP = BitUtils.ToUnsigned16(msb, lsb); }, })},
                                                                         
                // load register to register
                { 0x40, new Opcode(0x40, "LD B, B",              1,  4, new Step[] { () => { } })},
                { 0x49, new Opcode(0x49, "LD C, C",              1,  4, new Step[] { () => { } })},
                { 0x52, new Opcode(0x52, "LD D, D",              1,  4, new Step[] { () => { } })},
                { 0x5B, new Opcode(0x5B, "LD E, E",              1,  4, new Step[] { () => { } })},
                { 0x64, new Opcode(0x64, "LD H, H",              1,  4, new Step[] { () => { } })},
                { 0x6D, new Opcode(0x6D, "LD L, L",              1,  4, new Step[] { () => { } })},
                { 0x7F, new Opcode(0x7F, "LD A, A",              1,  4, new Step[] { () => { } })},
                { 0x41, new Opcode(0x41, "LD B, C",              1,  4, new Step[] { () => { regs.B = regs.C; } })},
                { 0x42, new Opcode(0x42, "LD B, D",              1,  4, new Step[] { () => { regs.B = regs.D; } })},
                { 0x43, new Opcode(0x43, "LD B, E",              1,  4, new Step[] { () => { regs.B = regs.E; } })},
                { 0x44, new Opcode(0x44, "LD B, H",              1,  4, new Step[] { () => { regs.B = regs.H; } })},
                { 0x45, new Opcode(0x45, "LD B, L",              1,  4, new Step[] { () => { regs.B = regs.L; } })},
                { 0x47, new Opcode(0x47, "LD B, A",              1,  4, new Step[] { () => { regs.B = regs.A; } })},
                { 0x48, new Opcode(0x48, "LD C, B",              1,  4, new Step[] { () => { regs.C = regs.B; } })},
                { 0x4A, new Opcode(0x4A, "LD C, D",              1,  4, new Step[] { () => { regs.C = regs.D; } })},
                { 0x4B, new Opcode(0x4B, "LD C, E",              1,  4, new Step[] { () => { regs.C = regs.E; } })},
                { 0x4C, new Opcode(0x4C, "LD C, H",              1,  4, new Step[] { () => { regs.C = regs.H; } })},
                { 0x4D, new Opcode(0x4D, "LD C, L",              1,  4, new Step[] { () => { regs.C = regs.L; } })},
                { 0x4F, new Opcode(0x4F, "LD C, A",              1,  4, new Step[] { () => { regs.C = regs.A; } })},
                { 0x50, new Opcode(0x50, "LD D, B",              1,  4, new Step[] { () => { regs.D = regs.B; } })},
                { 0x51, new Opcode(0x51, "LD D, C",              1,  4, new Step[] { () => { regs.D = regs.C; } })},
                { 0x53, new Opcode(0x53, "LD D, E",              1,  4, new Step[] { () => { regs.D = regs.E; } })},
                { 0x54, new Opcode(0x54, "LD D, H",              1,  4, new Step[] { () => { regs.D = regs.H; } })},
                { 0x55, new Opcode(0x55, "LD D, L",              1,  4, new Step[] { () => { regs.D = regs.L; } })},
                { 0x57, new Opcode(0x57, "LD D, A",              1,  4, new Step[] { () => { regs.D = regs.A; } })},
                { 0x58, new Opcode(0x58, "LD E, B",              1,  4, new Step[] { () => { regs.E = regs.B; } })},
                { 0x59, new Opcode(0x59, "LD E, C",              1,  4, new Step[] { () => { regs.E = regs.C; } })},
                { 0x5A, new Opcode(0x5A, "LD E, D",              1,  4, new Step[] { () => { regs.E = regs.D; } })},
                { 0x5C, new Opcode(0x5C, "LD E, H",              1,  4, new Step[] { () => { regs.E = regs.H; } })},
                { 0x5D, new Opcode(0x5D, "LD E, L",              1,  4, new Step[] { () => { regs.E = regs.L; } })},
                { 0x5F, new Opcode(0x5F, "LD E, A",              1,  4, new Step[] { () => { regs.E = regs.A; } })},
                { 0x60, new Opcode(0x60, "LD H, B",              1,  4, new Step[] { () => { regs.H = regs.B; } })},
                { 0x61, new Opcode(0x61, "LD H, C",              1,  4, new Step[] { () => { regs.H = regs.C; } })},
                { 0x62, new Opcode(0x62, "LD H, D",              1,  4, new Step[] { () => { regs.H = regs.D; } })},
                { 0x63, new Opcode(0x63, "LD H, E",              1,  4, new Step[] { () => { regs.H = regs.E; } })},
                { 0x65, new Opcode(0x65, "LD H, L",              1,  4, new Step[] { () => { regs.H = regs.L; } })},
                { 0x67, new Opcode(0x67, "LD H, A",              1,  4, new Step[] { () => { regs.H = regs.A; } })},
                { 0x68, new Opcode(0x68, "LD L, B",              1,  4, new Step[] { () => { regs.L = regs.B; } })},
                { 0x69, new Opcode(0x69, "LD L, C",              1,  4, new Step[] { () => { regs.L = regs.C; } })},
                { 0x6A, new Opcode(0x6A, "LD L, D",              1,  4, new Step[] { () => { regs.L = regs.D; } })},
                { 0x6B, new Opcode(0x6B, "LD L, E",              1,  4, new Step[] { () => { regs.L = regs.E; } })},
                { 0x6C, new Opcode(0x6C, "LD L, H",              1,  4, new Step[] { () => { regs.L = regs.H; } })},
                { 0x6F, new Opcode(0x6F, "LD L, A",              1,  4, new Step[] { () => { regs.L = regs.A; } })},
                { 0x78, new Opcode(0x78, "LD A, B",              1,  4, new Step[] { () => { regs.A = regs.B; } })},
                { 0x79, new Opcode(0x79, "LD A, C",              1,  4, new Step[] { () => { regs.A = regs.C; } })},
                { 0x7A, new Opcode(0x7A, "LD A, D",              1,  4, new Step[] { () => { regs.A = regs.D; } })},
                { 0x7B, new Opcode(0x7B, "LD A, E",              1,  4, new Step[] { () => { regs.A = regs.E; } })},
                { 0x7C, new Opcode(0x7C, "LD A, H",              1,  4, new Step[] { () => { regs.A = regs.H; } })},
                { 0x7D, new Opcode(0x7D, "LD A, L",              1,  4, new Step[] { () => { regs.A = regs.L; } })},
                { 0x08, new Opcode(0x08, "LD (${0:x4}), SP",     3, 20, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); address16 = BitUtils.ToUnsigned16(msb, lsb); },
                                                                             () => { mmu.Write8(address16, BitUtils.Lsb(regs.SP)); },
                                                                             () => { mmu.Write8((u16)(address16 + 1), BitUtils.Msb(regs.SP)); }, })},

                { 0x02, new Opcode(0x02, "LD (BC), A",           1,  8, new Step[] { () => { mmu.Write8(regs.BC, regs.A); } })},
                { 0x12, new Opcode(0x12, "LD (DE), A",           1,  8, new Step[] { () => { mmu.Write8(regs.DE, regs.A); } })},
                { 0x0A, new Opcode(0x0A, "LD A, (BC)",           1,  8, new Step[] { () => { regs.A = mmu.Read8(regs.BC); } })},
                { 0x1A, new Opcode(0x1A, "LD A, (DE)",           1,  8, new Step[] { () => { regs.A = mmu.Read8(regs.DE); } })},
                { 0x22, new Opcode(0x22, "LD (HL+), A",          1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.A); regs.HL++; } })},
                { 0x32, new Opcode(0x32, "LD (HL-), A",          1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.A); regs.HL--; } })},
                { 0x2A, new Opcode(0x2A, "LD A, (HL+)",          1,  8, new Step[] { () => { if (OAM_Bug(regs.HL)) CorruptOAM(CorruptionType.LD_HL);
                                                                                             regs.A = mmu.Read8(regs.HL); regs.HL++; } })},
                { 0x3A, new Opcode(0x3A, "LD A, (HL-)",          1,  8, new Step[] { () => { if (OAM_Bug(regs.HL)) CorruptOAM(CorruptionType.LD_HL);
                                                                                             regs.A = mmu.Read8(regs.HL); regs.HL--; } })},
                { 0x46, new Opcode(0x46, "LD B, (HL)",           1,  8, new Step[] { () => { regs.B = mmu.Read8(regs.HL); } })},
                { 0x4E, new Opcode(0x4E, "LD C, (HL)",           1,  8, new Step[] { () => { regs.C = mmu.Read8(regs.HL); } })},
                { 0x56, new Opcode(0x56, "LD D, (HL)",           1,  8, new Step[] { () => { regs.D = mmu.Read8(regs.HL); } })},
                { 0x5E, new Opcode(0x5E, "LD E, (HL)",           1,  8, new Step[] { () => { regs.E = mmu.Read8(regs.HL); } })},
                { 0x66, new Opcode(0x66, "LD H, (HL)",           1,  8, new Step[] { () => { regs.H = mmu.Read8(regs.HL); } })},
                { 0x6E, new Opcode(0x6E, "LD L, (HL)",           1,  8, new Step[] { () => { regs.L = mmu.Read8(regs.HL); } })},
                { 0x7E, new Opcode(0x7E, "LD A, (HL)",           1,  8, new Step[] { () => { regs.A = mmu.Read8(regs.HL); } })},
                { 0x70, new Opcode(0x70, "LD (HL), B",           1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.B); } })},
                { 0x71, new Opcode(0x71, "LD (HL), C",           1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.C); } })},
                { 0x72, new Opcode(0x72, "LD (HL), D",           1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.D); } })},
                { 0x73, new Opcode(0x73, "LD (HL), E",           1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.E); } })},
                { 0x74, new Opcode(0x74, "LD (HL), H",           1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.H); } })},
                { 0x75, new Opcode(0x75, "LD (HL), L",           1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.L); } })},
                { 0x77, new Opcode(0x77, "LD (HL), A",           1,  8, new Step[] { () => { mmu.Write8(regs.HL, regs.A); } })},

                { 0xF8, new Opcode(0xF8, "LD HL, SP+${0:x2}",    2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); },
                                                                             () => { regs.HL = ADD_Signed8(regs.SP, operand8); }, })},
                { 0xF9, new Opcode(0xF9, "LD SP, HL",            1,  8, new Step[] { () => { regs.SP = regs.HL; } })},
                { 0xEA, new Opcode(0xEA, "LD (${0:x4}), A",      3, 16, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); },
                                                                             () => { mmu.Write8(BitUtils.ToUnsigned16(msb, lsb), regs.A); }, })},
                { 0xFA, new Opcode(0xFA, "LD A, (${0:x4})",      3, 16, new Step[] {
                                                                             () => { lsb = mmu.Read8(regs.PC++); },
                                                                             () => { msb = mmu.Read8(regs.PC++); },
                                                                             () => { regs.A = mmu.Read8(BitUtils.ToUnsigned16(msb, lsb)); }, })},                                                                 
                // LDH - Put memory address $FF00+n into A
                { 0xE0, new Opcode(0xE0, "LDH (${0:x2}), A",     2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); },
                                                                             () => { mmu.Write8((u16)(0xFF00 + operand8), regs.A); }, })},
                { 0xF0, new Opcode(0xF0, "LDH A, (${0:x2})",     2, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.PC++); },
                                                                             () => { regs.A = mmu.Read8((u16)(0xFF00 + operand8)); }, })},

                { 0xE2, new Opcode(0xE2, "LDH (C), A",           1,  8, new Step[] { () => { mmu.Write8((u16)(0xFF00 + regs.C), regs.A); } })},
                { 0xF2, new Opcode(0xF2, "LDH A, (C)",           1,  8, new Step[] { () => { regs.A = mmu.Read8((u16)(0xFF00 + regs.C)); } })},

                // ==================================================================================================================
                // STACK
                // ==================================================================================================================
                // Pop
                { 0xC1, new Opcode(0xC1, "POP BC",               1, 12, new Step[] {
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => {  if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++;
                                                                                     regs.BC = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0xD1, new Opcode(0xD1, "POP DE",               1, 12, new Step[] {
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++;
                                                                                   },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++;
                                                                                     regs.DE = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0xE1, new Opcode(0xE1, "POP HL",               1, 12, new Step[] {
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++;
                                                                                     regs.HL = BitUtils.ToUnsigned16(msb, lsb); }, })},
                { 0xF1, new Opcode(0xF1, "POP AF",               1, 12, new Step[] {
                                                                             () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_1);
                                                                                     lsb = mmu.Read8(regs.SP); regs.SP++; },
                                                                             () => {  if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.POP_2);
                                                                                     msb = mmu.Read8(regs.SP); regs.SP++;
                                                                                     regs.AF = BitUtils.ToUnsigned16(msb, lsb); }, })},                                                                     
                // Push
                { 0xC5, new Opcode(0xC5, "PUSH BC",              1, 16, new Step[] {
                                                                            () => { },
                                                                            () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                    regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.BC)); },
                                                                            () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                    regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.BC)); }, })},
                { 0xD5, new Opcode(0xD5, "PUSH DE",              1, 16, new Step[] {
                                                                            () => { },
                                                                            () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                    regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.DE)); },
                                                                            () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                    regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.DE)); }, })},
                { 0xE5, new Opcode(0xE5, "PUSH HL",              1, 16, new Step[] {
                                                                            () => { },
                                                                            () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                    regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.HL)); },
                                                                            () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                    regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.HL)); }, })},
                { 0xF5, new Opcode(0xF5, "PUSH AF",              1, 16, new Step[] {
                                                                            () => { },
                                                                            () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                                                                                    regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.AF)); },
                                                                            () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                                                                                    regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.AF)); }, })},
                                                                       
                // Rotate A left. Old bit 7 to Carry flag.
                { 0x07, new Opcode(0x07, "RLCA",                 1,  4, new Step[] { () => { regs.A = RLC(regs.A); regs.FlagZ = false; } })},
                // Rotate A left through Carry flag.
                { 0x17, new Opcode(0x17, "RLA",                  1,  4, new Step[] { () => { regs.A = RL(regs.A); regs.FlagZ = false; } })},
                // Rotate A right. Old bit 0 to Carry flag.
                { 0x0F, new Opcode(0x0F, "RRCA",                 1,  4, new Step[] { () => { regs.A = RRC(regs.A); regs.FlagZ = false; } })},
                // Rotate A right through Carry flag.
                { 0x1F, new Opcode(0x1F, "RRA",                  1,  4, new Step[] { () => { regs.A = RR(regs.A); regs.FlagZ = false; } })},

                // Restart - Push present address onto stack
                // Jump to address n
                { 0xC7, new Opcode(0xC7, "RST 00",               1, 16, RST_Steps(0x0)  )},
                { 0xCF, new Opcode(0xCF, "RST 08",               1, 16, RST_Steps(0x8)  )},
                { 0xD7, new Opcode(0xD7, "RST 10",               1, 16, RST_Steps(0x10) )},
                { 0xDF, new Opcode(0xDF, "RST 18",               1, 16, RST_Steps(0x18) )},
                { 0xE7, new Opcode(0xE7, "RST 20",               1, 16, RST_Steps(0x20) )},
                { 0xEF, new Opcode(0xEF, "RST 28",               1, 16, RST_Steps(0x28) )},
                { 0xF7, new Opcode(0xF7, "RST 30",               1, 16, RST_Steps(0x30) )},
                { 0xFF, new Opcode(0xFF, "RST 38",               1, 16, RST_Steps(0x38) )},

                // ==================================================================================================================
                // SUBTRACTION FAMILY
                // ==================================================================================================================
                // DEC 8 bit                                
                { 0x05, new Opcode(0x05, "DEC B",                1,  4, new Step[] { () => { regs.B = DEC(regs.B); } })},
                { 0x0D, new Opcode(0x0D, "DEC C",                1,  4, new Step[] { () => { regs.C = DEC(regs.C); } })},
                { 0x15, new Opcode(0x15, "DEC D",                1,  4, new Step[] { () => { regs.D = DEC(regs.D); } })},
                { 0x1D, new Opcode(0x1D, "DEC E",                1,  4, new Step[] { () => { regs.E = DEC(regs.E); } })},
                { 0x25, new Opcode(0x25, "DEC H",                1,  4, new Step[] { () => { regs.H = DEC(regs.H); } })},
                { 0x2D, new Opcode(0x2D, "DEC L",                1,  4, new Step[] { () => { regs.L = DEC(regs.L); } })},
                { 0x3D, new Opcode(0x3D, "DEC A",                1,  4, new Step[] { () => { regs.A = DEC(regs.A); } })},
                { 0x35, new Opcode(0x35, "DEC (HL)",             1, 12, new Step[] {
                                                                             () => { operand8 = mmu.Read8(regs.HL); },
                                                                             () => { mmu.Write8(regs.HL, DEC(operand8)); }, })},
                // DEC 16 bit                                  
                { 0x0B, new Opcode(0x0B, "DEC BC",               1,  8, new Step[] { () => { if (OAM_Bug(regs.BC)) CorruptOAM(CorruptionType.INC_DEC);
                                                                                             regs.BC--;
                                                                                           } })},
                { 0x1B, new Opcode(0x1B, "DEC DE",               1,  8, new Step[] { () => { if (OAM_Bug(regs.DE)) CorruptOAM(CorruptionType.INC_DEC);
                                                                                             regs.DE--;
                                                                                           } })},
                { 0x2B, new Opcode(0x2B, "DEC HL",               1,  8, new Step[] { () => { if (OAM_Bug(regs.HL)) CorruptOAM(CorruptionType.INC_DEC);
                                                                                             regs.HL--;
                                                                                           } })},
                { 0x3B, new Opcode(0x3B, "DEC SP",               1,  8, new Step[] { () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.INC_DEC);
                                                                                             regs.SP--;
                                                                                           } })},
                // Subtract value + Carry flag from A
                { 0x98, new Opcode(0x98, "SBC A, B",             1,  4, new Step[] { () => { SBC(regs.B); } })},
                { 0x99, new Opcode(0x99, "SBC A, C",             1,  4, new Step[] { () => { SBC(regs.C); } })},
                { 0x9A, new Opcode(0x9A, "SBC A, D",             1,  4, new Step[] { () => { SBC(regs.D); } })},
                { 0x9B, new Opcode(0x9B, "SBC A, E",             1,  4, new Step[] { () => { SBC(regs.E); } })},
                { 0x9C, new Opcode(0x9C, "SBC A, H",             1,  4, new Step[] { () => { SBC(regs.H); } })},
                { 0x9D, new Opcode(0x9D, "SBC A, L",             1,  4, new Step[] { () => { SBC(regs.L); } })},
                { 0x9F, new Opcode(0x9F, "SBC A, A",             1,  4, new Step[] { () => { SBC(regs.A); } })},
                { 0x9E, new Opcode(0x9E, "SBC A, (HL)",          1,  8, new Step[] { () => { SBC(mmu.Read8(regs.HL)); } })},
                { 0xDE, new Opcode(0xDE, "SBC A, ${0:x2}",       2,  8, new Step[] { () => { SBC(mmu.Read8(regs.PC++)); } })},
                // Subtract value from A
                { 0x90, new Opcode(0x90, "SUB A, B",             1,  4, new Step[] { () => { SUB(regs.B); } })},
                { 0x91, new Opcode(0x91, "SUB A, C",             1,  4, new Step[] { () => { SUB(regs.C); } })},
                { 0x92, new Opcode(0x92, "SUB A, D",             1,  4, new Step[] { () => { SUB(regs.D); } })},
                { 0x93, new Opcode(0x93, "SUB A, E",             1,  4, new Step[] { () => { SUB(regs.E); } })},
                { 0x94, new Opcode(0x94, "SUB A, H",             1,  4, new Step[] { () => { SUB(regs.H); } })},
                { 0x95, new Opcode(0x95, "SUB A, L",             1,  4, new Step[] { () => { SUB(regs.L); } })},
                { 0x97, new Opcode(0x97, "SUB A, A",             1,  4, new Step[] { () => { SUB(regs.A); } })},
                { 0x96, new Opcode(0x96, "SUB A, (HL)",          1,  8, new Step[] { () => { SUB(mmu.Read8(regs.HL)); } })},
                { 0xD6, new Opcode(0xD6, "SUB ${0:x2}",          2,  8, new Step[] { () => { SUB(mmu.Read8(regs.PC++)); } })},

                // ==================================================================================================================
                // MISC
                // ==================================================================================================================
                { 0x00, new Opcode(0x00, "NOP",                  1,  4, new Step[] { () => { } })},
                // 
                { 0x10, new Opcode(0x10, "STOP",                 1,  4, new Step[] { () => { STOP(); } })},
                //
                { 0xCB, new Opcode(0xCB, "CB PREFIX",            1,  4, new Step[] { () => { } })},
                // CPL - Complement A register. (Flip all b }its.)
                { 0x2F, new Opcode(0x2F, "CPL",                  1,  4, new Step[] { () => { regs.A = (u8) ~regs.A; regs.FlagN = true; regs.FlagH = true; } })},
                // Decimal adjust register A. This instruction adjusts register A so that the correct representation of Binary Coded Decimal (BCD) is obtained.
                { 0x27, new Opcode(0x27, "DAA",                  1,  4, new Step[] { () => { DAA(); } })},
                // Set carry flag
                { 0x37, new Opcode(0x37, "SCF",                  1,  4, new Step[] { () => { regs.FlagN = false; regs.FlagH = false; regs.FlagC = true; } })},
                // Complement carry flag
                { 0x3F, new Opcode(0x3F, "CCF",                  1,  4, new Step[] { () => { regs.FlagC = !regs.FlagC; regs.FlagN = false; regs.FlagH = false; } })},
                // Halt CPU & LCD display until button pressed.
                { 0x76, new Opcode(0x76, "HALT",                 1,  4, new Step[] { () => { } })},

            };
        }

        private void STOP() {
            throw new NotImplementedException();
        }

        private Step[] RST_Steps(u16 address) {
            // the two first steps are equivalent to a PUSH, but done step by step
            return new Step[] {
                () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_1);
                        regs.SP--; mmu.Write8(regs.SP, BitUtils.Msb(regs.PC)); },
                () => { if (OAM_Bug(regs.SP)) CorruptOAM(CorruptionType.PUSH_2);
                        regs.SP--; mmu.Write8(regs.SP, BitUtils.Lsb(regs.PC)); },
                () => { regs.PC = address; },
            };
        }

        public static int ToSigned(u8 rawValue) {
            // If a positive value, return it
            if ((rawValue & 0b_1000_0000) == 0) {
                return rawValue;
            }

            // Otherwise perform the 2's complement math on the value
            return (byte)(~(rawValue - 0b_0000_0001)) * -1;
        }

        private bool HasCarry(int value) {
            return (value >> 8) != 0;
        }

        public void AND(u8 value) {
            regs.A = (u8)(regs.A & value);
            regs.FlagZ = (regs.A == 0);
            regs.FlagN = false;
            regs.FlagH = true;
            regs.FlagC = false;
        }

        public void OR(u8 value) {
            regs.A = (u8)(regs.A | value);
            regs.FlagZ = (regs.A == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = false;
        }

        public void XOR(u8 value) {
            regs.A = (u8)(regs.A ^ value);
            regs.FlagZ = (regs.A == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = false;
        }

        private void ADD(u8 value) {
            int result = regs.A + value;
            regs.FlagZ = (result & 0b_1111_1111) == 0;
            regs.FlagN = false;
            regs.FlagH = (regs.A & 0b_0000_1111) + (value & 0b_0000_1111) > 0b_0000_1111;
            regs.FlagC = HasCarry(result);
            regs.A = (u8)result;
        }

        // 16 bit Add, H - Set if carry from bit 11, C - Set if carry from bit 15. 
        private void ADD_HL(u16 value) {
            int result = regs.HL + value;
            u16 mask = 0b_0000_1111_1111_1111;
            // regs.FlagZ = Unmodified
            regs.FlagN = false;
            regs.FlagH = ((regs.HL & mask) + (value & mask)) > mask;
            regs.FlagC = result >> 16 != 0;
            regs.HL = (u16)result;
        }

        // n = one byte signed immediate value(#).
        private u16 ADD_Signed8(u16 value16, u8 value8) {
            regs.FlagZ = false;
            regs.FlagN = false;
            regs.FlagH = ((value16 & 0xF) + (value8 & 0xF)) > 0xF;
            regs.FlagC = HasCarry((byte)value16 + value8);
            return (ushort)(value16 + (sbyte)value8);
        }

        private void ADC(u8 value) {
            var carry = regs.FlagC ? 1 : 0;
            int result = (regs.A + value + carry) & 0b_1111_1111;
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = (regs.A & 0b_0000_1111) + (value & 0b_0000_1111) + carry > 0b_0000_1111;
            regs.FlagC = HasCarry(regs.A + value + carry);
            regs.A = (u8)result;
        }

        private void SUB(u8 value) {
            int result = regs.A - value;
            regs.FlagZ = (result == 0);
            regs.FlagN = true;
            regs.FlagH = (value & 0b_0000_1111) > (regs.A & 0b_0000_1111);
            regs.FlagC = HasCarry(result);
            regs.A = (u8)result;
        }

        // H = Set if no borrow from bit 4, C=Set if no borrow
        private void SBC(u8 value) {
            var carry = regs.FlagC ? 1 : 0;
            int result = regs.A - value - carry;
            regs.FlagZ = (result & 0b_1111_1111) == 0;
            regs.FlagN = true;
            regs.FlagH = (((regs.A ^ value ^ (result & 0b_1111_1111)) & (1 << 4)) != 0);
            regs.FlagC = (result < 0);
            regs.A = (u8)result;
        }

        private void CP(u8 value) {
            int result = regs.A - value;
            regs.FlagZ = (result == 0);
            regs.FlagN = true;
            regs.FlagH = (0b_0000_1111 & value) > (0b_0000_1111 & regs.A);
            regs.FlagC = HasCarry(result);
        }

        private u8 INC(u8 value) {
            u8 result = (u8)(value + 1);
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = (value & 0b_0000_1111) == 0b_0000_1111;
            // regs.FlagC -> unmodified
            return result;
        }

        private u8 DEC(u8 value) {
            u8 result = (u8)(value - 1);
            regs.FlagZ = (result == 0);
            regs.FlagN = true;
            regs.FlagH = (value & 0b_0000_1111) == 0b_0000_0000;
            // r.FlagC -> unmodified
            return result;
        }

        private u8 RLC(u8 value) {
            byte result = (byte)((value << 1) | (value >> 7));
            regs.FlagZ = (result == 0);
            regs.FlagN = false;
            regs.FlagH = false;
            regs.FlagC = (value & (0b_0000_0001 << 7)) == (0b_0000_0001 << 7);
            return result;
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

        private void DAA() {
            if (regs.FlagN) {
                if (regs.FlagC) {
                    regs.A -= 0x60;
                }
                if (regs.FlagH) {
                    regs.A -= 0x6;
                }
            }
            else {
                if (regs.FlagC || (regs.A > 0x99)) {
                    regs.A += 0x60;
                    regs.FlagC = true;
                }
                if (regs.FlagH || (regs.A & 0xF) > 0x9) {
                    regs.A += 0x6;
                }
            }
            regs.FlagZ = (regs.A == 0);
            regs.FlagH = false;

        }

        private bool OAM_Bug(u16 value) {
            if (value >= 0xfe00 && value <= 0xfeff) {

                if (!gpu.IsLcdEnabled()) {
                    return false;
                }

                var stat = gpu.STAT;
                if ((stat & 0b11) == GPU.MODE_SCANLINE_OAM && gpu.lineTicks < 79) {
                    return true;
                }
            }

            return false;
        }

        public enum CorruptionType {
            INC_DEC,
            POP_1,
            POP_2,
            PUSH_1,
            PUSH_2,
            LD_HL
        }

        public void CorruptOAM(CorruptionType type) {

            var cpuCycle = (gpu.lineTicks + 1) / 4 + 1;
            switch (type) {
                case CorruptionType.INC_DEC:
                    if (cpuCycle >= 2) {
                        CopyValues(mmu, (cpuCycle - 2) * 8 + 2, (cpuCycle - 1) * 8 + 2, 6);
                    }
                    break;

                case CorruptionType.POP_1:
                    if (cpuCycle >= 4) {
                        CopyValues(mmu, (cpuCycle - 3) * 8 + 2, (cpuCycle - 4) * 8 + 2, 8);
                        CopyValues(mmu, (cpuCycle - 3) * 8 + 8, (cpuCycle - 4) * 8 + 0, 2);
                        CopyValues(mmu, (cpuCycle - 4) * 8 + 2, (cpuCycle - 2) * 8 + 2, 6);
                    }
                    break;

                case CorruptionType.POP_2:
                    if (cpuCycle >= 5) {
                        CopyValues(mmu, (cpuCycle - 5) * 8 + 0, (cpuCycle - 2) * 8 + 0, 8);
                    }
                    break;

                case CorruptionType.PUSH_1:
                    if (cpuCycle >= 4) {
                        CopyValues(mmu, (cpuCycle - 4) * 8 + 2, (cpuCycle - 3) * 8 + 2, 8);
                        CopyValues(mmu, (cpuCycle - 3) * 8 + 2, (cpuCycle - 1) * 8 + 2, 6);
                    }
                    break;

                case CorruptionType.PUSH_2:
                    if (cpuCycle >= 5) {
                        CopyValues(mmu, (cpuCycle - 4) * 8 + 2, (cpuCycle - 3) * 8 + 2, 8);
                    }
                    break;

                case CorruptionType.LD_HL:
                    if (cpuCycle >= 4) {
                        CopyValues(mmu, (cpuCycle - 3) * 8 + 2, (cpuCycle - 4) * 8 + 2, 8);
                        CopyValues(mmu, (cpuCycle - 3) * 8 + 8, (cpuCycle - 4) * 8 + 0, 2);
                        CopyValues(mmu, (cpuCycle - 4) * 8 + 2, (cpuCycle - 2) * 8 + 2, 6);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static void CopyValues(MMU mmu, int from, int to, int length) {
            for (var i = length - 1; i >= 0; i--) {
                var b = mmu.Read8((u16)(0xfe00 + from + i)) % 0xff;
                mmu.Write8((u16)(0xfe00 + to + i), (u8)b);
            }
        }
    }
}
