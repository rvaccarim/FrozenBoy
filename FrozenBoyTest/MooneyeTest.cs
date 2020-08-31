using FrozenBoyCore;
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
        public void Test_Mem_OAM() {
            bool passed = Test(@"acceptance\bits\", "mem_oam.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_Reg_f() {
            bool passed = Test(@"acceptance\bits\", "reg_f.gb", false);
            Assert.True(passed);
        }

        private bool Test(string extraPath, string romName, bool logExecution) {
            string romFilename = mooneyePath + extraPath + romName;
            string logFilename = debugPath + romName + ".log.frozenBoy.txt";
            string expectedMD5 = File.ReadAllText(hashesPath + romName + ".hash.txt");

            TestOptions options = new TestOptions(TestOutput.MD5, expectedMD5, logExecution, logFilename);
            GameBoy gb = new GameBoy(romFilename);

            bool passed = gb.RunTest(options);
            output.WriteLine(gb.testResult);
            return passed;

        }
    }
}
