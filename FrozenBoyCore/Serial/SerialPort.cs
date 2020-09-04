using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Processor;

namespace FrozenBoyCore.Serial {
    public class SerialPort {

        private u8 _sb;
        private u8 _sc;
        private bool _transferInProgress = false;
        private int _divider;
        private readonly InterruptManager interruptManager;
        public string log = "";

        public SerialPort(InterruptManager interruptManager) {
            this.interruptManager = interruptManager;
        }

        // FF01 - SB - Serial transfer data (R/W)
        public u8 SB {
            get => _sb;  // 0xff means no other gameboy connected
            set => _sb = value;
        }

        // FF02 - SC - Serial Transfer Control(R/W)
        // Bit 7 - Transfer Start Flag(0=No transfer is in progress or requested, 1=Transfer in progress, or requested)
        // Bit 1 - Clock Speed(0=Normal, 1=Fast) ** CGB Mode Only**
        // Bit 0 - Shift Clock(0=External Clock, 1=Internal Clock)
        public u8 SC {
            get => (u8)(_sc | 0b01111110);
            set {
                _sc = value;

                // log bytes, for Blargg tests
                // The gameboy acting as master will load up a data byte in SB and then set SC to 0x81
                if (value == 0x81) {
                    log += System.Convert.ToChar(_sb);

                    if ((_sc & (1 << 7)) != 0) {
                        StartTransfer();
                    }
                }
            }
        }

        public void Tick() {
            if (!_transferInProgress) {
                return;
            }

            if (++_divider >= 4_194_304 / 8192) {
                _transferInProgress = false;

                // Transfer data
                _sb = 0xff;

                interruptManager.RequestInterruption(InterruptionType.SerialLink);
            }
        }

        private void StartTransfer() {
            _transferInProgress = true;
            _divider = 0;
        }
    }
}
