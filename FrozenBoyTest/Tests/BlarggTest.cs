using FrozenBoyCore;
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
        private Palettes palettes;

        private const string CPU_Path = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\cpu_instrs\individual\";
        private const string InstTimingPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\instr_timing\";
        private const string MemTimingPath1 = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\mem_timing\individual\";
        private const string MemTimingPath2 = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\mem_timing-2\rom_singles\";
        private const string HaltPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\halt_bug\";
        private const string OAMPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\oam_bug\rom_singles\";


        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        public BlarggTest(ITestOutputHelper output) {
            this.output = output;
            palettes = new Palettes();

        }

        [Fact]
        public void TestCPU_01() {
            bool passed = Test(@"01-special.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_02() {
            bool passed = Test(@"02-interrupts.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_03() {
            bool passed = Test(@"03-op sp,hl.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_04() {
            bool passed = Test(@"04-op r,imm.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_05() {
            bool passed = Test(@"05-op rp.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_06() {
            bool passed = Test(@"06-ld r,r.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_07() {
            bool passed = Test(@"07-jr,jp,call,ret,rst.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_08() {
            bool passed = Test(@"08-misc instrs.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_09() {
            bool passed = Test(@"09-op r,r.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_10() {
            bool passed = Test(@"10-bit ops.gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_11() {
            bool passed = Test(@"11-op a,(hl).gb", CPU_Path, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestInstruction_Timing() {
            bool passed = Test(@"instr_timing.gb", InstTimingPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_ReadTiming() {
            bool passed = Test(@"01-read_timing.gb", MemTimingPath1, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_WriteTiming() {
            bool passed = Test(@"02-write_timing.gb", MemTimingPath1, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_ModifyTiming() {
            bool passed = Test(@"03-modify_timing.gb", MemTimingPath1, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_ReadTiming() {
            bool passed = Test(@"01-read_timing2.gb", MemTimingPath2, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_WriteTiming() {
            bool passed = Test(@"02-write_timing2.gb", MemTimingPath2, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_ModifyTiming() {
            bool passed = Test(@"03-modify_timing2.gb", MemTimingPath2, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestHaltBug() {
            bool passed = Test(@"halt_bug.gb", HaltPath, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_LCD_Sync() {
            bool passed = Test(@"1-lcd_sync.gb", OAMPath, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_Causes() {
            bool passed = Test(@"2-causes.gb", OAMPath, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_NonCauses() {
            bool passed = Test(@"3-non_causes.gb", OAMPath, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        //[Fact]
        //public void TestOAM_Scanline_Timing() {
        //    bool passed = Test(@"4-scanline_timing.gb", OAMPath, logExecution: false, testOutput: TestOutput.Memory);
        //    Assert.True(passed);
        //}

        //[Fact]
        //public void TestOAM_TimingBug() {
        //    bool passed = Test(@"5-timing_bug.gb", OAMPath, logExecution: false, testOutput: TestOutput.Memory);
        //    Assert.True(passed);
        //}

        //[Fact]
        //public void TestOAM_TimingNoBug() {
        //    bool passed = Test(@"6-timing_no_bug.gb", OAMPath, logExecution: false, testOutput: TestOutput.Memory);
        //    Assert.True(passed);
        //}

        //[Fact]
        //public void TestOAM_InstrEffect() {
        //    bool passed = Test(@"8-instr_effect.gb", OAMPath, logExecution: false, testOutput: TestOutput.Memory);
        //    Assert.True(passed);
        //}

        public bool Test(string romFilename, string romPath, bool logExecution, TestOutput testOutput) {
            string logFilename = debugPath + romFilename + ".log.frozenBoy.txt";

            GameOptions gameOptions = new GameOptions(romFilename, romPath, palettes.GetWhitePalette());
            GameBoy gb = new GameBoy(gameOptions);

            TestOptions testOptions = new TestOptions(testOutput, logExecution, logFilename);

            Driver driver = new Driver();
            Result result = driver.RunTest(gb, testOptions);
            output.WriteLine(result.Message);

            return result.Passed;

        }
    }
}
