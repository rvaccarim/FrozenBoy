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
    public class GamesTest {
        private readonly ITestOutputHelper output;

        private string gamesPath = @"D:\Users\frozen\Documents\09_software\ROMs\GameBoy\Game Boy\Tested\";
        private string hashesPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\FrozenBoyTest\Hashes\";
        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        public GamesTest(ITestOutputHelper output) {
            this.output = output;
        }


        public GamesTest() {
        }


        //[Fact]
        //public void Test_Zelda() {
        //    bool passed = Test(@"", "Alleyway (World).gb", false);
        //    Assert.True(passed);
        //}

        //private bool Test(string romName, bool logExecution) {
        //    string romFilename = gamesPath + romName;
        //    string logFilename = debugPath + romName + ".log.frozenBoy.txt";

        //    var expectedList = util.GetExpected(romFilename, gamesPath);

        //    foreach (var e in expectedList) {

        //    }
        //    //string expectedMD5 = File.ReadAllText(hashesPath + romName + ".hash.txt");

        //    //GameOptions gameOptions = new GameOptions(romFilename, new TestUtils().GetTestPalette());
        //    //GameBoy gb = new GameBoy(gameOptions);

        //    //TestOptions testOptions = new TestOptions(TestOutput.MD5, expectedList, logExecution, logFilename);
        //    //bool passed = gb.RunTest(testOptions);
        //    //output.WriteLine(gb.testResult);
        //    return passed;
        //}


    }
}
