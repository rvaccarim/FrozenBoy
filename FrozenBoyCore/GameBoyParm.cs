using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public enum LogMode { Basic, Time, Full }

    public class GameBoyParm {
        public bool logExecution;
        public bool testingMode;
        public string logFilename;
        public LogMode logMode;

        public GameBoyParm(bool testingMode, bool logExecution, string logFilename, LogMode logMode) {
            this.testingMode = testingMode;
            this.logExecution = logExecution;
            this.logFilename = logFilename;
            this.logMode = logMode;
        }
    }
}
