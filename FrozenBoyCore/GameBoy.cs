using System;
using FrozenBoyCore.Processor;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Util;
using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Controls;
using System.Text;
using System.ComponentModel.Design;

namespace FrozenBoyCore {

    public class GameBoy {
        public const int ClockSpeed = 4_194_304;

        public CPU cpu;
        public GPU gpu;
        public MMU mmu;
        public InterruptManager intManager;
        public Timer timer;
        public Joypad joypad;
        public Logger logger;

        public string testResult = "";

        public int totalCycles;

        // constructor
        public GameBoy(string romName) {
            intManager = new InterruptManager();
            timer = new Timer(intManager);
            gpu = new GPU(intManager);
            joypad = new Joypad(intManager);
            mmu = new MMU(timer, intManager, gpu, joypad);
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

        private int MD5_Cycles = 69905;
        private int md5_cycles;

        public bool RunTest(TestOptions options) {
            totalCycles = 0;

            if (options.logExecution) {
                logger = new Logger(options.logFilename);
            }

            int MD5_attemtps = 0;
            int MD5_max_attempts = 20;

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

                if (options.testOutput == TestOutput.LinkPort || options.testOutput == TestOutput.Memory) {
                    int status = TestCompleted(options);
                    if (status != -1) {
                        if (logger != null) {
                            logger.Close();
                        }
                        return (status == 1);
                    }
                }
                else {
                    // 60 FPS
                    md5_cycles++;
                    if (md5_cycles == MD5_Cycles) {
                        string md5 = MD5(gpu.GetScreenBuffer());
                        if (options.expectedMD5 == md5) {
                            testResult = md5;
                            return true;
                        }

                        md5_cycles = 0;
                        MD5_attemtps++;
                    }

                    if (MD5_attemtps == MD5_max_attempts) {
                        testResult = "MD5s didn't match, timeout reached";
                        return false;
                    }
                }
            }
        }

        private int TestCompleted(TestOptions options) {
            if (options.testOutput == TestOutput.LinkPort) {
                // This is for Blargg testing, the ROMS write to the link port I/O
                if (mmu.linkPortOutput.Contains("Passed")) {
                    testResult = mmu.linkPortOutput;
                    return 1;
                }
                if (mmu.linkPortOutput.Contains("Failed")) {
                    testResult = mmu.linkPortOutput;
                    return 0;
                }
            }
            else {
                // while the test is running $A000 holds $80
                u16 address = 0xA000;
                testResult = "";

                if (mmu.data[0xA000] != 0x80) {
                    address++;
                    while (mmu.data[address] != 0x0) {
                        testResult += Convert.ToChar(mmu.data[address]);
                        address++;
                    }

                    if (testResult.Contains("Passed")) {
                        return 1;
                    }
                    if (testResult.Contains("Failed")) {
                        return 0;
                    }
                }
            }

            return -1;
        }

        private string MD5(byte[] backbuffer) {
            byte[] hash;
            using var md5 = System.Security.Cryptography.MD5.Create();
            md5.TransformFinalBlock(backbuffer, 0, backbuffer.Length);
            hash = md5.Hash;

            StringBuilder result = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
                result.Append(hash[i].ToString("X2"));

            return result.ToString();
        }

    }
}

