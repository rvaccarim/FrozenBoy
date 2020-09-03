using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Memory {
    public class Cartridge {

        public bool mbc1 = false;
        public bool mbc2 = false;
        public int currentRomBank = 1;
        public int currentRamBank = 0;
        public int ramSize;
        public int romBanks;

        // this includes fixed rom at romBank 0 and swappable ROM
        public u8[] rom;

        // max is 4 banks, 32K
        public u8[] switchableRAM = new u8[4 * 0x2000];

        public bool RAMEnabled;
        public bool romBanking;

        public Cartridge(byte[] data) {
            rom = new u8[data.Length];

            // 32K split in 16K
            Buffer.BlockCopy(data, 0, rom, 0, data.Length);

            // Value Definition
            // 00h No MBC
            // 01h MBC1
            // 02h MBC1 with external RAM
            // 03h MBC1 with battery-backed external RAM
            switch (rom[0x147]) {
                case 1:
                    mbc1 = true; break;
                case 2:
                    mbc1 = true; break;
                case 3:
                    mbc1 = true; break;
                case 5:
                    mbc2 = true; break;
                case 6:
                    mbc2 = true; break;
                default: break;
            }

            romBanks = rom[0x148];

            switch (data[0x149]) {
                case 0x00: ramSize = 0; break;
                case 0x01: ramSize = 2048; break;
                case 0x02: ramSize = 8192; break;
                case 0x03: ramSize = 32768; break;
                case 0x04: ramSize = 16 * 8192; break;
                case 0x05: ramSize = 8 * 8192; break;
            }

            switchableRAM = new u8[ramSize];
        }


    }
}
