using FrozenBoyCore;
using System;
using System.Diagnostics;

namespace FrozenBoyConsole {
    class Program {
        static void Main(string[] args) {

            GameBoy gameboy = new GameBoy();

            string command = "";
            while (command != "exit") {
                command = Console.ReadLine();

                if (command == "") {
                    Console.WriteLine(gameboy.cpu.GetCurrentInstruction());
                    gameboy.cpu.Next();
                    Console.WriteLine(gameboy.cpu.GetState());
                    Console.WriteLine("----------------------------------------------------------");
                }
            }
        }
    }
}
