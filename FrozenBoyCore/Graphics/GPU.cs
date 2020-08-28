using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Runtime.ExceptionServices;
using FrozenBoyCore.Processor;

namespace FrozenBoyCore.Graphics {
    // http://imrannazar.com/GameBoy-Emulation-in-JavaScript:-GPU-Timings
    // https://gbdev.gg8.se/wiki/articles/Video_Display#FF40_-_LCDC_-_LCD_Control_.28R.2FW.29

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
        private byte _ldcd;

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
        public u8 LCDC {
            get => _ldcd;
            set {
                _ldcd = value;

                if (IsLcdEnabled()) {
                    EnableLCD();
                }
                else {
                    DisableLCD();
                }
            }
        }

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
        public u8 ScrollY { get; set; }
        public u8 ScrollX { get; set; }
        // This is Y coordinate of the current scan line
        public u8 LY { get; set; }
        // LYC - Scanline compare register
        public u8 LYC { get; set; }
        public u8 BGPalette { get; set; }


        public GPU(InterruptManager iManager) {
            this.intManager = iManager;
            mode = 0;
            _ldcd = 0x91;
            STAT = 0b1000_0110;
            LYC = 0x0;
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
                            // RenderScan();
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
            return IsBitSet(_ldcd, 7);
        }
    }
}
