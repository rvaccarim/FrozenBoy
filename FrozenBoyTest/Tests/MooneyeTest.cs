using FrozenBoyCore;
using FrozenBoyCore.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace FrozenBoyTest {
    public class MooneyeTest {
        private readonly ITestOutputHelper output;

        private string mooneyePath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\mooneye\";
        private string hashesPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\FrozenBoyTest\Hashes\";
        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        public MooneyeTest(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void Test_call_timing() {
            bool passed = Test(@"acceptance\", "call_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_call_cc_timing() {
            bool passed = Test(@"acceptance\", "call_cc_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_div_timing() {
            bool passed = Test(@"acceptance\", "div_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ei_sequence() {
            bool passed = Test(@"acceptance\", "ei_sequence.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ei_timing() {
            bool passed = Test(@"acceptance\", "ei_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_halt_ime1_timing() {
            bool passed = Test(@"acceptance\", "halt_ime1_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_if_ie_registers() {
            bool passed = Test(@"acceptance\", "if_ie_registers.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_intr_timing() {
            bool passed = Test(@"acceptance\", "intr_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_jp_timing() {
            bool passed = Test(@"acceptance\", "jp_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_jp_cc_timing() {
            bool passed = Test(@"acceptance\", "jp_cc_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_oam_dma_start() {
            bool passed = Test(@"acceptance\", "oam_dma_start.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_oam_dma_restart() {
            bool passed = Test(@"acceptance\", "oam_dma_restart.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_oam_dma_timing() {
            bool passed = Test(@"acceptance\", "oam_dma_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ld_hl_sp_e_timing() {
            bool passed = Test(@"acceptance\", "ld_hl_sp_e_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_pop_timing() {
            bool passed = Test(@"acceptance\", "pop_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_push_timing() {
            bool passed = Test(@"acceptance\", "push_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_reti_intr_timing() {
            bool passed = Test(@"acceptance\", "reti_intr_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ret_timing() {
            bool passed = Test(@"acceptance\", "ret_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_reti_timing() {
            bool passed = Test(@"acceptance\", "reti_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ret_cc_timing() {
            bool passed = Test(@"acceptance\", "ret_cc_timing.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_rapid_di_ei() {
            bool passed = Test(@"acceptance\", "rapid_di_ei.gb", false);
            Assert.True(passed);
        }


        [Fact]
        public void Test_mem_oam() {
            bool passed = Test(@"acceptance\bits\", "mem_oam.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_reg_f() {
            bool passed = Test(@"acceptance\bits\", "reg_f.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_daa() {
            bool passed = Test(@"acceptance\instr\", "daa.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_basic_oam_dma() {
            bool passed = Test(@"acceptance\oam_dma\", "basic.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_basic_reg_dma() {
            bool passed = Test(@"acceptance\oam_dma\", "reg_read.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_div_write() {
            bool passed = Test(@"acceptance\timer\", "div_write.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_rapid_toggle() {
            bool passed = Test(@"acceptance\timer\", "rapid_toggle.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim00_div_trigger() {
            bool passed = Test(@"acceptance\timer\", "tim00_div_trigger.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim01() {
            bool passed = Test(@"acceptance\timer\", "tim01.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim01_div_trigger() {
            bool passed = Test(@"acceptance\timer\", "tim01_div_trigger.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim10() {
            bool passed = Test(@"acceptance\timer\", "tim10.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim10_div_trigger() {
            bool passed = Test(@"acceptance\timer\", "tim10_div_trigger.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim11() {
            bool passed = Test(@"acceptance\timer\", "tim11.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim11_div_trigger() {
            bool passed = Test(@"acceptance\timer\", "tim11_div_trigger.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tima_reload() {
            bool passed = Test(@"acceptance\timer\", "tima_reload.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tima_write_reloading() {
            bool passed = Test(@"acceptance\timer\", "tima_write_reloading.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tma_write_reloading() {
            bool passed = Test(@"acceptance\timer\", "tma_write_reloading.gb", false);
            Assert.True(passed);
        }

        private bool Test(string extraPath, string romName, bool logExecution) {
            string romFilename = mooneyePath + extraPath + romName;
            string logFilename = debugPath + romName + ".log.frozenBoy.txt";
            string expectedMD5 = File.ReadAllText(hashesPath + extraPath + romName + ".hash.txt");

            GameOptions gameOptions = new GameOptions(romFilename, new Driver().GetTestPalette());
            GameBoy gb = new GameBoy(gameOptions);

            TestOptions testOptions = new TestOptions(TestOutput.MD5, expectedMD5, logExecution, logFilename);

            Driver driver = new Driver();
            Result result = driver.RunTest(gb, testOptions);
            output.WriteLine(result.Message);
            return result.Passed;

        }


    }
}
