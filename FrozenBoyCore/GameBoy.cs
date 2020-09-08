using System;
using FrozenBoyCore.Processor;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Util;
using FrozenBoyCore.Memory;
using FrozenBoyCore.Controls;
using System.Text;
using FrozenBoyCore.Serial;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    public class GameBoy {
        public const int ClockSpeed = 4_194_304;
        private GameOptions gbOptions;

        public Cartridge cartridge;
        public CPU cpu;
        public GPU gpu;
        public MMU mmu;
        public Dma dma;
        public InterruptManager intManager;
        public Timer timer;
        public SerialPort serial;
        public Joypad joypad;
        public Logger logger;

        public string testResult = "";

        public int totalCycles;

        // constructor
        public GameBoy(GameOptions gbOptions) {
            this.gbOptions = gbOptions;

            intManager = new InterruptManager();
            timer = new Timer(intManager);
            gpu = new GPU(intManager, gbOptions.Palette);
            joypad = new Joypad(intManager);
            dma = new Dma();
            serial = new SerialPort(intManager);
            mmu = new MMU(timer, intManager, gpu, joypad, dma, serial);
            cpu = new CPU(mmu, timer, intManager, gpu);

            dma.SetMMU(mmu);

            mmu.LoadData(gbOptions.RomPath);

        }

        public int Step() {
            timer.Tick();
            cpu.ExecuteNext();
            dma.Tick();
            serial.Tick();
            gpu.Tick();
            return 1;
        }


        public bool RunTest(TestOptions options) {
            totalCycles = 0;

            if (options.logExecution) {
                logger = new Logger(options.logFilename);
            }

            int FPS_max_cycles = 69905;
            int fps_cycles = 0;
            int MD5_attemtps = 0;
            int MD5_max_attempts = 500;

            while (true) {

                //if (totalCycles == 5524 && cpu.opcode.value == 0xcd) {
                //    int z = 0;
                //}

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
                    fps_cycles++;
                    if (fps_cycles == FPS_max_cycles) {
                        string md5 = MD5(gpu.GetScreenBuffer());
                        if (options.expectedMD5 == md5) {
                            testResult = md5;
                            if (logger != null) {
                                logger.Close();
                            }
                            return true;
                        }

                        fps_cycles = 0;
                        MD5_attemtps++;
                    }

                    if (MD5_attemtps == MD5_max_attempts) {
                        testResult = "MD5s didn't match, timeout reached";
                        if (logger != null) {
                            logger.Close();
                        }
                        return false;
                    }
                }
            }
        }

        private int TestCompleted(TestOptions options) {
            if (options.testOutput == TestOutput.LinkPort) {
                // This is for Blargg testing, the ROMS write to the link port I/O
                if (serial.log.Contains("Passed")) {
                    testResult = serial.log;
                    return 1;
                }
                if (serial.log.Contains("Failed")) {
                    testResult = serial.log;
                    return 0;
                }
            }
            else {
                // while the test is running $A000 holds $80
                u16 address = 0xA000;
                testResult = "";

                if (mmu.Read8(0xA000) != 0x80) {
                    address++;
                    while (mmu.Read8(address) != 0x0) {
                        testResult += Convert.ToChar(mmu.Read8(address));
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

