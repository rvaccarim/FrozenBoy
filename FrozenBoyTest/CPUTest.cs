using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Diagnostics;
using Xunit.Abstractions;

namespace FrozenBoyTest {
    public class CPUTest {
        private ITestOutputHelper output;

        private const string opcodeFormat = "{0,-15} ;${1,-6:x4} O=0x{2:x2}";
        private const string stateFormat = "{0}   {1}";

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
            bool passed;
            string romFilename = romPath + romName;
            string debugOutput = debugPath + romName + ".debug.frozenBoy.txt";

            using (StreamWriter outputFile = new StreamWriter(debugOutput)) {
                GameBoy gb = new GameBoy(romFilename);
                passed = RunROM(gb, outputFile);
            }

            return passed;
        }

        private bool RunROM(GameBoy gb, StreamWriter outputFile) {
            int prevPC = gb.cpu.regs.PC;

            while (true) {
                gb.cpu.ExecuteNext();

                string instruction = OpcodeToStr(gb, opcodeFormat, gb.cpu.prevOpcode, prevPC, gb.cpu.mmu);
                outputFile.WriteLine(String.Format(stateFormat, instruction.Substring(16), gb.cpu.regs.ToString()));

                prevPC = gb.cpu.regs.PC;

                if (gb.mmu.linkPortText.Contains("Passed")) {
                    return true;
                }

                if (gb.mmu.linkPortText.Contains("Failed")) {
                    output.WriteLine(gb.mmu.linkPortText);
                    return false;
                }
            }
        }


        private string OpcodeToStr(GameBoy gb, string format, Opcode o, int address, MMU m) {
            return o.length switch
            {
                2 => String.Format(format,
                                   String.Format(o.asmInstruction, m.Read8((u16)(address + 1))),
                                   address,
                                   o.value),

                3 => String.Format(format,
                                   String.Format(o.asmInstruction, m.Read16((u16)(address + 1))),
                                   address,
                                   o.value),

                _ => String.Format(format,
                                   String.Format(o.value != 0xCB ? o.asmInstruction
                                                                 : gb.cpu.cbOpcodes[m.Read8((u16)(address + 1))].asmInstruction),
                                   address,
                                   o.value),
            };
        }
    }
}
