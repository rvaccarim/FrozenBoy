using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Processor;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Util;
using FrozenBoyCore.Memory;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Reflection.Metadata.Ecma335;

namespace FrozenBoyCore {

    public class GameBoy {
        public const int DMG_4Mhz = 4194304;
        public const float REFRESH_RATE = 59.7275f;
        public const int CYCLES_PER_UPDATE = (int)(DMG_4Mhz / REFRESH_RATE);

        public CPU cpu;
        public GPU gpu;
        public MMU mmu;
        public InterruptManager intManager;
        public Timer timer;
        public Logger logger;
        private readonly GameBoyOptions gbOptions;
        public string MemoryOutput = "";

        int totalCycles;
        int coreBoyCycles;

        // constructor
        public GameBoy(string romName, GameBoyOptions gbParm) {
            intManager = new InterruptManager();
            timer = new Timer(intManager);
            gpu = new GPU(intManager);
            mmu = new MMU(timer, intManager, gpu);
            cpu = new CPU(mmu, timer, intManager);

            mmu.LoadData(romName);

            this.gbOptions = gbParm;
            if (gbParm.logExecution) {
                logger = new Logger(gbParm.logFilename);
            }
        }


        public bool Run() {
            totalCycles = 0;
            coreBoyCycles = 0;

            while (true) {
                while (totalCycles < CYCLES_PER_UPDATE) {

                    if (totalCycles >= 8270) {
                        if (cpu.state == InstructionState.WorkPending) {
                            int x = 0;
                        }
                    }

                    timer.Tick();
                    cpu.ExecuteNext();
                    gpu.Tick();

                    totalCycles++;
                    coreBoyCycles++;

                    if (coreBoyCycles >= 65536) {
                        coreBoyCycles -= 65536;
                    }

                    // Debug stuff
                    if (gbOptions.logExecution) {
                        if (cpu.shouldLog) {
                            logger.LogState(cpu, gpu, timer, mmu, intManager, coreBoyCycles);
                        }
                    }

                    // Testing
                    if (gbOptions.testingMode) {
                        int status = TestCompleted();
                        if (status != -1) {
                            if (logger != null) {
                                logger.Close();
                            }
                            return status == 1;
                        }
                    }
                }

                totalCycles -= CYCLES_PER_UPDATE;
            }
        }


        private int TestCompleted() {
            if (gbOptions.testOutput == TestOutput.LinkPort) {
                // This is for Blargg testing, the ROMS write to the link port I/O
                if (mmu.linkPortOutput.Contains("Passed")) {
                    return 1;
                }
                if (mmu.linkPortOutput.Contains("Failed")) {
                    return 0;
                }

            }
            else {
                // while the test is running $A000 holds $80
                u16 address = 0xA000;
                MemoryOutput = "";

                if (mmu.data[0xA000] != 0x80) {
                    address++;
                    while (mmu.data[address] != 0x0) {
                        MemoryOutput += Convert.ToChar(mmu.data[address]);
                        address++;
                    }

                    if (MemoryOutput.Contains("Passed")) {
                        return 1;
                    }
                    if (MemoryOutput.Contains("Failed")) {
                        return 0;
                    }
                }
            }

            return -1;
        }

    }
}

