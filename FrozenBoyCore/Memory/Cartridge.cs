using System;
using System.IO;
using u8 = System.Byte;

namespace FrozenBoyCore.Memory
{

    public class Cartridge {

        private static readonly u8[] NintendoLogo = 
        [   0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D,
            0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E, 0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99,
            0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC, 0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E
        ];

        public enum BankType {
            None,
            MBC1,
            MBC2,
            MBC3
        }

        public BankType mbc;
        public int currentRomBank = 1;
        public int currentRamBank = 0;
        public int ramBanks;
        public int romBanks;
        public int memoryModel;
        public bool multicart;

        // this includes fixed rom at romBank 0 and swappable ROM
        // ROM banks are 0x4000 bytes long = 16K = 16384
        public u8[] rom;

        // max is 4 banks, 32K
        // ROM banks are 0x2000 bytes long =  8K = 8192
        public u8[] ram;
        public bool ramWriteEnabled;

        public Cartridge(string romName) {

            byte[] data = File.ReadAllBytes(romName);
            rom = new u8[data.Length];
            Buffer.BlockCopy(data, 0, rom, 0, data.Length);

            mbc = (rom[0x147]) switch
            {
                0x00 => BankType.None,
                0x01 => BankType.MBC1,
                0x02 => BankType.MBC1,
                0x03 => BankType.MBC1,
                0x05 => BankType.MBC2,
                0x06 => BankType.MBC2,
                0x0F => BankType.MBC3,
                0x10 => BankType.MBC3,
                0x11 => BankType.MBC3,
                0x12 => BankType.MBC3,
                0x13 => BankType.MBC3,
                _ => BankType.None,
            };

            romBanks = GetRomBanks(rom[0x0148]);
            multicart = romBanks == 64 && IsMulticart(rom);

            ramBanks = GetRamBanks(rom[0x0149]);

            if (ramBanks == 0) {
                ramBanks = 1;
            }

            ram = new u8[0x2000 * ramBanks];
            for (var i = 0; i < ram.Length; i++) {
                ram[i] = 0xff;
            }
        }

        private static int GetRomBanks(int id) {
            return id switch
            {
                0 => 2,
                1 => 4,
                2 => 8,
                3 => 16,
                4 => 32,
                5 => 64,
                6 => 128,
                7 => 256,
                0x52 => 72,
                0x53 => 80,
                0x54 => 96,
                _ => throw new ArgumentException("Unsupported ROM size")
            };
        }

        private static int GetRamBanks(int id) {
            return id switch
            {
                0 => 0,
                1 => 1,
                2 => 1,
                3 => 4,
                4 => 16,
                _ => throw new ArgumentException("Unsupported RAM size: ")
            };
        }

        private static bool IsMulticart(byte[] rom) {
            var logoCount = 0;
            for (var i = 0; i < rom.Length; i += 0x4000) {
                var logoMatches = true;
                for (var j = 0; j < NintendoLogo.Length; j++) {
                    if (rom[i + 0x104 + j] != NintendoLogo[j]) {
                        logoMatches = false;
                        break;
                    }
                }

                if (logoMatches) {
                    logoCount++;
                }
            }

            return logoCount > 1;
        }

    }
}
