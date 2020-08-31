using FrozenBoyCore.Memory;
using System.Runtime.ExceptionServices;
using FrozenBoyCore.Processor;
using System.Runtime.CompilerServices;
using System;
using u8 = System.Byte;
using s8 = System.SByte;
using u16 = System.UInt16;
using s16 = System.Int16;
using FrozenBoyCore.Util;

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
        private const int STATUS_LC_EQUALS_LYC = 6;

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
        public u8 WindowY { get; set; }
        // 0xFF4B The X Positions -7 of the VIEWING AREA to start drawing the window from
        public u8 WindowX { get; set; }

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
                            LY++;

                            if (LY == 1) {
                                // Restart scanning modes
                                mode = MODE_SCANLINE_OAM;
                                LY = 0;

                                if (BitUtils.IsBitSet(status, STATUS_SCANLINE_OAM_BITPOS)) {
                                    intManager.RequestInterruption(InterruptionType.LCD);
                                }
                            }
                        }
                        break;
                }
            }

            // Set STAT flags according to the new mode
            status = (u8)(STAT & ~0b_0000_0011);
            status = (u8)(status | mode);

            if (LY == LYC) {
                status = (u8)(status | (1 << STATUS_COINCIDENCE_BITPOS));

                if (mode == MODE_HBLANK || mode == MODE_VBLANK) {
                    if (BitUtils.IsBitSet(status, STATUS_LC_EQUALS_LYC)) {
                        intManager.RequestInterruption(InterruptionType.LCD);
                    }
                }
            }
            else {
                status = (u8)(status & ~(1 << STATUS_COINCIDENCE_BITPOS));
            }

            STAT = status;

        }

        private void RenderLine(int line) {
            bool WindowTileMapSelect = BitUtils.IsBitSet(lcd_control, 6);
            bool WindowEnabled = BitUtils.IsBitSet(lcd_control, 5);
            bool TileDataSelect = BitUtils.IsBitSet(lcd_control, 4);
            bool BgTileMapSelect = BitUtils.IsBitSet(lcd_control, 3);
            int SpriteSize = lcd_control & 0x04; // 0 = 8x8 else 8x16
            bool SpriteEnabled = BitUtils.IsBitSet(lcd_control, 1);
            bool BgEnabled = BitUtils.IsBitSet(lcd_control, 0);

            u8[] BgPalette = GetPalette(0xFF47);
            u8[] Obj0Palette = GetPalette(0xFF48);
            u8[] Obj1Palette = GetPalette(0xFF49);

            byte winX = (u8)(WindowX - 7);

            int y = line;

            //if (BgEnabled)
            //    Render2();

            // RENDER TILES
            // the display is 166x144
            for (int x = 0; x < 160; x++) {
                // background
                if (BgEnabled) {
                    RenderTile(x, y, ScrollX, ScrollY, BgTileMapSelect, TileDataSelect, BgPalette);
                }
                // window
                if (WindowEnabled && y >= WindowY && x >= winX) {
                    RenderTile(x, y, -winX, -WindowY, WindowTileMapSelect, TileDataSelect, BgPalette);
                }

            }

            // RENDER SPRITES       
            if (SpriteEnabled) {
                int[] priorityListId = new int[40];
                int[] priorityListX = new int[40];

                for (int i = 0; i < 40; i++) {
                    int x = mmu.data[0xFE01 + i * 4];

                    priorityListId[i] = i;

                    if (x <= 0 || x >= 168) {
                        x = -1;
                    }
                    priorityListX[i] = x;
                }

                int pos = 1;
                while (pos < 40) {
                    if (priorityListX[pos] <= priorityListX[pos - 1])
                        pos++;
                    else {
                        int tmp = priorityListX[pos];
                        priorityListX[pos] = priorityListX[pos - 1];
                        priorityListX[pos - 1] = tmp;
                        tmp = priorityListId[pos];
                        priorityListId[pos] = priorityListId[pos - 1];
                        priorityListId[pos - 1] = tmp;
                        if (pos > 1)
                            pos--;
                        else
                            pos++;
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
                        Sprite = Sprite & 0xFE;
                        startY = SpriteY - 1;
                    }

                    //int endX = SpriteX - 8;
                    //int endY = SpriteY - 8 - SpriteSize * 2;
                    int stepX = -1;
                    int stepY = -1;
                    if ((SpriteAttr & 0x20) != 0) // X-flip
                    {
                        //startX = Math.Abs(SpriteX - 7);
                        startX = SpriteX - 8;
                        //endX = SpriteX;
                        stepX = 1;
                    }
                    if ((SpriteAttr & 0x40) != 0) // Y-flip
                    {
                        //startY = Math.Abs(SpriteY - (SpriteSize - 1));
                        startY = Math.Abs(SpriteY - 8 - 9 - SpriteSize * 2);
                        //endY = SpriteY;
                        stepY = 1;
                    }

                    // draw
                    int yInTile = 7 + SpriteSize * 2;
                    int xInTile = 7;

                    for (y = startY; yInTile >= 0; y += stepY) {
                        if (y != line) {
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
                                byte[] ColorData = { color, color, color, 255 }; // B G R
                                frameBuffer[(x + y * 160) * 4] = ColorData[0];
                                frameBuffer[(x + y * 160) * 4 + 1] = ColorData[1];
                                frameBuffer[(x + y * 160) * 4 + 2] = ColorData[2];
                                frameBuffer[(x + y * 160) * 4 + 3] = ColorData[3];
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
            int tileId = (tileRow * 32) + tileCol;

            u8 tileNumber;
            if (mapSelect) {
                tileNumber = mmu.Read8((u16)(0x9C00 + tileId));
            }
            else {
                tileNumber = mmu.Read8((u16)(0x9800 + tileId));
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


        //private void Render2() {
        //    bool unsig = true;

        //    // where to draw the visual area and the window
        //    u8 scrollY = ScrollY;
        //    u8 scrollX = ScrollX;
        //    u8 windowY = WindowY;
        //    u8 windowX = (byte)(WindowX - 7);

        //    bool usingWindow = false;

        //    // is the window enabled?
        //    if (IsBitSet(lcd_control, 5)) {
        //        // is the current scanline we're drawing
        //        // within the windows Y pos?,
        //        if (windowY <= LY)
        //            usingWindow = true;
        //    }

        //    ushort tileData;
        //    // which tile data are we using?
        //    if (IsBitSet(lcd_control, 4)) {
        //        tileData = 0x8000;
        //    }
        //    else {
        //        // IMPORTANT: This memory region uses signed
        //        // bytes as tile identifiers
        //        tileData = 0x8800;
        //        unsig = false;
        //    }

        //    ushort backgroundMemory;
        //    // which background mem?
        //    if (false == usingWindow) {
        //        if (IsBitSet(lcd_control, 3))
        //            backgroundMemory = 0x9C00;
        //        else
        //            backgroundMemory = 0x9800;
        //    }
        //    else {
        //        // which window memory?
        //        if (IsBitSet(lcd_control, 6))
        //            backgroundMemory = 0x9C00;
        //        else
        //            backgroundMemory = 0x9800;
        //    }

        //    u8 yPos = 0;

        //    // yPos is used to calculate which of 32 vertical tiles the
        //    // current scanline is drawing
        //    if (!usingWindow)
        //        yPos = (byte)(scrollY + LY);
        //    else
        //        yPos = (byte)(LY - windowY);

        //    // which of the 8 vertical pixels of the current
        //    // tile is the scanline on?
        //    u16 tileRow = ((ushort)(((u8)(yPos / 8)) * 32));

        //    // time to start drawing the 160 horizontal pixels
        //    // for this scanline
        //    for (int pixel = 0; pixel < 160; pixel++) {
        //        u8 xPos = (byte)(pixel + scrollX);

        //        // translate the current x pos to window space if necessary
        //        if (usingWindow) {
        //            if (pixel >= windowX) {
        //                xPos = (byte)(pixel - windowX);
        //            }
        //        }

        //        // which of the 32 horizontal tiles does this xPos fall within?
        //        u16 tileCol = ((ushort)(xPos / 8));
        //        s16 tileNum;

        //        // get the tile identity number. Remember it can be signed
        //        // or unsigned
        //        u16 tileAddrss = (ushort)(backgroundMemory + tileRow + tileCol);
        //        if (unsig)
        //            tileNum = (u8)mmu.Read8(tileAddrss);
        //        else
        //            tileNum = (s8)mmu.Read8(tileAddrss);

        //        // deduce where this tile identifier is in memory. Remember i
        //        // shown this algorithm earlier
        //        u16 tileLocation = tileData;

        //        if (unsig)
        //            tileLocation += (u16)(tileNum * 16);
        //        else
        //            tileLocation += (u16)((tileNum + 128) * 16);

        //        // find the correct vertical line we're on of the
        //        // tile to get the tile data
        //        //from in memory
        //        u8 line = (byte)(yPos % 8);
        //        line *= 2; // each vertical line takes up two bytes of memory
        //        u8 tileLow = mmu.Read8((ushort)(tileLocation + line));
        //        u8 tileHigh = mmu.Read8((ushort)(tileLocation + line + 1));

        //        int zzz = (byte)(xPos % 8);

        //        tileLow = (u8)((u8)(tileLow << zzz) >> 7);
        //        tileHigh = (u8)((u8)(tileHigh << zzz) >> 7);

        //        u8[] BgPalette = GetPalette(0xFF47);

        //        int colorId = (tileHigh << 1) + tileLow;
        //        u8 color = (u8)((3 - BgPalette[colorId]) * 85);
        //        u8[] colorData = { color, color, color, 255 }; // B G R
        //        WriteBuffer(pixel, LY, colorData);
        //    }

        //}


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

        public void SetCoincidenceFlag() {
            if (LY == LYC) {
                STAT = (u8)(STAT | (1 << STATUS_COINCIDENCE_BITPOS));
            }
            else {
                STAT = (u8)(STAT & ~(1 << STATUS_COINCIDENCE_BITPOS));
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
