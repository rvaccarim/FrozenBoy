using System.IO;
using FrozenBoyCore;
using Xunit;
using Xunit.Abstractions;

namespace FrozenBoyTest
{
    public class GamesTest(ITestOutputHelper output)
    {
        private bool Test(string romFilename, bool logExecution)
        {
            Directory.CreateDirectory(Config.debugOutPath);
            string logFilename = Path.Combine(Config.debugOutPath, romFilename + ".log.frozenBoy.txt");

            GameOptions gameOptions = new(romFilename, Config.gamesRomsPath, Palettes.GetGreenPalette());
            GameBoy gb = new(gameOptions);

            TestOptions testOptions = new(TestOutput.MD5, logExecution, logFilename);

            Driver driver = new();
            Result result = driver.RunTest(gb, testOptions);
            output.WriteLine(result.Message);
            return result.Passed;

        }

        [Fact]
        public void Test_Alleyway() {
            bool passed = Test("Alleyway (World).gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_Zelda() {
            bool passed = Test("Legend of Zelda, The - Link's Awakening.gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_CastlevaniaTheAdventure() {
            bool passed = Test("Castlevania - The Adventure (USA).gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_Castlevania2() {
            bool passed = Test("Castlevania II - Belmont's Revenge (USA, Europe).gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_DonkeyKong() {
            bool passed = Test("Donkey Kong (World) (Rev A) (SGB Enhanced).gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_DrMario() {
            bool passed = Test("Dr. Mario (World) (Rev A).gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_Kirby() {
            bool passed = Test("Kirby's Dream Land (USA, Europe).gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_Retroid() {
            bool passed = Test("Retroid.gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_RoadRash() {
            bool passed = Test("Road Rash (USA, Europe).gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_SpaceInvaders() {
            bool passed = Test("Space Invaders (USA) (SGB Enhanced).gb", logExecution: false);;
            Assert.True(passed);
        }

        [Fact]
        public void Test_Tetris() {
            bool passed = Test("Tetris (World) (Rev A).gb", logExecution: false);;
            Assert.True(passed);
        }




    }
}
