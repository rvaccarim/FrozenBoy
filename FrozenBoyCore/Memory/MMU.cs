﻿using System;
using System.Collections.Generic;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Processor;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Controls;
using System.Runtime.InteropServices.WindowsRuntime;
using FrozenBoyCore.Serial;

namespace FrozenBoyCore.Memory {

    public class MMU {
        public u8[] data = new u8[0xFFFF + 1];

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

            intManager.IF = 0b_0000_0001;

            data[0xFF10] = 0x80;
            data[0xFF11] = 0xBF;
            data[0xFF12] = 0xF3;
            data[0xFF14] = 0xBF;
            data[0xFF16] = 0x3F;
            data[0xFF19] = 0xBF;
            data[0xFF1A] = 0x7F;
            data[0xFF1B] = 0xFF;
            data[0xFF1C] = 0x9F;
            data[0xFF1E] = 0xBF;
            data[0xFF20] = 0xFF;
            data[0xFF23] = 0xBF;
            data[0xFF24] = 0x77;
            data[0xFF25] = 0xF3;
            data[0xFF26] = 0xF1;

            data[0xFF48] = 0xFF;
            data[0xFF49] = 0xFF;
        }

        public void LoadData(string romName) {
            byte[] romData = File.ReadAllBytes(romName);
            Buffer.BlockCopy(romData, 0, data, 0, romData.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public u8 Read8(u16 address) {

            // OAM range
            if (address >= 0xFE00 && address <= 0xFE9F) {
                if (dma.IsOamBlocked()) {
                    return 0xff;
                }
            }

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
                _ => data[address],
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write8(u16 address, u8 value) {
            // dont allow any writing to the read only memory
            if (address < 0x8000) {
                return;
            }

            // this area is restricted
            if ((address >= 0xFEA0) && (address < 0xFEFF)) {
                return;
            }

            // writing to ECHO ram also writes in RAM
            if ((address >= 0xE000) && (address < 0xFE00)) {
                data[address] = value;
                Write8((ushort)(address - 0x2000), value);
            }

            //// output to serial port
            //if (address == 0xFF02 && value == 0x81) {
            //    linkPortOutput += System.Convert.ToChar(data[0xFF01]);
            //}

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
                case 0xFF41: gpu.STAT = 0; break;      // STAT
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
                default: data[address] = value; break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public u16 Read16(u16 address) {
            u8 a = data[address];
            u8 b = data[address + 1];
            return (u16)(b << 8 | a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write16(u16 address, u16 value) {
            data[address + 1] = (u8)((value & 0b_11111111_00000000) >> 8);
            data[address] = (u8)(value & 0b_00000000_11111111);
        }
    }
}
