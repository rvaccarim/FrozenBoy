using System;
using System.Diagnostics;
using System.Collections.Generic;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Net;
using System.Text.Json;

namespace FrozenBoyCore {

    public class CPU {
        public Registers regs;
        public Memory mem;

        public Dictionary<u8, Opcode> opcodes;
        public Dictionary<u8, Opcode> cbOpcodes;

        // Opcode related stuff
        public Opcode opcode;
        public bool handledPC;


        public CPU(Memory memory) {
            regs = new Registers();
            this.mem = memory;
            regs.PC = 0;

            opcodes = InitializeOpcodes();
            cbOpcodes = InitializeCB();
        }

        public void Execute() {
            handledPC = false;
            opcode = Disassemble();

            if (opcode != null) {
                opcode.logic.Invoke();

                // move to the next one
                if (!handledPC) {
                    regs.PC = (u16)(regs.PC + opcode.length);
                }
            }
            else {
                System.Environment.Exit(0);
            }
        }


        public Opcode Disassemble() {
            u8 opcodeValue = mem.data[regs.PC];

            if (opcodes.ContainsKey(opcodeValue)) {

                opcode = opcodes[opcodeValue];
                if (opcode.value != 0xCB) {
                    return opcode;
                }
                else {
                    u8 cbOpcodeValue = mem.data[regs.PC + 1];

                    if (cbOpcodes.ContainsKey(cbOpcodeValue)) {
                        return cbOpcodes[cbOpcodeValue];
                    }
                    else {
                        Debug.WriteLine(String.Format("Unsupported cb_opcode: {0:x2}", cbOpcodeValue));
                    }
                }
            }
            else {
                Debug.WriteLine(String.Format("Unsupported opcode: {0:x2}", opcodeValue));
            }

            return null;
        }

        private Dictionary<byte, Opcode> InitializeOpcodes() {
            return new Dictionary<byte, Opcode> {
                { 0xCB, new Opcode(0xCB, "CB PREFIX",            1,  4, () => { })},

                { 0x88, new Opcode(0x88, "ADC A, B",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x89, new Opcode(0x89, "ADC A, C",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x8A, new Opcode(0x8A, "ADC A, D",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x8B, new Opcode(0x8B, "ADC A, E",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x8C, new Opcode(0x8C, "ADC A, H",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x8D, new Opcode(0x8D, "ADC A, L",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x8E, new Opcode(0x8E, "ADC A, (HL)",          1,  8, () => {throw new NotImplementedException(); })},
                { 0x8F, new Opcode(0x8F, "ADC A, A",             1,  4, () => {throw new NotImplementedException(); })},
                { 0xCE, new Opcode(0xCE, "ADC A, ${0:x2}",       2,  8, () => {throw new NotImplementedException(); })},
                { 0x09, new Opcode(0x09, "ADD HL, BC",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x19, new Opcode(0x19, "ADD HL, DE",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x29, new Opcode(0x29, "ADD HL, HL",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x39, new Opcode(0x39, "ADD HL, SP",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x80, new Opcode(0x80, "ADD A, B",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x81, new Opcode(0x81, "ADD A, C",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x82, new Opcode(0x82, "ADD A, D",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x83, new Opcode(0x83, "ADD A, E",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x84, new Opcode(0x84, "ADD A, H",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x85, new Opcode(0x85, "ADD A, L",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x86, new Opcode(0x86, "ADD A, (HL)",          1,  8, () => {throw new NotImplementedException(); })},
                { 0x87, new Opcode(0x87, "ADD A, A",             1,  4, () => {throw new NotImplementedException(); })},
                { 0xC6, new Opcode(0xC6, "ADD A, ${0:x2}",       2,  8, () => {throw new NotImplementedException(); })},
                { 0xE8, new Opcode(0xE8, "ADD SP, ${0:x2}",      2, 16, () => {throw new NotImplementedException(); })},


                { 0xA0, new Opcode(0xA0, "AND B",                1,  4, () => { AND(regs.B); })},
                { 0xA1, new Opcode(0xA1, "AND C",                1,  4, () => { AND(regs.C); })},
                { 0xA2, new Opcode(0xA2, "AND D",                1,  4, () => { AND(regs.D); })},
                { 0xA3, new Opcode(0xA3, "AND E",                1,  4, () => { AND(regs.E); })},
                { 0xA4, new Opcode(0xA4, "AND H",                1,  4, () => { AND(regs.H); })},
                { 0xA5, new Opcode(0xA5, "AND L",                1,  4, () => { AND(regs.L); })},
                { 0xA6, new Opcode(0xA6, "AND (HL)",             1,  8, () => { AND(mem.Read8(regs.HL)); })},
                { 0xA7, new Opcode(0xA7, "AND A",                1,  4, () => { AND(regs.A); })},
                { 0xE6, new Opcode(0xE6, "AND ${0:x2}",          2,  8, () => { AND(mem.ReadParm8(regs.PC)); })},

                { 0xC4, new Opcode(0xC4, "CALL NZ, ${0:x4}",     3, 24, () => {throw new NotImplementedException(); })},
                { 0xCC, new Opcode(0xCC, "CALL Z, ${0:x4}",      3, 24, () => {throw new NotImplementedException(); })},
                { 0xCD, new Opcode(0xCD, "CALL ${0:x4}",         3, 24, () => {throw new NotImplementedException(); })},
                { 0xD4, new Opcode(0xD4, "CALL NC, ${0:x4}",     3, 24, () => {throw new NotImplementedException(); })},
                { 0xDC, new Opcode(0xDC, "CALL C, ${0:x4}",      3, 24, () => {throw new NotImplementedException(); })},
                { 0x3F, new Opcode(0x3F, "CCF",                  1,  4, () => {throw new NotImplementedException(); })},
                { 0xB8, new Opcode(0xB8, "CP B",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0xB9, new Opcode(0xB9, "CP C",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0xBA, new Opcode(0xBA, "CP D",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0xBB, new Opcode(0xBB, "CP E",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0xBC, new Opcode(0xBC, "CP H",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0xBD, new Opcode(0xBD, "CP L",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0xBE, new Opcode(0xBE, "CP (HL)",              1,  8, () => {throw new NotImplementedException(); })},
                { 0xBF, new Opcode(0xBF, "CP A",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0xFE, new Opcode(0xFE, "CP ${0:x2}",           2,  8, () => {throw new NotImplementedException(); })},
                { 0x2F, new Opcode(0x2F, "CPL",                  1,  4, () => {throw new NotImplementedException(); })},
                { 0x27, new Opcode(0x27, "DAA",                  1,  4, () => {throw new NotImplementedException(); })},
                                                             
                // DEC - 8 bit                                
                { 0x05, new Opcode(0x05, "DEC B",                1,  4, () => { regs.B = DEC(regs.B); })},
                { 0x0D, new Opcode(0x0D, "DEC C",                1,  4, () => { regs.C = DEC(regs.C); })},
                { 0x15, new Opcode(0x15, "DEC D",                1,  4, () => { regs.D = DEC(regs.D); })},
                { 0x1D, new Opcode(0x1D, "DEC E",                1,  4, () => { regs.E = DEC(regs.E); })},
                { 0x25, new Opcode(0x25, "DEC H",                1,  4, () => { regs.H = DEC(regs.H); })},
                { 0x2D, new Opcode(0x2D, "DEC L",                1,  4, () => { regs.L = DEC(regs.L); })},
                { 0x35, new Opcode(0x35, "DEC (HL)",             1, 12, () => { mem.Write8(regs.HL, DEC(mem.Read8(regs.HL))); })},
                { 0x3D, new Opcode(0x3D, "DEC A",                1,  4, () => { regs.A = DEC(regs.A); })},
                                                             
                // DEC XX                                        
                { 0x0B, new Opcode(0x0B, "DEC BC",               1,  8, () => { regs.BC--; })},
                { 0x1B, new Opcode(0x1B, "DEC DE",               1,  8, () => { regs.DE--; })},
                { 0x2B, new Opcode(0x2B, "DEC HL",               1,  8, () => { regs.HL--; })},
                { 0x3B, new Opcode(0x3B, "DEC SP",               1,  8, () => { regs.SP--; })},


                { 0xF3, new Opcode(0xF3, "DI",                   1,  4, () => {throw new NotImplementedException(); })},
                { 0xFB, new Opcode(0xFB, "EI",                   1,  4, () => {throw new NotImplementedException(); })},
                { 0x76, new Opcode(0x76, "HALT",                 1,  4, () => {throw new NotImplementedException(); })},
                                                             
                // INC - 8 bit                                  
                { 0x04, new Opcode(0x04, "INC B",                1,  4, () => { regs.B = INC(regs.B); })},
                { 0x0C, new Opcode(0x0C, "INC C",                1,  4, () => { regs.C = INC(regs.C); })},
                { 0x14, new Opcode(0x14, "INC D",                1,  4, () => { regs.D = INC(regs.D); })},
                { 0x1C, new Opcode(0x1C, "INC E",                1,  4, () => { regs.E = INC(regs.E); })},
                { 0x24, new Opcode(0x24, "INC H",                1,  4, () => { regs.H = INC(regs.H); })},
                { 0x2C, new Opcode(0x2C, "INC L",                1,  4, () => { regs.L = INC(regs.L); })},
                { 0x34, new Opcode(0x34, "INC (HL)",             1, 12, () => { mem.Write8(regs.HL, INC(mem.Read8(regs.HL))); })},
                { 0x3C, new Opcode(0x3C, "INC A",                1,  4, () => { regs.A = INC(regs.A); })},

                // INC - 16 bit
                { 0x03, new Opcode(0x03, "INC BC",               1,  8, () => { regs.BC++; })},
                { 0x13, new Opcode(0x13, "INC DE",               1,  8, () => { regs.DE++; })},
                { 0x23, new Opcode(0x23, "INC HL",               1,  8, () => { regs.HL++; })},
                { 0x33, new Opcode(0x33, "INC SP",               1,  8, () => { regs.SP++; })},

                // JP - Jump to location
                { 0xC3, new Opcode(0xC3, "JP ${0:x4}",           3, 16, () => { JP(mem.ReadParm16(regs.PC)); })},
                { 0xE9, new Opcode(0xE9, "JP (HL)",              1,  4, () => { JP(regs.HL); })},
                { 0xC2, new Opcode(0xC2, "JP NZ, ${0:x4}",       3, 16, () => { JP_FLAG(!regs.FlagZ, mem.ReadParm16(regs.PC)); })},
                { 0xCA, new Opcode(0xCA, "JP Z, ${0:x4}",        3, 16, () => { JP_FLAG( regs.FlagZ, mem.ReadParm16(regs.PC)); })},
                { 0xD2, new Opcode(0xD2, "JP NC, ${0:x4}",       3, 16, () => { JP_FLAG(!regs.FlagC, mem.ReadParm16(regs.PC)); })},
                { 0xDA, new Opcode(0xDA, "JP C, ${0:x4}",        3, 16, () => { JP_FLAG( regs.FlagC, mem.ReadParm16(regs.PC)); })},

                // Jump to location relative to the current location
                { 0x18, new Opcode(0x18, "JR ${0:x2}",           2, 12, () => { JR(mem.ReadParm8(regs.PC)); })},
                { 0x20, new Opcode(0x20, "JR NZ, ${0:x2}",       2, 12, () => { JR_FLAG(!regs.FlagZ, mem.ReadParm8(regs.PC)); })},
                { 0x28, new Opcode(0x28, "JR Z, ${0:x2}",        2, 12, () => { JR_FLAG( regs.FlagZ, mem.ReadParm8(regs.PC)); })},
                { 0x30, new Opcode(0x30, "JR NC, ${0:x2}",       2, 12, () => { JR_FLAG(!regs.FlagC, mem.ReadParm8(regs.PC)); })},
                { 0x38, new Opcode(0x38, "JR C, ${0:x2}",        2, 12, () => { JR_FLAG( regs.FlagC, mem.ReadParm8(regs.PC)); })},

                // load direct value into register - 8 bit
                { 0x06, new Opcode(0x06, "LD B, ${0:x2}",        2,  8, () => { regs.B = mem.ReadParm8(regs.PC); })},
                { 0x0E, new Opcode(0x0E, "LD C, ${0:x2}",        2,  8, () => { regs.C = mem.ReadParm8(regs.PC); })},
                { 0x16, new Opcode(0x16, "LD D, ${0:x2}",        2,  8, () => { regs.D = mem.ReadParm8(regs.PC); })},
                { 0x1E, new Opcode(0x1E, "LD E, ${0:x2}",        2,  8, () => { regs.E = mem.ReadParm8(regs.PC); })},
                { 0x26, new Opcode(0x26, "LD H, ${0:x2}",        2,  8, () => { regs.H = mem.ReadParm8(regs.PC); })},
                { 0x2E, new Opcode(0x2E, "LD L, ${0:x2}",        2,  8, () => { regs.L = mem.ReadParm8(regs.PC); })},
                { 0x36, new Opcode(0x36, "LD (HL), ${0:x2}",     2, 12, () => { mem.Write8(regs.HL, mem.ReadParm8(regs.PC)); })},
                { 0x3E, new Opcode(0x3E, "LD A, ${0:x2}",        2,  8, () => { regs.A = mem.ReadParm8(regs.PC); })},
             
                // load direct value into register - 16 bit
                { 0x01, new Opcode(0x01, "LD BC, ${0:x4}",       3, 12, () => { regs.BC = mem.ReadParm16(regs.PC); })},
                { 0x11, new Opcode(0x11, "LD DE, ${0:x4}",       3, 12, () => { regs.DE = mem.ReadParm16(regs.PC); })},
                { 0x21, new Opcode(0x21, "LD HL, ${0:x4}",       3, 12, () => { regs.HL = mem.ReadParm16(regs.PC); })},
                { 0x31, new Opcode(0x31, "LD SP, ${0:x4}",       3, 12, () => { regs.SP = mem.ReadParm16(regs.PC); })},


                { 0x02, new Opcode(0x02, "LD (BC), A",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x08, new Opcode(0x08, "LD (${0:x4}), SP",     3, 20, () => {throw new NotImplementedException(); })},
                { 0x0A, new Opcode(0x0A, "LD A, (BC)",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x12, new Opcode(0x12, "LD (DE), A",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x1A, new Opcode(0x1A, "LD A, (DE)",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x22, new Opcode(0x22, "LD (HL+), A",          1,  8, () => {throw new NotImplementedException(); })},
                { 0x2A, new Opcode(0x2A, "LD A, (HL+)",          1,  8, () => {throw new NotImplementedException(); })},
                { 0x32, new Opcode(0x32, "LD (HL-), A",          1,  8, () => { mem.Write8(regs.HL, regs.A);  regs.HL--; })},
                { 0x3A, new Opcode(0x3A, "LD A, (HL-)",          1,  8, () => {throw new NotImplementedException(); })},
                { 0x41, new Opcode(0x41, "LD B, C",              1,  4, () => { regs.B = regs.C; })},
                { 0x40, new Opcode(0x40, "LD B, B",              1,  4, () => { })},
                { 0x42, new Opcode(0x42, "LD B, D",              1,  4, () => { regs.B = regs.D; })},
                { 0x43, new Opcode(0x43, "LD B, E",              1,  4, () => { regs.B = regs.E; })},
                { 0x44, new Opcode(0x44, "LD B, H",              1,  4, () => { regs.B = regs.H; })},
                { 0x45, new Opcode(0x45, "LD B, L",              1,  4, () => { regs.B = regs.L; })},
                { 0x46, new Opcode(0x46, "LD B, (HL)",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x47, new Opcode(0x47, "LD B, A",              1,  4, () => {regs.B = regs.A; })},
                { 0x48, new Opcode(0x48, "LD C, B",              1,  4, () => {regs.C = regs.B; })},
                { 0x49, new Opcode(0x49, "LD C, C",              1,  4, () => { })},
                { 0x4A, new Opcode(0x4A, "LD C, D",              1,  4, () => {regs.C = regs.D; })},
                { 0x4B, new Opcode(0x4B, "LD C, E",              1,  4, () => {regs.C = regs.E; })},
                { 0x4C, new Opcode(0x4C, "LD C, H",              1,  4, () => {regs.C = regs.H; })},
                { 0x4D, new Opcode(0x4D, "LD C, L",              1,  4, () => {regs.C = regs.L; })},
                { 0x4E, new Opcode(0x4E, "LD C, (HL)",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x4F, new Opcode(0x4F, "LD C, A",              1,  4, () => { regs.C = regs.A; })},
                { 0x50, new Opcode(0x50, "LD D, B",              1,  4, () => { regs.D = regs.B; })},
                { 0x51, new Opcode(0x51, "LD D, C",              1,  4, () => { regs.D = regs.C; })},
                { 0x52, new Opcode(0x52, "LD D, D",              1,  4, () => { })},
                { 0x53, new Opcode(0x53, "LD D, E",              1,  4, () => { regs.D = regs.E; })},
                { 0x54, new Opcode(0x54, "LD D, H",              1,  4, () => { regs.D = regs.H; })},
                { 0x55, new Opcode(0x55, "LD D, L",              1,  4, () => { regs.D = regs.L; })},
                { 0x56, new Opcode(0x56, "LD D, (HL)",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x57, new Opcode(0x57, "LD D, A",              1,  4, () => { regs.D = regs.A; })},
                { 0x58, new Opcode(0x58, "LD E, B",              1,  4, () => { regs.E = regs.B; })},
                { 0x59, new Opcode(0x59, "LD E, C",              1,  4, () => { regs.E = regs.C; })},
                { 0x5A, new Opcode(0x5A, "LD E, D",              1,  4, () => { regs.E = regs.D; })},
                { 0x5B, new Opcode(0x5B, "LD E, E",              1,  4, () => { })},
                { 0x5C, new Opcode(0x5C, "LD E, H",              1,  4, () => { regs.E = regs.H; })},
                { 0x5D, new Opcode(0x5D, "LD E, L",              1,  4, () => { regs.E = regs.L; })},
                { 0x5E, new Opcode(0x5E, "LD E, (HL)",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x5F, new Opcode(0x5F, "LD E, A",              1,  4, () => { regs.E = regs.A; })},
                { 0x60, new Opcode(0x60, "LD H, B",              1,  4, () => { regs.H = regs.B; })},
                { 0x61, new Opcode(0x61, "LD H, C",              1,  4, () => { regs.H = regs.C; })},
                { 0x62, new Opcode(0x62, "LD H, D",              1,  4, () => { regs.H = regs.D; })},
                { 0x63, new Opcode(0x63, "LD H, E",              1,  4, () => { regs.H = regs.E; })},
                { 0x64, new Opcode(0x64, "LD H, H",              1,  4, () => { })},
                { 0x65, new Opcode(0x65, "LD H, L",              1,  4, () => { regs.H = regs.L; })},
                { 0x66, new Opcode(0x66, "LD H, (HL)",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x67, new Opcode(0x67, "LD H, A",              1,  4, () => { regs.H = regs.A; })},
                { 0x68, new Opcode(0x68, "LD L, B",              1,  4, () => { regs.L = regs.B; })},
                { 0x69, new Opcode(0x69, "LD L, C",              1,  4, () => { regs.L = regs.C; })},
                { 0x6A, new Opcode(0x6A, "LD L, D",              1,  4, () => { regs.L = regs.D; })},
                { 0x6B, new Opcode(0x6B, "LD L, E",              1,  4, () => { regs.L = regs.E; })},
                { 0x6C, new Opcode(0x6C, "LD L, H",              1,  4, () => { regs.L = regs.H; })},
                { 0x6D, new Opcode(0x6D, "LD L, L",              1,  4, () => {  })},
                { 0x6E, new Opcode(0x6E, "LD L, (HL)",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x6F, new Opcode(0x6F, "LD L, A",              1,  4, () => { regs.L = regs.A; })},
                { 0x70, new Opcode(0x70, "LD (HL), B",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x71, new Opcode(0x71, "LD (HL), C",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x72, new Opcode(0x72, "LD (HL), D",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x73, new Opcode(0x73, "LD (HL), E",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x74, new Opcode(0x74, "LD (HL), H",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x75, new Opcode(0x75, "LD (HL), L",           1,  8, () => {throw new NotImplementedException(); })},
                { 0x77, new Opcode(0x77, "LD (HL), A",           1,  8, () => {throw new NotImplementedException(); })},

                { 0x78, new Opcode(0x78, "LD A, B",              1,  4, () => { regs.A = regs.B; })},
                { 0x79, new Opcode(0x79, "LD A, C",              1,  4, () => { regs.A = regs.C; })},
                { 0x7A, new Opcode(0x7A, "LD A, D",              1,  4, () => { regs.A = regs.D; })},
                { 0x7B, new Opcode(0x7B, "LD A, E",              1,  4, () => { regs.A = regs.E; })},
                { 0x7C, new Opcode(0x7C, "LD A, H",              1,  4, () => { regs.A = regs.H; })},
                { 0x7D, new Opcode(0x7D, "LD A, L",              1,  4, () => { regs.A = regs.L; })},
                { 0x7E, new Opcode(0x7E, "LD A, (HL)",           1,  8, () => { throw new NotImplementedException(); })},
                { 0x7F, new Opcode(0x7F, "LD A, A",              1,  4, () => { })}, // A = A is do nothing

                { 0xEA, new Opcode(0xEA, "LD (${0:x4}), A",      3, 16, () => {throw new NotImplementedException(); })},
                { 0xF8, new Opcode(0xF8, "LD HL, SP+${0:x2}",    2, 12, () => {throw new NotImplementedException(); })},
                { 0xF9, new Opcode(0xF9, "LD SP, HL",            1,  8, () => {throw new NotImplementedException(); })},
                { 0xFA, new Opcode(0xFA, "LD A, (${0:x4})",      3, 16, () => {throw new NotImplementedException(); })},
                { 0xE0, new Opcode(0xE0, "LDH (${0:x2}), A",     2, 12, () => {throw new NotImplementedException(); })},
                { 0xE2, new Opcode(0xE2, "LDH (C), A",           1,  8, () => {throw new NotImplementedException(); })},
                { 0xF0, new Opcode(0xF0, "LDH A, (${0:x2})",     2, 12, () => {throw new NotImplementedException(); })},
                { 0xF2, new Opcode(0xF2, "LDH A, (C)",           1,  8, () => {throw new NotImplementedException(); })},

                { 0x00, new Opcode(0x00, "NOP",                  1,  4, () => { })},

                { 0xB0, new Opcode(0xB0, "OR B",                 1,  4, () => { OR(regs.B); })},
                { 0xB1, new Opcode(0xB1, "OR C",                 1,  4, () => { OR(regs.C); })},
                { 0xB2, new Opcode(0xB2, "OR D",                 1,  4, () => { OR(regs.D); })},
                { 0xB3, new Opcode(0xB3, "OR E",                 1,  4, () => { OR(regs.E); })},
                { 0xB4, new Opcode(0xB4, "OR H",                 1,  4, () => { OR(regs.H); })},
                { 0xB5, new Opcode(0xB5, "OR L",                 1,  4, () => { OR(regs.L); })},
                { 0xB6, new Opcode(0xB6, "OR (HL)",              1,  8, () => { OR(mem.Read8(regs.HL)); })},
                { 0xB7, new Opcode(0xB7, "OR A",                 1,  4, () => { OR(regs.A); })},
                { 0xF6, new Opcode(0xF6, "OR ${0:x2}",           2,  8, () => { OR(mem.ReadParm8(regs.PC)); })},

                { 0xC1, new Opcode(0xC1, "POP BC",               1, 12, () => {throw new NotImplementedException(); })},
                { 0xD1, new Opcode(0xD1, "POP DE",               1, 12, () => {throw new NotImplementedException(); })},
                { 0xE1, new Opcode(0xE1, "POP HL",               1, 12, () => {throw new NotImplementedException(); })},
                { 0xF1, new Opcode(0xF1, "POP AF",               1, 12, () => {throw new NotImplementedException(); })},
                { 0xC5, new Opcode(0xC5, "PUSH BC",              1, 16, () => {throw new NotImplementedException(); })},
                { 0xD5, new Opcode(0xD5, "PUSH DE",              1, 16, () => {throw new NotImplementedException(); })},
                { 0xE5, new Opcode(0xE5, "PUSH HL",              1, 16, () => {throw new NotImplementedException(); })},
                { 0xF5, new Opcode(0xF5, "PUSH AF",              1, 16, () => {throw new NotImplementedException(); })},
                { 0xC0, new Opcode(0xC0, "RET NZ",               1, 20, () => {throw new NotImplementedException(); })},
                { 0xC8, new Opcode(0xC8, "RET Z",                1, 20, () => {throw new NotImplementedException(); })},
                { 0xD0, new Opcode(0xD0, "RET NC",               1, 20, () => {throw new NotImplementedException(); })},
                { 0xD8, new Opcode(0xD8, "RET C",                1, 20, () => {throw new NotImplementedException(); })},
                { 0xC9, new Opcode(0xC9, "RET",                  1, 16, () => {throw new NotImplementedException(); })},
                { 0xD9, new Opcode(0xD9, "RETI",                 1, 16, () => {throw new NotImplementedException(); })},
                { 0x17, new Opcode(0x17, "RLA",                  1,  4, () => {throw new NotImplementedException(); })},
                { 0x07, new Opcode(0x07, "RLCA",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0x1F, new Opcode(0x1F, "RRA",                  1,  4, () => {throw new NotImplementedException(); })},
                { 0x0F, new Opcode(0x0F, "RRCA",                 1,  4, () => {throw new NotImplementedException(); })},
                { 0xC7, new Opcode(0xC7, "RST 00",               1, 16, () => {throw new NotImplementedException(); })},
                { 0xD7, new Opcode(0xD7, "RST 10",               1, 16, () => {throw new NotImplementedException(); })},
                { 0xE7, new Opcode(0xE7, "RST 20",               1, 16, () => {throw new NotImplementedException(); })},
                { 0xF7, new Opcode(0xF7, "RST 30",               1, 16, () => {throw new NotImplementedException(); })},
                { 0x98, new Opcode(0x98, "SBC A, B",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x99, new Opcode(0x99, "SBC A, C",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x9A, new Opcode(0x9A, "SBC A, D",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x9B, new Opcode(0x9B, "SBC A, E",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x9C, new Opcode(0x9C, "SBC A, H",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x9D, new Opcode(0x9D, "SBC A, L",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x9E, new Opcode(0x9E, "SBC A, (HL)",          1,  8, () => {throw new NotImplementedException(); })},
                { 0x9F, new Opcode(0x9F, "SBC A, A",             1,  4, () => {throw new NotImplementedException(); })},
                { 0xDE, new Opcode(0xDE, "SBC A, ${0:x2}",        2,  8, () => {throw new NotImplementedException(); })},
                { 0x37, new Opcode(0x37, "SCF",                  1,  4, () => {throw new NotImplementedException(); })},
                { 0x10, new Opcode(0x10, "STOP",                 1,  4, () => {throw new NotImplementedException(); })},

                { 0x90, new Opcode(0x90, "SUB A, B",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x91, new Opcode(0x91, "SUB A, C",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x92, new Opcode(0x92, "SUB A, D",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x93, new Opcode(0x93, "SUB A, E",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x94, new Opcode(0x94, "SUB A, H",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x95, new Opcode(0x95, "SUB A, L",             1,  4, () => {throw new NotImplementedException(); })},
                { 0x96, new Opcode(0x96, "SUB A, (HL)",          1,  8, () => {throw new NotImplementedException(); })},
                { 0x97, new Opcode(0x97, "SUB A, A",             1,  4, () => {throw new NotImplementedException(); })},
                { 0xD6, new Opcode(0xD6, "SUB ${0:x2}",          2,  8, () => {throw new NotImplementedException(); })},

                { 0xA8, new Opcode(0xA8, "XOR B",                1,  4, () => { XOR(regs.B); })},
                { 0xA9, new Opcode(0xA9, "XOR C",                1,  4, () => { XOR(regs.C); })},
                { 0xAA, new Opcode(0xAA, "XOR D",                1,  4, () => { XOR(regs.D); })},
                { 0xAB, new Opcode(0xAB, "XOR E",                1,  4, () => { XOR(regs.E); })},
                { 0xAC, new Opcode(0xAC, "XOR H",                1,  4, () => { XOR(regs.H); })},
                { 0xAD, new Opcode(0xAD, "XOR L",                1,  4, () => { XOR(regs.L); })},
                { 0xAE, new Opcode(0xAE, "XOR (HL)",             1,  8, () => { XOR(mem.Read8(regs.HL)); ; })},
                { 0xAF, new Opcode(0xAF, "XOR A",                1,  4, () => { XOR(regs.A); })},
                { 0xEE, new Opcode(0xEE, "XOR ${0:x2}",          2,  8, () => { XOR(mem.ReadParm8(regs.PC)); })},
            };
        }


        private Dictionary<byte, Opcode> InitializeCB() {
            return new Dictionary<byte, Opcode> {
                { 0x00, new Opcode(0x00, "RLC B",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x01, new Opcode(0x01, "RLC C",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x02, new Opcode(0x02, "RLC D",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x03, new Opcode(0x03, "RLC E",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x04, new Opcode(0x04, "RLC H",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x05, new Opcode(0x05, "RLC L",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x06, new Opcode(0x06, "RLC (HL)",             2, 16, () => { throw new NotImplementedException(); })},
                { 0x07, new Opcode(0x07, "RLC A",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x08, new Opcode(0x08, "RRC B",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x09, new Opcode(0x09, "RRC C",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x0A, new Opcode(0x0A, "RRC D",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x0B, new Opcode(0x0B, "RRC E",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x0C, new Opcode(0x0C, "RRC H",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x0D, new Opcode(0x0D, "RRC L",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x0E, new Opcode(0x0E, "RRC (HL)",             2, 16, () => { throw new NotImplementedException(); })},
                { 0x0F, new Opcode(0x0F, "RRC A",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x10, new Opcode(0x10, "RL B",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x11, new Opcode(0x11, "RL C",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x12, new Opcode(0x12, "RL D",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x13, new Opcode(0x13, "RL E",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x14, new Opcode(0x14, "RL H",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x15, new Opcode(0x15, "RL L",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x16, new Opcode(0x16, "RL (HL)",              2, 16, () => { throw new NotImplementedException(); })},
                { 0x17, new Opcode(0x17, "RL A",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x18, new Opcode(0x18, "RR B",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x19, new Opcode(0x19, "RR C",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x1A, new Opcode(0x1A, "RR D",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x1B, new Opcode(0x1B, "RR E",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x1C, new Opcode(0x1C, "RR H",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x1D, new Opcode(0x1D, "RR L",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x1E, new Opcode(0x1E, "RR (HL)",              2, 16, () => { throw new NotImplementedException(); })},
                { 0x1F, new Opcode(0x1F, "RR A",                 2,  8, () => { throw new NotImplementedException(); })},
                { 0x20, new Opcode(0x20, "SLA B",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x21, new Opcode(0x21, "SLA C",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x22, new Opcode(0x22, "SLA D",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x23, new Opcode(0x23, "SLA E",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x24, new Opcode(0x24, "SLA H",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x25, new Opcode(0x25, "SLA L",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x26, new Opcode(0x26, "SLA (HL)",             2, 16, () => { throw new NotImplementedException(); })},
                { 0x27, new Opcode(0x27, "SLA A",                2,  8, () => { throw new NotImplementedException(); })},

                { 0x28, new Opcode(0x28, "SRA B",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x29, new Opcode(0x29, "SRA C",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x2A, new Opcode(0x2A, "SRA D",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x2B, new Opcode(0x2B, "SRA E",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x2C, new Opcode(0x2C, "SRA H",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x2D, new Opcode(0x2D, "SRA L",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x2E, new Opcode(0x2E, "SRA (HL)",             2, 16, () => { throw new NotImplementedException(); })},
                { 0x2F, new Opcode(0x2F, "SRA A",                2,  8, () => { throw new NotImplementedException(); })},

                { 0x30, new Opcode(0x30, "SWAP B",               2,  8, () => { throw new NotImplementedException(); })},
                { 0x31, new Opcode(0x31, "SWAP C",               2,  8, () => { throw new NotImplementedException(); })},
                { 0x32, new Opcode(0x32, "SWAP D",               2,  8, () => { throw new NotImplementedException(); })},
                { 0x33, new Opcode(0x33, "SWAP E",               2,  8, () => { throw new NotImplementedException(); })},
                { 0x34, new Opcode(0x34, "SWAP H",               2,  8, () => { throw new NotImplementedException(); })},
                { 0x35, new Opcode(0x35, "SWAP L",               2,  8, () => { throw new NotImplementedException(); })},
                { 0x36, new Opcode(0x36, "SWAP (HL)",            2, 16, () => { throw new NotImplementedException(); })},
                { 0x37, new Opcode(0x37, "SWAP A",               2,  8, () => { throw new NotImplementedException(); })},

                { 0x38, new Opcode(0x38, "SRL B",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x39, new Opcode(0x39, "SRL C",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x3A, new Opcode(0x3A, "SRL D",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x3B, new Opcode(0x3B, "SRL E",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x3C, new Opcode(0x3C, "SRL H",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x3D, new Opcode(0x3D, "SRL L",                2,  8, () => { throw new NotImplementedException(); })},
                { 0x3E, new Opcode(0x3E, "SRL (HL)",             2, 16, () => { throw new NotImplementedException(); })},
                { 0x3F, new Opcode(0x3F, "SRL A",                2,  8, () => { throw new NotImplementedException(); })},
                                                                 
                //  Test bit b in register r.                    
                { 0x40, new Opcode(0x40, "BIT 0, B",             2,  8, () => { BIT(regs.B, 0); })},
                { 0x41, new Opcode(0x41, "BIT 0, C",             2,  8, () => { BIT(regs.C, 0); })},
                { 0x42, new Opcode(0x42, "BIT 0, D",             2,  8, () => { BIT(regs.D, 0); })},
                { 0x43, new Opcode(0x43, "BIT 0, E",             2,  8, () => { BIT(regs.E, 0); })},
                { 0x44, new Opcode(0x44, "BIT 0, H",             2,  8, () => { BIT(regs.H, 0); })},
                { 0x45, new Opcode(0x45, "BIT 0, L",             2,  8, () => { BIT(regs.L, 0); })},
                { 0x46, new Opcode(0x46, "BIT 0, (HL)",          2, 12, () => { BIT(mem.Read8(regs.HL), 0); })},
                { 0x47, new Opcode(0x47, "BIT 0, A",             2,  8, () => { BIT(regs.A, 0); })},
                { 0x48, new Opcode(0x48, "BIT 1, B",             2,  8, () => { BIT(regs.B, 1); })},
                { 0x49, new Opcode(0x49, "BIT 1, C",             2,  8, () => { BIT(regs.C, 1); })},
                { 0x4A, new Opcode(0x4A, "BIT 1, D",             2,  8, () => { BIT(regs.D, 1); })},
                { 0x4B, new Opcode(0x4B, "BIT 1, E",             2,  8, () => { BIT(regs.E, 1); })},
                { 0x4C, new Opcode(0x4C, "BIT 1, H",             2,  8, () => { BIT(regs.H, 1); })},
                { 0x4D, new Opcode(0x4D, "BIT 1, L",             2,  8, () => { BIT(regs.L, 1); })},
                { 0x4E, new Opcode(0x4E, "BIT 1, (HL)",          2, 12, () => { BIT(mem.Read8(regs.HL), 1); })},
                { 0x4F, new Opcode(0x4F, "BIT 1, A",             2,  8, () => { BIT(regs.A, 1); })},
                { 0x50, new Opcode(0x50, "BIT 2, B",             2,  8, () => { BIT(regs.B, 2); })},
                { 0x51, new Opcode(0x51, "BIT 2, C",             2,  8, () => { BIT(regs.C, 2); })},
                { 0x52, new Opcode(0x52, "BIT 2, D",             2,  8, () => { BIT(regs.D, 2); })},
                { 0x53, new Opcode(0x53, "BIT 2, E",             2,  8, () => { BIT(regs.E, 2); })},
                { 0x54, new Opcode(0x54, "BIT 2, H",             2,  8, () => { BIT(regs.H, 2); })},
                { 0x55, new Opcode(0x55, "BIT 2, L",             2,  8, () => { BIT(regs.L, 2); })},
                { 0x56, new Opcode(0x56, "BIT 2, (HL)",          2, 12, () => { BIT(mem.Read8(regs.HL), 2); })},
                { 0x57, new Opcode(0x57, "BIT 2, A",             2,  8, () => { BIT(regs.A, 2); })},
                { 0x58, new Opcode(0x58, "BIT 3, B",             2,  8, () => { BIT(regs.B, 3); })},
                { 0x59, new Opcode(0x59, "BIT 3, C",             2,  8, () => { BIT(regs.C, 3); })},
                { 0x5A, new Opcode(0x5A, "BIT 3, D",             2,  8, () => { BIT(regs.D, 3); })},
                { 0x5B, new Opcode(0x5B, "BIT 3, E",             2,  8, () => { BIT(regs.E, 3); })},
                { 0x5C, new Opcode(0x5C, "BIT 3, H",             2,  8, () => { BIT(regs.H, 3); })},
                { 0x5D, new Opcode(0x5D, "BIT 3, L",             2,  8, () => { BIT(regs.L, 3); })},
                { 0x5E, new Opcode(0x5E, "BIT 3, (HL)",          2, 12, () => { BIT(mem.Read8(regs.HL), 3); })},
                { 0x5F, new Opcode(0x5F, "BIT 3, A",             2,  8, () => { BIT(regs.A, 3); })},
                { 0x60, new Opcode(0x60, "BIT 4, B",             2,  8, () => { BIT(regs.B, 4); })},
                { 0x61, new Opcode(0x61, "BIT 4, C",             2,  8, () => { BIT(regs.C, 4); })},
                { 0x62, new Opcode(0x62, "BIT 4, D",             2,  8, () => { BIT(regs.D, 4); })},
                { 0x63, new Opcode(0x63, "BIT 4, E",             2,  8, () => { BIT(regs.E, 4); })},
                { 0x64, new Opcode(0x64, "BIT 4, H",             2,  8, () => { BIT(regs.H, 4); })},
                { 0x65, new Opcode(0x65, "BIT 4, L",             2,  8, () => { BIT(regs.L, 4); })},
                { 0x66, new Opcode(0x66, "BIT 4, (HL)",          2, 12, () => { BIT(mem.Read8(regs.HL), 4); })},
                { 0x67, new Opcode(0x67, "BIT 4, A",             2,  8, () => { BIT(regs.A, 4); })},
                { 0x68, new Opcode(0x68, "BIT 5, B",             2,  8, () => { BIT(regs.B, 5); })},
                { 0x69, new Opcode(0x69, "BIT 5, C",             2,  8, () => { BIT(regs.C, 5); })},
                { 0x6A, new Opcode(0x6A, "BIT 5, D",             2,  8, () => { BIT(regs.D, 5); })},
                { 0x6B, new Opcode(0x6B, "BIT 5, E",             2,  8, () => { BIT(regs.E, 5); })},
                { 0x6C, new Opcode(0x6C, "BIT 5, H",             2,  8, () => { BIT(regs.H, 5); })},
                { 0x6D, new Opcode(0x6D, "BIT 5, L",             2,  8, () => { BIT(regs.L, 5); })},
                { 0x6E, new Opcode(0x6E, "BIT 5, (HL)",          2, 12, () => { BIT(mem.Read8(regs.HL), 5); })},
                { 0x6F, new Opcode(0x6F, "BIT 5, A",             2,  8, () => { BIT(regs.A, 5); })},
                { 0x70, new Opcode(0x70, "BIT 6, B",             2,  8, () => { BIT(regs.B, 6); })},
                { 0x71, new Opcode(0x71, "BIT 6, C",             2,  8, () => { BIT(regs.C, 6); })},
                { 0x72, new Opcode(0x72, "BIT 6, D",             2,  8, () => { BIT(regs.D, 6); })},
                { 0x73, new Opcode(0x73, "BIT 6, E",             2,  8, () => { BIT(regs.E, 6); })},
                { 0x74, new Opcode(0x74, "BIT 6, H",             2,  8, () => { BIT(regs.H, 6); })},
                { 0x75, new Opcode(0x75, "BIT 6, L",             2,  8, () => { BIT(regs.L, 6); })},
                { 0x76, new Opcode(0x76, "BIT 6, (HL)",          2, 12, () => { BIT(mem.Read8(regs.HL), 6); })},
                { 0x77, new Opcode(0x77, "BIT 6, A",             2,  8, () => { BIT(regs.A, 6); })},
                { 0x78, new Opcode(0x78, "BIT 7, B",             2,  8, () => { BIT(regs.B, 7); })},
                { 0x79, new Opcode(0x79, "BIT 7, C",             2,  8, () => { BIT(regs.C, 7); })},
                { 0x7A, new Opcode(0x7A, "BIT 7, D",             2,  8, () => { BIT(regs.D, 7); })},
                { 0x7B, new Opcode(0x7B, "BIT 7, E",             2,  8, () => { BIT(regs.E, 7); })},
                { 0x7C, new Opcode(0x7C, "BIT 7, H",             2,  8, () => { BIT(regs.H, 7); })},
                { 0x7D, new Opcode(0x7D, "BIT 7, L",             2,  8, () => { BIT(regs.L, 7); })},
                { 0x7E, new Opcode(0x7E, "BIT 7, (HL)",          2, 12, () => { BIT(mem.Read8(regs.HL), 7); })},
                { 0x7F, new Opcode(0x7F, "BIT 7, A",             2,  8, () => { BIT(regs.A, 7); })},

                { 0x80, new Opcode(0x80, "RES 0, B",             2,  8, () => { regs.B = RES(regs.B, 0); })},
                { 0x81, new Opcode(0x81, "RES 0, C",             2,  8, () => { regs.C = RES(regs.C, 0); })},
                { 0x82, new Opcode(0x82, "RES 0, D",             2,  8, () => { regs.D = RES(regs.D, 0); })},
                { 0x83, new Opcode(0x83, "RES 0, E",             2,  8, () => { regs.E = RES(regs.E, 0); })},
                { 0x84, new Opcode(0x84, "RES 0, H",             2,  8, () => { regs.H = RES(regs.H, 0); })},
                { 0x85, new Opcode(0x85, "RES 0, L",             2,  8, () => { regs.L = RES(regs.L, 0); })},
                { 0x86, new Opcode(0x86, "RES 0, (HL)",          2, 16, () => { mem.Write8(regs.HL, RES(mem.Read8(regs.HL), 0)); })},
                { 0x87, new Opcode(0x87, "RES 0, A",             2,  8, () => { regs.A = RES(regs.A, 0); })},
                { 0x88, new Opcode(0x88, "RES 1, B",             2,  8, () => { regs.B = RES(regs.B, 1); })},
                { 0x89, new Opcode(0x89, "RES 1, C",             2,  8, () => { regs.C = RES(regs.C, 1); })},
                { 0x8A, new Opcode(0x8A, "RES 1, D",             2,  8, () => { regs.D = RES(regs.D, 1); })},
                { 0x8B, new Opcode(0x8B, "RES 1, E",             2,  8, () => { regs.E = RES(regs.E, 1); })},
                { 0x8C, new Opcode(0x8C, "RES 1, H",             2,  8, () => { regs.H = RES(regs.H, 1); })},
                { 0x8D, new Opcode(0x8D, "RES 1, L",             2,  8, () => { regs.L = RES(regs.L, 1); })},
                { 0x8E, new Opcode(0x8E, "RES 1, (HL)",          2, 16, () => { mem.Write8(regs.HL, RES(mem.Read8(regs.HL), 1)); })},
                { 0x8F, new Opcode(0x8F, "RES 1, A",             2,  8, () => { regs.A = RES(regs.A, 1); })},
                { 0x90, new Opcode(0x90, "RES 2, B",             2,  8, () => { regs.B = RES(regs.B, 2); })},
                { 0x91, new Opcode(0x91, "RES 2, C",             2,  8, () => { regs.C = RES(regs.C, 2); })},
                { 0x92, new Opcode(0x92, "RES 2, D",             2,  8, () => { regs.D = RES(regs.D, 2); })},
                { 0x93, new Opcode(0x93, "RES 2, E",             2,  8, () => { regs.E = RES(regs.E, 2); })},
                { 0x94, new Opcode(0x94, "RES 2, H",             2,  8, () => { regs.H = RES(regs.H, 2); })},
                { 0x95, new Opcode(0x95, "RES 2, L",             2,  8, () => { regs.L = RES(regs.L, 2); })},
                { 0x96, new Opcode(0x96, "RES 2, (HL)",          2, 16, () => { mem.Write8(regs.HL, RES(mem.Read8(regs.HL), 2)); })},
                { 0x97, new Opcode(0x97, "RES 2, A",             2,  8, () => { regs.A = RES(regs.A, 2); })},
                { 0x98, new Opcode(0x98, "RES 3, B",             2,  8, () => { regs.B = RES(regs.B, 3); })},
                { 0x99, new Opcode(0x99, "RES 3, C",             2,  8, () => { regs.C = RES(regs.C, 3); })},
                { 0x9A, new Opcode(0x9A, "RES 3, D",             2,  8, () => { regs.D = RES(regs.D, 3); })},
                { 0x9B, new Opcode(0x9B, "RES 3, E",             2,  8, () => { regs.E = RES(regs.E, 3); })},
                { 0x9C, new Opcode(0x9C, "RES 3, H",             2,  8, () => { regs.H = RES(regs.H, 3); })},
                { 0x9D, new Opcode(0x9D, "RES 3, L",             2,  8, () => { regs.L = RES(regs.L, 3); })},
                { 0x9E, new Opcode(0x9E, "RES 3, (HL)",          2, 16, () => { mem.Write8(regs.HL, RES(mem.Read8(regs.HL), 3)); })},
                { 0x9F, new Opcode(0x9F, "RES 3, A",             2,  8, () => { regs.A = RES(regs.A, 3); })},
                { 0xA0, new Opcode(0xA0, "RES 4, B",             2,  8, () => { regs.B = RES(regs.B, 4); })},
                { 0xA1, new Opcode(0xA1, "RES 4, C",             2,  8, () => { regs.C = RES(regs.C, 4); })},
                { 0xA2, new Opcode(0xA2, "RES 4, D",             2,  8, () => { regs.D = RES(regs.D, 4); })},
                { 0xA3, new Opcode(0xA3, "RES 4, E",             2,  8, () => { regs.E = RES(regs.E, 4); })},
                { 0xA4, new Opcode(0xA4, "RES 4, H",             2,  8, () => { regs.H = RES(regs.H, 4); })},
                { 0xA5, new Opcode(0xA5, "RES 4, L",             2,  8, () => { regs.L = RES(regs.L, 4); })},
                { 0xA6, new Opcode(0xA6, "RES 4, (HL)",          2, 16, () => { mem.Write8(regs.HL, RES(mem.Read8(regs.HL), 4)); })},
                { 0xA7, new Opcode(0xA7, "RES 4, A",             2,  8, () => { regs.A = RES(regs.A, 4); })},
                { 0xA8, new Opcode(0xA8, "RES 5, B",             2,  8, () => { regs.B = RES(regs.B, 5); })},
                { 0xA9, new Opcode(0xA9, "RES 5, C",             2,  8, () => { regs.C = RES(regs.C, 5); })},
                { 0xAA, new Opcode(0xAA, "RES 5, D",             2,  8, () => { regs.D = RES(regs.D, 5); })},
                { 0xAB, new Opcode(0xAB, "RES 5, E",             2,  8, () => { regs.E = RES(regs.E, 5); })},
                { 0xAC, new Opcode(0xAC, "RES 5, H",             2,  8, () => { regs.H = RES(regs.H, 5); })},
                { 0xAD, new Opcode(0xAD, "RES 5, L",             2,  8, () => { regs.L = RES(regs.L, 5); })},
                { 0xAE, new Opcode(0xAE, "RES 5, (HL)",          2, 16, () => { mem.Write8(regs.HL, RES(mem.Read8(regs.HL), 5)); })},
                { 0xAF, new Opcode(0xAF, "RES 5, A",             2,  8, () => { regs.A = RES(regs.A, 5); })},
                { 0xB0, new Opcode(0xB0, "RES 6, B",             2,  8, () => { regs.B = RES(regs.B, 6); })},
                { 0xB1, new Opcode(0xB1, "RES 6, C",             2,  8, () => { regs.C = RES(regs.C, 6); })},
                { 0xB2, new Opcode(0xB2, "RES 6, D",             2,  8, () => { regs.D = RES(regs.D, 6); })},
                { 0xB3, new Opcode(0xB3, "RES 6, E",             2,  8, () => { regs.E = RES(regs.E, 6); })},
                { 0xB4, new Opcode(0xB4, "RES 6, H",             2,  8, () => { regs.H = RES(regs.H, 6); })},
                { 0xB5, new Opcode(0xB5, "RES 6, L",             2,  8, () => { regs.L = RES(regs.L, 6); })},
                { 0xB6, new Opcode(0xB6, "RES 6, (HL)",          2, 16, () => { mem.Write8(regs.HL, RES(mem.Read8(regs.HL), 6)); })},
                { 0xB7, new Opcode(0xB7, "RES 6, A",             2,  8, () => { regs.A = RES(regs.A, 6); })},
                { 0xB8, new Opcode(0xB8, "RES 7, B",             2,  8, () => { regs.B = RES(regs.B, 7); })},
                { 0xB9, new Opcode(0xB9, "RES 7, C",             2,  8, () => { regs.C = RES(regs.C, 7); })},
                { 0xBA, new Opcode(0xBA, "RES 7, D",             2,  8, () => { regs.D = RES(regs.D, 7); })},
                { 0xBB, new Opcode(0xBB, "RES 7, E",             2,  8, () => { regs.E = RES(regs.E, 7); })},
                { 0xBC, new Opcode(0xBC, "RES 7, H",             2,  8, () => { regs.H = RES(regs.H, 7); })},
                { 0xBD, new Opcode(0xBD, "RES 7, L",             2,  8, () => { regs.L = RES(regs.L, 7); })},
                { 0xBE, new Opcode(0xBE, "RES 7, (HL)",          2, 16, () => { mem.Write8(regs.HL, RES(mem.Read8(regs.HL), 7)); })},
                { 0xBF, new Opcode(0xBF, "RES 7, A",             2,  8, () => { regs.A = RES(regs.A, 7); })},

                { 0xC0, new Opcode(0xC0, "SET 0, B",             2,  8, () => { regs.B = SET(regs.B, 0); })},
                { 0xC1, new Opcode(0xC1, "SET 0, C",             2,  8, () => { regs.C = SET(regs.C, 0); })},
                { 0xC2, new Opcode(0xC2, "SET 0, D",             2,  8, () => { regs.D = SET(regs.D, 0); })},
                { 0xC3, new Opcode(0xC3, "SET 0, E",             2,  8, () => { regs.E = SET(regs.E, 0); })},
                { 0xC4, new Opcode(0xC4, "SET 0, H",             2,  8, () => { regs.H = SET(regs.H, 0); })},
                { 0xC5, new Opcode(0xC5, "SET 0, L",             2,  8, () => { regs.L = SET(regs.L, 0); })},
                { 0xC6, new Opcode(0xC6, "SET 0, (HL)",          2, 16, () => { mem.Write8(regs.HL, SET(mem.Read8(regs.HL), 0)); })},
                { 0xC7, new Opcode(0xC7, "SET 0, A",             2,  8, () => { regs.A = SET(regs.A, 0); })},
                { 0xC8, new Opcode(0xC8, "SET 1, B",             2,  8, () => { regs.B = SET(regs.B, 1); })},
                { 0xC9, new Opcode(0xC9, "SET 1, C",             2,  8, () => { regs.C = SET(regs.C, 1); })},
                { 0xCA, new Opcode(0xCA, "SET 1, D",             2,  8, () => { regs.D = SET(regs.D, 1); })},
                { 0xCB, new Opcode(0xCB, "SET 1, E",             2,  8, () => { regs.E = SET(regs.E, 1); })},
                { 0xCC, new Opcode(0xCC, "SET 1, H",             2,  8, () => { regs.H = SET(regs.H, 1); })},
                { 0xCD, new Opcode(0xCD, "SET 1, L",             2,  8, () => { regs.L = SET(regs.L, 1); })},
                { 0xCE, new Opcode(0xCE, "SET 1, (HL)",          2, 16, () => { mem.Write8(regs.HL, SET(mem.Read8(regs.HL), 1)); })},
                { 0xCF, new Opcode(0xCF, "SET 1, A",             2,  8, () => { regs.A = SET(regs.A, 1); })},
                { 0xD0, new Opcode(0xD0, "SET 2, B",             2,  8, () => { regs.B = SET(regs.B, 2); })},
                { 0xD1, new Opcode(0xD1, "SET 2, C",             2,  8, () => { regs.C = SET(regs.C, 2); })},
                { 0xD2, new Opcode(0xD2, "SET 2, D",             2,  8, () => { regs.D = SET(regs.D, 2); })},
                { 0xD3, new Opcode(0xD3, "SET 2, E",             2,  8, () => { regs.E = SET(regs.E, 2); })},
                { 0xD4, new Opcode(0xD4, "SET 2, H",             2,  8, () => { regs.H = SET(regs.H, 2); })},
                { 0xD5, new Opcode(0xD5, "SET 2, L",             2,  8, () => { regs.L = SET(regs.L, 2); })},
                { 0xD6, new Opcode(0xD6, "SET 2, (HL)",          2, 16, () => { mem.Write8(regs.HL, SET(mem.Read8(regs.HL), 2)); })},
                { 0xD7, new Opcode(0xD7, "SET 2, A",             2,  8, () => { regs.A = SET(regs.A, 2); })},
                { 0xD8, new Opcode(0xD8, "SET 3, B",             2,  8, () => { regs.B = SET(regs.B, 3); })},
                { 0xD9, new Opcode(0xD9, "SET 3, C",             2,  8, () => { regs.C = SET(regs.C, 3); })},
                { 0xDA, new Opcode(0xDA, "SET 3, D",             2,  8, () => { regs.D = SET(regs.D, 3); })},
                { 0xDB, new Opcode(0xDB, "SET 3, E",             2,  8, () => { regs.E = SET(regs.E, 3); })},
                { 0xDC, new Opcode(0xDC, "SET 3, H",             2,  8, () => { regs.H = SET(regs.H, 3); })},
                { 0xDD, new Opcode(0xDD, "SET 3, L",             2,  8, () => { regs.L = SET(regs.L, 3); })},
                { 0xDE, new Opcode(0xDE, "SET 3, (HL)",          2, 16, () => { mem.Write8(regs.HL, SET(mem.Read8(regs.HL), 3)); })},
                { 0xDF, new Opcode(0xDF, "SET 3, A",             2,  8, () => { regs.A = SET(regs.A, 3); })},
                { 0xE0, new Opcode(0xE0, "SET 4, B",             2,  8, () => { regs.B = SET(regs.B, 4); })},
                { 0xE1, new Opcode(0xE1, "SET 4, C",             2,  8, () => { regs.C = SET(regs.C, 4); })},
                { 0xE2, new Opcode(0xE2, "SET 4, D",             2,  8, () => { regs.D = SET(regs.D, 4); })},
                { 0xE3, new Opcode(0xE3, "SET 4, E",             2,  8, () => { regs.E = SET(regs.E, 4); })},
                { 0xE4, new Opcode(0xE4, "SET 4, H",             2,  8, () => { regs.H = SET(regs.H, 4); })},
                { 0xE5, new Opcode(0xE5, "SET 4, L",             2,  8, () => { regs.L = SET(regs.L, 4); })},
                { 0xE6, new Opcode(0xE6, "SET 4, (HL)",          2, 16, () => { mem.Write8(regs.HL, SET(mem.Read8(regs.HL), 4)); })},
                { 0xE7, new Opcode(0xE7, "SET 4, A",             2,  8, () => { regs.A = SET(regs.A, 4); })},
                { 0xE8, new Opcode(0xE8, "SET 5, B",             2,  8, () => { regs.B = SET(regs.B, 5); })},
                { 0xE9, new Opcode(0xE9, "SET 5, C",             2,  8, () => { regs.C = SET(regs.C, 5); })},
                { 0xEA, new Opcode(0xEA, "SET 5, D",             2,  8, () => { regs.D = SET(regs.D, 5); })},
                { 0xEB, new Opcode(0xEB, "SET 5, E",             2,  8, () => { regs.E = SET(regs.E, 5); })},
                { 0xEC, new Opcode(0xEC, "SET 5, H",             2,  8, () => { regs.H = SET(regs.H, 5); })},
                { 0xED, new Opcode(0xED, "SET 5, L",             2,  8, () => { regs.L = SET(regs.L, 5); })},
                { 0xEE, new Opcode(0xEE, "SET 5, (HL)",          2, 16, () => { mem.Write8(regs.HL, SET(mem.Read8(regs.HL), 5)); })},
                { 0xEF, new Opcode(0xEF, "SET 5, A",             2,  8, () => { regs.A = SET(regs.A, 5); })},
                { 0xF0, new Opcode(0xF0, "SET 6, B",             2,  8, () => { regs.B = SET(regs.B, 6); })},
                { 0xF1, new Opcode(0xF1, "SET 6, C",             2,  8, () => { regs.C = SET(regs.C, 6); })},
                { 0xF2, new Opcode(0xF2, "SET 6, D",             2,  8, () => { regs.D = SET(regs.D, 6); })},
                { 0xF3, new Opcode(0xF3, "SET 6, E",             2,  8, () => { regs.E = SET(regs.E, 6); })},
                { 0xF4, new Opcode(0xF4, "SET 6, H",             2,  8, () => { regs.H = SET(regs.H, 6); })},
                { 0xF5, new Opcode(0xF5, "SET 6, L",             2,  8, () => { regs.L = SET(regs.L, 6); })},
                { 0xF6, new Opcode(0xF6, "SET 6, (HL)",          2, 16, () => { mem.Write8(regs.HL, SET(mem.Read8(regs.HL), 6)); })},
                { 0xF7, new Opcode(0xF7, "SET 6, A",             2,  8, () => { regs.A = SET(regs.A, 6); })},
                { 0xF8, new Opcode(0xF8, "SET 7, B",             2,  8, () => { regs.B = SET(regs.B, 7); })},
                { 0xF9, new Opcode(0xF9, "SET 7, C",             2,  8, () => { regs.C = SET(regs.C, 7); })},
                { 0xFA, new Opcode(0xFA, "SET 7, D",             2,  8, () => { regs.D = SET(regs.D, 7); })},
                { 0xFB, new Opcode(0xFB, "SET 7, E",             2,  8, () => { regs.E = SET(regs.E, 7); })},
                { 0xFC, new Opcode(0xFC, "SET 7, H",             2,  8, () => { regs.H = SET(regs.H, 7); })},
                { 0xFD, new Opcode(0xFD, "SET 7, L",             2,  8, () => { regs.L = SET(regs.L, 7); })},
                { 0xFE, new Opcode(0xFE, "SET 7, (HL)",          2, 16, () => { mem.Write8(regs.HL, SET(mem.Read8(regs.HL), 7)); })},
                { 0xFF, new Opcode(0xFF, "SET 7, A",             2,  8, () => { regs.A = SET(regs.A, 7); })},
            };
        }

        public static int TwosComplementByteToInt(u8 rawValue) {
            // If a positive value, return it
            if ((rawValue & 0x80) == 0) {
                return rawValue;
            }

            // Otherwise perform the 2's complement math on the value
            return (byte)(~(rawValue - 0x01)) * -1;
        }

        private void JP(u16 address) {
            regs.PC = address;
            handledPC = true;
        }

        private void JP_FLAG(bool flag, u16 address) {
            if (flag) {
                regs.PC = address;
                handledPC = true;
            }
        }

        private void JR(u8 offset) {
            // +2 because it's the size of the opcode
            regs.PC = (u16)(regs.PC + 2 + TwosComplementByteToInt(offset));
            handledPC = true;
        }

        private void JR_FLAG(bool flag, u8 offset) {
            if (flag) {
                // +2 because it's the size of opcode
                regs.PC = (u16)(regs.PC + 2 + TwosComplementByteToInt(offset));
                handledPC = true;
            }
        }

        private bool IsHalfCarry(u8 b1, u8 b2) {
            return ((b1 & 0xF) + (b2 & 0xF)) > 0xF;
        }

        private bool IsHalfCarrySub(byte b1, byte b2) {
            return (b1 & 0xF) < (b2 & 0xF);
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

        public u8 INC(u8 value) {
            value += 1;
            regs.FlagZ = (value == 0);
            regs.FlagN = false;
            regs.FlagH = IsHalfCarry(value, 1);
            // regs.FlagC -> unmodified
            return value;
        }

        private u8 DEC(u8 value) {
            value -= 1;
            regs.FlagZ = (value == 0);
            regs.FlagN = true;
            regs.FlagH = IsHalfCarrySub(value, 1);
            // r.FlagC -> unmodified
            return value;
        }

        // Test bit in value
        // Z - Set if bit b of register r is 0.
        private void BIT(u8 value, int bitPosition) {
            regs.FlagZ = ((value >> bitPosition) & 0b_0000_0001) == 0;
            regs.FlagN = false;
            regs.FlagH = true;
            // r.FlagC -> unmodified
        }

        // Set bit in value
        private byte SET(u8 value, int bitPosition) {
            return (u8)(value | (0b_0000_0001 << bitPosition));
        }

        // Reset bit in value
        private byte RES(byte value, int bitPosition) {
            return (byte)(value & ~(0b_0000_0001 << bitPosition));
        }
    }
}
