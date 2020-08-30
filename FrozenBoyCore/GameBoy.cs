using System;
using FrozenBoyCore.Processor;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Util;
using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    public class GameBoy {
        public const int ClockSpeed = 4_194_304;

        public CPU cpu;
        public GPU gpu;
        public MMU mmu;
        public InterruptManager intManager;
        public Timer timer;
        public Logger logger;
        public string MemoryOutput = "";
        public int totalCycles;

        // constructor
        public GameBoy(string romName) {
            intManager = new InterruptManager();
            timer = new Timer(intManager);
            gpu = new GPU(intManager);
            mmu = new MMU(timer, intManager, gpu);
            cpu = new CPU(mmu, timer, intManager);

            gpu.SetMMU(mmu);

            mmu.LoadData(romName);
        }

        public int Step() {
            timer.Tick();
            cpu.ExecuteNext();
            gpu.Tick();
            return 1;
        }

        public bool RunTest(TestOptions options) {
            totalCycles = 0;

            if (options.logExecution) {
                logger = new Logger(options.logFilename);
            }

            while (true) {
                Step();

                totalCycles++;
                if (totalCycles == 65536) {
                    totalCycles = 0;
                }

                if (options.logExecution) {
                    if (cpu.shouldLog) {
                        logger.LogState(cpu, gpu, timer, mmu, intManager, totalCycles);
                    }
                }

                int status = TestCompleted(options);
                if (status != -1) {
                    if (logger != null) {
                        logger.Close();
                    }
                    return status == 1;
                }
            }
        }

        private int TestCompleted(TestOptions options) {
            if (options.testOutput == TestOutput.LinkPort) {
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

