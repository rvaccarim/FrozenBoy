using FrozenBoyCore;
using FrozenBoyCore.Memory;
using FrozenBoyCore.Processor;
using FrozenBoyTest;
using System;
using System.Diagnostics;
using System.IO;
using Xunit.Abstractions;

namespace FrozenBoyConsole {
    class Program {
        static void Main(string[] args) {
            BlarggTest b = new BlarggTest();
            b.TestHaltBug();
            Console.ReadLine();
        }
    }
}
