﻿using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using System.Diagnostics;
using Xunit.Abstractions;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyTest {
    public class BlarggTest {
        private readonly ITestOutputHelper output;

        private const string CPU_Path = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\cpu_instrs\individual\";
        private const string InstTimingPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\instr_timing\";
        private const string MemTimingPath1 = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\mem_timing\individual\";
        private const string MemTimingPath2 = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\mem_timing-2\rom_singles\";
        private const string HaltPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\halt_bug\";
        private const string OAMPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\oam_bug\rom_singles\";


        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        public BlarggTest(ITestOutputHelper output) {
            this.output = output;
        }

        //public BlarggTest() {

        //}

        [Fact]
        public void TestCPU_01() {
            bool passed = Test(CPU_Path, @"01-special.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_02() {
            bool passed = Test(CPU_Path, @"02-interrupts.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_03() {
            bool passed = Test(CPU_Path, @"03-op sp,hl.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_04() {
            bool passed = Test(CPU_Path, @"04-op r,imm.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_05() {
            bool passed = Test(CPU_Path, @"05-op rp.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_06() {
            bool passed = Test(CPU_Path, @"06-ld r,r.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_07() {
            bool passed = Test(CPU_Path, @"07-jr,jp,call,ret,rst.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_08() {
            bool passed = Test(CPU_Path, @"08-misc instrs.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_09() {
            bool passed = Test(CPU_Path, @"09-op r,r.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_10() {
            bool passed = Test(CPU_Path, @"10-bit ops.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_11() {
            bool passed = Test(CPU_Path, @"11-op a,(hl).gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestInstruction_Timing() {
            bool passed = Test(InstTimingPath, @"instr_timing.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_ReadTiming() {
            bool passed = Test(MemTimingPath1, @"01-read_timing.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_WriteTiming() {
            bool passed = Test(MemTimingPath1, @"02-write_timing.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_ModifyTiming() {
            bool passed = Test(MemTimingPath1, @"03-modify_timing.gb", logExecution: false, TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_ReadTiming() {
            bool passed = Test(MemTimingPath2, @"01-read_timing2.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_WriteTiming() {
            bool passed = Test(MemTimingPath2, @"02-write_timing2.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_ModifyTiming() {
            bool passed = Test(MemTimingPath2, @"03-modify_timing2.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestHaltBug() {
            bool passed = Test(HaltPath, @"halt_bug.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_LCD_Sync() {
            bool passed = Test(OAMPath, @"1-lcd_sync.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_Causes() {
            bool passed = Test(OAMPath, @"2-causes.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_NonCauses() {
            bool passed = Test(OAMPath, @"3-non_causes.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_Scanline_Timing() {
            bool passed = Test(OAMPath, @"4-scanline_timing.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_TimingBug() {
            bool passed = Test(OAMPath, @"5-timing_bug.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_TimingNoBug() {
            bool passed = Test(OAMPath, @"6-timing_no_bug.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }

        //[Fact]
        //public void TestOAM_TimingEffect() {
        //    bool passed = Test(OAMPath, @"7-timing_effect.gb", logExecution: false, TestOutput.Memory);
        //    Assert.True(passed);
        //}


        [Fact]
        public void TestOAM_InstrEffect() {
            bool passed = Test(OAMPath, @"8-instr_effect.gb", logExecution: false, TestOutput.Memory);
            Assert.True(passed);
        }


        public bool Test(string path, string romName, bool logExecution, TestOutput testOutput) {
            string romFilename = path + romName;
            string logFilename = debugPath + romName + ".log.frozenBoy.txt";

            TestOptions options = new TestOptions(testOutput, "", logExecution, logFilename);
            GameBoy gb = new GameBoy(romFilename);

            bool passed = gb.RunTest(options);
            output.WriteLine(gb.testResult);

            return passed;

        }
    }
}
