using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public class GameBoyParm {
        public bool logExecution;
        public bool testingMode;
        public string logFilename;

        public GameBoyParm(bool testingMode, bool logExecution, string logFilename) {
            this.testingMode = testingMode;
            this.logExecution = logExecution;
            this.logFilename = logFilename;
        }
    }
}
