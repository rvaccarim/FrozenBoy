using System;
using System.IO;
using System.Runtime.Versioning;

namespace FrozenBoyTest
{
    public static class Config
    {
        // debug output
        public readonly static string debugOutPath = Path.Combine(AppContext.BaseDirectory, "GB_Debug");

        // real games tests
        public readonly static string gamesRomsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "GameROMs");
        public readonly static string gameHashesPath = Path.Combine(AppContext.BaseDirectory, "TestData", "GameHashes");

        // mooneye tests
        public readonly static string moonEyePath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "mooneye");

        // blargg tests
        public readonly static string CPUPath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "cpu_instrs", "individual");
        public readonly static string InstTimingPath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "instr_timing");
        public readonly static string MemTimingPath1 = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "mem_timing", "individual");
        public readonly static string MemTimingPath2 = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "mem_timing-2", "rom_singles");
        public readonly static string HaltPath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "halt_bug");
        public readonly static string OAMPath = Path.Combine(AppContext.BaseDirectory, "TestData", "TestROMs", "blargg", "oam_bug", "rom_singles");

    }
}
