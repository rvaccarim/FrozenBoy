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
        private Palettes palettes;

        private string gamesPath = @"D:\Users\frozen\Documents\09_software\ROMs\GameBoy\Game Boy\Tested\";
        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        public GamesTest(ITestOutputHelper output) {
            this.output = output;
            palettes = new Palettes();
        }

        [Fact]
        public void Test_Alleyway() {
            bool passed = Test("Alleyway (World).gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_Zelda() {
            bool passed = Test("Legend of Zelda, The - Link's Awakening.gb", false);
            Assert.True(passed);
        }

        private bool Test(string romFilename, bool logExecution) {
            string logFilename = debugPath + romFilename + ".log.frozenBoy.txt";

            GameOptions gameOptions = new GameOptions(romFilename, gamesPath, palettes.GetGreenPalette());
            GameBoy gb = new GameBoy(gameOptions);

            TestOptions testOptions = new TestOptions(TestOutput.MD5, logExecution, logFilename);

            Driver driver = new Driver();
            Result result = driver.RunTest(gb, testOptions);
            output.WriteLine(result.Message);
            return result.Passed;

        }


    }
}
