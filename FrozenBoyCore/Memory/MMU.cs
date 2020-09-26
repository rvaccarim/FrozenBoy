using System;
using FrozenBoyCore.Processor;
using System.Runtime.CompilerServices;
using System.IO;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Controls;
using FrozenBoyCore.Serial;
using FrozenBoyCore.Util;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Memory {

    public class MMU {
        public Space internalRam;
        public Space echoRam;
        public Space unusable;
        public Space IO;
        public Space highRam;
        public Cartridge cartridge;

        private readonly Timer timer;
        private readonly InterruptManager intManager;
        private readonly GPU gpu;
        private readonly Joypad joypad;
        private readonly Dma dma;
        private readonly SerialLink serial;
        // private StreamWriter logFile;

        public MMU(Timer timer, InterruptManager intManager, GPU gpu, Joypad joypad, Dma dma, SerialLink serial) {
            // logFile = new StreamWriter(@"D:\Users\frozen\Documents\99_temp\GB_Debug\rom_1Mb.gb.log.FrozenBoy.memory.txt");

            this.timer = timer;
            this.intManager = intManager;
            this.gpu = gpu;
            this.joypad = joypad;
            this.dma = dma;
            this.serial = serial;

            internalRam = new Space(0xC000, 0xDFFF);
            echoRam = new Space(0xE000, 0xFDFF);
            unusable = new Space(0xFEA0, 0xFEFF);
            IO = new Space(0xFF00, 0xFF7F);
            highRam = new Space(0xFF80, 0xFFFE);

            intManager.IF = 0b_0000_0001;

            Write8(0xFF00, 0xCF);  // JOYP
            Write8(0xFF02, 0x7E);  // SC
            Write8(0xFF04, 0xAB);  // DIV
            Write8(0xFF08, 0xF8);  // TAC 
            Write8(0xFF0F, 0xE1);  // IF 

            // Sound 1
            Write8(0xFF10, 0x80);  // ENT1
            Write8(0xFF11, 0xBF);  // LEN1
            Write8(0xFF12, 0xF3);  // ENV1
            Write8(0xFF13, 0xC1);  // FRQ1
            Write8(0xFF14, 0xBF);  // KIK1

            Write8(0xFF15, 0xFF);  // N/A
            Write8(0xFF16, 0x3F);  // LEN2
            Write8(0xFF19, 0xB8);  // KIK2
            Write8(0xFF1A, 0x7F);
            Write8(0xFF1B, 0xFF);
            Write8(0xFF1C, 0x9F);
            Write8(0xFF1E, 0xBF);
            Write8(0xFF20, 0xFF);
            Write8(0xFF23, 0xBF);
            Write8(0xFF24, 0x77);
            Write8(0xFF25, 0xF3);
            Write8(0xFF26, 0xF1);

            // graphics
            Write8(0xFF40, 0x91);  // LCDC
            Write8(0xFF41, 0x85);  // STAT
            Write8(0xFF46, 0xFF);  // DMA
            Write8(0xFF47, 0xFC);  // BGP
            Write8(0xFF48, 0xFF);  // OBJ0
            Write8(0xFF49, 0xFF);  // OBJ1

            Write8(0xFF70, 0xFF);  // SVBK
            Write8(0xFF4F, 0xFF);  // VBK
            Write8(0xFF4D, 0xFF);  // KEY1


        }


        public void LoadData(string romName) {
            cartridge = new Cartridge(romName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public u8 Read8(u16 address) {

            // Regular ROM
            if (address < 0x4000) {
                if (cartridge.mbc == Cartridge.BankType.None) {
                    return cartridge.rom[address];
                }
                else {
                    if (cartridge.mbc == Cartridge.BankType.MBC1) {
                        var bank = GetMBC1_RomBankLow();
                        int newAddress = (bank * 0x4000) + address;
                        // Log("R", newAddress, cartridge.rom[newAddress]);
                        return cartridge.rom[newAddress];
                    }
                }
            }

            // Switchable ROM / ROM memory bank
            if (address >= 0x4000 && address < 0x8000) {
                if (cartridge.mbc == Cartridge.BankType.None) {
                    return cartridge.rom[address];
                }
                else {
                    if (cartridge.mbc == Cartridge.BankType.MBC1) {
                        // this can be larger than u16
                        int newAdress = address - 0x4000 + (GetMBC1_RomBankHigh() * 0x4000);

                        if (newAdress < cartridge.rom.Length) {
                            // Log("R", address, cartridge.rom[newAdress]);
                            return cartridge.rom[newAdress];
                        }
                        // Log("R", address, 0xff);
                        return 0xff;
                    }
                }
            }

            // Video Ram
            if (gpu.vRam.Manages(address)) {
                return gpu.vRam[address];
            }

            // RAM 
            if (address >= 0xA000 && address < 0xC000) {
                int newAdress;

                if (cartridge.mbc == Cartridge.BankType.None) {
                    return cartridge.rom[address];
                }
                else {
                    if (cartridge.mbc == Cartridge.BankType.MBC1) {
                        if (cartridge.ramWriteEnabled) {
                            newAdress = GetMBC1_RamAddress(address);

                            if (newAdress < cartridge.ram.Length) {
                                return cartridge.ram[newAdress];
                            }
                            return 0xff;
                        }
                        return 0xff;
                    }
                }
            }

            if (internalRam.Manages(address)) {
                return internalRam[address];
            }

            if (echoRam.Manages(address)) {
                return echoRam[address];
            }

            // OAM range - Sprites
            if (address >= 0xFE00 && address < 0xFEA0) {
                if (dma.IsOamBlocked()) {
                    return 0xff;
                }
            }

            if (gpu.oamRam.Manages(address)) {
                return gpu.oamRam[address];
            }

            // not sure if I should return something...
            if (unusable.Manages(address)) {
                return unusable[address];
            }

            if (highRam.Manages(address)) {
                return highRam[address];
            }

            // IO Registers
            return address switch
            {
                // timer
                0xFF04 => timer.DIV,
                0xFF05 => timer.TIMA,
                0xFF06 => timer.TMA,
                0xFF07 => timer.TAC,
                // interruptions
                0xFFFF => intManager.IE,
                0xFF0F => intManager.IF,
                // graphics
                0xFF40 => gpu.LCDC,
                0xFF41 => gpu.STAT,
                0xFF42 => gpu.SCY,
                0xFF43 => gpu.SCX,
                0xFF44 => gpu.LY,
                0xFF45 => gpu.LYC,
                0xFF46 => dma.DMA_Register,
                0xFF47 => gpu.BGP,
                0xFF48 => gpu.OBP0,
                0xFF49 => gpu.OBP1,
                0xFF4A => gpu.WY,
                0xFF4B => gpu.WX,
                // joypad
                0xFF00 => joypad.JOYP,
                // serial
                0xFF01 => serial.SB,
                0xFF02 => serial.SC,
                // Case none of the above
                _ => IO[address],
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write8(u16 address, u8 value) {

            // ROM memory is read only, but there are some interactions to enable ROM and RAM banking
            if (address < 0x8000) {
                if (cartridge.mbc != Cartridge.BankType.None) {
                    HandleBanking(address, value);
                }
                return;
            }

            if ((address >= 0xA000) && (address < 0xC000)) {
                if (cartridge.mbc == Cartridge.BankType.MBC1) {
                    if (cartridge.ramWriteEnabled) {
                        int newAddress = GetMBC1_RamAddress(address);
                        if (newAddress < cartridge.ram.Length) {
                            cartridge.ram[newAddress] = value;
                            // Log("W", address, value);
                        }
                    }
                }
                return;
            }

            // video Ram
            if (gpu.vRam.Manages(address)) {
                gpu.vRam[address] = value;
                return;
            }

            // internal RAM 
            if (internalRam.Manages(address)) {
                internalRam[address] = value;
                return;
            }

            // writing here also writes in RAM
            if (echoRam.Manages(address)) {
                echoRam[address] = value;
                internalRam[(u16)(address - 0x2000)] = value;
                return;
            }

            if (gpu.oamRam.Manages(address)) {
                gpu.oamRam[address] = value;
                return;
            }

            // unusable memory
            if (unusable.Manages(address)) {
                return;
            }

            // High Ram
            if (highRam.Manages(address)) {
                highRam[address] = value;
                return;
            }

            // IO
            switch (address) {
                case 0xFF04: timer.DIV = value; break;
                case 0xFF05: timer.TIMA = value; break;
                case 0xFF06: timer.TMA = value; break;
                case 0xFF07: timer.TAC = value; break;
                // interrupts
                case 0xFFFF: intManager.IE = value; break;
                case 0xFF0F: intManager.IF = value; break;
                // graphics
                case 0xFF40: gpu.LCDC = value; break;
                case 0xFF41: gpu.STAT = value; break;
                case 0xFF42: gpu.SCY = value; break;
                case 0xFF43: gpu.SCX = value; break;
                case 0xFF44: gpu.LY = value; break;
                case 0xFF45: gpu.LYC = value; break;
                case 0xFF46: dma.DMA_Register = value; break;
                case 0xFF47: gpu.BGP = value; break;
                case 0xFF48: gpu.OBP0 = value; break;
                case 0xFF49: gpu.OBP1 = value; break;
                case 0xFF4A: gpu.WY = value; break;
                case 0xFF4B: gpu.WX = value; break;
                // joypad
                case 0xFF00: joypad.JOYP = value; break;
                // serial
                case 0xFF01: serial.SB = value; break;
                case 0xFF02: serial.SC = value; break;
                default: IO[address] = value; break;
            }
        }

        public void HandleBanking(u16 address, u8 value) {
            // This is Read only memory, no data is changed, only the values related to ROM
            // and RAM banking

            if (address >= 0x0000 && address < 0x2000) {
                if (cartridge.mbc == Cartridge.BankType.MBC1) {
                    cartridge.ramWriteEnabled = (value & 0b1111) == 0b1010;
                    // Log("W", address, value);
                    if (!cartridge.ramWriteEnabled) {
                        // _battery.SaveRam(cartridge.ram);
                    }
                }
                return;
            }

            if ((address >= 0x2000) && (address < 0x4000)) {
                if (cartridge.mbc == Cartridge.BankType.MBC1) {
                    var bank = cartridge.currentRomBank & 0b0110_0000;
                    bank |= (value & 0b00011111);
                    cartridge.currentRomBank = bank;
                    // Log("W", address, value);
                }
                return;
            }

            if ((address >= 0x4000) && (address < 0x8000)) {
                if (cartridge.mbc == Cartridge.BankType.MBC1) {
                    if ((address >= 0x4000) && (address < 0x6000) && cartridge.memoryModel == 0) {
                        var bank = cartridge.currentRomBank & 0b0001_1111;
                        bank |= ((value & 0b11) << 5);
                        cartridge.currentRomBank = bank;
                        // Log("W", address, value);
                    }
                    else {
                        if ((address >= 0x4000) && (address < 0x6000) && cartridge.memoryModel == 1) {
                            var bank = value & 0b11;
                            cartridge.currentRamBank = bank;
                        }
                        else {
                            if ((address >= 0x6000) && (address < 0x8000)) {
                                cartridge.memoryModel = value & 1;
                            }
                        }
                    }
                }
                return;
            }
        }

        private int GetMBC1_RamAddress(int address) {
            if (cartridge.memoryModel == 0) {
                return address - 0xA000;
            }
            else {
                return (cartridge.currentRamBank % cartridge.ramBanks) * 0x2000 + (address - 0xA000);
            }
        }

        private int GetMBC1_RomBankLow() {
            if (cartridge.memoryModel == 0) {
                return 0;
            }
            else {
                var bank = (cartridge.currentRamBank << 5);
                if (cartridge.multicart) {
                    bank >>= 1;
                }

                bank %= cartridge.romBanks;
                return bank;
            }
        }

        private int GetMBC1_RomBankHigh() {
            var bank = cartridge.currentRomBank;
            if (bank % 0x20 == 0) {
                bank++;
            }

            if (cartridge.memoryModel == 1) {
                bank &= 0b0001_1111;
                bank |= (cartridge.currentRamBank << 5);
            }

            if (cartridge.multicart) {
                bank = ((bank >> 1) & 0x30) | (bank & 0x0f);
            }

            bank %= cartridge.romBanks;

            return bank;
        }


        //public void Log(string action, int adddress, int value) {

        //    if (action.Equals("R") && adddress == 0x4000 && cartridge.currentRomBank == 3) {
        //        int z = 0;
        //    }

        //    logFile.WriteLine(String.Format("{0} {1:x4} {2:x2} cRom={3} cRam={4} mm={5} rw={6}",
        //                             action, adddress, value, cartridge.currentRomBank, cartridge.currentRamBank,
        //                             cartridge.memoryModel, cartridge.ramWriteEnabled));
        //    logFile.Flush();
        //}
    }
}
