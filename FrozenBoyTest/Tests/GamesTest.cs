﻿using FrozenBoyCore;
using Xunit;
using Xunit.Abstractions;

namespace FrozenBoyTest
{
    public class GamesTest(ITestOutputHelper output)
    {
        private bool Test(string romFilename, bool logExecution)
        {
            string logFilename = Config.debugOutPath + romFilename + ".log.frozenBoy.txt";

            GameOptions gameOptions = new(romFilename, Config.gamesPath, Palettes.GetGreenPalette());
            GameBoy gb = new(gameOptions);

            TestOptions testOptions = new(TestOutput.MD5, logExecution, logFilename);

            Driver driver = new();
            Result result = driver.RunTest(gb, testOptions);
            output.WriteLine(result.Message);
            return result.Passed;

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

        [Fact]
        public void Test_CastlevaniaTheAdventure() {
            bool passed = Test("Castlevania - The Adventure (USA).gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_Castlevania2() {
            bool passed = Test("Castlevania II - Belmont's Revenge (USA, Europe).gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_DonkeyKong() {
            bool passed = Test("Donkey Kong (World) (Rev A) (SGB Enhanced).gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_DrMario() {
            bool passed = Test("Dr. Mario (World) (Rev A).gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_Kirby() {
            bool passed = Test("Kirby's Dream Land (USA, Europe).gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_Retroid() {
            bool passed = Test("Retroid.gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_RoadRash() {
            bool passed = Test("Road Rash (USA, Europe).gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_SpaceInvaders() {
            bool passed = Test("Space Invaders (USA) (SGB Enhanced).gb", false);
            Assert.True(passed);
        }

        [Fact]
        public void Test_Tetris() {
            bool passed = Test("Tetris (World) (Rev A).gb", false);
            Assert.True(passed);
        }




    }
}
