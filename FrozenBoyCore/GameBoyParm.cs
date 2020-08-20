using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public class GameBoyParm {
        public bool debugMode;
        public bool checkLinkPort;
        public string logFilename;

        public GameBoyParm(bool debugMode, bool checkLinkPort, string logFilename) {
            this.debugMode = debugMode;
            this.checkLinkPort = checkLinkPort;
            this.logFilename = logFilename;
        }
    }
}
