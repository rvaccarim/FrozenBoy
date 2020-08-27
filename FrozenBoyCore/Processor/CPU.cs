using System;
using System.Diagnostics;
using System.Collections.Generic;
using FrozenBoyCore.Memory;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Processor {
    public enum InstructionState { Fetch, FetchPrefix, WorkPending }

    public class CPU {
        public Registers regs;
        public MMU mmu;
        public Timer timer;
        public InterruptManager intManager;

        public OpcodeHandler opHandler;
        public CB_OpcodeHandler cbHandler;

        // Opcode related stuff
        public Opcode opcode;
        public Opcode prevOpcode;
        public bool cbPrefix;
        private int ticksNext = 0;

        public bool shouldLog;
        public InstructionState state;

        private int currM;
        private int totalM;

        public CPU(MMU mmu, Timer timer, InterruptManager intManager) {
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

            opHandler = new OpcodeHandler(regs, mmu, intManager);
            cbHandler = new CB_OpcodeHandler(regs, mmu);

            state = InstructionState.Fetch;
        }

        public void ExecuteNext() {
            // we do something every exactly 4 ticks
            ticksNext++;
            if (ticksNext < 4) {
                shouldLog = false;
                return;
            }
            else {
                ticksNext = 0;
            }

            HandleInterrupts();

            cbPrefix = false;

            if (state == InstructionState.Fetch) {
                currM = 0;
                opHandler.stop = false;
                shouldLog = false;

                if (intManager.halt_bug) {
                    regs.PC--;
                    intManager.halt_bug = false;
                }

                regs.OpcodePC = regs.PC;
                opcode = Disassemble();

                if (cbPrefix && opcode.value == 0x06) {
                    int z = 0;
                }

                // points to the next one even if we haven't executed it yet
                regs.PC = (u16)(regs.PC + opcode.length);

                if (cbPrefix) {
                    state = InstructionState.FetchPrefix;
                    return;
                }
                else {
                    // https://gekkio.fi/files/gb-docs/gbctr.pdf
                    // there are some ALU operations that can be completed in the same cycle as fetch (fetch / overlap)
                    if (opcode.mcycles == 1) {
                        opcode.steps[0].Invoke();
                        shouldLog = true;
                        return;
                    }
                    else {
                        // for the other operations, fetch takes one cycle 
                        totalM = opcode.mcycles - 1;
                        state = InstructionState.WorkPending;
                        return;
                    }
                }

            }

            if (state == InstructionState.FetchPrefix) {
                // we already have it, but we are still going to pretend it took time to
                // read it in this step

                // there are some ALU operations that can be completed in the same cycle as fetch (fetch / overlap)
                if (opcode.mcycles == 2) {
                    opcode.steps[0].Invoke();
                    state = InstructionState.Fetch;
                    shouldLog = true;
                    return;
                }
                else {
                    totalM = opcode.mcycles - 2;   // +1 for fecth and +1 for fetch prefix 
                    state = InstructionState.WorkPending;
                    return;
                }
            }

            if (state == InstructionState.WorkPending) {
                // execute step
                opcode.steps[currM].Invoke();
                currM++;

                if (opHandler.stop) {
                    state = InstructionState.Fetch;
                    shouldLog = true;
                    return;
                }
                else {
                    if (currM == totalM) {
                        shouldLog = true;
                        state = InstructionState.Fetch;
                    }
                }
            }
        }

        public void HandleInterrupts() {
            for (int bitPos = 0; bitPos < 5; bitPos++) {
                if ((((intManager.IE & intManager.IF) >> bitPos) & 0x1) == 1) {
                    if (intManager.halted) {
                        regs.PC++;
                        intManager.halted = false;
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

        // TODO: Move to Disassembler class
        public Opcode Disassemble() {
            u8 opcodeValue = mmu.Read8(regs.PC);

            if (opHandler.opcodes.ContainsKey(opcodeValue)) {

                opcode = opHandler.opcodes[opcodeValue];
                if (opcode.value != 0xCB) {
                    return opcode;
                }
                else {
                    u8 cbOpcodeValue = ReadParm8();

                    if (cbHandler.cbOpcodes.ContainsKey(cbOpcodeValue)) {
                        cbPrefix = true;
                        return cbHandler.cbOpcodes[cbOpcodeValue];
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

        private void PUSH(u16 value) {
            regs.SP -= 2;
            mmu.Write16(regs.SP, value);
        }

        // Reset bit in value
        private byte RES(u8 value, int bitPosition) {
            return (byte)(value & ~(0b_0000_0001 << bitPosition));
        }

        public u8 ReadParm8() {
            return (u8)mmu.Read8((u16)(regs.OpcodePC + 1));
        }

    }
}
