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
        private ITestOutputHelper output;

        private const string CPU_Path = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\cpu_instrs\individual\";
        private const string InstTimingPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\instr_timing\";
        private const string InterruptTimePath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\interrupt_time\";
        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        public BlarggTest(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void TestCPU_01() {
            bool passed = Test(CPU_Path, @"01-special.gb", logExecution: true);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_02() {
            bool passed = Test(CPU_Path, @"02-interrupts.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_03() {
            bool passed = Test(CPU_Path, @"03-op sp,hl.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_04() {
            bool passed = Test(CPU_Path, @"04-op r,imm.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_05() {
            bool passed = Test(CPU_Path, @"05-op rp.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_06() {
            bool passed = Test(CPU_Path, @"06-ld r,r.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_07() {
            bool passed = Test(CPU_Path, @"07-jr,jp,call,ret,rst.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_08() {
            bool passed = Test(CPU_Path, @"08-misc instrs.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_09() {
            bool passed = Test(CPU_Path, @"09-op r,r.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_10() {
            bool passed = Test(CPU_Path, @"10-bit ops.gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_11() {
            bool passed = Test(CPU_Path, @"11-op a,(hl).gb", logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void TestInstTiming() {
            bool passed = Test(InstTimingPath, @"instr_timing.gb", logExecution: false);
            Assert.True(passed);
        }

        //[Fact]
        //public void TestInterruptTime() {
        //    bool passed = Test(InterruptTimePath, @"interrupt_time.gb", logExecution: false);
        //    Assert.True(passed);
        //}

        public bool Test(string path, string romName, bool logExecution) {
            bool testingMode = true;

            string romFilename = path + romName;
            string logFilename = debugPath + romName + ".log.frozenBoy.txt";
            LogMode logMode = LogMode.Time;

            GameBoyParm gbParm = new GameBoyParm(testingMode, logExecution, logFilename, logMode);
            GameBoy gb = new GameBoy(romFilename, gbParm);

            bool passed = gb.Run();
            output.WriteLine(gb.cpu.mmu.linkPortOutput);

            return passed;

        }
    }
}
