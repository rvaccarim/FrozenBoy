using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;

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


        // for the IF register
        private const int INTERRUPT_VBLANK_BITPOS = 0;
        private const int INTERRUPT_LCD_BITPOS = 1;

        // for the STATUS register
        private const int STATUS_COINCIDENCE_BITPOS = 2;
        private const int STATUS_HBLANK_BITPOS = 3;
        private const int STATUS_VBLANK_BITPOS = 4;
        private const int STATUS_SCANLINE_OAM_BITPOS = 5;

        private int modeClock;
        private readonly MMU mmu;

        private readonly byte modeMask = 0b_0000_0011;

        // Period                      GPU mode number     Time spent(clocks)
        // Horizontal blank                    0                  204
        // Vertical blank                      1                 4560 (10 lines)
        // Scanline(accessing OAM)             2                   80
        // Scanline(accessing VRAM)            3                  172
        // One line(scan and blank)                               456
        // Full frame(scans and vblank)                         70224
        public int mode;

        public GPU(MMU mmu) {
            this.mmu = mmu;

            mode = 0;
        }

        public void Update(int cycles) {
            // 7 = LCD Enabled
            if (!IsBitSet(mmu.LCDC, 7)) {
                modeClock = 0;
                mmu.LY = 0;
                mmu.Status = (byte)(mmu.Status & ~0x3);
                return;
            }

            u8 status = mmu.Status;
            var mode = mmu.Status & modeMask;
            modeClock += cycles;
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
                            mmu.RequestInterrupt(INTERRUPT_LCD_BITPOS);
                        }

                        // drawScanLine(mmu);
                    }
                    break;
                // Horizontal blank
                case MODE_HBLANK:
                    if (modeClock >= CYCLES_HBLANK) {
                        // it's the end of the line, we need to go to the next one
                        modeClock -= CYCLES_HBLANK;

                        mmu.LY++;
                        if (mmu.LY == SCREEN_HEIGHT) {
                            // it's the last line, we need to trigger a VBLANK interrupt to
                            // return to the first one
                            mode = MODE_VBLANK;
                            mmu.RequestInterrupt(INTERRUPT_VBLANK_BITPOS);
                            if (IsBitSet(status, STATUS_VBLANK_BITPOS)) {
                                mmu.RequestInterrupt(INTERRUPT_LCD_BITPOS);
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

                        // SCREEN_VBLANK_HEIGHT is largen than SCREEN_HEIGHT
                        mmu.LY++;
                        if (mmu.LY > SCREEN_VBLANK_HEIGHT) {
                            mode = MODE_SCANLINE_OAM;
                            mmu.LY = 0;

                            if (IsBitSet(status, STATUS_SCANLINE_OAM_BITPOS)) {
                                mmu.RequestInterrupt(INTERRUPT_LCD_BITPOS);
                            }
                        }
                    }
                    break;
            }

            status = (u8)(mmu.Status & ~0x3);
            status = (u8)(status | mode);

            if (mmu.LY == mmu.LYC) {
                status = (u8)(status | (1 << STATUS_COINCIDENCE_BITPOS));
            }
            else {
                status = (u8)(status & ~(1 << STATUS_COINCIDENCE_BITPOS));
            }

            mmu.Status = status;

        }


        private bool IsBitSet(u8 value, int bitPosition) {
            return ((value >> bitPosition) & 0b_0000_0001) == 1;
        }

    }
}
