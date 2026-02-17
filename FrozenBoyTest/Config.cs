using System;
using System.IO;
using System.Runtime.Versioning;

namespace FrozenBoyTest
{
    public static class Config
    {
        private readonly static string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public readonly static string PathSeparator = Path.DirectorySeparatorChar.ToString();

        //input 
        public readonly static string basePath = Path.Combine(home, "Documents", "01_hot", "03_programming", "emulation", "FrozenBoy");
        public readonly static string gamesPath = Path.Combine(home, "Documents", "02_cold", "c31_software", "ROMS", "GameBoy", "Game Boy", "Tested");
        public readonly static string debugOutPath = Path.Combine(AppContext.BaseDirectory, "GB_Debug");

        // Real games tests
        public readonly static string hashesPath = Path.Combine(AppContext.BaseDirectory, "TestData", "GameHashes");

        // Mooneye tests
        public readonly static string moonEyePath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "mooneye");

        // Blargg tests
        public readonly static string CPUPath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "cpu_instrs", "individual");
        public readonly static string InstTimingPath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "instr_timing");
        public readonly static string MemTimingPath1 = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "mem_timing", "individual");
        public readonly static string MemTimingPath2 = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "mem_timing-2", "rom_singles");
        public readonly static string HaltPath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "halt_bug");
        public readonly static string OAMPath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "oam_bug", "rom_singles");

    }
}
