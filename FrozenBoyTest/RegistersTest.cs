using FrozenBoyCore;
using System;
using Xunit;

namespace FrozenBoyTest {
    public class RegistersTest {
        [Fact]
        public void TestVirtualRegisters() {
            Registers registers = new Registers {
                A = 0b_1111_0000,
                F = 0b_1111_0000,

                B = 0b_1111_0000,
                C = 0b_1111_0000,

                D = 0b_1111_0000,
                E = 0b_1111_0000,

                H = 0b_1111_0000,
                L = 0b_1111_0000,

            };
            Assert.Equal(0b_1111_0000_1111_0000, registers.AF);
            Assert.Equal(0b_1111_0000_1111_0000, registers.BC);
            Assert.Equal(0b_1111_0000_1111_0000, registers.DE);
            Assert.Equal(0b_1111_0000_1111_0000, registers.HL);

            // the last four in f are always zero
            registers.AF = 0b_1010_1010_1010_0000;
            registers.BC = 0b_1010_1010_1000_0001;
            registers.DE = 0b_1010_1010_1000_0001;
            registers.HL = 0b_1010_1010_1000_0001;

            Assert.Equal(0b_1010_1010, registers.A);
            Assert.Equal(0b_1010_0000, registers.F);

            Assert.Equal(0b_1010_1010, registers.B);
            Assert.Equal(0b_1000_0001, registers.C);

            Assert.Equal(0b_1010_1010, registers.D);
            Assert.Equal(0b_1000_0001, registers.E);

            Assert.Equal(0b_1010_1010, registers.H);
            Assert.Equal(0b_1000_0001, registers.L);
        }


        [Fact]
        public void TestFlagRegister() {
            Registers registers = new Registers {
                F = 0b_1111_0000,
            };
            Assert.True(registers.FlagZ);
            Assert.True(registers.FlagN);
            Assert.True(registers.FlagH);
            Assert.True(registers.FlagC);

            registers.F = 0b_0111_0000;
            Assert.False(registers.FlagZ);
            Assert.True(registers.FlagN);
            Assert.True(registers.FlagH);
            Assert.True(registers.FlagC);

            registers.F = 0b_0011_0000;
            Assert.False(registers.FlagZ);
            Assert.False(registers.FlagN);
            Assert.True(registers.FlagH);
            Assert.True(registers.FlagC);

            registers.F = 0b_0001_0000;
            Assert.False(registers.FlagZ);
            Assert.False(registers.FlagN);
            Assert.False(registers.FlagH);
            Assert.True(registers.FlagC);

            registers.F = 0b_0000_0000;
            Assert.False(registers.FlagZ);
            Assert.False(registers.FlagN);
            Assert.False(registers.FlagH);
            Assert.False(registers.FlagC);
        }
    }
}
