using FrozenBoyCore;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Memory;
using FrozenBoyCore.Processor;
using FrozenBoyCore.Util;
using System;
using System.Collections.Generic;
using System.IO;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyTest;

namespace FrozenBoyTest {
    public class Driver {

        private string hashesPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\FrozenBoyTest\Hashes\";

        public GPU_Palette GetTestPalette() {
            var white = new GPU_Color(255, 255, 255, 255);
            var lightGray = new GPU_Color(170, 170, 170, 255);
            var darkGray = new GPU_Color(85, 85, 85, 255);
            var black = new GPU_Color(0, 0, 0, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }

        public List<MD5_Progress> GetExpected(string romName, string targetDirectory) {
            var md5_list = new List<MD5_Progress>();

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries) {
                if (fileName.StartsWith(romName) && fileName.EndsWith("hash.txt")) {
                    md5_list.Add(new MD5_Progress(File.ReadAllText(fileName), false));
                }
            }
            return md5_list;
        }


        public Result RunTest(GameBoy gb, TestOptions options) {

            StreamWriter logFile = null;

            if (options.logExecution) {
                logFile = new StreamWriter(options.logFilename);
            }

            int totalCycles = 0;
            int FPS_max_cycles = 69905;
            int fps_cycles = 0;
            int attempts = 0;
            int maxAttempts = 5000;
            string memoryOutput = "";
            List<MD5_Progress> md5s = null;

            if (options.testOutput == TestOutput.MD5) {
                md5s = GetExpected(gb.gbOptions.RomPath, hashesPath);
            }

            while (true) {

                gb.Step();

                if (options.logExecution) {
                    if (gb.cpu.state == InstructionState.Fetch) {
                        LogState(logFile, gb, totalCycles);
                    }
                }

                // do the equivalent to produce 60 FPS
                fps_cycles++;
                if (fps_cycles == FPS_max_cycles) {
                    fps_cycles = 0;
                    attempts++;
                }

                switch (options.testOutput) {
                    case TestOutput.LinkPort:
                        if (gb.serial.log.Contains("Passed")) {
                            CloseLog(logFile);
                            return new Result(true, gb.serial.log);
                        }
                        if (gb.serial.log.Contains("Failed")) {
                            CloseLog(logFile);
                            return new Result(false, gb.serial.log);
                        }
                        break;

                    case TestOutput.Memory:
                        // while the test is running $A000 holds $80
                        u16 startAddress = 0xA000;

                        if (gb.mmu.Read8(0xA000) != 0x80) {
                            startAddress++;
                            while (gb.mmu.Read8(startAddress) != 0x0) {
                                memoryOutput += Convert.ToChar(gb.mmu.Read8(startAddress));
                                startAddress++;
                            }

                            if (memoryOutput.Contains("Passed")) {
                                CloseLog(logFile);
                                return new Result(true, memoryOutput);
                            }
                            if (memoryOutput.Contains("Failed")) {
                                CloseLog(logFile);
                                return new Result(false, memoryOutput);
                            }
                        }
                        break;

                    case TestOutput.MD5:
                        string md5 = Crypto.MD5(gb.gpu.GetScreenBuffer());

                        bool allPassed = true;
                        for (int i = 0; i < md5s.Count; i++) {
                            if (md5.Equals(md5s[i])) {
                                md5s[i].Passed = true;
                            }

                            if (!md5s[i].Passed) {
                                allPassed = false;
                            }
                        }

                        if (allPassed) {
                            CloseLog(logFile);
                            return new Result(true, "All MD5s matched");
                        }

                        break;
                }

                if (attempts == maxAttempts) {
                    if (logFile != null) {
                        CloseLog(logFile);
                    }
                    return new Result(false, "Timeout reached");
                }


                // for comparisons with CoreBoy
                totalCycles++;
                if (totalCycles == 65536) {
                    totalCycles = 0;
                }

            }
        }

        private static void CloseLog(StreamWriter logFile) {
            if (logFile != null) {
                logFile.Flush();
                logFile.Close();
                logFile.Dispose();
            }
        }

        private const string gameboyState =
            "{0}   cycles:{1,6}  AF={2:x4} BC={3:x4} DE={4:x4} HL={5:x4}   Z={6} N={7} H={8} C={9}   " +
            "PC={10:x4} SP={11:x4}   IME={12} IE={13:x4} IF={14:x4} halted={15}   " +
            "DIV={16:x4} TIMA={17:x4} TMA={18:x4} TAC={19:x4}   " + "" +
            "LCDC={20:x2} STAT={21} LY={22:x2} LYC={23:x2} gpuClock={24} delay={25}";

        public void LogState(StreamWriter logFile, GameBoy gb, int cycle) {
            string instruction;

            bool disAsm = false;

            if (gb.cpu.opcode != null) {
                // if (disAsm) {
                // instruction = Disassembler.OpcodeToStr(cpu, cpu.opcode, cpu.regs.OpcodePC);
                // }
                // else {
                instruction = String.Format("O=0x{0:x2}", gb.cpu.opcode.value);
                // }
            }
            else {
                instruction = "";
            }
            logFile.WriteLine(
                String.Format(gameboyState,
                instruction, cycle,
                gb.cpu.regs.AF, gb.cpu.regs.BC, gb.cpu.regs.DE, gb.cpu.regs.HL,
                Convert.ToInt32(gb.cpu.regs.FlagZ), Convert.ToInt32(gb.cpu.regs.FlagN),
                Convert.ToInt32(gb.cpu.regs.FlagH), Convert.ToInt32(gb.cpu.regs.FlagC),
                gb.cpu.regs.PC, gb.cpu.regs.SP,
                Convert.ToInt32(gb.intManager.IME), gb.intManager.IE, Convert.ToString(gb.intManager.IF, 2).PadLeft(8, '0').Substring(3),
                Convert.ToInt32(gb.cpu.haltBug),
                gb.timer.DIV, gb.timer.TIMA, gb.timer.TMA, gb.timer.tac,
                Convert.ToString(gb.gpu.LCDC, 2).PadLeft(8, '0'), "",
                gb.gpu.LY, gb.gpu.LYC, gb.gpu.lineTicks, (gb.gpu.wasDisabled && !gb.gpu.IsLcdEnabled()) ? -1 : gb.gpu.enableDelay));
        }

    }
}
