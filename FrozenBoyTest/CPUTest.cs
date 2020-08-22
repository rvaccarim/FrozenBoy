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
    public class CPUTest {
        private ITestOutputHelper output;

        private const string romPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\cpu_instrs\individual\";
        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        public CPUTest(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void TestCPU_Blargg_01() {
            bool passed = TestCPU_Blargg(@"01-special.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_02() {
            bool passed = TestCPU_Blargg(@"02-interrupts.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_03() {
            bool passed = TestCPU_Blargg(@"03-op sp,hl.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_04() {
            bool passed = TestCPU_Blargg(@"04-op r,imm.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_05() {
            bool passed = TestCPU_Blargg(@"05-op rp.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_06() {
            bool passed = TestCPU_Blargg(@"06-ld r,r.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_07() {
            bool passed = TestCPU_Blargg(@"07-jr,jp,call,ret,rst.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_08() {
            bool passed = TestCPU_Blargg(@"08-misc instrs.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_09() {
            bool passed = TestCPU_Blargg(@"09-op r,r.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_10() {
            bool passed = TestCPU_Blargg(@"10-bit ops.gb");
            Assert.True(passed);
        }

        [Fact]
        public void TestCPU_Blargg_11() {
            bool passed = TestCPU_Blargg(@"11-op a,(hl).gb");
            Assert.True(passed);
        }

        public bool TestCPU_Blargg(string romName) {
            bool debugMode = false;
            bool checkLinkPort = true;

            string romFilename = romPath + romName;
            string logOutput = debugPath + romName + ".log.frozenBoy.txt";

            GameBoyParm gbParm = new GameBoyParm(checkLinkPort, debugMode, logOutput);
            GameBoy gb = new GameBoy(romFilename, gbParm);

            bool passed = gb.Run();
            output.WriteLine(gb.cpu.mmu.linkPortOutput);

            return passed;

        }
    }
}
