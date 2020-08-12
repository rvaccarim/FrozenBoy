using FrozenBoy;
using System;
using System.Diagnostics;

namespace FrozenBoyConsole {
    class Program {
        static void Main(string[] args) {

            CPU cpu = new CPU();
            cpu.registers.b = 0b_1111_0000;
            cpu.registers.c = 0b_1111_0000;
            Debug.WriteLine(Convert.ToString(cpu.registers.bc, toBase: 2));

            cpu.registers.bc = 0b_1010_1010_1000_0001;
            Debug.WriteLine(Convert.ToString(cpu.registers.b, toBase: 2));
            Debug.WriteLine(Convert.ToString(cpu.registers.c, toBase: 2));

        }
    }
}
