﻿using FrozenBoyCore.Memory;
using System.Runtime.ExceptionServices;
using FrozenBoyCore.Processor;
using System.Runtime.CompilerServices;
using System;
using FrozenBoyCore.Util;
using u8 = System.Byte;
using s8 = System.SByte;
using u16 = System.UInt16;
using s16 = System.Int16;

namespace FrozenBoyCore.Graphics {

    public class GPU {
        public Space vRam;
        public Space oamRam;

        public const int MODE_HBLANK = 0b00;
        public const int MODE_VBLANK = 0b01;
        public const int MODE_SCANLINE_OAM = 0b10;
        public const int MODE_SCANLINE_VRAM = 0b11;

        // for the STATUS register
        private const int STATUS_COINCIDENCE_BITPOS = 2;
        private const int STATUS_HBLANK_BITPOS = 3;
        private const int STATUS_VBLANK_BITPOS = 4;
        private const int STATUS_SCANLINE_OAM_BITPOS = 5;
        private const int STATUS_LC_EQUALS_LYC = 6;

        public int modeTicks;
        public int lineTicks;
        public int enableDelay = 0;
        public bool wasDisabled = false;
        private byte[] frameBuffer = new byte[92160];

        private readonly InterruptManager intManager;

        private readonly byte modeMask = 0b_0000_0011;

        // Period                      GPU mode number     Time spent(clocks)
        // Horizontal blank                    0                  204
        // Vertical blank                      1                 4560 (10 lines)
        // Scanline(accessing OAM)             2                   80
        // Scanline(accessing VRAM)            3                  172
        // One line(scan and blank)                               456
        // Full frame(scans and vblank)                         70224
        public int mode;
        private byte lcd_control;

        // 0xFF40
        // GPU Registers
        // LCD and GPU Control
        // Bit Function                 
        // 7	Display: on/off
        // 6	Window Tile Map Display Select (0 = 9800-9BFF, 1 = 9C00-9FFF)
        // 5	Window Display: on/off  
        // 4	BG & Window Tile Data Select   (0 = 8800-97FF, 1 = 8000-8FFF)
        // 3	BG Tile Map Display Select     (0 = 9800-9BFF, 1 = 9C00-9FFF)
        // 2	OBJ (Sprite) Size (0=8x8, 1=8x16)
        // 1	OBJ (Sprite) Display Enable (0=Off, 1=On)                
        // 0	Background            
        public u8 LCDC {
            get => lcd_control;
            set {
                lcd_control = value;
                if (IsLcdEnabled()) {
                    EnableLCD();
                }
                else {
                    DisableLCD();
                }
            }
        }

        // 0xFF41 
        // Bits
        // 0-1
        //         00: H-Blank
        //         01: V-Blank
        //         10: Searching Sprites Atts
        //         11: Transfering Data to LCD Driver
        // 2       Set to 1 if register (0xFF44) is the same value as (0xFF45) 
        // 3, 4, 5 are interrupt enabled flags (similar to how the IE Register works), when the mode changes the
        //         corresponding bit 3,4,5 is set
        // 6       LYC=LY Coincidence Interrupt (1=Enable) (Read/Write)
        public u8 _stat;

        public u8 STAT {
            get => _stat;
            set {
                // undocumented bug
                // http://www.devrs.com/gb/files/faqs.html#GBBugs
                if (mode == MODE_VBLANK || mode == MODE_HBLANK) {
                    if (IsLcdEnabled()) {
                        intManager.RequestInterruption(InterruptionType.LCD);
                    }
                }
                BitUtils.ChangeBits(_stat, 0b_1111_1000, value);
            }
        }

        // 0xFF42, The Y Position of the BACKGROUND where to start drawing the viewing area from
        public u8 SCY { get; set; }
        // 0xFF43 The X Position of the BACKGROUND to start drawing the viewing area from
        public u8 SCX { get; set; }
        // 0xFF44 This is Y coordinate of the current line
        private byte _LY;
        public u8 LY {
            get { return _LY; }
            set { // it's ignored if the screen is enabled
                if (!IsLcdEnabled()) {
                    _LY = 0;
                }
            }
        }

        // 0xFF45 LYC - Scanline compare register
        public u8 LYC { get; set; }
        // 0xFF47
        // Every two bits in the palette data byte represent a colour.
        // Bits 7-6 maps to colour id 11, bits 5-4 map to colour id 10, bits 3-2 map to colour id 01 and bits 1-0 map to colour id 00. 
        // Each two bits will give the colour to use like so:
        // 00: White
        // 01: Light Grey
        // 10: Dark Grey
        // 11: Black
        public u8 BGP { get; set; }
        public u8 OBP0 { get; set; }
        public u8 OBP1 { get; set; }

        // 0xFF4A The Y Position of the VIEWING AREA to start drawing the window from
        public u8 WY { get; set; }
        // 0xFF4B The X Positions -7 of the VIEWING AREA to start drawing the window from
        public u8 WX { get; set; }

        public GPU(InterruptManager iManager) {
            vRam = new Space(0x8000, 0x9FFF);
            oamRam = new Space(0xFE00, 0xFE9F);

            this.intManager = iManager;
            mode = 0;
            //lcd_control = 0x91;
            _stat = 0b1000_0110;
            _LY = 0;
            //LYC = 0x0;
        }

        public void EnableLCD() {
            enableDelay = 244;
        }

        public void DisableLCD() {
            modeTicks = -2;
            lineTicks = 0;
            wasDisabled = true;
            enableDelay = 244;

            _LY = 0;
            _stat = (byte)(_stat & ~0x3);
            SetCoincidenceFlag();
        }

        public void Tick() {

            if (IsLcdEnabled()) {
                if (wasDisabled) {
                    enableDelay--;

                    if (enableDelay == 0) {
                        wasDisabled = false;
                    }
                    else {
                        return;
                    }
                }
            }
            else {
                return;
            }

            u8 status = _stat;
            u8 mode = (byte)(_stat & modeMask);

            modeTicks++;
            lineTicks++;

            // The VBlank interrupt triggers as soon as VBlank starts
            if (modeTicks == 4 && mode == MODE_VBLANK && _LY == 153) {
                _LY = 0;
                if (BitUtils.IsBitSet(status, STATUS_SCANLINE_OAM_BITPOS)) {
                    intManager.RequestInterruption(InterruptionType.LCD);
                }
            }
            else {
                switch (mode) {
                    case MODE_SCANLINE_OAM:
                        // it takes 80 Ticks for this step to be completed
                        if (modeTicks == 80) {
                            modeTicks = 0;
                            mode = MODE_SCANLINE_VRAM;
                        }
                        break;

                    case MODE_SCANLINE_VRAM:
                        if (modeTicks == 172) {
                            modeTicks = 0;
                            mode = MODE_HBLANK;

                            if (_LY == LYC) {
                                if (BitUtils.IsBitSet(status, STATUS_LC_EQUALS_LYC)) {
                                    intManager.RequestInterruption(InterruptionType.LCD);
                                }
                            }

                            RenderLine(_LY);

                            if (BitUtils.IsBitSet(_stat, STATUS_HBLANK_BITPOS)) {
                                intManager.RequestInterruption(InterruptionType.LCD);
                            }
                        }
                        break;
                    case MODE_HBLANK:
                        if (modeTicks == 204) {
                            modeTicks = 0;
                            lineTicks = 0;
                            _LY++;

                            if (_LY == 144) {
                                mode = MODE_VBLANK;
                                intManager.RequestInterruption(InterruptionType.VBlank);

                                if (BitUtils.IsBitSet(status, STATUS_VBLANK_BITPOS)) {
                                    intManager.RequestInterruption(InterruptionType.LCD);
                                }
                            }
                            else {
                                mode = MODE_SCANLINE_OAM;
                            }
                        }
                        break;
                    case MODE_VBLANK:
                        // 456 is a full line
                        if (modeTicks == 456) {
                            modeTicks = 0;
                            lineTicks = 0;
                            _LY++;

                            if (_LY == 1) {
                                // Restart scanning modes
                                mode = MODE_SCANLINE_OAM;
                                _LY = 0;

                                if (BitUtils.IsBitSet(status, STATUS_SCANLINE_OAM_BITPOS)) {
                                    intManager.RequestInterruption(InterruptionType.LCD);
                                }
                            }
                        }
                        break;
                }
            }

            // Set STAT flags according to the new mode
            status = BitUtils.ChangeBits(_stat, modeMask, mode);

            if (_LY == LYC) {
                status = BitUtils.BitSet(status, STATUS_COINCIDENCE_BITPOS);
            }
            else {
                status = BitUtils.BitReset(status, STATUS_COINCIDENCE_BITPOS);
            }

            _stat = status;

        }

        private void RenderLine(int line) {
            bool WindowTileMapSelect = BitUtils.IsBitSet(lcd_control, 6);
            bool WindowEnabled = BitUtils.IsBitSet(lcd_control, 5);
            bool TileDataSelect = BitUtils.IsBitSet(lcd_control, 4);
            bool BgTileMapSelect = BitUtils.IsBitSet(lcd_control, 3);
            bool SpriteEnabled = BitUtils.IsBitSet(lcd_control, 1);
            bool BgEnabled = BitUtils.IsBitSet(lcd_control, 0);

            u8[] BgPalette = GetPalette(BGP);
            u8[] Obj0Palette = GetPalette(OBP0);
            u8[] Obj1Palette = GetPalette(OBP1);

            int y = line;
            // do not move this inside the loop
            int winx = WX - 7;

            // RENDER TILES
            // the display is 166x144
            for (int x = 0; x < 160; x++) {
                if (BgEnabled) {
                    int realX = (x + SCX) % 256;
                    int realY = (y + SCY) % 256;
                    RenderTile(x, y, realX, realY, BgTileMapSelect, TileDataSelect, BgPalette);
                }

                if (WindowEnabled) {
                    if (y >= WY && x >= winx) {
                        int realX = x - winx;
                        int realY = y - WY;
                        RenderTile(x, y, realX, realY, WindowTileMapSelect, TileDataSelect, BgPalette);
                    }
                }
            }

            // RENDER SPRITES       
            if (SpriteEnabled) {
                RenderSprites(BgPalette, Obj0Palette, Obj1Palette);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderTile(int x, int y, int realX, int realY, bool mapSelect, bool tileDataSelect, u8[] BgPalette) {
            // the BG is 256x256 pixels
            // calculate the coordinates of the tile where the pixel belongs
            // there are 32 possible tiles (256 / 8)
            int tileCol = realX / 8;
            int tileRow = realY / 8;

            // Get tile number from memory map
            // map the values to a flat memory structure
            int tileId = (tileRow * 32) + tileCol;

            u8 tileNumber;
            if (mapSelect) {
                tileNumber = vRam[(u16)(0x9C00 + tileId)];
            }
            else {
                tileNumber = vRam[(u16)(0x9800 + tileId)];
            }

            // get tile data
            u16 tileAddress;
            if (tileDataSelect) {
                // unsigned $8000-8FFF
                tileAddress = (u16)(0x8000 + (tileNumber * 16));
            }
            else {
                // signed $8800-97FF (9000 = 0)
                s8 id = (s8)tileNumber;
                if (id >= 0) {
                    tileAddress = (u16)(0x9000 + (id * 16));
                }
                else {
                    tileAddress = (u16)(0x8800 + ((id + 128) * 16));
                }
            }

            int tileXpos = realX % 8;
            int tileYPos = realY % 8;

            // each pixel in the tile data set is represented by two bits
            u8 tileLow = vRam[(u16)(tileAddress + (tileYPos * 2))];
            u8 tileHigh = vRam[(u16)(tileAddress + (tileYPos * 2) + 1)];

            tileLow = (u8)((u8)(tileLow << tileXpos) >> 7);
            tileHigh = (u8)((u8)(tileHigh << tileXpos) >> 7);

            int colorId = (tileHigh << 1) + tileLow;
            u8 color = (u8)((3 - BgPalette[colorId]) * 85);
            u8[] colorData = { color, color, color, 255 }; // B G R
            WriteBuffer(x, y, colorData);
        }

        // Sprite attribute table, 0xFE00-0xFE9F
        // Each sprite has 4 bytes associated
        // 0: Sprite Y Position: Position of the sprite on the Y axis of the viewing display minus 16
        // 1: Sprite X Position: Position of the sprite on the X axis of the viewing display minus 8
        // 2: Pattern number: This is the sprite identifier used for looking up the sprite data in memory region 0x8000 - 0x8FFF
        // 3: Attributes: These are the attributes of the sprite
        //    Bit7: Sprite to Background Priority
        //    Bit6: Y flip
        //    Bit5: X flip
        //    Bit4: Palette number
        //    Bit3: Not used in standard gameboy
        //    Bit2 - 0: Not used in standard gameboy
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderSprites(u8[] BgPalette, u8[] Obj0Palette, u8[] Obj1Palette) {
            for (int i = 0; i < 40; i++) {
                u8 spriteY = (u8)(oamRam[(u16)(0xFE00 + i * 4)] - 16);
                u8 spriteX = (u8)(oamRam[(u16)(0xFE01 + i * 4)] - 8);
                u8 spriteNumber = oamRam[(u16)(0xFE02 + i * 4)];
                u8 spriteAttr = oamRam[(u16)(0xFE03 + i * 4)];

                int size = GetSpriteSize();

                if ((_LY >= spriteY) && (_LY < (spriteY + size))) {
                    int tileRow = IsYFlipped(spriteAttr) ? size - 1 - (_LY - spriteY) : (_LY - spriteY);

                    u16 spriteAddress = (u16)(0x8000 + (spriteNumber * 16) + (tileRow * 2));
                    u8 low = vRam[spriteAddress];
                    u8 high = vRam[(u16)(spriteAddress + 1)];

                    // render the 8x8 sprite pixels
                    for (int p = 0; p < 8; p++) {
                        int bitPos = !IsXFlipped(spriteAttr) ? p : 7 - p;

                        byte b1 = (byte)((byte)(low << bitPos) >> 7);
                        byte b2 = (byte)((byte)(high << bitPos) >> 7);
                        int colorId = (b2 << 1) + b1;

                        byte[] palette = BitUtils.IsBitSet(spriteAttr, 4) ? Obj1Palette : Obj0Palette;
                        byte color = (byte)((3 - palette[colorId]) * 85);

                        byte[] colorData = { color, color, color, 255 }; // B G R

                        if ((spriteX + p) >= 0 && (spriteX + p) < 160) {
                            if (!IsTransparent(colorId) && (IsAboveBG(spriteAttr) || IsBgWhite(spriteX + p, _LY, BgPalette))) {
                                WriteBuffer(spriteX + p, _LY, colorData);
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBgWhite(int x, int y, byte[] palette) {
            // check if the background is "white". 
            // White means the first color of the palette, it's not necessarily color white
            byte color = (byte)((3 - palette[0]) * 85);

            if (frameBuffer[(x + y * 160) * 4 + 0] == color &&
                frameBuffer[(x + y * 160) * 4 + 1] == color &&
                frameBuffer[(x + y * 160) * 4 + 2] == color &&
                frameBuffer[(x + y * 160) * 4 + 3] == 255)
                return true;

            return false;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsTransparent(int b) {
            return b == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsAboveBG(byte attr) {
            //Bit7 OBJ-to - BG Priority(0 = OBJ Above BG, 1 = OBJ Behind BG color 1 - 3)
            return attr >> 7 == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsXFlipped(u8 attr) {
            //Bit5   X flip(0 = Normal, 1 = Horizontally mirrored)
            return BitUtils.IsBitSet(attr, 5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsYFlipped(u8 attr) {
            //Bit6 Y flip(0 = Normal, 1 = Vertically mirrored)
            return BitUtils.IsBitSet(attr, 6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetSpriteSize() {
            //Bit 2 - OBJ (Sprite) Size (0=8x8, 1=8x16)
            return BitUtils.IsBitSet(lcd_control, 2) ? 16 : 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] GetPalette(u8 rawPalette) {
            byte[] palette = new byte[4];
            palette[0] = (byte)(rawPalette & 0x03);
            palette[1] = (byte)((rawPalette & 0x0C) >> 2);
            palette[2] = (byte)((rawPalette & 0x30) >> 4);
            palette[3] = (byte)((rawPalette & 0xC0) >> 6);
            return palette;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteBuffer(int x, int y, byte[] ColorData) {
            frameBuffer[(x + y * 160) * 4] = ColorData[0];
            frameBuffer[(x + y * 160) * 4 + 1] = ColorData[1];
            frameBuffer[(x + y * 160) * 4 + 2] = ColorData[2];
            frameBuffer[(x + y * 160) * 4 + 3] = ColorData[3];
        }

        public void SetCoincidenceFlag() {
            if (_LY == LYC) {
                _stat = (u8)(_stat | (1 << STATUS_COINCIDENCE_BITPOS));
            }
            else {
                _stat = (u8)(_stat & ~(1 << STATUS_COINCIDENCE_BITPOS));
            }
        }

        public bool IsLcdEnabled() {
            return BitUtils.IsBitSet(lcd_control, 7);
        }

        public byte[] GetScreenBuffer() {
            return frameBuffer;
        }

    }
}
