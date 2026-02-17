using System.IO;
using FrozenBoyCore;
using Xunit;
using Xunit.Abstractions;

namespace FrozenBoyTest
{
    public class BlarggTest(ITestOutputHelper output)
    {

        public bool Test(string romFilename, string romPath, bool logExecution, TestOutput testOutput)
        {
            Directory.CreateDirectory(Config.debugOutPath);
            string logFilename = Path.Combine(Config.debugOutPath, romFilename + ".log.frozenBoy.txt");

            GameOptions gameOptions = new(romFilename, romPath, Palettes.GetWhitePalette());
            GameBoy gb = new(gameOptions);

            TestOptions testOptions = new(testOutput, logExecution, logFilename);

            Driver driver = new();
            Result result = driver.RunTest(gb, testOptions);
            output.WriteLine(result.Message);

            return result.Passed;

        }

        [Fact]
        public void TestCPU_01()
        {
            bool passed = Test(@"01-special.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_02()
        {
            bool passed = Test(@"02-interrupts.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_03()
        {
            bool passed = Test(@"03-op sp,hl.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_04()
        {
            bool passed = Test(@"04-op r,imm.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_05()
        {
            bool passed = Test(@"05-op rp.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_06()
        {
            bool passed = Test(@"06-ld r,r.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_07()
        {
            bool passed = Test(@"07-jr,jp,call,ret,rst.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_08()
        {
            bool passed = Test(@"08-misc instrs.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_09()
        {
            bool passed = Test(@"09-op r,r.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_10()
        {
            bool passed = Test(@"10-bit ops.gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_11()
        {
            bool passed = Test(@"11-op a,(hl).gb", Config.CPUPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestInstruction_Timing()
        {
            bool passed = Test(@"instr_timing.gb", Config.InstTimingPath, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_ReadTiming()
        {
            bool passed = Test(@"01-read_timing.gb", Config.MemTimingPath1, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_WriteTiming()
        {
            bool passed = Test(@"02-write_timing.gb", Config.MemTimingPath1, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory1_ModifyTiming()
        {
            bool passed = Test(@"03-modify_timing.gb", Config.MemTimingPath1, logExecution: false, testOutput: TestOutput.LinkPort);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_ReadTiming()
        {
            bool passed = Test(@"01-read_timing2.gb", Config.MemTimingPath2, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_WriteTiming()
        {
            bool passed = Test(@"02-write_timing2.gb", Config.MemTimingPath2, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestMemory2_ModifyTiming()
        {
            bool passed = Test(@"03-modify_timing2.gb", Config.MemTimingPath2, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestHaltBug()
        {
            bool passed = Test(@"halt_bug.gb", Config.HaltPath, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_LCD_Sync()
        {
            bool passed = Test(@"1-lcd_sync.gb", Config.OAMPath, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_Causes()
        {
            bool passed = Test(@"2-causes.gb", Config.OAMPath, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        [Fact]
        public void TestOAM_NonCauses()
        {
            bool passed = Test(@"3-non_causes.gb", Config.OAMPath, logExecution: false, testOutput: TestOutput.Memory);
            Assert.True(passed);
        }

        //[Fact]
        //public void TestOAM_Scanline_Timing() {
        //    bool passed = Test(@"4-scanline_timing.gb", Config.OAMPath, logExecution: false, testOutput: TestOutput.Memory);
        //    Assert.True(passed);
        //}

        //[Fact]
        //public void TestOAM_TimingBug() {
        //    bool passed = Test(@"5-timing_bug.gb", Config.OAMPath, logExecution: false, testOutput: TestOutput.Memory);
        //    Assert.True(passed);
        //}

        //[Fact]
        //public void TestOAM_TimingNoBug() {
        //    bool passed = Test(@"6-timing_no_bug.gb", Config.OAMPath, logExecution: false, testOutput: TestOutput.Memory);
        //    Assert.True(passed);
        //}

        //[Fact]
        //public void TestOAM_InstrEffect() {
        //    bool passed = Test(@"8-instr_effect.gb", Config.OAMPath, logExecution: false, testOutput: TestOutput.Memory);
        //    Assert.True(passed);
        //}


    }
}
