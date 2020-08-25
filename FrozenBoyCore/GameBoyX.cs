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

    public class GameBoyX {
        public const int DMG_4Mhz = 4194304;
        public const float REFRESH_RATE = 59.7275f;
        public const int CYCLES_PER_UPDATE = (int)(DMG_4Mhz / REFRESH_RATE);

        public CPUX cpu;
        public GPU gpu;
        public MMU mmu;
        public InterruptManager intManager;
        public Timer timer;
        public LoggerX logger;
        private readonly GameBoyOptions gbParm;

        int totalCycles;
        int coreBoyCycles;
        int cycles;

        // constructor
        public GameBoyX(string romName, GameBoyOptions gbParm) {
            intManager = new InterruptManager();
            timer = new Timer(intManager);
            gpu = new GPU(intManager);
            mmu = new MMU(timer, intManager, gpu);
            cpu = new CPUX(mmu, timer, intManager);

            mmu.LoadData(romName);

            this.gbParm = gbParm;
            if (gbParm.logExecution) {
                logger = new LoggerX(gbParm.logFilename, gbParm.logMode);
            }
        }

        public bool Run() {
            totalCycles = 0;
            coreBoyCycles = 0;


            while (true) {

                while (totalCycles < CYCLES_PER_UPDATE) {
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
                            Log();
                        }
                    }

                    if (gbParm.testingMode) {
                        // This is for Blargg testing, the ROMS write to the link port I/O
                        if (mmu.linkPortOutput.Length != 0) {
                            Console.WriteLine(mmu.linkPortOutput);
                        }

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

        private void Log() {
            logger.LogState(cpu, gpu, timer, mmu, coreBoyCycles);
        }

    }
}

