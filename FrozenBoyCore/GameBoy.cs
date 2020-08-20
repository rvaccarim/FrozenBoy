using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    public class GameBoy {
        private const int MAXCYCLES = 69905;

        public CPU cpu;
        public MMU mmu;
        public Disassembler disassembler;
        public Logger logger;
        private readonly GameBoyParm gbParm;
        int prevPC;

        // constructor
        public GameBoy(string romName, GameBoyParm gbParm) {
            mmu = new MMU();
            byte[] romData = File.ReadAllBytes(romName);
            Buffer.BlockCopy(romData, 0, mmu.data, 0, romData.Length);

            cpu = new CPU(mmu);
            disassembler = new Disassembler();

            this.gbParm = gbParm;
            if (gbParm.debugMode) {
                logger = new Logger(gbParm.logFilename);
            }
        }

        public bool Run() {
            prevPC = cpu.regs.PC;

            while (true) {
                int cpuCycles = 0;

                while (cpuCycles < MAXCYCLES) {
                    // perform next instruction
                    cpuCycles += cpu.ExecuteNext();

                    // handle interruptions
                    cpu.HandleInterrupts();

                    // Debug stuff
                    if (gbParm.debugMode) {
                        Log();

                        if (gbParm.checkLinkPort) {
                            // This is for Blargg testing, the ROMS write to the link port I/O
                            if (mmu.linkPortOutput.Contains("Passed")) {
                                return true;
                            }
                            if (mmu.linkPortOutput.Contains("Failed")) {
                                return false;
                            }
                        }
                    }
                }

                // Render frame
            }
        }

        private void Log() {
            string instruction = disassembler.OpcodeToStr(cpu, cpu.prevOpcode, prevPC);
            logger.LogState(instruction, cpu.regs.ToString());
            prevPC = cpu.regs.PC;
        }

    }
}
