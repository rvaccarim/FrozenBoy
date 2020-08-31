using System;
using System.Diagnostics;
using System.Collections.Generic;
using FrozenBoyCore.Memory;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Util;

namespace FrozenBoyCore.Processor {
    public enum InstructionState {
        Fetch, FetchPrefix, WorkPending,
        Interrupt_IF, Interrupt_IE, Interrupt_Push1, Interrupt_Push2, Interrupt_Jump,
        Halted, Stopped
    }

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
        private int executeLock = 0;
        public int interruptBit;
        public u8 lsb;
        public u8 msb;
        public bool haltBug;

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
            executeLock++;
            if (executeLock < 4) {
                shouldLog = false;
                return;
            }
            else {
                executeLock = 0;
            }

            if (state == InstructionState.Fetch || state == InstructionState.Halted || state == InstructionState.Stopped) {
                if (intManager.IME && intManager.IsInterruptRequested()) {
                    state = InstructionState.Interrupt_IF;
                }
            }

            switch (state) {
                case InstructionState.Interrupt_IF:
                case InstructionState.Interrupt_IE:
                case InstructionState.Interrupt_Push1:
                case InstructionState.Interrupt_Push2:
                case InstructionState.Interrupt_Jump:
                    HandleInterrupt();
                    return;
                case InstructionState.Halted when intManager.IsInterruptRequested():
                    state = InstructionState.Fetch;
                    break;
            }

            if (state == InstructionState.Halted || state == InstructionState.Stopped) {
                return;
            }

            cbPrefix = false;

            switch (state) {
                case InstructionState.Fetch:
                    currM = 0;
                    opHandler.stop = false;
                    shouldLog = false;

                    opcode = Disassemble();

                    if (haltBug) {
                        haltBug = false;
                    }
                    else {
                        // points to the next one even if we haven't executed it yet
                        // regs.PC = (u16)(regs.PC + opcode.length);
                        regs.OpcodePC = regs.PC;
                        regs.PC++;
                    }

                    if (cbPrefix) {
                        state = InstructionState.FetchPrefix;
                        return;
                    }
                    else {
                        // https://gekkio.fi/files/gb-docs/gbctr.pdf
                        // there are some ALU operations that can be completed in the same cycle as fetch (fetch / overlap)
                        if (opcode.mcycles == 1) {
                            if (opcode.value == 0x76) {
                                if (intManager.IsHaltBug()) {
                                    haltBug = true;
                                }
                                else {
                                    state = InstructionState.Halted;
                                }
                                return;
                            }

                            opcode.steps[0].Invoke();

                            intManager.OnInstructionFinished();
                            shouldLog = true;
                        }
                        else {
                            // for the other operations, fetch takes one cycle 
                            totalM = opcode.mcycles - 1;
                            state = InstructionState.WorkPending;
                        }
                    }
                    break;

                case InstructionState.FetchPrefix:
                    // we already have it, but we are still going to pretend it took time to read it in this step
                    // there are some ALU operations that can be completed in the same cycle as fetch (fetch / overlap)
                    regs.PC++;

                    if (opcode.mcycles == 2) {
                        opcode.steps[0].Invoke();

                        state = InstructionState.Fetch;
                        intManager.OnInstructionFinished();
                        shouldLog = true;
                    }
                    else {
                        totalM = opcode.mcycles - 2;   // +1 for fecth and +1 for fetch prefix 
                        state = InstructionState.WorkPending;
                    }
                    break;

                case InstructionState.WorkPending:
                    // execute step
                    opcode.steps[currM].Invoke();
                    currM++;

                    if (opHandler.stop) {
                        state = InstructionState.Fetch;
                        intManager.OnInstructionFinished();
                        shouldLog = true;
                    }
                    else {
                        if (currM == totalM) {
                            state = InstructionState.Fetch;
                            intManager.OnInstructionFinished();
                            shouldLog = true;
                        }
                    }
                    break;
            }
        }


        public void HandleInterrupt() {

            switch (state) {
                case InstructionState.Interrupt_IF:
                    // take 4t, simulate we are reading IF from memory
                    state = InstructionState.Interrupt_IE;
                    break;

                case InstructionState.Interrupt_IE:
                    interruptBit = intManager.GetEnabledInterrupt();
                    if (interruptBit == -1) {
                        state = InstructionState.Fetch;
                    }
                    else {
                        state = InstructionState.Interrupt_Push1;
                        // clear flags
                        intManager.IF = BitUtils.BitReset(intManager.IF, interruptBit);
                        intManager.DisableInterrupts();
                    }
                    break;

                case InstructionState.Interrupt_Push1:
                    regs.SP--;
                    mmu.Write8(regs.SP, BitUtils.Msb(regs.PC));
                    state = InstructionState.Interrupt_Push2;
                    break;

                case InstructionState.Interrupt_Push2:
                    regs.SP--;
                    mmu.Write8(regs.SP, BitUtils.Lsb(regs.PC));
                    state = InstructionState.Interrupt_Jump;
                    break;

                case InstructionState.Interrupt_Jump:
                    regs.PC = intManager.ISR_Address[interruptBit];
                    state = InstructionState.Fetch;
                    break;
            }

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
                    u8 cbOpcodeValue = mmu.Read8((u16)(regs.PC + 1));

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

        public u8 ReadParm8() {
            return (u8)mmu.Read8((u16)(regs.OpcodePC + 1));
        }

    }
}
