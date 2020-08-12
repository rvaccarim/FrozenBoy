using FrozenBoy;
using System;
using Xunit;

namespace FrozenBoyTest {
    public class RegistersTest {
        [Fact]
        public void TestVirtualRegisters() {
            Registers registers = new Registers {
                a = 0b_1111_0000,
                f = 0b_1111_0000,

                b = 0b_1111_0000,
                c = 0b_1111_0000,

                d = 0b_1111_0000,
                e = 0b_1111_0000,

                h = 0b_1111_0000,
                l = 0b_1111_0000,

            };
            Assert.Equal(0b_1111_0000_1111_0000, registers.af);
            Assert.Equal(0b_1111_0000_1111_0000, registers.bc);
            Assert.Equal(0b_1111_0000_1111_0000, registers.de);
            Assert.Equal(0b_1111_0000_1111_0000, registers.hl);

            // the last four in f are always zero
            registers.af = 0b_1010_1010_1010_0000;
            registers.bc = 0b_1010_1010_1000_0001;
            registers.de = 0b_1010_1010_1000_0001;
            registers.hl = 0b_1010_1010_1000_0001;

            Assert.Equal(0b_1010_1010, registers.a);
            Assert.Equal(0b_1010_0000, registers.f);

            Assert.Equal(0b_1010_1010, registers.b);
            Assert.Equal(0b_1000_0001, registers.c);

            Assert.Equal(0b_1010_1010, registers.d);
            Assert.Equal(0b_1000_0001, registers.e);

            Assert.Equal(0b_1010_1010, registers.h);
            Assert.Equal(0b_1000_0001, registers.l);
        }


        [Fact]
        public void TestFlagRegister() {
            Registers registers = new Registers {
                f = 0b_1111_0000,
            };
            Assert.True(registers.flagZero);
            Assert.True(registers.flagSubtract);
            Assert.True(registers.flagHalfCarry);
            Assert.True(registers.flagCarry);

            registers.f = 0b_0111_0000;
            Assert.False(registers.flagZero);
            Assert.True(registers.flagSubtract);
            Assert.True(registers.flagHalfCarry);
            Assert.True(registers.flagCarry);

            registers.f = 0b_0011_0000;
            Assert.False(registers.flagZero);
            Assert.False(registers.flagSubtract);
            Assert.True(registers.flagHalfCarry);
            Assert.True(registers.flagCarry);

            registers.f = 0b_0001_0000;
            Assert.False(registers.flagZero);
            Assert.False(registers.flagSubtract);
            Assert.False(registers.flagHalfCarry);
            Assert.True(registers.flagCarry);

            registers.f = 0b_0000_0000;
            Assert.False(registers.flagZero);
            Assert.False(registers.flagSubtract);
            Assert.False(registers.flagHalfCarry);
            Assert.False(registers.flagCarry);
        }
    }
}
