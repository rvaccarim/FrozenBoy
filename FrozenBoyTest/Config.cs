namespace FrozenBoyTest
{
    public static class Config
    {
        //input 
        public readonly static string basePath       = @"D:\Users\frozen\Documents\02_cold\c03_programming\emulation\FrozenBoy\";
        public readonly static string gamesPath      = @"D:\Users\frozen\Documents\02_cold\c31_software\ROMs\GameBoy\Game Boy\Tested\";
        public readonly static string debugOutPath   = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        public readonly static string hashesPath     = basePath + @"FrozenBoyTest\Hashes\";
        public readonly static string mooneyePath    = basePath + @"ROMS\mooneye\";

        public readonly static string CPU_Path       = basePath + @"ROMS\blargg\cpu_instrs\individual\";
        public readonly static string InstTimingPath = basePath + @"ROMS\blargg\instr_timing\";
        public readonly static string MemTimingPath1 = basePath + @"ROMS\blargg\mem_timing\individual\";
        public readonly static string MemTimingPath2 = basePath + @"ROMS\blargg\mem_timing-2\rom_singles\";
        public readonly static string HaltPath       = basePath + @"ROMS\blargg\halt_bug\";
        public readonly static string OAMPath        = basePath + @"ROMS\blargg\oam_bug\rom_singles\";

    }
}
