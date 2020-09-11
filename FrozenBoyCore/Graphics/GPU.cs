using FrozenBoyCore.Memory;
using System.Runtime.ExceptionServices;
using FrozenBoyCore.Processor;
using System.Runtime.CompilerServices;
using System;
using FrozenBoyCore.Util;
using u8 = System.Byte;
using s8 = System.SByte;
using u16 = System.UInt16;
using s16 = System.Int16;
using System.Collections.Generic;
using System.Linq;

namespace FrozenBoyCore.Graphics {

    public class GPU {

        public GPU(InterruptManager iManager, GPU_Palette gpu_palette) {
            vRam = new Space(0x8000, 0x9FFF);
            oamRam = new Space(0xFE00, 0xFE9F);

            this.intManager = iManager;
            this.gpu_palette = gpu_palette;

            mode = 0;
            //lcd_control = 0x91;
            _stat = 0b1000_0110;
            _LY = 0;
            //LYC = 0x0;
        }

        private GPU_Palette gpu_palette;

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

        private readonly u8[] frameBuffer = new byte[160 * 144 * 4];
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
                // This is needed by Road Rash
                // http://www.devrs.com/gb/files/faqs.html#GBBugs
                if (mode == MODE_VBLANK || mode == MODE_HBLANK) {
                    if (IsLcdEnabled()) {
                        intManager.RequestInterruption(InterruptionType.LCD);
                    }
                }
                _stat = BitUtils.ChangeBits(_stat, 0b_1111_1000, value);
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
            bool WindowTileMap = BitUtils.IsBitSet(lcd_control, 6);
            bool WindowEnabled = BitUtils.IsBitSet(lcd_control, 5);
            bool tileSelect = BitUtils.IsBitSet(lcd_control, 4);
            bool BgTileMap = BitUtils.IsBitSet(lcd_control, 3);
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
                    RenderTile(x, y, realX, realY, BgTileMap, tileSelect, BgPalette);
                }

                if (WindowEnabled) {
                    if (y >= WY && x >= winx) {
                        int realX = x - winx;
                        int realY = y - WY;
                        RenderTile(x, y, realX, realY, WindowTileMap, tileSelect, BgPalette);
                    }
                }
            }

            // RENDER SPRITES       
            if (SpriteEnabled) {
                RenderSprites(Obj0Palette, Obj1Palette);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderTile(int x, int y, int realX, int realY, bool tileMap, bool tileSelect, u8[] palette) {
            // the BG is 256x256 pixels
            // calculate the coordinates of the tile where the pixel belongs
            // there are 32 possible tiles (256 / 8)
            int tileCol = realX / 8;
            int tileRow = realY / 8;

            // Get tile number from memory map
            // map the values to a flat memory structure
            int tileId = (tileRow * 32) + tileCol;

            u8 tileNumber;
            if (tileMap) {
                tileNumber = vRam[(u16)(0x9C00 + tileId)];
            }
            else {
                tileNumber = vRam[(u16)(0x9800 + tileId)];
            }

            // get tile data
            u16 tileAddress;
            if (tileSelect) {
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

            if (tileAddress == 0x88F0) {
                int dummy = 0;
            }

            int tileXpos = realX % 8;
            int tileYPos = realY % 8;

            // each pixel in the tile data set is represented by two bits
            u8 tileLsb = vRam[(u16)(tileAddress + (tileYPos * 2))];
            u8 tileMsb = vRam[(u16)(tileAddress + (tileYPos * 2) + 1)];

            int colorIndex = GetColorIndex(tileMsb, tileLsb, tileXpos);
            u8 color = palette[colorIndex];
            WriteBuffer(x, y, color);
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
        private void RenderSprites(u8[] Obj0Palette, u8[] Obj1Palette) {

            // this is system wide, not on a tile by tile 
            int spriteSize = GetSpriteSize();

            // SORT, see Sprites_DrawPriority1.png and Sprites_DrawPriority2.png
            var sprites = new List<Tuple<int, int>>();
            for (int i = 0; i < 40; i++) {
                // we store the original X and index
                sprites.Add(Tuple.Create((int)(oamRam[(u16)(0xFE01 + (i * 4))]), i));
            }
            sprites = sprites.OrderByDescending(t => t.Item1).ThenByDescending(t => t.Item2).ToList();

            int spritesInRow = 0;

            foreach (var sprite in sprites) {
                // there's a limit on how many sprites can be drawn
                if (spritesInRow == 10) {
                    break;
                }

                int offset = sprite.Item2 * 4;
                int spriteX = sprite.Item1 - 0x08;
                int spriteY = oamRam[(u16)(0xFE00 + offset)] - 0x10;

                if ((_LY >= spriteY) && (_LY < (spriteY + spriteSize))) {
                    u8 spriteID = oamRam[(u16)(0xFE02 + offset)];

                    u8 spriteAttr = oamRam[(u16)(0xFE03 + offset)];

                    int spriteRow = IsYFlipped(spriteAttr) ? spriteSize - 1 - (_LY - spriteY) : (_LY - spriteY);

                    u16 spriteRowAddress;
                    if (spriteSize == 8) {
                        spriteRowAddress = (u16)(0x8000 + (spriteID * 16) + (spriteRow * 2));
                    }
                    else {
                        // In 8x16 mode, in OAM they don't put the ID of the two sprites and we need to handle the 
                        // case in which they put the ID of the second sprite
                        if (spriteID % 2 == 0) {
                            spriteRowAddress = (u16)(0x8000 + (spriteID * 16) + (spriteRow * 2));
                        }
                        else {
                            // If they put the ID of the second one, we fetch the first one
                            spriteRowAddress = (u16)(0x8000 + ((spriteID - 1) * 16) + (spriteRow * 2));
                        }
                    }

                    u8 spriteRowLsb = vRam[spriteRowAddress];
                    u8 spriteRowMsb = vRam[(u16)(spriteRowAddress + 1)];

                    u8[] spritePalette = BitUtils.IsBitSet(spriteAttr, 4) ? GetPalette(OBP1) : GetPalette(OBP0);
                    int priority = BitUtils.IsBitSet(spriteAttr, 7) ? 1 : 0;

                    // render the 8 pixels in the row
                    for (int p = 0; p < 8; p++) {

                        if ((spriteX + p) >= 0 && (spriteX + p) < 160) {
                            int bitPos = !IsXFlipped(spriteAttr) ? p : 7 - p;

                            int colorIdx = GetColorIndex(spriteRowMsb, spriteRowLsb, bitPos);

                            if (DrawPixel(spriteX + p, _LY, colorIdx, priority)) {
                                u8 color = spritePalette[colorIdx];
                                WriteBuffer(spriteX + p, _LY, color);
                            }
                        }
                    }

                    spritesInRow++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DrawPixel(int x, int y, int spriteColorId, int spritePriority) {

            // 0 is transparent, no need to do anything
            if (spriteColorId == 0) {
                return false;
            }

            // priority 0 draws on top of everything
            if (spritePriority == 0) {
                return true;
            }
            else {
                // priority 1 draws the pixel only if the background is "white". 
                // White means the first color in the palette, it's not necessarily color white
                u8 bgWhitePositionColor = (u8)((3 - GetPalette(BGP)[0]) * 85);

                int offset = (y * 160) + x;

                if (frameBuffer[(offset * 4) + 0] == bgWhitePositionColor &&
                    frameBuffer[(offset * 4) + 1] == bgWhitePositionColor &&
                    frameBuffer[(offset * 4) + 2] == bgWhitePositionColor)
                    return true;

                return false;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetColorIndex(u8 msb, u8 lsb, int bitPos) {
            u8 bitLsb = (u8)((u8)(lsb << bitPos) >> 7);
            u8 bitMsb = (u8)((u8)(msb << bitPos) >> 7);
            return (bitMsb << 1) | bitLsb;
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
            u8[] palette = new u8[4];
            palette[0] = (u8)(rawPalette & 0b_0000_0011);
            palette[1] = (u8)((rawPalette & 0b_0000_1100) >> 2);
            palette[2] = (u8)((rawPalette & 0b_0011_0000) >> 4);
            palette[3] = (u8)((rawPalette & 0b_1100_0000) >> 6);
            return palette;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteBuffer(int x, int y, u8 color) {
            int offset = (y * 160) + x;
            // RGB Alpha
            frameBuffer[(offset * 4) + 0] = gpu_palette.colors[color].Red;
            frameBuffer[(offset * 4) + 1] = gpu_palette.colors[color].Green;
            frameBuffer[(offset * 4) + 2] = gpu_palette.colors[color].Blue;
            frameBuffer[(offset * 4) + 3] = gpu_palette.colors[color].Alpha;
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
