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
        private readonly GameBoyOptions gbParm;

        int totalCycles;
        int coreBoyCycles;
        int cycles;

        // constructor
        public GameBoy(string romName, GameBoyOptions gbParm) {
            intManager = new InterruptManager();
            timer = new Timer(intManager);
            gpu = new GPU(intManager);
            mmu = new MMU(timer, intManager, gpu);
            cpu = new CPU(mmu, timer, intManager);

            mmu.LoadData(romName);

            this.gbParm = gbParm;
            if (gbParm.logExecution) {
                logger = new Logger(gbParm.logFilename, gbParm.logMode);
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
                    gpu.Update(1);

                    totalCycles++;
                    coreBoyCycles++;

                    if (coreBoyCycles >= 65536) {
                        coreBoyCycles -= 65536;
                    }

                    // Debug stuff
                    if (gbParm.logExecution) {
                        if (cpu.shouldLog) {
                            logger.LogState(cpu, gpu, timer, mmu, coreBoyCycles);
                        }
                    }

                    if (gbParm.testingMode) {
                        // This is for Blargg testing, the ROMS write to the link port I/O
                        if (mmu.linkPortOutput.Contains("Passed")) {
                            if (logger != null) {
                                logger.Close();
                            }
                            return true;
                        }
                        if (mmu.linkPortOutput.Contains("Failed")) {
                            if (logger != null) {
                                logger.Close();
                            }
                            return false;
                        }
                    }


                }

                totalCycles -= CYCLES_PER_UPDATE;
            }
        }

    }
}

