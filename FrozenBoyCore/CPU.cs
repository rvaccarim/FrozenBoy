using System;
using System.Diagnostics;
using System.Collections.Generic;
using u8 = System.Byte;
using u16 = System.UInt16;


namespace FrozenBoyCore {

    public class CPU {
        public Registers regs;
        public MMU mmu;

        public Dictionary<u8, Opcode> opcodes;
        public Dictionary<u8, Opcode> cbOpcodes;

        // Opcode related stuff
        public Opcode opcode;
        public Opcode prevOpcode;
        public u16 opLocation;

        public bool IME;  // Interrupt Master Enable Register, it's a master switch for all interruptions
        public bool IME_Scheduled = true;
        public bool halted = false;

        public CPU(MMU mmu) {
            this.mmu = mmu;

            regs = new Registers {
                AF = 0x01B0,
                BC = 0x0013,
                DE = 0x00D8,
                HL = 0x014d,
                PC = 0x100,
                SP = 0xFFFE
            };
            IME = false;

            opcodes = InitializeOpcodes();
            cbOpcodes = InitializeCB();
        }

        public int ExecuteNext() {
            int cycles = 0;

            opLocation = regs.PC;
            opcode = Disassemble();

            if (opcode != null) {
                // points to the next one even if we haven't executed it yet
                regs.PC = (u16)(regs.PC + opcode.length);

                // execute opcode
                opcode.logic.Invoke();
            }
            else {
                System.Environment.Exit(0);
            }

            cycles += opcode.mcycles;
            prevOpcode = opcode;

            return cycles;
        }

        public void HandleInterrupts() {
            // IE and IF are positions in memory
            // IE = granular interrupt enabler. When bits are set, the corresponding interrupt can be triggered
            // IF = When bits are set, an interrupt has happened
            // They use the same bit positions
            // 
            // Bit When 0  When 1
            // 0   Vblank 
            // 1   LCD stat
            // 2   Timer 
            // 3   Serial Link 
            // 4   Joypad 

            for (int bitPos = 0; bitPos < 5; bitPos++) {
                if ((((mmu.IE & mmu.IF) >> bitPos) & 0x1) == 1) {
                    if (halted) {
                        regs.PC++;
                        halted = false;
                    }
                    if (IME) {
                        PUSH(regs.PC);
                        regs.PC = mmu.ISR_Address[bitPos];
                        IME = false;
                        mmu.IF = RES(mmu.IF, bitPos);
                    }
                }
            }

            IME |= IME_Scheduled;
            IME_Scheduled = false;
        }

        public void UpdateIME() {

        }


        public Opcode Disassemble() {
            u8 opcodeValue = mmu.Read8(regs.PC);

            if (opcodes.ContainsKey(opcodeValue)) {

                opcode = opcodes[opcodeValue];
                if (opcode.value != 0xCB) {
                    return opcode;
                }
                else {
                    u8 cbOpcodeValue = Parm8();

                    if (cbOpcodes.ContainsKey(cbOpcodeValue)) {
                        return cbOpcodes[cbOpcodeValue];
                    }
                    else {
                        Debug.WriteLine(String.Format("Cb_opcode not found: {0:x2}", cbOpcodeValue));
                    }
                }
            }
            else {
                Debug.WriteLine(String.Format("Opcode not found: {0:x2}", opcodeValue));
            }

            return null;
        }

        private void STOP() {
            throw new NotImplementedException();
        }

        private void HALT() {
            if (!IME) {
                if ((mmu.IE & mmu.IF & 0x1F) == 0) {
                    halted = true;
                    regs.PC--;
                }
            }
        }

        private Dictionary<u8, Opcode> InitializeOpcodes() {
            return new Dictionary<u8, Opcode> {
                // ==================================================================================================================
                // ADDITION FAMILY
                // ==================================================================================================================
                // Add value + Carry flag to A
                { 0x88, new Opcode(0x88, "ADC A, B",             1,  4, () => { ADC(regs.B); })},
                { 0x89, new Opcode(0x89, "ADC A, C",             1,  4, () => { ADC(regs.C); })},
                { 0x8A, new Opcode(0x8A, "ADC A, D",             1,  4, () => { ADC(regs.D); })},
                { 0x8B, new Opcode(0x8B, "ADC A, E",             1,  4, () => { ADC(regs.E); })},
                { 0x8C, new Opcode(0x8C, "ADC A, H",             1,  4, () => { ADC(regs.H); })},
                { 0x8D, new Opcode(0x8D, "ADC A, L",             1,  4, () => { ADC(regs.L); })},
                { 0x8E, new Opcode(0x8E, "ADC A, (HL)",          1,  8, () => { ADC(mmu.Read8(regs.HL)); })},
                { 0x8F, new Opcode(0x8F, "ADC A, A",             1,  4, () => { ADC(regs.A); })},
                { 0xCE, new Opcode(0xCE, "ADC A, ${0:x2}",       2,  8, () => { ADC(Parm8()); })},
                // Add 16 bit
                { 0x09, new Opcode(0x09, "ADD HL, BC",           1,  8, () => { ADD_HL(regs.BC); })},
                { 0x19, new Opcode(0x19, "ADD HL, DE",           1,  8, () => { ADD_HL(regs.DE); })},
                { 0x29, new Opcode(0x29, "ADD HL, HL",           1,  8, () => { ADD_HL(regs.HL); })},
                { 0x39, new Opcode(0x39, "ADD HL, SP",           1,  8, () => { ADD_HL(regs.SP); })},
                // Add 8 bit
                { 0x80, new Opcode(0x80, "ADD A, B",             1,  4, () => { ADD(regs.B); })},
                { 0x81, new Opcode(0x81, "ADD A, C",             1,  4, () => { ADD(regs.C); })},
                { 0x82, new Opcode(0x82, "ADD A, D",             1,  4, () => { ADD(regs.D); })},
                { 0x83, new Opcode(0x83, "ADD A, E",             1,  4, () => { ADD(regs.E); })},
                { 0x84, new Opcode(0x84, "ADD A, H",             1,  4, () => { ADD(regs.H); })},
                { 0x85, new Opcode(0x85, "ADD A, L",             1,  4, () => { ADD(regs.L); })},
                { 0x86, new Opcode(0x86, "ADD A, (HL)",          1,  8, () => { ADD(mmu.Read8(regs.HL)); })},
                { 0x87, new Opcode(0x87, "ADD A, A",             1,  4, () => { ADD(regs.A); })},
                { 0xC6, new Opcode(0xC6, "ADD A, ${0:x2}",       2,  8, () => { ADD(Parm8()); })},
                { 0xE8, new Opcode(0xE8, "ADD SP, ${0:x2}",      2, 16, () => { regs.SP = ADD_Signed8(regs.SP, Parm8()); })},
                // INC - 8 bit                                  
                { 0x04, new Opcode(0x04, "INC B",                1,  4, () => { regs.B = INC(regs.B); })},
                { 0x0C, new Opcode(0x0C, "INC C",                1,  4, () => { regs.C = INC(regs.C); })},
                { 0x14, new Opcode(0x14, "INC D",                1,  4, () => { regs.D = INC(regs.D); })},
                { 0x1C, new Opcode(0x1C, "INC E",                1,  4, () => { regs.E = INC(regs.E); })},
                { 0x24, new Opcode(0x24, "INC H",                1,  4, () => { regs.H = INC(regs.H); })},
                { 0x2C, new Opcode(0x2C, "INC L",                1,  4, () => { regs.L = INC(regs.L); })},
                { 0x34, new Opcode(0x34, "INC (HL)",             1, 12, () => { mmu.Write8(regs.HL, INC(mmu.Read8(regs.HL))); })},
                { 0x3C, new Opcode(0x3C, "INC A",                1,  4, () => { regs.A = INC(regs.A); })},
                // INC - 16 bit
                { 0x03, new Opcode(0x03, "INC BC",               1,  8, () => { regs.BC++; })},
                { 0x13, new Opcode(0x13, "INC DE",               1,  8, () => { regs.DE++; })},
                { 0x23, new Opcode(0x23, "INC HL",               1,  8, () => { regs.HL++; })},
                { 0x33, new Opcode(0x33, "INC SP",               1,  8, () => { regs.SP++; })},

                // ==================================================================================================================
                // AND, OR, XOR
                // ==================================================================================================================
                { 0xA0, new Opcode(0xA0, "AND B",                1,  4, () => { AND(regs.B); })},
                { 0xA1, new Opcode(0xA1, "AND C",                1,  4, () => { AND(regs.C); })},
                { 0xA2, new Opcode(0xA2, "AND D",                1,  4, () => { AND(regs.D); })},
                { 0xA3, new Opcode(0xA3, "AND E",                1,  4, () => { AND(regs.E); })},
                { 0xA4, new Opcode(0xA4, "AND H",                1,  4, () => { AND(regs.H); })},
                { 0xA5, new Opcode(0xA5, "AND L",                1,  4, () => { AND(regs.L); })},
                { 0xA6, new Opcode(0xA6, "AND (HL)",             1,  8, () => { AND(mmu.Read8(regs.HL)); })},
                { 0xA7, new Opcode(0xA7, "AND A",                1,  4, () => { AND(regs.A); })},
                { 0xE6, new Opcode(0xE6, "AND ${0:x2}",          2,  8, () => { AND(Parm8()); })},
                // OR
                { 0xB0, new Opcode(0xB0, "OR B",                 1,  4, () => { OR(regs.B); })},
                { 0xB1, new Opcode(0xB1, "OR C",                 1,  4, () => { OR(regs.C); })},
                { 0xB2, new Opcode(0xB2, "OR D",                 1,  4, () => { OR(regs.D); })},
                { 0xB3, new Opcode(0xB3, "OR E",                 1,  4, () => { OR(regs.E); })},
                { 0xB4, new Opcode(0xB4, "OR H",                 1,  4, () => { OR(regs.H); })},
                { 0xB5, new Opcode(0xB5, "OR L",                 1,  4, () => { OR(regs.L); })},
                { 0xB6, new Opcode(0xB6, "OR (HL)",              1,  8, () => { OR(mmu.Read8(regs.HL)); })},
                { 0xB7, new Opcode(0xB7, "OR A",                 1,  4, () => { OR(regs.A); })},
                { 0xF6, new Opcode(0xF6, "OR ${0:x2}",           2,  8, () => { OR(Parm8()); })},
                // XOR
                { 0xA8, new Opcode(0xA8, "XOR B",                1,  4, () => { XOR(regs.B); })},
                { 0xA9, new Opcode(0xA9, "XOR C",                1,  4, () => { XOR(regs.C); })},
                { 0xAA, new Opcode(0xAA, "XOR D",                1,  4, () => { XOR(regs.D); })},
                { 0xAB, new Opcode(0xAB, "XOR E",                1,  4, () => { XOR(regs.E); })},
                { 0xAC, new Opcode(0xAC, "XOR H",                1,  4, () => { XOR(regs.H); })},
                { 0xAD, new Opcode(0xAD, "XOR L",                1,  4, () => { XOR(regs.L); })},
                { 0xAE, new Opcode(0xAE, "XOR (HL)",             1,  8, () => { XOR(mmu.Read8(regs.HL)); ; })},
                { 0xAF, new Opcode(0xAF, "XOR A",                1,  4, () => { XOR(regs.A); })},
                { 0xEE, new Opcode(0xEE, "XOR ${0:x2}",          2,  8, () => { XOR(Parm8()); })},

                // ==================================================================================================================
                // CALL AND RETURN
                // ==================================================================================================================
                // Push address of next instruction onto stack and then jump to address nn.
                { 0xCD, new Opcode(0xCD, "CALL ${0:x4}",         3, 24, () => { CALL(true, Parm16()); })},
                { 0xC4, new Opcode(0xC4, "CALL NZ, ${0:x4}",     3, 24, () => { CALL(!regs.FlagZ, Parm16()); })},
                { 0xCC, new Opcode(0xCC, "CALL Z, ${0:x4}",      3, 24, () => { CALL( regs.FlagZ, Parm16()); })},
                { 0xD4, new Opcode(0xD4, "CALL NC, ${0:x4}",     3, 24, () => { CALL(!regs.FlagC, Parm16()); })},
                { 0xDC, new Opcode(0xDC, "CALL C, ${0:x4}",      3, 24, () => { CALL( regs.FlagC, Parm16()); })},
                // Return - Pop two bytes from stack & jump to that address.
                { 0xC0, new Opcode(0xC0, "RET NZ",               1, 20, () => { RET(!regs.FlagZ); })},
                { 0xC8, new Opcode(0xC8, "RET Z",                1, 20, () => { RET( regs.FlagZ); })},
                { 0xD0, new Opcode(0xD0, "RET NC",               1, 20, () => { RET(!regs.FlagC); })},
                { 0xD8, new Opcode(0xD8, "RET C",                1, 20, () => { RET( regs.FlagC); })},
                { 0xC9, new Opcode(0xC9, "RET",                  1, 16, () => { RET(true); })},
                // Pop two bytes from stack & jump to that address then enable interrupts
                { 0xD9, new Opcode(0xD9, "RETI",                 1, 16, () => { RET(true); IME = true; })},

                // ==================================================================================================================
                // COMPARE
                // ==================================================================================================================
                // Compare A with n. This is basically an A - n subtraction instruction but the results are thrown away
                { 0xB8, new Opcode(0xB8, "CP B",                 1,  4, () => { CP(regs.B); })},
                { 0xB9, new Opcode(0xB9, "CP C",                 1,  4, () => { CP(regs.C); })},
                { 0xBA, new Opcode(0xBA, "CP D",                 1,  4, () => { CP(regs.D); })},
                { 0xBB, new Opcode(0xBB, "CP E",                 1,  4, () => { CP(regs.E); })},
                { 0xBC, new Opcode(0xBC, "CP H",                 1,  4, () => { CP(regs.H); })},
                { 0xBD, new Opcode(0xBD, "CP L",                 1,  4, () => { CP(regs.L); })},
                { 0xBE, new Opcode(0xBE, "CP (HL)",              1,  8, () => { CP(mmu.Read8(regs.HL)); })},
                { 0xBF, new Opcode(0xBF, "CP A",                 1,  4, () => { CP(regs.A); })},
                { 0xFE, new Opcode(0xFE, "CP ${0:x2}",           2,  8, () => { CP(Parm8()); })},

                // ==================================================================================================================
                // INTERRUPTS
                // ==================================================================================================================
                // Disables interrupt handling by setting IME=0 
                { 0xF3, new Opcode(0xF3, "DI",                   1,  4, () => { IME = false; })},
                // Schedules interrupt handling to be enabled
                { 0xFB, new Opcode(0xFB, "EI",                   1,  4, () => { IME_Scheduled = true; })},
                                        
                // ==================================================================================================================
                // JUMP FAMILY
                // ==================================================================================================================
                // JP - Jump to location
                { 0xC3, new Opcode(0xC3, "JP ${0:x4}",           3, 16, () => { JP(true, Parm16()); })},
                { 0xE9, new Opcode(0xE9, "JP (HL)",              1,  4, () => { JP(true, regs.HL); })},
                { 0xC2, new Opcode(0xC2, "JP NZ, ${0:x4}",       3, 16, () => { JP(!regs.FlagZ, Parm16()); })},
                { 0xCA, new Opcode(0xCA, "JP Z, ${0:x4}",        3, 16, () => { JP( regs.FlagZ, Parm16()); })},
                { 0xD2, new Opcode(0xD2, "JP NC, ${0:x4}",       3, 16, () => { JP(!regs.FlagC, Parm16()); })},
                { 0xDA, new Opcode(0xDA, "JP C, ${0:x4}",        3, 16, () => { JP( regs.FlagC, Parm16()); })},
                // Jump to location relative to the current location
                { 0x18, new Opcode(0x18, "JR ${0:x2}",           2, 12, () => { JR(true, Parm8()); })},
                { 0x20, new Opcode(0x20, "JR NZ, ${0:x2}",       2, 12, () => { JR(!regs.FlagZ, Parm8()); })},
                { 0x28, new Opcode(0x28, "JR Z, ${0:x2}",        2, 12, () => { JR( regs.FlagZ, Parm8()); })},
                { 0x30, new Opcode(0x30, "JR NC, ${0:x2}",       2, 12, () => { JR(!regs.FlagC, Parm8()); })},
                { 0x38, new Opcode(0x38, "JR C, ${0:x2}",        2, 12, () => { JR( regs.FlagC, Parm8()); })},

                // ==================================================================================================================
                // LOAD FANILY
                // ==================================================================================================================
                // load direct value into register - 8 bit
                { 0x06, new Opcode(0x06, "LD B, ${0:x2}",        2,  8, () => { regs.B = Parm8(); })},
                { 0x0E, new Opcode(0x0E, "LD C, ${0:x2}",        2,  8, () => { regs.C = Parm8(); })},
                { 0x16, new Opcode(0x16, "LD D, ${0:x2}",        2,  8, () => { regs.D = Parm8(); })},
                { 0x1E, new Opcode(0x1E, "LD E, ${0:x2}",        2,  8, () => { regs.E = Parm8(); })},
                { 0x26, new Opcode(0x26, "LD H, ${0:x2}",        2,  8, () => { regs.H = Parm8(); })},
                { 0x2E, new Opcode(0x2E, "LD L, ${0:x2}",        2,  8, () => { regs.L = Parm8(); })},
                { 0x36, new Opcode(0x36, "LD (HL), ${0:x2}",     2, 12, () => { mmu.Write8(regs.HL, Parm8()); })},
                { 0x3E, new Opcode(0x3E, "LD A, ${0:x2}",        2,  8, () => { regs.A = Parm8(); })},         
                // load direct value into register - 16 bit
                { 0x01, new Opcode(0x01, "LD BC, ${0:x4}",       3, 12, () => { regs.BC = Parm16(); })},
                { 0x11, new Opcode(0x11, "LD DE, ${0:x4}",       3, 12, () => { regs.DE = Parm16(); })},
                { 0x21, new Opcode(0x21, "LD HL, ${0:x4}",       3, 12, () => { regs.HL = Parm16(); })},
                { 0x31, new Opcode(0x31, "LD SP, ${0:x4}",       3, 12, () => { regs.SP = Parm16(); })},
                // load register to register
                { 0x41, new Opcode(0x41, "LD B, C",              1,  4, () => { regs.B = regs.C; })},
                { 0x40, new Opcode(0x40, "LD B, B",              1,  4, () => { })},
                { 0x42, new Opcode(0x42, "LD B, D",              1,  4, () => { regs.B = regs.D; })},
                { 0x43, new Opcode(0x43, "LD B, E",              1,  4, () => { regs.B = regs.E; })},
                { 0x44, new Opcode(0x44, "LD B, H",              1,  4, () => { regs.B = regs.H; })},
                { 0x45, new Opcode(0x45, "LD B, L",              1,  4, () => { regs.B = regs.L; })},
                { 0x47, new Opcode(0x47, "LD B, A",              1,  4, () => { regs.B = regs.A; })},
                { 0x48, new Opcode(0x48, "LD C, B",              1,  4, () => { regs.C = regs.B; })},
                { 0x49, new Opcode(0x49, "LD C, C",              1,  4, () => { })},
                { 0x4A, new Opcode(0x4A, "LD C, D",              1,  4, () => { regs.C = regs.D; })},
                { 0x4B, new Opcode(0x4B, "LD C, E",              1,  4, () => { regs.C = regs.E; })},
                { 0x4C, new Opcode(0x4C, "LD C, H",              1,  4, () => { regs.C = regs.H; })},
                { 0x4D, new Opcode(0x4D, "LD C, L",              1,  4, () => { regs.C = regs.L; })},
                { 0x4F, new Opcode(0x4F, "LD C, A",              1,  4, () => { regs.C = regs.A; })},
                { 0x50, new Opcode(0x50, "LD D, B",              1,  4, () => { regs.D = regs.B; })},
                { 0x51, new Opcode(0x51, "LD D, C",              1,  4, () => { regs.D = regs.C; })},
                { 0x52, new Opcode(0x52, "LD D, D",              1,  4, () => { })},
                { 0x53, new Opcode(0x53, "LD D, E",              1,  4, () => { regs.D = regs.E; })},
                { 0x54, new Opcode(0x54, "LD D, H",              1,  4, () => { regs.D = regs.H; })},
                { 0x55, new Opcode(0x55, "LD D, L",              1,  4, () => { regs.D = regs.L; })},
                { 0x57, new Opcode(0x57, "LD D, A",              1,  4, () => { regs.D = regs.A; })},
                { 0x58, new Opcode(0x58, "LD E, B",              1,  4, () => { regs.E = regs.B; })},
                { 0x59, new Opcode(0x59, "LD E, C",              1,  4, () => { regs.E = regs.C; })},
                { 0x5A, new Opcode(0x5A, "LD E, D",              1,  4, () => { regs.E = regs.D; })},
                { 0x5B, new Opcode(0x5B, "LD E, E",              1,  4, () => { })},
                { 0x5C, new Opcode(0x5C, "LD E, H",              1,  4, () => { regs.E = regs.H; })},
                { 0x5D, new Opcode(0x5D, "LD E, L",              1,  4, () => { regs.E = regs.L; })},
                { 0x5F, new Opcode(0x5F, "LD E, A",              1,  4, () => { regs.E = regs.A; })},
                { 0x60, new Opcode(0x60, "LD H, B",              1,  4, () => { regs.H = regs.B; })},
                { 0x61, new Opcode(0x61, "LD H, C",              1,  4, () => { regs.H = regs.C; })},
                { 0x62, new Opcode(0x62, "LD H, D",              1,  4, () => { regs.H = regs.D; })},
                { 0x63, new Opcode(0x63, "LD H, E",              1,  4, () => { regs.H = regs.E; })},
                { 0x64, new Opcode(0x64, "LD H, H",              1,  4, () => { })},
                { 0x65, new Opcode(0x65, "LD H, L",              1,  4, () => { regs.H = regs.L; })},
                { 0x67, new Opcode(0x67, "LD H, A",              1,  4, () => { regs.H = regs.A; })},
                { 0x68, new Opcode(0x68, "LD L, B",              1,  4, () => { regs.L = regs.B; })},
                { 0x69, new Opcode(0x69, "LD L, C",              1,  4, () => { regs.L = regs.C; })},
                { 0x6A, new Opcode(0x6A, "LD L, D",              1,  4, () => { regs.L = regs.D; })},
                { 0x6B, new Opcode(0x6B, "LD L, E",              1,  4, () => { regs.L = regs.E; })},
                { 0x6C, new Opcode(0x6C, "LD L, H",              1,  4, () => { regs.L = regs.H; })},
                { 0x6D, new Opcode(0x6D, "LD L, L",              1,  4, () => {  })},
                { 0x6F, new Opcode(0x6F, "LD L, A",              1,  4, () => { regs.L = regs.A; })},
                { 0x78, new Opcode(0x78, "LD A, B",              1,  4, () => { regs.A = regs.B; })},
                { 0x79, new Opcode(0x79, "LD A, C",              1,  4, () => { regs.A = regs.C; })},
                { 0x7A, new Opcode(0x7A, "LD A, D",              1,  4, () => { regs.A = regs.D; })},
                { 0x7B, new Opcode(0x7B, "LD A, E",              1,  4, () => { regs.A = regs.E; })},
                { 0x7C, new Opcode(0x7C, "LD A, H",              1,  4, () => { regs.A = regs.H; })},
                { 0x7D, new Opcode(0x7D, "LD A, L",              1,  4, () => { regs.A = regs.L; })},
                { 0x7F, new Opcode(0x7F, "LD A, A",              1,  4, () => { })},
                { 0x08, new Opcode(0x08, "LD (${0:x4}), SP",     3, 20, () => { mmu.Write16(Parm16(), regs.SP); })},
                { 0x02, new Opcode(0x02, "LD (BC), A",           1,  8, () => { mmu.Write8(regs.BC, regs.A); })},
                { 0x12, new Opcode(0x12, "LD (DE), A",           1,  8, () => { mmu.Write8(regs.DE, regs.A); })},
                { 0x0A, new Opcode(0x0A, "LD A, (BC)",           1,  8, () => { regs.A = mmu.Read8(regs.BC); })},
                { 0x1A, new Opcode(0x1A, "LD A, (DE)",           1,  8, () => { regs.A = mmu.Read8(regs.DE); })},
                { 0x22, new Opcode(0x22, "LD (HL+), A",          1,  8, () => { mmu.Write8(regs.HL, regs.A); regs.HL++; })},
                { 0x32, new Opcode(0x32, "LD (HL-), A",          1,  8, () => { mmu.Write8(regs.HL, regs.A); regs.HL--; })},
                { 0x2A, new Opcode(0x2A, "LD A, (HL+)",          1,  8, () => { regs.A = mmu.Read8(regs.HL); regs.HL++; })},
                { 0x3A, new Opcode(0x3A, "LD A, (HL-)",          1,  8, () => { regs.A = mmu.Read8(regs.HL); regs.HL--; })},
                { 0x46, new Opcode(0x46, "LD B, (HL)",           1,  8, () => { regs.B = mmu.Read8(regs.HL); })},
                { 0x4E, new Opcode(0x4E, "LD C, (HL)",           1,  8, () => { regs.C = mmu.Read8(regs.HL); })},
                { 0x56, new Opcode(0x56, "LD D, (HL)",           1,  8, () => { regs.D = mmu.Read8(regs.HL); })},
                { 0x5E, new Opcode(0x5E, "LD E, (HL)",           1,  8, () => { regs.E = mmu.Read8(regs.HL); })},
                { 0x66, new Opcode(0x66, "LD H, (HL)",           1,  8, () => { regs.H = mmu.Read8(regs.HL); })},
                { 0x6E, new Opcode(0x6E, "LD L, (HL)",           1,  8, () => { regs.L = mmu.Read8(regs.HL); })},
                { 0x7E, new Opcode(0x7E, "LD A, (HL)",           1,  8, () => { regs.A = mmu.Read8(regs.HL); })},
                { 0x70, new Opcode(0x70, "LD (HL), B",           1,  8, () => { mmu.Write8(regs.HL, regs.B); })},
                { 0x71, new Opcode(0x71, "LD (HL), C",           1,  8, () => { mmu.Write8(regs.HL, regs.C); })},
                { 0x72, new Opcode(0x72, "LD (HL), D",           1,  8, () => { mmu.Write8(regs.HL, regs.D); })},
                { 0x73, new Opcode(0x73, "LD (HL), E",           1,  8, () => { mmu.Write8(regs.HL, regs.E); })},
                { 0x74, new Opcode(0x74, "LD (HL), H",           1,  8, () => { mmu.Write8(regs.HL, regs.H); })},
                { 0x75, new Opcode(0x75, "LD (HL), L",           1,  8, () => { mmu.Write8(regs.HL, regs.L); })},
                { 0x77, new Opcode(0x77, "LD (HL), A",           1,  8, () => { mmu.Write8(regs.HL, regs.A); })},

                { 0xF8, new Opcode(0xF8, "LD HL, SP+${0:x2}",    2, 12, () => { regs.HL = ADD_Signed8(regs.SP, Parm8()); })},
                { 0xF9, new Opcode(0xF9, "LD SP, HL",            1,  8, () => { regs.SP = regs.HL; })},

                { 0xEA, new Opcode(0xEA, "LD (${0:x4}), A",      3, 16, () => { mmu.Write8(Parm16(), regs.A); })},
                { 0xFA, new Opcode(0xFA, "LD A, (${0:x4})",      3, 16, () => { regs.A = mmu.Read8(Parm16()); })},
                // LDH - Put memory address $FF00+n into A
                { 0xE0, new Opcode(0xE0, "LDH (${0:x2}), A",     2, 12, () => { mmu.Write8((u16)(0xFF00 + Parm8()), regs.A); })},
                { 0xF0, new Opcode(0xF0, "LDH A, (${0:x2})",     2, 12, () => { regs.A = mmu.Read8((u16)(0xFF00 + Parm8())); })},
                { 0xE2, new Opcode(0xE2, "LDH (C), A",           1,  8, () => { mmu.Write8((u16)(0xFF00 + regs.C), regs.A); })},
                { 0xF2, new Opcode(0xF2, "LDH A, (C)",           1,  8, () => { regs.A = mmu.Read8((u16)(0xFF00 + regs.C)); })},

                // ==================================================================================================================
                // STACK
                // ==================================================================================================================
                // Pop
                { 0xC1, new Opcode(0xC1, "POP BC",               1, 12, () => { regs.BC = POP(); })},
                { 0xD1, new Opcode(0xD1, "POP DE",               1, 12, () => { regs.DE = POP(); })},
                { 0xE1, new Opcode(0xE1, "POP HL",               1, 12, () => { regs.HL = POP(); })},
                { 0xF1, new Opcode(0xF1, "POP AF",               1, 12, () => { regs.AF = POP(); })},
                // Push
                { 0xC5, new Opcode(0xC5, "PUSH BC",              1, 16, () => { PUSH(regs.BC); })},
                { 0xD5, new Opcode(0xD5, "PUSH DE",              1, 16, () => { PUSH(regs.DE); })},
                { 0xE5, new Opcode(0xE5, "PUSH HL",              1, 16, () => { PUSH(regs.HL); })},
                { 0xF5, new Opcode(0xF5, "PUSH AF",              1, 16, () => { PUSH(regs.AF); })},

                // Rotate A left. Old bit 7 to Carry flag.
                { 0x07, new Opcode(0x07, "RLCA",                 1,  4, () => { regs.A = RLC(regs.A); regs.FlagZ = false; })},
                // Rotate A left through Carry flag.
                { 0x17, new Opcode(0x17, "RLA",                  1,  4, () => { regs.A = RL(regs.A); regs.FlagZ = false; })},
                // Rotate A right. Old bit 0 to Carry flag.
                { 0x0F, new Opcode(0x0F, "RRCA",                 1,  4, () => { regs.A = RRC(regs.A); regs.FlagZ = false; })},
                // Rotate A right through Carry flag.
                { 0x1F, new Opcode(0x1F, "RRA",                  1,  4, () => { regs.A = RR(regs.A); regs.FlagZ = false; })},

                // Restart - Push present address onto stack
                // Jump to address n
                { 0xC7, new Opcode(0xC7, "RST 00",               1, 16, () => { RST(0x0); })},
                { 0xCF, new Opcode(0xCF, "RST 08",               1, 16, () => { RST(0x8); })},
                { 0xD7, new Opcode(0xD7, "RST 10",               1, 16, () => { RST(0x10); })},
                { 0xDF, new Opcode(0xDF, "RST 18",               1, 16, () => { RST(0x18); })},
                { 0xE7, new Opcode(0xE7, "RST 20",               1, 16, () => { RST(0x20); })},
                { 0xEF, new Opcode(0xEF, "RST 28",               1, 16, () => { RST(0x28); })},
                { 0xF7, new Opcode(0xF7, "RST 30",               1, 16, () => { RST(0x30); })},
                { 0xFF, new Opcode(0xFF, "RST 38",               1, 16, () => { RST(0x38); })},

                // ==================================================================================================================
                // SUBTRACTION FAMILY
                // ==================================================================================================================
                // DEC 8 bit                                
                { 0x05, new Opcode(0x05, "DEC B",                1,  4, () => { regs.B = DEC(regs.B); })},
                { 0x0D, new Opcode(0x0D, "DEC C",                1,  4, () => { regs.C = DEC(regs.C); })},
                { 0x15, new Opcode(0x15, "DEC D",                1,  4, () => { regs.D = DEC(regs.D); })},
                { 0x1D, new Opcode(0x1D, "DEC E",                1,  4, () => { regs.E = DEC(regs.E); })},
                { 0x25, new Opcode(0x25, "DEC H",                1,  4, () => { regs.H = DEC(regs.H); })},
                { 0x2D, new Opcode(0x2D, "DEC L",                1,  4, () => { regs.L = DEC(regs.L); })},
                { 0x35, new Opcode(0x35, "DEC (HL)",             1, 12, () => { mmu.Write8(regs.HL, DEC(mmu.Read8(regs.HL))); })},
                { 0x3D, new Opcode(0x3D, "DEC A",                1,  4, () => { regs.A = DEC(regs.A); })},                                                          
                // DEC 16 bit                                  
                { 0x0B, new Opcode(0x0B, "DEC BC",               1,  8, () => { regs.BC--; })},
                { 0x1B, new Opcode(0x1B, "DEC DE",               1,  8, () => { regs.DE--; })},
                { 0x2B, new Opcode(0x2B, "DEC HL",               1,  8, () => { regs.HL--; })},
                { 0x3B, new Opcode(0x3B, "DEC SP",               1,  8, () => { regs.SP--; })},
                // Subtract value + Carry flag from A
                { 0x98, new Opcode(0x98, "SBC A, B",             1,  4, () => { SBC(regs.B); })},
                { 0x99, new Opcode(0x99, "SBC A, C",             1,  4, () => { SBC(regs.C); })},
                { 0x9A, new Opcode(0x9A, "SBC A, D",             1,  4, () => { SBC(regs.D); })},
                { 0x9B, new Opcode(0x9B, "SBC A, E",             1,  4, () => { SBC(regs.E); })},
                { 0x9C, new Opcode(0x9C, "SBC A, H",             1,  4, () => { SBC(regs.H); })},
                { 0x9D, new Opcode(0x9D, "SBC A, L",             1,  4, () => { SBC(regs.L); })},
                { 0x9E, new Opcode(0x9E, "SBC A, (HL)",          1,  8, () => { SBC(mmu.Read8(regs.HL)); })},
                { 0x9F, new Opcode(0x9F, "SBC A, A",             1,  4, () => { SBC(regs.A); })},
                { 0xDE, new Opcode(0xDE, "SBC A, ${0:x2}",       2,  8, () => { SBC(Parm8()); })},
                // Subtract value from A
                { 0x90, new Opcode(0x90, "SUB A, B",             1,  4, () => { SUB(regs.B); })},
                { 0x91, new Opcode(0x91, "SUB A, C",             1,  4, () => { SUB(regs.C); })},
                { 0x92, new Opcode(0x92, "SUB A, D",             1,  4, () => { SUB(regs.D); })},
                { 0x93, new Opcode(0x93, "SUB A, E",             1,  4, () => { SUB(regs.E); })},
                { 0x94, new Opcode(0x94, "SUB A, H",             1,  4, () => { SUB(regs.H); })},
                { 0x95, new Opcode(0x95, "SUB A, L",             1,  4, () => { SUB(regs.L); })},
                { 0x96, new Opcode(0x96, "SUB A, (HL)",          1,  8, () => { SUB(mmu.Read8(regs.HL)); })},
                { 0x97, new Opcode(0x97, "SUB A, A",             1,  4, () => { SUB(regs.A); })},
                { 0xD6, new Opcode(0xD6, "SUB ${0:x2}",          2,  8, () => { SUB(Parm8()); })},
                        
                // ==================================================================================================================
                // MISC
                // ==================================================================================================================
                { 0x00, new Opcode(0x00, "NOP",                  1,  4, () => { })},
                // 
                { 0x10, new Opcode(0x10, "STOP",                 1,  4, () => { STOP(); })},
                //
                { 0xCB, new Opcode(0xCB, "CB PREFIX",            1,  4, () => { })},
                // CPL - Complement A register. (Flip all bits.)
                { 0x2F, new Opcode(0x2F, "CPL",                  1,  4, () => { regs.A = (u8) ~regs.A; regs.FlagN = true; regs.FlagH = true; })},
                // Decimal adjust register A. This instruction adjusts register A so that the correct representation of Binary Coded Decimal (BCD) is obtained.
                { 0x27, new Opcode(0x27, "DAA",                  1,  4, () => { DAA(); })},
                // Set carry flag
                { 0x37, new Opcode(0x37, "SCF",                  1,  4, () => { regs.FlagN = false; regs.FlagH = false; regs.FlagC = true; })},
                // Complement carry flag
                { 0x3F, new Opcode(0x3F, "CCF",                  1,  4, () => { regs.FlagC = !regs.FlagC; regs.FlagN = false; regs.FlagH = false; })},
                // Halt CPU & LCD display until button pressed.
                { 0x76, new Opcode(0x76, "HALT",                 1,  4, () => { HALT(); })},
            };
        }

        private Dictionary<u8, Opcode> InitializeCB() {
            return new Dictionary<u8, Opcode> {
                // rotate left (one position)
                { 0x00, new Opcode(0x00, "RLC B",                2,  8, () => { regs.B = RLC(regs.B); })},
                { 0x01, new Opcode(0x01, "RLC C",                2,  8, () => { regs.C = RLC(regs.C); })},
                { 0x02, new Opcode(0x02, "RLC D",                2,  8, () => { regs.D = RLC(regs.D); })},
                { 0x03, new Opcode(0x03, "RLC E",                2,  8, () => { regs.E = RLC(regs.E); })},
                { 0x04, new Opcode(0x04, "RLC H",                2,  8, () => { regs.H = RLC(regs.H); })},
                { 0x05, new Opcode(0x05, "RLC L",                2,  8, () => { regs.L = RLC(regs.L); })},
                { 0x06, new Opcode(0x06, "RLC (HL)",             2, 16, () => { mmu.Write8(regs.HL, RLC(mmu.Read8(regs.HL))); })},
                { 0x07, new Opcode(0x07, "RLC A",                2,  8, () => { regs.A = RLC(regs.A); })},

                // rotate right (one position)
                { 0x08, new Opcode(0x08, "RRC B",                2,  8, () => { regs.B = RRC(regs.B); })},
                { 0x09, new Opcode(0x09, "RRC C",                2,  8, () => { regs.C = RRC(regs.C); })},
                { 0x0A, new Opcode(0x0A, "RRC D",                2,  8, () => { regs.D = RRC(regs.D); })},
                { 0x0B, new Opcode(0x0B, "RRC E",                2,  8, () => { regs.E = RRC(regs.E); })},
                { 0x0C, new Opcode(0x0C, "RRC H",                2,  8, () => { regs.H = RRC(regs.H); })},
                { 0x0D, new Opcode(0x0D, "RRC L",                2,  8, () => { regs.L = RRC(regs.L); })},
                { 0x0E, new Opcode(0x0E, "RRC (HL)",             2, 16, () => { mmu.Write8(regs.HL, RRC(mmu.Read8(regs.HL))); })},
                { 0x0F, new Opcode(0x0F, "RRC A",                2,  8, () => { regs.A = RRC(regs.A); })},

                // Rotate n left through Carry flag
                { 0x10, new Opcode(0x10, "RL B",                 2,  8, () => { regs.B = RL(regs.B); })},
                { 0x11, new Opcode(0x11, "RL C",                 2,  8, () => { regs.C = RL(regs.C); })},
                { 0x12, new Opcode(0x12, "RL D",                 2,  8, () => { regs.D = RL(regs.D); })},
                { 0x13, new Opcode(0x13, "RL E",                 2,  8, () => { regs.E = RL(regs.E); })},
                { 0x14, new Opcode(0x14, "RL H",                 2,  8, () => { regs.H = RL(regs.H); })},
                { 0x15, new Opcode(0x15, "RL L",                 2,  8, () => { regs.L = RL(regs.L); })},
                { 0x16, new Opcode(0x16, "RL (HL)",              2, 16, () => { mmu.Write8(regs.HL, RL(mmu.Read8(regs.HL))); })},
                { 0x17, new Opcode(0x17, "RL A",                 2,  8, () => { regs.A = RL(regs.A); })},

                // Rotate n right through Carry flag.
                { 0x18, new Opcode(0x18, "RR B",                 2,  8, () => { regs.B = RR(regs.B); })},
                { 0x19, new Opcode(0x19, "RR C",                 2,  8, () => { regs.C = RR(regs.C); })},
                { 0x1A, new Opcode(0x1A, "RR D",                 2,  8, () => { regs.D = RR(regs.D); })},
                { 0x1B, new Opcode(0x1B, "RR E",                 2,  8, () => { regs.E = RR(regs.E); })},
                { 0x1C, new Opcode(0x1C, "RR H",                 2,  8, () => { regs.H = RR(regs.H); })},
                { 0x1D, new Opcode(0x1D, "RR L",                 2,  8, () => { regs.L = RR(regs.L); })},
                { 0x1E, new Opcode(0x1E, "RR (HL)",              2, 16, () => { mmu.Write8(regs.HL, RR(mmu.Read8(regs.HL))); })},
                { 0x1F, new Opcode(0x1F, "RR A",                 2,  8, () => { regs.A = RR(regs.A); })},

                // Shift n left into Carry. LSB of n set to 0
                { 0x20, new Opcode(0x20, "SLA B",                2,  8, () => { regs.B = SLA(regs.B); })},
                { 0x21, new Opcode(0x21, "SLA C",                2,  8, () => { regs.C = SLA(regs.C); })},
                { 0x22, new Opcode(0x22, "SLA D",                2,  8, () => { regs.D = SLA(regs.D); })},
                { 0x23, new Opcode(0x23, "SLA E",                2,  8, () => { regs.E = SLA(regs.E); })},
                { 0x24, new Opcode(0x24, "SLA H",                2,  8, () => { regs.H = SLA(regs.H); })},
                { 0x25, new Opcode(0x25, "SLA L",                2,  8, () => { regs.L = SLA(regs.L); })},
                { 0x26, new Opcode(0x26, "SLA (HL)",             2, 16, () => { mmu.Write8(regs.HL, SLA(mmu.Read8(regs.HL))); })},
                { 0x27, new Opcode(0x27, "SLA A",                2,  8, () => { regs.A = SLA(regs.A); })},

                // Shift n right into Carry
                { 0x28, new Opcode(0x28, "SRA B",                2,  8, () => { regs.B = SRA(regs.B); })},
                { 0x29, new Opcode(0x29, "SRA C",                2,  8, () => { regs.C = SRA(regs.C); })},
                { 0x2A, new Opcode(0x2A, "SRA D",                2,  8, () => { regs.D = SRA(regs.D); })},
                { 0x2B, new Opcode(0x2B, "SRA E",                2,  8, () => { regs.E = SRA(regs.E); })},
                { 0x2C, new Opcode(0x2C, "SRA H",                2,  8, () => { regs.H = SRA(regs.H); })},
                { 0x2D, new Opcode(0x2D, "SRA L",                2,  8, () => { regs.L = SRA(regs.L); })},
                { 0x2E, new Opcode(0x2E, "SRA (HL)",             2, 16, () => { mmu.Write8(regs.HL, SRA(mmu.Read8(regs.HL))); })},
                { 0x2F, new Opcode(0x2F, "SRA A",                2,  8, () => { regs.A = SRA(regs.A); })},

                // Swap upper & lower nibles of n
                { 0x30, new Opcode(0x30, "SWAP B",               2,  8, () => { regs.B = SWAP(regs.B); })},
                { 0x31, new Opcode(0x31, "SWAP C",               2,  8, () => { regs.C = SWAP(regs.C); })},
                { 0x32, new Opcode(0x32, "SWAP D",               2,  8, () => { regs.D = SWAP(regs.D); })},
                { 0x33, new Opcode(0x33, "SWAP E",               2,  8, () => { regs.E = SWAP(regs.E); })},
                { 0x34, new Opcode(0x34, "SWAP H",               2,  8, () => { regs.H = SWAP(regs.H); })},
                { 0x35, new Opcode(0x35, "SWAP L",               2,  8, () => { regs.L = SWAP(regs.L); })},
                { 0x36, new Opcode(0x36, "SWAP (HL)",            2, 16, () => { mmu.Write8(regs.HL, SWAP(mmu.Read8(regs.HL))); })},
                { 0x37, new Opcode(0x37, "SWAP A",               2,  8, () => { regs.A = SWAP(regs.A); })},

                // Shift n right into Carry. MSB set to 0
                { 0x38, new Opcode(0x38, "SRL B",                2,  8, () => { regs.B = SRL(regs.B); })},
                { 0x39, new Opcode(0x39, "SRL C",                2,  8, () => { regs.C = SRL(regs.C); })},
                { 0x3A, new Opcode(0x3A, "SRL D",                2,  8, () => { regs.D = SRL(regs.D); })},
                { 0x3B, new Opcode(0x3B, "SRL E",                2,  8, () => { regs.E = SRL(regs.E); })},
                { 0x3C, new Opcode(0x3C, "SRL H",                2,  8, () => { regs.H = SRL(regs.H); })},
                { 0x3D, new Opcode(0x3D, "SRL L",                2,  8, () => { regs.L = SRL(regs.L); })},
                { 0x3E, new Opcode(0x3E, "SRL (HL)",             2, 16, () => { mmu.Write8(regs.HL, SRL(mmu.Read8(regs.HL))); })},
                { 0x3F, new Opcode(0x3F, "SRL A",                2,  8, () => { regs.A = SRL(regs.A); })},
                                                                 
                // Test bit b in register r                    
                { 0x40, new Opcode(0x40, "BIT 0, B",             2,  8, () => { BIT(regs.B, 0); })},
                { 0x41, new Opcode(0x41, "BIT 0, C",             2,  8, () => { BIT(regs.C, 0); })},
                { 0x42, new Opcode(0x42, "BIT 0, D",             2,  8, () => { BIT(regs.D, 0); })},
                { 0x43, new Opcode(0x43, "BIT 0, E",             2,  8, () => { BIT(regs.E, 0); })},
                { 0x44, new Opcode(0x44, "BIT 0, H",             2,  8, () => { BIT(regs.H, 0); })},
                { 0x45, new Opcode(0x45, "BIT 0, L",             2,  8, () => { BIT(regs.L, 0); })},
                { 0x46, new Opcode(0x46, "BIT 0, (HL)",          2, 12, () => { BIT(mmu.Read8(regs.HL), 0); })},
                { 0x47, new Opcode(0x47, "BIT 0, A",             2,  8, () => { BIT(regs.A, 0); })},
                { 0x48, new Opcode(0x48, "BIT 1, B",             2,  8, () => { BIT(regs.B, 1); })},
                { 0x49, new Opcode(0x49, "BIT 1, C",             2,  8, () => { BIT(regs.C, 1); })},
                { 0x4A, new Opcode(0x4A, "BIT 1, D",             2,  8, () => { BIT(regs.D, 1); })},
                { 0x4B, new Opcode(0x4B, "BIT 1, E",             2,  8, () => { BIT(regs.E, 1); })},
                { 0x4C, new Opcode(0x4C, "BIT 1, H",             2,  8, () => { BIT(regs.H, 1); })},
                { 0x4D, new Opcode(0x4D, "BIT 1, L",             2,  8, () => { BIT(regs.L, 1); })},
                { 0x4E, new Opcode(0x4E, "BIT 1, (HL)",          2, 12, () => { BIT(mmu.Read8(regs.HL), 1); })},
                { 0x4F, new Opcode(0x4F, "BIT 1, A",             2,  8, () => { BIT(regs.A, 1); })},
                { 0x50, new Opcode(0x50, "BIT 2, B",             2,  8, () => { BIT(regs.B, 2); })},
                { 0x51, new Opcode(0x51, "BIT 2, C",             2,  8, () => { BIT(regs.C, 2); })},
                { 0x52, new Opcode(0x52, "BIT 2, D",             2,  8, () => { BIT(regs.D, 2); })},
                { 0x53, new Opcode(0x53, "BIT 2, E",             2,  8, () => { BIT(regs.E, 2); })},
                { 0x54, new Opcode(0x54, "BIT 2, H",             2,  8, () => { BIT(regs.H, 2); })},
                { 0x55, new Opcode(0x55, "BIT 2, L",             2,  8, () => { BIT(regs.L, 2); })},
                { 0x56, new Opcode(0x56, "BIT 2, (HL)",          2, 12, () => { BIT(mmu.Read8(regs.HL), 2); })},
                { 0x57, new Opcode(0x57, "BIT 2, A",             2,  8, () => { BIT(regs.A, 2); })},
                { 0x58, new Opcode(0x58, "BIT 3, B",             2,  8, () => { BIT(regs.B, 3); })},
                { 0x59, new Opcode(0x59, "BIT 3, C",             2,  8, () => { BIT(regs.C, 3); })},
                { 0x5A, new Opcode(0x5A, "BIT 3, D",             2,  8, () => { BIT(regs.D, 3); })},
                { 0x5B, new Opcode(0x5B, "BIT 3, E",             2,  8, () => { BIT(regs.E, 3); })},
                { 0x5C, new Opcode(0x5C, "BIT 3, H",             2,  8, () => { BIT(regs.H, 3); })},
                { 0x5D, new Opcode(0x5D, "BIT 3, L",             2,  8, () => { BIT(regs.L, 3); })},
                { 0x5E, new Opcode(0x5E, "BIT 3, (HL)",          2, 12, () => { BIT(mmu.Read8(regs.HL), 3); })},
                { 0x5F, new Opcode(0x5F, "BIT 3, A",             2,  8, () => { BIT(regs.A, 3); })},
                { 0x60, new Opcode(0x60, "BIT 4, B",             2,  8, () => { BIT(regs.B, 4); })},
                { 0x61, new Opcode(0x61, "BIT 4, C",             2,  8, () => { BIT(regs.C, 4); })},
                { 0x62, new Opcode(0x62, "BIT 4, D",             2,  8, () => { BIT(regs.D, 4); })},
                { 0x63, new Opcode(0x63, "BIT 4, E",             2,  8, () => { BIT(regs.E, 4); })},
                { 0x64, new Opcode(0x64, "BIT 4, H",             2,  8, () => { BIT(regs.H, 4); })},
                { 0x65, new Opcode(0x65, "BIT 4, L",             2,  8, () => { BIT(regs.L, 4); })},
                { 0x66, new Opcode(0x66, "BIT 4, (HL)",          2, 12, () => { BIT(mmu.Read8(regs.HL), 4); })},
                { 0x67, new Opcode(0x67, "BIT 4, A",             2,  8, () => { BIT(regs.A, 4); })},
                { 0x68, new Opcode(0x68, "BIT 5, B",             2,  8, () => { BIT(regs.B, 5); })},
                { 0x69, new Opcode(0x69, "BIT 5, C",             2,  8, () => { BIT(regs.C, 5); })},
                { 0x6A, new Opcode(0x6A, "BIT 5, D",             2,  8, () => { BIT(regs.D, 5); })},
                { 0x6B, new Opcode(0x6B, "BIT 5, E",             2,  8, () => { BIT(regs.E, 5); })},
                { 0x6C, new Opcode(0x6C, "BIT 5, H",             2,  8, () => { BIT(regs.H, 5); })},
                { 0x6D, new Opcode(0x6D, "BIT 5, L",             2,  8, () => { BIT(regs.L, 5); })},
                { 0x6E, new Opcode(0x6E, "BIT 5, (HL)",          2, 12, () => { BIT(mmu.Read8(regs.HL), 5); })},
                { 0x6F, new Opcode(0x6F, "BIT 5, A",             2,  8, () => { BIT(regs.A, 5); })},
                { 0x70, new Opcode(0x70, "BIT 6, B",             2,  8, () => { BIT(regs.B, 6); })},
                { 0x71, new Opcode(0x71, "BIT 6, C",             2,  8, () => { BIT(regs.C, 6); })},
                { 0x72, new Opcode(0x72, "BIT 6, D",             2,  8, () => { BIT(regs.D, 6); })},
                { 0x73, new Opcode(0x73, "BIT 6, E",             2,  8, () => { BIT(regs.E, 6); })},
                { 0x74, new Opcode(0x74, "BIT 6, H",             2,  8, () => { BIT(regs.H, 6); })},
                { 0x75, new Opcode(0x75, "BIT 6, L",             2,  8, () => { BIT(regs.L, 6); })},
                { 0x76, new Opcode(0x76, "BIT 6, (HL)",          2, 12, () => { BIT(mmu.Read8(regs.HL), 6); })},
                { 0x77, new Opcode(0x77, "BIT 6, A",             2,  8, () => { BIT(regs.A, 6); })},
                { 0x78, new Opcode(0x78, "BIT 7, B",             2,  8, () => { BIT(regs.B, 7); })},
                { 0x79, new Opcode(0x79, "BIT 7, C",             2,  8, () => { BIT(regs.C, 7); })},
                { 0x7A, new Opcode(0x7A, "BIT 7, D",             2,  8, () => { BIT(regs.D, 7); })},
                { 0x7B, new Opcode(0x7B, "BIT 7, E",             2,  8, () => { BIT(regs.E, 7); })},
                { 0x7C, new Opcode(0x7C, "BIT 7, H",             2,  8, () => { BIT(regs.H, 7); })},
                { 0x7D, new Opcode(0x7D, "BIT 7, L",             2,  8, () => { BIT(regs.L, 7); })},
                { 0x7E, new Opcode(0x7E, "BIT 7, (HL)",          2, 12, () => { BIT(mmu.Read8(regs.HL), 7); })},
                { 0x7F, new Opcode(0x7F, "BIT 7, A",             2,  8, () => { BIT(regs.A, 7); })},

                // Reset bit in value
                { 0x80, new Opcode(0x80, "RES 0, B",             2,  8, () => { regs.B = RES(regs.B, 0); })},
                { 0x81, new Opcode(0x81, "RES 0, C",             2,  8, () => { regs.C = RES(regs.C, 0); })},
                { 0x82, new Opcode(0x82, "RES 0, D",             2,  8, () => { regs.D = RES(regs.D, 0); })},
                { 0x83, new Opcode(0x83, "RES 0, E",             2,  8, () => { regs.E = RES(regs.E, 0); })},
                { 0x84, new Opcode(0x84, "RES 0, H",             2,  8, () => { regs.H = RES(regs.H, 0); })},
                { 0x85, new Opcode(0x85, "RES 0, L",             2,  8, () => { regs.L = RES(regs.L, 0); })},
                { 0x86, new Opcode(0x86, "RES 0, (HL)",          2, 16, () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 0)); })},
                { 0x87, new Opcode(0x87, "RES 0, A",             2,  8, () => { regs.A = RES(regs.A, 0); })},
                { 0x88, new Opcode(0x88, "RES 1, B",             2,  8, () => { regs.B = RES(regs.B, 1); })},
                { 0x89, new Opcode(0x89, "RES 1, C",             2,  8, () => { regs.C = RES(regs.C, 1); })},
                { 0x8A, new Opcode(0x8A, "RES 1, D",             2,  8, () => { regs.D = RES(regs.D, 1); })},
                { 0x8B, new Opcode(0x8B, "RES 1, E",             2,  8, () => { regs.E = RES(regs.E, 1); })},
                { 0x8C, new Opcode(0x8C, "RES 1, H",             2,  8, () => { regs.H = RES(regs.H, 1); })},
                { 0x8D, new Opcode(0x8D, "RES 1, L",             2,  8, () => { regs.L = RES(regs.L, 1); })},
                { 0x8E, new Opcode(0x8E, "RES 1, (HL)",          2, 16, () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 1)); })},
                { 0x8F, new Opcode(0x8F, "RES 1, A",             2,  8, () => { regs.A = RES(regs.A, 1); })},
                { 0x90, new Opcode(0x90, "RES 2, B",             2,  8, () => { regs.B = RES(regs.B, 2); })},
                { 0x91, new Opcode(0x91, "RES 2, C",             2,  8, () => { regs.C = RES(regs.C, 2); })},
                { 0x92, new Opcode(0x92, "RES 2, D",             2,  8, () => { regs.D = RES(regs.D, 2); })},
                { 0x93, new Opcode(0x93, "RES 2, E",             2,  8, () => { regs.E = RES(regs.E, 2); })},
                { 0x94, new Opcode(0x94, "RES 2, H",             2,  8, () => { regs.H = RES(regs.H, 2); })},
                { 0x95, new Opcode(0x95, "RES 2, L",             2,  8, () => { regs.L = RES(regs.L, 2); })},
                { 0x96, new Opcode(0x96, "RES 2, (HL)",          2, 16, () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 2)); })},
                { 0x97, new Opcode(0x97, "RES 2, A",             2,  8, () => { regs.A = RES(regs.A, 2); })},
                { 0x98, new Opcode(0x98, "RES 3, B",             2,  8, () => { regs.B = RES(regs.B, 3); })},
                { 0x99, new Opcode(0x99, "RES 3, C",             2,  8, () => { regs.C = RES(regs.C, 3); })},
                { 0x9A, new Opcode(0x9A, "RES 3, D",             2,  8, () => { regs.D = RES(regs.D, 3); })},
                { 0x9B, new Opcode(0x9B, "RES 3, E",             2,  8, () => { regs.E = RES(regs.E, 3); })},
                { 0x9C, new Opcode(0x9C, "RES 3, H",             2,  8, () => { regs.H = RES(regs.H, 3); })},
                { 0x9D, new Opcode(0x9D, "RES 3, L",             2,  8, () => { regs.L = RES(regs.L, 3); })},
                { 0x9E, new Opcode(0x9E, "RES 3, (HL)",          2, 16, () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 3)); })},
                { 0x9F, new Opcode(0x9F, "RES 3, A",             2,  8, () => { regs.A = RES(regs.A, 3); })},
                { 0xA0, new Opcode(0xA0, "RES 4, B",             2,  8, () => { regs.B = RES(regs.B, 4); })},
                { 0xA1, new Opcode(0xA1, "RES 4, C",             2,  8, () => { regs.C = RES(regs.C, 4); })},
                { 0xA2, new Opcode(0xA2, "RES 4, D",             2,  8, () => { regs.D = RES(regs.D, 4); })},
                { 0xA3, new Opcode(0xA3, "RES 4, E",             2,  8, () => { regs.E = RES(regs.E, 4); })},
                { 0xA4, new Opcode(0xA4, "RES 4, H",             2,  8, () => { regs.H = RES(regs.H, 4); })},
                { 0xA5, new Opcode(0xA5, "RES 4, L",             2,  8, () => { regs.L = RES(regs.L, 4); })},
                { 0xA6, new Opcode(0xA6, "RES 4, (HL)",          2, 16, () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 4)); })},
                { 0xA7, new Opcode(0xA7, "RES 4, A",             2,  8, () => { regs.A = RES(regs.A, 4); })},
                { 0xA8, new Opcode(0xA8, "RES 5, B",             2,  8, () => { regs.B = RES(regs.B, 5); })},
                { 0xA9, new Opcode(0xA9, "RES 5, C",             2,  8, () => { regs.C = RES(regs.C, 5); })},
                { 0xAA, new Opcode(0xAA, "RES 5, D",             2,  8, () => { regs.D = RES(regs.D, 5); })},
                { 0xAB, new Opcode(0xAB, "RES 5, E",             2,  8, () => { regs.E = RES(regs.E, 5); })},
                { 0xAC, new Opcode(0xAC, "RES 5, H",             2,  8, () => { regs.H = RES(regs.H, 5); })},
                { 0xAD, new Opcode(0xAD, "RES 5, L",             2,  8, () => { regs.L = RES(regs.L, 5); })},
                { 0xAE, new Opcode(0xAE, "RES 5, (HL)",          2, 16, () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 5)); })},
                { 0xAF, new Opcode(0xAF, "RES 5, A",             2,  8, () => { regs.A = RES(regs.A, 5); })},
                { 0xB0, new Opcode(0xB0, "RES 6, B",             2,  8, () => { regs.B = RES(regs.B, 6); })},
                { 0xB1, new Opcode(0xB1, "RES 6, C",             2,  8, () => { regs.C = RES(regs.C, 6); })},
                { 0xB2, new Opcode(0xB2, "RES 6, D",             2,  8, () => { regs.D = RES(regs.D, 6); })},
                { 0xB3, new Opcode(0xB3, "RES 6, E",             2,  8, () => { regs.E = RES(regs.E, 6); })},
                { 0xB4, new Opcode(0xB4, "RES 6, H",             2,  8, () => { regs.H = RES(regs.H, 6); })},
                { 0xB5, new Opcode(0xB5, "RES 6, L",             2,  8, () => { regs.L = RES(regs.L, 6); })},
                { 0xB6, new Opcode(0xB6, "RES 6, (HL)",          2, 16, () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 6)); })},
                { 0xB7, new Opcode(0xB7, "RES 6, A",             2,  8, () => { regs.A = RES(regs.A, 6); })},
                { 0xB8, new Opcode(0xB8, "RES 7, B",             2,  8, () => { regs.B = RES(regs.B, 7); })},
                { 0xB9, new Opcode(0xB9, "RES 7, C",             2,  8, () => { regs.C = RES(regs.C, 7); })},
                { 0xBA, new Opcode(0xBA, "RES 7, D",             2,  8, () => { regs.D = RES(regs.D, 7); })},
                { 0xBB, new Opcode(0xBB, "RES 7, E",             2,  8, () => { regs.E = RES(regs.E, 7); })},
                { 0xBC, new Opcode(0xBC, "RES 7, H",             2,  8, () => { regs.H = RES(regs.H, 7); })},
                { 0xBD, new Opcode(0xBD, "RES 7, L",             2,  8, () => { regs.L = RES(regs.L, 7); })},
                { 0xBE, new Opcode(0xBE, "RES 7, (HL)",          2, 16, () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 7)); })},
                { 0xBF, new Opcode(0xBF, "RES 7, A",             2,  8, () => { regs.A = RES(regs.A, 7); })},

                // Set bit in value
                { 0xC0, new Opcode(0xC0, "SET 0, B",             2,  8, () => { regs.B = SET(regs.B, 0); })},
                { 0xC1, new Opcode(0xC1, "SET 0, C",             2,  8, () => { regs.C = SET(regs.C, 0); })},
                { 0xC2, new Opcode(0xC2, "SET 0, D",             2,  8, () => { regs.D = SET(regs.D, 0); })},
                { 0xC3, new Opcode(0xC3, "SET 0, E",             2,  8, () => { regs.E = SET(regs.E, 0); })},
                { 0xC4, new Opcode(0xC4, "SET 0, H",             2,  8, () => { regs.H = SET(regs.H, 0); })},
                { 0xC5, new Opcode(0xC5, "SET 0, L",             2,  8, () => { regs.L = SET(regs.L, 0); })},
                { 0xC6, new Opcode(0xC6, "SET 0, (HL)",          2, 16, () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 0)); })},
                { 0xC7, new Opcode(0xC7, "SET 0, A",             2,  8, () => { regs.A = SET(regs.A, 0); })},
                { 0xC8, new Opcode(0xC8, "SET 1, B",             2,  8, () => { regs.B = SET(regs.B, 1); })},
                { 0xC9, new Opcode(0xC9, "SET 1, C",             2,  8, () => { regs.C = SET(regs.C, 1); })},
                { 0xCA, new Opcode(0xCA, "SET 1, D",             2,  8, () => { regs.D = SET(regs.D, 1); })},
                { 0xCB, new Opcode(0xCB, "SET 1, E",             2,  8, () => { regs.E = SET(regs.E, 1); })},
                { 0xCC, new Opcode(0xCC, "SET 1, H",             2,  8, () => { regs.H = SET(regs.H, 1); })},
                { 0xCD, new Opcode(0xCD, "SET 1, L",             2,  8, () => { regs.L = SET(regs.L, 1); })},
                { 0xCE, new Opcode(0xCE, "SET 1, (HL)",          2, 16, () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 1)); })},
                { 0xCF, new Opcode(0xCF, "SET 1, A",             2,  8, () => { regs.A = SET(regs.A, 1); })},
                { 0xD0, new Opcode(0xD0, "SET 2, B",             2,  8, () => { regs.B = SET(regs.B, 2); })},
                { 0xD1, new Opcode(0xD1, "SET 2, C",             2,  8, () => { regs.C = SET(regs.C, 2); })},
                { 0xD2, new Opcode(0xD2, "SET 2, D",             2,  8, () => { regs.D = SET(regs.D, 2); })},
                { 0xD3, new Opcode(0xD3, "SET 2, E",             2,  8, () => { regs.E = SET(regs.E, 2); })},
                { 0xD4, new Opcode(0xD4, "SET 2, H",             2,  8, () => { regs.H = SET(regs.H, 2); })},
                { 0xD5, new Opcode(0xD5, "SET 2, L",             2,  8, () => { regs.L = SET(regs.L, 2); })},
                { 0xD6, new Opcode(0xD6, "SET 2, (HL)",          2, 16, () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 2)); })},
                { 0xD7, new Opcode(0xD7, "SET 2, A",             2,  8, () => { regs.A = SET(regs.A, 2); })},
                { 0xD8, new Opcode(0xD8, "SET 3, B",             2,  8, () => { regs.B = SET(regs.B, 3); })},
                { 0xD9, new Opcode(0xD9, "SET 3, C",             2,  8, () => { regs.C = SET(regs.C, 3); })},
                { 0xDA, new Opcode(0xDA, "SET 3, D",             2,  8, () => { regs.D = SET(regs.D, 3); })},
                { 0xDB, new Opcode(0xDB, "SET 3, E",             2,  8, () => { regs.E = SET(regs.E, 3); })},
                { 0xDC, new Opcode(0xDC, "SET 3, H",             2,  8, () => { regs.H = SET(regs.H, 3); })},
                { 0xDD, new Opcode(0xDD, "SET 3, L",             2,  8, () => { regs.L = SET(regs.L, 3); })},
                { 0xDE, new Opcode(0xDE, "SET 3, (HL)",          2, 16, () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 3)); })},
                { 0xDF, new Opcode(0xDF, "SET 3, A",             2,  8, () => { regs.A = SET(regs.A, 3); })},
                { 0xE0, new Opcode(0xE0, "SET 4, B",             2,  8, () => { regs.B = SET(regs.B, 4); })},
                { 0xE1, new Opcode(0xE1, "SET 4, C",             2,  8, () => { regs.C = SET(regs.C, 4); })},
                { 0xE2, new Opcode(0xE2, "SET 4, D",             2,  8, () => { regs.D = SET(regs.D, 4); })},
                { 0xE3, new Opcode(0xE3, "SET 4, E",             2,  8, () => { regs.E = SET(regs.E, 4); })},
                { 0xE4, new Opcode(0xE4, "SET 4, H",             2,  8, () => { regs.H = SET(regs.H, 4); })},
                { 0xE5, new Opcode(0xE5, "SET 4, L",             2,  8, () => { regs.L = SET(regs.L, 4); })},
                { 0xE6, new Opcode(0xE6, "SET 4, (HL)",          2, 16, () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 4)); })},
                { 0xE7, new Opcode(0xE7, "SET 4, A",             2,  8, () => { regs.A = SET(regs.A, 4); })},
                { 0xE8, new Opcode(0xE8, "SET 5, B",             2,  8, () => { regs.B = SET(regs.B, 5); })},
                { 0xE9, new Opcode(0xE9, "SET 5, C",             2,  8, () => { regs.C = SET(regs.C, 5); })},
                { 0xEA, new Opcode(0xEA, "SET 5, D",             2,  8, () => { regs.D = SET(regs.D, 5); })},
                { 0xEB, new Opcode(0xEB, "SET 5, E",             2,  8, () => { regs.E = SET(regs.E, 5); })},
                { 0xEC, new Opcode(0xEC, "SET 5, H",             2,  8, () => { regs.H = SET(regs.H, 5); })},
                { 0xED, new Opcode(0xED, "SET 5, L",             2,  8, () => { regs.L = SET(regs.L, 5); })},
                { 0xEE, new Opcode(0xEE, "SET 5, (HL)",          2, 16, () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 5)); })},
                { 0xEF, new Opcode(0xEF, "SET 5, A",             2,  8, () => { regs.A = SET(regs.A, 5); })},
                { 0xF0, new Opcode(0xF0, "SET 6, B",             2,  8, () => { regs.B = SET(regs.B, 6); })},
                { 0xF1, new Opcode(0xF1, "SET 6, C",             2,  8, () => { regs.C = SET(regs.C, 6); })},
                { 0xF2, new Opcode(0xF2, "SET 6, D",             2,  8, () => { regs.D = SET(regs.D, 6); })},
                { 0xF3, new Opcode(0xF3, "SET 6, E",             2,  8, () => { regs.E = SET(regs.E, 6); })},
                { 0xF4, new Opcode(0xF4, "SET 6, H",             2,  8, () => { regs.H = SET(regs.H, 6); })},
                { 0xF5, new Opcode(0xF5, "SET 6, L",             2,  8, () => { regs.L = SET(regs.L, 6); })},
                { 0xF6, new Opcode(0xF6, "SET 6, (HL)",          2, 16, () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 6)); })},
                { 0xF7, new Opcode(0xF7, "SET 6, A",             2,  8, () => { regs.A = SET(regs.A, 6); })},
                { 0xF8, new Opcode(0xF8, "SET 7, B",             2,  8, () => { regs.B = SET(regs.B, 7); })},
                { 0xF9, new Opcode(0xF9, "SET 7, C",             2,  8, () => { regs.C = SET(regs.C, 7); })},
                { 0xFA, new Opcode(0xFA, "SET 7, D",             2,  8, () => { regs.D = SET(regs.D, 7); })},
                { 0xFB, new Opcode(0xFB, "SET 7, E",             2,  8, () => { regs.E = SET(regs.E, 7); })},
                { 0xFC, new Opcode(0xFC, "SET 7, H",             2,  8, () => { regs.H = SET(regs.H, 7); })},
                { 0xFD, new Opcode(0xFD, "SET 7, L",             2,  8, () => { regs.L = SET(regs.L, 7); })},
                { 0xFE, new Opcode(0xFE, "SET 7, (HL)",          2, 16, () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 7)); })},
                { 0xFF, new Opcode(0xFF, "SET 7, A",             2,  8, () => { regs.A = SET(regs.A, 7); })},
            };
        }

        //private void HALT() {
        //    if (!IME) {
        //        if ((mmu.IE & mmu.IF & 0x1F) == 0) {
        //            halted = true;
        //            regs.PC--;
        //        }
        //        else {
        //            halted = true;
        //        }
        //    }
        //}

        //public void UpdateIME() {
        //    IME |= IMEEnabler;
        //    IMEEnabler = false;
        //}

        private void PUSH(u16 value) {
            regs.SP -= 2;
            mmu.Write16(regs.SP, value);
        }

        //private void handleInterrupts() {
        //    u8 IE = mmu.IE;
        //    u8 IF = mmu.IF;

        //    for (int i = 0; i < 5; i++) {
        //        if ((((IE & IF) >> i) & 0x1) == 1) {
        //            ExecuteInterrupt(i);
        //        }
        //    }

        //    UpdateIME();
        //}

        //public void ExecuteInterrupt(int b) {
        //    if (halted) {
        //        regs.PC++;
        //        halted = false;
        //    }
        //    if (IME) {
        //        PUSH(regs.PC);
        //        regs.PC = (ushort)(0x40 + (8 * b));
        //        IME = false;
        //        mmu.IF = bitClear(b, mmu.IF);
        //    }
        //}

        //public static byte bitClear(int n, byte v) {
        //    return v &= (byte)~(1 << n);
        //}

        private u16 POP() {
            u16 value = mmu.Read16(regs.SP);
            regs.SP += 2;
            return value;
        }

        private void CALL(bool flag, u16 address) {
            if (flag) {
                // push address of next instruction
                PUSH((ushort)(regs.PC));
                regs.PC = address;
            }
        }

        private void RET(bool flag) {
            if (flag) {
                // Pop two bytes from stack & jump to that address.
                regs.PC = POP();
            }
        }

        private void JP(bool flag, u16 address) {
            if (flag) {
                regs.PC = address;
            }
        }

        private void JR(bool flag, u8 offset) {
            if (flag) {
                // +2 because it's the size of opcode
                regs.PC = (u16)(opLocation + 2 + ToSigned(offset));
            }
        }

        private void RST(u8 b) {
            PUSH(regs.PC);
            regs.PC = b;
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
        private byte RES(u8 value, int bitPosition) {
            return (byte)(value & ~(0b_0000_0001 << bitPosition));
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

        public u8 Parm8() {
            return (u8)mmu.Read8((u16)(opLocation + 1));
        }

        public u16 Parm16() {
            return (u16)mmu.Read16((u16)(opLocation + 1));
        }
    }
}
