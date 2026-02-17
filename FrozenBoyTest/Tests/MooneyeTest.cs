using System;
using System.IO;
using FrozenBoyCore;
using Xunit;
using Xunit.Abstractions;

namespace FrozenBoyTest
{
    public class MooneyeTest(ITestOutputHelper output)
    {

        private bool Test(string romFilename, string romPath, bool logExecution)
        {
            Directory.CreateDirectory(Config.debugOutPath);
            string logFilename = Path.Combine(Config.debugOutPath, romFilename + ".log.frozenBoy.txt");

            GameOptions gameOptions = new(romFilename, romPath, Palettes.GetWhitePalette());
            GameBoy gb = new(gameOptions);

            TestOptions testOptions = new(TestOutput.MD5, logExecution, logFilename);

            Driver driver = new();
            Result result = driver.RunTest(gb, testOptions);
            output.WriteLine(result.Message);
            return result.Passed;
        }

        [Fact]
        public void Test_MBC1_bits_bank1()
        {
            bool passed = Test("bits_bank1.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_bits_bank2()
        {
            bool passed = Test("bits_bank2.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_bits_mode()
        {
            bool passed = Test("bits_mode.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_bits_ramg()
        {
            bool passed = Test("bits_ramg.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_ram_64kb()
        {
            bool passed = Test("ram_64kb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_ram256kb()
        {
            bool passed = Test("ram_256kb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_rom512kb()
        {
            bool passed = Test("rom_512kb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_rom_1Mb()
        {
            bool passed = Test("rom_1Mb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_rom_2Mb()
        {
            bool passed = Test("rom_2Mb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_rom_4Mb()
        {
            bool passed = Test("rom_4Mb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_rom_8Mb()
        {
            bool passed = Test("rom_8Mb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_rom_16Mb()
        {
            bool passed = Test("rom_16Mb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_MBC1_multicart_rom_8Mb()
        {
            bool passed = Test("multicart_rom_8Mb.gb", Path.Combine(Config.moonEyePath,"emulator-only", "mbc1"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_call_timing()
        {
            bool passed = Test("call_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_call_cc_timing()
        {
            bool passed = Test("call_cc_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_div_timing()
        {
            bool passed = Test("div_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ei_sequence()
        {
            bool passed = Test("ei_sequence.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ei_timing()
        {
            bool passed = Test("ei_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_halt_ime1_timing()
        {
            bool passed = Test("halt_ime1_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_if_ie_registers()
        {
            bool passed = Test("if_ie_registers.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_intr_timing()
        {
            bool passed = Test("intr_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_jp_timing()
        {
            bool passed = Test("jp_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_jp_cc_timing()
        {
            bool passed = Test("jp_cc_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_oam_dma_start()
        {
            bool passed = Test("oam_dma_start.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_oam_dma_restart()
        {
            bool passed = Test("oam_dma_restart.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_oam_dma_timing()
        {
            bool passed = Test("oam_dma_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ld_hl_sp_e_timing()
        {
            bool passed = Test("ld_hl_sp_e_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_pop_timing()
        {
            bool passed = Test("pop_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_push_timing()
        {
            bool passed = Test("push_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_reti_intr_timing()
        {
            bool passed = Test("reti_intr_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ret_timing()
        {
            bool passed = Test("ret_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_reti_timing()
        {
            bool passed = Test("reti_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_ret_cc_timing()
        {
            bool passed = Test("ret_cc_timing.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_rapid_di_ei()
        {
            bool passed = Test("rapid_di_ei.gb", Path.Combine(Config.moonEyePath,"acceptance"), logExecution: false);
            Assert.True(passed);
        }


        [Fact]
        public void Test_mem_oam()
        {
            bool passed = Test("mem_oam.gb", Path.Combine(Config.moonEyePath,"acceptance", "bits"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_reg_f()
        {
            bool passed = Test("reg_f.gb", Path.Combine(Config.moonEyePath,"acceptance", "bits"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_daa()
        {
            bool passed = Test("daa.gb", Path.Combine(Config.moonEyePath,"acceptance", "instr"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_basic_oam_dma()
        {
            bool passed = Test("basic.gb", Path.Combine(Config.moonEyePath,"acceptance", "oam_dma"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_basic_reg_dma()
        {
            bool passed = Test("reg_read.gb", Path.Combine(Config.moonEyePath,"acceptance", "oam_dma"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_div_write()
        {
            bool passed = Test("div_write.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_rapid_toggle()
        {
            bool passed = Test("rapid_toggle.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim00_div_trigger()
        {
            bool passed = Test("tim00_div_trigger.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim01()
        {
            bool passed = Test("tim01.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim01_div_trigger()
        {
            bool passed = Test("tim01_div_trigger.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim10()
        {
            bool passed = Test("tim10.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim10_div_trigger()
        {
            bool passed = Test("tim10_div_trigger.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim11()
        {
            bool passed = Test("tim11.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tim11_div_trigger()
        {
            bool passed = Test("tim11_div_trigger.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tima_reload()
        {
            bool passed = Test("tima_reload.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tima_write_reloading()
        {
            bool passed = Test("tima_write_reloading.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_tma_write_reloading()
        {
            bool passed = Test("tma_write_reloading.gb", Path.Combine(Config.moonEyePath,"acceptance", "timer"), logExecution: false);
            Assert.True(passed);
        }

    }
}
