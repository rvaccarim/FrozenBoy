using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    public class GameBoy {
        public const int DMG_4Mhz = 4194304;
        public const float REFRESH_RATE = 59.7275f;
        public const int CYCLES_PER_UPDATE = (int)(DMG_4Mhz / REFRESH_RATE);
        public const float MILLIS_PER_FRAME = 16.74f;

        public CPU cpu;
        public MMU mmu;
        public Timer timer;
        public Logger logger;
        private readonly GameBoyParm gbParm;

        int totalCycles;
        int cycles;
        int prevPC;

        // constructor
        public GameBoy(string romName, GameBoyParm gbParm) {
            mmu = new MMU();
            byte[] romData = File.ReadAllBytes(romName);
            Buffer.BlockCopy(romData, 0, mmu.data, 0, romData.Length);

            timer = new Timer(mmu);
            cpu = new CPU(mmu);

            this.gbParm = gbParm;
            if (gbParm.debugMode) {
                logger = new Logger(gbParm.logFilename);
            }
        }

        public bool Run() {
            prevPC = cpu.regs.PC;

            totalCycles = 0;

            while (true) {

                while (totalCycles < CYCLES_PER_UPDATE) {
                    cycles = cpu.ExecuteNext();
                    timer.Update(cycles);
                    // UpdateGraphics(cycles);
                    cpu.HandleInterrupts();

                    totalCycles += cycles;

                    // Debug stuff
                    if (gbParm.debugMode) {
                        Log();

                        if (gbParm.checkLinkPort) {
                            // This is for Blargg testing, the ROMS write to the link port I/O
                            if (mmu.linkPortOutput.Contains("Passed")) {
                                logger.Close();
                                return true;
                            }
                            if (mmu.linkPortOutput.Contains("Failed")) {
                                logger.Close();
                                return false;
                            }
                        }
                    }
                }

                totalCycles -= CYCLES_PER_UPDATE;
            }
        }

        private void Log() {
            logger.LogState(cpu, mmu, totalCycles);
        }

    }
}
