using FrozenBoyCore.Memory;
using System.Runtime.ExceptionServices;
using FrozenBoyCore.Processor;
using u8 = System.Byte;
using s8 = System.SByte;
using u16 = System.UInt16;
using s16 = System.Int16;
using System.Runtime.CompilerServices;
using System;

namespace FrozenBoyCore.Graphics {

    public class GPU {
        private const int MODE_HBLANK = 0b00;
        private const int MODE_VBLANK = 0b01;
        private const int MODE_SCANLINE_OAM = 0b10;
        private const int MODE_SCANLINE_VRAM = 0b11;

        // for the STATUS register
        private const int STATUS_COINCIDENCE_BITPOS = 2;
        private const int STATUS_VBLANK_BITPOS = 4;
        private const int STATUS_SCANLINE_OAM_BITPOS = 5;

        public int modeTicks;
        public int lineTicks;
        public int enableDelay = 0;
        public bool wasDisabled = false;
        private byte[] frameBuffer = new byte[92160];

        private MMU mmu;
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
        public u8 STAT { get; set; }

        // 0xFF42, The Y Position of the BACKGROUND where to start drawing the viewing area from
        public u8 ScrollY { get; set; }
        // 0xFF43 The X Position of the BACKGROUND to start drawing the viewing area from
        public u8 ScrollX { get; set; }
        // 0xFF44 This is Y coordinate of the current line
        public u8 LY { get; set; }
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
        public u8 BGPalette { get; set; }

        // 0xFF4A The Y Position of the VIEWING AREA to start drawing the window from
        public u8 windowY { get; set; }
        // 0xFF4B The X Positions -7 of the VIEWING AREA to start drawing the window from
        public u8 windowX { get; set; }

        public GPU(InterruptManager iManager) {
            this.intManager = iManager;
            mode = 0;
            lcd_control = 0x91;
            STAT = 0b1000_0110;
            LYC = 0x0;
        }

        public void SetMMU(MMU mmu) {
            this.mmu = mmu;
        }

        public void EnableLCD() {
            enableDelay = 244;
        }

        public void DisableLCD() {
            modeTicks = -2;
            lineTicks = 0;
            wasDisabled = true;
            enableDelay = 244;

            LY = 0;
            STAT = (byte)(STAT & ~0x3);
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

            u8 status = STAT;
            var mode = STAT & modeMask;

            modeTicks++;
            lineTicks++;

            // The VBlank interrupt triggers as soon as VBlank starts
            if (modeTicks == 4 && mode == MODE_VBLANK && LY == 153) {
                LY = 0;
                if (IsBitSet(status, STATUS_SCANLINE_OAM_BITPOS)) {
                    intManager.RequestLCD();
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
                            RenderLine(LY);
                        }
                        break;
                    case MODE_HBLANK:
                        if (modeTicks == 204) {
                            modeTicks = 0;
                            lineTicks = 0;
                            LY++;

                            if (LY == 144) {
                                mode = MODE_VBLANK;
                                // _canvas.putImageData(GPU._scrn, 0, 0);

                                intManager.RequestVBlank();
                                if (IsBitSet(status, STATUS_VBLANK_BITPOS)) {
                                    intManager.RequestLCD();
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
                            LY++;

                            if (LY == 1) {
                                // Restart scanning modes
                                mode = MODE_SCANLINE_OAM;
                                LY = 0;

                                if (IsBitSet(status, STATUS_SCANLINE_OAM_BITPOS)) {
                                    intManager.RequestLCD();
                                }
                            }
                        }
                        break;
                }
            }

            status = (u8)(STAT & ~0x3);
            status = (u8)(status | mode);

            if (LY == LYC) {
                status = (u8)(status | (1 << STATUS_COINCIDENCE_BITPOS));
            }
            else {
                status = (u8)(status & ~(1 << STATUS_COINCIDENCE_BITPOS));
            }

            STAT = status;

        }

        private void RenderLine(int line) {
            bool WindowTileMapSelect = IsBitSet(lcd_control, 6);
            bool WindowEnabled = IsBitSet(lcd_control, 5);
            bool TileDataSelect = IsBitSet(lcd_control, 4);
            bool BgTileMapSelect = IsBitSet(lcd_control, 3);
            int SpriteSize = lcd_control & 0x04; // 0 = 8x8 else 8x16
            bool SpriteEnabled = IsBitSet(lcd_control, 1);
            bool BgEnabled = IsBitSet(lcd_control, 0);

            u8[] BgPalette = GetPalette(0xFF47);
            u8[] Obj0Palette = GetPalette(0xFF48);
            u8[] Obj1Palette = GetPalette(0xFF49);

            byte _winX = (u8)(windowX - 7);

            int pixelY = line;

            // RENDER TILES
            // the display is 166x144
            for (int pixelX = 0; pixelX < 160; pixelX++) {
                // background
                if (BgEnabled) {
                    RenderTile(pixelX, pixelY, ScrollX, ScrollY, BgTileMapSelect, TileDataSelect, BgPalette);
                }
                // window
                if (WindowEnabled && pixelY >= windowY && pixelX >= _winX) {
                    RenderTile(pixelX, pixelY, -_winX, -windowY, WindowTileMapSelect, TileDataSelect, BgPalette);
                }
            }

            // RENDER SPRITES       
            if (SpriteEnabled) {
                int[] priorityListId = new int[40];
                int[] priorityListX = new int[40];
                for (int i = 0; i < 40; i++) {
                    int x = mmu.Read8((u16)(0xFE01 + i * 4));
                    priorityListId[i] = i;
                    if (x <= 0 || x >= 168) {
                        x = -1;
                    }
                    priorityListX[i] = x;
                }

                int pos = 1;
                while (pos < 40) {
                    if (priorityListX[pos] <= priorityListX[pos - 1]) {
                        pos++;
                    }
                    else {
                        int tmp = priorityListX[pos];
                        priorityListX[pos] = priorityListX[pos - 1];
                        priorityListX[pos - 1] = tmp;
                        tmp = priorityListId[pos];
                        priorityListId[pos] = priorityListId[pos - 1];
                        priorityListId[pos - 1] = tmp;
                        if (pos > 1) {
                            pos--;
                        }
                        else {
                            pos++;
                        }
                    }
                }

                for (int i = 0; i < 40; i++) {
                    if (priorityListX[i] == -1)
                        break;
                    int id = priorityListId[i];

                    byte SpriteY = mmu.Read8((ushort)(0xFE00 + id * 4));
                    if (SpriteY <= 0 || SpriteY >= 160)
                        continue;

                    byte SpriteX = mmu.Read8((ushort)(0xFE01 + id * 4));
                    int Sprite = mmu.Read8((ushort)(0xFE02 + id * 4));
                    byte SpriteAttr = mmu.Read8((ushort)(0xFE03 + id * 4));

                    int startX = SpriteX - 1;
                    int startY = SpriteY - 9;

                    if (SpriteSize != 0) {
                        Sprite &= 0xFE;
                        startY = SpriteY - 1;
                    }

                    int stepX = -1;
                    int stepY = -1;
                    if ((SpriteAttr & 0x20) != 0) // X-flip
                    {
                        startX = SpriteX - 8;
                        stepX = 1;
                    }
                    if ((SpriteAttr & 0x40) != 0) // Y-flip
                    {
                        startY = Math.Abs(SpriteY - 8 - 9 - SpriteSize * 2);
                        stepY = 1;
                    }

                    // draw
                    int yInTile = 7 + SpriteSize * 2;
                    int xInTile = 7;

                    for (pixelY = startY; yInTile >= 0; pixelY += stepY) {
                        if (pixelY != LY) {
                            yInTile--;
                            continue;
                        }

                        byte tileData0 = mmu.Read8((ushort)(0x8000 + Sprite * 16 + yInTile * 2));
                        byte tileData1 = mmu.Read8((ushort)(0x8000 + Sprite * 16 + yInTile * 2 + 1));

                        for (int x = startX; xInTile >= 0; x += stepX) {
                            if (x < 0 || x >= 160) {
                                xInTile--;
                                continue;
                            }

                            byte tileData2 = (byte)((byte)(tileData0 << xInTile) >> 7);
                            byte tileData3 = (byte)((byte)(tileData1 << xInTile) >> 7);
                            int colorId = (tileData3 << 1) + tileData2;
                            if (colorId != 0) {
                                byte[] palette = ((SpriteAttr & 0x10) != 0 ? Obj1Palette : Obj0Palette);
                                byte color = (byte)((3 - palette[colorId]) * 85);
                                byte[] colorData = { color, color, color, 255 }; // B G R
                                WriteBuffer(x, pixelY, colorData);
                            }
                            xInTile--;
                        }
                        yInTile--;
                        xInTile = 7;
                    }
                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderTile(int x, int y, int offsetX, int offsetY, bool mapSelect, bool tileDataSelect, u8[] BgPalette) {
            int realX = x + offsetX;
            int realY = y + offsetY;

            // the BG is 256x256 pixels
            // calculate the coordinates of the tile where the pixel belongs
            // there are 32 possible tiles (256 / 8)
            int tileCol = realX / 8;
            int tileRow = realY / 8;

            // Get tile number from memory map
            // map the values to a flat memory structure
            int tileOffset = (tileRow * 32) + tileCol;

            u8 tileNumber;
            if (mapSelect) {
                tileNumber = mmu.Read8((u16)(0x9C00 + tileOffset));
            }
            else {
                tileNumber = mmu.Read8((u16)(0x9800 + tileOffset));
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
            u8 tileLow = mmu.Read8((u16)(tileAddress + tileYPos * 2));
            u8 tileHigh = mmu.Read8((u16)(tileAddress + tileYPos * 2 + 1));

            tileLow = (u8)((u8)(tileLow << tileXpos) >> 7);
            tileHigh = (u8)((u8)(tileHigh << tileXpos) >> 7);

            int colorId = (tileHigh << 1) + tileLow;
            u8 color = (u8)((3 - BgPalette[colorId]) * 85);
            u8[] colorData = { color, color, color, 255 }; // B G R
            WriteBuffer(x, y, colorData);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] GetPalette(u16 address) {
            byte[] palette = new byte[4];
            byte rawPalette = mmu.Read8(address);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBitSet(u8 value, int bitPosition) {
            return ((value >> bitPosition) & 0b_0000_0001) == 1;
        }

        public void SetCoincidenceFlag() {
            if (LY == LYC) {
                STAT = (u8)(STAT | (1 << STATUS_COINCIDENCE_BITPOS));
            }
            else {
                STAT = (u8)(STAT & ~(1 << STATUS_COINCIDENCE_BITPOS));
            }
        }

        public bool IsLcdEnabled() {
            return IsBitSet(lcd_control, 7);
        }

        public byte[] GetScreenBuffer() {
            return frameBuffer;
        }

    }
}
