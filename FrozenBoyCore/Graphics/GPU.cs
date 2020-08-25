using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Runtime.ExceptionServices;
using FrozenBoyCore.Processor;

namespace FrozenBoyCore.Graphics {
    // http://imrannazar.com/GameBoy-Emulation-in-JavaScript:-GPU-Timings

    public class GPU {
        private const int SCREEN_WIDTH = 160;
        private const int SCREEN_HEIGHT = 144;
        private const int SCREEN_VBLANK_HEIGHT = 153;

        private const int MODE_HBLANK = 0b00;
        private const int MODE_VBLANK = 0b01;
        private const int MODE_SCANLINE_OAM = 0b10;
        private const int MODE_SCANLINE_VRAM = 0b11;

        private const int CYCLES_SCANLINE_OAM = 80;
        private const int CYCLES_SCANLINE_VRAM = 172;
        private const int CYCLES_HBLANK = 204;
        private const int CYCLES_ONE_LINE = CYCLES_SCANLINE_OAM + CYCLES_SCANLINE_VRAM + CYCLES_HBLANK; // 456
        private const int CYCLES_VBLANK = CYCLES_ONE_LINE * 10;

        // for the STATUS register
        private const int STATUS_COINCIDENCE_BITPOS = 2;
        private const int STATUS_HBLANK_BITPOS = 3;
        private const int STATUS_VBLANK_BITPOS = 4;
        private const int STATUS_SCANLINE_OAM_BITPOS = 5;

        public int modeClock;
        public int cumModeClock;
        public bool lcdEnabled = true;
        public bool prevLcdEnabled = true;
        public int turnOnDelay = 0;
        public bool first = true;

        private readonly InterruptManager iManager;

        private readonly byte modeMask = 0b_0000_0011;

        // Period                      GPU mode number     Time spent(clocks)
        // Horizontal blank                    0                  204
        // Vertical blank                      1                 4560 (10 lines)
        // Scanline(accessing OAM)             2                   80
        // Scanline(accessing VRAM)            3                  172
        // One line(scan and blank)                               456
        // Full frame(scans and vblank)                         70224
        public int mode;

        // GPU Registers
        // LCD and GPU Control
        // Bit Function                 When 0   When 1
        // 0	Background: on/off           
        // 1	Sprites: on/off             
        // 2	Sprites: size(pixels)   8x8   	8x16
        // 3	Background: tile map	 #0	      #1
        // 4	Background: tile set	 #0	      #1
        // 5	Window: on/off                             A "window" layer which can appear above the background
        // 6	Window: tile map	     #0	      #1
        // 7	Display: on/off        
        public u8 LCDC { get; set; }

        // Bits
        // 0-1
        //         00: H-Blank
        //         01: V-Blank
        //         10: Searching Sprites Atts
        //         11: Transfering Data to LCD Driver
        // 2       Set to 1 if register (0xFF44) is the same value as (0xFF45) 
        // 3, 4, 5 are interrupt enabled flags (similar to how the IE Register works), when the mode changes the
        //         corresponding bit 3,4,5 is set
        public u8 STAT { get; set; }
        public u8 ScrollY { get; set; }
        public u8 ScrollX { get; set; }
        // This is Y coordinate of the current scan line
        public u8 LY { get; set; }
        // LYC - Scanline compare register
        public u8 LYC { get; set; }
        public u8 BGPalette { get; set; }


        public GPU(InterruptManager iManager) {
            this.iManager = iManager;

            mode = 0;
            LCDC = 0x91;
            STAT = 0b1000_0110;
            LYC = 0xFC;
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
            return IsBitSet(LCDC, 7);
        }

        public void Update(int cycles) {
            lcdEnabled = IsLcdEnabled();

            if (!lcdEnabled) {
                modeClock = 0;
                cumModeClock = 0;

                LY = 0;
                STAT = (byte)(STAT & ~0x3);
                SetCoincidenceFlag();

                turnOnDelay = 244;
                prevLcdEnabled = lcdEnabled;
                return;
            }
            else {
                if (!prevLcdEnabled) {
                    int prevDelay = turnOnDelay;

                    // it was turned off and now it's turned on
                    if (first) {
                        turnOnDelay--;
                        first = false;
                    }
                    else {
                        turnOnDelay -= cycles;
                    }

                    if (turnOnDelay <= 0) {
                        modeClock = -prevDelay + 1;
                        cumModeClock = modeClock;

                        turnOnDelay = 0;
                        first = true;
                        prevLcdEnabled = lcdEnabled;
                    }
                    else {
                        return;
                    }
                }
            }

            u8 status = STAT;
            var mode = STAT & modeMask;
            modeClock += cycles;
            cumModeClock += cycles;

            // The GB simulates a cathodic-ray tube display, see horizontal and vertical blank
            switch (mode) {
                // Scanline(accessing OAM) -> Object Attribute Memory       
                case MODE_SCANLINE_OAM:
                    if (modeClock >= CYCLES_SCANLINE_OAM) {
                        modeClock -= CYCLES_SCANLINE_OAM;

                        mode = MODE_SCANLINE_VRAM;
                    }
                    break;
                // Scanline(accessing VRAM) -> Video Ram
                case MODE_SCANLINE_VRAM:
                    if (modeClock >= CYCLES_SCANLINE_VRAM) {
                        modeClock -= CYCLES_SCANLINE_VRAM;

                        mode = MODE_HBLANK;
                        if (IsBitSet(status, STATUS_HBLANK_BITPOS)) {
                            iManager.RequestLCD();
                        }

                        // drawScanLine(mmu);
                    }
                    break;
                // Horizontal blank
                case MODE_HBLANK:
                    if (modeClock >= CYCLES_HBLANK) {
                        modeClock -= CYCLES_HBLANK;

                        LY++;
                        cumModeClock = modeClock;

                        if (LY == SCREEN_HEIGHT) {
                            // it's the last line, we need to trigger a VBLANK interrupt to
                            // return to the first one
                            mode = MODE_VBLANK;
                            iManager.RequestVBlank();
                            if (IsBitSet(status, STATUS_VBLANK_BITPOS)) {
                                iManager.RequestLCD();
                            }
                        }
                        else {
                            mode = MODE_SCANLINE_OAM;
                        }
                    }
                    break;
                // Vertical blank
                case MODE_VBLANK:
                    if (modeClock >= CYCLES_ONE_LINE) {
                        modeClock -= CYCLES_ONE_LINE;
                        cumModeClock = modeClock;

                        // SCREEN_VBLANK_HEIGHT is largen than SCREEN_HEIGHT
                        LY++;
                        if (LY > SCREEN_VBLANK_HEIGHT) {
                            mode = MODE_SCANLINE_OAM;
                            LY = 0;

                            if (IsBitSet(status, STATUS_SCANLINE_OAM_BITPOS)) {
                                iManager.RequestLCD();
                            }
                        }
                    }
                    break;
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

        public int TicksUntilChange() {
            var mode = STAT & modeMask;

            return mode switch
            {
                MODE_SCANLINE_OAM => CYCLES_SCANLINE_OAM - modeClock,
                MODE_SCANLINE_VRAM => CYCLES_SCANLINE_VRAM - modeClock,
                MODE_HBLANK => CYCLES_HBLANK - modeClock,
                MODE_VBLANK => CYCLES_ONE_LINE - modeClock,
                _ => 0,
            };
        }

        private bool IsBitSet(u8 value, int bitPosition) {
            return ((value >> bitPosition) & 0b_0000_0001) == 1;
        }

    }
}
