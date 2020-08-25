using System;
using System.Diagnostics;
using System.Collections.Generic;
using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Processor {
    public enum InstructionState { Fetch, ReadWrites, Finish }

    public class CPUX {
        public Registers regs;
        public MMU mmu;
        public Timer timer;
        public InterruptManager intManager;

        public Dictionary<u8, OpcodeX> opcodes;
        public Dictionary<u8, OpcodeX> cbOpcodes;

        // Opcode related stuff
        public OpcodeX opcode;
        public OpcodeX prevOpcode;
        public u16 opLocation;
        public bool cbPrefix;

        public bool halted = false;
        public bool halt_bug = false;

        public int instructionCycles;
        public int partialCycles = 4;
        public int deltaCycles;
        private int ticksNext = 0;

        public bool shouldLog = false;
        public InstructionState state = InstructionState.Fetch;

        public CPUX(MMU mmu, Timer timer, InterruptManager intManager) {
            this.mmu = mmu;
            this.timer = timer;
            this.intManager = intManager;

            regs = new Registers {
                AF = 0x01B0,
                BC = 0x0013,
                DE = 0x00D8,
                HL = 0x014d,
                PC = 0x100,
                SP = 0xFFFE
            };
            this.intManager.IME = false;

            opcodes = InitializeOpcodes();
            cbOpcodes = InitializeCB();
        }

        public void ExecuteNext() {
            ticksNext++;
            if (ticksNext < 4) {
                return;
            }
            else {
                ticksNext = 0;
            }

            HandleInterrupts();

            instructionCycles = 0;
            deltaCycles = 0;
            cbPrefix = false;

            switch (state) {
                case InstructionState.Fetch:
                    opLocation = regs.PC;
                    opcode = Disassemble();
                    partialCycles = 4;

                    if (halt_bug) {
                        regs.PC--;
                        halt_bug = false;
                    }

                    if (opcode != null) {
                        // points to the next one even if we haven't executed it yet
                        regs.PC = (u16)(regs.PC + opcode.length);
                    }
                    else {
                        System.Environment.Exit(0);
                    }

                    if (opcode.length == 1) {
                        goto invoke;
                    }
                    else {
                        state = InstructionState.ReadWrites;
                    }

                    break;

                case InstructionState.ReadWrites:
                    if (opcode.length > 1) {
                        if (!cbPrefix && (opcode.value == 0xEA || opcode.value == 0xFA)) {
                            partialCycles += 8;
                        }
                        else {
                            partialCycles += 4;
                        }
                    }

                    if (opcode.length == 2) {
                        goto invoke;
                    }
                    else {
                        state = InstructionState.Finish;
                    }

                    goto invoke;

                case InstructionState.Finish:
                    invoke:
                    // execute opcode
                    opcode.logic[0].Invoke();

                    instructionCycles += opcode.mcycles + deltaCycles;
                    prevOpcode = opcode;

                    shouldLog = true;
                    state = InstructionState.Fetch;
                    break;
            }

        }

        public void HandleInterrupts() {

            for (int bitPos = 0; bitPos < 5; bitPos++) {
                if ((((intManager.IE & intManager.IF) >> bitPos) & 0x1) == 1) {
                    if (halted) {
                        regs.PC++;
                        halted = false;
                    }
                    if (intManager.IME) {
                        PUSH(regs.PC);
                        regs.PC = intManager.ISR_Address[bitPos];
                        intManager.IME = false;
                        intManager.IF = RES(intManager.IF, bitPos);
                    }
                }
            }

            intManager.IME |= intManager.IME_Scheduled;
            intManager.IME_Scheduled = false;
        }

        public OpcodeX Disassemble() {
            u8 opcodeValue = mmu.Read8(regs.PC);

            if (opcodes.ContainsKey(opcodeValue)) {

                opcode = opcodes[opcodeValue];
                if (opcode.value != 0xCB) {
                    return opcode;
                }
                else {
                    u8 cbOpcodeValue = Parm8();

                    if (cbOpcodes.ContainsKey(cbOpcodeValue)) {
                        cbPrefix = true;
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
            if (!intManager.IME) {
                if ((intManager.IE & intManager.IF & 0x1F) == 0) {
                    halted = true;
                    regs.PC--;
                }
                else {
                    halt_bug = true;
                }
            }
        }

        private Dictionary<u8, OpcodeX> InitializeOpcodes() {
            return new Dictionary<u8, OpcodeX> {
                // ==================================================================================================================
                // ADDITION FAMILY
                // ==================================================================================================================
                // Add value + Carry flag to A
                { 0x88, new OpcodeX(0x88, "ADC A, B",             1,  4, new List<LogicX> { () => { ADC(regs.B); } })},
                { 0x89, new OpcodeX(0x89, "ADC A, C",             1,  4, new List<LogicX> { () => { ADC(regs.C); } })},
                { 0x8A, new OpcodeX(0x8A, "ADC A, D",             1,  4, new List<LogicX> { () => { ADC(regs.D); } })},
                { 0x8B, new OpcodeX(0x8B, "ADC A, E",             1,  4, new List<LogicX> { () => { ADC(regs.E); } })},
                { 0x8C, new OpcodeX(0x8C, "ADC A, H",             1,  4, new List<LogicX> { () => { ADC(regs.H); } })},
                { 0x8D, new OpcodeX(0x8D, "ADC A, L",             1,  4, new List<LogicX> { () => { ADC(regs.L); } })},
                { 0x8E, new OpcodeX(0x8E, "ADC A, (HL)",          1,  8, new List<LogicX> { () => { ADC(mmu.Read8(regs.HL)); } })},
                { 0x8F, new OpcodeX(0x8F, "ADC A, A",             1,  4, new List<LogicX> { () => { ADC(regs.A); } })},
                { 0xCE, new OpcodeX(0xCE, "ADC A, ${0:x2}",       2,  8, new List<LogicX> { () => { ADC(Parm8()); } })},
                // Add 16 bit
                { 0x09, new OpcodeX(0x09, "ADD HL, BC",           1,  8, new List<LogicX> { () => { ADD_HL(regs.BC); } })},
                { 0x19, new OpcodeX(0x19, "ADD HL, DE",           1,  8, new List<LogicX> { () => { ADD_HL(regs.DE); } })},
                { 0x29, new OpcodeX(0x29, "ADD HL, HL",           1,  8, new List<LogicX> { () => { ADD_HL(regs.HL); } })},
                { 0x39, new OpcodeX(0x39, "ADD HL, SP",           1,  8, new List<LogicX> { () => { ADD_HL(regs.SP); } })},
                // Add 8 bit
                { 0x80, new OpcodeX(0x80, "ADD A, B",             1,  4, new List<LogicX> { () => { ADD(regs.B); } })},
                { 0x81, new OpcodeX(0x81, "ADD A, C",             1,  4, new List<LogicX> { () => { ADD(regs.C); } })},
                { 0x82, new OpcodeX(0x82, "ADD A, D",             1,  4, new List<LogicX> { () => { ADD(regs.D); } })},
                { 0x83, new OpcodeX(0x83, "ADD A, E",             1,  4, new List<LogicX> { () => { ADD(regs.E); } })},
                { 0x84, new OpcodeX(0x84, "ADD A, H",             1,  4, new List<LogicX> { () => { ADD(regs.H); } })},
                { 0x85, new OpcodeX(0x85, "ADD A, L",             1,  4, new List<LogicX> { () => { ADD(regs.L); } })},
                { 0x86, new OpcodeX(0x86, "ADD A, (HL)",          1,  8, new List<LogicX> { () => { ADD(mmu.Read8(regs.HL)); } })},
                { 0x87, new OpcodeX(0x87, "ADD A, A",             1,  4, new List<LogicX> { () => { ADD(regs.A); } })},
                { 0xC6, new OpcodeX(0xC6, "ADD A, ${0:x2}",       2,  8, new List<LogicX> { () => { ADD(Parm8()); } })},
                { 0xE8, new OpcodeX(0xE8, "ADD SP, ${0:x2}",      2, 16, new List<LogicX> { () => { regs.SP = ADD_Signed8(regs.SP, Parm8()); } })},
                // INC - 8 bit                                  
                { 0x04, new OpcodeX(0x04, "INC B",                1,  4, new List<LogicX> { () => { regs.B = INC(regs.B); } })},
                { 0x0C, new OpcodeX(0x0C, "INC C",                1,  4, new List<LogicX> { () => { regs.C = INC(regs.C); } })},
                { 0x14, new OpcodeX(0x14, "INC D",                1,  4, new List<LogicX> { () => { regs.D = INC(regs.D); } })},
                { 0x1C, new OpcodeX(0x1C, "INC E",                1,  4, new List<LogicX> { () => { regs.E = INC(regs.E); } })},
                { 0x24, new OpcodeX(0x24, "INC H",                1,  4, new List<LogicX> { () => { regs.H = INC(regs.H); } })},
                { 0x2C, new OpcodeX(0x2C, "INC L",                1,  4, new List<LogicX> { () => { regs.L = INC(regs.L); } })},
                { 0x34, new OpcodeX(0x34, "INC (HL)",             1, 12, new List<LogicX> { () => { mmu.Write8(regs.HL, INC(mmu.Read8(regs.HL))); } })},
                { 0x3C, new OpcodeX(0x3C, "INC A",                1,  4, new List<LogicX> { () => { regs.A = INC(regs.A); } })},
                // INC - 16 bit
                { 0x03, new OpcodeX(0x03, "INC BC",               1,  8, new List<LogicX> { () => { regs.BC++; } })},
                { 0x13, new OpcodeX(0x13, "INC DE",               1,  8, new List<LogicX> { () => { regs.DE++; } })},
                { 0x23, new OpcodeX(0x23, "INC HL",               1,  8, new List<LogicX> { () => { regs.HL++; } })},
                { 0x33, new OpcodeX(0x33, "INC SP",               1,  8, new List<LogicX> { () => { regs.SP++; } })},

                // ==================================================================================================================
                // AND, OR, XOR
                // ==================================================================================================================
                { 0xA0, new OpcodeX(0xA0, "AND B",                1,  4, new List<LogicX> { () => { AND(regs.B); } })},
                { 0xA1, new OpcodeX(0xA1, "AND C",                1,  4, new List<LogicX> { () => { AND(regs.C); } })},
                { 0xA2, new OpcodeX(0xA2, "AND D",                1,  4, new List<LogicX> { () => { AND(regs.D); } })},
                { 0xA3, new OpcodeX(0xA3, "AND E",                1,  4, new List<LogicX> { () => { AND(regs.E); } })},
                { 0xA4, new OpcodeX(0xA4, "AND H",                1,  4, new List<LogicX> { () => { AND(regs.H); } })},
                { 0xA5, new OpcodeX(0xA5, "AND L",                1,  4, new List<LogicX> { () => { AND(regs.L); } })},
                { 0xA6, new OpcodeX(0xA6, "AND (HL)",             1,  8, new List<LogicX> { () => { AND(mmu.Read8(regs.HL)); } })},
                { 0xA7, new OpcodeX(0xA7, "AND A",                1,  4, new List<LogicX> { () => { AND(regs.A); } })},
                { 0xE6, new OpcodeX(0xE6, "AND ${0:x2}",          2,  8, new List<LogicX> { () => { AND(Parm8()); } })},
                // OR
                { 0xB0, new OpcodeX(0xB0, "OR B",                 1,  4, new List<LogicX> { () => { OR(regs.B); } })},
                { 0xB1, new OpcodeX(0xB1, "OR C",                 1,  4, new List<LogicX> { () => { OR(regs.C); } })},
                { 0xB2, new OpcodeX(0xB2, "OR D",                 1,  4, new List<LogicX> { () => { OR(regs.D); } })},
                { 0xB3, new OpcodeX(0xB3, "OR E",                 1,  4, new List<LogicX> { () => { OR(regs.E); } })},
                { 0xB4, new OpcodeX(0xB4, "OR H",                 1,  4, new List<LogicX> { () => { OR(regs.H); } })},
                { 0xB5, new OpcodeX(0xB5, "OR L",                 1,  4, new List<LogicX> { () => { OR(regs.L); } })},
                { 0xB6, new OpcodeX(0xB6, "OR (HL)",              1,  8, new List<LogicX> { () => { OR(mmu.Read8(regs.HL)); } })},
                { 0xB7, new OpcodeX(0xB7, "OR A",                 1,  4, new List<LogicX> { () => { OR(regs.A); } })},
                { 0xF6, new OpcodeX(0xF6, "OR ${0:x2}",           2,  8, new List<LogicX> { () => { OR(Parm8()); } })},
                // XOR
                { 0xA8, new OpcodeX(0xA8, "XOR B",                1,  4, new List<LogicX> { () => { XOR(regs.B); } })},
                { 0xA9, new OpcodeX(0xA9, "XOR C",                1,  4, new List<LogicX> { () => { XOR(regs.C); } })},
                { 0xAA, new OpcodeX(0xAA, "XOR D",                1,  4, new List<LogicX> { () => { XOR(regs.D); } })},
                { 0xAB, new OpcodeX(0xAB, "XOR E",                1,  4, new List<LogicX> { () => { XOR(regs.E); } })},
                { 0xAC, new OpcodeX(0xAC, "XOR H",                1,  4, new List<LogicX> { () => { XOR(regs.H); } })},
                { 0xAD, new OpcodeX(0xAD, "XOR L",                1,  4, new List<LogicX> { () => { XOR(regs.L); } })},
                { 0xAE, new OpcodeX(0xAE, "XOR (HL)",             1,  8, new List<LogicX> { () => { XOR(mmu.Read8(regs.HL)); ; } })},
                { 0xAF, new OpcodeX(0xAF, "XOR A",                1,  4, new List<LogicX> { () => { XOR(regs.A); } })},
                { 0xEE, new OpcodeX(0xEE, "XOR ${0:x2}",          2,  8, new List<LogicX> { () => { XOR(Parm8()); } })},

                // ==================================================================================================================
                // CALL AND RETURN
                // ==================================================================================================================
                // Push address of next instruction onto stack and then jump to address nn.
                { 0xCD, new OpcodeX(0xCD, "CALL ${0:x4}",         3, 24, new List<LogicX> { () => { CALL(true, Parm16()); } })},
                { 0xC4, new OpcodeX(0xC4, "CALL NZ, ${0:x4}",     3, 24, new List<LogicX> { () => { CALL(!regs.FlagZ, Parm16()); } })},
                { 0xCC, new OpcodeX(0xCC, "CALL Z, ${0:x4}",      3, 24, new List<LogicX> { () => { CALL( regs.FlagZ, Parm16()); } })},
                { 0xD4, new OpcodeX(0xD4, "CALL NC, ${0:x4}",     3, 24, new List<LogicX> { () => { CALL(!regs.FlagC, Parm16()); } })},
                { 0xDC, new OpcodeX(0xDC, "CALL C, ${0:x4}",      3, 24, new List<LogicX> { () => { CALL( regs.FlagC, Parm16()); } })},
                // Return - Pop two bytes from stack & jump to that address.
                { 0xC0, new OpcodeX(0xC0, "RET NZ",               1, 20, new List<LogicX> { () => { RET_CONDITIONAL(!regs.FlagZ); } })},
                { 0xC8, new OpcodeX(0xC8, "RET Z",                1, 20, new List<LogicX> { () => { RET_CONDITIONAL( regs.FlagZ); } })},
                { 0xD0, new OpcodeX(0xD0, "RET NC",               1, 20, new List<LogicX> { () => { RET_CONDITIONAL(!regs.FlagC); } })},
                { 0xD8, new OpcodeX(0xD8, "RET C",                1, 20, new List<LogicX> { () => { RET_CONDITIONAL( regs.FlagC); } })},
                // Different function because RET has different timing information
                { 0xC9, new OpcodeX(0xC9, "RET",                  1, 16, new List<LogicX> { () => { RET(); } })},
                // Pop two bytes from stack & jump to that address then enable interrupts
                { 0xD9, new OpcodeX(0xD9, "RETI",                 1, 16, new List<LogicX> { () => { RET(); intManager.IME = true; } })},

                // ==================================================================================================================
                // COMPARE
                // ==================================================================================================================
                // Compare A with n. This is basically an A - n subtraction instruction but the results are thrown away
                { 0xB8, new OpcodeX(0xB8, "CP B",                 1,  4, new List<LogicX> { () => { CP(regs.B); } })},
                { 0xB9, new OpcodeX(0xB9, "CP C",                 1,  4, new List<LogicX> { () => { CP(regs.C); } })},
                { 0xBA, new OpcodeX(0xBA, "CP D",                 1,  4, new List<LogicX> { () => { CP(regs.D); } })},
                { 0xBB, new OpcodeX(0xBB, "CP E",                 1,  4, new List<LogicX> { () => { CP(regs.E); } })},
                { 0xBC, new OpcodeX(0xBC, "CP H",                 1,  4, new List<LogicX> { () => { CP(regs.H); } })},
                { 0xBD, new OpcodeX(0xBD, "CP L",                 1,  4, new List<LogicX> { () => { CP(regs.L); } })},
                { 0xBE, new OpcodeX(0xBE, "CP (HL)",              1,  8, new List<LogicX> { () => { CP(mmu.Read8(regs.HL)); } })},
                { 0xBF, new OpcodeX(0xBF, "CP A",                 1,  4, new List<LogicX> { () => { CP(regs.A); } })},
                { 0xFE, new OpcodeX(0xFE, "CP ${0:x2}",           2,  8, new List<LogicX> { () => { CP(Parm8()); } })},

                // ==================================================================================================================
                // INTERRUPTS
                // ==================================================================================================================
                // Disables interrupt handling by setting IME=0 
                { 0xF3, new OpcodeX(0xF3, "DI",                   1,  4, new List<LogicX> { () => { intManager.IME = false; } })},
                // Schedules interrupt handling to be enabled
                { 0xFB, new OpcodeX(0xFB, "EI",                   1,  4, new List<LogicX> { () => { intManager.IME_Scheduled = true; } })},
                                        
                // ==================================================================================================================
                // JUMP FAMILY
                // ==================================================================================================================
                // JP - Jump to location
                { 0xC3, new OpcodeX(0xC3, "JP ${0:x4}",           3, 16, new List<LogicX> { () => { JP(Parm16()); } })},
                { 0xE9, new OpcodeX(0xE9, "JP (HL)",              1,  4, new List<LogicX> { () => { JP(regs.HL); } })},
                { 0xC2, new OpcodeX(0xC2, "JP NZ, ${0:x4}",       3, 16, new List<LogicX> { () => { JP_CONDITIONAL(!regs.FlagZ, Parm16()); } })},
                { 0xCA, new OpcodeX(0xCA, "JP Z, ${0:x4}",        3, 16, new List<LogicX> { () => { JP_CONDITIONAL( regs.FlagZ, Parm16()); } })},
                { 0xD2, new OpcodeX(0xD2, "JP NC, ${0:x4}",       3, 16, new List<LogicX> { () => { JP_CONDITIONAL(!regs.FlagC, Parm16()); } })},
                { 0xDA, new OpcodeX(0xDA, "JP C, ${0:x4}",        3, 16, new List<LogicX> { () => { JP_CONDITIONAL( regs.FlagC, Parm16()); } })},
                // Jump to location relative to the current location
                { 0x18, new OpcodeX(0x18, "JR ${0:x2}",           2, 12, new List<LogicX> { () => { JR(Parm8()); } })},
                { 0x20, new OpcodeX(0x20, "JR NZ, ${0:x2}",       2, 12, new List<LogicX> { () => { JR_CONDITIONAL(!regs.FlagZ, Parm8()); } })},
                { 0x28, new OpcodeX(0x28, "JR Z, ${0:x2}",        2, 12, new List<LogicX> { () => { JR_CONDITIONAL( regs.FlagZ, Parm8()); } })},
                { 0x30, new OpcodeX(0x30, "JR NC, ${0:x2}",       2, 12, new List<LogicX> { () => { JR_CONDITIONAL(!regs.FlagC, Parm8()); } })},
                { 0x38, new OpcodeX(0x38, "JR C, ${0:x2}",        2, 12, new List<LogicX> { () => { JR_CONDITIONAL( regs.FlagC, Parm8()); } })},

                // ==================================================================================================================
                // LOAD FANILY
                // ==================================================================================================================
                // load direct value into register - 8 bit
                { 0x06, new OpcodeX(0x06, "LD B, ${0:x2}",        2,  8, new List<LogicX> { () => { regs.B = Parm8(); } })},
                { 0x0E, new OpcodeX(0x0E, "LD C, ${0:x2}",        2,  8, new List<LogicX> { () => { regs.C = Parm8(); } })},
                { 0x16, new OpcodeX(0x16, "LD D, ${0:x2}",        2,  8, new List<LogicX> { () => { regs.D = Parm8(); } })},
                { 0x1E, new OpcodeX(0x1E, "LD E, ${0:x2}",        2,  8, new List<LogicX> { () => { regs.E = Parm8(); } })},
                { 0x26, new OpcodeX(0x26, "LD H, ${0:x2}",        2,  8, new List<LogicX> { () => { regs.H = Parm8(); } })},
                { 0x2E, new OpcodeX(0x2E, "LD L, ${0:x2}",        2,  8, new List<LogicX> { () => { regs.L = Parm8(); } })},
                { 0x36, new OpcodeX(0x36, "LD (HL), ${0:x2}",     2, 12, new List<LogicX> { () => { mmu.Write8(regs.HL, Parm8()); } })},
                { 0x3E, new OpcodeX(0x3E, "LD A, ${0:x2}",        2,  8, new List<LogicX> { () => { regs.A = Parm8(); } })},  
                // load direct value into register - 16 bit
                { 0x01, new OpcodeX(0x01, "LD BC, ${0:x4}",       3, 12, new List<LogicX> { () => { regs.BC = Parm16(); } })},
                { 0x11, new OpcodeX(0x11, "LD DE, ${0:x4}",       3, 12, new List<LogicX> { () => { regs.DE = Parm16(); } })},
                { 0x21, new OpcodeX(0x21, "LD HL, ${0:x4}",       3, 12, new List<LogicX> { () => { regs.HL = Parm16(); } })},
                { 0x31, new OpcodeX(0x31, "LD SP, ${0:x4}",       3, 12, new List<LogicX> { () => { regs.SP = Parm16(); } })},
                // load register to register
                { 0x41, new OpcodeX(0x41, "LD B, C",              1,  4, new List<LogicX> { () => { regs.B = regs.C; } })},
                { 0x40, new OpcodeX(0x40, "LD B, B",              1,  4, new List<LogicX> { () => { } })},
                { 0x42, new OpcodeX(0x42, "LD B, D",              1,  4, new List<LogicX> { () => { regs.B = regs.D; } })},
                { 0x43, new OpcodeX(0x43, "LD B, E",              1,  4, new List<LogicX> { () => { regs.B = regs.E; } })},
                { 0x44, new OpcodeX(0x44, "LD B, H",              1,  4, new List<LogicX> { () => { regs.B = regs.H; } })},
                { 0x45, new OpcodeX(0x45, "LD B, L",              1,  4, new List<LogicX> { () => { regs.B = regs.L; } })},
                { 0x47, new OpcodeX(0x47, "LD B, A",              1,  4, new List<LogicX> { () => { regs.B = regs.A; } })},
                { 0x48, new OpcodeX(0x48, "LD C, B",              1,  4, new List<LogicX> { () => { regs.C = regs.B; } })},
                { 0x49, new OpcodeX(0x49, "LD C, C",              1,  4, new List<LogicX> { () => { } })},
                { 0x4A, new OpcodeX(0x4A, "LD C, D",              1,  4, new List<LogicX> { () => { regs.C = regs.D; } })},
                { 0x4B, new OpcodeX(0x4B, "LD C, E",              1,  4, new List<LogicX> { () => { regs.C = regs.E; } })},
                { 0x4C, new OpcodeX(0x4C, "LD C, H",              1,  4, new List<LogicX> { () => { regs.C = regs.H; } })},
                { 0x4D, new OpcodeX(0x4D, "LD C, L",              1,  4, new List<LogicX> { () => { regs.C = regs.L; } })},
                { 0x4F, new OpcodeX(0x4F, "LD C, A",              1,  4, new List<LogicX> { () => { regs.C = regs.A; } })},
                { 0x50, new OpcodeX(0x50, "LD D, B",              1,  4, new List<LogicX> { () => { regs.D = regs.B; } })},
                { 0x51, new OpcodeX(0x51, "LD D, C",              1,  4, new List<LogicX> { () => { regs.D = regs.C; } })},
                { 0x52, new OpcodeX(0x52, "LD D, D",              1,  4, new List<LogicX> { () => { } })},
                { 0x53, new OpcodeX(0x53, "LD D, E",              1,  4, new List<LogicX> { () => { regs.D = regs.E; } })},
                { 0x54, new OpcodeX(0x54, "LD D, H",              1,  4, new List<LogicX> { () => { regs.D = regs.H; } })},
                { 0x55, new OpcodeX(0x55, "LD D, L",              1,  4, new List<LogicX> { () => { regs.D = regs.L; } })},
                { 0x57, new OpcodeX(0x57, "LD D, A",              1,  4, new List<LogicX> { () => { regs.D = regs.A; } })},
                { 0x58, new OpcodeX(0x58, "LD E, B",              1,  4, new List<LogicX> { () => { regs.E = regs.B; } })},
                { 0x59, new OpcodeX(0x59, "LD E, C",              1,  4, new List<LogicX> { () => { regs.E = regs.C; } })},
                { 0x5A, new OpcodeX(0x5A, "LD E, D",              1,  4, new List<LogicX> { () => { regs.E = regs.D; } })},
                { 0x5B, new OpcodeX(0x5B, "LD E, E",              1,  4, new List<LogicX> { () => { } })},
                { 0x5C, new OpcodeX(0x5C, "LD E, H",              1,  4, new List<LogicX> { () => { regs.E = regs.H; } })},
                { 0x5D, new OpcodeX(0x5D, "LD E, L",              1,  4, new List<LogicX> { () => { regs.E = regs.L; } })},
                { 0x5F, new OpcodeX(0x5F, "LD E, A",              1,  4, new List<LogicX> { () => { regs.E = regs.A; } })},
                { 0x60, new OpcodeX(0x60, "LD H, B",              1,  4, new List<LogicX> { () => { regs.H = regs.B; } })},
                { 0x61, new OpcodeX(0x61, "LD H, C",              1,  4, new List<LogicX> { () => { regs.H = regs.C; } })},
                { 0x62, new OpcodeX(0x62, "LD H, D",              1,  4, new List<LogicX> { () => { regs.H = regs.D; } })},
                { 0x63, new OpcodeX(0x63, "LD H, E",              1,  4, new List<LogicX> { () => { regs.H = regs.E; } })},
                { 0x64, new OpcodeX(0x64, "LD H, H",              1,  4, new List<LogicX> { () => { } })},
                { 0x65, new OpcodeX(0x65, "LD H, L",              1,  4, new List<LogicX> { () => { regs.H = regs.L; } })},
                { 0x67, new OpcodeX(0x67, "LD H, A",              1,  4, new List<LogicX> { () => { regs.H = regs.A; } })},
                { 0x68, new OpcodeX(0x68, "LD L, B",              1,  4, new List<LogicX> { () => { regs.L = regs.B; } })},
                { 0x69, new OpcodeX(0x69, "LD L, C",              1,  4, new List<LogicX> { () => { regs.L = regs.C; } })},
                { 0x6A, new OpcodeX(0x6A, "LD L, D",              1,  4, new List<LogicX> { () => { regs.L = regs.D; } })},
                { 0x6B, new OpcodeX(0x6B, "LD L, E",              1,  4, new List<LogicX> { () => { regs.L = regs.E; } })},
                { 0x6C, new OpcodeX(0x6C, "LD L, H",              1,  4, new List<LogicX> { () => { regs.L = regs.H; } })},
                { 0x6D, new OpcodeX(0x6D, "LD L, L",              1,  4, new List<LogicX> { () => {  } })},
                { 0x6F, new OpcodeX(0x6F, "LD L, A",              1,  4, new List<LogicX> { () => { regs.L = regs.A; } })},
                { 0x78, new OpcodeX(0x78, "LD A, B",              1,  4, new List<LogicX> { () => { regs.A = regs.B; } })},
                { 0x79, new OpcodeX(0x79, "LD A, C",              1,  4, new List<LogicX> { () => { regs.A = regs.C; } })},
                { 0x7A, new OpcodeX(0x7A, "LD A, D",              1,  4, new List<LogicX> { () => { regs.A = regs.D; } })},
                { 0x7B, new OpcodeX(0x7B, "LD A, E",              1,  4, new List<LogicX> { () => { regs.A = regs.E; } })},
                { 0x7C, new OpcodeX(0x7C, "LD A, H",              1,  4, new List<LogicX> { () => { regs.A = regs.H; } })},
                { 0x7D, new OpcodeX(0x7D, "LD A, L",              1,  4, new List<LogicX> { () => { regs.A = regs.L; } })},
                { 0x7F, new OpcodeX(0x7F, "LD A, A",              1,  4, new List<LogicX> { () => { } })},
                { 0x08, new OpcodeX(0x08, "LD (${0:x4}), SP",     3, 20, new List<LogicX> { () => { mmu.Write16(Parm16(), regs.SP); } })},
                { 0x02, new OpcodeX(0x02, "LD (BC), A",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.BC, regs.A); } })},
                { 0x12, new OpcodeX(0x12, "LD (DE), A",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.DE, regs.A); } })},
                { 0x0A, new OpcodeX(0x0A, "LD A, (BC)",           1,  8, new List<LogicX> { () => { regs.A = mmu.Read8(regs.BC); } })},
                { 0x1A, new OpcodeX(0x1A, "LD A, (DE)",           1,  8, new List<LogicX> { () => { regs.A = mmu.Read8(regs.DE); } })},
                { 0x22, new OpcodeX(0x22, "LD (HL+), A",          1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.A); regs.HL++; } })},
                { 0x32, new OpcodeX(0x32, "LD (HL-), A",          1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.A); regs.HL--; } })},
                { 0x2A, new OpcodeX(0x2A, "LD A, (HL+)",          1,  8, new List<LogicX> { () => { regs.A = mmu.Read8(regs.HL); regs.HL++; } })},
                { 0x3A, new OpcodeX(0x3A, "LD A, (HL-)",          1,  8, new List<LogicX> { () => { regs.A = mmu.Read8(regs.HL); regs.HL--; } })},
                { 0x46, new OpcodeX(0x46, "LD B, (HL)",           1,  8, new List<LogicX> { () => { regs.B = mmu.Read8(regs.HL); } })},
                { 0x4E, new OpcodeX(0x4E, "LD C, (HL)",           1,  8, new List<LogicX> { () => { regs.C = mmu.Read8(regs.HL); } })},
                { 0x56, new OpcodeX(0x56, "LD D, (HL)",           1,  8, new List<LogicX> { () => { regs.D = mmu.Read8(regs.HL); } })},
                { 0x5E, new OpcodeX(0x5E, "LD E, (HL)",           1,  8, new List<LogicX> { () => { regs.E = mmu.Read8(regs.HL); } })},
                { 0x66, new OpcodeX(0x66, "LD H, (HL)",           1,  8, new List<LogicX> { () => { regs.H = mmu.Read8(regs.HL); } })},
                { 0x6E, new OpcodeX(0x6E, "LD L, (HL)",           1,  8, new List<LogicX> { () => { regs.L = mmu.Read8(regs.HL); } })},
                { 0x7E, new OpcodeX(0x7E, "LD A, (HL)",           1,  8, new List<LogicX> { () => { regs.A = mmu.Read8(regs.HL); } })},
                { 0x70, new OpcodeX(0x70, "LD (HL), B",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.B); } })},
                { 0x71, new OpcodeX(0x71, "LD (HL), C",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.C); } })},
                { 0x72, new OpcodeX(0x72, "LD (HL), D",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.D); } })},
                { 0x73, new OpcodeX(0x73, "LD (HL), E",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.E); } })},
                { 0x74, new OpcodeX(0x74, "LD (HL), H",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.H); } })},
                { 0x75, new OpcodeX(0x75, "LD (HL), L",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.L); } })},
                { 0x77, new OpcodeX(0x77, "LD (HL), A",           1,  8, new List<LogicX> { () => { mmu.Write8(regs.HL, regs.A); } })},

                { 0xF8, new OpcodeX(0xF8, "LD HL, SP+${0:x2}",    2, 12, new List<LogicX> { () => { regs.HL = ADD_Signed8(regs.SP, Parm8()); } })},
                { 0xF9, new OpcodeX(0xF9, "LD SP, HL",            1,  8, new List<LogicX> { () => { regs.SP = regs.HL; } })},

                { 0xEA, new OpcodeX(0xEA, "LD (${0:x4}), A",      3, 16, new List<LogicX> { () => { mmu.Write8(Parm16(), regs.A); } })},
                { 0xFA, new OpcodeX(0xFA, "LD A, (${0:x4})",      3, 16, new List<LogicX> { () => { regs.A = mmu.Read8(Parm16()); } })},
                // LDH - Put memory address $FF00+n into A
                { 0xE0, new OpcodeX(0xE0, "LDH (${0:x2}), A",     2, 12, new List<LogicX> { () => { mmu.Write8((u16)(0xFF00 + Parm8()), regs.A); } })},
                { 0xF0, new OpcodeX(0xF0, "LDH A, (${0:x2})",     2, 12, new List<LogicX> { () => { regs.A = mmu.Read8((u16)(0xFF00 + Parm8())); } })},
                { 0xE2, new OpcodeX(0xE2, "LDH (C), A",           1,  8, new List<LogicX> { () => { mmu.Write8((u16)(0xFF00 + regs.C), regs.A); } })},
                { 0xF2, new OpcodeX(0xF2, "LDH A, (C)",           1,  8, new List<LogicX> { () => { regs.A = mmu.Read8((u16)(0xFF00 + regs.C)); } })},

                // ==================================================================================================================
                // STACK
                // ==================================================================================================================
                // Pop
                { 0xC1, new OpcodeX(0xC1, "POP BC",               1, 12, new List<LogicX> { () => { regs.BC = POP(); } })},
                { 0xD1, new OpcodeX(0xD1, "POP DE",               1, 12, new List<LogicX> { () => { regs.DE = POP(); } })},
                { 0xE1, new OpcodeX(0xE1, "POP HL",               1, 12, new List<LogicX> { () => { regs.HL = POP(); } })},
                { 0xF1, new OpcodeX(0xF1, "POP AF",               1, 12, new List<LogicX> { () => { regs.AF = POP(); } })},
                // Push
                { 0xC5, new OpcodeX(0xC5, "PUSH BC",              1, 16, new List<LogicX> { () => { PUSH(regs.BC); } })},
                { 0xD5, new OpcodeX(0xD5, "PUSH DE",              1, 16, new List<LogicX> { () => { PUSH(regs.DE); } })},
                { 0xE5, new OpcodeX(0xE5, "PUSH HL",              1, 16, new List<LogicX> { () => { PUSH(regs.HL); } })},
                { 0xF5, new OpcodeX(0xF5, "PUSH AF",              1, 16, new List<LogicX> { () => { PUSH(regs.AF); } })},

                // Rotate A left. Old bit 7 to Carry flag.
                { 0x07, new OpcodeX(0x07, "RLCA",                 1,  4, new List<LogicX> { () => { regs.A = RLC(regs.A); regs.FlagZ = false; } })},
                // Rotate A left through Carry flag.
                { 0x17, new OpcodeX(0x17, "RLA",                  1,  4, new List<LogicX> { () => { regs.A = RL(regs.A); regs.FlagZ = false; } })},
                // Rotate A right. Old bit 0 to Carry flag.
                { 0x0F, new OpcodeX(0x0F, "RRCA",                 1,  4, new List<LogicX> { () => { regs.A = RRC(regs.A); regs.FlagZ = false; } })},
                // Rotate A right through Carry flag.
                { 0x1F, new OpcodeX(0x1F, "RRA",                  1,  4, new List<LogicX> { () => { regs.A = RR(regs.A); regs.FlagZ = false; } })},

                // Restart - Push present address onto stack
                // Jump to address n
                { 0xC7, new OpcodeX(0xC7, "RST 00",               1, 16, new List<LogicX> { () => { RST(0x0); } })},
                { 0xCF, new OpcodeX(0xCF, "RST 08",               1, 16, new List<LogicX> { () => { RST(0x8); } })},
                { 0xD7, new OpcodeX(0xD7, "RST 10",               1, 16, new List<LogicX> { () => { RST(0x10); } })},
                { 0xDF, new OpcodeX(0xDF, "RST 18",               1, 16, new List<LogicX> { () => { RST(0x18); } })},
                { 0xE7, new OpcodeX(0xE7, "RST 20",               1, 16, new List<LogicX> { () => { RST(0x20); } })},
                { 0xEF, new OpcodeX(0xEF, "RST 28",               1, 16, new List<LogicX> { () => { RST(0x28); } })},
                { 0xF7, new OpcodeX(0xF7, "RST 30",               1, 16, new List<LogicX> { () => { RST(0x30); } })},
                { 0xFF, new OpcodeX(0xFF, "RST 38",               1, 16, new List<LogicX> { () => { RST(0x38); } })},

                // ==================================================================================================================
                // SUBTRACTION FAMILY
                // ==================================================================================================================
                // DEC 8 bit                                
                { 0x05, new OpcodeX(0x05, "DEC B",                1,  4, new List<LogicX> { () => { regs.B = DEC(regs.B); } })},
                { 0x0D, new OpcodeX(0x0D, "DEC C",                1,  4, new List<LogicX> { () => { regs.C = DEC(regs.C); } })},
                { 0x15, new OpcodeX(0x15, "DEC D",                1,  4, new List<LogicX> { () => { regs.D = DEC(regs.D); } })},
                { 0x1D, new OpcodeX(0x1D, "DEC E",                1,  4, new List<LogicX> { () => { regs.E = DEC(regs.E); } })},
                { 0x25, new OpcodeX(0x25, "DEC H",                1,  4, new List<LogicX> { () => { regs.H = DEC(regs.H); } })},
                { 0x2D, new OpcodeX(0x2D, "DEC L",                1,  4, new List<LogicX> { () => { regs.L = DEC(regs.L); } })},
                { 0x35, new OpcodeX(0x35, "DEC (HL)",             1, 12, new List<LogicX> { () => { mmu.Write8(regs.HL, DEC(mmu.Read8(regs.HL))); } })},
                { 0x3D, new OpcodeX(0x3D, "DEC A",                1,  4, new List<LogicX> { () => { regs.A = DEC(regs.A); } })},                                                         
                // DEC 16 bit                                  
                { 0x0B, new OpcodeX(0x0B, "DEC BC",               1,  8, new List<LogicX> { () => { regs.BC--; } })},
                { 0x1B, new OpcodeX(0x1B, "DEC DE",               1,  8, new List<LogicX> { () => { regs.DE--; } })},
                { 0x2B, new OpcodeX(0x2B, "DEC HL",               1,  8, new List<LogicX> { () => { regs.HL--; } })},
                { 0x3B, new OpcodeX(0x3B, "DEC SP",               1,  8, new List<LogicX> { () => { regs.SP--; } })},
                // Subtract value + Carry flag from A
                { 0x98, new OpcodeX(0x98, "SBC A, B",             1,  4, new List<LogicX> { () => { SBC(regs.B); } })},
                { 0x99, new OpcodeX(0x99, "SBC A, C",             1,  4, new List<LogicX> { () => { SBC(regs.C); } })},
                { 0x9A, new OpcodeX(0x9A, "SBC A, D",             1,  4, new List<LogicX> { () => { SBC(regs.D); } })},
                { 0x9B, new OpcodeX(0x9B, "SBC A, E",             1,  4, new List<LogicX> { () => { SBC(regs.E); } })},
                { 0x9C, new OpcodeX(0x9C, "SBC A, H",             1,  4, new List<LogicX> { () => { SBC(regs.H); } })},
                { 0x9D, new OpcodeX(0x9D, "SBC A, L",             1,  4, new List<LogicX> { () => { SBC(regs.L); } })},
                { 0x9E, new OpcodeX(0x9E, "SBC A, (HL)",          1,  8, new List<LogicX> { () => { SBC(mmu.Read8(regs.HL)); } })},
                { 0x9F, new OpcodeX(0x9F, "SBC A, A",             1,  4, new List<LogicX> { () => { SBC(regs.A); } })},
                { 0xDE, new OpcodeX(0xDE, "SBC A, ${0:x2}",       2,  8, new List<LogicX> { () => { SBC(Parm8()); } })},
                // Subtract value from A
                { 0x90, new OpcodeX(0x90, "SUB A, B",             1,  4, new List<LogicX> { () => { SUB(regs.B); } })},
                { 0x91, new OpcodeX(0x91, "SUB A, C",             1,  4, new List<LogicX> { () => { SUB(regs.C); } })},
                { 0x92, new OpcodeX(0x92, "SUB A, D",             1,  4, new List<LogicX> { () => { SUB(regs.D); } })},
                { 0x93, new OpcodeX(0x93, "SUB A, E",             1,  4, new List<LogicX> { () => { SUB(regs.E); } })},
                { 0x94, new OpcodeX(0x94, "SUB A, H",             1,  4, new List<LogicX> { () => { SUB(regs.H); } })},
                { 0x95, new OpcodeX(0x95, "SUB A, L",             1,  4, new List<LogicX> { () => { SUB(regs.L); } })},
                { 0x96, new OpcodeX(0x96, "SUB A, (HL)",          1,  8, new List<LogicX> { () => { SUB(mmu.Read8(regs.HL)); } })},
                { 0x97, new OpcodeX(0x97, "SUB A, A",             1,  4, new List<LogicX> { () => { SUB(regs.A); } })},
                { 0xD6, new OpcodeX(0xD6, "SUB ${0:x2}",          2,  8, new List<LogicX> { () => { SUB(Parm8()); } })},

                // ==================================================================================================================
                // MISC
                // ==================================================================================================================
                { 0x00, new OpcodeX(0x00, "NOP",                  1,  4, new List<LogicX> { () => { } })},
                // 
                { 0x10, new OpcodeX(0x10, "STOP",                 1,  4, new List<LogicX> { () => { STOP(); } })},
                //
                { 0xCB, new OpcodeX(0xCB, "CB PREFIX",            1,  4, new List<LogicX> { () => { } })},
                // CPL - Complement A register. (Flip all b }its.)
                { 0x2F, new OpcodeX(0x2F, "CPL",                  1,  4, new List<LogicX> { () => { regs.A = (u8) ~regs.A; regs.FlagN = true; regs.FlagH = true; } })},
                // Decimal adjust register A. This instruction adjusts register A so that the correct representation of Binary Coded Decimal (BCD) is obtained.
                { 0x27, new OpcodeX(0x27, "DAA",                  1,  4, new List<LogicX> { () => { DAA(); } })},
                // Set carry flag
                { 0x37, new OpcodeX(0x37, "SCF",                  1,  4, new List<LogicX> { () => { regs.FlagN = false; regs.FlagH = false; regs.FlagC = true; } })},
                // Complement carry flag
                { 0x3F, new OpcodeX(0x3F, "CCF",                  1,  4, new List<LogicX> { () => { regs.FlagC = !regs.FlagC; regs.FlagN = false; regs.FlagH = false; } })},
                // Halt CPU & LCD display until button pressed.
                { 0x76, new OpcodeX(0x76, "HALT",                 1,  4, new List<LogicX> { () => { HALT(); } })},

            };
        }

        private Dictionary<u8, OpcodeX> InitializeCB() {
            return new Dictionary<u8, OpcodeX> {
                { 0x10, new OpcodeX(0x10, "STOP",                 1,  4, new List<LogicX> { () => { STOP(); } })},
                
                // rotate left (one position)
                { 0x00, new OpcodeX(0x00, "RLC B",                2,  8, new List<LogicX> { () => { regs.B = RLC(regs.B); } })},
                { 0x01, new OpcodeX(0x01, "RLC C",                2,  8, new List<LogicX> { () => { regs.C = RLC(regs.C); } })},
                { 0x02, new OpcodeX(0x02, "RLC D",                2,  8, new List<LogicX> { () => { regs.D = RLC(regs.D); } })},
                { 0x03, new OpcodeX(0x03, "RLC E",                2,  8, new List<LogicX> { () => { regs.E = RLC(regs.E); } })},
                { 0x04, new OpcodeX(0x04, "RLC H",                2,  8, new List<LogicX> { () => { regs.H = RLC(regs.H); } })},
                { 0x05, new OpcodeX(0x05, "RLC L",                2,  8, new List<LogicX> { () => { regs.L = RLC(regs.L); } })},
                { 0x06, new OpcodeX(0x06, "RLC (HL)",             2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RLC(mmu.Read8(regs.HL))); } })},
                { 0x07, new OpcodeX(0x07, "RLC A",                2,  8, new List<LogicX> { () => { regs.A = RLC(regs.A); } })},

                // rotate right (one position)
                { 0x08, new OpcodeX(0x08, "RRC B",                2,  8, new List<LogicX> { () => { regs.B = RRC(regs.B); } })},
                { 0x09, new OpcodeX(0x09, "RRC C",                2,  8, new List<LogicX> { () => { regs.C = RRC(regs.C); } })},
                { 0x0A, new OpcodeX(0x0A, "RRC D",                2,  8, new List<LogicX> { () => { regs.D = RRC(regs.D); } })},
                { 0x0B, new OpcodeX(0x0B, "RRC E",                2,  8, new List<LogicX> { () => { regs.E = RRC(regs.E); } })},
                { 0x0C, new OpcodeX(0x0C, "RRC H",                2,  8, new List<LogicX> { () => { regs.H = RRC(regs.H); } })},
                { 0x0D, new OpcodeX(0x0D, "RRC L",                2,  8, new List<LogicX> { () => { regs.L = RRC(regs.L); } })},
                { 0x0E, new OpcodeX(0x0E, "RRC (HL)",             2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RRC(mmu.Read8(regs.HL))); } })},
                { 0x0F, new OpcodeX(0x0F, "RRC A",                2,  8, new List<LogicX> { () => { regs.A = RRC(regs.A); } })},

                // Rotate n left through Carry flag
                { 0x10, new OpcodeX(0x10, "RL B",                 2,  8, new List<LogicX> { () => { regs.B = RL(regs.B); } })},
                { 0x11, new OpcodeX(0x11, "RL C",                 2,  8, new List<LogicX> { () => { regs.C = RL(regs.C); } })},
                { 0x12, new OpcodeX(0x12, "RL D",                 2,  8, new List<LogicX> { () => { regs.D = RL(regs.D); } })},
                { 0x13, new OpcodeX(0x13, "RL E",                 2,  8, new List<LogicX> { () => { regs.E = RL(regs.E); } })},
                { 0x14, new OpcodeX(0x14, "RL H",                 2,  8, new List<LogicX> { () => { regs.H = RL(regs.H); } })},
                { 0x15, new OpcodeX(0x15, "RL L",                 2,  8, new List<LogicX> { () => { regs.L = RL(regs.L); } })},
                { 0x16, new OpcodeX(0x16, "RL (HL)",              2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RL(mmu.Read8(regs.HL))); } })},
                { 0x17, new OpcodeX(0x17, "RL A",                 2,  8, new List<LogicX> { () => { regs.A = RL(regs.A); } })},

                // Rotate n right through Carry flag.
                { 0x18, new OpcodeX(0x18, "RR B",                 2,  8, new List<LogicX> { () => { regs.B = RR(regs.B); } })},
                { 0x19, new OpcodeX(0x19, "RR C",                 2,  8, new List<LogicX> { () => { regs.C = RR(regs.C); } })},
                { 0x1A, new OpcodeX(0x1A, "RR D",                 2,  8, new List<LogicX> { () => { regs.D = RR(regs.D); } })},
                { 0x1B, new OpcodeX(0x1B, "RR E",                 2,  8, new List<LogicX> { () => { regs.E = RR(regs.E); } })},
                { 0x1C, new OpcodeX(0x1C, "RR H",                 2,  8, new List<LogicX> { () => { regs.H = RR(regs.H); } })},
                { 0x1D, new OpcodeX(0x1D, "RR L",                 2,  8, new List<LogicX> { () => { regs.L = RR(regs.L); } })},
                { 0x1E, new OpcodeX(0x1E, "RR (HL)",              2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RR(mmu.Read8(regs.HL))); } })},
                { 0x1F, new OpcodeX(0x1F, "RR A",                 2,  8, new List<LogicX> { () => { regs.A = RR(regs.A); } })},

                // Shift n left into Carry. LSB of n set to 0
                { 0x20, new OpcodeX(0x20, "SLA B",                2,  8, new List<LogicX> { () => { regs.B = SLA(regs.B); } })},
                { 0x21, new OpcodeX(0x21, "SLA C",                2,  8, new List<LogicX> { () => { regs.C = SLA(regs.C); } })},
                { 0x22, new OpcodeX(0x22, "SLA D",                2,  8, new List<LogicX> { () => { regs.D = SLA(regs.D); } })},
                { 0x23, new OpcodeX(0x23, "SLA E",                2,  8, new List<LogicX> { () => { regs.E = SLA(regs.E); } })},
                { 0x24, new OpcodeX(0x24, "SLA H",                2,  8, new List<LogicX> { () => { regs.H = SLA(regs.H); } })},
                { 0x25, new OpcodeX(0x25, "SLA L",                2,  8, new List<LogicX> { () => { regs.L = SLA(regs.L); } })},
                { 0x26, new OpcodeX(0x26, "SLA (HL)",             2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SLA(mmu.Read8(regs.HL))); } })},
                { 0x27, new OpcodeX(0x27, "SLA A",                2,  8, new List<LogicX> { () => { regs.A = SLA(regs.A); } })},

                // Shift n right into Carry
                { 0x28, new OpcodeX(0x28, "SRA B",                2,  8, new List<LogicX> { () => { regs.B = SRA(regs.B); } })},
                { 0x29, new OpcodeX(0x29, "SRA C",                2,  8, new List<LogicX> { () => { regs.C = SRA(regs.C); } })},
                { 0x2A, new OpcodeX(0x2A, "SRA D",                2,  8, new List<LogicX> { () => { regs.D = SRA(regs.D); } })},
                { 0x2B, new OpcodeX(0x2B, "SRA E",                2,  8, new List<LogicX> { () => { regs.E = SRA(regs.E); } })},
                { 0x2C, new OpcodeX(0x2C, "SRA H",                2,  8, new List<LogicX> { () => { regs.H = SRA(regs.H); } })},
                { 0x2D, new OpcodeX(0x2D, "SRA L",                2,  8, new List<LogicX> { () => { regs.L = SRA(regs.L); } })},
                { 0x2E, new OpcodeX(0x2E, "SRA (HL)",             2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SRA(mmu.Read8(regs.HL))); } })},
                { 0x2F, new OpcodeX(0x2F, "SRA A",                2,  8, new List<LogicX> { () => { regs.A = SRA(regs.A); } })},

                // Swap upper & lower nibles of n
                { 0x30, new OpcodeX(0x30, "SWAP B",               2,  8, new List<LogicX> { () => { regs.B = SWAP(regs.B); } })},
                { 0x31, new OpcodeX(0x31, "SWAP C",               2,  8, new List<LogicX> { () => { regs.C = SWAP(regs.C); } })},
                { 0x32, new OpcodeX(0x32, "SWAP D",               2,  8, new List<LogicX> { () => { regs.D = SWAP(regs.D); } })},
                { 0x33, new OpcodeX(0x33, "SWAP E",               2,  8, new List<LogicX> { () => { regs.E = SWAP(regs.E); } })},
                { 0x34, new OpcodeX(0x34, "SWAP H",               2,  8, new List<LogicX> { () => { regs.H = SWAP(regs.H); } })},
                { 0x35, new OpcodeX(0x35, "SWAP L",               2,  8, new List<LogicX> { () => { regs.L = SWAP(regs.L); } })},
                { 0x36, new OpcodeX(0x36, "SWAP (HL)",            2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SWAP(mmu.Read8(regs.HL))); } })},
                { 0x37, new OpcodeX(0x37, "SWAP A",               2,  8, new List<LogicX> { () => { regs.A = SWAP(regs.A); } })},

                // Shift n right into Carry. MSB set to 0
                { 0x38, new OpcodeX(0x38, "SRL B",                2,  8, new List<LogicX> { () => { regs.B = SRL(regs.B); } })},
                { 0x39, new OpcodeX(0x39, "SRL C",                2,  8, new List<LogicX> { () => { regs.C = SRL(regs.C); } })},
                { 0x3A, new OpcodeX(0x3A, "SRL D",                2,  8, new List<LogicX> { () => { regs.D = SRL(regs.D); } })},
                { 0x3B, new OpcodeX(0x3B, "SRL E",                2,  8, new List<LogicX> { () => { regs.E = SRL(regs.E); } })},
                { 0x3C, new OpcodeX(0x3C, "SRL H",                2,  8, new List<LogicX> { () => { regs.H = SRL(regs.H); } })},
                { 0x3D, new OpcodeX(0x3D, "SRL L",                2,  8, new List<LogicX> { () => { regs.L = SRL(regs.L); } })},
                { 0x3E, new OpcodeX(0x3E, "SRL (HL)",             2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SRL(mmu.Read8(regs.HL))); } })},
                { 0x3F, new OpcodeX(0x3F, "SRL A",                2,  8, new List<LogicX> { () => { regs.A = SRL(regs.A); } })},
                                                                 
                // Test bit b in register r                    
                { 0x40, new OpcodeX(0x40, "BIT 0, B",             2,  8, new List<LogicX> { () => { BIT(regs.B, 0); } })},
                { 0x41, new OpcodeX(0x41, "BIT 0, C",             2,  8, new List<LogicX> { () => { BIT(regs.C, 0); } })},
                { 0x42, new OpcodeX(0x42, "BIT 0, D",             2,  8, new List<LogicX> { () => { BIT(regs.D, 0); } })},
                { 0x43, new OpcodeX(0x43, "BIT 0, E",             2,  8, new List<LogicX> { () => { BIT(regs.E, 0); } })},
                { 0x44, new OpcodeX(0x44, "BIT 0, H",             2,  8, new List<LogicX> { () => { BIT(regs.H, 0); } })},
                { 0x45, new OpcodeX(0x45, "BIT 0, L",             2,  8, new List<LogicX> { () => { BIT(regs.L, 0); } })},
                { 0x46, new OpcodeX(0x46, "BIT 0, (HL)",          2, 12, new List<LogicX> { () => { BIT(mmu.Read8(regs.HL), 0); } })},
                { 0x47, new OpcodeX(0x47, "BIT 0, A",             2,  8, new List<LogicX> { () => { BIT(regs.A, 0); } })},
                { 0x48, new OpcodeX(0x48, "BIT 1, B",             2,  8, new List<LogicX> { () => { BIT(regs.B, 1); } })},
                { 0x49, new OpcodeX(0x49, "BIT 1, C",             2,  8, new List<LogicX> { () => { BIT(regs.C, 1); } })},
                { 0x4A, new OpcodeX(0x4A, "BIT 1, D",             2,  8, new List<LogicX> { () => { BIT(regs.D, 1); } })},
                { 0x4B, new OpcodeX(0x4B, "BIT 1, E",             2,  8, new List<LogicX> { () => { BIT(regs.E, 1); } })},
                { 0x4C, new OpcodeX(0x4C, "BIT 1, H",             2,  8, new List<LogicX> { () => { BIT(regs.H, 1); } })},
                { 0x4D, new OpcodeX(0x4D, "BIT 1, L",             2,  8, new List<LogicX> { () => { BIT(regs.L, 1); } })},
                { 0x4E, new OpcodeX(0x4E, "BIT 1, (HL)",          2, 12, new List<LogicX> { () => { BIT(mmu.Read8(regs.HL), 1); } })},
                { 0x4F, new OpcodeX(0x4F, "BIT 1, A",             2,  8, new List<LogicX> { () => { BIT(regs.A, 1); } })},
                { 0x50, new OpcodeX(0x50, "BIT 2, B",             2,  8, new List<LogicX> { () => { BIT(regs.B, 2); } })},
                { 0x51, new OpcodeX(0x51, "BIT 2, C",             2,  8, new List<LogicX> { () => { BIT(regs.C, 2); } })},
                { 0x52, new OpcodeX(0x52, "BIT 2, D",             2,  8, new List<LogicX> { () => { BIT(regs.D, 2); } })},
                { 0x53, new OpcodeX(0x53, "BIT 2, E",             2,  8, new List<LogicX> { () => { BIT(regs.E, 2); } })},
                { 0x54, new OpcodeX(0x54, "BIT 2, H",             2,  8, new List<LogicX> { () => { BIT(regs.H, 2); } })},
                { 0x55, new OpcodeX(0x55, "BIT 2, L",             2,  8, new List<LogicX> { () => { BIT(regs.L, 2); } })},
                { 0x56, new OpcodeX(0x56, "BIT 2, (HL)",          2, 12, new List<LogicX> { () => { BIT(mmu.Read8(regs.HL), 2); } })},
                { 0x57, new OpcodeX(0x57, "BIT 2, A",             2,  8, new List<LogicX> { () => { BIT(regs.A, 2); } })},
                { 0x58, new OpcodeX(0x58, "BIT 3, B",             2,  8, new List<LogicX> { () => { BIT(regs.B, 3); } })},
                { 0x59, new OpcodeX(0x59, "BIT 3, C",             2,  8, new List<LogicX> { () => { BIT(regs.C, 3); } })},
                { 0x5A, new OpcodeX(0x5A, "BIT 3, D",             2,  8, new List<LogicX> { () => { BIT(regs.D, 3); } })},
                { 0x5B, new OpcodeX(0x5B, "BIT 3, E",             2,  8, new List<LogicX> { () => { BIT(regs.E, 3); } })},
                { 0x5C, new OpcodeX(0x5C, "BIT 3, H",             2,  8, new List<LogicX> { () => { BIT(regs.H, 3); } })},
                { 0x5D, new OpcodeX(0x5D, "BIT 3, L",             2,  8, new List<LogicX> { () => { BIT(regs.L, 3); } })},
                { 0x5E, new OpcodeX(0x5E, "BIT 3, (HL)",          2, 12, new List<LogicX> { () => { BIT(mmu.Read8(regs.HL), 3); } })},
                { 0x5F, new OpcodeX(0x5F, "BIT 3, A",             2,  8, new List<LogicX> { () => { BIT(regs.A, 3); } })},
                { 0x60, new OpcodeX(0x60, "BIT 4, B",             2,  8, new List<LogicX> { () => { BIT(regs.B, 4); } })},
                { 0x61, new OpcodeX(0x61, "BIT 4, C",             2,  8, new List<LogicX> { () => { BIT(regs.C, 4); } })},
                { 0x62, new OpcodeX(0x62, "BIT 4, D",             2,  8, new List<LogicX> { () => { BIT(regs.D, 4); } })},
                { 0x63, new OpcodeX(0x63, "BIT 4, E",             2,  8, new List<LogicX> { () => { BIT(regs.E, 4); } })},
                { 0x64, new OpcodeX(0x64, "BIT 4, H",             2,  8, new List<LogicX> { () => { BIT(regs.H, 4); } })},
                { 0x65, new OpcodeX(0x65, "BIT 4, L",             2,  8, new List<LogicX> { () => { BIT(regs.L, 4); } })},
                { 0x66, new OpcodeX(0x66, "BIT 4, (HL)",          2, 12, new List<LogicX> { () => { BIT(mmu.Read8(regs.HL), 4); } })},
                { 0x67, new OpcodeX(0x67, "BIT 4, A",             2,  8, new List<LogicX> { () => { BIT(regs.A, 4); } })},
                { 0x68, new OpcodeX(0x68, "BIT 5, B",             2,  8, new List<LogicX> { () => { BIT(regs.B, 5); } })},
                { 0x69, new OpcodeX(0x69, "BIT 5, C",             2,  8, new List<LogicX> { () => { BIT(regs.C, 5); } })},
                { 0x6A, new OpcodeX(0x6A, "BIT 5, D",             2,  8, new List<LogicX> { () => { BIT(regs.D, 5); } })},
                { 0x6B, new OpcodeX(0x6B, "BIT 5, E",             2,  8, new List<LogicX> { () => { BIT(regs.E, 5); } })},
                { 0x6C, new OpcodeX(0x6C, "BIT 5, H",             2,  8, new List<LogicX> { () => { BIT(regs.H, 5); } })},
                { 0x6D, new OpcodeX(0x6D, "BIT 5, L",             2,  8, new List<LogicX> { () => { BIT(regs.L, 5); } })},
                { 0x6E, new OpcodeX(0x6E, "BIT 5, (HL)",          2, 12, new List<LogicX> { () => { BIT(mmu.Read8(regs.HL), 5); } })},
                { 0x6F, new OpcodeX(0x6F, "BIT 5, A",             2,  8, new List<LogicX> { () => { BIT(regs.A, 5); } })},
                { 0x70, new OpcodeX(0x70, "BIT 6, B",             2,  8, new List<LogicX> { () => { BIT(regs.B, 6); } })},
                { 0x71, new OpcodeX(0x71, "BIT 6, C",             2,  8, new List<LogicX> { () => { BIT(regs.C, 6); } })},
                { 0x72, new OpcodeX(0x72, "BIT 6, D",             2,  8, new List<LogicX> { () => { BIT(regs.D, 6); } })},
                { 0x73, new OpcodeX(0x73, "BIT 6, E",             2,  8, new List<LogicX> { () => { BIT(regs.E, 6); } })},
                { 0x74, new OpcodeX(0x74, "BIT 6, H",             2,  8, new List<LogicX> { () => { BIT(regs.H, 6); } })},
                { 0x75, new OpcodeX(0x75, "BIT 6, L",             2,  8, new List<LogicX> { () => { BIT(regs.L, 6); } })},
                { 0x76, new OpcodeX(0x76, "BIT 6, (HL)",          2, 12, new List<LogicX> { () => { BIT(mmu.Read8(regs.HL), 6); } })},
                { 0x77, new OpcodeX(0x77, "BIT 6, A",             2,  8, new List<LogicX> { () => { BIT(regs.A, 6); } })},
                { 0x78, new OpcodeX(0x78, "BIT 7, B",             2,  8, new List<LogicX> { () => { BIT(regs.B, 7); } })},
                { 0x79, new OpcodeX(0x79, "BIT 7, C",             2,  8, new List<LogicX> { () => { BIT(regs.C, 7); } })},
                { 0x7A, new OpcodeX(0x7A, "BIT 7, D",             2,  8, new List<LogicX> { () => { BIT(regs.D, 7); } })},
                { 0x7B, new OpcodeX(0x7B, "BIT 7, E",             2,  8, new List<LogicX> { () => { BIT(regs.E, 7); } })},
                { 0x7C, new OpcodeX(0x7C, "BIT 7, H",             2,  8, new List<LogicX> { () => { BIT(regs.H, 7); } })},
                { 0x7D, new OpcodeX(0x7D, "BIT 7, L",             2,  8, new List<LogicX> { () => { BIT(regs.L, 7); } })},
                { 0x7E, new OpcodeX(0x7E, "BIT 7, (HL)",          2, 12, new List<LogicX> { () => { BIT(mmu.Read8(regs.HL), 7); } })},
                { 0x7F, new OpcodeX(0x7F, "BIT 7, A",             2,  8, new List<LogicX> { () => { BIT(regs.A, 7); } })},

                // Reset bit in value
                { 0x80, new OpcodeX(0x80, "RES 0, B",             2,  8, new List<LogicX> { () => { regs.B = RES(regs.B, 0); } })},
                { 0x81, new OpcodeX(0x81, "RES 0, C",             2,  8, new List<LogicX> { () => { regs.C = RES(regs.C, 0); } })},
                { 0x82, new OpcodeX(0x82, "RES 0, D",             2,  8, new List<LogicX> { () => { regs.D = RES(regs.D, 0); } })},
                { 0x83, new OpcodeX(0x83, "RES 0, E",             2,  8, new List<LogicX> { () => { regs.E = RES(regs.E, 0); } })},
                { 0x84, new OpcodeX(0x84, "RES 0, H",             2,  8, new List<LogicX> { () => { regs.H = RES(regs.H, 0); } })},
                { 0x85, new OpcodeX(0x85, "RES 0, L",             2,  8, new List<LogicX> { () => { regs.L = RES(regs.L, 0); } })},
                { 0x86, new OpcodeX(0x86, "RES 0, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 0)); } })},
                { 0x87, new OpcodeX(0x87, "RES 0, A",             2,  8, new List<LogicX> { () => { regs.A = RES(regs.A, 0); } })},
                { 0x88, new OpcodeX(0x88, "RES 1, B",             2,  8, new List<LogicX> { () => { regs.B = RES(regs.B, 1); } })},
                { 0x89, new OpcodeX(0x89, "RES 1, C",             2,  8, new List<LogicX> { () => { regs.C = RES(regs.C, 1); } })},
                { 0x8A, new OpcodeX(0x8A, "RES 1, D",             2,  8, new List<LogicX> { () => { regs.D = RES(regs.D, 1); } })},
                { 0x8B, new OpcodeX(0x8B, "RES 1, E",             2,  8, new List<LogicX> { () => { regs.E = RES(regs.E, 1); } })},
                { 0x8C, new OpcodeX(0x8C, "RES 1, H",             2,  8, new List<LogicX> { () => { regs.H = RES(regs.H, 1); } })},
                { 0x8D, new OpcodeX(0x8D, "RES 1, L",             2,  8, new List<LogicX> { () => { regs.L = RES(regs.L, 1); } })},
                { 0x8E, new OpcodeX(0x8E, "RES 1, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 1)); } })},
                { 0x8F, new OpcodeX(0x8F, "RES 1, A",             2,  8, new List<LogicX> { () => { regs.A = RES(regs.A, 1); } })},
                { 0x90, new OpcodeX(0x90, "RES 2, B",             2,  8, new List<LogicX> { () => { regs.B = RES(regs.B, 2); } })},
                { 0x91, new OpcodeX(0x91, "RES 2, C",             2,  8, new List<LogicX> { () => { regs.C = RES(regs.C, 2); } })},
                { 0x92, new OpcodeX(0x92, "RES 2, D",             2,  8, new List<LogicX> { () => { regs.D = RES(regs.D, 2); } })},
                { 0x93, new OpcodeX(0x93, "RES 2, E",             2,  8, new List<LogicX> { () => { regs.E = RES(regs.E, 2); } })},
                { 0x94, new OpcodeX(0x94, "RES 2, H",             2,  8, new List<LogicX> { () => { regs.H = RES(regs.H, 2); } })},
                { 0x95, new OpcodeX(0x95, "RES 2, L",             2,  8, new List<LogicX> { () => { regs.L = RES(regs.L, 2); } })},
                { 0x96, new OpcodeX(0x96, "RES 2, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 2)); } })},
                { 0x97, new OpcodeX(0x97, "RES 2, A",             2,  8, new List<LogicX> { () => { regs.A = RES(regs.A, 2); } })},
                { 0x98, new OpcodeX(0x98, "RES 3, B",             2,  8, new List<LogicX> { () => { regs.B = RES(regs.B, 3); } })},
                { 0x99, new OpcodeX(0x99, "RES 3, C",             2,  8, new List<LogicX> { () => { regs.C = RES(regs.C, 3); } })},
                { 0x9A, new OpcodeX(0x9A, "RES 3, D",             2,  8, new List<LogicX> { () => { regs.D = RES(regs.D, 3); } })},
                { 0x9B, new OpcodeX(0x9B, "RES 3, E",             2,  8, new List<LogicX> { () => { regs.E = RES(regs.E, 3); } })},
                { 0x9C, new OpcodeX(0x9C, "RES 3, H",             2,  8, new List<LogicX> { () => { regs.H = RES(regs.H, 3); } })},
                { 0x9D, new OpcodeX(0x9D, "RES 3, L",             2,  8, new List<LogicX> { () => { regs.L = RES(regs.L, 3); } })},
                { 0x9E, new OpcodeX(0x9E, "RES 3, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 3)); } })},
                { 0x9F, new OpcodeX(0x9F, "RES 3, A",             2,  8, new List<LogicX> { () => { regs.A = RES(regs.A, 3); } })},
                { 0xA0, new OpcodeX(0xA0, "RES 4, B",             2,  8, new List<LogicX> { () => { regs.B = RES(regs.B, 4); } })},
                { 0xA1, new OpcodeX(0xA1, "RES 4, C",             2,  8, new List<LogicX> { () => { regs.C = RES(regs.C, 4); } })},
                { 0xA2, new OpcodeX(0xA2, "RES 4, D",             2,  8, new List<LogicX> { () => { regs.D = RES(regs.D, 4); } })},
                { 0xA3, new OpcodeX(0xA3, "RES 4, E",             2,  8, new List<LogicX> { () => { regs.E = RES(regs.E, 4); } })},
                { 0xA4, new OpcodeX(0xA4, "RES 4, H",             2,  8, new List<LogicX> { () => { regs.H = RES(regs.H, 4); } })},
                { 0xA5, new OpcodeX(0xA5, "RES 4, L",             2,  8, new List<LogicX> { () => { regs.L = RES(regs.L, 4); } })},
                { 0xA6, new OpcodeX(0xA6, "RES 4, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 4)); } })},
                { 0xA7, new OpcodeX(0xA7, "RES 4, A",             2,  8, new List<LogicX> { () => { regs.A = RES(regs.A, 4); } })},
                { 0xA8, new OpcodeX(0xA8, "RES 5, B",             2,  8, new List<LogicX> { () => { regs.B = RES(regs.B, 5); } })},
                { 0xA9, new OpcodeX(0xA9, "RES 5, C",             2,  8, new List<LogicX> { () => { regs.C = RES(regs.C, 5); } })},
                { 0xAA, new OpcodeX(0xAA, "RES 5, D",             2,  8, new List<LogicX> { () => { regs.D = RES(regs.D, 5); } })},
                { 0xAB, new OpcodeX(0xAB, "RES 5, E",             2,  8, new List<LogicX> { () => { regs.E = RES(regs.E, 5); } })},
                { 0xAC, new OpcodeX(0xAC, "RES 5, H",             2,  8, new List<LogicX> { () => { regs.H = RES(regs.H, 5); } })},
                { 0xAD, new OpcodeX(0xAD, "RES 5, L",             2,  8, new List<LogicX> { () => { regs.L = RES(regs.L, 5); } })},
                { 0xAE, new OpcodeX(0xAE, "RES 5, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 5)); } })},
                { 0xAF, new OpcodeX(0xAF, "RES 5, A",             2,  8, new List<LogicX> { () => { regs.A = RES(regs.A, 5); } })},
                { 0xB0, new OpcodeX(0xB0, "RES 6, B",             2,  8, new List<LogicX> { () => { regs.B = RES(regs.B, 6); } })},
                { 0xB1, new OpcodeX(0xB1, "RES 6, C",             2,  8, new List<LogicX> { () => { regs.C = RES(regs.C, 6); } })},
                { 0xB2, new OpcodeX(0xB2, "RES 6, D",             2,  8, new List<LogicX> { () => { regs.D = RES(regs.D, 6); } })},
                { 0xB3, new OpcodeX(0xB3, "RES 6, E",             2,  8, new List<LogicX> { () => { regs.E = RES(regs.E, 6); } })},
                { 0xB4, new OpcodeX(0xB4, "RES 6, H",             2,  8, new List<LogicX> { () => { regs.H = RES(regs.H, 6); } })},
                { 0xB5, new OpcodeX(0xB5, "RES 6, L",             2,  8, new List<LogicX> { () => { regs.L = RES(regs.L, 6); } })},
                { 0xB6, new OpcodeX(0xB6, "RES 6, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 6)); } })},
                { 0xB7, new OpcodeX(0xB7, "RES 6, A",             2,  8, new List<LogicX> { () => { regs.A = RES(regs.A, 6); } })},
                { 0xB8, new OpcodeX(0xB8, "RES 7, B",             2,  8, new List<LogicX> { () => { regs.B = RES(regs.B, 7); } })},
                { 0xB9, new OpcodeX(0xB9, "RES 7, C",             2,  8, new List<LogicX> { () => { regs.C = RES(regs.C, 7); } })},
                { 0xBA, new OpcodeX(0xBA, "RES 7, D",             2,  8, new List<LogicX> { () => { regs.D = RES(regs.D, 7); } })},
                { 0xBB, new OpcodeX(0xBB, "RES 7, E",             2,  8, new List<LogicX> { () => { regs.E = RES(regs.E, 7); } })},
                { 0xBC, new OpcodeX(0xBC, "RES 7, H",             2,  8, new List<LogicX> { () => { regs.H = RES(regs.H, 7); } })},
                { 0xBD, new OpcodeX(0xBD, "RES 7, L",             2,  8, new List<LogicX> { () => { regs.L = RES(regs.L, 7); } })},
                { 0xBE, new OpcodeX(0xBE, "RES 7, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, RES(mmu.Read8(regs.HL), 7)); } })},
                { 0xBF, new OpcodeX(0xBF, "RES 7, A",             2,  8, new List<LogicX> { () => { regs.A = RES(regs.A, 7); } })},

                // Set bit in value
                { 0xC0, new OpcodeX(0xC0, "SET 0, B",             2,  8, new List<LogicX> { () => { regs.B = SET(regs.B, 0); } })},
                { 0xC1, new OpcodeX(0xC1, "SET 0, C",             2,  8, new List<LogicX> { () => { regs.C = SET(regs.C, 0); } })},
                { 0xC2, new OpcodeX(0xC2, "SET 0, D",             2,  8, new List<LogicX> { () => { regs.D = SET(regs.D, 0); } })},
                { 0xC3, new OpcodeX(0xC3, "SET 0, E",             2,  8, new List<LogicX> { () => { regs.E = SET(regs.E, 0); } })},
                { 0xC4, new OpcodeX(0xC4, "SET 0, H",             2,  8, new List<LogicX> { () => { regs.H = SET(regs.H, 0); } })},
                { 0xC5, new OpcodeX(0xC5, "SET 0, L",             2,  8, new List<LogicX> { () => { regs.L = SET(regs.L, 0); } })},
                { 0xC6, new OpcodeX(0xC6, "SET 0, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 0)); } })},
                { 0xC7, new OpcodeX(0xC7, "SET 0, A",             2,  8, new List<LogicX> { () => { regs.A = SET(regs.A, 0); } })},
                { 0xC8, new OpcodeX(0xC8, "SET 1, B",             2,  8, new List<LogicX> { () => { regs.B = SET(regs.B, 1); } })},
                { 0xC9, new OpcodeX(0xC9, "SET 1, C",             2,  8, new List<LogicX> { () => { regs.C = SET(regs.C, 1); } })},
                { 0xCA, new OpcodeX(0xCA, "SET 1, D",             2,  8, new List<LogicX> { () => { regs.D = SET(regs.D, 1); } })},
                { 0xCB, new OpcodeX(0xCB, "SET 1, E",             2,  8, new List<LogicX> { () => { regs.E = SET(regs.E, 1); } })},
                { 0xCC, new OpcodeX(0xCC, "SET 1, H",             2,  8, new List<LogicX> { () => { regs.H = SET(regs.H, 1); } })},
                { 0xCD, new OpcodeX(0xCD, "SET 1, L",             2,  8, new List<LogicX> { () => { regs.L = SET(regs.L, 1); } })},
                { 0xCE, new OpcodeX(0xCE, "SET 1, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 1)); } })},
                { 0xCF, new OpcodeX(0xCF, "SET 1, A",             2,  8, new List<LogicX> { () => { regs.A = SET(regs.A, 1); } })},
                { 0xD0, new OpcodeX(0xD0, "SET 2, B",             2,  8, new List<LogicX> { () => { regs.B = SET(regs.B, 2); } })},
                { 0xD1, new OpcodeX(0xD1, "SET 2, C",             2,  8, new List<LogicX> { () => { regs.C = SET(regs.C, 2); } })},
                { 0xD2, new OpcodeX(0xD2, "SET 2, D",             2,  8, new List<LogicX> { () => { regs.D = SET(regs.D, 2); } })},
                { 0xD3, new OpcodeX(0xD3, "SET 2, E",             2,  8, new List<LogicX> { () => { regs.E = SET(regs.E, 2); } })},
                { 0xD4, new OpcodeX(0xD4, "SET 2, H",             2,  8, new List<LogicX> { () => { regs.H = SET(regs.H, 2); } })},
                { 0xD5, new OpcodeX(0xD5, "SET 2, L",             2,  8, new List<LogicX> { () => { regs.L = SET(regs.L, 2); } })},
                { 0xD6, new OpcodeX(0xD6, "SET 2, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 2)); } })},
                { 0xD7, new OpcodeX(0xD7, "SET 2, A",             2,  8, new List<LogicX> { () => { regs.A = SET(regs.A, 2); } })},
                { 0xD8, new OpcodeX(0xD8, "SET 3, B",             2,  8, new List<LogicX> { () => { regs.B = SET(regs.B, 3); } })},
                { 0xD9, new OpcodeX(0xD9, "SET 3, C",             2,  8, new List<LogicX> { () => { regs.C = SET(regs.C, 3); } })},
                { 0xDA, new OpcodeX(0xDA, "SET 3, D",             2,  8, new List<LogicX> { () => { regs.D = SET(regs.D, 3); } })},
                { 0xDB, new OpcodeX(0xDB, "SET 3, E",             2,  8, new List<LogicX> { () => { regs.E = SET(regs.E, 3); } })},
                { 0xDC, new OpcodeX(0xDC, "SET 3, H",             2,  8, new List<LogicX> { () => { regs.H = SET(regs.H, 3); } })},
                { 0xDD, new OpcodeX(0xDD, "SET 3, L",             2,  8, new List<LogicX> { () => { regs.L = SET(regs.L, 3); } })},
                { 0xDE, new OpcodeX(0xDE, "SET 3, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 3)); } })},
                { 0xDF, new OpcodeX(0xDF, "SET 3, A",             2,  8, new List<LogicX> { () => { regs.A = SET(regs.A, 3); } })},
                { 0xE0, new OpcodeX(0xE0, "SET 4, B",             2,  8, new List<LogicX> { () => { regs.B = SET(regs.B, 4); } })},
                { 0xE1, new OpcodeX(0xE1, "SET 4, C",             2,  8, new List<LogicX> { () => { regs.C = SET(regs.C, 4); } })},
                { 0xE2, new OpcodeX(0xE2, "SET 4, D",             2,  8, new List<LogicX> { () => { regs.D = SET(regs.D, 4); } })},
                { 0xE3, new OpcodeX(0xE3, "SET 4, E",             2,  8, new List<LogicX> { () => { regs.E = SET(regs.E, 4); } })},
                { 0xE4, new OpcodeX(0xE4, "SET 4, H",             2,  8, new List<LogicX> { () => { regs.H = SET(regs.H, 4); } })},
                { 0xE5, new OpcodeX(0xE5, "SET 4, L",             2,  8, new List<LogicX> { () => { regs.L = SET(regs.L, 4); } })},
                { 0xE6, new OpcodeX(0xE6, "SET 4, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 4)); } })},
                { 0xE7, new OpcodeX(0xE7, "SET 4, A",             2,  8, new List<LogicX> { () => { regs.A = SET(regs.A, 4); } })},
                { 0xE8, new OpcodeX(0xE8, "SET 5, B",             2,  8, new List<LogicX> { () => { regs.B = SET(regs.B, 5); } })},
                { 0xE9, new OpcodeX(0xE9, "SET 5, C",             2,  8, new List<LogicX> { () => { regs.C = SET(regs.C, 5); } })},
                { 0xEA, new OpcodeX(0xEA, "SET 5, D",             2,  8, new List<LogicX> { () => { regs.D = SET(regs.D, 5); } })},
                { 0xEB, new OpcodeX(0xEB, "SET 5, E",             2,  8, new List<LogicX> { () => { regs.E = SET(regs.E, 5); } })},
                { 0xEC, new OpcodeX(0xEC, "SET 5, H",             2,  8, new List<LogicX> { () => { regs.H = SET(regs.H, 5); } })},
                { 0xED, new OpcodeX(0xED, "SET 5, L",             2,  8, new List<LogicX> { () => { regs.L = SET(regs.L, 5); } })},
                { 0xEE, new OpcodeX(0xEE, "SET 5, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 5)); } })},
                { 0xEF, new OpcodeX(0xEF, "SET 5, A",             2,  8, new List<LogicX> { () => { regs.A = SET(regs.A, 5); } })},
                { 0xF0, new OpcodeX(0xF0, "SET 6, B",             2,  8, new List<LogicX> { () => { regs.B = SET(regs.B, 6); } })},
                { 0xF1, new OpcodeX(0xF1, "SET 6, C",             2,  8, new List<LogicX> { () => { regs.C = SET(regs.C, 6); } })},
                { 0xF2, new OpcodeX(0xF2, "SET 6, D",             2,  8, new List<LogicX> { () => { regs.D = SET(regs.D, 6); } })},
                { 0xF3, new OpcodeX(0xF3, "SET 6, E",             2,  8, new List<LogicX> { () => { regs.E = SET(regs.E, 6); } })},
                { 0xF4, new OpcodeX(0xF4, "SET 6, H",             2,  8, new List<LogicX> { () => { regs.H = SET(regs.H, 6); } })},
                { 0xF5, new OpcodeX(0xF5, "SET 6, L",             2,  8, new List<LogicX> { () => { regs.L = SET(regs.L, 6); } })},
                { 0xF6, new OpcodeX(0xF6, "SET 6, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 6)); } })},
                { 0xF7, new OpcodeX(0xF7, "SET 6, A",             2,  8, new List<LogicX> { () => { regs.A = SET(regs.A, 6); } })},
                { 0xF8, new OpcodeX(0xF8, "SET 7, B",             2,  8, new List<LogicX> { () => { regs.B = SET(regs.B, 7); } })},
                { 0xF9, new OpcodeX(0xF9, "SET 7, C",             2,  8, new List<LogicX> { () => { regs.C = SET(regs.C, 7); } })},
                { 0xFA, new OpcodeX(0xFA, "SET 7, D",             2,  8, new List<LogicX> { () => { regs.D = SET(regs.D, 7); } })},
                { 0xFB, new OpcodeX(0xFB, "SET 7, E",             2,  8, new List<LogicX> { () => { regs.E = SET(regs.E, 7); } })},
                { 0xFC, new OpcodeX(0xFC, "SET 7, H",             2,  8, new List<LogicX> { () => { regs.H = SET(regs.H, 7); } })},
                { 0xFD, new OpcodeX(0xFD, "SET 7, L",             2,  8, new List<LogicX> { () => { regs.L = SET(regs.L, 7); } })},
                { 0xFE, new OpcodeX(0xFE, "SET 7, (HL)",          2, 16, new List<LogicX> { () => { mmu.Write8(regs.HL, SET(mmu.Read8(regs.HL), 7)); } })},
                { 0xFF, new OpcodeX(0xFF, "SET 7, A",             2,  8, new List<LogicX> { () => { regs.A = SET(regs.A, 7); } })},
            };
        }

        private void PUSH(u16 value) {
            regs.SP -= 2;
            mmu.Write16(regs.SP, value);
        }

        private u16 POP() {
            u16 value = mmu.Read16(regs.SP);
            regs.SP += 2;
            return value;
        }

        private void CALL(bool flag, u16 address) {
            // without branch (12t)	with branch (24t)
            if (flag) {
                // push address of next instruction
                PUSH((ushort)(regs.PC));
                regs.PC = address;
            }
            else {
                deltaCycles = -12;
            }
        }

        private void RET_CONDITIONAL(bool flag) {
            // without branch (8t) with branch (20t)
            if (flag) {
                // Pop two bytes from stack & jump to that address.
                regs.PC = POP();
            }
            else {
                deltaCycles = -12;
            }
        }

        private void RET() {
            // RET AND RET_CONDITIONAL have different timings
            // Pop two bytes from stack & jump to that address.
            regs.PC = POP();
        }

        private void JP(u16 address) {
            regs.PC = address;
        }

        private void JP_CONDITIONAL(bool flag, u16 address) {
            // without branch (12t)	with branch (16t)
            if (flag) {
                regs.PC = address;
            }
            else {
                deltaCycles = -4;
            }
        }

        private void JR(u8 offset) {
            regs.PC = (u16)(opLocation + 2 + ToSigned(offset));
        }

        private void JR_CONDITIONAL(bool flag, u8 offset) {
            // Timing without branch(8t) with branch(12t)
            if (flag) {
                // +2 because it's the size of opcode
                regs.PC = (u16)(opLocation + 2 + ToSigned(offset));
            }
            else {
                deltaCycles = -4;
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
