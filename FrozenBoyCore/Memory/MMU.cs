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

        private Cartridge cartridge;
        private readonly Timer timer;
        private readonly InterruptManager intManager;
        private readonly GPU gpu;
        private readonly Joypad joypad;
        private readonly Dma dma;
        private readonly SerialPort serial;

        public MMU(Timer timer, InterruptManager intManager, GPU gpu, Joypad joypad, Dma dma, SerialPort serial) {
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

            Write8(0xFF10, 0x80);
            Write8(0xFF11, 0xBF);
            Write8(0xFF12, 0xF3);
            Write8(0xFF14, 0xBF);
            Write8(0xFF16, 0x3F);
            Write8(0xFF19, 0xBF);
            Write8(0xFF1A, 0x7F);
            Write8(0xFF1B, 0xFF);
            Write8(0xFF1C, 0x9F);
            Write8(0xFF1E, 0xBF);
            Write8(0xFF20, 0xFF);
            Write8(0xFF23, 0xBF);
            Write8(0xFF24, 0x77);
            Write8(0xFF25, 0xF3);
            Write8(0xFF26, 0xF1);

            Write8(0xFF48, 0xFF);
            Write8(0xFF49, 0xFF);
        }


        public void LoadData(string romName) {
            cartridge = new Cartridge(romName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public u8 Read8(u16 address) {

            // Regular ROM
            if (address >= 0 && address < 0x4000) {
                return cartridge.rom[address];
            }

            // Switchable ROM / ROM memory bank
            if (address >= 0x4000 && address < 0x8000) {
                u16 newAdress = (u16)(address - 0x4000);
                return cartridge.rom[newAdress + (cartridge.currentRomBank * 0x4000)];
            }

            // Video Ram
            if (gpu.vRam.Manages(address)) {
                return gpu.vRam[address];
            }

            // Switchable RAM 
            if (address >= 0xA000 && address < 0xC000) {
                u16 newAdress = (u16)(address - 0xA000);
                return cartridge.switchableRAM[newAdress + (cartridge.currentRamBank * 0x2000)];
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
                0xFF42 => gpu.ScrollY,
                0xFF43 => gpu.ScrollX,
                0xFF44 => gpu.LY,
                0xFF45 => gpu.LYC,
                0xFF46 => dma.DMA_Register,
                0xFF47 => gpu.BGP,
                0xFF48 => gpu.OBP0,
                0xFF49 => gpu.OBP1,
                0xFF4A => gpu.WindowY,
                0xFF4B => gpu.WindowX,
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
                HandleBanking(address, value);
                return;
            }

            // video Ram
            if (gpu.vRam.Manages(address)) {
                gpu.vRam[address] = value;
                return;
            }

            // switchable RAM
            if ((address >= 0xA000) && (address < 0xC000)) {
                if (cartridge.RAMEnabled) {
                    u16 newAddress = (u16)(address - 0xA000);
                    cartridge.switchableRAM[newAddress + (cartridge.currentRamBank * 0x2000)] = value;
                }
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
                case 0xFF41: gpu.STAT = BitUtils.ChangeBits(gpu.STAT, 0b_1111_1000, value); break; // last 3 are read only
                case 0xFF42: gpu.ScrollY = value; break;
                case 0xFF43: gpu.ScrollX = value; break;
                case 0xFF44: gpu.LY = 0; break;        // LY = 0 if someone writes to it
                case 0xFF45: gpu.LYC = value; break;
                case 0xFF46: dma.DMA_Register = value; break;
                case 0xFF47: gpu.BGP = value; break;
                case 0xFF48: gpu.OBP0 = value; break;
                case 0xFF49: gpu.OBP1 = value; break;
                case 0xFF4A: gpu.WindowY = value; break;
                case 0xFF4B: gpu.WindowX = value; break;
                // joypad
                case 0xFF00: joypad.JOYP = value; break;
                // serial
                case 0xFF01: serial.SB = value; break;
                case 0xFF02: serial.SC = value; break;
                default: IO[address] = value; break;
            }
        }

        public void HandleBanking(u16 address, u8 data) {
            // do RAM enabling
            if (address < 0x2000) {
                if (cartridge.mbc1 || cartridge.mbc2) {
                    DoRAMBankEnable(address, data);
                }
            }
            else {
                // do ROM bank change
                if ((address >= 0x200) && (address < 0x4000)) {
                    if (cartridge.mbc1 || cartridge.mbc2) {
                        DoChangeLowROMBank(data);
                    }
                }
                else {
                    // do ROM or RAM bank change
                    if ((address >= 0x4000) && (address < 0x6000)) {
                        // there is no rambank in mbc2 so always use rambank 0
                        if (cartridge.mbc1) {
                            if (cartridge.romBanking) {
                                DoChangeHighRomBank(data);
                            }
                            else {
                                DoRAMBankChange(data);
                            }
                        }
                    }
                    else {
                        // this will change whether we are doing ROM banking
                        // or RAM banking with the above if statement
                        if ((address >= 0x6000) && (address < 0x8000)) {
                            if (cartridge.mbc1) {
                                DoChangeROMRAMMode(data);
                            }
                        }
                    }
                }
            }
        }

        private void DoRAMBankChange(u8 data) {
            cartridge.currentRamBank = data & 0x3;
        }

        private void DoRAMBankEnable(u16 address, u8 data) {
            if (cartridge.mbc2) {
                if (BitUtils.IsBitSet(BitUtils.Lsb(address), 4)) return;
            }

            u8 testData = (u8)(data & 0xF);
            if (testData == 0xA)
                cartridge.RAMEnabled = true;
            else if (testData == 0x0)
                cartridge.RAMEnabled = false;
        }

        private void DoChangeROMRAMMode(u8 data) {
            u8 newData = (u8)(data & 0x1);
            cartridge.romBanking = (newData == 0);
            if (cartridge.romBanking)
                cartridge.currentRamBank = 0;
        }

        private void DoChangeLowROMBank(u8 data) {
            if (cartridge.mbc2) {
                cartridge.currentRomBank = (u8)(data & 0xF);
                if (cartridge.currentRomBank == 0) {
                    cartridge.currentRomBank++;
                }

                return;
            }

            u8 lower5 = (u8)(data & 31);
            cartridge.currentRomBank &= 224; // turn off the lower 5
            cartridge.currentRomBank |= lower5;

            if (cartridge.currentRomBank == 0)
                cartridge.currentRomBank++;
        }

        private void DoChangeHighRomBank(u8 data) {
            // turn off the upper 3 bits of the current rom
            cartridge.currentRomBank &= 31;

            // turn off the lower 5 bits of the data
            data &= 224;
            cartridge.currentRomBank |= data;

            if (cartridge.currentRomBank == 0) {
                cartridge.currentRomBank++;
            }
        }
    }
}
