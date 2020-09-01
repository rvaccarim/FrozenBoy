using FrozenBoyCore.Memory;
using FrozenBoyCore.Processor;
using Xunit;
using FrozenBoyCore.Graphics;
using u8 = System.Byte;
using u16 = System.UInt16;
using System;
using Xunit.Abstractions;
using FrozenBoyCore.Controls;

namespace FrozenBoyTest {
    public class OpcodeListsTest {
        private ITestOutputHelper output;

        public OpcodeListsTest(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void TestOpcodeList() {
            InterruptManager intManager = new InterruptManager();
            Timer timer = new Timer(intManager);
            GPU gpu = new GPU(intManager);
            Joypad joypad = new Joypad(intManager);
            Dma dma = new Dma();
            MMU mmu = new MMU(timer, intManager, gpu, joypad, dma);
            CPU cpu = new CPU(mmu, timer, intManager);

            string msg;

            bool ok = true;
            foreach (var item in cpu.opHandler.opcodes) {
                var opcode = item.Value;

                if (opcode.tcycles == 4) {
                    if (opcode.steps.Length != 1) {
                        ok = false;
                        msg = "There should be only one step in a 1 M-cycle instruction: ";
                        output.WriteLine(String.Format("{0}   0x{1:X2}   {2}", msg, opcode.value, opcode.label));
                    }
                }
                else {
                    if (((opcode.steps.Length + 1) * 4) != opcode.tcycles) {
                        ok = false;
                        msg = "Steps don't match: ";
                        output.WriteLine(String.Format("{0}   0x{1:X2}   {2}", msg, opcode.value, opcode.label));
                    }
                }
            }

            Assert.True(ok);

        }

        [Fact]
        public void TestCBOpcodeList() {
            InterruptManager intManager = new InterruptManager();
            Timer timer = new Timer(intManager);
            GPU gpu = new GPU(intManager);
            Joypad joypad = new Joypad(intManager);
            Dma dma = new Dma();
            MMU mmu = new MMU(timer, intManager, gpu, joypad, dma);
            CPU cpu = new CPU(mmu, timer, intManager);

            string msg;

            bool ok = true;
            foreach (var item in cpu.cbHandler.cbOpcodes) {
                var opcode = item.Value;

                if (opcode.tcycles == 8) {
                    if (opcode.steps.Length != 1) {
                        ok = false;
                        msg = "There should be only one step in a 1 M-cycle instruction: ";
                        output.WriteLine(String.Format("{0}   0x{1:X2}   {2}", msg, opcode.value, opcode.label));
                    }
                }
                else {
                    if (((opcode.steps.Length + 2) * 4) != opcode.tcycles) {
                        ok = false;
                        msg = "Steps don't match: ";
                        output.WriteLine(String.Format("{0}   0x{1:X2}   {2}", msg, opcode.value, opcode.label));
                    }
                }
            }

            Assert.True(ok);

        }

    }
}
