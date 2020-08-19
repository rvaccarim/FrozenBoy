using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Diagnostics;

namespace FrozenBoyCore {

    // MMU = Memory Management Unit
    public class MMU {
        // 0xFFFF = 65535
        public u8[] data = new u8[0xFFFF + 1];
        string linkPortText = "";

        //public u8[] boot;                     //     0 ->   255, 0x0000 -> 0x00FF, after boot it's used for Restart and Interrupt Vectors
        //public u8[] cartridge_header;         //   256 ->   335, 0x0100 -> 0x014F
        //public u8[] cartridge_bank0;          //   336 -> 16383, 0x0150 -> 0x3FFF 
        //public u8[] cartridge_switch;         // 16384 -> 32767, 0x4000 -> 0x7FFF switchable bank 1-7
        //public u8[] charVRAM;                 //  2768 -> 38911, 0x8000 -> 0x97FF Character RAM (VRAM) 
        //public u8[] BGMap1;                   // 38912 -> 39935, 0x9800 -> 0x9BFF BG Map Data 1 (VRAM)
        //public u8[] BGMap2;                   // 39936 -> 40959, 0x9C00 -> 0x9FFF BG Map Data 2 (VRAM)
        //public u8[] cWRAM;                    // 40960 -> 49151, 0xA000 -> 0xBFFF Cartridge RAM(If Available) (WRAM)
        //public u8[] iWRAM_bank0;              // 49152 -> 53247, 0xC000 -> 0xCFFF Internal RAM - Bank 0 (fixed) (WRAM)
        //public u8[] WRAM_switchable;          // 53248 -> 57343, 0xD000 -> 0xDFFF Internal RAM - Bank 1 - 7(switchable - CGB only)
        //public u8[] Echo;                     // 57344 -> 65023, 0xE000 -> 0xFDFF Echo RAM - Reserved, Do Not Use
        //public u8[] OAM;                      // 65024 -> 65183, 0xFE00 -> 0xFE9F OAM - Object Attribute Memory
        //public u8[] unusable;                 // 65184 -> 65279, 0xFEA0 -> 0xFEFF Unusable Memory
        //public u8[] IO;                       // 65280 -> 65407, 0xFF00 -> 0xFF7F Hardware I / O Registers
        //public u8[] HRAM;                     // 65408 -> 65534, 0xFF80 -> 0xFFFE Zero Page(HRAM / HiRam)
        //public u8[] IE;                       // 65535 -> 65535, 0xFFFF -> 0xFFFF (1 byte) Interrupt Enable Flag

        public MMU() {
            //boot = InitMemory(0x0000, 0x00FF);
            //cartridge_header = InitMemory(0x0100, 0x014F);
            //cartridge_bank0 = InitMemory(0x0150, 0x3FFF);
            //cartridge_switch = InitMemory(0x4000, 0x7FFF);
            //charVRAM = InitMemory(0x8000, 0x97FF);
            //BGMap1 = InitMemory(0x9800, 0x9BFF);
            //BGMap2 = InitMemory(0x9C00, 0x9FFF);
            //cWRAM = InitMemory(0xA000, 0xBFFF);
            //iWRAM_bank0 = InitMemory(0xC000, 0xCFFF);
            //WRAM_switchable = InitMemory(0xD000, 0xDFFF);
            //Echo = InitMemory(0xE000, 0xFDFF);
            //OAM = InitMemory(0xFE00, 0xFE9F);
            //unusable = InitMemory(0xFEA0, 0xFEFF);
            //IO = InitMemory(0xFF00, 0xFF7F);
            //HRAM = InitMemory(0xFF80, 0xFFFE);
            //IE = InitMemory(0xFFFF, 0xFFFF);
        }

        //public u8[] InitMemory(int from, int to) {
        //    u8[] memory = new u8[to - from + 1];
        //    return memory;
        //}



        public u8 Read8(u16 address) {
            //if (address == 0xFF44) {
            //    Debug.WriteLine("Reading 0xFF44");
            //}

            return data[address];

            //switch (address) {
            //    case u16 _ when address <= 0x00FF:
            //        return boot[address];
            //    case u16 _ when address <= 0x014F:
            //        return cartridge_header[address - 0x0100];
            //    case u16 _ when address <= 0x3FFF:
            //        return cartridge_bank0[address - 0x0150];
            //    case u16 _ when address <= 0x7FFF:
            //        return cartridge_switch[address - 0x4000];
            //    case u16 _ when address <= 0x97FF:
            //        return charVRAM[address - 0x8000];
            //    case u16 _ when address <= 0x9BFF:
            //        return BGMap1[address - 0x9800];
            //    case u16 _ when address <= 0x9FFF:
            //        return BGMap2[address - 0x9C00];
            //    case u16 _ when address <= 0xBFFF:
            //        return cWRAM[address - 0xA000];
            //    case u16 _ when address <= 0xCFFF:
            //        return iWRAM_bank0[address - 0xC000];
            //    case u16 _ when address <= 0xDFFF:
            //        return WRAM_switchable[address - 0xD000];
            //    case u16 _ when address <= 0xFDFF:
            //        return Echo[address - 0xE000];
            //    case u16 _ when address <= 0xFE9F:
            //        return OAM[address - 0xFE00];
            //    case u16 _ when address <= 0xFEFF:
            //        Debug.WriteLine("Unusable memory requested: {0:x4}", address);
            //        System.Environment.Exit(0);
            //        return 0xFF;
            //    case u16 _ when address <= 0xFF7F:
            //        return IO[address - 0xFF00];
            //    case u16 _ when address <= 0xFFFE:
            //        return HRAM[address - 0xFF80];
            //    case u16 _ when address <= 0xFFFF:
            //        return IE[address - 0xFFFF];
            //    default:
            //        Debug.WriteLine("Unmapped memory: {0:x4}", address);
            //        System.Environment.Exit(0);
            //        return 0xFF;
            //}
        }

        public u16 Read16(u16 address) {
            u8 a = data[address];
            u8 b = data[address + 1];
            return (u16)(b << 8 | a);
        }

        public void Write8(u16 address, u8 value) {
            data[address] = value;

            //if (address == 0xFF44) {
            //    Debug.WriteLine(String.Format("Written by opcode at {0:x4}, value {1:x2}", gb.cpu.opLocation, value));
            //}

            if (address == 0xFF02 && value == 0x81) {
                linkPortText += System.Convert.ToChar(data[0xFF01]);
                Debug.Write(System.Convert.ToChar(data[0xFF01]));
            }
        }

        public void Write16(u16 address, u16 value) {
            data[address + 1] = (u8)((value & 0b_11111111_00000000) >> 8);
            data[address] = (u8)(value & 0b_00000000_11111111);

            //if (address == 0xFF44) {
            //    Debug.WriteLine(String.Format("Written by opcode at {0:x4}, value {1:x4}", gb.cpu.opLocation, value));
            //}

        }

    }

}
